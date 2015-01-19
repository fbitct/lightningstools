using System.Drawing;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class CompassRoseRenderer
    {
        internal static void DrawCompassRose(Graphics g, RectangleF outerBounds, PrivateFontCollection fonts, InstrumentState instrumentState, Options options)
        {
            var initialState = g.Save();
            g.InterpolationMode = options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = options.GDIPlusOptions.TextRenderingHint;
            var basicState = g.Save();

            var majorHeadingDigitStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };

            var fontFamily = fonts.Families[1];

            var majorHeadingDigitFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);

            var linePen = new Pen(Color.White);
            linePen.Width = 3;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            const float majorHeadingLineLength = 28;
            const float minorHeadingLineLength = majorHeadingLineLength/2.0f;
            const float majorHeadingLegendLayoutRectangleHeight = 30;
            const float majorHeadingLegendLayoutRectangleWidth = 30;
            var majorHeadingBrush = new SolidBrush(Color.White);

            var innerBounds = new RectangleF(outerBounds.X, outerBounds.Y, outerBounds.Width, outerBounds.Height);
            const float marginWidth = 30f;
            innerBounds.Inflate(-marginWidth, -marginWidth);

            for (var i = 0; i < 360; i += 45)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width/2.0f, innerBounds.Height/2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-innerBounds.Width/2.0f, -innerBounds.Height/2.0f);

                float separationPixels = 2;
                //draw 45-degree outer ticks
                if (i%90 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, -separationPixels),
                        new PointF(innerBounds.Width/2.0f, -((minorHeadingLineLength*1.5f) + separationPixels)));
                }
                else
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, -separationPixels),
                        new PointF(innerBounds.Width/2.0f, -((majorHeadingLineLength) + separationPixels)));
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }


            for (var i = 0; i < 360; i++)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
                g.RotateTransform(-instrumentState.MagneticHeadingDegrees);
                g.TranslateTransform(-outerBounds.Width/2.0f, -outerBounds.Height/2.0f);

                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width/2.0f, innerBounds.Height/2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-innerBounds.Width/2.0f, -innerBounds.Height/2.0f);
                if (i%10 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, 0),
                        new PointF(innerBounds.Width/2.0f, majorHeadingLineLength));
                }
                else if (i%5 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, 0),
                        new PointF(innerBounds.Width/2.0f, minorHeadingLineLength));
                }
                if (i%30 == 0)
                {
                    var majorHeadingLegendText = string.Format("{0:##}", i/10);
                    if (i == 90)
                    {
                        majorHeadingLegendText = "E";
                    }
                    else if (i == 180)
                    {
                        majorHeadingLegendText = "S";
                    }
                    else if (i == 270)
                    {
                        majorHeadingLegendText = "W";
                    }
                    else if (i == 0)
                    {
                        majorHeadingLegendText = "N";
                    }

                    var majorHeadingLegendLayoutRectangle =
                        new RectangleF(((innerBounds.Width/2.0f) - majorHeadingLegendLayoutRectangleWidth/2.0f),
                            (majorHeadingLegendLayoutRectangleHeight/2.0f),
                            majorHeadingLegendLayoutRectangleWidth, majorHeadingLegendLayoutRectangleHeight);
                    majorHeadingLegendLayoutRectangle.Offset(0, 18);
                    g.DrawString(majorHeadingLegendText, majorHeadingDigitFont, majorHeadingBrush,
                        majorHeadingLegendLayoutRectangle, majorHeadingDigitStringFormat);
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }
    }
}