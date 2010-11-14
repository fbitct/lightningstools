using System;
using System.Collections.Generic;

using System.Text;

namespace F4Utils.Terrain.Structs
{
    [Serializable]
    public struct TextureBinTileRecord
    {
        public string tileName;
        public uint numAreas;
        public uint numPaths;
        public TextureBinAreaRecord[] areaRecords;
        public TextureBinPathRecord[] pathRecords;
    }
}
