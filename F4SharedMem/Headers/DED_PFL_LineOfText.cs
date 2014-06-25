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
    public struct DED_PFL_LineOfText
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public sbyte[] chars;
    }
}
