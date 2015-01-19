using System.Drawing;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class MiddleLethalityRingRenderer
    {
        internal static void DrawMiddleLethalityRing(Graphics gfx, float middleRingLeft, float middleRingTop, Pen grayPen,
            float atdMiddleRingDiameter, int lineLength, InstrumentState instrumentState)
        {
            var toRestore = gfx.Transform;
            gfx.TranslateTransform(middleRingLeft, middleRingTop);
            gfx.DrawEllipse(grayPen, 0, 0, atdMiddleRingDiameter, atdMiddleRingDiameter);

            gfx.TranslateTransform(atdMiddleRingDiameter/2.0f, atdMiddleRingDiameter/2.0f);
            gfx.RotateTransform(-instrumentState.MagneticHeadingDegrees);
            gfx.TranslateTransform(-atdMiddleRingDiameter/2.0f, -atdMiddleRingDiameter/2.0f);

            //draw north line
            gfx.DrawLine(grayPen, atdMiddleRingDiameter/2.0f, -(lineLength*2), atdMiddleRingDiameter/2.0f, 0);

            //draw west line
            gfx.DrawLine(grayPen, 0, (atdMiddleRingDiameter/2.0f), lineLength, (atdMiddleRingDiameter/2.0f));

            //draw east line
            gfx.DrawLine(grayPen, atdMiddleRingDiameter, (atdMiddleRingDiameter/2.0f),
                atdMiddleRingDiameter - lineLength, (atdMiddleRingDiameter/2.0f));

            //draw south line
            gfx.DrawLine(grayPen, (atdMiddleRingDiameter/2.0f), atdMiddleRingDiameter - lineLength,
                (atdMiddleRingDiameter/2.0f), atdMiddleRingDiameter + lineLength);

            //draw north flag
            gfx.DrawLine(grayPen, (atdMiddleRingDiameter/2.0f), -(lineLength*2),
                (atdMiddleRingDiameter/2.0f) + 7, -(lineLength*0.75f));
            gfx.DrawLine(grayPen, (atdMiddleRingDiameter/2.0f), -(lineLength*0.75f),
                (atdMiddleRingDiameter/2.0f) + 7, -(lineLength*0.75f));

            gfx.Transform = toRestore;
        }
    }
}