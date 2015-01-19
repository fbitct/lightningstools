using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class FromFlagRenderer
    {
        internal static void DrawFromFlag(Graphics destinationGraphics, ref GraphicsState basicState, float centerX, float centerY, InstrumentState instrumentState, Image fromFlagMaskedImage)
        {
            //draw the FROM flag 
            if (!instrumentState.ShowToFromFlag || !instrumentState.FromFlag) return;
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.TranslateTransform(centerX, centerY);
            destinationGraphics.RotateTransform(-instrumentState.MagneticHeadingDegrees);
            destinationGraphics.RotateTransform(instrumentState.DesiredCourseDegrees);
            destinationGraphics.TranslateTransform(-centerX, -centerY);
            destinationGraphics.DrawImage(fromFlagMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}