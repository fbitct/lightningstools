using F4Utils.Terrain;
using F4Utils.Terrain.Structs;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    internal interface ITerrainTextureByTextureIdRetriever
    {
        Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod,
            TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo, TextureDotBinFileInfo textureDotBinFileInfo,
            Dictionary<uint, Bitmap> nearTileTextures, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries,
            Dictionary<uint, Bitmap> farTileTextures, string farTilesDotDdsFilePath, string farTilesDotRawFilePath, FarTilesDotPalFileInfo farTilesDotPalFileInfo,
            string currentTheaterTextureBaseFolderPath);
    }
    class TerrainTextureByTextureIdRetriever:ITerrainTextureByTextureIdRetriever
    {
        private INearTileTextureLoader _nearTileTextureLoader;
        private IFarTileTextureRetriever _farTileTextureRetriever;
        public TerrainTextureByTextureIdRetriever(INearTileTextureLoader nearTileTextureLoader = null, IFarTileTextureRetriever farTileTextureRetriever = null)
        {
            _nearTileTextureLoader = nearTileTextureLoader ?? new NearTileTextureLoader();
            _farTileTextureRetriever = farTileTextureRetriever ?? new FarTileTextureRetriever();
        }
        public Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod, 
            TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo, TextureDotBinFileInfo textureDotBinFileInfo,
            Dictionary<uint, Bitmap> nearTileTextures, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries,
            Dictionary<uint, Bitmap> farTileTextures, string farTilesDotDdsFilePath, string farTilesDotRawFilePath, FarTilesDotPalFileInfo farTilesDotPalFileInfo,
            string currentTheaterTextureBaseFolderPath)
        {
            var lodInfo = theaterDotLxFiles[lod];
            Bitmap toReturn = null;

            if (lod <= theaterDotMapFileInfo.LastNearTiledLOD)
            {
                var textureBinInfo = textureDotBinFileInfo;
                var textureBaseFolderPath = currentTheaterTextureBaseFolderPath;
                textureId -= lodInfo.minTexOffset;
                if (nearTileTextures.ContainsKey(textureId)) return nearTileTextures[textureId];

                var setNum = textureId / Constants.NUM_TEXTURES_PER_SET;
                var tileNum = textureId % Constants.NUM_TEXTURES_PER_SET;
                var thisSet = textureBinInfo.setRecords[setNum];
                var tileName = thisSet.tileRecords[tileNum].tileName;
                toReturn = _nearTileTextureLoader.LoadNearTileTexture(textureBaseFolderPath, tileName, ref textureZipFile, ref textureDotZipFileEntries);
                if (toReturn != null)
                {
                    nearTileTextures.Add(textureId, toReturn);
                }
            }
            else if (lod <= theaterDotMapFileInfo.LastFarTiledLOD)
            {
                toReturn = _farTileTextureRetriever.GetFarTileTexture(textureId, farTileTextures, farTilesDotDdsFilePath, farTilesDotRawFilePath, farTilesDotPalFileInfo);
            }
            return toReturn;
        }
    }
}
