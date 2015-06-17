using System;
using System.Drawing;
using Common.Imaging;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IInnerMapRangeCircleRenderer
    {
        void DrawInnerMapRangeCircle(Graphics g, Size theaterMapSizePixels, Pen mapRingPen, int mapRingLineWidths, int innerMapRingRadiusPixels, out Rectangle innerMapRingBoundingRect, out int innerMapRingBoundingRectMiddleX);
    }

    internal class InnerMapRangeCircleRenderer : IInnerMapRangeCircleRenderer
    {
        public void DrawInnerMapRangeCircle(Graphics g, Size theaterMapSizePixels, Pen mapRingPen, int mapRingLineWidths, int innerMapRingRadiusPixels, out Rectangle innerMapRingBoundingRect, out int innerMapRingBoundingRectMiddleX)
        {
            //draw inner map range circle
            innerMapRingBoundingRect =
                new Rectangle(((theaterMapSizePixels.Width - (innerMapRingRadiusPixels*2))/2),
                    ((theaterMapSizePixels.Height - (innerMapRingRadiusPixels*2))/2),
                    innerMapRingRadiusPixels*2, innerMapRingRadiusPixels*2);
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