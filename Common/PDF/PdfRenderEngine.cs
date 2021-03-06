﻿using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;
using Microsoft.Win32;

namespace Common.PDF
{
    public static class PdfRenderEngine
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PdfRenderEngine));
        private static bool _loaded;
        private static bool _checked;

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_new_instance")]
        private static extern int CreateAPIInstance(out IntPtr pinstance,
                                                    IntPtr caller_handle);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_init_with_args")]
        private static extern int InitAPI(IntPtr instance, int argc, IntPtr argv);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_exit")]
        private static extern int ExitAPI(IntPtr instance);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_delete_instance")]
        private static extern void DeleteAPIInstance(IntPtr instance);

        public static int NumPagesInPdf(string fileName)
        {
            var result = 0;
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var r = new StreamReader(fs))
                {
                    var pdfText = r.ReadToEnd();
                    r.Close();
                    fs.Close();
                    var regx = new Regex(@"/Type\s*/Page[^s]");
                    var matches = regx.Matches(pdfText);
                    result = matches.Count;
                }
            }
            catch (IOException)
            {
            }
            return result;
        }

        private static string[] GetArgs(string inputPath, string outputPath, int firstPage, int lastPage, int width,
                                        int height)
        {
            return new[]
                       {
                           // Keep gs from writing information to standard output        
                           "-q",
                           "-dQUIET",
                           "-dPARANOIDSAFER", // Run this command in safe mode        
                           "-dBATCH", // Keep gs from going into interactive mode        
                           "-dNOPAUSE", // Do not prompt and pause for each page        
                           "-dNOPROMPT", // Disable prompts for user interaction                   
                           "-dMaxBitmap=500000000", // Set high for better performance                
                           // Set the starting and ending pages        
                           String.Format("-dFirstPage={0}", firstPage),
                           String.Format("-dLastPage={0}", lastPage),
                           // Configure the output anti-aliasing, resolution, etc        
                           "-dAlignToPixels=0",
                           "-dGridFitTT=0",
                           "-sDEVICE=png16m",
                           "-dTextAlphaBits=4",
                           "-dGraphicsAlphaBits=4",
                           String.Format("-r{0}x{1}", width, height),
                           // Set the input and output files        
                           String.Format("-sOutputFile={0}", outputPath),
                           inputPath
                       };
        }

        private static bool IsGhostscriptInstalled()
        {
            if (_loaded) return true;
            if (_checked) return _loaded;
            _checked = true;
            RegistryKey key;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GPL GhostScript", false);
            }
            catch (Exception e)
            {
                Log.Debug(e.Message, e);
                return false;
            }

            if (key == null) return false;

            float highestDetectedVersion = 0;
            foreach (var subkey in key.GetSubKeyNames())
            {
                float version = 0;
                var isNumeric = float.TryParse(subkey, out version);
                if (isNumeric && version > highestDetectedVersion)
                {
                    highestDetectedVersion = version;
                } 
            }
            key.Close();
            key = Registry.LocalMachine.OpenSubKey(string.Format(@"SOFTWARE\GPL GhostScript\{0}", highestDetectedVersion), false);
            var val = key.GetValue("GS_DLL");
            if (val == null) return false;
            var valString = val.ToString();
            if (String.IsNullOrEmpty(valString)) return false;
            var fi = new FileInfo(valString);
            if (!fi.Directory.Exists) return false;
            if (!fi.Exists) return false;
            var oldPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            if (!oldPath.Contains(fi.DirectoryName))
            {
                var newPath = oldPath + Path.PathSeparator + fi.DirectoryName;
                Environment.SetEnvironmentVariable("PATH", newPath);
            }
            _loaded = true;
            return true;
        }

        private static void CallAPI(string[] args)
        {
            if (!IsGhostscriptInstalled()) return;


            var argStrHandles = new GCHandle[args.Length];
            var argPtrs = new IntPtr[args.Length];
            // Create a handle for each of the arguments after 
            // they've been converted to an ANSI null terminated
            // string. Then store the pointers for each of the handles
            for (var i = 0; i < args.Length; i++)
            {
                argStrHandles[i] = GCHandle.Alloc(StringToAnsi(args[i]), GCHandleType.Pinned);
                argPtrs[i] = argStrHandles[i].AddrOfPinnedObject();
            }
            // Get a new handle for the array of argument pointers
            var argPtrsHandle = GCHandle.Alloc(argPtrs, GCHandleType.Pinned);
            // Get a pointer to an instance of the GhostScript API 
            // and run the API with the current arguments
            IntPtr gsInstancePtr;
            CreateAPIInstance(out gsInstancePtr, IntPtr.Zero);
            InitAPI(gsInstancePtr, args.Length, argPtrsHandle.AddrOfPinnedObject());
            Cleanup(argStrHandles, argPtrsHandle, gsInstancePtr);
        }

        private static void Cleanup(GCHandle[] argStrHandles, GCHandle argPtrsHandle, IntPtr gsInstancePtr)
        {
            for (var i = 0; i < argStrHandles.Length; i++)
            {
                argStrHandles[i].Free();
            }
            argPtrsHandle.Free();
            ExitAPI(gsInstancePtr);
            DeleteAPIInstance(gsInstancePtr);
        }

        public static byte[] StringToAnsi(string original)
        {
            var strBytes = new byte[original.Length + 1];
            for (var i = 0; i < original.Length; i++)
            {
                strBytes[i] = (byte) original[i];
            }
            strBytes[original.Length] = 0;
            return strBytes;
        }

        public static Bitmap GeneratePageBitmap(string pdfPath, int page, Size size)
        {
            var outputFileName = Path.GetTempFileName();
            var outputPath = Win32.Paths.Util.GetShortPathName(outputFileName);
            var inputPath = Win32.Paths.Util.GetShortPathName(pdfPath);
            var fi = new FileInfo(outputPath);

            Bitmap toReturn = null;
            try
            {
                GeneratePageBitmap(inputPath, outputPath, page, page, size.Width, size.Height);
                if (fi.Exists && fi.Length > 0)
                {
                    try
                    {
                        using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))
                        {
                            toReturn = (Bitmap) Image.FromStream(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message, e);
                    }
                }
            }
            finally
            {
                try
                {
                    fi.Delete();
                }
                catch (IOException e)
                {
                    Log.Debug(e.Message, e);
                }
            }
            return toReturn;
        }

        public static void GeneratePageBitmap(string inputPath, string outputPath, int firstPage, int lastPage,
                                              int width, int height)
        {
            CallAPI(GetArgs(inputPath, outputPath, firstPage, lastPage, width, height));
        }
    }
}