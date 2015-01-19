using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class AirplaneSymbolRenderer
    {
        internal static void DrawAirplaneSymbol(Graphics destinationGraphics, ref GraphicsState basicState, Image airplaneSymbolMaskedImage)
        {
            //draw airplane symbol
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.TranslateTransform(0, 5);
            destinationGraphics.DrawImage(airplaneSymbolMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}