using System.Drawing;
using Common.Imaging;
using F4Utils.Terrain;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMapTextureRenderer
    {
        void RenderMapTextureForCurrentElevationPost(uint lod, int detailTextureWidthPixels,
            int mapBoundsLeftX, int mapBoundsTopY, Graphics h, int y, int x);
    }

    internal class MapTextureRenderer : IMapTextureRenderer
    {
        private readonly TerrainDB _terrainDB;

        public MapTextureRenderer(
            TerrainDB terrainDB
            )
        {
            _terrainDB = terrainDB;
        }

        public void RenderMapTextureForCurrentElevationPost(uint lod, int detailTextureWidthPixels,
            int mapBoundsLeftX, int mapBoundsTopY, Graphics h, int y, int x)
        {
            //retrieve the detail texture corresponding to the current elevation post offset 
            var detailTexture = _terrainDB.GetDetailTextureForElevationPost (x, y, lod);
            if (detailTexture == null) return;
           
            //now draw the detail texture onto the render target
            var sourceRect = new Rectangle(0, 0, detailTexture.Width, detailTexture.Height);
            
            //determine the upper-left pixel at which to place this detail texture on the render target
            var destPoint = new Point(
                (x - mapBoundsLeftX) * detailTextureWidthPixels,
                (mapBoundsTopY - y - 1) * detailTextureWidthPixels
                );
            
            //calculate the destination rectangle (in pixels) on the render target, that we'll be placing the detail texture inside of
            var destRect = new Rectangle(destPoint,
                new Size(
                    detailTextureWidthPixels + 2,
                    detailTextureWidthPixels + 2));

            h.DrawImageFast(detailTexture, destRect, sourceRect, GraphicsUnit.Pixel);
            detailTexture.Dispose();
        }
    }
}