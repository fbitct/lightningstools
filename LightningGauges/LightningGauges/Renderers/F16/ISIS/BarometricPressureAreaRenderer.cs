using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class BarometricPressureAreaRenderer
    {
        internal static void DrawBarometricPressureArea(Graphics gfx, ref GraphicsState basicState, RectangleF topRectangle, InstrumentState instrumentState, Options options, PrivateFontCollection fonts)
        {
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
            const int barometricPressureAreaWidth = 65;
            var barometricPressureStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Far,
                Trimming = StringTrimming.None
            };

            var pressure = instrumentState.BarometricPressure;

            var barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                20, barometricPressureAreaWidth,
                topRectangle.Height - 20);
            var barometricPressureBrush = Brushes.White;

            string baroString = null;
            string units = null;
            if (options.PressureAltitudeUnits == PressureUnits.InchesOfMercury)
            {
                //baroString = string.Format("{0:#0.00}", pressure);
                baroString = string.Format("{0:#0.00}", pressure/100);
                units = "in";
                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                    20, barometricPressureAreaWidth,
                    topRectangle.Height - 20);
            }
            else if (options.PressureAltitudeUnits == PressureUnits.Millibars)
            {
                baroString = string.Format("{0:###0}", pressure);
                units = "hPa";
                barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 25,
                    20, barometricPressureAreaWidth,
                    topRectangle.Height - 20);
            }
            gfx.DrawString(baroString, new Font(fonts.Families[0], 20, FontStyle.Regular, GraphicsUnit.Point),
                barometricPressureBrush, barometricPressureRectangle, barometricPressureStringFormat);

            var unitsRectangle = new RectangleF(topRectangle.Width - 22, 18, 15, topRectangle.Height - 20);
            gfx.DrawString(units, new Font(fonts.Families[0], 8, FontStyle.Regular, GraphicsUnit.Point),
                barometricPressureBrush, unitsRectangle, barometricPressureStringFormat);

            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
        }
    }
}