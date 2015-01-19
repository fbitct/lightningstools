using System;
using System.Drawing;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class PitchLadderRenderer
    {
        internal static void DrawPitchLadder(Graphics g, RectangleF bounds, float pitchDegrees, PrivateFontCollection fonts)
        {
            var pitchDigitFont = new Font(fonts.Families[0], 12, FontStyle.Bold, GraphicsUnit.Point);
            var pitchDigitFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };


            var pitchLadderWidth = bounds.Width;
            var pitchLadderHeight = bounds.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;

            const int minorLineWidth = 15;
            var groundColor = Color.FromArgb(111, 72, 31);
            Brush groundBrush = new SolidBrush(groundColor);
            var skyColor = Color.FromArgb(3, 174, 252);
            Brush skyBrush = new SolidBrush(skyColor);
            var pitchBarColor = Color.White;
            var pitchDigitColor = Color.White;
            Brush pitchDigitBrush = new SolidBrush(pitchDigitColor);
            var pitchBarPen = new Pen(pitchBarColor);
            const int pitchBarPenWidth = 2;
            pitchBarPen.Width = pitchBarPenWidth;
            var pixelsPerDegree = pitchLadderHeight/(180.0f + 90);

            //draw the ground
            g.FillRectangle(groundBrush,
                new RectangleF(0, pitchLadderHeight/2.0f, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the sky
            g.FillRectangle(skyBrush, new RectangleF(0, 0, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the horizon line
            g.DrawLine(pitchBarPen, new PointF(0, (pitchLadderHeight/2.0f)),
                new PointF(pitchLadderWidth, (pitchLadderHeight/2.0f)));

            //draw zenith/nadir symbol
            const float zenithNadirSymbolWidth = minorLineWidth;
            DrawZenithNadirSymbol(g, pitchLadderHeight, pixelsPerDegree, pitchLadderWidth, zenithNadirSymbolWidth, pitchBarPen);
            DrawClimbPitchBars(g, pitchDegrees, minorLineWidth, pitchLadderWidth, pitchLadderHeight, pixelsPerDegree, pitchBarPen, pitchDigitFont, pitchDigitBrush, pitchDigitFormat);

            g.Transform = startingTransform;
            g.Clip = startingClip;
        }

        private static void DrawZenithNadirSymbol(Graphics g, float pitchLadderHeight, float pixelsPerDegree,
            float pitchLadderWidth, float zenithNadirSymbolWidth, Pen pitchBarPen)
        {
            {
                var y = (pitchLadderHeight/2.0f) - (90*pixelsPerDegree);
                var zenithOrNadirRectangle = new RectangleF((pitchLadderWidth/2.0f) - (zenithNadirSymbolWidth/2.0f),
                    y - (zenithNadirSymbolWidth/2.0f), zenithNadirSymbolWidth,
                    zenithNadirSymbolWidth);
                g.DrawEllipse(pitchBarPen, zenithOrNadirRectangle);
            }
            {
                var y = (pitchLadderHeight/2.0f) - (-90*pixelsPerDegree);
                var zenithOrNadirRectangle = new RectangleF((pitchLadderWidth/2.0f) - (zenithNadirSymbolWidth/2.0f),
                    y - (zenithNadirSymbolWidth/2.0f), zenithNadirSymbolWidth,
                    zenithNadirSymbolWidth);
                g.DrawEllipse(pitchBarPen, zenithOrNadirRectangle);
                g.DrawLine(pitchBarPen, new PointF(zenithOrNadirRectangle.Left, zenithOrNadirRectangle.Top),
                    new PointF(zenithOrNadirRectangle.Right, zenithOrNadirRectangle.Bottom));
                g.DrawLine(pitchBarPen, new PointF(zenithOrNadirRectangle.Left, zenithOrNadirRectangle.Bottom),
                    new PointF(zenithOrNadirRectangle.Right, zenithOrNadirRectangle.Top));
            }
        }

        private static void DrawClimbPitchBars(Graphics g, float pitchDegrees, int minorLineWidth, float pitchLadderWidth,
            float pitchLadderHeight, float pixelsPerDegree, Pen pitchBarPen, Font pitchDigitFont, Brush pitchDigitBrush,
            StringFormat pitchDigitFormat)
        {
            //draw the climb pitch bars
            for (float i = -85; i <= 85; i += 2.5f)
            {
                if (i < (pitchDegrees - 15) || i > (pitchDegrees + 15.0f)) continue;
                float lineWidth = minorLineWidth;
                if (i%5 == 0) lineWidth *= 2;
                if (i%10 == 0) lineWidth *= 2;
                if (i == 0) lineWidth = pitchLadderWidth;
                var y = (pitchLadderHeight/2.0f) - (i*pixelsPerDegree);

                //draw this line
                g.DrawLine(pitchBarPen, new PointF((pitchLadderWidth/2.0f) - (lineWidth/2.0f), y),
                    new PointF((pitchLadderWidth/2.0f) + (lineWidth/2.0f), y));

                //draw the pitch digits
                if (i%10 == 0)
                {
                    var toDisplayVal = (int) Math.Abs(i);
                    var toDisplayString = string.Format("{0:##}", toDisplayVal);

                    var digitSize = g.MeasureString(toDisplayString, pitchDigitFont);
                    var lhsRect = new RectangleF(
                        new PointF((pitchLadderWidth/2.0f) - (lineWidth/2.0f) - digitSize.Width,
                            y - (digitSize.Height/2.0f)),
                        digitSize);
                    g.DrawString(toDisplayString, pitchDigitFont, pitchDigitBrush, lhsRect, pitchDigitFormat);

                    var rhsRect = new RectangleF(
                        new PointF((pitchLadderWidth/2.0f) + (lineWidth/2.0f), y - (digitSize.Height/2.0f)),
                        digitSize);
                    g.DrawString(toDisplayString, pitchDigitFont, pitchDigitBrush, rhsRect, pitchDigitFormat);
                }
            }
        }
    }
}