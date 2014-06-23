using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    internal interface INearestElevationPostColumnAndRowCalculator
    {
        void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast, out int col, out int row, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo);
    }
    class NearestElevationPostColumnAndRowCalculator:INearestElevationPostColumnAndRowCalculator
    {
        private IDistanceBetweenElevationPostsCalculator _distanceBetweenElevationPostsCalculator;
        private IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        public NearestElevationPostColumnAndRowCalculator(
            IDistanceBetweenElevationPostsCalculator distanceBetweenElevationPostsCalculator = null,
            IElevationPostCoordinateClamper elevationPostCoordinateClamper = null) {
            _distanceBetweenElevationPostsCalculator = distanceBetweenElevationPostsCalculator ?? new DistanceBetweenElevationPostsCalculator();
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
        }
        public void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast,
                                                                               out int col, out int row, TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo)
        {
            const int lod = 0;
            var feetBetweenElevationPosts = _distanceBetweenElevationPostsCalculator.GetNumFeetBetweenElevationPosts(lod, theaterDotLxFiles, theaterDotMapFileInfo);
            col = (int)Math.Floor(feetEast / feetBetweenElevationPosts);
            row = (int)Math.Floor(feetNorth / feetBetweenElevationPosts);
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(theaterDotMapFileInfo, theaterDotLxFiles,ref row, ref col, lod);
        }
    }
}
