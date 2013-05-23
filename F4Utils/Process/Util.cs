using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Win32;
using F4SharedMem;
using log4net;
using System.ComponentModel;

namespace F4Utils.Process
{
    public static class Util
    {
        private const string MODULENAME_F4 = "F4";
        private const string MODULENAME_FALCON = "FALCON";
        private const string WINDOW_CLASS_FALCONDISPLAY = "FalconDisplay";
        private const string EXE_NAME__F4AF = "FalconAF.exe";
        private const string EXE_NAME__REDVIPER = "RedViper.exe";
        private const string EXE_NAME__FREEFALCON = "FFViper.exe";
        private const string EXENAME__BMS3_EARLY_BMS4 = "F4-BMS.exe";
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
                var process = System.Diagnostics.Process.GetProcessById(procId);
                try
                {
                    toReturn = (from ProcessModule module in process.Modules
                                where
                                    module.ModuleName.Contains(MODULENAME_F4) ||
                                    module.ModuleName.ToUpper().Contains(MODULENAME_FALCON)
                                select module.FileName).FirstOrDefault();
                }
                catch (Win32Exception)
                {
                    
                }
            }
            return toReturn;
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
                else if (exePath.ToLowerInvariant().Contains(EXE_NAME__REDVIPER.ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.BMS2;
                }
                else if (exePath.ToLowerInvariant().Contains(EXE_NAME__FREEFALCON.ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.FreeFalcon5;
                }
                else if (exePath.ToLowerInvariant().Contains(EXENAME__BMS3_EARLY_BMS4.ToLowerInvariant()))
                {
                    toReturn = FalconDataFormats.BMS3;
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
                    catch (Exception e)
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
                    catch (Exception e)
                    {

                    }
                }
            }

            return toReturn;
        }
    }
}