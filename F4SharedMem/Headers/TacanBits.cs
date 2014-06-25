using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    public enum TacanBits : byte
    {
        band = 0x01,   // true in this bit position if band is X
        mode = 0x02,   // true in this bit position if domain is air to air
    };
}
