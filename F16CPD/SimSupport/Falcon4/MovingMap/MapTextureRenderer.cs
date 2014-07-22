using System.Drawing;
using F4Utils.Terrain;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMapTextureRenderer
    {
        void RenderMapTextureForCurrentElevationPost(uint lod, int detailTextureWidthPixels,
            int leftXPost, int topYPost, Graphics h, int elevationPostY, int elevationPostX);
    }

    internal class MapTextureRenderer : IMapTextureRenderer
    {
        private readonly IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        private readonly TerrainDB _terrainDB;

        public MapTextureRenderer(IDetailTextureForElevationPostRetriever detailTextureForElevationPostRetriever, TerrainDB terrainDB)
        {
            _terrainDB = terrainDB;
            _detailTextureForElevationPostRetriever = detailTextureForElevationPostRetriever;
        }

        public void RenderMapTextureForCurrentElevationPost(uint lod, int detailTextureWidthPixels,
            int leftXPost, int topYPost, Graphics h, int elevationPostY, int elevationPostX)
        {
            //retrieve the detail texture corresponding to the current elevation post offset 
            var thisElevationPostDetailTexture = _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost
                (elevationPostX, elevationPostY, lod, _terrainDB);
            if (thisElevationPostDetailTexture == null) return;
            //now draw the detail texture onto the render target
            var sourceRect = new Rectangle(0, 0, thisElevationPostDetailTexture.Width,
                thisElevationPostDetailTexture.Height);
            //determine the upper-left pixel at which to place this detail texture on the render target
            var destPoint = new Point(
                (elevationPostX - leftXPost)*detailTextureWidthPixels,
                (topYPost - elevationPostY - 1)*detailTextureWidthPixels
                );
            //calculate the destination rectangle (in pixels) on the render target, that we'll be placing the detail texture inside of
            var destRect = new Rectangle(destPoint,
                new Size(detailTextureWidthPixels + 2,
                    detailTextureWidthPixels + 2));
            h.DrawImage(thisElevationPostDetailTexture, destRect, sourceRect, GraphicsUnit.Pixel);
        }
    }
}