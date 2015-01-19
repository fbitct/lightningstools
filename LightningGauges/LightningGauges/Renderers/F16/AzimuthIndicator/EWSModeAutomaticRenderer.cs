using System.Drawing;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class EWSModeAutomaticRenderer
    {
        internal static void DrawEWSModeAutomatic(Graphics gfx, RectangleF ewmsModeRectangle, StringFormat miscTextStringFormat, Color okColor, Font font)
        {
            var legendColor = okColor;
            Brush legendBrush = new SolidBrush(legendColor);
            StringRenderer.DrawString(gfx, "AUT", font, legendBrush, ewmsModeRectangle,
                miscTextStringFormat);
        }
    }
}