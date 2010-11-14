using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrowserWindowCaptureControl
{
    internal class NativeMethods
    {
        public const int GA_PARENT =1;
        public const int GA_ROOT =2;
        public const int GA_ROOTOWNER = 3;
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int x;
            public int y;
            public int width;
            public int height;
        }
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError=true)]
        public static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, uint dwFlags);

    }
}
