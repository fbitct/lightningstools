using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace F4Utils.Terrain.Structs
{
    [Serializable]
    public struct TheaterDotMapFileInfo
    {
        public float FeetBetweenL0Posts;
        public UInt32 MEAMapWidth;
        public UInt32 MEAMapHeight;
        public float FeetToMeaCellConversionFactor;
        public UInt32 NumLODs;
        public UInt32 LastNearTiledLOD;
        public UInt32 LastFarTiledLOD;
        public Color[] Pallete;
        public Color[] GreenPallete;
        public UInt32[] LODMapWidths;
        public UInt32[] LODMapHeights;
        public UInt32 flags;
        public float baseLong;
        public float baseLat;
    }
}
