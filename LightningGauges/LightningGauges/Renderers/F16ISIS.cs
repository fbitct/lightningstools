using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Common.SimSupport;
using Common.UI;
using Util = Common.Imaging.Util;

//TODO: add options to the options screen for setting pressure units, metric/standard velocity units, etc.
//TODO: test this instro with Falcon
//TODO: baro adjust?

namespace LightningGauges.Renderers
{
    public interface IF16ISIS : IInstrumentRenderer
    {
        F16ISIS.F16ISISOptions Options { get; set; }
        F16ISIS.F16ISISInstrumentState InstrumentState { get; set; }
    }

    public class F16ISIS : InstrumentRendererBase, IF16ISIS
    {
        #region Image Location Constants

        private const string ISIS_ILS_DOT_IMAGE_FILENAME = "isis_ilsdot.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly PrivateFontCollection _fonts = new PrivateFontCollection();
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;
        private static Bitmap _markerDiamond;

        #endregion

        public F16ISIS()
        {
            InstrumentState = new F16ISISInstrumentState();
            Options = new F16ISISOptions();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            _fonts.AddFontFile("ISISDigits.ttf");

            if (_markerDiamond == null)
            {
                using (
                    var markerDiamond =
                        (Bitmap)
                        Util.LoadBitmapFromFile(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar +
                                                ISIS_ILS_DOT_IMAGE_FILENAME))
                {
                    markerDiamond.MakeTransparent(Color.FromArgb(255, 0, 255));
                    _markerDiamond = (Bitmap) Util.ResizeBitmap(markerDiamond, new Size(15, 15));
                }
            }

            _imagesLoaded = true;
        }

        #endregion

        public F16ISISOptions Options { get; set; }
        public F16ISISInstrumentState InstrumentState { get; set; }

        public override void Render(Graphics g, Rectangle bounds)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            var gfx = g;
            Bitmap fullBright = null;
            if (InstrumentState.Brightness != InstrumentState.MaxBrightness)
            {
                fullBright = new Bitmap(bounds.Size.Width, bounds.Size.Height, PixelFormat.Format32bppPArgb);
                gfx = Graphics.FromImage(fullBright);
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = gfx.Save();
                //set up the canvas scale and clipping region
                var width = 256;
                var height = 256;
                gfx.ResetTransform(); //clear any existing transforms
                gfx.SetClip(bounds);
                //set the clipping region on the graphics object to our render rectangle's boundaries
                gfx.FillRectangle(Brushes.Black, bounds);
                gfx.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 
                //g.TranslateTransform(0, 0);
                gfx.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
                gfx.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
                gfx.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
                gfx.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = gfx.Save();


                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                gfx.TranslateTransform(-7, 0);
                DrawAttitude(gfx, width, height);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw heading tape
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                var headingTapeSize = new SizeF(width - 96, 25);
                gfx.TranslateTransform(40, height - headingTapeSize.Height);
                var heading = InstrumentState.MagneticHeadingDegrees;
                DrawHeadingTape(gfx, headingTapeSize, heading);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw heading triangle
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                float headingTriangleHeight = 15;
                float headingTriangleWidth = 10;
                var headingTriangleCenter = new PointF((width/2.0f) - 8.5f, height - headingTapeSize.Height);
                var headingTriangleLeft = new PointF((width/2.0f) - 8.5f - (headingTriangleWidth/2.0f),
                                                     height - headingTriangleHeight - headingTapeSize.Height);
                var headingTriangleRight = new PointF((width/2.0f) - 8.5f + (headingTriangleWidth/2.0f),
                                                      height - headingTriangleHeight - headingTapeSize.Height);
                gfx.FillPolygon(Brushes.White, new[] {headingTriangleCenter, headingTriangleLeft, headingTriangleRight});
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw airspeed tape
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                var airspeedKnots = InstrumentState.AirspeedKnots;
                float airspeedTapeWidth = 42;
                DrawAirspeedTape(gfx, new SizeF(airspeedTapeWidth, height), airspeedKnots);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw altitude tape
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                var altitudeMsl = InstrumentState.IndicatedAltitudeFeetMSL;
                float altitudeTapeWidth = 55;
                gfx.TranslateTransform(width - altitudeTapeWidth, 0);
                DrawAltitudeTape(gfx, new SizeF(altitudeTapeWidth, height), altitudeMsl);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw top rectangle
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                var topRectangle = new RectangleF(0, 0, width, 42);
                gfx.FillRectangle(Brushes.Black, topRectangle);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw mach rectangle
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                var machRectangle = new RectangleF(0, 12, 57, topRectangle.Height - 12);
                var machRectanglePen = new Pen(Color.White);
                machRectanglePen.Width = 1;
                gfx.DrawRectangle(machRectanglePen, (int) machRectangle.X, (int) machRectangle.Y,
                                  (int) machRectangle.Width, (int) machRectangle.Height);

                var machNumberStringFormat = new StringFormat();
                machNumberStringFormat.Alignment = StringAlignment.Near;
                machNumberStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                     StringFormatFlags.NoWrap;
                machNumberStringFormat.LineAlignment = StringAlignment.Center;
                machNumberStringFormat.Trimming = StringTrimming.None;
                var machNumber = InstrumentState.MachNumber;
                var machNumberString = string.Format("{0:0.00}", machNumber);
                var normalTransform = gfx.Transform;
                var machNumberRectangle = new RectangleF(machRectangle.X, machRectangle.Y - 2, machRectangle.Width,
                                                         machRectangle.Height);
                machNumberRectangle.Offset(0, -7.5f);
                gfx.ScaleTransform(1, 1.50f);
                gfx.DrawString(machNumberString, new Font(_fonts.Families[0], 15, FontStyle.Regular, GraphicsUnit.Point),
                               Brushes.White, machNumberRectangle, machNumberStringFormat);
                gfx.Transform = normalTransform;

                var machLetterStringFormat = new StringFormat();
                machLetterStringFormat.Alignment = StringAlignment.Far;
                machLetterStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                     StringFormatFlags.NoWrap;
                machLetterStringFormat.LineAlignment = StringAlignment.Center;
                machLetterStringFormat.Trimming = StringTrimming.None;
                machRectangle.Offset(3, -1);
                gfx.DrawString("M", new Font(_fonts.Families[0], 22, FontStyle.Regular, GraphicsUnit.Point),
                               Brushes.White, machRectangle, machLetterStringFormat);


                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw the radar altimeter area
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                float raltRectangleWidth = 80;
                var raltRectangle = new RectangleF((topRectangle.Width/2.0f) - (raltRectangleWidth/2.0f), 10,
                                                   raltRectangleWidth, topRectangle.Height - 10);
                var raltRectanglePen = Pens.White;
                raltRectangle.Offset(-5, 0);
                gfx.DrawRectangle(raltRectanglePen, raltRectangle.X, raltRectangle.Y, raltRectangle.Width,
                                  raltRectangle.Height);

                var raltStringFormat = new StringFormat();
                raltStringFormat.Alignment = StringAlignment.Far;
                raltStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                               StringFormatFlags.NoWrap;
                raltStringFormat.LineAlignment = StringAlignment.Center;
                raltStringFormat.Trimming = StringTrimming.None;
                var ralt = InstrumentState.RadarAltitudeAGL;
                var raltString = string.Format("{0:#####0}", ralt);
                var raltColor = Color.FromArgb(183, 243, 244);
                Brush raltBrush = new SolidBrush(raltColor);
                float fontSize = 20;

                if (
                    (!raltString.StartsWith("-") && raltString.Length > 4)
                    ||
                    (raltString.StartsWith("-") && raltString.Length > 5)
                    )
                {
                    fontSize = 18;
                }

                if (
                    (!raltString.StartsWith("-") && raltString.Length > 5)
                    ||
                    (raltString.StartsWith("-") && raltString.Length > 6)
                    )
                {
                    fontSize = 15;
                }
                if (Options.RadarAltitudeUnits == F16ISISOptions.AltitudeUnits.Meters)
                {
                    raltString += "m";
                }
                else
                {
                    raltString += "ft";
                }

                gfx.DrawString(raltString, new Font(_fonts.Families[0], fontSize, FontStyle.Regular, GraphicsUnit.Point),
                               raltBrush, raltRectangle, raltStringFormat);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //draw the barometric pressure area
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                float barometricPressureAreaWidth = 65;
                var barometricPressureStringFormat = new StringFormat();
                barometricPressureStringFormat.Alignment = StringAlignment.Far;
                barometricPressureStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                             StringFormatFlags.NoWrap;
                barometricPressureStringFormat.LineAlignment = StringAlignment.Far;
                barometricPressureStringFormat.Trimming = StringTrimming.None;

                var pressure = InstrumentState.BarometricPressure;

                var barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                                                                 20, barometricPressureAreaWidth,
                                                                 topRectangle.Height - 20);
                var barometricPressureBrush = Brushes.White;

                string baroString = null;
                string units = null;
                if (Options.PressureAltitudeUnits == F16ISISOptions.PressureUnits.InchesOfMercury)
                {
                    //baroString = string.Format("{0:#0.00}", pressure);
                    baroString = string.Format("{0:#0.00}", pressure / 100);
                    units = "in";
                    barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 15,
                                                                 20, barometricPressureAreaWidth,
                                                                 topRectangle.Height - 20);
                }
                else if (Options.PressureAltitudeUnits == F16ISISOptions.PressureUnits.Millibars)
                {
                    baroString = string.Format("{0:###0}", pressure);
                    units = "hPa";
                    barometricPressureRectangle = new RectangleF(topRectangle.Width - barometricPressureAreaWidth - 25,
                                                                 20, barometricPressureAreaWidth,
                                                                 topRectangle.Height - 20);
                }
                gfx.DrawString(baroString, new Font(_fonts.Families[0], 20, FontStyle.Regular, GraphicsUnit.Point),
                               barometricPressureBrush, barometricPressureRectangle, barometricPressureStringFormat);

                var unitsRectangle = new RectangleF(topRectangle.Width - 22, 18, 15, topRectangle.Height - 20);
                gfx.DrawString(units, new Font(_fonts.Families[0], 8, FontStyle.Regular, GraphicsUnit.Point),
                               barometricPressureBrush, unitsRectangle, barometricPressureStringFormat);

                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
                DrawIlsBars(gfx, width, height);
                GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);

                //restore the canvas's transform and clip settings to what they were when we entered this method
                gfx.Restore(initialState);
            }
            if (fullBright != null)
            {
                var ia = new ImageAttributes();
                var dimmingMatrix =
                    Util.GetDimmingColorMatrix(InstrumentState.Brightness/(float) InstrumentState.MaxBrightness);
                ia.SetColorMatrix(dimmingMatrix);
                g.DrawImage(fullBright, bounds, 0, 0, fullBright.Width, fullBright.Height, GraphicsUnit.Pixel, ia);
                Common.Util.DisposeObject(gfx);
                Common.Util.DisposeObject(fullBright);
            }
        }

        private void DrawIlsBars(Graphics g, int width, int height)
        {
            //prepare to draw glideslope and localizer stuff
            var distanceBetweenDots = 19.5f;
            float farLeftLocalizerMarkerX = 82;
            float farLeftLocalizerMarkerY = height - 55;
            var farLeftLocalizerMarkerCenterPoint = new PointF(farLeftLocalizerMarkerX, farLeftLocalizerMarkerY);
            var leftMiddleLocalizerMarkerCenterPoint =
                new PointF(farLeftLocalizerMarkerCenterPoint.X + distanceBetweenDots,
                           farLeftLocalizerMarkerCenterPoint.Y);
            var middleLocalizerMarkerCenterPoint =
                new PointF(farLeftLocalizerMarkerCenterPoint.X + (distanceBetweenDots*2),
                           farLeftLocalizerMarkerCenterPoint.Y);
            var rightMiddleLocalizerMarkerCenterPoint =
                new PointF(farLeftLocalizerMarkerCenterPoint.X + (distanceBetweenDots*3),
                           farLeftLocalizerMarkerCenterPoint.Y);
            var farRightLocalizerMarkerCenterPoint =
                new PointF(farLeftLocalizerMarkerCenterPoint.X + (distanceBetweenDots*4),
                           farLeftLocalizerMarkerCenterPoint.Y);

            float topLeftGlideslopeMarkerX = width - 76;
            float topLeftGlideslopeMarkerY = 89;
            var topGlideSlopeMarkerCenterPoint = new PointF(topLeftGlideslopeMarkerX, topLeftGlideslopeMarkerY);
            var upperMiddleGlideSlopeMarkerCenterPoint = new PointF(topGlideSlopeMarkerCenterPoint.X,
                                                                    topGlideSlopeMarkerCenterPoint.Y +
                                                                    distanceBetweenDots);
            var middleGlideSlopeMarkerCenterPoint = new PointF(topGlideSlopeMarkerCenterPoint.X,
                                                               topGlideSlopeMarkerCenterPoint.Y +
                                                               (distanceBetweenDots*2));
            var lowerMiddleGlideSlopeMarkerCenterPoint = new PointF(topGlideSlopeMarkerCenterPoint.X,
                                                                    topGlideSlopeMarkerCenterPoint.Y +
                                                                    (distanceBetweenDots*3));
            var bottomGlideSlopeMarkerCenterPoint = new PointF(topGlideSlopeMarkerCenterPoint.X,
                                                               topGlideSlopeMarkerCenterPoint.Y +
                                                               (distanceBetweenDots*4));


            //prepare draw localizer indicator line

            var minIlsHorizontalPositionVal = -InstrumentState.LocalizerDeviationLimitDegrees;
            var maxIlsHorizontalPositionVal = InstrumentState.LocalizerDeviationLimitDegrees;
            var IlsHorizontalPositionRange = maxIlsHorizontalPositionVal - minIlsHorizontalPositionVal;
            var currentIlsHorizontalPositionVal = InstrumentState.LocalizerDeviationDegrees +
                                                  Math.Abs(minIlsHorizontalPositionVal);
            if (currentIlsHorizontalPositionVal < 0) currentIlsHorizontalPositionVal = 0;
            if (currentIlsHorizontalPositionVal > IlsHorizontalPositionRange)
                currentIlsHorizontalPositionVal = IlsHorizontalPositionRange;

            var minIlsBarX = farLeftLocalizerMarkerCenterPoint.X;
            var maxIlsBarX = farRightLocalizerMarkerCenterPoint.X;
            float ilsBarXRange = (int) (maxIlsBarX - minIlsBarX) + 1;

            float currentIlsBarX =
                (int) (minIlsBarX + ((currentIlsHorizontalPositionVal/IlsHorizontalPositionRange)*ilsBarXRange));

            var ilsBarTop = new PointF(currentIlsBarX, topGlideSlopeMarkerCenterPoint.Y);
            var ilsBarBottom = new PointF(currentIlsBarX, bottomGlideSlopeMarkerCenterPoint.Y);

            var localizerBarPen = new Pen(Color.Yellow);
            localizerBarPen.Width = 3;
            var glideslopeBarPen = new Pen(Color.Yellow);
            glideslopeBarPen.Width = 3;
            if (InstrumentState.ShowCommandBars && !InstrumentState.LocalizerFlag && !InstrumentState.OffFlag)
            {
                if (InstrumentState.LocalizerFlag)
                {
                    localizerBarPen.DashStyle = DashStyle.Dash;
                    localizerBarPen.DashOffset = 3;
                }
                //draw localizer command bar
                g.DrawLine(localizerBarPen, ilsBarTop, ilsBarBottom);
                g.DrawImage(_markerDiamond, currentIlsBarX - (_markerDiamond.Width/2),
                            farLeftLocalizerMarkerCenterPoint.Y - (_markerDiamond.Width/2));
            }

            //prepare to draw glideslope bar
            var minIlsVerticalPositionVal = -InstrumentState.GlideslopeDeviationLimitDegrees;
            var maxIlsVerticalPositionVal = InstrumentState.GlideslopeDeviationLimitDegrees;
            var IlsVerticalPositionRange = maxIlsVerticalPositionVal - minIlsVerticalPositionVal;

            var currentIlsVerticalPositionVal = (-InstrumentState.GlideslopeDeviationDegrees) +
                                                Math.Abs(minIlsVerticalPositionVal);
            if (currentIlsVerticalPositionVal < 0) currentIlsVerticalPositionVal = 0;
            if (currentIlsVerticalPositionVal > IlsVerticalPositionRange)
                currentIlsVerticalPositionVal = IlsVerticalPositionRange;


            var minIlsBarY = topGlideSlopeMarkerCenterPoint.Y;
            var maxIlsBarY = bottomGlideSlopeMarkerCenterPoint.Y;
            float ilsBarYRange = (int) (maxIlsBarY - minIlsBarY) + 1;

            var currentIlsBarY =
                (int) (minIlsBarY + ((currentIlsVerticalPositionVal/IlsVerticalPositionRange)*ilsBarYRange));

            var ilsBarLeft = new PointF(farLeftLocalizerMarkerCenterPoint.X - 7, currentIlsBarY);
            var ilsBarRight = new PointF(farRightLocalizerMarkerCenterPoint.X + 7, currentIlsBarY);

            //draw glideslope bar
            if (InstrumentState.ShowCommandBars && !InstrumentState.GlideslopeFlag && !InstrumentState.OffFlag)
            {
                if (InstrumentState.GlideslopeFlag)
                {
                    glideslopeBarPen.DashStyle = DashStyle.Dash;
                    glideslopeBarPen.DashOffset = 3;
                }
                g.DrawLine(glideslopeBarPen, ilsBarLeft, ilsBarRight);
                g.DrawImage(_markerDiamond, topGlideSlopeMarkerCenterPoint.X - (_markerDiamond.Width/2),
                            currentIlsBarY - (_markerDiamond.Width/2));
            }

            DrawGlideslopeMarkers(g, topGlideSlopeMarkerCenterPoint, upperMiddleGlideSlopeMarkerCenterPoint,
                                  middleGlideSlopeMarkerCenterPoint, lowerMiddleGlideSlopeMarkerCenterPoint,
                                  bottomGlideSlopeMarkerCenterPoint);
            DrawLocalizerMarkers(g, farLeftLocalizerMarkerCenterPoint, leftMiddleLocalizerMarkerCenterPoint,
                                 middleLocalizerMarkerCenterPoint, farRightLocalizerMarkerCenterPoint,
                                 rightMiddleLocalizerMarkerCenterPoint);
        }

        private void DrawGlideslopeMarkers(Graphics g, PointF topGlideSlopeMarkerCenterPoint,
                                           PointF upperMiddleGlideSlopeMarkerCenterPoint,
                                           PointF middleGlideSlopeMarkerCenterPoint,
                                           PointF lowerMiddleGlideSlopeMarkerCenterPoint,
                                           PointF bottomGlideSlopeMarkerCenterPoint)
        {
            //draw glideslope markers
            if (InstrumentState.ShowCommandBars && !InstrumentState.GlideslopeFlag && !InstrumentState.OffFlag)
            {
                var blackPen = new Pen(Color.Black);
                var path = new GraphicsPath();
                path.AddEllipse(new RectangleF(topGlideSlopeMarkerCenterPoint.X - 3,
                                               topGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(topGlideSlopeMarkerCenterPoint.X - 5,
                                               topGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new RectangleF(upperMiddleGlideSlopeMarkerCenterPoint.X - 3,
                                               upperMiddleGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(upperMiddleGlideSlopeMarkerCenterPoint.X - 5,
                                               upperMiddleGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddRectangle(
                    new RectangleF(
                        new PointF(
                            middleGlideSlopeMarkerCenterPoint.X - 6,
                            middleGlideSlopeMarkerCenterPoint.Y - 1), new Size(12, 2)));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);


                path.Reset();
                path.AddEllipse(new RectangleF(lowerMiddleGlideSlopeMarkerCenterPoint.X - 3,
                                               lowerMiddleGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(lowerMiddleGlideSlopeMarkerCenterPoint.X - 5,
                                               lowerMiddleGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new RectangleF(bottomGlideSlopeMarkerCenterPoint.X - 3,
                                               bottomGlideSlopeMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(bottomGlideSlopeMarkerCenterPoint.X - 5,
                                               bottomGlideSlopeMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);
            }
        }

        private void DrawLocalizerMarkers(Graphics g, PointF farLeftLocalizerMarkerCenterPoint,
                                          PointF leftMiddleLocalizerMarkerCenterPoint,
                                          PointF middleLocalizerMarkerCenterPoint,
                                          PointF farRightLocalizerMarkerCenterPoint,
                                          PointF rightMiddleLocalizerMarkerCenterPoint)
        {
            //draw localizer markers
            if (InstrumentState.ShowCommandBars && !InstrumentState.LocalizerFlag && !InstrumentState.OffFlag)
            {
                var blackPen = new Pen(Color.Black);

                var path = new GraphicsPath();
                path.AddEllipse(new RectangleF(farLeftLocalizerMarkerCenterPoint.X - 3,
                                               farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(farLeftLocalizerMarkerCenterPoint.X - 5,
                                               farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new RectangleF(leftMiddleLocalizerMarkerCenterPoint.X - 3,
                                               farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(leftMiddleLocalizerMarkerCenterPoint.X - 5,
                                               farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddRectangle(
                    new RectangleF(
                        new PointF(middleLocalizerMarkerCenterPoint.X - 1, middleLocalizerMarkerCenterPoint.Y - 6),
                        new SizeF(2, 12)));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new RectangleF(rightMiddleLocalizerMarkerCenterPoint.X - 3,
                                               farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(rightMiddleLocalizerMarkerCenterPoint.X - 5,
                                               farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);

                path.Reset();
                path.AddEllipse(new RectangleF(farRightLocalizerMarkerCenterPoint.X - 3,
                                               farLeftLocalizerMarkerCenterPoint.Y - 3, 6, 6));
                path.AddEllipse(new RectangleF(farRightLocalizerMarkerCenterPoint.X - 5,
                                               farLeftLocalizerMarkerCenterPoint.Y - 5, 10, 10));
                g.DrawPath(blackPen, path);
                g.FillPath(Brushes.White, path);
            }
        }

        private void DrawAttitude(Graphics g, int width, int height)
        {
            var basicState = g.Save();
            //draw the pitch ladder
            float pitchLadderHeight = width*4;
            float pitchLadderWidth = height*4;
            var pixelsPerDegreePitch = pitchLadderHeight/(180.0f + 90);
            var translateX = (width/2.0f) - (pitchLadderWidth/2.0f);
            var pitchDegrees = InstrumentState.PitchDegrees;
            var rollDegrees = InstrumentState.RollDegrees;
            var translateY = ((pixelsPerDegreePitch*pitchDegrees) - (pitchLadderHeight/2.0f)) + (height/2.0f);
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            g.TranslateTransform(width/2.0f, (height)/2.0f);
            g.RotateTransform(-rollDegrees);
            g.TranslateTransform(-(float) width/2.0f, -(height)/2.0f);
            g.TranslateTransform(translateX, translateY);
            DrawPitchLadder(g, new RectangleF(0, 0, pitchLadderWidth, pitchLadderHeight), pitchDegrees);
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            //draw the fixed airplane symbol
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float airplaneSymbolBarThickness = 6;
            float airplaneSymbolWidthEdgeToEdge = 80;
            var centerX = width/2.0f;
            var centerY = height/2.0f;
            var sidebarSpaceFromCenter = 10.0f;

            var lhsTopLeftX = centerX - (airplaneSymbolWidthEdgeToEdge/2.0f) - sidebarSpaceFromCenter;
            var lhsTopLeftY = centerY - (airplaneSymbolBarThickness/2.0f);
            var lhsTopRightX = centerX - sidebarSpaceFromCenter;
            var lhsTopRightY = lhsTopLeftY;
            var lhsBottomRightX = lhsTopRightX;
            var lhsBottomRightY = lhsTopRightY + (airplaneSymbolBarThickness*2);
            var lhsBottomCenterX = lhsBottomRightX - airplaneSymbolBarThickness;
            var lhsBottomCenterY = lhsBottomRightY;
            var lhsMiddleCenterX = lhsBottomCenterX;
            var lhsMiddleCenterY = lhsBottomCenterY - airplaneSymbolBarThickness;
            var lhsMiddleLeftX = lhsTopLeftX;
            var lhsMiddleLeftY = lhsMiddleCenterY;

            var rhsTopLeftX = centerX + sidebarSpaceFromCenter;
            var rhsTopLeftY = centerY - (airplaneSymbolBarThickness/2.0f);
            var rhsTopRightX = centerX + sidebarSpaceFromCenter + (airplaneSymbolWidthEdgeToEdge/2.0f);
            var rhsTopRightY = rhsTopLeftY;
            var rhsMiddleRightX = rhsTopRightX;
            var rhsMiddleRightY = rhsTopRightY + airplaneSymbolBarThickness;
            var rhsMiddleCenterX = rhsTopLeftX + airplaneSymbolBarThickness;
            var rhsMiddleCenterY = rhsMiddleRightY;
            var rhsBottomCenterX = rhsMiddleCenterX;
            var rhsBottomCenterY = rhsTopLeftY + (airplaneSymbolBarThickness*2);
            var rhsBottomLeftX = rhsTopLeftX;
            var rhsBottomLeftY = rhsBottomCenterY;


            var airplaneSymbolOutlineColor = Color.White;
            var airplaneSymbolOutlinePen = new Pen(airplaneSymbolOutlineColor);
            airplaneSymbolOutlinePen.Width = 2;
            var airplaneSymbolInsideBrush = Brushes.Black;
            var airplaneSymbolLhsPoints = new[]
                                              {
                                                  new PointF(lhsTopLeftX, lhsTopLeftY), //LHS top-left
                                                  new PointF(lhsTopRightX, lhsTopRightY), //LHS top-right
                                                  new PointF(lhsBottomRightX, lhsBottomRightY), //LHS bottom-right
                                                  new PointF(lhsBottomCenterX, lhsBottomCenterY), //LHS bottom-center
                                                  new PointF(lhsMiddleCenterX, lhsMiddleCenterY), //LHS middle-center
                                                  new PointF(lhsMiddleLeftX, lhsMiddleLeftY) //LHS middle-left
                                              };
            g.FillPolygon(airplaneSymbolInsideBrush, airplaneSymbolLhsPoints);
            g.DrawPolygon(airplaneSymbolOutlinePen, airplaneSymbolLhsPoints);

            var airplaneSymbolRhsPoints = new[]
                                              {
                                                  new PointF(rhsTopLeftX, rhsTopLeftY), //rhs top-left
                                                  new PointF(rhsTopRightX, rhsTopRightY), //rhs top-right
                                                  new PointF(rhsMiddleRightX, rhsMiddleRightY), //rhs middle-right
                                                  new PointF(rhsMiddleCenterX, rhsMiddleCenterY), //rhs middle-center
                                                  new PointF(rhsBottomCenterX, rhsBottomCenterY), //rhs bottom-center
                                                  new PointF(rhsBottomLeftX, rhsBottomLeftY) //rhs bottom-right
                                              };

            g.FillPolygon(airplaneSymbolInsideBrush, airplaneSymbolRhsPoints);
            g.DrawPolygon(airplaneSymbolOutlinePen, airplaneSymbolRhsPoints);

            var airplaneSymbolCenterRectangle = new RectangleF(centerX - (airplaneSymbolBarThickness/2.0f),
                                                               centerY - (airplaneSymbolBarThickness/2.0f),
                                                               airplaneSymbolBarThickness, airplaneSymbolBarThickness);
            g.FillRectangle(airplaneSymbolInsideBrush, airplaneSymbolCenterRectangle);
            g.DrawRectangle(airplaneSymbolOutlinePen,
                            new Rectangle((int) airplaneSymbolCenterRectangle.X, (int) airplaneSymbolCenterRectangle.Y,
                                          (int) airplaneSymbolCenterRectangle.Width,
                                          (int) airplaneSymbolCenterRectangle.Height));

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            //draw the roll angle index marks
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float majorIndexLineLength = 15;
            var minorIndexLineLength = majorIndexLineLength/2.0f;
            var radiusFromCenterToBottomOfIndexLine = pixelsPerDegreePitch*20.0f;
            var startingTransform = g.Transform;
            var rollIndexPen = new Pen(Color.White);
            rollIndexPen.Width = 2;
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

            //draw zero degree triangle
            var rollTriangleWidthAtBase = centerY - radiusFromCenterToBottomOfIndexLine;
            var center = new PointF(centerX, centerY - radiusFromCenterToBottomOfIndexLine);
            var topLeft = new PointF(centerX - (rollTriangleWidthAtBase/8.0f), center.Y - 6);
            var topRight = new PointF(centerX + (rollTriangleWidthAtBase/8.0f), center.Y - 6);
            if (Math.Abs(rollDegrees) < 0.5f)
            {
                g.FillPolygon(Brushes.White, new[] {topLeft, topRight, center});
            }
            else
            {
                g.DrawPolygon(Pens.White, new[] {topLeft, topRight, center});
            }
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            //draw sky pointer triangle
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            g.TranslateTransform(width/2.0f, (height)/2.0f);
            g.RotateTransform(-rollDegrees);
            g.TranslateTransform(-(float) width/2.0f, -(height)/2.0f);
            float triangleWidth = 15;
            float triangleHeight = 10;
            var bottomLeft = new PointF((width/2.0f) - (triangleWidth/2.0f),
                                        centerY - radiusFromCenterToBottomOfIndexLine + triangleHeight);
            var topCenter = new PointF((width/2.0f), centerY - radiusFromCenterToBottomOfIndexLine);
            var bottomRight = new PointF((width/2.0f) + (triangleWidth/2.0f),
                                         centerY - radiusFromCenterToBottomOfIndexLine + triangleHeight);
            rollIndexPen.Width = 4;
            g.FillPolygon(Brushes.White, new[] {bottomLeft, topCenter, bottomRight});
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
        }

        private void DrawHeadingTape(Graphics g, SizeF size, float magneticHeadingDegrees)
        {
            var headingDigitFont = new Font(_fonts.Families[0], 16, FontStyle.Bold, GraphicsUnit.Point);
            var headingDigitFormat = new StringFormat();
            headingDigitFormat.Alignment = StringAlignment.Far;
            headingDigitFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            headingDigitFormat.LineAlignment = StringAlignment.Center;
            headingDigitFormat.Trimming = StringTrimming.None;
            var headingDigitColor = Color.White;
            Brush headingDigitBrush = new SolidBrush(headingDigitColor);
            var headingDigitPen = new Pen(headingDigitColor);
            headingDigitPen.Width = 2;
            var headingTapeWidth = size.Width;
            var headingTapeHeight = size.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;


            //draw the background
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, (int) size.Width, (int) size.Height));
            var pixelsPerDegree = (size.Width/30.0f);
            g.TranslateTransform(-pixelsPerDegree*magneticHeadingDegrees, 0);
            for (var i = -30; i <= 400; i += 5)
            {
                float angle = i;
                if (i < 0) angle = 360 + i;
                if (Math.Abs(angle)%10 == 0)
                {
                    var toDisplay = string.Format("{0:000}", Math.Abs(angle)%360);

                    var toDisplaySize = g.MeasureString(toDisplay, headingDigitFont);
                    var x = (i*pixelsPerDegree) + (size.Width/2.0f) - (toDisplaySize.Width/2.0f);
                    var y = (size.Height/2.0f) - (toDisplaySize.Height/2.0f);
                    var layoutRect = new RectangleF(x, y, toDisplaySize.Width, toDisplaySize.Height);
                    g.DrawString(toDisplay, headingDigitFont, headingDigitBrush, layoutRect, headingDigitFormat);

                    //draw point above text
                    {
                        var xprime = (i*pixelsPerDegree) + (size.Width/2.0f);
                        float yprime = 2;
                        float yprimeprime = 4;
                        g.DrawLine(headingDigitPen, new PointF(xprime, yprime), new PointF(xprime, yprimeprime));
                    }
                }
                else if (Math.Abs(i)%5 == 0)
                {
                    //draw point indicating 5 degree mark
                    var x = (i*pixelsPerDegree) + (size.Width/2.0f);
                    float y = 2;
                    float yPrime = 3;
                    g.DrawLine(headingDigitPen, new PointF(x, y), new PointF(x, yPrime));
                }
            }
        }

        private void DrawAltitudeTape(Graphics g, SizeF size, float altitudeFeetMSL)
        {
            var absAltitudeFeetMSL = Math.Abs(altitudeFeetMSL);
            var originalTransform = g.Transform;
            var altitudeDigitFontSmall = new Font(_fonts.Families[0], 16, FontStyle.Regular, GraphicsUnit.Point);
            var altitudeDigitFontLarge = new Font(_fonts.Families[0], 22, FontStyle.Regular, GraphicsUnit.Point);
            var altitudeDigitFormat = new StringFormat();
            altitudeDigitFormat.Alignment = StringAlignment.Far;
            altitudeDigitFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            altitudeDigitFormat.LineAlignment = StringAlignment.Center;
            altitudeDigitFormat.Trimming = StringTrimming.None;
            var altitudeDigitColor = Color.White;
            Brush altitudeDigitBrush = new SolidBrush(altitudeDigitColor);
            var altitudeDigitPen = new Pen(altitudeDigitColor);
            altitudeDigitPen.Width = 2;
            var altitudeTapeWidth = size.Width;
            var altitudeTapeHeight = size.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;

            var backgroundColor = Color.FromArgb(117, 123, 121);
            Brush backgroundBrush = new SolidBrush(backgroundColor);


            //draw the background
            g.FillRectangle(backgroundBrush, new Rectangle(0, 0, (int) size.Width, (int) size.Height));
            g.DrawLine(altitudeDigitPen, new PointF(0, 43), new PointF(size.Width, 43));
            g.DrawLine(altitudeDigitPen, new PointF(0, 0), new PointF(0, size.Height));
            var pixelsPerFoot = (size.Height/2.0f)/550.0f;
            g.TranslateTransform(0, pixelsPerFoot*altitudeFeetMSL);
            for (var i = -10000; i < 100000; i += 100)
            {
                if (Math.Abs(i) < (absAltitudeFeetMSL - 550) || Math.Abs(i) > (absAltitudeFeetMSL + 550)) continue;
                if (Math.Abs(i)%200 == 0)
                {
                    var altitudeString = string.Format("{0:00000}", Math.Abs(i));
                    var hundredsString = altitudeString.Substring(2, 3);
                    var thousandsString = altitudeString.Substring(0, 2);
                    while (thousandsString.StartsWith("0"))
                    {
                        thousandsString = thousandsString.Substring(1, thousandsString.Length - 1);
                    }
                    if (i < 0) thousandsString = "-" + thousandsString;

                    var hundredsDisplaySize = g.MeasureString(hundredsString, altitudeDigitFontSmall, size,
                                                              altitudeDigitFormat);
                    float offsetBoth = 20;
                    float offsetHundreds = 2;
                    float offsetThousands = -6;
                    var x = offsetBoth + offsetHundreds;
                    var y = (-i*pixelsPerFoot) - (hundredsDisplaySize.Height/2.0f) + (size.Height/2.0f);
                    var layoutRect = new RectangleF(x, y, hundredsDisplaySize.Width, hundredsDisplaySize.Height);
                    g.DrawString(hundredsString, altitudeDigitFontSmall, altitudeDigitBrush, layoutRect,
                                 altitudeDigitFormat);

                    var thousandsDisplaySize = g.MeasureString(thousandsString, altitudeDigitFontLarge, size,
                                                               altitudeDigitFormat);
                    y = (-i*pixelsPerFoot) - (thousandsDisplaySize.Height/2.0f) + (size.Height/2.0f);
                    var layoutRect2 =
                        new RectangleF(
                            size.Width - hundredsDisplaySize.Width - thousandsDisplaySize.Width + offsetBoth +
                            offsetThousands, y, thousandsDisplaySize.Width, thousandsDisplaySize.Height);
                    g.DrawString(thousandsString, altitudeDigitFontLarge, altitudeDigitBrush, layoutRect2,
                                 altitudeDigitFormat);
                }
                else if (Math.Abs(i)%100 == 0)
                {
                    var lineWidth = 15;
                    var y = (-i*pixelsPerFoot) + (size.Height/2.0f);
                    g.DrawLine(altitudeDigitPen, new PointF(0, y), new PointF(lineWidth, y));
                }
            }


            g.Transform = originalTransform;
            //calculate digits
            float tenThousands = (int) Math.Floor((absAltitudeFeetMSL/10000.0f)%10);
            float thousands = (int) Math.Floor((absAltitudeFeetMSL/1000.0f)%10);
            float hundreds = (int) Math.Floor(absAltitudeFeetMSL/100.0f)%10;
            var twenties = (absAltitudeFeetMSL/20.0f)%5;
            if (twenties > 4) hundreds += (twenties - 4);
            if (hundreds > 9) thousands += (hundreds - 9);
            if (thousands > 9) tenThousands += (thousands - 9);

            float altitudeBoxHeight = 35;
            var altitudeBoxWidth = size.Width + 5;
            float altitudeDigitFontSize = 22;
            float altitudeDigitFontSizeSmall = 16;
            var outerRectangle = new RectangleF(
                -5,
                (size.Height/2.0f) - (altitudeBoxHeight/2.0f),
                altitudeBoxWidth,
                altitudeBoxHeight
                );
            g.FillRectangle(Brushes.Black, outerRectangle);
            g.DrawRectangle(altitudeDigitPen, (int) outerRectangle.X, (int) outerRectangle.Y, (int) outerRectangle.Width,
                            (int) outerRectangle.Height);

            var twentiesRectangle = new RectangleF(
                altitudeBoxWidth - ((altitudeBoxWidth/6.0f)*2),
                (size.Height/2.0f) - (altitudeBoxHeight/2.0f),
                ((altitudeBoxWidth/6.0f)*2),
                altitudeBoxHeight
                );
            twentiesRectangle.Offset(outerRectangle.X, 4);

            DrawAltitudeDigits(g, twenties, twentiesRectangle, outerRectangle, altitudeDigitFontSizeSmall, true, true,
                               StringAlignment.Center);

            var hundredsRectangle = new RectangleF(
                altitudeBoxWidth - ((altitudeBoxWidth/6.0f)*3),
                (size.Height/2.0f) - (altitudeBoxHeight/2.0f),
                (altitudeBoxWidth/6.0f),
                altitudeBoxHeight
                );
            hundredsRectangle.Offset(outerRectangle.X - 1, 4);

            DrawAltitudeDigits(g, hundreds, hundredsRectangle, outerRectangle, altitudeDigitFontSizeSmall, false, true,
                               StringAlignment.Center);

            var thousandsRectangle = new RectangleF(
                altitudeBoxWidth - ((altitudeBoxWidth/6.0f)*4.5f),
                (size.Height/2.0f) - (altitudeBoxHeight/2.0f),
                (altitudeBoxWidth/6.0f)*1.5f,
                altitudeBoxHeight
                );
            thousandsRectangle.Offset(outerRectangle.X - 5, 0);
            DrawAltitudeDigits(g, thousands, thousandsRectangle, outerRectangle, altitudeDigitFontSize, false, true,
                               StringAlignment.Near);

            var tenThousandsRectangle = new RectangleF(
                altitudeBoxWidth - ((altitudeBoxWidth/6.0f)*6),
                (size.Height/2.0f) - (altitudeBoxHeight/2.0f),
                (altitudeBoxWidth/6.0f)*1.5f,
                altitudeBoxHeight
                );
            tenThousandsRectangle.Offset(outerRectangle.X - 4, 0);
            if (tenThousands > 0)
            {
                DrawAltitudeDigits(g, tenThousands, tenThousandsRectangle, outerRectangle, altitudeDigitFontSize, false,
                                   true, StringAlignment.Near);
            }
            if (altitudeFeetMSL < 0)
            {
                DrawAltitudeDigits(g, -1, tenThousandsRectangle, outerRectangle, altitudeDigitFontSize, false, false,
                                   StringAlignment.Near);
            }
        }

        private void DrawAirspeedTape(Graphics g, SizeF size, float airspeedKnots)
        {
            var airspeedDigitFont = new Font(_fonts.Families[0], 22, FontStyle.Regular, GraphicsUnit.Point);
            var airspeedDigitFormat = new StringFormat();
            airspeedDigitFormat.Alignment = StringAlignment.Far;
            airspeedDigitFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            airspeedDigitFormat.LineAlignment = StringAlignment.Center;
            airspeedDigitFormat.Trimming = StringTrimming.None;
            var airspeedDigitColor = Color.White;
            Brush airspeedDigitBrush = new SolidBrush(airspeedDigitColor);
            var airspeedDigitPen = new Pen(airspeedDigitColor);
            airspeedDigitPen.Width = 2;
            var airspeedTapeWidth = size.Width;
            var airspeedTapeHeight = size.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;

            var backgroundColor = Color.FromArgb(117, 123, 121);
            Brush backgroundBrush = new SolidBrush(backgroundColor);


            //draw the background
            g.FillRectangle(backgroundBrush, new Rectangle(0, 0, (int) size.Width, (int) size.Height));
            g.DrawLine(airspeedDigitPen, new PointF(size.Width, 0), new PointF(size.Width, size.Height));

            var originalTransform = g.Transform;
            var pixelsPerKnot = (size.Height/2.0f)/70.0f;
            g.TranslateTransform(0, pixelsPerKnot*airspeedKnots);
            for (var i = 0; i < 2000; i += 10)
            {
                if (i < (airspeedKnots - 100) || i > (airspeedKnots + 50)) continue;
                if (i%20 == 0)
                {
                    var toDisplay = string.Format("{0:####0}", i);
                    var toDisplaySize = g.MeasureString(toDisplay, airspeedDigitFont);
                    float x = 3;
                    var y = (-i*pixelsPerKnot) - (toDisplaySize.Height/2.0f) + (size.Height/2.0f);
                    var layoutRect = new RectangleF(x, y, size.Width, toDisplaySize.Height);
                    g.DrawString(toDisplay, airspeedDigitFont, airspeedDigitBrush, layoutRect, airspeedDigitFormat);
                }
                else if (i%10 == 0)
                {
                    var lineWidth = 15;
                    var x = size.Width - lineWidth;
                    var y = (-i*pixelsPerKnot) + (size.Height/2.0f);
                    g.DrawLine(airspeedDigitPen, new PointF(x, y), new PointF(size.Width, y));
                }
            }
            g.Transform = originalTransform;
            //calculate digits
            float thousands = (int) Math.Floor((airspeedKnots/1000.0f)%10);
            float hundreds = (int) Math.Floor((airspeedKnots/100.0f)%10);
            float tens = (int) Math.Floor((airspeedKnots/10.0f)%10);
            var ones = (airspeedKnots%10);

            if (ones > 9) tens += (ones - 9);
            if (tens > 9) hundreds += (tens - 9);
            if (hundreds > 9) thousands += (hundreds - 9);

            float airspeedBoxHeight = 35;
            var airspeedBoxWidth = size.Width + 5;
            float airspeedDigitFontSize = 22;
            var outerRectangle = new RectangleF(
                0,
                (size.Height/2.0f) - (airspeedBoxHeight/2.0f),
                airspeedBoxWidth,
                airspeedBoxHeight
                );
            g.FillRectangle(Brushes.Black, outerRectangle);
            g.DrawRectangle(airspeedDigitPen, (int) outerRectangle.X, (int) outerRectangle.Y, (int) outerRectangle.Width,
                            (int) outerRectangle.Height);
            var onesRectangle = new RectangleF(
                airspeedBoxWidth - (airspeedBoxWidth/3.0f),
                (size.Height/2.0f) - (airspeedBoxHeight/2.0f),
                (airspeedBoxWidth/3.0f),
                airspeedBoxHeight
                );
            onesRectangle.Offset(outerRectangle.X, -10);

            DrawAirspeedDigits(g, ones, onesRectangle, outerRectangle, airspeedDigitFontSize, true);

            var tensRectangle = new RectangleF(
                airspeedBoxWidth - ((airspeedBoxWidth/3.0f)*2),
                (size.Height/2.0f) - (airspeedBoxHeight/2.0f),
                (airspeedBoxWidth/3.0f),
                airspeedBoxHeight
                );
            tensRectangle.Offset(outerRectangle.X, -10);
            if (airspeedKnots >= 10)
            {
                DrawAirspeedDigits(g, tens, tensRectangle, outerRectangle, airspeedDigitFontSize, true);
            }

            var hundredsRectangle = new RectangleF(
                airspeedBoxWidth - ((airspeedBoxWidth/3.0f)*3),
                (size.Height/2.0f) - (airspeedBoxHeight/2.0f),
                (airspeedBoxWidth/3.0f),
                airspeedBoxHeight
                );
            hundredsRectangle.Offset(outerRectangle.X + 1, -10);
            if (airspeedKnots >= 100)
            {
                DrawAirspeedDigits(g, hundreds, hundredsRectangle, outerRectangle, airspeedDigitFontSize, true);
            }
        }

        private void DrawAltitudeDigits(Graphics g, float digit, RectangleF layoutRectangle, RectangleF clipRectangle,
                                        float pointSize, bool goByTwenty, bool cyclical, StringAlignment alignment)
        {
            var digitFont = new Font(_fonts.Families[0], pointSize, FontStyle.Regular, GraphicsUnit.Point);
            var digitSF = new StringFormat();
            digitSF.Alignment = alignment;
            digitSF.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            digitSF.LineAlignment = StringAlignment.Center;
            digitSF.Trimming = StringTrimming.None;

            var digitBrush = Brushes.White;
            var initialClip = g.Clip;
            var initialState = g.Save();

            g.SetClip(clipRectangle);
            var basicState = g.Save();
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            float digitSpacing = 0;
            if (cyclical)
            {
                for (var i = -1; i <= 11; i++)
                {
                    var thisDigit = 0;
                    if (i >= 0)
                    {
                        thisDigit = i%10;
                    }
                    else
                    {
                        thisDigit = (10 - (Math.Abs(i)%10));
                    }

                    var toDraw = string.Format("{0:#0}", thisDigit);
                    if (goByTwenty)
                    {
                        toDraw = string.Format("{0:00}", (thisDigit*20)%100);
                        if (toDraw == "100") toDraw = "00";
                    }
                    var digitSize = g.MeasureString(toDraw, digitFont);
                    var layoutRectangle2 = new RectangleF(
                        layoutRectangle.X,
                        layoutRectangle.Y - (i*(digitSize.Height + digitSpacing)),
                        layoutRectangle.Width, digitSize.Height
                        );
                    layoutRectangle2.Offset(0, ((digitSize.Height + digitSpacing)*digit));
                    g.DrawString(toDraw, digitFont, digitBrush, layoutRectangle2, digitSF);
                }
            }
            else
            {
                var thisDigit = (int) Math.Floor(digit);
                var toDraw = string.Format("{0:0}", thisDigit);
                if (toDraw.Length > 1) toDraw = toDraw.Substring(0, 1);
                if (goByTwenty)
                {
                    toDraw = string.Format("{0:00}", thisDigit*20);
                }
                var digitSize = g.MeasureString(toDraw, digitFont);
                var layoutRectangle2 = new RectangleF(
                    layoutRectangle.X,
                    layoutRectangle.Y - (digit*(digitSize.Height + digitSpacing)),
                    layoutRectangle.Width, digitSize.Height
                    );
                layoutRectangle2.Offset(0, ((digitSize.Height + digitSpacing)*digit));
                g.DrawString(toDraw, digitFont, digitBrush, layoutRectangle2, digitSF);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
            g.Clip = initialClip;
        }

        private void DrawAirspeedDigits(Graphics g, float digit, RectangleF layoutRectangle, RectangleF clipRectangle,
                                        float pointSize, bool cyclical)
        {
            var digitFont = new Font(_fonts.Families[0], pointSize, FontStyle.Regular, GraphicsUnit.Point);
            var digitSF = new StringFormat();
            digitSF.Alignment = StringAlignment.Center;
            digitSF.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
            digitSF.LineAlignment = StringAlignment.Center;
            digitSF.Trimming = StringTrimming.None;

            var digitBrush = Brushes.White;
            var initialClip = g.Clip;
            var initialState = g.Save();

            g.SetClip(clipRectangle);
            var basicState = g.Save();
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);

            float digitSpacing = 0;
            float start = -1;
            float end = 11;
            if (!cyclical)
            {
                start = digit;
                end = digit;
            }
            for (var i = start; i <= end; i++)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                float thisDigit = 0;
                if (i >= 0)
                {
                    thisDigit = i%10;
                }
                else
                {
                    thisDigit = (10 - (Math.Abs(i)%10));
                }

                var toDraw = string.Format("{0:0}", thisDigit);
                var digitSize = g.MeasureString(toDraw, digitFont);
                g.TranslateTransform(0,
                                     ((digitSize.Height + digitSpacing)*digit) + (layoutRectangle.Height/4.0f) +
                                     (digitSpacing/2.0f));
                var layoutRectangle2 = new RectangleF(
                    layoutRectangle.X,
                    layoutRectangle.Y - (i*(digitSize.Height + digitSpacing)),
                    layoutRectangle.Width, digitSize.Height
                    );
                g.DrawString(toDraw, digitFont, digitBrush, layoutRectangle2, digitSF);
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
            g.Clip = initialClip;
        }

        private void DrawPitchLadder(Graphics g, RectangleF bounds, float pitchDegrees)
        {
            var pitchDigitFont = new Font(_fonts.Families[0], 12, FontStyle.Bold, GraphicsUnit.Point);
            var pitchDigitFormat = new StringFormat();
            pitchDigitFormat.Alignment = StringAlignment.Center;
            pitchDigitFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            pitchDigitFormat.LineAlignment = StringAlignment.Center;
            pitchDigitFormat.Trimming = StringTrimming.None;


            var pitchLadderWidth = bounds.Width;
            var pitchLadderHeight = bounds.Height;
            var startingTransform = g.Transform;
            var startingClip = g.Clip;

            var minorLineWidth = 15;
            var groundColor = Color.FromArgb(111, 72, 31);
            Brush groundBrush = new SolidBrush(groundColor);
            var skyColor = Color.FromArgb(3, 174, 252);
            Brush skyBrush = new SolidBrush(skyColor);
            var pitchBarColor = Color.White;
            var pitchDigitColor = Color.White;
            Brush pitchDigitBrush = new SolidBrush(pitchDigitColor);
            var pitchBarPen = new Pen(pitchBarColor);
            var pitchBarPenWidth = 2;
            pitchBarPen.Width = pitchBarPenWidth;
            var pixelsPerDegree = pitchLadderHeight/(180.0f + 90);

            //draw the ground
            g.FillRectangle(groundBrush,
                            new RectangleF(0, pitchLadderHeight/2.0f, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the sky
            g.FillRectangle(skyBrush, new RectangleF(0, 0, pitchLadderWidth, pitchLadderHeight/2.0f));

            //draw the horizon line
            g.DrawLine(pitchBarPen, new PointF(0, (pitchLadderHeight/2.0f)),
                       new PointF(pitchLadderWidth, (pitchLadderHeight/2.0f)));

            //draw zenith/nadir symbol
            float zenithNadirSymbolWidth = minorLineWidth;
            {
                var y = (pitchLadderHeight/2.0f) - (90*pixelsPerDegree);
                var zenithOrNadirRectangle = new RectangleF((pitchLadderWidth/2.0f) - (zenithNadirSymbolWidth/2.0f),
                                                            y - (zenithNadirSymbolWidth/2.0f), zenithNadirSymbolWidth,
                                                            zenithNadirSymbolWidth);
                g.DrawEllipse(pitchBarPen, zenithOrNadirRectangle);
            }
            {
                var y = (pitchLadderHeight/2.0f) - (-90*pixelsPerDegree);
                var zenithOrNadirRectangle = new RectangleF((pitchLadderWidth/2.0f) - (zenithNadirSymbolWidth/2.0f),
                                                            y - (zenithNadirSymbolWidth/2.0f), zenithNadirSymbolWidth,
                                                            zenithNadirSymbolWidth);
                g.DrawEllipse(pitchBarPen, zenithOrNadirRectangle);
                g.DrawLine(pitchBarPen, new PointF(zenithOrNadirRectangle.Left, zenithOrNadirRectangle.Top),
                           new PointF(zenithOrNadirRectangle.Right, zenithOrNadirRectangle.Bottom));
                g.DrawLine(pitchBarPen, new PointF(zenithOrNadirRectangle.Left, zenithOrNadirRectangle.Bottom),
                           new PointF(zenithOrNadirRectangle.Right, zenithOrNadirRectangle.Top));
            }
            //draw the climb pitch bars
            for (float i = -85; i <= 85; i += 2.5f)
            {
                if (i < (pitchDegrees - 15) || i > (pitchDegrees + 15.0f)) continue;
                float lineWidth = minorLineWidth;
                if (i%5 == 0) lineWidth *= 2;
                if (i%10 == 0) lineWidth *= 2;
                if (i == 0) lineWidth = pitchLadderWidth;
                var y = (pitchLadderHeight/2.0f) - (i*pixelsPerDegree);

                //draw this line
                g.DrawLine(pitchBarPen, new PointF((pitchLadderWidth/2.0f) - (lineWidth/2.0f), y),
                           new PointF((pitchLadderWidth/2.0f) + (lineWidth/2.0f), y));

                //draw the pitch digits
                if (i%10 == 0)
                {
                    var toDisplayVal = (int) Math.Abs(i);
                    var toDisplayString = string.Format("{0:##}", toDisplayVal);

                    var digitSize = g.MeasureString(toDisplayString, pitchDigitFont);
                    var lhsRect = new RectangleF(
                        new PointF((pitchLadderWidth/2.0f) - (lineWidth/2.0f) - digitSize.Width,
                                   y - (digitSize.Height/2.0f)),
                        digitSize);
                    g.DrawString(toDisplayString, pitchDigitFont, pitchDigitBrush, lhsRect, pitchDigitFormat);

                    var rhsRect = new RectangleF(
                        new PointF((pitchLadderWidth/2.0f) + (lineWidth/2.0f), y - (digitSize.Height/2.0f)),
                        digitSize);
                    g.DrawString(toDisplayString, pitchDigitFont, pitchDigitBrush, rhsRect, pitchDigitFormat);
                }
            }

            g.Transform = startingTransform;
            g.Clip = startingClip;
        }

        #region Options Class

        public class F16ISISOptions
        {
            #region AltitudeUnits enum

            public enum AltitudeUnits
            {
                Feet,
                Meters
            }

            #endregion

            #region PressureUnits enum

            public enum PressureUnits
            {
                InchesOfMercury,
                Millibars
            }

            #endregion

            public F16ISISOptions()
            {
                PressureAltitudeUnits = PressureUnits.InchesOfMercury;
                RadarAltitudeUnits = AltitudeUnits.Feet;
            }

            public AltitudeUnits RadarAltitudeUnits { get; set; }
            public PressureUnits PressureAltitudeUnits { get; set; }
            public GdiPlusOptions GDIPlusOptions { get; set; }
        }

        #endregion

        #region Instrument State

        [Serializable]
        public class F16ISISInstrumentState : InstrumentStateBase
        {
            private const int MAX_BRIGHTNESS = 255;
            private const float MAX_VELOCITY = 100000;
            private const float MIN_VELOCITY = -100000;
            private const float MIN_PITCH = -90;
            private const float MAX_PITCH = 90;
            private const float MIN_ROLL = -180;
            private const float MAX_ROLL = 180;
            private const float MAX_MACH = 3.0F;
            private const float MAX_AIRSPEED = 3000.0F;
            private const float DEFAULT_AIRSPEED_INDEX_KNOTS = 250;
            private const float DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES = 1.0F;
            private const float DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES = 5.0F;
            private float _airspeedKnots;
            private float _barometricPressure = 2992f;
            private int _brightness = MAX_BRIGHTNESS;
            private float _glideslopeDeviationDegrees;
            private float _glideslopeDeviationLimitDegrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
            private float _localizerDeviationDegrees;
            private float _localizerDeviationLimitDegrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;
            private float _machNumber;
            private float _magneticHeadingDegrees;
            private float _neverExceedSpeedKnots;
            private float _pitchDegrees;
            private float _rollDegrees;
            private float _verticalVelocityFeetPerMinute;

            public F16ISISInstrumentState()
            {
                PitchDegrees = 0;
                RollDegrees = 0;
                AirspeedKnots = 0;
                MachNumber = 0;
                NeverExceedSpeedKnots = 0;
                IndicatedAltitudeFeetMSL = 0;
                VerticalVelocityFeetPerMinute = 0;
                GlideslopeDeviationLimitDegrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
                GlideslopeDeviationDegrees = 0;
                LocalizerDeviationLimitDegrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;
                LocalizerDeviationDegrees = 0;
                GlideslopeFlag = false;
                LocalizerFlag = false;
                ShowCommandBars = false;
                AuxFlag = false;
                OffFlag = false;
            }

            public bool OffFlag { get; set; }
            public bool AuxFlag { get; set; }
            public bool GlideslopeFlag { get; set; }
            public bool LocalizerFlag { get; set; }
            public bool ShowCommandBars { get; set; }

            public float BarometricPressure
            {
                get { return _barometricPressure; }
                set { _barometricPressure = value; }
            }

            public float VerticalVelocityFeetPerMinute
            {
                get { return _verticalVelocityFeetPerMinute; }
                set
                {
                    var vv = value;
                    if (vv < MIN_VELOCITY) vv = MIN_VELOCITY;
                    if (vv > MAX_VELOCITY) vv = MAX_VELOCITY;
                    _verticalVelocityFeetPerMinute = vv;
                }
            }

            public float MagneticHeadingDegrees
            {
                get { return _magneticHeadingDegrees; }
                set
                {
                    var heading = value;
                    heading %= 360.0f;
                    _magneticHeadingDegrees = heading;
                }
            }

            public float IndicatedAltitudeFeetMSL { get; set; }
            public float RadarAltitudeAGL { get; set; }

            public float AirspeedKnots
            {
                get { return _airspeedKnots; }
                set
                {
                    var knots = value;
                    if (knots < 0) knots = 0;
                    if (knots > MAX_AIRSPEED) knots = MAX_AIRSPEED;
                    _airspeedKnots = knots;
                }
            }

            public float MachNumber
            {
                get { return _machNumber; }
                set
                {
                    var mach = value;
                    if (mach < 0) mach = 0;
                    if (mach > MAX_MACH) mach = MAX_MACH;
                    _machNumber = mach;
                }
            }

            public float NeverExceedSpeedKnots
            {
                get { return _neverExceedSpeedKnots; }
                set
                {
                    var vne = value;
                    if (vne < 0) vne = 0;
                    if (vne > MAX_AIRSPEED) vne = MAX_AIRSPEED;
                    _neverExceedSpeedKnots = vne;
                }
            }

            public float PitchDegrees
            {
                get { return _pitchDegrees; }
                set
                {
                    var pitch = value;
                    if (pitch < MIN_PITCH) pitch = MIN_PITCH;
                    if (pitch > MAX_PITCH) pitch = MAX_PITCH;
                    if (float.IsNaN(pitch) || float.IsInfinity(pitch))
                    {
                        pitch = 0;
                    }
                    _pitchDegrees = pitch;
                }
            }

            public float RollDegrees
            {
                get { return _rollDegrees; }
                set
                {
                    var roll = value;
                    if (roll < MIN_ROLL) roll = MIN_ROLL;
                    if (roll > MAX_ROLL) roll = MAX_ROLL;
                    if (float.IsInfinity(roll) || float.IsNaN(roll))
                    {
                        roll = 0;
                    }
                    _rollDegrees = roll;
                }
            }

            public int Brightness
            {
                get { return _brightness; }
                set
                {
                    var brightness = value;
                    if (brightness < 0) brightness = 0;
                    if (brightness > MAX_BRIGHTNESS) brightness = MAX_BRIGHTNESS;
                    _brightness = brightness;
                }
            }

            public int MaxBrightness
            {
                get { return MAX_BRIGHTNESS; }
            }

            public float LocalizerDeviationDegrees
            {
                get { return _localizerDeviationDegrees; }
                set
                {
                    var degrees = value;
                    degrees %= 360.0f;
                    if (float.IsNaN(degrees) || float.IsInfinity(degrees))
                    {
                        degrees = 0;
                    }
                    _localizerDeviationDegrees = degrees;
                }
            }

            public float LocalizerDeviationLimitDegrees
            {
                get { return _localizerDeviationLimitDegrees; }
                set
                {
                    var degrees = value;
                    degrees %= 360.0f;
                    if (float.IsInfinity(degrees) || float.IsNaN(degrees) || degrees == 0)
                    {
                        degrees = DEFAULT_LOCALIZER_DEVIATION_LIMIT_DEGREES;
                    }
                    _localizerDeviationLimitDegrees = degrees;
                }
            }

            public float GlideslopeDeviationLimitDegrees
            {
                get { return _glideslopeDeviationLimitDegrees; }
                set
                {
                    var degrees = value;
                    degrees %= 360.0f;
                    if (float.IsNaN(degrees) || float.IsInfinity(degrees) || degrees == 0)
                    {
                        degrees = DEFAULT_GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
                    }
                    _glideslopeDeviationLimitDegrees = degrees;
                }
            }

            public float GlideslopeDeviationDegrees
            {
                get { return _glideslopeDeviationDegrees; }
                set
                {
                    var degrees = value;
                    degrees %= 360.0f;
                    if (float.IsInfinity(degrees) || float.IsNaN(degrees))
                    {
                        degrees = 0;
                    }
                    _glideslopeDeviationDegrees = degrees;
                }
            }
        }

        #endregion
    }
}