using System.Drawing;
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
        private readonly IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        private readonly TerrainDB _terrainDB;

        public MapTextureRenderer(
            TerrainDB terrainDB,
            IDetailTextureForElevationPostRetriever detailTextureForElevationPostRetriever=null 
            )
        {
            _terrainDB = terrainDB;
            _detailTextureForElevationPostRetriever = detailTextureForElevationPostRetriever ?? new DetailTextureForElevationPostRetriever();
        }

        public void RenderMapTextureForCurrentElevationPost(uint lod, int detailTextureWidthPixels,
            int mapBoundsLeftX, int mapBoundsTopY, Graphics h, int y, int x)
        {
            //retrieve the detail texture corresponding to the current elevation post offset 
            var detailTexture = _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost (x, y, lod, _terrainDB);
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

            h.DrawImage(detailTexture, destRect, sourceRect, GraphicsUnit.Pixel);
        }
    }
}