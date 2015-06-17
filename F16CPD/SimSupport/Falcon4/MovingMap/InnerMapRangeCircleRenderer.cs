using System;
using System.Drawing;
using Common.Imaging;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IInnerMapRangeCircleRenderer
    {
        void DrawInnerMapRangeCircle(Graphics g, Rectangle renderRectangle, Pen mapRingPen, int mapRingLineWidths, int outerMapRingRadiusPixelsScaled, out Rectangle innerMapRingBoundingRect, out int innerMapRingBoundingRectMiddleX);
    }

    internal class InnerMapRangeCircleRenderer : IInnerMapRangeCircleRenderer
    {
        public void DrawInnerMapRangeCircle(Graphics g, Rectangle renderRectangle, Pen mapRingPen, int mapRingLineWidths, int outerMapRingRadiusPixelsScaled, out Rectangle innerMapRingBoundingRect, out int innerMapRingBoundingRectMiddleX)
        {
            //draw inner map range circle
            var innerMapRingRadiusPixelsScaled = (int) (Math.Floor(outerMapRingRadiusPixelsScaled/2.0f));
            innerMapRingBoundingRect =
                new Rectangle(((renderRectangle.Width - (innerMapRingRadiusPixelsScaled*2))/2),
                    ((renderRectangle.Height - (innerMapRingRadiusPixelsScaled*2))/2),
                    innerMapRingRadiusPixelsScaled*2, innerMapRingRadiusPixelsScaled*2);
            g.DrawEllipse(mapRingPen, innerMapRingBoundingRect);
            innerMapRingBoundingRectMiddleX = innerMapRingBoundingRect.X +
                                              (int) (Math.Floor(innerMapRingBoundingRect.Width/(float) 2));
            var innerMapRingBoundingRectMiddleY = innerMapRingBoundingRect.Y +
                                                  (int) (Math.Floor(innerMapRingBoundingRect.Height/(float) 2));
            g.DrawLineFast(mapRingPen, new Point(innerMapRingBoundingRect.X, innerMapRingBoundingRectMiddleY),
                new Point(innerMapRingBoundingRect.X + mapRingLineWidths, innerMapRingBoundingRectMiddleY));
            g.DrawLineFast(mapRingPen,
                new Point(innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width,
                    innerMapRingBoundingRectMiddleY),
                new Point(
                    innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width - mapRingLineWidths,
                    innerMapRingBoundingRectMiddleY));
            g.DrawLineFast(mapRingPen, new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Bottom),
                new Point(innerMapRingBoundingRectMiddleX,
                    innerMapRingBoundingRect.Bottom - mapRingLineWidths));
        }
    }
}