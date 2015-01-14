using System.Drawing;

namespace Common.UI.Layout
{
    public class Util
    {
        public static Point RotatePoint(Point toRotate, Point origin, float angleInDegrees)
        {
            toRotate.Offset(origin.X, origin.Y);
            var theta = angleInDegrees * Math.Constants.RADIANS_PER_DEGREE;
            var x = (int)(((System.Math.Cos(theta) * toRotate.X) - (System.Math.Sin(theta) * toRotate.Y)));
            var y = (int)(((System.Math.Sin(theta) * toRotate.X) + (System.Math.Cos(theta) * toRotate.Y)));
            return new Point(x - origin.X, y - origin.Y);
        }
        public static Rectangle MultiplyRectangle(Rectangle rect, int factor)
        {
            return new Rectangle(rect.X * factor, rect.Y * factor, rect.Width * factor,
                                 rect.Height * factor);
        }

    }
}
