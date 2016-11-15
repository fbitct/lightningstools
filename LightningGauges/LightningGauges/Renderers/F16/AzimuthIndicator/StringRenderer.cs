using Common.Drawing;
using Common.Drawing.Drawing2D;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class StringRenderer
    {
        private static readonly GraphicsPath FontPath = new GraphicsPath();

        internal static void DrawString(Graphics g, string s, Font font, Brush brush, RectangleF layoutRectangle,
            StringFormat format)
        {
            FontPath.Reset();
            FontPath.AddString(s, font.FontFamily, (int) font.Style, font.SizeInPoints, layoutRectangle, format);
            g.FillPathFast(brush, FontPath);
        }
    }
}