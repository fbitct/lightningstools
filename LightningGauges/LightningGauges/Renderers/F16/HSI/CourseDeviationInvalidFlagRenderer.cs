using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.HSI
{
    internal static class CourseDeviationInvalidFlagRenderer
    {
        internal static void DrawCourseDeviationInvalidFlag(Graphics destinationGraphics, ref GraphicsState basicState, float centerX, float centerY, InstrumentState instrumentState, Image hsiCDIFlagMaskedImage)
        {
            //draw the CDI flag
            if (instrumentState.DeviationInvalidFlag)
            {
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
                destinationGraphics.TranslateTransform(centerX, centerY);
                destinationGraphics.RotateTransform(-instrumentState.MagneticHeadingDegrees);
                destinationGraphics.RotateTransform(instrumentState.DesiredCourseDegrees);
                destinationGraphics.TranslateTransform(-centerX, -centerY);
                destinationGraphics.DrawImage(hsiCDIFlagMaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(destinationGraphics, ref basicState);
            }
        }
    }
}