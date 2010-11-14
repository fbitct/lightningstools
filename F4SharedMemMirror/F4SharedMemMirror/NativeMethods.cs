using System;
using System.Runtime.InteropServices;
using System.Text;

namespace F4SharedMemMirror
{

    internal class NativeMethods
    {
        private NativeMethods()
        {
        }
        public const Int64 INVALID_HANDLE_VALUE = -1;

        [Flags]
        public enum PageProtection : uint
        {
            NoAccess = 0x01,
            Readonly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
        }
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SRCCOPY = 13369376;
        public const int CAPTUREBLT = 0x40000000;
        public const int HGDI_ERR = 0xFFFF;
        /// <summary>
        /// WM is just a placeholder class for WM (WindowMessage) definitions
        /// </summary>
        public class WM
        {
            public const int WM_RESIZE = 0x0802;
            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_NCMOUSEMOVE = 0x00A0;
            public const int WM_NCLBUTTONDOWN = 0x00A1;
            public const int WM_NCLBUTTONUP = 0x00A2;
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_KEYDOWN = 0x0100;
        }

        /// <summary>
        /// HT is just a placeholder for HT (HitTest) definitions
        /// </summary>
        public class HT
        {
            public const int HTERROR = (-2);
            public const int HTTRANSPARENT = (-1);
            public const int HTNOWHERE = 0;
            public const int HTCLIENT = 1;
            public const int HTCAPTION = 2;
            public const int HTSYSMENU = 3;
            public const int HTGROWBOX = 4;
            public const int HTSIZE = HTGROWBOX;
            public const int HTMENU = 5;
            public const int HTHSCROLL = 6;
            public const int HTVSCROLL = 7;
            public const int HTMINBUTTON = 8;
            public const int HTMAXBUTTON = 9;
            public const int HTLEFT = 10;
            public const int HTRIGHT = 11;
            public const int HTTOP = 12;
            public const int HTTOPLEFT = 13;
            public const int HTTOPRIGHT = 14;
            public const int HTBOTTOM = 15;
            public const int HTBOTTOMLEFT = 16;
            public const int HTBOTTOMRIGHT = 17;
            public const int HTBORDER = 18;
            public const int HTREDUCE = HTMINBUTTON;
            public const int HTZOOM = HTMAXBUTTON;
            public const int HTSIZEFIRST = HTLEFT;
            public const int HTSIZELAST = HTBOTTOMRIGHT;

            public const int HTOBJECT = 19;
            public const int HTCLOSE = 20;
            public const int HTHELP = 21;
        }

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }
        public const uint BI_RGB = 0;
        public const uint DIB_RGB_COLORS = 0;
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public uint biSize;
            public int biWidth, biHeight;
            public short biPlanes, biBitCount;
            public uint biCompression, biSizeImage;
            public int biXPelsPerMeter, biYPelsPerMeter;
            public uint biClrUsed, biClrImportant;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] cols;
        }
        public static uint MAKERGB(int r, int g, int b)
        {
            return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
        }

        [StructLayout(LayoutKind.Sequential,
        Pack = 1, CharSet = CharSet.Unicode)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = 32)]
            public char[] DeviceName;
            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = 128)]
            public char[] DeviceString;
            public int StateFlags;
            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = 128)]
            public char[] DeviceID;
            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = 128)]
            public char[] DeviceKey;
        }

        //API constants.
        public const int MAX_PATH = 260;


        //API declares. For each API wrapped by this sample DLL (there are 5),
        //there will be a commented call containing the API call in C++ from the
        //Microsoft Win32 API reference. Immediately underneath the C++ call will
        //be the corresponding C# api call.

        /*
        BOOL PathCompactPathEx(
            LPTSTR pszOut,
            LPCTSTR pszSrc,
            UINT cchMax,
            DWORD dwFlags
        );
        */
        [DllImport("shlwapi", EntryPoint = "PathCompactPathEx", SetLastError = true)]
        public static extern bool PathCompactPathEx(
            StringBuilder pszOut,
            string pszSrc,
            int cchMax,
            int dwFlags
        );

        /*
        BOOL PathIsContentType(
            LPCTSTR pszPath,
            LPCTSTR pszContentType
        );
        */
        [DllImport("shlwapi", EntryPoint = "PathIsContentType", SetLastError = true)]
        public static extern bool PathIsContentType(
            string pszPath,
            string pszContentType
        );

        /*
        BOOL PathMakePretty(
            LPTSTR lpPath
        );
        */
        [DllImport("shlwapi", EntryPoint = "PathMakePretty", SetLastError = true)]
        public static extern bool PathMakePretty(
            StringBuilder lpPath
        );

        /*
        BOOL PathCanonicalize(
            LPTSTR lpszDst,
            LPCTSTR lpszSrc
        );
        */
        [DllImport("shlwapi", EntryPoint = "PathCanonicalize", SetLastError = true)]
        public static extern bool PathCanonicalize(
            StringBuilder lpszDst,
            string lpszSrc
        );

        /*
        LPCTSTR PathFindSuffixArray(
            LPCTSTR pszPath,
            LPCTSTR* apszSuffix,
            int iArraySize
        );
        */
        [DllImport("shlwapi", EntryPoint = "PathFindSuffixArray", SetLastError = true)]
        public static extern string PathFindSuffixArray(
            string pszPath,
            string[] apszSuffix,
            int iArraySize
        );

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        [DllImportAttribute("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "GetWindowDC", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC", SetLastError = true)]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", SetLastError = true)]
        public static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);


        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE
            lpDisplayDevice,
            uint dwFlags);


        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest,
                                         int yDest, int wDest,
                                         int hDest, IntPtr hdcSource,
                                         int xSrc, int ySrc, int RasterOp);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap", SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap
                                    (IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC", SetLastError = true)]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice,
           string lpszOutput, IntPtr lpInitData);

        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const UInt32 SECTION_QUERY = 0x0001;
        public const UInt32 SECTION_MAP_WRITE = 0x0002;
        public const UInt32 SECTION_MAP_READ = 0x0004;
        public const UInt32 SECTION_MAP_EXECUTE = 0x0008;
        public const UInt32 SECTION_EXTEND_SIZE = 0x0010;
        public const UInt32 SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SECTION_QUERY |
            SECTION_MAP_WRITE |
            SECTION_MAP_READ |
            SECTION_MAP_EXECUTE |
            SECTION_EXTEND_SIZE);
        public const UInt32 FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenFileMapping(uint dwDesiredAccess, 
            bool bInheritHandle,
           string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint
           dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
           IntPtr dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFileMapping(IntPtr hFile,
           IntPtr lpFileMappingAttributes, PageProtection flProtect, uint dwMaximumSizeHigh,
           uint dwMaximumSizeLow, string lpName);


    }
}
