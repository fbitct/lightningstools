using F4Utils.Terrain.Structs;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    internal interface IDetailTextureForElevationPostRetriever
    {
        Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod,
            TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo,
            TextureDotBinFileInfo textureDotBinFileInfo,
            Dictionary<uint, Bitmap> nearTileTextures, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries,
            Dictionary<uint, Bitmap> farTileTextures, string farTilesDotDdsFilePath, string farTilesDotRawFilePath, FarTilesDotPalFileInfo farTilesDotPalFileInfo,
            Dictionary<LodTextureKey, Bitmap> elevationPostTextures,
            string currentTheaterTextureBaseFolderPath);
    }
    class DetailTextureForElevationPostRetriever:IDetailTextureForElevationPostRetriever
    {
        private IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        private ITerrainTextureByTextureIdRetriever _terrainTextureByTextureIdRetriever;
        private IColumnAndRowElevationPostRecordRetriever _columnAndRowElevationPostRetriever;
        public DetailTextureForElevationPostRetriever(
            IElevationPostCoordinateClamper elevationPostCoordinateClamper=null,
            ITerrainTextureByTextureIdRetriever terrainTextureByTextureIdRetriever=null,
            IColumnAndRowElevationPostRecordRetriever columnAndRowElevationPostRetriever=null
            )
        {
            _elevationPostCoordinateClamper = elevationPostCoordinateClamper ?? new ElevationPostCoordinateClamper();
            _terrainTextureByTextureIdRetriever = terrainTextureByTextureIdRetriever ?? new TerrainTextureByTextureIdRetriever();
            _columnAndRowElevationPostRetriever = columnAndRowElevationPostRetriever ?? new ColumnAndRowElevationPostRecordRetriever();
        }
        public Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod, 
            TheaterDotLxFileInfo[] theaterDotLxFiles, TheaterDotMapFileInfo theaterDotMapFileInfo,
            TextureDotBinFileInfo textureDotBinFileInfo,
            Dictionary<uint, Bitmap> nearTileTextures, ref ZipFile textureZipFile, ref Dictionary<string, ZipEntry> textureDotZipFileEntries,
            Dictionary<uint, Bitmap> farTileTextures, string farTilesDotDdsFilePath, string farTilesDotRawFilePath, FarTilesDotPalFileInfo farTilesDotPalFileInfo,
            Dictionary<LodTextureKey, Bitmap> elevationPostTextures,
            string currentTheaterTextureBaseFolderPath)
        {
           
            var col = postCol;
            var row = postRow;

            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(theaterDotMapFileInfo, theaterDotLxFiles, ref col, ref row, lod);
            if (postCol != col || postRow != row)
            {
                col = 0;
                row = 0;
            }


            var lRecord = _columnAndRowElevationPostRetriever.GetElevationPostRecordByColumnAndRow(col, row, lod, theaterDotLxFiles, theaterDotMapFileInfo);

            var textureId = lRecord.TextureId;
            var bigTexture = _terrainTextureByTextureIdRetriever.GetTerrainTextureByTextureId(
                textureId, lod,
                theaterDotLxFiles, theaterDotMapFileInfo, 
                textureDotBinFileInfo, nearTileTextures, 
                ref textureZipFile, ref textureDotZipFileEntries, 
                farTileTextures, farTilesDotDdsFilePath, farTilesDotRawFilePath, farTilesDotPalFileInfo, 
                currentTheaterTextureBaseFolderPath);

            Bitmap toReturn;
            if (lod <= theaterDotMapFileInfo.LastNearTiledLOD)
            {
                var chunksWide = 4 >> (int)lod;
                var thisChunkXIndex = (uint)(col % chunksWide);
                var thisChunkYIndex = (uint)(row % chunksWide);

                var key = new LodTextureKey
                {
                    Lod = lod,
                    textureId = textureId,
                    chunkXIndex = thisChunkXIndex,
                    chunkYIndex = thisChunkYIndex
                };
                if (elevationPostTextures.ContainsKey(key))
                {
                    toReturn = elevationPostTextures[key];
                }
                else
                {
                    var leftX = (int)(thisChunkXIndex * (bigTexture.Width / chunksWide));
                    var rightX = (int)((thisChunkXIndex + 1) * (bigTexture.Width / chunksWide)) - 1;
                    var topY = (int)(bigTexture.Height - (thisChunkYIndex + 1) * (bigTexture.Height / chunksWide));
                    var bottomY = (int)(bigTexture.Height - thisChunkYIndex * (bigTexture.Height / chunksWide)) - 1;

                    var sourceRect = new Rectangle(leftX, topY, (rightX - leftX) + 1, (bottomY - topY) + 1);

                    toReturn = (Bitmap)Common.Imaging.Util.CropBitmap(bigTexture, sourceRect);
                    elevationPostTextures.Add(key, toReturn);
                }
            }
            else
            {
                toReturn = bigTexture;
            }
            return toReturn;
        }
    }
}
