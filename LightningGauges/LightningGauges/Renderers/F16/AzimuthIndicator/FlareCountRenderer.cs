using System.Drawing;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class FlareCountRenderer
    {
        internal static void DrawFlareCount(Color severeColor, Color warnColor, Color okColor, Graphics gfx,
            RectangleF flareCountRectangle, StringFormat chaffFlareCountStringFormat, InstrumentState instrumentState, Font font)
        {
            Color flareCountColor;
            if (instrumentState.FlareCount == 0)
            {
                flareCountColor = severeColor;
            }
            else if (instrumentState.FlareLow)
            {
                flareCountColor = warnColor;
            }
            else
            {
                flareCountColor = okColor;
            }
            Brush flareCountBrush = new SolidBrush(flareCountColor);
            StringRenderer.DrawString(gfx, "FLAR", font, flareCountBrush, flareCountRectangle,
                chaffFlareCountStringFormat);
            flareCountRectangle.Offset(0, 12);
            StringRenderer.DrawString(gfx, string.Format("{0:00}", instrumentState.FlareCount), font,
                flareCountBrush, flareCountRectangle, chaffFlareCountStringFormat);
        }
    }
}