using F4Utils.Terrain;
using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    internal interface IColumnAndRowElevationPostRecordRetriever
    {
        TheaterDotLxFileRecord GetElevationPostRecordByColumnAndRow(int postColumn, int postRow, uint lod, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo);
    }
    internal class ColumnAndRowElevationPostRecordRetriever:IColumnAndRowElevationPostRecordRetriever
    {
        private IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        public ColumnAndRowElevationPostRecordRetriever(IElevationPostCoordinateClamper elevationPostCoordinateClamper = null)
        {
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
        }
        public TheaterDotLxFileRecord GetElevationPostRecordByColumnAndRow(int postColumn, int postRow, uint lod, TheaterDotLxFileInfo[] theaterDotLxFiles,TheaterDotMapFileInfo theaterDotMapFileInfo)
        {
            
            var lodInfo = theaterDotLxFiles[lod];
            var mapInfo = theaterDotMapFileInfo;
            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(theaterDotMapFileInfo, theaterDotLxFiles, ref postColumn, ref postRow, lodInfo.LoDLevel);
            var blockRow = (int)Math.Floor((postRow / (float)postsAcross));
            var blockCol = (int)Math.Floor((postColumn / (float)postsAcross));
            var oIndex = (int)(blockRow * mapInfo.LODMapHeights[lodInfo.LoDLevel]) + blockCol;
            var block = lodInfo.O[oIndex];
            var col = (postColumn % postsAcross);
            var row = (postRow % postsAcross);
            var lIndex =
                (int)
                (((block.LRecordStartingOffset / (lodInfo.LRecordSizeBytes * postsAcross * postsAcross)) * postsAcross *
                  postsAcross) + ((row * postsAcross) + col));
            var lRecord = lodInfo.L[lIndex];
            return lRecord;
        }
    }
}
