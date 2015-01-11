using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Common.Win32;
using log4net;

namespace F4Utils.Process
{
    public static class Util
    {
        private const string MODULENAME_F4 = "F4";
        private const string MODULENAME_FALCON = "FALCON";
        private const string WINDOW_CLASS_FALCONDISPLAY = "FalconDisplay";
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
        private static int? _lastFalconExeProcessId;
        private static string _lastFalconExePath;
        private static string ExePath(int processId)
        {
            if (_lastFalconExeProcessId.HasValue && _lastFalconExeProcessId == processId && _lastFalconExePath !=null )
            {
                return _lastFalconExePath;
            }
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
                var exePath= item != null ? item.Path : null;
                _lastFalconExeProcessId = processId;
                _lastFalconExePath = exePath;
                return exePath;
            }
        }

        public static IntPtr GetFalconWindowHandle()
        {
            var hWnd = NativeMethods.FindWindow(WINDOW_CLASS_FALCONDISPLAY, null);
            return hWnd;
        }

    }
}