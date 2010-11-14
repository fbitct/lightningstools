using System;
using System.Collections.Generic;

using System.Text;

namespace F4Utils.Terrain.Structs
{
    [Serializable]
    public struct TheaterDotLxFileInfo
    {
        public TheaterDotOxFileRecord[] O;
        public TheaterDotLxFileRecord[] L;
        public uint LRecordSizeBytes;
        public uint LoDLevel;
        public UInt16 MinElevation;
        public UInt16 MaxElevation;
        public uint minTexOffset;
        public uint maxTexOffset;
    }
}
