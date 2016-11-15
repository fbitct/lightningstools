using System;
using Common.Drawing;
using Common.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class AirspeedDigitsRenderer
    {
        private static readonly StringFormat DigitSf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        private static readonly Brush DigitBrush = Brushes.White;
        private static Font _digitFont;

        internal static void DrawAirspeedDigits(Graphics g, float digit, RectangleF layoutRectangle, RectangleF clipRectangle,
            float pointSize, bool cyclical, PrivateFontCollection fonts)
        {
            if (_digitFont == null)
            {
                 _digitFont = new Font(fonts.Families[0], pointSize, FontStyle.Regular, GraphicsUnit.Point);
            }
            var initialClip = g.Clip;
            var initialState = g.Save();

            g.SetClip(clipRectangle);
            var basicState = g.Save();
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            const float digitSpacing = 0;
            float start = -1;
            float end = 11;
            if (!cyclical)
            {
                start = digit;
                end = digit;
            }
            for (var i = start; i <= end; i++)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float thisDigit;
                if (i >= 0)
                {
                    thisDigit = i%10;
                }
                else
                {
                    thisDigit = (10 - (Math.Abs(i)%10));
                }

                var toDraw = string.Format("{0:0}", thisDigit);
                var digitSize = g.MeasureString(toDraw, _digitFont);
                g.TranslateTransform(0,
                    ((digitSize.Height + digitSpacing)*digit) + (layoutRectangle.Height/4.0f) +
                    (digitSpacing/2.0f));
                var layoutRectangle2 = new RectangleF(
                    layoutRectangle.X,
                    layoutRectangle.Y - (i*(digitSize.Height + digitSpacing)),
                    layoutRectangle.Width, digitSize.Height
                    );
                g.DrawStringFast(toDraw, _digitFont, DigitBrush, layoutRectangle2, DigitSf);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
            g.Clip = initialClip;
        }
    }
}