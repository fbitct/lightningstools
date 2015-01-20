using System.Drawing;
using System.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class DistanceToBeaconRenderer
    {
        internal static void DrawDistanceToBeacon(Graphics g, PrivateFontCollection fonts, InstrumentState instrumentState, Options options)
        {
            var fontFamily = fonts.Families[0];
            var digitsFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);
            var nmFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            var distanceDigitStringFormat = new StringFormat();
            distanceDigitStringFormat.Alignment = StringAlignment.Center;
            distanceDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                    StringFormatFlags.NoWrap;
            distanceDigitStringFormat.LineAlignment = StringAlignment.Center;
            distanceDigitStringFormat.Trimming = StringTrimming.None;

            var nmStringFormat = new StringFormat();
            nmStringFormat.Alignment = StringAlignment.Center;
            nmStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                         StringFormatFlags.NoWrap;
            nmStringFormat.LineAlignment = StringAlignment.Center;
            nmStringFormat.Trimming = StringTrimming.None;

            var initialState = g.Save();
            g.InterpolationMode = options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = options.GDIPlusOptions.TextRenderingHint;
            var basicState = g.Save();

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            var distanceToBeaconString = string.Format("{0:000.0}", instrumentState.DistanceToBeaconNauticalMiles);
            var hundredsDigit = distanceToBeaconString.Substring(0, 1);
            var tensDigit = distanceToBeaconString.Substring(1, 1);
            var onesDigit = distanceToBeaconString.Substring(2, 1);
            var tenthsDigit = distanceToBeaconString.Substring(4, 1);

            float digitWidth = 22;
            float digitHeight = 32;
            float digitSeparationPixels = -4;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            var hundredsRect = new RectangleF(12, 8, digitWidth, digitHeight);
            var tensRect = new RectangleF(hundredsRect.X + digitWidth + digitSeparationPixels, hundredsRect.Y,
                digitWidth, digitHeight);
            var onesRect = new RectangleF(tensRect.X + digitWidth + digitSeparationPixels, tensRect.Y, digitWidth,
                digitHeight);
            var tenthsRect = new RectangleF(onesRect.X + digitWidth + 4, onesRect.Y, digitWidth, digitHeight);

            g.DrawStringFast(hundredsDigit, digitsFont, Brushes.White, hundredsRect, distanceDigitStringFormat);
            g.DrawStringFast(tensDigit, digitsFont, Brushes.White, tensRect, distanceDigitStringFormat);
            g.DrawStringFast(onesDigit, digitsFont, Brushes.White, onesRect, distanceDigitStringFormat);

            g.FillRectangle(Brushes.White, tenthsRect);
            g.DrawStringFast(tenthsDigit, digitsFont, Brushes.Black, tenthsRect, distanceDigitStringFormat);

            if (instrumentState.DmeInvalidFlag)
            {
                var dmeInvalidFlagUpperLeft = new PointF(hundredsRect.X, hundredsRect.Y + 8);
                var dmeInvalidFlagSize = new SizeF((tenthsRect.X + tenthsRect.Width) - hundredsRect.X, 16);
                var dmeInvalidFlagRect = new RectangleF(dmeInvalidFlagUpperLeft, dmeInvalidFlagSize);
                var redFlagColor = Color.FromArgb(224, 43, 48);
                Brush redFlagBrush = new SolidBrush(redFlagColor);
                g.FillRectangle(redFlagBrush, dmeInvalidFlagRect);
            }

            var nmRect = new RectangleF(hundredsRect.X, 45, 30, 20);
            g.DrawStringFast("NM", nmFont, Brushes.White, nmRect, nmStringFormat);

            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }
    }
}