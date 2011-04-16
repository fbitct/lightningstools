using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Common.MacroProgramming
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode,
            EntryPoint = "MapVirtualKey", SetLastError = true)]
        internal static extern uint MapVirtualKey(uint uCode, MAPVK_MAPTYPES uMapType);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode,
            EntryPoint = "MapVirtualKeyEx", SetLastError = true)]
        internal static extern uint MapVirtualKeyEx(uint uCode, MAPVK_MAPTYPES uMapType, IntPtr dwHkl);

        //        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "mouse_event", ExactSpelling = true, SetLastError = false)]
//        internal static extern void mouse_event(MOUSEEVENTF dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
//        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "keybd_event", ExactSpelling = true, SetLastError = true)]
//        internal static extern void keybd_event(byte bVk, byte bScan, KEYEVENTF dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        internal static void SendMouseInput(MOUSEEVENTF dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo)
        {
            var input = new INPUT();
            input.mi = new MOUSEINPUT();
            input.type = INPUT_TYPE.INPUT_MOUSE;
            input.mi.dwFlags = dwFlags;
            input.mi.dx = (int) dx;
            input.mi.dy = (int) dy;
            input.mi.mouseData = dwData;
            input.mi.time = 0;
            input.mi.dwExtraInfo = dwExtraInfo;
            SendInput(1, new[] {input}, Marshal.SizeOf(typeof (INPUT)));
        }

        internal static void SendKeyInput(ushort scanCode, bool press, bool release)
        {
            if (!press && !release)
            {
                return;
            }

            int numInputs = 0;
            if (press && release)
            {
                numInputs = 2;
            }
            else
            {
                numInputs = 1;
            }
            var inputs = new INPUT[numInputs];
            int curInput = 0;
            if (press)
            {
                var input = new INPUT();
                input.ki = new KEYBDINPUT();
                input.ki.wScan = scanCode;
                input.ki.time = 0;
                input.ki.dwFlags = KEYEVENTF.SCANCODE;
                if ((scanCode & 0x80) > 0)
                {
                    input.ki.dwFlags |= KEYEVENTF.EXTENDEDKEY;
                }
                Debug.WriteLine(input.ki.wScan);
                input.type = INPUT_TYPE.INPUT_KEYBOARD;
                inputs[curInput] = input;
                curInput++;
            }
            if (release)
            {
                var input = new INPUT();
                input.ki = new KEYBDINPUT();
                input.ki.wScan = scanCode;
                input.ki.time = 0;
                input.ki.dwFlags = (KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE);
                if ((scanCode & 0x80) > 0)
                {
                    input.ki.dwFlags |= KEYEVENTF.EXTENDEDKEY;
                }
                input.type = INPUT_TYPE.INPUT_KEYBOARD;
                inputs[curInput] = input;
            }
            SendInput((uint) numInputs, inputs, Marshal.SizeOf(typeof (INPUT)));
        }

        internal static void SendKeyInput(Keys keys, bool extendedKey, bool press, bool release)
        {
            var scanCode = (ushort) MapVirtualKey((uint) (keys & Keys.KeyCode), MAPVK_MAPTYPES.MAPVK_VK_TO_VSC);
            if (extendedKey) scanCode |= 0x80;
            SendKeyInput(scanCode, press, release);
        }

        #region Nested type: HARDWAREINPUT

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        #endregion

        #region Nested type: INPUT

        [StructLayout(LayoutKind.Explicit)]
        internal struct INPUT
        {
            [FieldOffset(0)] public INPUT_TYPE type;
            [FieldOffset(4)] public MOUSEINPUT mi;
            [FieldOffset(4)] public KEYBDINPUT ki;
            [FieldOffset(4)] public HARDWAREINPUT hi;
        }

        #endregion

        #region Nested type: INPUT_TYPE

        internal enum INPUT_TYPE : uint
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }

        #endregion

        #region Nested type: KEYBDINPUT

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region Nested type: KEYEVENTF

        [Flags]
        internal enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008,
        }

        #endregion

        #region Nested type: MAPVK_MAPTYPES

        internal enum MAPVK_MAPTYPES : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
            MAPVK_VK_TO_VSC_EX = 0x4,
        }

        #endregion

        #region Nested type: MOUSEEVENTF

        [Flags]
        internal enum MOUSEEVENTF : uint
        {
            MOVE = 0x0001, // mouse move 
            LEFTDOWN = 0x0002, // left button down
            LEFTUP = 0x0004, // left button up
            RIGHTDOWN = 0x0008, // right button down
            RIGHTUP = 0x0010, // right button up
            MIDDLEDOWN = 0x0020, // middle button down
            MIDDLEUP = 0x0040, // middle button up
            XDOWN = 0x0080, // x button down 
            XUP = 0x0100, // x button down
            WHEEL = 0x0800, // wheel button rolled
            VIRTUALDESK = 0x4000, // map to entire virtual desktop
            ABSOLUTE = 0x8000, // absolute move
        }

        #endregion

        #region Nested type: MOUSEINPUT

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MOUSEEVENTF dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        #endregion

        #region Nested type: MOUSEXBUTTONS

        internal enum MOUSEXBUTTONS : uint
        {
            XBUTTON1 = 0x0001,
            XBUTTON2 = 0x0002,
        }

        #endregion
    }
}