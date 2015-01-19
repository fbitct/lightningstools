using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class HeadingTapeRenderer
    {
        internal static SizeF DrawHeadingTape(Graphics gfx, int width, int height, ref GraphicsState basicState, InstrumentState instrumentState, PrivateFontCollection fonts)
        {
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
            var headingTapeSize = new SizeF(width - 96, 25);
            gfx.TranslateTransform(40, height - headingTapeSize.Height);
            var heading = instrumentState.MagneticHeadingDegrees;
            DrawHeadingTape(gfx, headingTapeSize, heading, fonts);
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
            return headingTapeSize;
        }


        private static void DrawHeadingTape(Graphics g, SizeF size, float magneticHeadingDegrees, PrivateFontCollection fonts)
        {
            var headingDigitFont = new Font(fonts.Families[0], 16, FontStyle.Bold, GraphicsUnit.Point);
            var headingDigitFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };
            var headingDigitColor = Color.White;
            Brush headingDigitBrush = new SolidBrush(headingDigitColor);
            var headingDigitPen = new Pen(headingDigitColor) {Width = 2};


            //draw the background
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, (int) size.Width, (int) size.Height));
            var pixelsPerDegree = (size.Width/30.0f);
            g.TranslateTransform(-pixelsPerDegree*magneticHeadingDegrees, 0);
            for (var i = -30; i <= 400; i += 5)
            {
                float angle = i;
                if (i < 0) angle = 360 + i;
                if (Math.Abs(angle)%10 == 0)
                {
                    var toDisplay = string.Format("{0:000}", Math.Abs(angle)%360);

                    var toDisplaySize = g.MeasureString(toDisplay, headingDigitFont);
                    var x = (i*pixelsPerDegree) + (size.Width/2.0f) - (toDisplaySize.Width/2.0f);
                    var y = (size.Height/2.0f) - (toDisplaySize.Height/2.0f);
                    var layoutRect = new RectangleF(x, y, toDisplaySize.Width, toDisplaySize.Height);
                    g.DrawString(toDisplay, headingDigitFont, headingDigitBrush, layoutRect, headingDigitFormat);

                    //draw point above text
                    {
                        var xprime = (i*pixelsPerDegree) + (size.Width/2.0f);
                        const float yprime = 2;
                        const float yprimeprime = 4;
                        g.DrawLine(headingDigitPen, new PointF(xprime, yprime), new PointF(xprime, yprimeprime));
                    }
                }
                else if (Math.Abs(i)%5 == 0)
                {
                    //draw point indicating 5 degree mark
                    var x = (i*pixelsPerDegree) + (size.Width/2.0f);
                    const float y = 2;
                    const float yPrime = 3;
                    g.DrawLine(headingDigitPen, new PointF(x, y), new PointF(x, yPrime));
                }
            }
        }
    }
}