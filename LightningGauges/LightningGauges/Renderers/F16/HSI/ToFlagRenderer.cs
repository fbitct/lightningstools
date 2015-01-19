using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class ToFlagRenderer
    {
        internal static void DrawToFlag(Graphics destinationGraphics, ref GraphicsState basicState, float centerX, float centerY, InstrumentState instrumentState, Image toFlagMaskedImage)
        {
            //draw the TO flag 
            if (!instrumentState.ShowToFromFlag || !instrumentState.ToFlag) return;
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.TranslateTransform(centerX, centerY);
            destinationGraphics.RotateTransform(-instrumentState.MagneticHeadingDegrees);
            destinationGraphics.RotateTransform(instrumentState.DesiredCourseDegrees);
            destinationGraphics.TranslateTransform(-centerX, -centerY);
            destinationGraphics.DrawImage(toFlagMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}