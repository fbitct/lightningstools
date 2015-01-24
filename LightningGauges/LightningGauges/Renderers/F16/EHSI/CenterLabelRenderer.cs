using System.Drawing;
using System.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class CenterLabelRenderer
    {
        private static Font _labelFont;
        internal static void DrawCenterLabel(Graphics g, RectangleF outerBounds, string label, PrivateFontCollection fonts)
        {
            if (_labelFont == null)
            {
                var fontFamily = fonts.Families[0];
                _labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);
            }

            var labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                            StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;
            var charSize = g.MeasureString("A", _labelFont);
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

            g.DrawStringFast(label, _labelFont, Brushes.White, labelRect, labelStringFormat);
        }
    }
}