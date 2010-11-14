using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
namespace Common.Math
{
    public class Util
    {
        public static Point RotatePoint(Point toRotate, Point origin, float angleInDegrees)
        {
            toRotate.Offset(origin.X, origin.Y);
            float theta = angleInDegrees * Constants.RADIANS_PER_DEGREE;
            int x = (int)((double)((System.Math.Cos(theta) * toRotate.X) - (System.Math.Sin(theta) * toRotate.Y)));
            int y = (int)((double)((System.Math.Sin(theta) * toRotate.X) + (System.Math.Cos(theta) * toRotate.Y)));
            return new Point(x - origin.X, y - origin.Y);
        }

        public static float AngleDelta(float Ang1, float Ang2)
        {
            Ang1 = Ang1 % 360;
            Ang2 = Ang2 % 360;
            if (Ang1 == Ang2)
            {
                return 0.0f; //No angle to compute
            }
            else
            {
                float fDif = (Ang2 - Ang1);//There is an angle to compute
                if (fDif >= 180.0f)
                {
                    fDif = fDif - 180.0f; //correct the half
                    fDif = 180.0f - fDif; //invert the half
                    fDif = 0 - fDif; //reverse direction
                    return fDif;
                }
                else
                {
                    if (fDif <= -180.0f)
                    {
                        fDif = fDif + 180.0f; //correct the half
                        fDif = 180.0f + fDif;
                        return fDif;
                    }
                }
                return fDif;
            }
        }
    }
}
