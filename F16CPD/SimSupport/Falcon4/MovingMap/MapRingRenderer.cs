using System.Drawing;

namespace F16CPD.SimSupport.Falcon4.MovingMap
{
    internal interface IMapRingRenderer
    {
        void DrawMapRing(Graphics g, Rectangle renderRectangle, float outerMapRingDiameterPixelsUnscaled,
            float renderRectangleScaleFactor, float magneticHeadingInDecimalDegrees);
    }

    internal class MapRingRenderer : IMapRingRenderer
    {
        private readonly IOuterMapRangeCircleRenderer _outerMapRangeCircleRenderer;
        private readonly IInnerMapRangeCircleRenderer _innerMapRangeCircleRenderer;
        private readonly IInnerMapRangeCircleNorthMarkerRenderer _innerMapRangeCircleNorthMarkerRenderer;

        public MapRingRenderer(
            IOuterMapRangeCircleRenderer outerMapRangeCircleRenderer = null,
            IInnerMapRangeCircleRenderer innerMapRangeCircleRenderer = null,
            IInnerMapRangeCircleNorthMarkerRenderer innerMapRangeCircleNorthMarkerRenderer = null)
        {
            _outerMapRangeCircleRenderer = outerMapRangeCircleRenderer ?? new OuterMapRangeCircleRenderer();
            _innerMapRangeCircleRenderer = innerMapRangeCircleRenderer ?? new InnerMapRangeCircleRenderer();
            _innerMapRangeCircleNorthMarkerRenderer = innerMapRangeCircleNorthMarkerRenderer ?? new InnerMapRangeCircleNorthMarkerRenderer();
        }

        public void DrawMapRing(Graphics g, Rectangle renderRectangle, float outerMapRingDiameterPixelsUnscaled,
            float renderRectangleScaleFactor, float magneticHeadingInDecimalDegrees)
        {
            var mapRingPen = new Pen(Color.Magenta);
            var mapRingBrush = new SolidBrush(Color.Magenta);
            mapRingPen.Width = 1;
            const int mapRingLineWidths = 25;

            var originalGTransform = g.Transform;

            g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
            g.RotateTransform(-magneticHeadingInDecimalDegrees);
            g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

            int outerMapRingDiameterPixelsScaled;
            _outerMapRangeCircleRenderer.DrawOuterMapRangeCircle(g, renderRectangle, outerMapRingDiameterPixelsUnscaled,
                out outerMapRingDiameterPixelsScaled, renderRectangleScaleFactor, mapRingPen, mapRingLineWidths);

            Rectangle innerMapRingBoundingRect;
            int innerMapRingBoundingRectMiddleX;
            _innerMapRangeCircleRenderer.DrawInnerMapRangeCircle(g, renderRectangle, mapRingPen, mapRingLineWidths,
                outerMapRingDiameterPixelsScaled, out innerMapRingBoundingRect, out innerMapRingBoundingRectMiddleX);
            _innerMapRangeCircleNorthMarkerRenderer.DrawNorthMarkerOnInnerMapRangeCircle(g, mapRingBrush, innerMapRingBoundingRect,
                innerMapRingBoundingRectMiddleX);
            g.Transform = originalGTransform;
        }
    }
}