using System;
using System.Collections.Generic;

using System.Text;

namespace F4Utils.Terrain.Structs
{
    [Serializable]
    public class TheaterDotLxFileRecord
    {
        public UInt32 TextureId;
        public UInt16 Elevation;
        public byte Pallete;
        public byte X1;
        public byte X2;
    }
}
