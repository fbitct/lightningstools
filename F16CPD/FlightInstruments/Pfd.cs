using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Collections;
using F16CPD.Mfd.Controls;
namespace F16CPD.FlightInstruments
{
    //TODO: if WOW or on-ground or zero VVI, climb/dive marker should be zero too -- but BMS sharedmem doesn't do this sometimes in catastrophic situations...
    public sealed class Pfd : IDisposable
    {
        public const int AOA_YELLOW_RANGE_MIN_ANGLE_DEGREES = 8;
        public const int AOA_YELLOW_RANGE_MAX_ANGLE_DEGREES = 11;
        public const int AOA_GREEN_RANGE_MIN_ANGLE_DEGREES = 11;
        public const int AOA_GREEN_RANGE_MAX_ANGLE_DEGREES = 14;
        public const int AOA_RED_RANGE_MIN_ANGLE_DEGREES = 14;
        public const int AOA_RED_RANGE_MAX_ANGLE_DEGREES = 17;
        public const float ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES = 1.0f;
        public const float ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES = 5.0f;
        public const float MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND = 3.0f;

        private const float baroChangePerThousandFeet = 1.08f;
        private Bitmap _climbDiveMarkerSymbol = null;
        private Bitmap _cagedClimbDiveMarkerSymbol = null;
        private Bitmap _markerDiamond = null;
        private Bitmap _adiPitchBars = null;
        private Bitmap _airspeedTape = null;
        private Bitmap _aoaTape = null;
        private Bitmap _vviTape = null;
        private Bitmap[] _altitudeTapes = new Bitmap[200];
        private Bitmap _verticalNumberStrip = null;
        private bool _isDisposed = false;
        private Bitmap[] _singleDigitBitmaps = new Bitmap[1000];
        private Bitmap[] _tripleDigitBitmaps = new Bitmap[1000];
        private struct TapeEdgeColoringInstruction
        {
            public int minVal;
            public int maxVal;
            public Color color;
        }
        public F16CpdMfdManager Manager
        {
            get;
            set;
        }
        public void Render(Graphics g, Size renderSize)
        {
            Matrix origTransform = g.Transform;

            Rectangle topBox = new Rectangle(0, 0, renderSize.Width, 239);
            int centerYPfd = topBox.Bottom;
            int centerXPfd = 300;

            if (Manager.FlightData.PfdOffFlag)
            {
                DrawPfdOff(g, renderSize);
                return;
            }

            DrawAttitudeIndicator(g, renderSize, centerYPfd, centerXPfd);

            //draw airspeed tape Bitmap
            DrawAirspeedTape(g);

            //draw AOA tape
            DrawAoaTape(g, centerYPfd);
            //draw VVI
            DrawVVITape(g, centerYPfd);

            g.Transform = origTransform;

            //draw altitude tape
            DrawAltitudeTape(g);

            //draw PFD summary text
            DrawPfdSummaryText(g);
        }

        private static void DrawPfdOff(Graphics g, Size renderSize)
        {
            Color greenColor = Color.FromArgb(0, 255, 0);
            Brush greenBrush = new SolidBrush(greenColor);

            string toDisplay = "NO PFD DATA";
            GraphicsPath path = new GraphicsPath();
            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Rectangle layoutRectangle = new Rectangle(new Point(0, 0), renderSize);
            path.AddString(toDisplay, FontFamily.GenericMonospace, (int)System.Drawing.FontStyle.Bold, 20, layoutRectangle, sf);
            g.FillPath(greenBrush, path);
            return;
        }

        private void DrawAttitudeIndicator(Graphics g, Size renderSize, int centerYPfd, int centerXPfd)
        {
            //************************************
            //ADI 
            //************************************
            float pitchAngleDegrees = Manager.FlightData.PitchAngleInDecimalDegrees;
            float rollAngleDegrees = Manager.FlightData.RollAngleInDecimalDegrees;
            float aoaAngleDegrees = Manager.FlightData.AngleOfAttackInDegrees;
            float sideslipAngleDegrees = Manager.FlightData.BetaAngleInDecimalDegrees;
            float climbAngleDegrees = Manager.FlightData.GammaAngleInDecimalDegrees;
            float windOffsetDegrees = Manager.FlightData.WindOffsetToFlightPathMarkerInDecimalDegrees;
            if (_climbDiveMarkerSymbol == null)
            {
                _climbDiveMarkerSymbol = (Bitmap)Properties.Resources.climbDiveMarker;
                _climbDiveMarkerSymbol.MakeTransparent(Color.FromArgb(0, 255, 0, 128));
            }

            if (_cagedClimbDiveMarkerSymbol == null)
            {
                _cagedClimbDiveMarkerSymbol = (Bitmap)Properties.Resources.cagedClimbDiveMarker;
                _cagedClimbDiveMarkerSymbol.MakeTransparent(Color.FromArgb(0, 255, 0, 128));
            }
            int verticalDistanceBetweenPitchLines = 25;
            int degreesBetweenTicks = 5;
            float pixelsSeparationPerDegreeOfPitch = verticalDistanceBetweenPitchLines / degreesBetweenTicks;


            DrawAdiSkyAndGround(g, renderSize, centerYPfd, centerXPfd, pitchAngleDegrees, rollAngleDegrees, pixelsSeparationPerDegreeOfPitch);
            DrawAdiPitchLadder(g, renderSize, centerYPfd, centerXPfd, pitchAngleDegrees, rollAngleDegrees);
            DrawAdiRollIndexLines(g, centerXPfd, centerYPfd);

            Pen whitePen = new Pen(Color.White);
            Pen blackPen = new Pen(Color.Black);
            whitePen.Width = 2;
            blackPen.Width = 1;


            Point[] zeroDegreeTriangle = DrawAdiRollTriangles(g, centerXPfd, centerYPfd, this.Manager.FlightData.RollAngleInDecimalDegrees, this.Manager.FlightData.AdiOffFlag);
            DrawAdiGsFlag(g, new Point(zeroDegreeTriangle[0].X - 89, zeroDegreeTriangle[0].Y - 30));
            DrawAdiAuxFlag(g, new Point(zeroDegreeTriangle[0].X - 16, zeroDegreeTriangle[0].Y - 30));
            DrawAdiLocFlag(g, new Point(zeroDegreeTriangle[0].X + 56, zeroDegreeTriangle[0].Y - 30));
            DrawAdiOffFlag(g, new Point(zeroDegreeTriangle[0].X - 22, centerYPfd-12));
            DrawAdiFixedPitchReferenceBars(g, centerXPfd, centerYPfd);
            DrawRateOfTurnIndicator(g);

            whitePen.Width = 2;

            //prepare to draw glideslope and localizer stuff
            Point farLeftLocalizerMarkerCenterPoint = new Point(208, 400);
            Point leftMiddleLocalizerMarkerCenterPoint = new Point(farLeftLocalizerMarkerCenterPoint.X + 45, farLeftLocalizerMarkerCenterPoint.Y);
            Point middleLocalizerMarkerCenterPoint = new Point(farLeftLocalizerMarkerCenterPoint.X + 90, farLeftLocalizerMarkerCenterPoint.Y);
            Point rightMiddleLocalizerMarkerCenterPoint = new Point(farLeftLocalizerMarkerCenterPoint.X + 135, farLeftLocalizerMarkerCenterPoint.Y);
            Point farRightLocalizerMarkerCenterPoint = new Point(farLeftLocalizerMarkerCenterPoint.X + 180, farLeftLocalizerMarkerCenterPoint.Y);

            Point topGlideSlopeMarkerCenterPoint = new Point(440, 139);
            Point upperMiddleGlideSlopeMarkerCenterPoint = new Point(topGlideSlopeMarkerCenterPoint.X, topGlideSlopeMarkerCenterPoint.Y + 50);
            Point middleGlideSlopeMarkerCenterPoint = new Point(topGlideSlopeMarkerCenterPoint.X, topGlideSlopeMarkerCenterPoint.Y + 100);
            Point lowerMiddleGlideSlopeMarkerCenterPoint = new Point(topGlideSlopeMarkerCenterPoint.X, topGlideSlopeMarkerCenterPoint.Y + 150);
            Point bottomGlideSlopeMarkerCenterPoint = new Point(topGlideSlopeMarkerCenterPoint.X, topGlideSlopeMarkerCenterPoint.Y + 200);


            if (_markerDiamond == null)
            {
                Bitmap markerDiamond = Properties.Resources.adidiamond;
                markerDiamond.MakeTransparent(Color.FromArgb(255, 0, 255));
                _markerDiamond = Common.Imaging.Util.ResizeBitmap(markerDiamond, new Size(15, 15));
            }

            //prepare draw localizer indicator line

            float minIlsHorizontalPositionVal = -ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES;
            float maxIlsHorizontalPositionVal = ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES;
            float IlsHorizontalPositionRange = maxIlsHorizontalPositionVal - minIlsHorizontalPositionVal;
            float currentIlsHorizontalPositionVal = Manager.FlightData.AdiIlsLocalizerDeviationInDecimalDegrees + Math.Abs(minIlsHorizontalPositionVal);
            if (currentIlsHorizontalPositionVal < 0) currentIlsHorizontalPositionVal = 0;
            if (currentIlsHorizontalPositionVal > IlsHorizontalPositionRange) currentIlsHorizontalPositionVal = IlsHorizontalPositionRange;

            int minIlsBarX = farLeftLocalizerMarkerCenterPoint.X;
            int maxIlsBarX = farRightLocalizerMarkerCenterPoint.X;
            int ilsBarXRange = (int)(maxIlsBarX - minIlsBarX) + 1;

            int currentIlsBarX = (int)(minIlsBarX + ((currentIlsHorizontalPositionVal / IlsHorizontalPositionRange) * ilsBarXRange));

            Point ilsBarTop = new Point(currentIlsBarX, topGlideSlopeMarkerCenterPoint.Y);
            Point ilsBarBottom = new Point(currentIlsBarX, bottomGlideSlopeMarkerCenterPoint.Y);

            Pen localizerBarPen = new Pen(Color.Yellow);
            localizerBarPen.Width = 3;
            Pen glideslopeBarPen = new Pen(Color.Yellow);
            glideslopeBarPen.Width = 3;
            if (Manager.FlightData.AdiEnableCommandBars && !Manager.FlightData.AdiLocalizerInvalidFlag && !Manager.FlightData.AdiOffFlag)
            {
                //if (this.FlightData.AdiLocalizerInvalidFlag)
                //{
                //    localizerBarPen.DashStyle = DashStyle.Dash;
                //    localizerBarPen.DashOffset = 3;
                //}
                //draw localizer command bar
                g.DrawLine(localizerBarPen, ilsBarTop, ilsBarBottom);
                g.DrawImage(_markerDiamond, currentIlsBarX - (_markerDiamond.Width / 2), farLeftLocalizerMarkerCenterPoint.Y - (_markerDiamond.Width / 2));
            }

            //prepare to draw glideslope bar
            float minIlsVerticalPositionVal = -ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES;
            float maxIlsVerticalPositionVal = ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES;
            float IlsVerticalPositionRange = maxIlsVerticalPositionVal - minIlsVerticalPositionVal;

            float currentIlsVerticalPositionVal = (-Manager.FlightData.AdiIlsGlideslopeDeviationInDecimalDegrees) + Math.Abs(minIlsVerticalPositionVal);
            if (currentIlsVerticalPositionVal < 0) currentIlsVerticalPositionVal = 0;
            if (currentIlsVerticalPositionVal > IlsVerticalPositionRange) currentIlsVerticalPositionVal = IlsVerticalPositionRange;


            int minIlsBarY = topGlideSlopeMarkerCenterPoint.Y;
            int maxIlsBarY = bottomGlideSlopeMarkerCenterPoint.Y;
            int ilsBarYRange = (int)(maxIlsBarY - minIlsBarY) + 1;

            int currentIlsBarY = (int)(minIlsBarY + ((currentIlsVerticalPositionVal / IlsVerticalPositionRange) * ilsBarYRange));

            Point ilsBarLeft = new Point(farLeftLocalizerMarkerCenterPoint.X - 7, currentIlsBarY);
            Point ilsBarRight = new Point(farRightLocalizerMarkerCenterPoint.X + 7, currentIlsBarY);
            DrawMarkerBeacon(g, topGlideSlopeMarkerCenterPoint);

            //draw glideslope bar
            if (Manager.FlightData.AdiEnableCommandBars && !Manager.FlightData.AdiGlideslopeInvalidFlag && !Manager.FlightData.AdiOffFlag)
            {
                //if (this.FlightData.AdiGlideslopeInvalidFlag)
                //{
                //    glideslopeBarPen.DashStyle = DashStyle.Dash;
                //    glideslopeBarPen.DashOffset = 3;
                //}
                g.DrawLine(glideslopeBarPen, ilsBarLeft, ilsBarRight);
                g.DrawImage(_markerDiamond, topGlideSlopeMarkerCenterPoint.X - (_markerDiamond.Width / 2), currentIlsBarY - (_markerDiamond.Width / 2));
            }

            DrawGlideslopeMarkers(g, topGlideSlopeMarkerCenterPoint, upperMiddleGlideSlopeMarkerCenterPoint, middleGlideSlopeMarkerCenterPoint, lowerMiddleGlideSlopeMarkerCenterPoint, bottomGlideSlopeMarkerCenterPoint);
            DrawLocalizerMarkers(g, farLeftLocalizerMarkerCenterPoint, leftMiddleLocalizerMarkerCenterPoint, middleLocalizerMarkerCenterPoint, farRightLocalizerMarkerCenterPoint, rightMiddleLocalizerMarkerCenterPoint);
            DrawClimbDiveMarkerSymbol(g, centerYPfd, centerXPfd, aoaAngleDegrees, pixelsSeparationPerDegreeOfPitch);
        }

        private void DrawAdiFixedPitchReferenceBars(Graphics g, int centerXPfd, int centerYPfd)
        {
            Pen whitePen = new Pen(Color.White);
            whitePen.Width = 2;
            int baseY = centerYPfd -3;
            int baseX = centerXPfd -100;
            List<Point> pointsList = new List<Point>();
            if (!Manager.FlightData.AdiOffFlag)
            {
                //draw fixed pitch reference bars
                pointsList.Add(new Point(baseX, baseY));
                pointsList.Add(new Point(baseX, baseY + 6));
                pointsList.Add(new Point(baseX + 67, baseY + 6));
                pointsList.Add(new Point(baseX + 67, baseY + 23));
                pointsList.Add(new Point(baseX + 72, baseY + 23));
                pointsList.Add(new Point(baseX + 72, baseY));
                g.FillPolygon(Brushes.Black, pointsList.ToArray());
                g.DrawPolygon(whitePen, pointsList.ToArray());
                pointsList.Clear();
                pointsList.Add(new Point(baseX + 128, baseY));
                pointsList.Add(new Point(baseX + 128, baseY + 23));
                pointsList.Add(new Point(baseX + 133, baseY + 23));
                pointsList.Add(new Point(baseX + 133, baseY + 6));
                pointsList.Add(new Point(baseX + 197, baseY + 6));
                pointsList.Add(new Point(baseX + 197, baseY));
                g.FillPolygon(Brushes.Black, pointsList.ToArray());
                g.DrawPolygon(whitePen, pointsList.ToArray());

            }
        }

        private static Point[] DrawAdiRollTriangles(Graphics g, int centerXPfd, int centerYPfd, float rollDegrees, bool off)
        {
            int baseX = centerXPfd;
            int baseY = centerYPfd+3;
            Pen blackPen = new Pen(Color.Black);
            //draw roll triangles
            Point[] zeroDegreeTriangle = new Point[] { new Point(baseX -14, baseY -203), new Point(baseX, baseY -173), new Point(baseX + 14, baseY - 203) };
            if (rollDegrees >= -0.5f && rollDegrees <= 0.5f && !off)
            {
                g.FillPolygon(Brushes.White, zeroDegreeTriangle);
            }
            g.DrawPolygon(blackPen, zeroDegreeTriangle);
            Point[] leftTriangleOutside = new Point[] { new Point(baseX -130, baseY-154), new Point(baseX -150, baseY -134), new Point(baseX -119, baseY -123) };
            Point[] leftTriangleInside = new Point[] { new Point(baseX -131, baseY-147), new Point(baseX -143, baseY-135), new Point(baseX -125, baseY-128) };

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(leftTriangleInside);
            path.AddPolygon(leftTriangleOutside);
            Region region = new Region(path);
            g.FillPath(Brushes.White, path);
            if (rollDegrees >= 44 && rollDegrees <= 46 && !off)
            {
                g.FillPolygon(Brushes.White, leftTriangleInside);
            }
            g.DrawPath(blackPen, path);

            Point[] rightTriangleOutside = new Point[] { new Point(baseX + 130, baseY-154), new Point(baseX + 120, baseY -123), new Point(baseX + 151, baseY-134) };
            Point[] rightTriangleInside = new Point[] { new Point(baseX + 132,baseY -147), new Point(baseX + 125, baseY -128), new Point(baseX + 144, baseY -136) };


            path.Reset();
            path.AddPolygon(rightTriangleInside);
            path.AddPolygon(rightTriangleOutside);
            g.FillPath(Brushes.White, path);

            if (rollDegrees <= -44 && rollDegrees >= -46 && !off)
            {
                g.FillPolygon(Brushes.White, rightTriangleInside);
            }
            g.DrawPath(blackPen, path);
            
            return zeroDegreeTriangle;
        }

        private static void DrawAdiRollIndexLines(Graphics g, int centerXPfd, int centerYPfd)
        {
            Pen whitePen = new Pen(Color.White);
            whitePen.Width = 2;
            int baseX = centerXPfd;
            int baseY = centerYPfd+3;

            //draw roll index lines
            g.DrawLine(whitePen, baseX -32, baseY -188, baseX -29, baseY -171);
            g.DrawLine(whitePen, baseX -64, baseY -180, baseX -57, baseY -162);
            g.DrawLine(whitePen, baseX -99, baseY-176, baseX -84, baseY -150);
            g.DrawLine(whitePen, baseX -171, baseY -102, baseX -146, baseY -87);
            g.DrawLine(whitePen, baseX +33, baseY -188, baseX +30, baseY -171);
            g.DrawLine(whitePen, baseX +58, baseY -163, baseX +64, baseY -179);
            g.DrawLine(whitePen, baseX +85, baseY -150, baseX +99, baseY -176);
            g.DrawLine(whitePen, baseX +147, baseY -87, baseX +171, baseY -102);
        }

        private void DrawAdiPitchLadder(Graphics g, Size renderSize, int centerYPfd, int centerXPfd, float pitchAngleDegrees, float rollAngleDegrees)
        {
            //obtain pitch ladder Bitmap
            Bitmap adiPitchLadder = GetAdiPitchLadder(pitchAngleDegrees, rollAngleDegrees, renderSize.Width, renderSize.Height);
            int centerYAdiBars = (adiPitchLadder.Height / 2) + 1;
            int adiBarsXpos = centerXPfd - (adiPitchLadder.Width / 2);
            int adiBarsYpos = centerYPfd - centerYAdiBars;

            //draw pitch ladder
            if (!Manager.FlightData.AdiOffFlag)
            {
                g.DrawImage(adiPitchLadder, adiBarsXpos, adiBarsYpos);
            }
        }

        private void DrawAdiSkyAndGround(Graphics g, Size renderSize, int centerYPfd, int centerXPfd, float pitchAngleDegrees, float rollAngleDegrees, float pixelsSeparationPerDegreeOfPitch)
        {
            //draw sky/ground
            Color skyColor = Color.FromArgb(50, 145, 255);
            Color groundColor = Color.FromArgb(215, 97, 55);
            Brush skyBrush = new SolidBrush(skyColor);

            Brush groundBrush = new SolidBrush(groundColor);
            {
                Matrix curTransform = g.Transform;
                int centerX = centerXPfd;
                int centerY = centerYPfd;
                if (!Manager.FlightData.AdiOffFlag)
                {
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(-rollAngleDegrees);
                    g.TranslateTransform(-centerX, -centerY);
                    g.TranslateTransform(0, (pixelsSeparationPerDegreeOfPitch * pitchAngleDegrees));
                }
                g.FillRectangle(skyBrush, new Rectangle(-(int)(renderSize.Width / 2), (int)(-pixelsSeparationPerDegreeOfPitch * 180) + centerY, (int)(renderSize.Width * 2), (int)(pixelsSeparationPerDegreeOfPitch * 180)));
                g.FillRectangle(groundBrush, new Rectangle(-(int)(renderSize.Width / 2), centerY, (int)(renderSize.Width * 2), (int)(pixelsSeparationPerDegreeOfPitch * 180)));
                Pen horizonLinePen = new Pen(Color.Black);
                horizonLinePen.Width = 1;
                g.DrawLine(horizonLinePen, -1000, centerY, renderSize.Width + 1000, centerYPfd);
                g.Transform = curTransform;
            }
        }
        private void DrawAdiOffFlag(Graphics g, Point location)
        {
            //draw ADI OFF flag
            if (Manager.FlightData.AdiOffFlag)
            {
                Font adiOffFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                GraphicsPath path = new GraphicsPath();
                StringFormat adiOffFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                adiOffFlagStringFormat.Alignment = StringAlignment.Center;
                adiOffFlagStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle adiOffFlagTextLayoutRectangle = new Rectangle(location, new Size(75, 25));
                path.AddString("OFF", adiOffFlagFont.FontFamily, (int)adiOffFlagFont.Style, adiOffFlagFont.SizeInPoints, adiOffFlagTextLayoutRectangle, adiOffFlagStringFormat);
                Brush adiOffFlagBrush = Brushes.Red;
                Brush adiOffFlagTextBrush = Brushes.Black;
                if (Manager.NightMode)
                {
                    adiOffFlagBrush = Brushes.Black;
                    adiOffFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(adiOffFlagBrush, adiOffFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, adiOffFlagTextLayoutRectangle);
                g.FillPath(adiOffFlagTextBrush, path);
            }
        }
        private void DrawAoaOffFlag(Graphics g, Point location)
        {
            //draw AOA OFF flag
            if (Manager.FlightData.AoaOffFlag)
            {
                Font aoaOffFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                GraphicsPath path = new GraphicsPath();
                StringFormat aoaOffFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                aoaOffFlagStringFormat.Alignment = StringAlignment.Center;
                aoaOffFlagStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle aoaOffFlagTextLayoutRectangle = new Rectangle(location, new Size(75, 25));
                path.AddString("OFF", aoaOffFlagFont.FontFamily, (int)aoaOffFlagFont.Style, aoaOffFlagFont.SizeInPoints, aoaOffFlagTextLayoutRectangle, aoaOffFlagStringFormat);
                Brush aoaOffFlagBrush = Brushes.Red;
                Brush aoaOffFlagTextBrush = Brushes.Black;
                if (Manager.NightMode)
                {
                    aoaOffFlagBrush = Brushes.Black;
                    aoaOffFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(aoaOffFlagBrush, aoaOffFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, aoaOffFlagTextLayoutRectangle);
                g.FillPath(aoaOffFlagTextBrush, path);
            }
        }

        private void DrawVVIOffFlag(Graphics g, Point location)
        {
            //draw VVI OFF flag
            if (Manager.FlightData.VviOffFlag)
            {
                Font vviOffFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                GraphicsPath path = new GraphicsPath();
                StringFormat vviOffFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                vviOffFlagStringFormat.Alignment = StringAlignment.Center;
                vviOffFlagStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle vviOffFlagTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("OFF", vviOffFlagFont.FontFamily, (int)vviOffFlagFont.Style, vviOffFlagFont.SizeInPoints, vviOffFlagTextLayoutRectangle, vviOffFlagStringFormat);
                Brush vviOffFlagBrush = Brushes.Red;
                Brush vviOffFlagTextBrush = Brushes.Black;
                if (Manager.NightMode)
                {
                    vviOffFlagBrush = Brushes.Black;
                    vviOffFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(vviOffFlagBrush, vviOffFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, vviOffFlagTextLayoutRectangle);
                g.FillPath(vviOffFlagTextBrush, path);
            }
        }
        private void DrawAdiGsFlag(Graphics g, Point location)
        {
            //draw ADI GS flag
            if (Manager.FlightData.AdiGlideslopeInvalidFlag)
            {
                Font adiGsFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                GraphicsPath path = new GraphicsPath();
                StringFormat adiGsFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                adiGsFlagStringFormat.Alignment = StringAlignment.Center;
                adiGsFlagStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle adiGsFlagTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("GS", adiGsFlagFont.FontFamily, (int)adiGsFlagFont.Style, adiGsFlagFont.SizeInPoints, adiGsFlagTextLayoutRectangle, adiGsFlagStringFormat);
                Brush gsFlagBrush = Brushes.Red;
                Brush gsFlagTextBrush = Brushes.Black;
                if (Manager.NightMode)
                {
                    gsFlagBrush = Brushes.Black;
                    gsFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(gsFlagBrush, adiGsFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, adiGsFlagTextLayoutRectangle);
                g.FillPath(gsFlagTextBrush, path);
            }
        }

        private void DrawAdiAuxFlag(Graphics g, Point location)
        {
            //draw ADI aux flag
            if (Manager.FlightData.AdiAuxFlag)
            {
                Font adiAuxFont = new Font("Lucida Console", 25, FontStyle.Bold);
                GraphicsPath path = new GraphicsPath();
                StringFormat adiAuxStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                adiAuxStringFormat.Alignment = StringAlignment.Center;
                adiAuxStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle adiAuxTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("AUX", adiAuxFont.FontFamily, (int)adiAuxFont.Style, adiAuxFont.SizeInPoints, adiAuxTextLayoutRectangle, adiAuxStringFormat);
                g.FillRectangle(Brushes.Yellow, adiAuxTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, adiAuxTextLayoutRectangle);
                g.FillPath(Brushes.Black, path);
            }
        }

        private void DrawAdiLocFlag(Graphics g, Point location)
        {
            //draw ADI LOC flag
            if (Manager.FlightData.AdiLocalizerInvalidFlag)
            {
                Font adiLocFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);

                GraphicsPath path = new GraphicsPath();
                StringFormat adiLocFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                adiLocFlagStringFormat.Alignment = StringAlignment.Center;
                adiLocFlagStringFormat.LineAlignment = StringAlignment.Near;
                Rectangle adiLocFlagTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("LOC", adiLocFlagFont.FontFamily, (int)adiLocFlagFont.Style, adiLocFlagFont.SizeInPoints, adiLocFlagTextLayoutRectangle, adiLocFlagStringFormat);
                Brush locFlagBrush = Brushes.Red;
                Brush locFlagTextBrush = Brushes.Black;
                if (Manager.NightMode)
                {
                    locFlagBrush = Brushes.Black;
                    locFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(locFlagBrush, adiLocFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, adiLocFlagTextLayoutRectangle);
                g.FillPath(locFlagTextBrush, path);
            }
        }

        private void DrawGlideslopeMarkers(Graphics g, Point topGlideSlopeMarkerCenterPoint, Point upperMiddleGlideSlopeMarkerCenterPoint, Point middleGlideSlopeMarkerCenterPoint, Point lowerMiddleGlideSlopeMarkerCenterPoint, Point bottomGlideSlopeMarkerCenterPoint)
        {

            //draw glideslope markers
            if (Manager.FlightData.AdiEnableCommandBars && !Manager.FlightData.AdiGlideslopeInvalidFlag && !Manager.FlightData.AdiOffFlag)
            {
                Pen blackPen = new Pen(Color.Black);
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(new Rectangle(topGlideSlopeMarkerCenterPoint.X - 3, topGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(topGlideSlopeMarkerCenterPoint.X - 5, topGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new Rectangle(upperMiddleGlideSlopeMarkerCenterPoint.X - 3, upperMiddleGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(upperMiddleGlideSlopeMarkerCenterPoint.X - 5, upperMiddleGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddRectangle(
                    new Rectangle(
                        new Point(
                            middleGlideSlopeMarkerCenterPoint.X - 6,
                            middleGlideSlopeMarkerCenterPoint.Y - 1), new Size(12, 2)));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);


                path.Reset();
                path.AddEllipse(new Rectangle(lowerMiddleGlideSlopeMarkerCenterPoint.X - 3, lowerMiddleGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(lowerMiddleGlideSlopeMarkerCenterPoint.X - 5, lowerMiddleGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new Rectangle(bottomGlideSlopeMarkerCenterPoint.X - 3, bottomGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(bottomGlideSlopeMarkerCenterPoint.X - 5, bottomGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);
            }
        }

        private void DrawLocalizerMarkers(Graphics g, Point farLeftLocalizerMarkerCenterPoint, Point leftMiddleLocalizerMarkerCenterPoint, Point middleLocalizerMarkerCenterPoint, Point farRightLocalizerMarkerCenterPoint, Point rightMiddleLocalizerMarkerCenterPoint)
        {
            //draw localizer markers
            if (Manager.FlightData.AdiEnableCommandBars && !Manager.FlightData.AdiLocalizerInvalidFlag && !Manager.FlightData.AdiOffFlag)
            {
                Pen blackPen = new Pen(Color.Black);

                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(new Rectangle(farLeftLocalizerMarkerCenterPoint.X - 3, farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(farLeftLocalizerMarkerCenterPoint.X - 5, farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new Rectangle(leftMiddleLocalizerMarkerCenterPoint.X - 3, farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(leftMiddleLocalizerMarkerCenterPoint.X - 5, farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddRectangle(
                    new Rectangle(
                        new Point(middleLocalizerMarkerCenterPoint.X - 1, middleLocalizerMarkerCenterPoint.Y - 6), new Size(2, 12)));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new Rectangle(rightMiddleLocalizerMarkerCenterPoint.X - 3, farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(rightMiddleLocalizerMarkerCenterPoint.X - 5, farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new Rectangle(farRightLocalizerMarkerCenterPoint.X - 3, farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new Rectangle(farRightLocalizerMarkerCenterPoint.X - 5, farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);
            }
        }

        private void DrawClimbDiveMarkerSymbol(Graphics g, int centerYPfd, int centerXPfd, float aoaAngleDegrees, float pixelsSeparationPerDegreeOfPitch)
        {
            //draw climb/dive marker symbol
            int climbDiveMarkerCenterX = (int)(centerXPfd - (_climbDiveMarkerSymbol.Width / 2))-2;
            int climbDiveMarkerCenterY = (int)(centerYPfd - (_climbDiveMarkerSymbol.Height / 2) + (aoaAngleDegrees * pixelsSeparationPerDegreeOfPitch) - 7) + 1;
            int minCdmCenterX = 130;
            int minCdmCenterY = 30;
            int maxCdmX = 470;
            int maxCdmY = 420;

            bool climbDiveMarkerOutOfBounds = false;
            if (climbDiveMarkerCenterX < minCdmCenterX)
            {
                climbDiveMarkerCenterX = minCdmCenterX;
                climbDiveMarkerOutOfBounds = true;
            }
            else if (climbDiveMarkerCenterX > maxCdmX)
            {
                climbDiveMarkerCenterX = maxCdmX;
                climbDiveMarkerOutOfBounds = true;
            }
            else if (climbDiveMarkerCenterY < minCdmCenterY)
            {
                climbDiveMarkerCenterY = minCdmCenterY;
                climbDiveMarkerOutOfBounds = true;

            }
            else if (climbDiveMarkerCenterY > maxCdmY)
            {
                climbDiveMarkerCenterY = maxCdmY;
                climbDiveMarkerOutOfBounds = true;
            }
            if (!Manager.FlightData.AdiOffFlag)
            {
                if (climbDiveMarkerOutOfBounds)
                {
                    g.DrawImage(_cagedClimbDiveMarkerSymbol, climbDiveMarkerCenterX, climbDiveMarkerCenterY);
                }
                else
                {
                    g.DrawImage(_climbDiveMarkerSymbol, climbDiveMarkerCenterX, climbDiveMarkerCenterY);
                }
            }
        }

        private void DrawRateOfTurnIndicator(Graphics g)
        {
            Pen whitePen = new Pen(Color.White);
            //draw rate of turn indicator
            int dashWidth = 31;
            int dashHeight = 5;
            int leftDashX = 222;
            int leftDashY = 450;
            Pen dashWhitePen = new Pen(Color.White);
            Pen dashRedPen = new Pen(Color.FromArgb(89, 1, 0));
            Pen pen = null;
            for (int i = 1; i <= 5; i++)
            {
                if (i % 2 == 1)
                {
                    pen = dashWhitePen;
                }
                else
                {
                    pen = dashRedPen;
                }
                pen.Width = dashHeight;
                g.DrawLine(pen, new Point(leftDashX + (dashWidth * (i - 1)), leftDashY), new Point(leftDashX + (dashWidth * (i)), leftDashY));
            }
            int rateOfTurnXRange = (int)((dashWidth * 4.0f));
            int rateOfTurnCenterXPos = leftDashX + (rateOfTurnXRange / 2) + (dashWidth / 2);
            float instantaneousRateOfTurn = Manager.FlightData.RateOfTurnInDecimalDegreesPerSecond;
            float indicatedRateOfTurn = LimitRateOfTurn(instantaneousRateOfTurn);
            rateOfTurnCenterXPos = (int)((indicatedRateOfTurn / Pfd.MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND) * (rateOfTurnXRange / 2)) + rateOfTurnCenterXPos;

            whitePen.Width = 5;
            g.DrawLine(whitePen, rateOfTurnCenterXPos - (dashWidth / 2), leftDashY + 7, rateOfTurnCenterXPos + (dashWidth / 2), leftDashY + 7);
            g.DrawLine(whitePen, rateOfTurnCenterXPos, leftDashY + 7, rateOfTurnCenterXPos, leftDashY + 14);
        }

        private void DrawPfdSummaryText(Graphics g)
        {
            /**************************
             * TEXT AT BOTTOM
             **************************/
            Pen whitePen = new Pen(Color.White);
            Pen blackPen = new Pen(Color.Black);

            Font groundSpeedFont = new Font("Lucida Console", 17, FontStyle.Bold);
            Font machFont = new Font("Lucida Console", 17, FontStyle.Bold);
            Font alowFont = new Font("Lucida Console", 17, FontStyle.Bold);
            Font aglFont = new Font("Lucida Console", 17, FontStyle.Bold);


            string groundSpeed = string.Format("{0:GS 000}", Manager.FlightData.GroundSpeedInDecimalFeetPerSecond / Constants.FPS_PER_KNOT);
            string mach = string.Format("{0:M 0.00}", Manager.FlightData.MachNumber);
            string alow = string.Format("{0:00000}", Manager.FlightData.AutomaticLowAltitudeWarningInFeet);
            float agl = Manager.FlightData.AltitudeAboveGroundLevelInDecimalFeet;
            agl = (int)(10.0f * Math.Floor(agl / 10.0f));
            string aglString = string.Format("{0:00000}", agl);
            aglString = aglString.Substring(0, 4) + "0";

            if (Manager.FlightData.RadarAltimeterOffFlag)
            {
                aglString = "";
            }

            Rectangle groundSpeedRectangle = new Rectangle(new Point(7, 430), new Size(85, 18));
            Rectangle machRectangle = new Rectangle(new Point(7, 455), new Size(85, 18));
            Rectangle alowTextRectangle = new Rectangle(new Point(463, 429), new Size(59, 16));
            Rectangle aglTextRectangle = new Rectangle(new Point(475, 456), new Size(43, 16));
            Rectangle alowRectangle = new Rectangle(new Point(537, 430), new Size(63, 16));
            Rectangle aglRectangle = new Rectangle(new Point(537, 457), new Size(63, 16));
            Rectangle aglOutlineRectangle = new Rectangle(new Point(532, 453), new Size(74, 24));

            StringFormat groundSpeedFormat = new StringFormat(StringFormatFlags.NoWrap);
            groundSpeedFormat.Alignment = StringAlignment.Near;
            groundSpeedFormat.LineAlignment = StringAlignment.Center;
            groundSpeedFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            StringFormat machFormat = new StringFormat(StringFormatFlags.NoWrap);
            machFormat.Alignment = StringAlignment.Near;
            machFormat.LineAlignment = StringAlignment.Center;
            machFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            StringFormat alowFormat = new StringFormat(StringFormatFlags.NoWrap);
            alowFormat.Alignment = StringAlignment.Far;
            alowFormat.LineAlignment = StringAlignment.Near;
            alowFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            StringFormat aglFormat = new StringFormat(StringFormatFlags.NoWrap);
            aglFormat.Alignment = StringAlignment.Far;
            aglFormat.LineAlignment = StringAlignment.Near;
            aglFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            GraphicsPath path = new GraphicsPath();

            path.AddString("AGL", aglFont.FontFamily, (int)aglFont.Style, aglFont.SizeInPoints, aglTextRectangle, aglFormat);
            path.AddString(aglString, aglFont.FontFamily, (int)aglFont.Style, aglFont.SizeInPoints, aglRectangle, aglFormat);
            
            path.AddString("ALOW", alowFont.FontFamily, (int)alowFont.Style, alowFont.SizeInPoints, alowTextRectangle, alowFormat);
            path.AddString(alow, alowFont.FontFamily, (int)alowFont.Style, alowFont.SizeInPoints, alowRectangle, alowFormat);
            g.DrawRectangle(whitePen, aglOutlineRectangle);

            path.AddString(mach, machFont.FontFamily, (int)machFont.Style, machFont.SizeInPoints, machRectangle, machFormat);
            path.AddString(groundSpeed, groundSpeedFont.FontFamily, (int)groundSpeedFont.Style, groundSpeedFont.SizeInPoints, groundSpeedRectangle, groundSpeedFormat);
            blackPen.Width = 1;
            whitePen.Width = 1;
            g.DrawPath(blackPen, path);
            g.FillPath(Brushes.White, path);
        }

        private void DrawVVITape(Graphics g, int centerYPfd)
        {
            //*************************
            //VVI TAPE
            //*************************
            Font vviTotalFont = new Font("Lucida Console", 14, FontStyle.Bold);

            Pen whitePen = new Pen(Color.White);
            Pen blackPen = new Pen(Color.Black);

            Rectangle vviBoundingBox = new Rectangle(new Point(473, 111), new Size(27, 256));
            Rectangle vviUpperBoundingBox = new Rectangle(new Point(vviBoundingBox.Left, vviBoundingBox.Top), new Size(vviBoundingBox.Width, centerYPfd- vviBoundingBox.Top));
            Rectangle vviLowerBoundingBox = new Rectangle(new Point(vviBoundingBox.Left, vviUpperBoundingBox.Bottom + 1), new Size(vviBoundingBox.Width, vviBoundingBox.Bottom - centerYPfd));

            //draw vertical velocity tape Bitmap
            float verticalVelocityFpm = (float)(Manager.FlightData.VerticalVelocityInDecimalFeetPerSecond * 60);
            if (Manager.FlightData.VviOffFlag)
            {
                verticalVelocityFpm = 0.00f;
            }

            float verticalVelocityKFpm = (float)(verticalVelocityFpm / 1000.0f);
            float verticalVelocityKFpmNormalized = verticalVelocityKFpm;
            if (verticalVelocityKFpmNormalized < -6.5f) verticalVelocityKFpmNormalized = -6.5f;
            if (verticalVelocityKFpmNormalized > 6.5f) verticalVelocityKFpmNormalized = 6.5f;
            GraphicsPath path = new GraphicsPath();
            List<Point> pointList = new List<Point>();

            Bitmap verticalVelocityTapeBitmap = GetVerticalVelocityTapeBitmap(verticalVelocityKFpmNormalized, vviBoundingBox.Width, vviBoundingBox.Height);

            g.DrawImage(
                verticalVelocityTapeBitmap,
                vviBoundingBox.Left,
                vviUpperBoundingBox.Top,
                new Rectangle(
                    new Point(0, 0),
                    new Size(verticalVelocityTapeBitmap.Width, vviBoundingBox.Height)
                    ), GraphicsUnit.Pixel
            );


            //draw bounding box 
            pointList.Clear();
            pointList.Add(new Point(vviBoundingBox.Left, vviBoundingBox.Top));
            pointList.Add(new Point(vviBoundingBox.Left, vviBoundingBox.Bottom));
            pointList.Add(new Point(vviBoundingBox.Right, vviBoundingBox.Bottom));
            pointList.Add(new Point(vviBoundingBox.Right, vviBoundingBox.Top + (vviBoundingBox.Height / 2) + 20));
            pointList.Add(new Point(vviBoundingBox.Left + 1, vviBoundingBox.Top + (vviBoundingBox.Height / 2)));
            pointList.Add(new Point(vviBoundingBox.Right, vviBoundingBox.Top + (vviBoundingBox.Height / 2) - 20));
            pointList.Add(new Point(vviBoundingBox.Right, vviBoundingBox.Top));
            path.Reset();
            path.AddPolygon(pointList.ToArray());
            whitePen.Width = 2;
            g.DrawPath(whitePen, path);
            whitePen.Width = 1;

            if (!Manager.FlightData.VviOffFlag)
            {
                //draw VVI quantity box
                pointList.Clear();
                pointList.Add(new Point(vviUpperBoundingBox.Left, vviUpperBoundingBox.Bottom - 2));
                pointList.Add(new Point(vviUpperBoundingBox.Left + (vviUpperBoundingBox.Width / 2), vviUpperBoundingBox.Bottom + 7));
                pointList.Add(new Point(vviUpperBoundingBox.Right + 26, vviUpperBoundingBox.Bottom + 7));
                pointList.Add(new Point(vviUpperBoundingBox.Right + 26, vviUpperBoundingBox.Bottom - 10));
                pointList.Add(new Point(vviUpperBoundingBox.Left + (vviUpperBoundingBox.Width / 2), vviUpperBoundingBox.Bottom - 10));
                path.Reset();
                path.AddPolygon(pointList.ToArray());
                g.TranslateTransform(3, 0 - (verticalVelocityKFpmNormalized * 20) + 2);
                g.FillPath(Brushes.Gray, path);
                blackPen.Width = 1;
                g.DrawPath(blackPen, path);

                //draw VVI quantity text
                string vviQuantity = null;
                if (Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands)
                {
                    vviQuantity = string.Format("{0:0.0}", verticalVelocityFpm / 1000.0f);
                }
                else
                {
                    vviQuantity = string.Format("{0:0}", verticalVelocityFpm);
                }
                StringFormat vviQuantityFormat = new StringFormat(StringFormatFlags.NoWrap);
                vviQuantityFormat.Alignment = StringAlignment.Far; //right justify
                vviQuantityFormat.LineAlignment = StringAlignment.Near;
                float fontSize = vviTotalFont.Size;
                if (vviQuantity.Length > 4)
                {
                    vviTotalFont = new Font(vviTotalFont.FontFamily, fontSize - 1, vviTotalFont.Style);
                }
                vviTotalFont = new Font(vviTotalFont.FontFamily, fontSize, vviTotalFont.Style);
                path.Reset();
                path.AddString(vviQuantity, vviTotalFont.FontFamily, (int)vviTotalFont.Style, vviTotalFont.SizeInPoints, new Point(vviUpperBoundingBox.Right + 26, vviUpperBoundingBox.Bottom - 8), vviQuantityFormat);
                TextRenderingHint hint = g.TextRenderingHint;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);
                g.TextRenderingHint = hint;

            }

            if (Manager.FlightData.VviOffFlag)
            {
                DrawVVIOffFlag(g, new Point(vviBoundingBox.Left-15, vviBoundingBox.Top + (vviBoundingBox.Height / 2) - 12));
            }

        }

        private void DrawAoaTape(Graphics g, int centerYPfd)
        {
            //*************************
            //AOA TAPE
            //*************************

            Pen blackPen = new Pen(Color.Black);
            Pen whitePen = new Pen(Color.White);
            //draw bounding box for AOA tape
            Rectangle aoaStripBoundingBox = new Rectangle(new Point(87, 168), new Size(42, 144));

            //draw aoa tape Bitmap
            float aoa = Manager.FlightData.AngleOfAttackInDegrees;

            if (Manager.FlightData.AoaOffFlag) aoa = 0.00f;

            Bitmap aoaBitmap = GetAoATapeBitmap(aoa, aoaStripBoundingBox.Width, aoaStripBoundingBox.Height);

            g.DrawImage(
                aoaBitmap,
                aoaStripBoundingBox.Left,
                aoaStripBoundingBox.Top,
                new Rectangle(
                    new Point(0, 0),
                    new Size(aoaBitmap.Width, aoaStripBoundingBox.Height)
                    ), GraphicsUnit.Pixel
            );


            //trace the outline of the AOA tape with black
            g.DrawRectangle(blackPen, aoaStripBoundingBox);

            //draw white line at top of AOA tape
            whitePen.Width = 2;
            g.DrawLine(whitePen, aoaStripBoundingBox.Left, aoaStripBoundingBox.Top, aoaStripBoundingBox.Right, aoaStripBoundingBox.Top);

            //draw white line at bottom of AOA tape
            whitePen.Width = 2;
            g.DrawLine(whitePen, aoaStripBoundingBox.Left, aoaStripBoundingBox.Bottom, aoaStripBoundingBox.Right, aoaStripBoundingBox.Bottom);

            //draw left AOA indicator triangle
            Point[] aoaTriangleLeftPoints = new Point[3];
            aoaTriangleLeftPoints[0] = new Point(aoaStripBoundingBox.Left, centerYPfd); //right-most point on left triangle
            aoaTriangleLeftPoints[1] = new Point(aoaStripBoundingBox.Left - 16, centerYPfd- 10); //upper point on left triangle
            aoaTriangleLeftPoints[2] = new Point(aoaStripBoundingBox.Left - 16, centerYPfd+ 10); //lower point on left triangle
            g.FillPolygon(Brushes.Black, aoaTriangleLeftPoints);
            whitePen.Width = 1;
            g.DrawPolygon(whitePen, aoaTriangleLeftPoints);

            //draw right AOA indicator triangle
            Point[] aoaTriangleRightPoints = new Point[3];
            aoaTriangleRightPoints[0] = new Point(aoaStripBoundingBox.Right, centerYPfd); //left-most point on right triangle
            aoaTriangleRightPoints[1] = new Point(aoaStripBoundingBox.Right + 16, centerYPfd- 10); //upper point on left triangle
            aoaTriangleRightPoints[2] = new Point(aoaStripBoundingBox.Right + 16, centerYPfd + 10); //lower point on left triangle
            g.FillPolygon(Brushes.Black, aoaTriangleRightPoints);
            whitePen.Width = 1;
            g.DrawPolygon(whitePen, aoaTriangleRightPoints);

            //draw line between indicator triangle points
            g.DrawLine(whitePen, aoaTriangleLeftPoints[0], aoaTriangleRightPoints[0]);

            Point location = new Point(aoaStripBoundingBox.Left -17, aoaStripBoundingBox.Top + (aoaStripBoundingBox.Height/2)-12);
            if (Manager.FlightData.AoaOffFlag) DrawAoaOffFlag(g, location);
        }

        private void DrawMarkerBeacon(Graphics g, Point topGlideSlopeMarkerCenterPoint)
        {
            //Draw MARKER BEACON light
            Color greenColor = Color.FromArgb(0, 255, 0);
            Brush greenBrush = new SolidBrush(greenColor);
            Color blueColor = Color.FromArgb(0, 128, 255);
            Brush blueBrush = new SolidBrush(blueColor);
            Color amberColor = Color.FromArgb(255, 191, 0);
            Brush amberBrush = new SolidBrush(amberColor);
            Font markerBeaconFont = new Font("Lucida Console", 10, FontStyle.Bold);

            Rectangle markerBeaconIndicatorRectangle = new Rectangle(topGlideSlopeMarkerCenterPoint.X, 430, 30, 30);
            Brush markerBeaconBackgroundBrush = null;
            if (Manager.FlightData.MarkerBeaconOuterMarkerFlag)
            {
                if (!Manager.NightMode)
                {
                    //markerBeaconBackgroundBrush = blueBrush;
                    markerBeaconBackgroundBrush = greenBrush;
                }
                else
                {
                    markerBeaconBackgroundBrush = greenBrush;
                }
            }
            else if (Manager.FlightData.MarkerBeaconMiddleMarkerFlag)
            {
                if (!Manager.NightMode)
                {
                    //markerBeaconBackgroundBrush = amberBrush;
                    markerBeaconBackgroundBrush = greenBrush;
                }
                else
                {
                    markerBeaconBackgroundBrush = greenBrush;
                }
            }
            else
            {
                markerBeaconBackgroundBrush = Brushes.Black;
            }
            g.FillEllipse(markerBeaconBackgroundBrush, markerBeaconIndicatorRectangle);
            string markerBeaconString = "MRK\nBCN";
            GraphicsPath path = new GraphicsPath();
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            path.AddString(markerBeaconString, markerBeaconFont.FontFamily, (int)markerBeaconFont.Style, markerBeaconFont.SizeInPoints, markerBeaconIndicatorRectangle, sf);
            Brush markerBeaconTextBrush = null;
            if (Manager.FlightData.MarkerBeaconOuterMarkerFlag || Manager.FlightData.MarkerBeaconMiddleMarkerFlag)
            {
                markerBeaconTextBrush = Brushes.Black;
            }
            else
            {
                markerBeaconTextBrush = Brushes.White;
            }
            g.FillPath(markerBeaconTextBrush, path);
        }

        private void DrawAltitudeTape(Graphics g)
        {
            //*******************************
            //* ALTITUDE TAPE
            //*******************************

            Rectangle altitudeStripBoundingBox = new Rectangle(new Point(535, 99), new Size(70, 278));
            Rectangle altitudeIndexBox = new Rectangle(new Point(altitudeStripBoundingBox.Left, altitudeStripBoundingBox.Top - 19), new Size(altitudeStripBoundingBox.Width, 19));
            Rectangle altitudeIndexTextBox = new Rectangle(new Point(altitudeStripBoundingBox.Left, altitudeStripBoundingBox.Top - 20), new Size(altitudeStripBoundingBox.Width, 25));
            Rectangle barometricPressureBox = new Rectangle(new Point(altitudeStripBoundingBox.Left, altitudeStripBoundingBox.Bottom + 1), new Size(altitudeStripBoundingBox.Width, 19));

            Color greenColor = Color.FromArgb(0, 255, 0);
            Color indexTextColor = Color.FromArgb(255, 128, 255);

            Brush purpleBrush = new SolidBrush(indexTextColor);
            Brush greenBrush = new SolidBrush(greenColor);

            Pen whitePen = new Pen(Color.White);
            Pen blackPen = new Pen(Color.Black);

            Font altitudeIndexFont = new Font("Lucida Console", 13, FontStyle.Bold);
            Font barometricPressureFont = new Font("Lucida Console", 10, FontStyle.Bold);
            Font altitudeFeetTextFont = new Font("Lucida Console", 14, FontStyle.Bold);

            float trueAltitudeMsl = (float)(Manager.FlightData.TrueAltitudeAboveMeanSeaLevelInDecimalFeet);
            float indicatedAltitude = (float)(Manager.FlightData.IndicatedAltitudeAboveMeanSeaLevelInDecimalFeet);
            if (trueAltitudeMsl == indicatedAltitude) //perform our own simulated indicated altitude adjustment for F4 versions that do not support the barometer setting
            {
                float baroDifference = Manager.FlightData.BarometricPressureInDecimalInchesOfMercury - 29.92f;
                float indicatedAltitudeCorrection = (baroDifference / baroChangePerThousandFeet) * 1000.0f;
                indicatedAltitude = trueAltitudeMsl + indicatedAltitudeCorrection; //correct indicated altitude for current baro pressure setting
            }
            
            Bitmap altitudeTapeBitmap = GetAltitudeTapeBitmap(indicatedAltitude, altitudeStripBoundingBox.Width - 1, altitudeStripBoundingBox.Height);

            g.DrawImage(
                altitudeTapeBitmap,
                altitudeStripBoundingBox.Left + 1,
                altitudeStripBoundingBox.Top + 1,
                new Rectangle(
                    new Point(0, 0),
                    new Size(altitudeTapeBitmap.Width, altitudeStripBoundingBox.Height)
                    ), GraphicsUnit.Pixel
            );
            //trace the outline of the altitude tape with black
            g.DrawRectangle(blackPen, altitudeStripBoundingBox);

            //draw altitude index box
            StringFormat altitudeIndexFormat = new StringFormat(StringFormatFlags.NoWrap);
            altitudeIndexFormat.Alignment = StringAlignment.Center;
            altitudeIndexFormat.LineAlignment = StringAlignment.Center;
            g.FillRectangle(Brushes.Black, altitudeIndexBox);

            //add altitude index text to altitude index box
            string altitudeIndex = string.Format("{0:00000}", Manager.AltitudeIndexInFeet);
            if (altitudeIndex[0] == '-') altitudeIndex = altitudeIndex.Substring(1, altitudeIndex.Length - 1);
            g.DrawString(altitudeIndex, altitudeIndexFont, purpleBrush, altitudeIndexTextBox, altitudeIndexFormat);

            //draw white line under altitude index box
            whitePen.Width = 2;
            g.DrawLine(whitePen, altitudeIndexBox.Left, altitudeIndexBox.Bottom, altitudeIndexBox.Right, altitudeIndexBox.Bottom);

            //draw white line at bottom of altitude strip
            whitePen.Width = 2;
            g.DrawLine(whitePen, altitudeStripBoundingBox.Left, altitudeStripBoundingBox.Bottom, altitudeStripBoundingBox.Right, altitudeStripBoundingBox.Bottom);

            //draw barometric pressure box
            StringFormat baroPressureFormat = new StringFormat(StringFormatFlags.NoWrap);
            baroPressureFormat.Alignment = StringAlignment.Center;
            baroPressureFormat.LineAlignment = StringAlignment.Center;
            g.FillRectangle(Brushes.Black, barometricPressureBox);

            float barometricPressure = Manager.FlightData.BarometricPressureInDecimalInchesOfMercury;
            //add barometric pressure text to barometric pressure box
            g.DrawString(string.Format("{0:00.00IN}", barometricPressure), barometricPressureFont, greenBrush, barometricPressureBox, baroPressureFormat);

            //draw altitude counter
            Point pointA = new Point(546, 221);
            Point pointB = new Point(546, 233);
            Point pointC = new Point(539, 239);
            Point pointD = new Point(546, 244);
            Point pointE = new Point(546, 255);
            Point pointF = new Point(581, 255);
            Point pointG = new Point(581, 265);
            Point pointH = new Point(608, 265);
            Point pointI = new Point(608, 210);
            Point pointJ = new Point(581, 210);
            Point pointK = new Point(581, 221);
            whitePen.Width = 1;
            Point[] points = new Point[] { pointA, pointB, pointC, pointD, pointE, pointF, pointG, pointH, pointI, pointJ, pointK };
            g.FillPolygon(Brushes.Black, points);
            g.DrawPolygon(whitePen, points);


            //draw altitude counter digits
            string altitudeString = string.Format("{0:00000}", indicatedAltitude);
            if (altitudeString[0] == '-') altitudeString = altitudeString.Substring(1, altitudeString.Length - 1);

            Bitmap tenThousandsDigitBitmap = GetSingleDigitBitmap(Int32.Parse(new String(altitudeString[0], 1)));
            Bitmap thousandsDigitBitmap = GetSingleDigitBitmap(Int32.Parse(new String(altitudeString[1], 1)));
            Bitmap hundredsDigitBitmap = GetSingleDigitBitmap(Int32.Parse(new String(altitudeString[2], 1)));
            float tensDigitFrac = (float)(Int32.Parse(new String(altitudeString[4], 1)) / 10.0f);
            float tensDigitsVal = (float)(Int32.Parse(new String(altitudeString[3], 1)));
            float tensDigits = tensDigitsVal + tensDigitFrac;
            Bitmap tensDigitsBitmap = GetSingleDigitBitmap(tensDigits, true);
            Bitmap onesDigitBitmap = GetSingleDigitBitmap(0);

            Rectangle tenThousandsDigitRectangle = new Rectangle(new Point(547, 230), new Size(tenThousandsDigitBitmap.Width, tenThousandsDigitBitmap.Height));
            Rectangle thousandsDigitRectangle = new Rectangle(new Point(558, 230), new Size(thousandsDigitBitmap.Width, thousandsDigitBitmap.Height));
            Rectangle hundredsDigitRectangle = new Rectangle(new Point(569, 230), new Size(hundredsDigitBitmap.Width, hundredsDigitBitmap.Height));
            Rectangle tensDigitsRectangle = new Rectangle(new Point(581, 218), new Size(tensDigitsBitmap.Width, 47));
            Rectangle onesDigitRectangle = new Rectangle(new Point(592, 230), new Size(onesDigitBitmap.Width, onesDigitBitmap.Height));

            g.DrawImage(tenThousandsDigitBitmap, tenThousandsDigitRectangle);
            g.DrawImage(thousandsDigitBitmap, thousandsDigitRectangle);
            g.DrawImage(hundredsDigitBitmap, hundredsDigitRectangle);
            g.DrawImage(tensDigitsBitmap, tensDigitsRectangle);
            g.DrawImage(onesDigitBitmap, onesDigitRectangle);

            Rectangle altitudeFeetTextRectangle = new Rectangle(new Point(540, 210), new Size(45, 10));
            StringFormat altitudeFeetTextFormat = new StringFormat(StringFormatFlags.NoWrap);
            altitudeFeetTextFormat.Alignment = StringAlignment.Center;
            altitudeFeetTextFormat.LineAlignment = StringAlignment.Center;
            altitudeFeetTextFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;
            GraphicsPath path = new GraphicsPath();
            path.AddString("FEET", altitudeFeetTextFont.FontFamily, (int)altitudeFeetTextFont.Style, altitudeFeetTextFont.SizeInPoints, altitudeFeetTextRectangle, altitudeFeetTextFormat);
            g.DrawPath(blackPen, path);
            g.FillPath(Brushes.White, path);
        }

        private void DrawAirspeedTape(Graphics g)
        {
            //************************************
            //AIRSPEED TAPE
            //************************************

            Point pointA, pointB, pointC, pointD, pointE, pointF, pointG, pointH, pointI, pointJ, pointK;
            Point[] points;
            
            Bitmap hundredsDigit, tensDigit, onesDigits;
            
            Rectangle hundredsDigitRectangle, tensDigitRectangle, onesDigitRectangle, onesDigitsRectangle;
            
            Font airspeedIndexFont = new Font("Lucida Console", 13, FontStyle.Bold);
            Font airspeedKtsFont = new Font("Lucida Console", 14, FontStyle.Bold);

            Color indexTextColor = Color.FromArgb(255, 128, 255);
            Brush purpleBrush = new SolidBrush(indexTextColor);

            //draw bounding box for airspeed tape
            Rectangle airspeedStripBoundingBox = new Rectangle(new Point(5, 100), new Size(54, 279));
            Rectangle airspeedIndexBox = new Rectangle(new Point(airspeedStripBoundingBox.Left, airspeedStripBoundingBox.Top - 19), new Size(airspeedStripBoundingBox.Width, 19));
            Rectangle airspeedIndexTextBox = new Rectangle(new Point(airspeedStripBoundingBox.Left, airspeedStripBoundingBox.Top - 20), new Size(airspeedStripBoundingBox.Width, 25));

            float airspeedFps = Manager.FlightData.IndicatedAirspeedInDecimalFeetPerSecond;
            float airspeedKnots = (float)(Math.Round((airspeedFps / Constants.FPS_PER_KNOT), 1));
            float airspeedIndexKnots = Manager.AirspeedIndexInKnots;


            Bitmap airspeedBitmap = GetAirspeedTapeBitmap(airspeedKnots, airspeedStripBoundingBox.Width, airspeedStripBoundingBox.Height);
            g.DrawImage(
                airspeedBitmap,
                airspeedStripBoundingBox.Left,
                airspeedStripBoundingBox.Top,
                new Rectangle(
                    new Point(0, 0),
                    new Size(airspeedBitmap.Width, airspeedStripBoundingBox.Height)
                    ), GraphicsUnit.Pixel
            );
            //trace the outline of the airspeed tape with black
            g.DrawRectangle(new Pen(Color.Black), airspeedStripBoundingBox);

            //draw airspeed index box
            StringFormat airspeedIndexFormat = new StringFormat(StringFormatFlags.NoWrap);
            airspeedIndexFormat.Alignment = StringAlignment.Center;
            airspeedIndexFormat.LineAlignment = StringAlignment.Center;
            g.FillRectangle(Brushes.Black, airspeedIndexBox);

            //add airspeed index text to index box
            g.DrawString(string.Format("{0:000}", airspeedIndexKnots), airspeedIndexFont, purpleBrush, airspeedIndexTextBox, airspeedIndexFormat);

            //draw white line under airspeed index box
            Pen whitePen = new Pen(Color.White);
            whitePen.Width = 2;
            g.DrawLine(whitePen, airspeedIndexBox.Left, airspeedIndexBox.Bottom, airspeedIndexBox.Right, airspeedIndexBox.Bottom);

            //draw white line at bottom of airspeed strip
            whitePen.Width = 2;
            g.DrawLine(whitePen, airspeedStripBoundingBox.Left, airspeedStripBoundingBox.Bottom, airspeedStripBoundingBox.Right, airspeedStripBoundingBox.Bottom);

            //draw airspeed counter box
            pointA = new Point(
                    2, 222);
            pointB = new Point(
                    2, 255);
            pointC = new Point(
                    26, 255);
            pointD = new Point(
                    26, 267);
            pointE = new Point(
                    47, 267);
            pointF = new Point(
                    47, 243);
            pointG = new Point(
                    54, 239);
            pointH = new Point(
                    47, 234);
            pointI = new Point(
                    47, 212);
            pointJ = new Point(
                    26, 212);
            pointK = new Point(
                    26, 222);

            whitePen.Width = 1;
            points = new Point[] { pointA, pointB, pointC, pointD, pointE, pointF, pointG, pointH, pointI, pointJ, pointK };
            g.FillPolygon(Brushes.Black, points);
            g.DrawPolygon(whitePen, points);

            string airspeedString = string.Format("{0:0000}", Math.Truncate(airspeedKnots));
            hundredsDigit = GetSingleDigitBitmap(Int32.Parse(new String(airspeedString[1], 1)));
            tensDigit = GetSingleDigitBitmap(Int32.Parse(new String(airspeedString[2], 1)));
            int onesVal = Int32.Parse(new String(airspeedString[3], 1));
            float onesFrac = (float)Math.Round((onesVal + (airspeedKnots - Math.Truncate(airspeedKnots))), 1);
            onesDigits = GetSingleDigitBitmap(onesFrac, true);
            hundredsDigitRectangle = new Rectangle(new Point(5, 230), new Size(hundredsDigit.Width, hundredsDigit.Height));
            tensDigitRectangle = new Rectangle(new Point(18, 230), new Size(tensDigit.Width, tensDigit.Height));
            onesDigitsRectangle = new Rectangle(new Point(32, 217), new Size(onesDigits.Width, onesDigits.Height));

            //draw airspeed counter digits
            g.DrawImage(hundredsDigit, hundredsDigitRectangle);
            g.DrawImage(tensDigit, tensDigitRectangle);
            g.DrawImage(onesDigits, onesDigitsRectangle);

            Rectangle airspeedKtsRectangle = new Rectangle(new Point(15, 200), new Size(40, 15));
            StringFormat airspeedKtsFormat = new StringFormat(StringFormatFlags.NoWrap);
            airspeedKtsFormat.Alignment = StringAlignment.Center;
            airspeedKtsFormat.LineAlignment = StringAlignment.Center;
            airspeedKtsFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;
            GraphicsPath path = new GraphicsPath();
            path.AddString("KTS", airspeedKtsFont.FontFamily, (int)airspeedKtsFont.Style, airspeedKtsFont.SizeInPoints, airspeedKtsRectangle, airspeedKtsFormat);
            g.DrawPath(new Pen(Color.Black), path);
            g.FillPath(Brushes.White, path);
        }

        private float LimitRateOfTurn(float instantaneousRateOfTurnDegreesPerSecond)
        {
            float indicatedRateOfTurnDegreesPerSecond = instantaneousRateOfTurnDegreesPerSecond;

            /*  LIMIT INDICATED RATE OF TURN TO BE WITHIN CERTAIN OUTER BOUNDARIES */
            float maxIndicatedRateOfTurnDegreesPerSecond = Pfd.MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND+ 0.75f;
            float minIndicatedRateOfTurnDegreesPerSecond = -maxIndicatedRateOfTurnDegreesPerSecond;

            if (instantaneousRateOfTurnDegreesPerSecond < minIndicatedRateOfTurnDegreesPerSecond)
            {
                indicatedRateOfTurnDegreesPerSecond = minIndicatedRateOfTurnDegreesPerSecond;
            }
            else if (instantaneousRateOfTurnDegreesPerSecond > maxIndicatedRateOfTurnDegreesPerSecond)
            {
                indicatedRateOfTurnDegreesPerSecond = maxIndicatedRateOfTurnDegreesPerSecond;
            }
            return indicatedRateOfTurnDegreesPerSecond;
        }
        private Bitmap GetAltitudeTapeBitmap(float altitudeInFeet, int widthPixels, int heightPixels)
        {
            int verticalSeparationBetweenTicks = 10;

            int thousands = (int)(altitudeInFeet / 1000.0f);

            if (_altitudeTapes[thousands + 4] == null)
            {
                _altitudeTapes[thousands + 4] = GenerateValuesTape(
                    Color.FromArgb(160, Color.Gray), //positiveBackgroundColor
                    Color.White, //positiveForegroundColor
                    Color.FromArgb(160, Color.Gray), //negativeBackgroundColor
                    Color.White, //negativeForegroundColor
                    100,//majorUnitInterval 
                    20, //minorUnitInterval
                    12, //majorUnitLineLengthPixels 
                    5, //minorUnitLineLengthPixels
                    true, //negativeUnitsLabelled
                    verticalSeparationBetweenTicks, //verticalSeparationBetweenTicksInPixels
                    (thousands * 1000) + 2000, //scaleMaxVal
                    (thousands * 1000) - 2000, //scaleMinVal
                    widthPixels, //tapeWidthInPixels
                    HAlignment.Left, //ticksAlignment
                    0, //textPaddingPixels
                    new Font("Lucida Console", 12, FontStyle.Bold),//majorUnitFont 
                    HAlignment.Left,//textAlignment
                    true, //negativeUnitsHaveSign
                    null
                );
            }

            Bitmap start = (Bitmap)_altitudeTapes[thousands + 4].Clone();
            Point centerPoint = new Point(start.Width / 2, start.Height / 2);
            int altitudeBetweenTicksInFeet = 20;
            float pixelsSeparationPerFootOfAltitude = (float)verticalSeparationBetweenTicks / (float)altitudeBetweenTicksInFeet;
            int currentValY = centerPoint.Y - ((int)(((altitudeInFeet - (thousands * 1000)) * (float)pixelsSeparationPerFootOfAltitude)));
            int topY = (int)(currentValY - (heightPixels / 2));
            int bottomY = (int)(currentValY + (heightPixels / 2));

            Point topLeftCrop = new Point(0, topY);
            Point bottomRightCrop = new Point(start.Width, bottomY);

            if (topLeftCrop.Y < 0) topLeftCrop.Y = 0;
            if (topLeftCrop.Y > start.Height) topLeftCrop.Y = start.Height;
            if (bottomRightCrop.Y < 0) bottomRightCrop.Y = 0;
            if (bottomRightCrop.Y > start.Height) bottomRightCrop.Y = start.Height;

            Rectangle cropRectangle = new Rectangle(topLeftCrop.X, topLeftCrop.Y, bottomRightCrop.X - topLeftCrop.X, bottomRightCrop.Y - topLeftCrop.Y);

            //add index bug to altitude tape
            Point bugA = new Point(-2, 0);
            Point bugB = new Point(-2, 21);
            Point bugC = new Point(10, 21);
            Point bugD = new Point(10, 16);
            Point bugE = new Point(3, 11);
            Point bugF = new Point(3, 10);
            Point bugG = new Point(10, 5);
            Point bugH = new Point(10, 0);
            Point[] bugPoints = new Point[] { bugA, bugB, bugC, bugD, bugE, bugF, bugG, bugH };
            Color altitudeIndexBugColor = Color.Magenta;
            Brush altitudeIndexBugBrush = new SolidBrush(altitudeIndexBugColor);
            using (Graphics h = Graphics.FromImage(start))
            {
                Matrix origTransform = h.Transform;
                h.SmoothingMode = SmoothingMode.AntiAlias;
                int altitudeIndexBugY = centerPoint.Y - ((int)(((Manager.AltitudeIndexInFeet - (thousands * 1000)) * (float)pixelsSeparationPerFootOfAltitude)));
                if (altitudeIndexBugY < (cropRectangle.Top + 3)) altitudeIndexBugY = cropRectangle.Top + 3;
                if (altitudeIndexBugY > (cropRectangle.Bottom - 3)) altitudeIndexBugY = cropRectangle.Bottom - 3;
                h.TranslateTransform(0, altitudeIndexBugY - 11);
                h.FillPolygon(altitudeIndexBugBrush, bugPoints);
                h.Transform = origTransform;
            }

            Bitmap cropped = Common.Imaging.Util.CropBitmap(start, cropRectangle);
            return cropped;
        }

        private Bitmap GetVerticalVelocityTapeBitmap(float verticalVelocity, int widthPixels, int heightPixels)
        {
            verticalVelocity = 0;//override passed-in vertical velocity so tape doesn't move
            int verticalSeparationBetweenTicks = 20;
            if (_vviTape == null)
            {

                _vviTape = GenerateValuesTape(
                    Color.White, //positiveBackgroundColor
                    Color.Black, //positiveForegroundColor
                    Color.Black, //negativeBackgroundColor
                    Color.White, //positiveBackgroundColor
                    2, //majorUnitInterval
                    1, //minorUnitInterval 
                    11,//majorUnitLineLengthInPixels
                    7,//minorUnitLineLengthInPixels 
                    true, //negativeUnitsLabelled
                    verticalSeparationBetweenTicks, //verticalSeparationBetweenTicksInPixels
                    7, //scaleMaxVal
                    -7, //scaleMinVal
                    widthPixels, //tapeWidthInPixels
                    HAlignment.Left, //ticksAlignment
                    0, //textPaddingPixels
                    new Font("Lucida Console", 10, FontStyle.Bold), //majorUnitFont
                    HAlignment.Right,//textAlignment
                    false, //negativeUnitsHaveNegativeSign
                    null
                    );
                Bitmap start = (Bitmap)_vviTape;
                Point centerPoint = new Point(start.Width / 2, start.Height / 2);
                int velocityBetweenTicksInHundredFps = 1;//in hundreds of feet per second
                float pixelsSeparationPerHundredFps = (float)verticalSeparationBetweenTicks / (float)velocityBetweenTicksInHundredFps;
                int currentValY = centerPoint.Y - ((int)((verticalVelocity * (float)pixelsSeparationPerHundredFps)));
                int topY = (int)(currentValY - (heightPixels / 2));
                int bottomY = (int)(currentValY + (heightPixels / 2));

                Point topLeftCrop = new Point(0, topY);
                Point bottomRightCrop = new Point(start.Width, bottomY);

                if (topLeftCrop.Y < 0) topLeftCrop.Y = 0;
                if (topLeftCrop.Y > start.Height) topLeftCrop.Y = start.Height;
                if (bottomRightCrop.Y < 0) bottomRightCrop.Y = 0;
                if (bottomRightCrop.Y > start.Height) bottomRightCrop.Y = start.Height;

                Rectangle cropRectangle = new Rectangle(topLeftCrop.X, topLeftCrop.Y, bottomRightCrop.X - topLeftCrop.X, bottomRightCrop.Y - topLeftCrop.Y);
                Bitmap cropped = Common.Imaging.Util.CropBitmap(start, cropRectangle);

                //cut zero-mark cutout into tape
                using (Graphics g = Graphics.FromImage(cropped))
                {
                    int centerY = (cropped.Height / 2);
                    int rightX = cropped.Width;
                    int leftX = 0;
                    Point upperRightTriangleCorner = new Point(rightX, centerY - 20);
                    Point lowerRightTriangleCorner = new Point(rightX, centerY + 20);
                    Point middleLeftTriangleCorner = new Point(leftX, centerY);
                    g.FillPolygon(Brushes.Fuchsia, new Point[] { upperRightTriangleCorner, middleLeftTriangleCorner, lowerRightTriangleCorner });
                }
                cropped.MakeTransparent(Color.Fuchsia);
                _vviTape = cropped;
            }
            return _vviTape;

        }
        private Bitmap GetAoATapeBitmap(float aoaInDegrees, int widthPixels, int heightPixels)
        {
            int verticalSeparationBetweenTicks = 10;
            int scaleMaxVal = 90;
            int scaleMinVal = -90;
            if (aoaInDegrees > scaleMaxVal) aoaInDegrees = scaleMaxVal;
            if (aoaInDegrees < scaleMinVal) aoaInDegrees = scaleMinVal;
            if (_aoaTape == null)
            {
                TapeEdgeColoringInstruction aoaYellow = new TapeEdgeColoringInstruction();
                aoaYellow.color = Color.Yellow;
                aoaYellow.minVal = AOA_YELLOW_RANGE_MIN_ANGLE_DEGREES;
                aoaYellow.maxVal = AOA_YELLOW_RANGE_MAX_ANGLE_DEGREES;

                TapeEdgeColoringInstruction aoaGreen = new TapeEdgeColoringInstruction();
                aoaGreen.color = Color.Green;
                aoaGreen.minVal = AOA_GREEN_RANGE_MIN_ANGLE_DEGREES;
                aoaGreen.maxVal = AOA_GREEN_RANGE_MAX_ANGLE_DEGREES;


                TapeEdgeColoringInstruction aoaRed = new TapeEdgeColoringInstruction();
                aoaRed.color = Color.Red;
                aoaRed.minVal = AOA_RED_RANGE_MIN_ANGLE_DEGREES;
                aoaRed.maxVal = AOA_RED_RANGE_MAX_ANGLE_DEGREES;

                _aoaTape = GenerateValuesTape(
                    Color.FromArgb(160, Color.Gray), //positiveBackgroundColor
                    Color.White, //postiveForegroundColor
                    Color.FromArgb(160, Color.Gray), //negativeBackgroundColor
                    Color.White, //negativeForegroundColor
                    5,//majorUnitInterval
                    1, //minorUnitInterval
                    0, //majorUnitLineLengthPixels
                    21, //minorUnitLineLengthPixels
                    true, //negativeUnitsLabelled 
                    verticalSeparationBetweenTicks, //verticalSeparationBetweenTicksPixels
                    scaleMaxVal, //scaleMaxVal 
                    scaleMinVal,//scaleMinVal 
                    widthPixels, //tapeWidthPixels
                    HAlignment.Center, //ticksAlignment
                    0, //textPaddingPixels
                    new Font("Lucida Console", 12, FontStyle.Bold),//majorUnitFont 
                    HAlignment.Right, //textAlignment
                    true, //negativeUnitsHaveNegativeSign
                    new TapeEdgeColoringInstruction[] { aoaYellow, aoaGreen, aoaRed }

                );
            }

            Bitmap start = (Bitmap)_aoaTape;
            Point centerPoint = new Point(start.Width / 2, start.Height / 2);
            int quantityBetweenTicksInDegreesAoa = 1;
            float pixelsSeparationPerDegreeAoa = (float)verticalSeparationBetweenTicks / (float)quantityBetweenTicksInDegreesAoa;
            int currentValY = centerPoint.Y - ((int)((aoaInDegrees * (float)pixelsSeparationPerDegreeAoa)));
            int topY = (int)(currentValY - (heightPixels / 2));
            int bottomY = (int)(currentValY + (heightPixels / 2));

            Point topLeftCrop = new Point(0, topY);
            Point bottomRightCrop = new Point(start.Width, bottomY);

            if (topLeftCrop.Y < 0) topLeftCrop.Y = 0;
            if (topLeftCrop.Y > start.Height) topLeftCrop.Y = start.Height;
            if (bottomRightCrop.Y < 0) bottomRightCrop.Y = 0;
            if (bottomRightCrop.Y > start.Height) bottomRightCrop.Y = start.Height;

            Rectangle cropRectangle = new Rectangle(topLeftCrop.X, topLeftCrop.Y, bottomRightCrop.X - topLeftCrop.X, bottomRightCrop.Y - topLeftCrop.Y);
            Bitmap cropped = Common.Imaging.Util.CropBitmap(start, cropRectangle);

            return cropped;
        }
        private Bitmap GetAirspeedTapeBitmap(float indicatedAirspeedKnots, int widthPixels, int heightPixels)
        {
            int verticalSeparationBetweenTicks = 9;
            int scaleMaxVal = 1200;
            int scaleMinVal = -1200;
            if (indicatedAirspeedKnots > scaleMaxVal) indicatedAirspeedKnots = scaleMaxVal;
            if (indicatedAirspeedKnots < scaleMinVal) indicatedAirspeedKnots = scaleMinVal;


            if (_airspeedTape == null)
            {
                _airspeedTape = GenerateValuesTape(
                    Color.FromArgb(160, Color.Gray), //positiveBackgroundColor
                    Color.White, //positiveForegroundColor
                    Color.FromArgb(160, Color.Gray), //negativeBackgroundColor
                    Color.White, //negativeForegroundColor
                    100, //majorUnitInterval
                    20, //minorUnitInterval 
                    12, //majorUnitLineLengthInPixels
                    7, //minorUnitLineLengthInPixels,
                    false, //negativeUnitsLabelled
                    verticalSeparationBetweenTicks, //verticalSeparationBetweenTicksInPixels
                    scaleMaxVal, //scaleMaxVal
                    scaleMinVal, //scaleMinVal
                    widthPixels, //tapeWidthInPixels
                    HAlignment.Right, //ticsAlignment
                    3, //paddingPixels
                    new Font("Lucida Console", 11, FontStyle.Bold), //majorUnitFont
                    HAlignment.Right, //textAlignment
                    true, //negativeUnitsHaveNegativeSign
                    null
                    );
            }

            Bitmap start = (Bitmap)_airspeedTape.Clone();

            Point centerPoint = new Point(start.Width / 2, start.Height / 2);
            int knotsBetweenTicks = 20;
            float pixelsSeparationPerKnot = (float)verticalSeparationBetweenTicks / (float)knotsBetweenTicks;
            int currentAirspeedY = centerPoint.Y - ((int)((indicatedAirspeedKnots * (float)pixelsSeparationPerKnot)));


            int topY = (int)(currentAirspeedY - (heightPixels / 2));
            int bottomY = (int)(currentAirspeedY + (heightPixels / 2));

            Point topLeftCrop = new Point(0, topY);
            Point bottomRightCrop = new Point(start.Width, bottomY);

            if (topLeftCrop.Y < 0) topLeftCrop.Y = 0;
            if (topLeftCrop.Y > start.Height) topLeftCrop.Y = start.Height;
            if (bottomRightCrop.Y < 0) bottomRightCrop.Y = 0;
            if (bottomRightCrop.Y > start.Height) bottomRightCrop.Y = start.Height;

            Rectangle cropRectangle = new Rectangle(topLeftCrop.X, topLeftCrop.Y, bottomRightCrop.X - topLeftCrop.X, bottomRightCrop.Y - topLeftCrop.Y);


            //add index bug to airspeed tape
            Point bugA = new Point(-2, 0);
            Point bugB = new Point(-2, 5);
            Point bugC = new Point(5, 10);
            Point bugD = new Point(5, 11);
            Point bugE = new Point(-2, 16);
            Point bugF = new Point(-2, 21);
            Point bugG = new Point(10, 21);
            Point bugH = new Point(10, 0);

            Point[] bugPoints = new Point[] { bugA, bugB, bugC, bugD, bugE, bugF, bugG, bugH };
            Color airspeedIndexBugColor = Color.Magenta;
            Brush airspeedIndexBugBrush = new SolidBrush(airspeedIndexBugColor);
            using (Graphics h = Graphics.FromImage(start))
            {
                h.SmoothingMode = SmoothingMode.AntiAlias;
                Matrix origTransform = h.Transform;
                int airspeedIndexBugY = centerPoint.Y - ((int)((Manager.AirspeedIndexInKnots * (float)pixelsSeparationPerKnot)));
                if (airspeedIndexBugY < (cropRectangle.Top + 3)) airspeedIndexBugY = cropRectangle.Top + 3;
                if (airspeedIndexBugY > (cropRectangle.Bottom - 3)) airspeedIndexBugY = cropRectangle.Bottom - 3;

                h.TranslateTransform(start.Width - 10, airspeedIndexBugY - 11);
                h.FillPolygon(airspeedIndexBugBrush, bugPoints);
                h.Transform = origTransform;
            }



            Bitmap cropped = Common.Imaging.Util.CropBitmap(start, cropRectangle);
            return cropped;
        }

        private Bitmap GetAdiPitchLadder(float degreesPitch, float degreesRoll, int width, int height)
        {
            if (_adiPitchBars == null)
            {
                _adiPitchBars = GenerateAdiPitchBarBitmap();
            }
            Bitmap start = (Bitmap)_adiPitchBars;
            Point centerPoint = new Point(start.Width / 2, start.Height / 2);

            int verticalDistanceBetweenPitchLines = 25;
            int degreesBetweenTicks = 5;
            float pixelsSeparationPerDegreeOfPitch = verticalDistanceBetweenPitchLines / degreesBetweenTicks;
            int currentPitchY = centerPoint.Y - ((int)((degreesPitch * (float)pixelsSeparationPerDegreeOfPitch)));
            int topY = (int)(currentPitchY - (25 * pixelsSeparationPerDegreeOfPitch));
            int bottomY = (int)(currentPitchY + (25 * pixelsSeparationPerDegreeOfPitch) + 1);

            Point topLeftCrop = new Point(0, topY);
            Point bottomRightCrop = new Point(start.Width, bottomY);

            if (topLeftCrop.Y < 0) topLeftCrop.Y = 0;
            if (topLeftCrop.Y > start.Height) topLeftCrop.Y = start.Height;
            if (bottomRightCrop.Y < 0) bottomRightCrop.Y = 0;
            if (bottomRightCrop.Y > start.Height) bottomRightCrop.Y = start.Height;

            Rectangle cropRectangle = new Rectangle(topLeftCrop.X, topLeftCrop.Y, bottomRightCrop.X - topLeftCrop.X, bottomRightCrop.Y - topLeftCrop.Y);
            //Bitmap cropped = Common.Imaging.Util.CropBitmap(start, cropRectangle);

            int heightOffset = 44;
            Bitmap adi = new Bitmap(cropRectangle.Height + (heightOffset * 2), cropRectangle.Height + (heightOffset * 2), PixelFormat.Format16bppRgb565);
            adi.MakeTransparent();
            using (Graphics g = Graphics.FromImage(adi))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.TranslateTransform((int)((float)adi.Width / (float)2), (int)((float)adi.Height / (float)2));
                g.RotateTransform(-degreesRoll);
                g.TranslateTransform((int)((float)-adi.Width / (float)2), (int)((float)-adi.Height / (float)2));
                g.DrawImage(start, new Rectangle(heightOffset, heightOffset, cropRectangle.Width, cropRectangle.Height), cropRectangle, GraphicsUnit.Pixel);

                //draw sky pointer triangle
                Brush whiteBrush = new SolidBrush(Color.White);
                Pen greyPen = new Pen(Color.DarkGray);
                int centerX = (adi.Width / 2);
                Point[] skyPointerTriangle = new Point[] { new Point(centerX, 0), new Point(centerX - 12, 25), new Point(centerX + 12, 25) };
                g.FillPolygon(whiteBrush, skyPointerTriangle);
                g.DrawPolygon(greyPen, skyPointerTriangle);
            }
            //Bitmap rotated = Common.Imaging.Util.RotateBitmap((Bitmap)adi, degreesRoll);
            return adi;
        }
        private static Bitmap GenerateAdiPitchBarBitmap()
        {
            bool useBentPitchLines = false;
            int positivePitchLineExtenderHeightPixels = 14;
            int negativePitchLineExtenderHeightPixels = 13;
            int negativePitchInsideDashWidthPixels = 13;
            int negativePitchDashGapPixels = 13;
            int negativePitchMiddleDashWidthPixels = 11;
            int negativePitchOuterDashWidthPixels = 11;

            int majorPositivePitchLineWidthPixels = 61;
            int minorPositivePitchLineWidthPixels = 39;
            int verticalDistanceBetweenPitchLines = 25;
            int degreesBetweenTicks = 5;
            int pixelsSeparationPerDegreeOfPitch = verticalDistanceBetweenPitchLines / degreesBetweenTicks;
            int horizontalOffsetPixelsFromCenterLine = 28;

            Font labelFont = new Font("Lucida Console", 10, FontStyle.Bold);

            Rectangle boundingRectangle = new Rectangle(0, 0, 250, (pixelsSeparationPerDegreeOfPitch * 360) + 2);
            //Rectangle boundingRectangle = new Rectangle(0, 0, (pixelsSeparationPerDegreeOfPitch * 360) + 2,(pixelsSeparationPerDegreeOfPitch * 360) + 2);
            Rectangle positiveBoundingRectangle = new Rectangle(new Point(0, 0), new Size(boundingRectangle.Width, boundingRectangle.Height / 2));
            Rectangle negativeBoundingRectangle = new Rectangle(new Point(0, positiveBoundingRectangle.Bottom), new Size(boundingRectangle.Width, boundingRectangle.Height - positiveBoundingRectangle.Height));

            Bitmap bitmap = new Bitmap(boundingRectangle.Width, boundingRectangle.Height, PixelFormat.Format16bppRgb565);
            bitmap.MakeTransparent();
            int centerPointX = boundingRectangle.Left + (boundingRectangle.Width / 2);
            Pen whitePen = new Pen(Color.White);
            Brush whiteBrush = new SolidBrush(Color.White);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                whitePen.Width = 2;
                Matrix origTransform = g.Transform;

                //produce positive pitch lines
                for (int i = 5; i < 90; i += degreesBetweenTicks)
                {
                    int pitchLineCenterY = positiveBoundingRectangle.Bottom - (i * pixelsSeparationPerDegreeOfPitch);
                    int pitchLineWidthPixels = minorPositivePitchLineWidthPixels;
                    if (i % 10 == 0)
                    {
                        pitchLineWidthPixels = majorPositivePitchLineWidthPixels;

                        if (useBentPitchLines)
                        {
                            g.Transform = origTransform;
                            g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                            g.RotateTransform(i / 2);
                            g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                        }

                        //print the label for this pitch line
                        StringFormat labelFormat = new StringFormat(StringFormatFlags.NoWrap);
                        labelFormat.Alignment = StringAlignment.Near;
                        labelFormat.LineAlignment = StringAlignment.Center;
                        g.DrawString(string.Format("{0:0}", i), labelFont, whiteBrush, new Point(0, pitchLineCenterY + 7), labelFormat);
                    }
                    int lhsPitchLineMinX = centerPointX - pitchLineWidthPixels - horizontalOffsetPixelsFromCenterLine;
                    int lhsPitchLineMaxX = centerPointX - horizontalOffsetPixelsFromCenterLine;
                    int lhsPitchLineExtenderBottomY = pitchLineCenterY + positivePitchLineExtenderHeightPixels;

                    int rhsPitchLineMinX = centerPointX + horizontalOffsetPixelsFromCenterLine;
                    int rhsPitchLineMaxX = centerPointX + pitchLineWidthPixels + horizontalOffsetPixelsFromCenterLine;
                    int rhsPitchLineExtenderBottomY = pitchLineCenterY + positivePitchLineExtenderHeightPixels;

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                        g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                        g.RotateTransform(i / 2);
                        g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                    }
                    g.DrawLine(whitePen, lhsPitchLineMinX, pitchLineCenterY, lhsPitchLineMaxX, pitchLineCenterY); //draw LHS pitch line
                    g.DrawLine(whitePen, lhsPitchLineMinX, pitchLineCenterY, lhsPitchLineMinX, lhsPitchLineExtenderBottomY); //draw LHS pitch line extender

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                        g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                        g.RotateTransform(-i / 2);
                        g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                    }
                    g.DrawLine(whitePen, rhsPitchLineMinX, pitchLineCenterY, rhsPitchLineMaxX, pitchLineCenterY); //draw RHS pitch line
                    g.DrawLine(whitePen, rhsPitchLineMaxX, pitchLineCenterY, rhsPitchLineMaxX, rhsPitchLineExtenderBottomY); //draw RHS pitch line extender

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                    }
                }

                if (useBentPitchLines)
                {
                    g.Transform = origTransform;
                }

                //draw the zenith symbol
                whitePen.Width = 1;
                int zenithSymbolWidth = 25;
                Rectangle zenithRectangle = new Rectangle(new Point((int)((positiveBoundingRectangle.Width - zenithSymbolWidth) / 2), (int)(positiveBoundingRectangle.Bottom - (90 * pixelsSeparationPerDegreeOfPitch) - (zenithSymbolWidth / 2))), new Size(zenithSymbolWidth, zenithSymbolWidth));
                g.DrawEllipse(whitePen, zenithRectangle);
                g.DrawLine(whitePen, zenithRectangle.Left + (zenithRectangle.Width / 2), zenithRectangle.Bottom, zenithRectangle.Left + (zenithRectangle.Width / 2), zenithRectangle.Bottom + 10);

                whitePen.Width = 2;
                //produce negative pitch lines
                for (int i = -5; i > -90; i -= degreesBetweenTicks)
                {
                    int pitchLineCenterY = negativeBoundingRectangle.Top + ((Math.Abs(i) * pixelsSeparationPerDegreeOfPitch));
                    int pitchLineExtenderY = pitchLineCenterY - negativePitchLineExtenderHeightPixels;
                    int lhsPitchLineInsideDashMinX = centerPointX - negativePitchInsideDashWidthPixels - horizontalOffsetPixelsFromCenterLine;
                    int lhsPitchLineInsideDashMaxX = centerPointX - horizontalOffsetPixelsFromCenterLine;
                    int lhsPitchLineMiddleDashMinX = lhsPitchLineInsideDashMinX - negativePitchDashGapPixels - negativePitchMiddleDashWidthPixels;
                    int lhsPitchLineMiddleDashMaxX = lhsPitchLineInsideDashMinX - negativePitchDashGapPixels;
                    int lhsPitchLineOuterDashMinX = lhsPitchLineMiddleDashMinX - negativePitchDashGapPixels - negativePitchOuterDashWidthPixels;
                    int lhsPitchLineOuterDashMaxX = lhsPitchLineMiddleDashMinX - negativePitchDashGapPixels;

                    int rhsPitchLineInsideDashMinX = centerPointX + horizontalOffsetPixelsFromCenterLine;
                    int rhsPitchLineInsideDashMaxX = centerPointX + negativePitchInsideDashWidthPixels + horizontalOffsetPixelsFromCenterLine;
                    int rhsPitchLineMiddleDashMinX = rhsPitchLineInsideDashMaxX + negativePitchDashGapPixels;
                    int rhsPitchLineMiddleDashMaxX = rhsPitchLineInsideDashMaxX + negativePitchDashGapPixels + negativePitchMiddleDashWidthPixels;
                    int rhsPitchLineOuterDashMinX = rhsPitchLineMiddleDashMaxX + negativePitchDashGapPixels;
                    int rhsPitchLineOuterDashMaxX = rhsPitchLineMiddleDashMaxX + negativePitchDashGapPixels + negativePitchOuterDashWidthPixels;

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                        g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                        g.RotateTransform(i / 2);
                        g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                    }

                    g.DrawLine(whitePen, lhsPitchLineInsideDashMinX, pitchLineCenterY, lhsPitchLineInsideDashMaxX, pitchLineCenterY); //draw LHS inside dash
                    g.DrawLine(whitePen, lhsPitchLineMiddleDashMinX, pitchLineCenterY, lhsPitchLineMiddleDashMaxX, pitchLineCenterY); //draw LHS middle dash
                    if (i % 10 == 0)
                    {
                        //if a major pitch line, then draw the LHS outer dash
                        g.DrawLine(whitePen, lhsPitchLineOuterDashMinX, pitchLineCenterY, lhsPitchLineOuterDashMaxX, pitchLineCenterY);

                        //print the label for this pitch line
                        if (useBentPitchLines)
                        {
                            g.Transform = origTransform;
                            g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                            g.RotateTransform(i / 2);
                            g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                        }

                        StringFormat labelFormat = new StringFormat(StringFormatFlags.NoWrap);
                        labelFormat.Alignment = StringAlignment.Near;
                        labelFormat.LineAlignment = StringAlignment.Center;
                        g.DrawString(string.Format("{0:0}", Math.Abs(i)), labelFont, whiteBrush, new Point(0, pitchLineCenterY), labelFormat);
                    }
                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                        g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                        g.RotateTransform(i / 2);
                        g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                    }
                    //draw the LHS pitch extender line
                    g.DrawLine(whitePen, lhsPitchLineInsideDashMaxX, pitchLineCenterY, lhsPitchLineInsideDashMaxX, pitchLineExtenderY);

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                        g.TranslateTransform(bitmap.Width / 2, pitchLineCenterY);
                        g.RotateTransform(-i / 2);
                        g.TranslateTransform(-bitmap.Width / 2, -pitchLineCenterY);
                    }

                    g.DrawLine(whitePen, rhsPitchLineInsideDashMinX, pitchLineCenterY, rhsPitchLineInsideDashMaxX, pitchLineCenterY); //draw RHS inside dash
                    g.DrawLine(whitePen, rhsPitchLineMiddleDashMinX, pitchLineCenterY, rhsPitchLineMiddleDashMaxX, pitchLineCenterY); //draw RHS middle dash
                    if (i % 10 == 0)
                    {
                        //if a major pitch line, then draw the RHS outer dash
                        g.DrawLine(whitePen, rhsPitchLineOuterDashMinX, pitchLineCenterY, rhsPitchLineOuterDashMaxX, pitchLineCenterY);
                    }
                    //draw the RHS pitch extender line
                    g.DrawLine(whitePen, rhsPitchLineInsideDashMinX, pitchLineCenterY, rhsPitchLineInsideDashMinX, pitchLineExtenderY);

                    if (useBentPitchLines)
                    {
                        g.Transform = origTransform;
                    }
                }

                if (useBentPitchLines)
                {
                    g.Transform = origTransform;
                }

                //draw the nadir symbol
                int nadirSymbolWidth = 25;
                whitePen.Width = 1;
                Rectangle nadirRectangle = new Rectangle(new Point((int)((negativeBoundingRectangle.Width - nadirSymbolWidth) / 2), (int)(negativeBoundingRectangle.Top + (90 * pixelsSeparationPerDegreeOfPitch) - (nadirSymbolWidth / 2))), new Size(nadirSymbolWidth, nadirSymbolWidth));
                g.FillEllipse(whiteBrush, nadirRectangle);
                g.DrawLine(whitePen, nadirRectangle.Left + (nadirRectangle.Width / 2), nadirRectangle.Top, nadirRectangle.Left + (nadirRectangle.Width / 2), nadirRectangle.Top - 10);

            }

            return bitmap;
        }

        private static Bitmap GenerateValuesTape(Color positiveBackgroundColor, Color positiveForegroundColor, Color negativeBackgroundColor, Color negativeForegroundColor, int majorUnitInterval, int minorUnitInterval, int majorUnitLineLengthInPixels, int minorUnitLineLengthInPixels, bool negativeUnitsLabelled, int verticalSeparationBetweenTicksInPixels, int scaleMaxVal, int scaleMinVal, int tapeWidthInPixels, HAlignment ticsAlignment, int textPaddingPixels, Font majorUnitFont, HAlignment textAlignment, bool negativeUnitsHaveNegativeSign, TapeEdgeColoringInstruction[] coloringInstructions)
        {
            int tapeHeightInPixels = (int)((((scaleMaxVal - scaleMinVal) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
            int positiveRange = scaleMaxVal;
            if (scaleMinVal > 0) positiveRange = scaleMaxVal - scaleMinVal;
            int positiveRegionHeightInPixels = ((positiveRange / minorUnitInterval) * verticalSeparationBetweenTicksInPixels);
            int negativeRange = Math.Abs(scaleMinVal);
            if (scaleMaxVal <= 0)
            {
                negativeRange = Math.Abs(scaleMaxVal - scaleMinVal);
            }
            if (scaleMinVal >= 0) negativeRange = 0;
            int negativeRegionHeightInPixels = (negativeRange / minorUnitInterval) * verticalSeparationBetweenTicksInPixels;
            Bitmap toReturn = new Bitmap(tapeWidthInPixels, tapeHeightInPixels, PixelFormat.Format16bppRgb565);
            toReturn.MakeTransparent();
            Rectangle boundingRectangle = new Rectangle(new Point(0, 0), new Size(toReturn.Width, toReturn.Height));
            Rectangle positiveRegionBoundingRectangle = new Rectangle(new Point(0, 0), new Size(toReturn.Width, positiveRegionHeightInPixels));
            Rectangle negativeRegionBoundingRectangle = new Rectangle(new Point(0, positiveRegionBoundingRectangle.Bottom), new Size(toReturn.Width, negativeRegionHeightInPixels));
            float baseFontSize = majorUnitFont.SizeInPoints;
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Matrix origTransform = g.Transform;
                Brush negativeBrush = new SolidBrush(negativeBackgroundColor);
                Brush positiveBrush = new SolidBrush(positiveBackgroundColor);
                Pen blackPen = new Pen(Color.Black);
                Pen positiveForegroundPen = new Pen(positiveForegroundColor);
                Pen negativeForegroundPen = new Pen(negativeForegroundColor);
                blackPen.Width = 2;
                Brush positiveForegroundBrush = new SolidBrush(positiveForegroundColor);
                Brush negativeForegroundBrush = new SolidBrush(negativeForegroundColor);

                //color negative portion of tape
                g.FillRectangle(negativeBrush, negativeRegionBoundingRectangle);
                //color positive portion of tape
                g.FillRectangle(positiveBrush, positiveRegionBoundingRectangle);

                //draw black line between negative and positive portion of tape
                g.DrawLine(blackPen, negativeRegionBoundingRectangle.Left, negativeRegionBoundingRectangle.Top, negativeRegionBoundingRectangle.Right, negativeRegionBoundingRectangle.Top);
                if (scaleMaxVal >= 0)
                {
                    int positiveScaleMin = 0;
                    if (scaleMinVal > 0) positiveScaleMin = scaleMinVal;
                    //draw positive unit marks and numbers
                    for (int i = positiveScaleMin; i <= scaleMaxVal; i += minorUnitInterval)
                    {
                        if ((i % minorUnitInterval == 0) && (i % majorUnitInterval != 0)) //this is a minor unit
                        {
                            //draw minor unit tick mark
                            if (ticsAlignment == HAlignment.Right)
                            {
                                //draw tick on the right hand side
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Right - minorUnitLineLengthInPixels,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Right, positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Left)
                            {
                                //draw tic on the left hand side
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Left,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Left + minorUnitLineLengthInPixels, positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Center)
                            {
                                //draw tic in the center 
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Left + ((positiveRegionBoundingRectangle.Width - minorUnitLineLengthInPixels) / 2),
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Left + ((positiveRegionBoundingRectangle.Width - minorUnitLineLengthInPixels) / 2) + minorUnitLineLengthInPixels,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                        }
                        else if (i % majorUnitInterval == 0) //this is a major unit
                        {
                            //draw major unit tick mark
                            if (ticsAlignment == HAlignment.Right)
                            {
                                //draw tic on the right hand side
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Right - majorUnitLineLengthInPixels,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Right, positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Left)
                            {
                                //draw tic on the left hand side
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Left,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Left + majorUnitLineLengthInPixels, positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Center)
                            {
                                //draw tic in the center
                                g.DrawLine(positiveForegroundPen,
                                        positiveRegionBoundingRectangle.Left + ((positiveRegionBoundingRectangle.Width - majorUnitLineLengthInPixels) / 2),
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        positiveRegionBoundingRectangle.Left + ((positiveRegionBoundingRectangle.Width - majorUnitLineLengthInPixels) / 2) + majorUnitLineLengthInPixels,
                                        positiveRegionBoundingRectangle.Bottom - (((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            Rectangle majorUnitTextBoundingRectangle = Rectangle.Empty;
                            if (ticsAlignment == HAlignment.Right)
                            {
                                //tic is on the right, so draw major unit text to left of tic 
                                majorUnitTextBoundingRectangle = new Rectangle(
                                    new Point(0,
                                        positiveRegionBoundingRectangle.Bottom - (
                                            ((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                        ) - verticalSeparationBetweenTicksInPixels
                                    ),
                                    new Size(positiveRegionBoundingRectangle.Width - majorUnitLineLengthInPixels - textPaddingPixels, verticalSeparationBetweenTicksInPixels * 2)
                                );
                            }
                            else if (ticsAlignment == HAlignment.Left)
                            {
                                //tic is on the left, so draw major unit text to right of tic
                                majorUnitTextBoundingRectangle = new Rectangle(
                                    new Point(majorUnitLineLengthInPixels + textPaddingPixels,
                                        positiveRegionBoundingRectangle.Bottom - (
                                            ((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                        ) - verticalSeparationBetweenTicksInPixels
                                    ),
                                    new Size(positiveRegionBoundingRectangle.Width - majorUnitLineLengthInPixels - textPaddingPixels, verticalSeparationBetweenTicksInPixels * 2)
                                );
                            }
                            else if (ticsAlignment == HAlignment.Center)
                            {
                                int lineLength = majorUnitLineLengthInPixels;
                                if (majorUnitLineLengthInPixels == 0) lineLength = minorUnitLineLengthInPixels;
                                lineLength += 8;

                                majorUnitTextBoundingRectangle = new Rectangle(
                                    new Point((positiveRegionBoundingRectangle.Width - lineLength) / 2,
                                        positiveRegionBoundingRectangle.Bottom - (
                                            ((i - positiveScaleMin) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                        ) - verticalSeparationBetweenTicksInPixels
                                    ),
                                    new Size(lineLength, verticalSeparationBetweenTicksInPixels * 2)
                                );
                            }

                            string majorUnitString = String.Empty;

                            if (i.ToString().Length > 3) // num >= 1000
                            {
                                majorUnitString = String.Format("{0:0000}", i);
                                majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize - 2, majorUnitFont.Style);
                            }
                            else if (i.ToString().Length > 2) // num >= 100
                            {
                                majorUnitString = String.Format("{0:000}", i);
                                majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                            }
                            else if (i.ToString().Length > 1) // num >= 10
                            {
                                majorUnitString = String.Format("{0:00}", i);
                                majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                            }
                            else if (i.ToString().Length == 1) // num between 1 and 10
                            {
                                majorUnitString = String.Format("{0:0}", i);
                                majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                            }
                            if (i == 0) majorUnitString = "0";
                            StringFormat majorUnitStringFormat = new StringFormat(StringFormatFlags.NoWrap);
                            if (textAlignment == HAlignment.Right)
                            {
                                majorUnitStringFormat.Alignment = StringAlignment.Far;
                            }
                            else if (textAlignment == HAlignment.Left)
                            {
                                majorUnitStringFormat.Alignment = StringAlignment.Near;
                            }
                            else if (textAlignment == HAlignment.Center)
                            {
                                majorUnitStringFormat.Alignment = StringAlignment.Center;
                            }
                            majorUnitStringFormat.LineAlignment = StringAlignment.Center;
                            g.TranslateTransform(0, 2);
                            g.DrawString(majorUnitString, majorUnitFont, positiveForegroundBrush, majorUnitTextBoundingRectangle, majorUnitStringFormat);
                            g.Transform = origTransform;
                        } //end else
                    } //end for
                }
                if (scaleMinVal <= 0)
                {
                    //draw negative unit marks and numbers
                    for (int i = 0 - minorUnitInterval; i >= scaleMinVal; i -= minorUnitInterval)
                    {
                        if ((i % minorUnitInterval == 0) && (i % majorUnitInterval != 0)) //this is a minor unit
                        {
                            if (ticsAlignment == HAlignment.Right)
                            {
                                //draw minor unit tic mark on right hand side
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Right - minorUnitLineLengthInPixels,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Right,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)
                                );
                            }
                            else if (ticsAlignment == HAlignment.Left)
                            {
                                //draw minor unit tic mark on left hand side
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Left,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Left + minorUnitLineLengthInPixels,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)
                                );
                            }
                            else if (ticsAlignment == HAlignment.Center)
                            {
                                //draw minor unit tic mark in center
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Left + ((negativeRegionBoundingRectangle.Width - minorUnitLineLengthInPixels) / 2),
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Left + ((negativeRegionBoundingRectangle.Width - minorUnitLineLengthInPixels) / 2) + minorUnitLineLengthInPixels,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)
                                );

                            }
                        }
                        else if (i % majorUnitInterval == 0) //this is a major unit
                        {
                            if (ticsAlignment == HAlignment.Right)
                            {
                                //draw major unit tick mark on right hand side
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Right - majorUnitLineLengthInPixels,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Right, negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Left)
                            {
                                //draw major unit tick mark on left hand side
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Left,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Left + majorUnitLineLengthInPixels, negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            else if (ticsAlignment == HAlignment.Center)
                            {
                                //draw major unit tick mark in center
                                g.DrawLine(negativeForegroundPen,
                                        negativeRegionBoundingRectangle.Left + ((negativeRegionBoundingRectangle.Width - majorUnitLineLengthInPixels) / 2),
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels),
                                        negativeRegionBoundingRectangle.Left + ((negativeRegionBoundingRectangle.Width - majorUnitLineLengthInPixels) / 2) + majorUnitLineLengthInPixels,
                                        negativeRegionBoundingRectangle.Top + ((Math.Abs(i) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels));
                            }
                            if (negativeUnitsLabelled) //if we're supposed to add text labels to negative units...
                            {
                                Rectangle majorUnitTextBoundingRectangle = Rectangle.Empty;
                                //draw major unit text
                                if (ticsAlignment == HAlignment.Right)
                                {
                                    //tics are on the right so draw text on the left
                                    majorUnitTextBoundingRectangle = new Rectangle(
                                        new Point(0,
                                            negativeRegionBoundingRectangle.Top + (
                                                ((Math.Abs(i) - 1) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                            )
                                        ),
                                        new Size(negativeRegionBoundingRectangle.Width - majorUnitLineLengthInPixels - textPaddingPixels, verticalSeparationBetweenTicksInPixels * 2)
                                    );
                                }
                                else if (ticsAlignment == HAlignment.Left)
                                {
                                    //tics are on the left so draw text to the right of them
                                    majorUnitTextBoundingRectangle = new Rectangle(
                                        new Point(majorUnitLineLengthInPixels + textPaddingPixels,
                                            negativeRegionBoundingRectangle.Top + (
                                                ((Math.Abs(i) - 1) / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                            )
                                        ),
                                        new Size(negativeRegionBoundingRectangle.Width - majorUnitLineLengthInPixels - textPaddingPixels, verticalSeparationBetweenTicksInPixels * 2)
                                    );
                                }
                                else if (ticsAlignment == HAlignment.Center)
                                {
                                    int lineLength = majorUnitLineLengthInPixels;
                                    if (majorUnitLineLengthInPixels == 0) lineLength = minorUnitLineLengthInPixels;
                                    //tic is in the center so draw text in the center
                                    majorUnitTextBoundingRectangle = new Rectangle(
                                        new Point(
                                            ((negativeRegionBoundingRectangle.Width - lineLength) / 2) - 15,
                                            negativeRegionBoundingRectangle.Top + (
                                                ((Math.Abs(i) - 1 / minorUnitInterval) * verticalSeparationBetweenTicksInPixels
                                            )
                                        )),
                                        new Size(negativeRegionBoundingRectangle.Width, verticalSeparationBetweenTicksInPixels * 2)
                                    );
                                }
                                //*****

                                string majorUnitString = String.Empty;
                                int majorUnitVal = i;
                                if (!negativeUnitsHaveNegativeSign && majorUnitVal < 0)
                                {
                                    majorUnitVal = Math.Abs(majorUnitVal);
                                }
                                if (Math.Abs(i).ToString().Length > 3) // num >= 1000
                                {
                                    majorUnitString = String.Format("{0:0000}", majorUnitVal);
                                    majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize - 2, majorUnitFont.Style);
                                }
                                else if (Math.Abs(i).ToString().Length > 2) // num >= 100
                                {
                                    majorUnitString = String.Format("{0:000}", majorUnitVal);
                                    majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                                }
                                else if (Math.Abs(i).ToString().Length > 1) // num >= 10
                                {
                                    majorUnitString = String.Format("{0:00}", majorUnitVal);
                                    majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                                }
                                else if (Math.Abs(i).ToString().Length == 1) // num between 1 and 10
                                {
                                    majorUnitString = String.Format("{0:0}", majorUnitVal);
                                    majorUnitFont = new Font(majorUnitFont.FontFamily.Name, baseFontSize, majorUnitFont.Style);
                                }
                                if (i == 0) majorUnitString = "0";
                                StringFormat majorUnitStringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip);
                                if (textAlignment == HAlignment.Right)
                                {
                                    majorUnitStringFormat.Alignment = StringAlignment.Far;
                                }
                                else if (textAlignment == HAlignment.Left)
                                {
                                    majorUnitStringFormat.Alignment = StringAlignment.Near;
                                }
                                else if (textAlignment == HAlignment.Center)
                                {
                                    majorUnitStringFormat.Alignment = StringAlignment.Center;
                                }
                                majorUnitStringFormat.LineAlignment = StringAlignment.Center;

                                g.TranslateTransform(0, 2);
                                g.DrawString(majorUnitString, majorUnitFont, negativeForegroundBrush, majorUnitTextBoundingRectangle, majorUnitStringFormat);
                                g.ResetTransform();
                                //*****
                            }
                        } //end else
                    } //end for         
                }
                if (coloringInstructions != null)
                {
                    foreach (TapeEdgeColoringInstruction instruction in coloringInstructions)
                    {
                        Color color = instruction.color;
                        Brush colorBrush = new SolidBrush(color);
                        Rectangle rect = Rectangle.Empty;
                        if (instruction.minVal > 0)
                        {
                            rect = new Rectangle(
                                new Point(
                                    positiveRegionBoundingRectangle.Right - 5,
                                    positiveRegionBoundingRectangle.Bottom - ((instruction.maxVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)
                                ),
                                new Size(
                                    5,
                                    (positiveRegionBoundingRectangle.Bottom - ((instruction.minVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)) -
                                    (positiveRegionBoundingRectangle.Bottom - ((instruction.maxVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels))
                                    )
                            );
                        }
                        else
                        {
                            rect = new Rectangle(
                                new Point(
                                    negativeRegionBoundingRectangle.Right - 5,
                                    (negativeRegionBoundingRectangle.Top + ((instruction.minVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels))
                                ),
                                new Size(
                                    5,
                                    (negativeRegionBoundingRectangle.Top + ((instruction.maxVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels)) -
                                    (negativeRegionBoundingRectangle.Top + ((instruction.minVal / minorUnitInterval) * verticalSeparationBetweenTicksInPixels))
                                    )
                            );
                        }
                        g.FillRectangle(colorBrush, rect);
                    }
                }
            }
            return toReturn;
        }
        private Bitmap GetSingleDigitBitmap(float digit, bool showExtraDigitsTopAndBottom)
        {

            if (showExtraDigitsTopAndBottom)
            {
                if (_tripleDigitBitmaps[(int)(digit * 10.0f)] != null)
                {
                    return _tripleDigitBitmaps[(int)(digit * 10.0f)];
                }
            }
            else
            {
                if (_singleDigitBitmaps[(int)(digit * 10.0f)] != null)
                {
                    return _singleDigitBitmaps[(int)(digit * 10.0f)];
                }
            }
            if (digit < 1 && digit >= 0) digit += 10;
            int digitWidth = 9;
            int digitHeight = 15;
            int digitHorizontalMargin = 4;
            int digitVerticalMargin = 4;
            Rectangle digitRectangle = new Rectangle(
                    new Point(0, 0),
                    new Size(
                        digitWidth + (digitHorizontalMargin * 2),
                        digitHeight + (digitVerticalMargin * 2)
                    )
            );

            Bitmap verticalNumberStrip = GetVerticalNumberStrip();

            int leftX = 0;
            int rightX = verticalNumberStrip.Width;
            int topY = (int)(verticalNumberStrip.Height - ((digitVerticalMargin * 2) + digitHeight) * (digit + 1));
            topY += digitVerticalMargin;
            int bottomY = topY + digitHeight + digitVerticalMargin;

            if (showExtraDigitsTopAndBottom)
            {
                int overRunArea = (int)((digitHeight + (digitVerticalMargin * 2)) * 0.6);
                topY -= (overRunArea + digitVerticalMargin);
                bottomY += overRunArea;
            }
            Bitmap toReturn = new Bitmap(rightX - leftX, bottomY - topY, PixelFormat.Format16bppRgb565);
            toReturn.MakeTransparent();
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle sourceRectangle = new Rectangle(new Point(leftX, topY), new Size(rightX - leftX, bottomY - topY));
                Rectangle destRectangle = Rectangle.Empty;
                if (showExtraDigitsTopAndBottom)
                {
                    destRectangle = new Rectangle(new Point(0, 0), new Size(verticalNumberStrip.Width, bottomY - topY));
                }
                else
                {
                    destRectangle = new Rectangle(new Point(0, digitVerticalMargin), new Size(verticalNumberStrip.Width, bottomY - topY));
                }
                g.DrawImage(verticalNumberStrip, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }

            if (showExtraDigitsTopAndBottom)
            {
                _tripleDigitBitmaps[(int)(digit * 10.0f)] = toReturn;
            }
            else
            {
                _singleDigitBitmaps[(int)(digit * 10.0f)] = toReturn;
            }

            return toReturn;
        }
        private Bitmap GetSingleDigitBitmap(int digit)
        {
            return GetSingleDigitBitmap(digit, false);
        }
        private Bitmap GetVerticalNumberStrip()
        {
            if (_verticalNumberStrip != null) return (Bitmap)_verticalNumberStrip;
            int digitWidth = 9;
            int digitHeight = 15;

            int digitHorizontalMargin = 4;
            int digitVerticalMargin = 4;

            Rectangle digitRectangle = new Rectangle(
                    new Point(0, 0),
                    new Size(
                        digitWidth + (digitHorizontalMargin * 2),
                        digitHeight + (digitVerticalMargin * 2)
                    )
            );
            Rectangle overallRectangle = new Rectangle(
                new Point(0, 0),
                new Size(
                        digitRectangle.Width,
                        (digitRectangle.Height * 12)
                    )
            );

            Bitmap toReturn = new Bitmap(overallRectangle.Width, overallRectangle.Height, PixelFormat.Format16bppRgb565);

            toReturn.MakeTransparent();
            using (Graphics g = Graphics.FromImage(toReturn))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush whiteBrush = new SolidBrush(Color.White);
                Font font = new Font("Lucida Console", 12, FontStyle.Bold);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                digitRectangle.Offset(0, digitVerticalMargin);

                for (int i = 11; i >= 0; i--)
                {
                    g.DrawString((i % 10).ToString(), font, whiteBrush, digitRectangle);
                    digitRectangle.Offset(0, digitRectangle.Height);
                }
                g.DrawString("9", font, whiteBrush, digitRectangle);
                digitRectangle.Offset(0, digitRectangle.Height);
            }
            _verticalNumberStrip = toReturn;
            return toReturn;
        }
        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~Pfd()
        {
            Dispose();
        }
        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(_climbDiveMarkerSymbol);
                    Common.Util.DisposeObject(_markerDiamond);
                    Common.Util.DisposeObject(_adiPitchBars);
                    Common.Util.DisposeObject(_airspeedTape);
                    Common.Util.DisposeObject(_aoaTape);
                    Common.Util.DisposeObject(_vviTape);
                    Common.Util.DisposeObject(_altitudeTapes);
                    Common.Util.DisposeObject(_verticalNumberStrip);
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;

        }
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
