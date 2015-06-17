using System;
using System.Drawing;
using Common.Imaging;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IOuterMapRangeCircleRenderer
    {
        void DrawOuterMapRangeCircle(Graphics g, Rectangle renderRectangle,
            float outerMapRingRadiusPixelsUnscaled, out int outerMapRingRadiusPixelsScaled,
            float renderRectangleScaleFactor, Pen mapRingPen, int mapRingLineWidths);
    }

    internal class OuterMapRangeCircleRenderer : IOuterMapRangeCircleRenderer
    {
        public void DrawOuterMapRangeCircle(Graphics g, Rectangle renderRectangle,
            float outerMapRingRadiusPixelsUnscaled, out int outerMapRingRadiusPixelsScaled,
            float renderRectangleScaleFactor, Pen mapRingPen, int mapRingLineWidths)
        {
            //rotate 45 degrees before drawing outer map range circle
            var preRotate = g.Transform;
            //capture current rotation so we can set it back before drawing inner map range circle
            g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
            g.RotateTransform(-45);
            g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

            //now draw outer map range circle
            outerMapRingRadiusPixelsScaled =
                (int) Math.Floor(outerMapRingRadiusPixelsUnscaled*renderRectangleScaleFactor);
            var outerMapRingBoundingRect =
                new Rectangle(((renderRectangle.Width - (outerMapRingRadiusPixelsScaled*2))/2),
                    ((renderRectangle.Height - (outerMapRingRadiusPixelsScaled*2))/2),
                    outerMapRingRadiusPixelsScaled*2, outerMapRingRadiusPixelsScaled*2);
            g.DrawEllipse(mapRingPen, outerMapRingBoundingRect);
            var outerMapRingBoundingRectMiddleX = outerMapRingBoundingRect.X +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Width/(float) 2));
            var outerMapRingBoundingRectMiddleY = outerMapRingBoundingRect.Y +
                                                  (int) (Math.Floor(outerMapRingBoundingRect.Height/(float) 2));
            g.DrawLineFast(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Top),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Top + mapRingLineWidths));
            g.DrawLineFast(mapRingPen, new Point(outerMapRingBoundingRect.X, outerMapRingBoundingRectMiddleY),
                new Point(outerMapRingBoundingRect.X + mapRingLineWidths, outerMapRingBoundingRectMiddleY));
            g.DrawLineFast(mapRingPen,
                new Point(outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width,
                    outerMapRingBoundingRectMiddleY),
                new Point(
                    outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width - mapRingLineWidths,
                    outerMapRingBoundingRectMiddleY));
            g.DrawLineFast(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Bottom),
                new Point(outerMapRingBoundingRectMiddleX,
                    outerMapRingBoundingRect.Bottom - mapRingLineWidths));

            //set rotation back before drawing inner map range circle
            g.Transform = preRotate;
        }
    }
}