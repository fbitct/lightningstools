using System.Drawing;

namespace F4Utils.Terrain
{
    public interface ITerrainTextureByTextureIdRetriever
    {
        Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod, TerrainDB terrainDB);
    }
    public class TerrainTextureByTextureIdRetriever:ITerrainTextureByTextureIdRetriever
    {
        private readonly INearTileTextureLoader _nearTileTextureLoader;
        private readonly IFarTileTextureRetriever _farTileTextureRetriever;
        public TerrainTextureByTextureIdRetriever(INearTileTextureLoader nearTileTextureLoader = null, IFarTileTextureRetriever farTileTextureRetriever = null)
        {
            _nearTileTextureLoader = nearTileTextureLoader ?? new NearTileTextureLoader();
            _farTileTextureRetriever = farTileTextureRetriever ?? new FarTileTextureRetriever();
        }
        public Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod, TerrainDB terrainDB)
        {
            var lodInfo = terrainDB.TheaterDotLxFiles[lod];
            Bitmap toReturn = null;

            if (lod <= terrainDB.TheaterDotMap.LastNearTiledLOD)
            {
                var textureBinInfo = terrainDB.TextureDotBin;
                var textureBaseFolderPath = terrainDB.CurrentTheaterTextureBaseFolderPath;
                textureId -= lodInfo.minTexOffset;
                if (terrainDB.NearTileTextures.ContainsKey(textureId)) return terrainDB.NearTileTextures[textureId];

                var setNum = textureId / Constants.NUM_TEXTURES_PER_SET;
                var tileNum = textureId % Constants.NUM_TEXTURES_PER_SET;
                var thisSet = textureBinInfo.setRecords[setNum];
                var tileName = thisSet.tileRecords[tileNum].tileName;
                toReturn = _nearTileTextureLoader.LoadNearTileTexture(tileName, terrainDB);
                if (toReturn != null)
                {
                    terrainDB.NearTileTextures.Add(textureId, toReturn);
                }
            }
            else if (lod <= terrainDB.TheaterDotMap.LastFarTiledLOD)
            {
                toReturn = _farTileTextureRetriever.GetFarTileTexture(textureId, terrainDB);
            }
            return toReturn;
        }
    }
}
