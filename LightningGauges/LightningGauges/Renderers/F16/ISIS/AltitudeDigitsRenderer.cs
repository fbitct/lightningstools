using System;
using System.Drawing;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class AltitudeDigitsRenderer
    {
        internal static void DrawAltitudeDigits(Graphics g, float digit, RectangleF layoutRectangle, RectangleF clipRectangle,
            float pointSize, bool goByTwenty, bool cyclical, StringAlignment alignment, PrivateFontCollection fonts)
        {
            var digitFont = new Font(fonts.Families[0], pointSize, FontStyle.Regular, GraphicsUnit.Point);
            var digitSF = new StringFormat
            {
                Alignment = alignment,
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };


            var digitBrush = Brushes.White;
            var initialClip = g.Clip;
            var initialState = g.Save();

            g.SetClip(clipRectangle);
            var basicState = g.Save();
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            const float digitSpacing = 0;
            if (cyclical)
            {
                for (var i = -1; i <= 11; i++)
                {
                    var thisDigit = 0;
                    if (i >= 0)
                    {
                        thisDigit = i % 10;
                    }
                    else
                    {
                        thisDigit = (10 - (Math.Abs(i) % 10));
                    }

                    var toDraw = string.Format("{0:#0}", thisDigit);
                    if (goByTwenty)
                    {
                        toDraw = string.Format("{0:00}", (thisDigit * 20) % 100);
                        if (toDraw == "100") toDraw = "00";
                    }
                    var digitSize = g.MeasureString(toDraw, digitFont);
                    var layoutRectangle2 = new RectangleF(
                        layoutRectangle.X,
                        layoutRectangle.Y - (i * (digitSize.Height + digitSpacing)),
                        layoutRectangle.Width, digitSize.Height
                        );
                    layoutRectangle2.Offset(0, ((digitSize.Height + digitSpacing) * digit));
                    g.DrawString(toDraw, digitFont, digitBrush, layoutRectangle2, digitSF);
                }
            }
            else
            {
                var thisDigit = (int)Math.Floor(digit);
                var toDraw = string.Format("{0:0}", thisDigit);
                if (toDraw.Length > 1) toDraw = toDraw.Substring(0, 1);
                if (goByTwenty)
                {
                    toDraw = string.Format("{0:00}", thisDigit * 20);
                }
                var digitSize = g.MeasureString(toDraw, digitFont);
                var layoutRectangle2 = new RectangleF(
                    layoutRectangle.X,
                    layoutRectangle.Y - (digit * (digitSize.Height + digitSpacing)),
                    layoutRectangle.Width, digitSize.Height
                    );
                layoutRectangle2.Offset(0, ((digitSize.Height + digitSpacing) * digit));
                g.DrawString(toDraw, digitFont, digitBrush, layoutRectangle2, digitSF);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
            g.Clip = initialClip;
        }
    }
}