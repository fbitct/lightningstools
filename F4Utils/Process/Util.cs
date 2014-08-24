using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Win32;
using F4SharedMem;
using log4net;
using System.ComponentModel;
using System.Management;

namespace F4Utils.Process
{
    public static class Util
    {
        private const string MODULENAME_F4 = "F4";
        private const string MODULENAME_FALCON = "FALCON";
        private const string WINDOW_CLASS_FALCONDISPLAY = "FalconDisplay";
        private const string EXE_NAME__F4AF = "FalconAF.exe";
        private const string EXENAME__BMS4 = "Falcon BMS.exe";
        private static readonly ILog Log = LogManager.GetLogger(typeof (Util));

        public static void ActivateFalconWindow()
        {
            if (!IsFalconWindowForeground())
            {
                var windowHandle = GetFalconWindowHandle();
                if (windowHandle != IntPtr.Zero)
                {
                    NativeMethods.SetForegroundWindow(windowHandle);
                    Thread.SpinWait(20);
                }
            }
        }

        public static bool IsFalconWindowForeground()
        {
            var windowHandle = GetFalconWindowHandle();
            if (windowHandle != IntPtr.Zero)
            {
                var foregroundWindowHandle = NativeMethods.GetForegroundWindow();
                return foregroundWindowHandle == windowHandle;
            }
            return false;
        }

        public static string GetFalconWindowTitle()
        {
            var txt = new StringBuilder(NativeMethods.MAX_PATH);
            var windowHandle = GetFalconWindowHandle();
            string toReturn = null;
            if (windowHandle != IntPtr.Zero)
            {
                NativeMethods.GetWindowText(windowHandle, txt, NativeMethods.MAX_PATH);
                toReturn = txt.ToString();
            }
            return toReturn;
        }

        public static bool IsFalconRunning()
        {
            var windowHandle = GetFalconWindowHandle();
            return (windowHandle != IntPtr.Zero);
        }

        public static string GetFalconExePath()
        {
            var windowHandle = GetFalconWindowHandle();
            string toReturn = null;
            if (windowHandle != IntPtr.Zero)
            {
                int procId;
                NativeMethods.GetWindowThreadProcessId(windowHandle, out procId);
                toReturn = ExePath(procId);
            }
            return toReturn;
        }
        private static string ExePath(int processId)
        {
            var wmiQueryString = string.Format("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId={0}", processId);
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var item = (from p in System.Diagnostics.Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            }).FirstOrDefault();
                return item != null ? item.Path : null;
            }
        }

        public static IntPtr GetFalconWindowHandle()
        {
            var hWnd = NativeMethods.FindWindow(WINDOW_CLASS_FALCONDISPLAY, null);
            return hWnd;
        }

        public static FalconDataFormats? DetectFalconFormat()
        {
            FalconDataFormats? toReturn = null;
            var exePath = GetFalconExePath();
            if (exePath != null)
            {
                if (exePath.ToLowerInvariant().Contains(EXE_NAME__F4AF.ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.AlliedForce;
                }
                else if (exePath.ToLowerInvariant().Contains(EXENAME__BMS4.ToLowerInvariant()))
                {
                    try
                    {
                        var verInfo = FileVersionInfo.GetVersionInfo(exePath);

                        if (verInfo.ProductMajorPart >= 4)
                        {
                            toReturn = FalconDataFormats.BMS4;
                        }
                    }
                    catch 
                    {

                    }
                }
                else if (exePath.ToLowerInvariant().Contains("falcon bms test.exe")) //Added by Falcas to make it possible to run with test exe files
                {
                    try
                    {
                        var verInfo = FileVersionInfo.GetVersionInfo(exePath);

                        if (verInfo.ProductMajorPart >= 4)
                        {
                            toReturn = FalconDataFormats.BMS4;
                        }
                    }
                    catch 
                    {

                    }
                }
            }

            return toReturn;
        }
    }
}