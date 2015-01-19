using System.Drawing;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class CenterLabelRenderer
    {
        internal static void DrawCenterLabel(Graphics g, RectangleF outerBounds, string label, PrivateFontCollection fonts)
        {
            var fontFamily = fonts.Families[0];
            var labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            var labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                            StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;
            var charSize = g.MeasureString("A", labelFont);
            var labelRect = new RectangleF(
                new PointF(
                    0,
                    (((outerBounds.Y + outerBounds.Height) - charSize.Height)/2.0f)
                    ),
                new SizeF(
                    outerBounds.Width,
                    charSize.Height
                    )
                );
            labelRect.Offset(0, -40);

            g.DrawString(label, labelFont, Brushes.White, labelRect, labelStringFormat);
        }
    }
}