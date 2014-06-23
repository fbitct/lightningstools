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
    public interface IDetailTextureForElevationPostRetriever
    {
        Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod, TerrainDB terrainDB);
    }
    public class DetailTextureForElevationPostRetriever:IDetailTextureForElevationPostRetriever
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
        public Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod, TerrainDB terrainDB)
        {
           
            var col = postCol;
            var row = postRow;

            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(ref col, ref row, lod, terrainDB);
            if (postCol != col || postRow != row)
            {
                col = 0;
                row = 0;
            }


            var lRecord = _columnAndRowElevationPostRetriever.GetElevationPostRecordByColumnAndRow(col, row, lod, terrainDB);

            var textureId = lRecord.TextureId;
            var bigTexture = _terrainTextureByTextureIdRetriever.GetTerrainTextureByTextureId(textureId, lod, terrainDB);

            Bitmap toReturn;
            if (lod <= terrainDB.TheaterDotMap.LastNearTiledLOD)
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
                if (terrainDB.ElevationPostTextures.ContainsKey(key))
                {
                    toReturn = terrainDB.ElevationPostTextures[key];
                }
                else
                {
                    var leftX = (int)(thisChunkXIndex * (bigTexture.Width / chunksWide));
                    var rightX = (int)((thisChunkXIndex + 1) * (bigTexture.Width / chunksWide)) - 1;
                    var topY = (int)(bigTexture.Height - (thisChunkYIndex + 1) * (bigTexture.Height / chunksWide));
                    var bottomY = (int)(bigTexture.Height - thisChunkYIndex * (bigTexture.Height / chunksWide)) - 1;

                    var sourceRect = new Rectangle(leftX, topY, (rightX - leftX) + 1, (bottomY - topY) + 1);

                    toReturn = (Bitmap)Common.Imaging.Util.CropBitmap(bigTexture, sourceRect);
                    terrainDB.ElevationPostTextures.Add(key, toReturn);
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
