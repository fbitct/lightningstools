using System;

namespace F4Utils.Terrain
{
    internal interface INearestElevationPostColumnAndRowCalculator
    {
        void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast, out int col, out int row, TerrainDB terrainDB);
    }
    class NearestElevationPostColumnAndRowCalculator:INearestElevationPostColumnAndRowCalculator
    {
        private readonly IDistanceBetweenElevationPostsCalculator _distanceBetweenElevationPostsCalculator;
        private readonly IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        public NearestElevationPostColumnAndRowCalculator(
            IDistanceBetweenElevationPostsCalculator distanceBetweenElevationPostsCalculator = null,
            IElevationPostCoordinateClamper elevationPostCoordinateClamper = null) {
            _distanceBetweenElevationPostsCalculator = distanceBetweenElevationPostsCalculator ?? new DistanceBetweenElevationPostsCalculator();
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
        }
        public void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast,
                                                                               out int col, out int row, TerrainDB terrainDB)
        {
            const int lod = 0;
            var feetBetweenElevationPosts = _distanceBetweenElevationPostsCalculator.GetNumFeetBetweenElevationPosts(lod, terrainDB);
            col = (int)Math.Floor(feetEast / feetBetweenElevationPosts);
            row = (int)Math.Floor(feetNorth / feetBetweenElevationPosts);
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(ref row, ref col, lod, terrainDB);
        }
    }
}
