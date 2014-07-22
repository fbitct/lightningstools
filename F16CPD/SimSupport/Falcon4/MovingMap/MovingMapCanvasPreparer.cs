using System.Drawing;
using System.Drawing.Drawing2D;
using F4Utils.Terrain;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMovingMapCanvasPreparer
    {
        void PrepareCanvas(float magneticHeadingInDecimalDegrees, MapRotationMode rotationMode, Graphics h,
            int cropWidth, float toScale, int xOffset, int yOffset);
    }

    internal class MovingMapCanvasPreparer : IMovingMapCanvasPreparer
    {
        private readonly TerrainDB _terrainDB;
        private readonly ITheaterMapBuilder _theaterMapBuilder;
        public MovingMapCanvasPreparer(
            TerrainDB terrainDB,
            ITheaterMapBuilder theaterMapBuilder=null)
        {
            _terrainDB = terrainDB;
            _theaterMapBuilder = theaterMapBuilder ?? new TheaterMapBuilder();
        }

        public void PrepareCanvas(float magneticHeadingInDecimalDegrees, MapRotationMode rotationMode, Graphics h,
            int cropWidth, float toScale, int xOffset, int yOffset)
        {
            h.PixelOffsetMode = PixelOffsetMode.Half;
            if (rotationMode == MapRotationMode.CurrentHeadingOnTop)
            {
                RotateDestinationGraphicsSurfaceToPutCurrentHeadingOnTop(cropWidth, h, magneticHeadingInDecimalDegrees);
            }

            h.ScaleTransform(toScale, toScale);
            h.TranslateTransform(-xOffset, -yOffset);

            var crapMap = _theaterMapBuilder.GetTheaterMap(_terrainDB.TheaterDotMap.NumLODs - 1, _terrainDB);
            if (crapMap != null)
            {
                h.Clear(crapMap.GetPixel(crapMap.Width - 1, crapMap.Height - 1));
            }
        }
        private static void RotateDestinationGraphicsSurfaceToPutCurrentHeadingOnTop(int cropWidth, Graphics h, float magneticHeadingInDecimalDegrees)
        {
            h.TranslateTransform(cropWidth / 2.0f, cropWidth / 2.0f);
            h.RotateTransform(-(magneticHeadingInDecimalDegrees));
            h.TranslateTransform(-cropWidth / 2.0f, -cropWidth / 2.0f);
        }
    }
}