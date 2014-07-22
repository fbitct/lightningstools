using System;
using System.Drawing;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IOuterMapRangeCircleRenderer
    {
        void DrawOuterMapRangeCircle(Graphics g, Rectangle renderRectangle,
            float outerMapRingDiameterPixelsUnscaled, out int outerMapRingDiameterPixelsScaled,
            float renderRectangleScaleFactor, Pen mapRingPen, int mapRingLineWidths);
    }

    internal class OuterMapRangeCircleRenderer : IOuterMapRangeCircleRenderer
    {
        public void DrawOuterMapRangeCircle(Graphics g, Rectangle renderRectangle,
            float outerMapRingDiameterPixelsUnscaled, out int outerMapRingDiameterPixelsScaled,
            float renderRectangleScaleFactor, Pen mapRingPen, int mapRingLineWidths)
        {
            //rotate 45 degrees before drawing outer map range circle
            var preRotate = g.Transform;
            //capture current rotation so we can set it back before drawing inner map range circle
            g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
            g.RotateTransform(-45);
            g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

            //now draw outer map range circle
            outerMapRingDiameterPixelsScaled =
                (int) Math.Floor(outerMapRingDiameterPixelsUnscaled*renderRectangleScaleFactor);
            var outerMapRingBoundingRect =
                new Rectangle(((renderRectangle.Width - outerMapRingDiameterPixelsScaled)/2),
                    ((renderRectangle.Height - outerMapRingDiameterPixelsScaled)/2),
                    outerMapRingDiameterPixelsScaled, outerMapRingDiameterPixelsScaled);
            g.DrawEllipse(mapRingPen, outerMapRingBoundingRect);
            var outerMapRingBoundingRectMiddleX = outerMapRingBoundingRect.X +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Width/(float) 2));
            var outerMapRingBoundingRectMiddleY = outerMapRingBoundingRect.Y +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Height/(float) 2));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Top),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Top + mapRingLineWidths));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRect.X, outerMapRingBoundingRectMiddleY),
                new Point(outerMapRingBoundingRect.X + mapRingLineWidths, outerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen,
                new Point(outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width,
                    outerMapRingBoundingRectMiddleY),
                new Point(
                    outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width - mapRingLineWidths,
                    outerMapRingBoundingRectMiddleY));
            g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Bottom),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Bottom - mapRingLineWidths));

            //set rotation back before drawing inner map range circle
            g.Transform = preRotate;
        }
    }
}