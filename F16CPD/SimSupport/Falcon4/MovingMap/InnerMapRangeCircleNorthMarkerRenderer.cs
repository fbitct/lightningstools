using System.Drawing;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IInnerMapRangeCircleNorthMarkerRenderer
    {
        void DrawNorthMarkerOnInnerMapRangeCircle(Graphics g, Brush mapRingBrush,
            Rectangle innerMapRingBoundingRect, int innerMapRingBoundingRectMiddleX);
    }

    internal class InnerMapRangeCircleNorthMarkerRenderer : IInnerMapRangeCircleNorthMarkerRenderer
    {
        public void DrawNorthMarkerOnInnerMapRangeCircle(Graphics g, Brush mapRingBrush,
            Rectangle innerMapRingBoundingRect, int innerMapRingBoundingRectMiddleX)
        {
            //draw north marker on inner map range circle
            var northMarkerPoints = new Point[3];
            northMarkerPoints[0] = new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Top - 15);
            northMarkerPoints[1] = new Point(innerMapRingBoundingRectMiddleX - 12,
                innerMapRingBoundingRect.Top + 1);
            northMarkerPoints[2] = new Point(innerMapRingBoundingRectMiddleX + 12,
                innerMapRingBoundingRect.Top + 1);
            g.FillPolygon(mapRingBrush, northMarkerPoints);
        }
    }
}