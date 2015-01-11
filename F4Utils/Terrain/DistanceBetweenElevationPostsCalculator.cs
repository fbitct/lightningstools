namespace F4Utils.Terrain
{
    internal interface IDistanceBetweenElevationPostsCalculator
    {
        float GetNumFeetBetweenElevationPosts(int lod, TerrainDB terrainDB);
    }
    class DistanceBetweenElevationPostsCalculator:IDistanceBetweenElevationPostsCalculator
    {
        public float GetNumFeetBetweenElevationPosts(int lod, TerrainDB terrainDB)
        {
            
            if (terrainDB == null || terrainDB.TheaterDotLxFiles == null) return 0;
            var lodInfo = terrainDB.TheaterDotLxFiles[0];
            var mapInfo = terrainDB.TheaterDotMap;
            var feetBetweenPosts = mapInfo.FeetBetweenL0Posts;
            for (var i = 1; i <= lodInfo.LoDLevel; i++)
            {
                feetBetweenPosts *= 2;
            }
            return feetBetweenPosts;
        }

    }
}
