using System.Drawing;
using System.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class TextWarningFlagRenderer
    {
        private static Font _inuFlagFont;
        internal static void DrawTextWarningFlag(Graphics g, string flagText, Color flagColor,
            Color textColor, PrivateFontCollection fonts)
        {
            Brush flagBrush = new SolidBrush(flagColor);
            if (_inuFlagFont ==null)
            {
                var fontFamily = fonts.Families[0];
                _inuFlagFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);
            }
            var inuFlagStringFormat = new StringFormat();
            inuFlagStringFormat.Alignment = StringAlignment.Center;
            inuFlagStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                              StringFormatFlags.NoWrap;
            inuFlagStringFormat.LineAlignment = StringAlignment.Center;
            inuFlagStringFormat.Trimming = StringTrimming.None;

            var flagLocation = new PointF(12, 75);
            var flagSize = new SizeF(25, 75);
            var flagRect = new RectangleF(flagLocation, flagSize);
            g.FillRectangle(flagBrush, flagRect);
            Brush textBrush = new SolidBrush(textColor);
            g.DrawStringFast(flagText, _inuFlagFont, textBrush, flagRect, inuFlagStringFormat);
        }
    }
}