using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class RadarAltimeterAreaRenderer
    {
        internal static void DrawRadarAltimeterArea(Graphics gfx, ref GraphicsState basicState, RectangleF topRectangle, InstrumentState instrumentState, Options options, PrivateFontCollection fonts)
        {
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
            const float raltRectangleWidth = 80;
            var raltRectangle = new RectangleF((topRectangle.Width/2.0f) - (raltRectangleWidth/2.0f), 10,
                raltRectangleWidth, topRectangle.Height - 10);
            var raltRectanglePen = Pens.White;
            raltRectangle.Offset(-5, 0);
            gfx.DrawRectangle(raltRectanglePen, raltRectangle.X, raltRectangle.Y, raltRectangle.Width,
                raltRectangle.Height);

            var raltStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };
            var ralt = instrumentState.RadarAltitudeAGL;
            var raltString = string.Format("{0:#####0}", ralt);
            var raltColor = Color.FromArgb(183, 243, 244);
            Brush raltBrush = new SolidBrush(raltColor);
            float fontSize = 20;

            if (
                (!raltString.StartsWith("-") && raltString.Length > 4)
                ||
                (raltString.StartsWith("-") && raltString.Length > 5)
                )
            {
                fontSize = 18;
            }

            if (
                (!raltString.StartsWith("-") && raltString.Length > 5)
                ||
                (raltString.StartsWith("-") && raltString.Length > 6)
                )
            {
                fontSize = 15;
            }
            if (options.RadarAltitudeUnits == AltitudeUnits.Meters)
            {
                raltString += "m";
            }
            else
            {
                raltString += "ft";
            }

            gfx.DrawString(raltString, new Font(fonts.Families[0], fontSize, FontStyle.Regular, GraphicsUnit.Point),
                raltBrush, raltRectangle, raltStringFormat);
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
        }
    }
}