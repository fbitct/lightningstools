using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class RangeToBeaconRenderer
    {
        internal static void DrawRangeToBeacon(Graphics destinationGraphics, ref GraphicsState basicState, InstrumentState instrumentState, FontGraphic rangeFontGraphic)
        {
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            var distanceToBeacon = instrumentState.DistanceToBeaconNauticalMiles;
            if (distanceToBeacon > 999.9) distanceToBeacon = 999.9f;
            var distanceToBeaconString = string.Format("{0:000.0}", distanceToBeacon);
            var distanceToBeaconHundreds = distanceToBeaconString[0];
            var distanceToBeaconTens = distanceToBeaconString[1];
            var distanceToBeaconOnes = distanceToBeaconString[2];
            var distanceToBeaconHundredsImage = rangeFontGraphic.GetCharImage(distanceToBeaconHundreds);
            var distanceToBeaconTensImage = rangeFontGraphic.GetCharImage(distanceToBeaconTens);
            var distanceToBeaconOnesImage = rangeFontGraphic.GetCharImage(distanceToBeaconOnes);

            var currentX = 29;
            var y = 45;
            const int spacingPixels = -5;
            destinationGraphics.DrawImage(distanceToBeaconHundredsImage, new Point(currentX, y));
            currentX += distanceToBeaconHundredsImage.Width + spacingPixels;
            destinationGraphics.DrawImage(distanceToBeaconTensImage, new Point(currentX, y));
            currentX += distanceToBeaconTensImage.Width + spacingPixels;
            destinationGraphics.DrawImage(distanceToBeaconOnesImage, new Point(currentX, y));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}