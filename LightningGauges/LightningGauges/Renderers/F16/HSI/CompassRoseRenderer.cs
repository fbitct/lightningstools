using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class CompassRoseRenderer
    {
        internal static void DrawCompassRose(Graphics destinationGraphics, ref GraphicsState basicState, float centerX,float centerY, InstrumentState instrumentState, Image compassRoseMaskedImage )
        {
            //draw compass rose
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.TranslateTransform(centerX, centerY);
            destinationGraphics.RotateTransform(-instrumentState.MagneticHeadingDegrees);
            destinationGraphics.TranslateTransform(-centerX, -centerY);
            destinationGraphics.DrawImage(compassRoseMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}