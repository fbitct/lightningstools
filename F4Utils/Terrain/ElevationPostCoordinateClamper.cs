using F4Utils.Terrain;
using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    internal interface IElevationPostCoordinateClamper
    {
        void ClampElevationPostCoordinates(TheaterDotMapFileInfo theaterDotMapFileInfo, TheaterDotLxFileInfo[] theaterDotLxFiles, ref int postColumn, ref int postRow, uint lod);
    }
    class ElevationPostCoordinateClamper:IElevationPostCoordinateClamper
    {
        public void ClampElevationPostCoordinates(TheaterDotMapFileInfo theaterDotMapFileInfo, TheaterDotLxFileInfo[] theaterDotLxFiles, ref int postColumn, ref int postRow, uint lod)
        {
            var mapInfo = theaterDotMapFileInfo;
            var lodInfo = theaterDotLxFiles[lod];

            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            if (postColumn < 0) postColumn = 0;
            if (postRow < 0) postRow = 0;
            if (postColumn > (mapInfo.LODMapWidths[lodInfo.LoDLevel] * postsAcross) - 1)
                postColumn = (int)(mapInfo.LODMapWidths[lodInfo.LoDLevel] * postsAcross) - 1;
            if (postRow > (mapInfo.LODMapHeights[lodInfo.LoDLevel] * postsAcross) - 1)
                postRow = (int)(mapInfo.LODMapHeights[lodInfo.LoDLevel] * postsAcross) - 1;
        }
    }
}
