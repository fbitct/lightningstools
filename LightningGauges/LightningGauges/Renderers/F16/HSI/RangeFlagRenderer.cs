using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class RangeFlagRenderer
    {
        internal static void DrawRangeFlag(Graphics destinationGraphics, ref GraphicsState basicState, InstrumentState instrumentState, Image hsiRangeFlagMaskedImage)
        {
            //draw the range flag
            if (!instrumentState.DmeInvalidFlag) return;
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            destinationGraphics.DrawImage(hsiRangeFlagMaskedImage, new Point(0, 0));
            GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
        }
    }
}