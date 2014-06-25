using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace F4SharedMem.Headers
{
    [ComVisible(false)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OSBData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public OSBLabel[] leftMFD;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public OSBLabel[] rightMFD;
    }
}
