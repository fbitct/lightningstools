using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class HeartbeatCrossRenderer
    {
        internal static void DrawHeartbeatCross(Graphics gfx, ref GraphicsState basicState, int backgroundWidth, int backgroundHeight, Pen scopeGreenPen)
        {
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

            //draw heartbeat cross
            gfx.DrawLine(scopeGreenPen,
                new PointF((backgroundWidth/2.0f) - 20, (backgroundHeight/2.0f)),
                new PointF((backgroundWidth/2.0f) - 10, (backgroundHeight/2.0f))
                );
            gfx.DrawLine(scopeGreenPen,
                new PointF((backgroundWidth/2.0f) + 20, (backgroundHeight/2.0f)),
                new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f))
                );
            gfx.DrawLine(scopeGreenPen,
                new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) - 20),
                new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) - 10)
                );
            gfx.DrawLine(scopeGreenPen,
                new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) + 20),
                new PointF((backgroundWidth/2.0f), (backgroundHeight/2.0f) + 10)
                );
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
        }
    }
}