using System;
using Common.Drawing;
using Common.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class PitchLadderRenderer
    {
        private static readonly StringFormat PitchDigitFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

       private const int MinorLineWidth = 15;
       private const int PitchBarPenWidth = 2;
       private static readonly Color GroundColor = Color.FromArgb(111, 72, 31);
       private static readonly Brush GroundBrush = new SolidBrush(GroundColor);
       private static readonly Color SkyColor = Color.FromArgb(3, 174, 252);
       private static readonly Brush SkyBrush = new SolidBrush(SkyColor);
       private static readonly Color PitchBarColor = Color.White;
       private static readonly Color PitchDigitColor = Color.White;
       private static readonly Brush PitchDigitBrush = new SolidBrush(PitchDigitColor);
       private static readonly Pen PitchBarPen = new Pen(PitchBarColor) { Width = PitchBarPenWidth };

       private static Font _pitchDigitFont;


        internal static void DrawPitchLadder(Graphics g, RectangleF bounds, float pitchDegrees, PrivateFontCollection fonts)
        {
            if (_pitchDigitFont == null)
            {
                _pitchDigitFont = new Font(fonts.Families[0], 12, FontStyle.Bold, GraphicsUnit.Point);
            }

            var pitchLadderWidth = bounds.Width;
            var pitchLadderHeight = bounds.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;

            var pixelsPerDegree = pitchLadderHeight/(180.0f + 90);

            //draw the ground
            g.FillRectangle(GroundBrush,
                new RectangleF(0, pitchLadderHeight/2.0f, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the sky
            g.FillRectangle(SkyBrush, new RectangleF(0, 0, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the horizon line
            g.DrawLineFast(PitchBarPen, new PointF(0, (pitchLadderHeight/2.0f)),
                new PointF(pitchLadderWidth, (pitchLadderHeight/2.0f)));

            //draw zenith/nadir symbol
            const float zenithNadirSymbolWidth = MinorLineWidth;
            ZenithNadirSymbolRenderer.DrawZenithNadirSymbol(g, pitchLadderHeight, pixelsPerDegree, pitchLadderWidth, zenithNadirSymbolWidth, PitchBarPen);
            ClimbPitchBarsRenderer.DrawClimbPitchBars(g, pitchDegrees, MinorLineWidth, pitchLadderWidth, pitchLadderHeight, pixelsPerDegree, PitchBarPen, _pitchDigitFont, PitchDigitBrush, PitchDigitFormat);

            g.Transform = startingTransform;
            g.Clip = startingClip;
        }
    }
}