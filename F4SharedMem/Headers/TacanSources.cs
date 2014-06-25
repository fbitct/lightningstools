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
    public enum TacanSources : int
    {
        UFC = 0,
        AUX,
        NUMBER_OF_SOURCES,
    };
}
