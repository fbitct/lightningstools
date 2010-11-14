using System;
using System.Collections.Generic;

using System.Text;

namespace F4Utils.Terrain.Structs
{
    [Serializable]
    public struct TextureDotBinFileInfo
    {
        public uint numSets;
        public uint totalTiles;
        public TextureBinSetRecord[] setRecords;
    }
}
