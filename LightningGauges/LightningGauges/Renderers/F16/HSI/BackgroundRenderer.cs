using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class BackgroundRenderer
    {
        internal static void DrawBackground(Graphics destinationGraphics, ref GraphicsState basicState, Image hsiBackgroundMaskedImage)
        {
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.TranslateTransform(0, -11);
            destinationGraphics.DrawImage(hsiBackgroundMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}