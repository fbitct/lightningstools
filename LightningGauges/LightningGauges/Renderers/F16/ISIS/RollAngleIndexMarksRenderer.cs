using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class RollAngleIndexMarksRenderer
    {
        internal static float DrawRollAngleIndexMarks(Graphics g, int width, int height, float pixelsPerDegreePitch,
            ref GraphicsState basicState, out Pen rollIndexPen)
        {
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            const float majorIndexLineLength = 15;
            const float minorIndexLineLength = majorIndexLineLength/2.0f;
            var radiusFromCenterToBottomOfIndexLine = pixelsPerDegreePitch*20.0f;
            var startingTransform = g.Transform;
            rollIndexPen = new Pen(Color.White) {Width = 2};
            for (var i = -60; i <= 60; i += 5)
            {
                var drawLine = false;
                var lineLength = minorIndexLineLength;
                if (Math.Abs(i) == 60 || Math.Abs(i) == 30)
                {
                    drawLine = true;
                    lineLength = majorIndexLineLength;
                }
                else if (Math.Abs(i) == 45 || Math.Abs(i) == 20 || Math.Abs(i) == 10)
                {
                    drawLine = true;
                }
                g.Transform = startingTransform;
                g.TranslateTransform(width/2.0f, height/2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-(float) width/2.0f, -(float) height/2.0f);

                if (drawLine)
                {
                    g.DrawLine(rollIndexPen,
                        new PointF((width/2.0f), (height/2.0f) - radiusFromCenterToBottomOfIndexLine),
                        new PointF((width/2.0f), (height/2.0f) - radiusFromCenterToBottomOfIndexLine - lineLength)
                        );
                }
            }
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            return radiusFromCenterToBottomOfIndexLine;
        }
    }
}