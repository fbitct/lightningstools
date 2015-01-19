using System.Drawing;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class ChaffCountRenderer
    {
        internal static void DrawChaffCount(Color severeColor, Color warnColor, Color okColor, Graphics gfx,
            RectangleF chaffCountRectangle, StringFormat chaffFlareCountStringFormat, InstrumentState instrumentState, Font font)
        {
            //draw chaff count
            Color chaffCountColor;
            if (instrumentState.ChaffCount == 0)
            {
                chaffCountColor = severeColor;
            }
            else if (instrumentState.ChaffLow)
            {
                chaffCountColor = warnColor;
            }
            else
            {
                chaffCountColor = okColor;
            }
            Brush chaffCountBrush = new SolidBrush(chaffCountColor);
            StringRenderer.DrawString(gfx, "CHAF", font, chaffCountBrush, chaffCountRectangle,
                chaffFlareCountStringFormat);
            chaffCountRectangle.Offset(0, 12);
            StringRenderer.DrawString(gfx, string.Format("{0:00}", instrumentState.ChaffCount), font,
                chaffCountBrush, chaffCountRectangle, chaffFlareCountStringFormat);
        }
    }
}