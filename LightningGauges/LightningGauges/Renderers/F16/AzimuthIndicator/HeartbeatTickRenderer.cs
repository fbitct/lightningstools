using System;
using System.Drawing;

namespace LightningGauges.Renderers.F16.AzimuthIndicator
{
    internal static class HeartbeatTickRenderer
    {
        internal static void DrawHeartbeatTick(Graphics gfx, int backgroundWidth, int backgroundHeight, Pen scopeGreenPen)
        {
            if (DateTime.Now.Millisecond < 500)
            {
                gfx.DrawLine(scopeGreenPen,
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f)),
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f) + 5)
                    );
            }
            else
            {
                gfx.DrawLine(scopeGreenPen,
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f)),
                    new PointF((backgroundWidth/2.0f) + 10, (backgroundHeight/2.0f) - 5)
                    );
            }
        }
    }
}