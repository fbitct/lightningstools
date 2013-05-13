using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;
using Common.UI;
using log4net;
using Util = Common.Imaging.Util;

namespace LightningGauges.Renderers
{
    public interface IF16EHSI : IInstrumentRenderer
    {
        F16EHSI.F16EHSIInstrumentState InstrumentState { get; set; }
        F16EHSI.F16EHSIOptions Options { get; set; }
    }

    public class F16EHSI : InstrumentRendererBase, IDisposable, IF16EHSI
    {
        #region Image Location Constants

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private const string EHSI_NO_DATA_IMAGE_FILENAME = "ehsioff.bmp";
        private const string EHSI_NO_DATA_MASK_FILENAME = "ehsioff_mask.bmp";
        private static readonly ILog _log = LogManager.GetLogger(typeof (F16EHSI));
        private static readonly object _imagesLock = new object();
        private static bool _imagesLoaded;
        private static ImageMaskPair _noData;
        private readonly PrivateFontCollection _fonts = new PrivateFontCollection();

        #endregion

        public F16EHSI()
        {
            _fonts.AddFontFile("isisdigits.ttf");
            _fonts.AddFontFile("ehsidigits.ttf");
            InstrumentState = new F16EHSIInstrumentState();
            Options = new F16EHSIOptions();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_noData == null)
            {
                _noData = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + EHSI_NO_DATA_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + EHSI_NO_DATA_MASK_FILENAME
                    );
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16EHSIInstrumentState InstrumentState { get; set; }
        public F16EHSIOptions Options { get; set; }

        #region Options Class

        public class F16EHSIOptions
        {
            public GDIPlusOptions GDIPlusOptions { get; set; }
        }

        #endregion

        #region Instrument State

        [Serializable]
        public class F16EHSIInstrumentState : InstrumentStateBase
        {
            #region InstrumentModes enum

            public enum InstrumentModes
            {
                Unknown,
                PlsTacan,
                Tacan,
                Nav,
                PlsNav
            }

            #endregion

            private const float DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES = 5.0F;
            private const float MAX_RANGE = 999.9F;
            private const int MAX_BRIGHTNESS = 255;
            private float _bearingToBeaconDegrees;
            private int _brightness = MAX_BRIGHTNESS;
            private float _courseDeviationDegrees;
            private float _courseDeviationLimitDegrees = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
            private int _desiredCourseDegrees;
            private int _desiredHeadingDegrees;
            private float _distanceToBeaconNauticalMiles;
            private InstrumentModes _instrumentMode = InstrumentModes.Unknown;
            private float _magneticHeadingDegrees;
            private DateTime _whenInstrumentModeLastChanged = DateTime.Now;

            public F16EHSIInstrumentState()
            {
                MagneticHeadingDegrees = 0.0f;
                BearingToBeaconDegrees = 0.0f;
                DeviationInvalidFlag = false;
                CourseDeviationDegrees = 0.0f;
                CourseDeviationLimitDegrees = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
                DesiredHeadingDegrees = 0;
                DesiredCourseDegrees = 0;
                DistanceToBeaconNauticalMiles = 0.0f;
                ToFlag = false;
                FromFlag = false;
                ShowToFromFlag = true;
                NoDataFlag = false;
            }

            public bool NoDataFlag { get; set; }
            public bool ShowToFromFlag { get; set; }
            public bool ToFlag { get; set; }
            public bool FromFlag { get; set; }

            public float DistanceToBeaconNauticalMiles
            {
                get { return _distanceToBeaconNauticalMiles; }
                set
                {
                    var distance = value;
                    if (distance < 0) distance = 0;
                    if (distance > MAX_RANGE) distance = MAX_RANGE;
                    if (float.IsNaN(distance) || float.IsNegativeInfinity(distance))
                    {
                        distance = 0;
                    }
                    if (float.IsPositiveInfinity(distance))
                    {
                        distance = MAX_RANGE;
                    }
                    _distanceToBeaconNauticalMiles = distance;
                }
            }

            public bool DmeInvalidFlag { get; set; }

            public int DesiredCourseDegrees
            {
                get { return _desiredCourseDegrees; }
                set
                {
                    var desiredCourse = value;
                    if (desiredCourse > 360) desiredCourse %= 360;
                    _desiredCourseDegrees = desiredCourse;
                }
            }

            public int DesiredHeadingDegrees
            {
                get { return _desiredHeadingDegrees; }
                set
                {
                    var desiredHeading = value;
                    desiredHeading %= 360;
                    _desiredHeadingDegrees = desiredHeading;
                }
            }

            public float CourseDeviationDegrees
            {
                get { return _courseDeviationDegrees; }
                set
                {
                    var courseDeviation = value;
                    courseDeviation %= 360.0f;
                    if (float.IsInfinity(courseDeviation) || float.IsNaN(courseDeviation))
                    {
                        courseDeviation = 0;
                    }
                    _courseDeviationDegrees = courseDeviation;
                }
            }

            public float CourseDeviationLimitDegrees
            {
                get { return _courseDeviationLimitDegrees; }
                set
                {
                    var courseDeviationLimit = value;
                    courseDeviationLimit %= 360.0f;
                    if (float.IsInfinity(courseDeviationLimit) || float.IsNaN(courseDeviationLimit) ||
                        courseDeviationLimit == 0)
                    {
                        courseDeviationLimit = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
                    }
                    _courseDeviationLimitDegrees = courseDeviationLimit;
                }
            }

            public float MagneticHeadingDegrees
            {
                get { return _magneticHeadingDegrees; }
                set
                {
                    var heading = value;
                    heading %= 360.0f;
                    if (float.IsNaN(heading) || float.IsInfinity(heading))
                    {
                        heading = 0;
                    }
                    _magneticHeadingDegrees = heading;
                }
            }

            public float BearingToBeaconDegrees
            {
                get { return _bearingToBeaconDegrees; }
                set
                {
                    var bearingToBeacon = value;
                    bearingToBeacon %= 360.0f;
                    if (float.IsInfinity(bearingToBeacon) || float.IsNaN(bearingToBeacon))
                    {
                        bearingToBeacon = 0;
                    }
                    _bearingToBeaconDegrees = bearingToBeacon;
                }
            }

            public bool DeviationInvalidFlag { get; set; }
            public bool INUInvalidFlag { get; set; }
            public bool AttitudeFailureFlag { get; set; }

            public InstrumentModes InstrumentMode
            {
                get { return _instrumentMode; }
                set
                {
                    var currentMode = _instrumentMode;
                    if (currentMode != value)
                    {
                        _instrumentMode = value;
                        _whenInstrumentModeLastChanged = DateTime.Now;
                    }
                }
            }

            public DateTime WhenInstrumentModeLastChanged
            {
                get { return _whenInstrumentModeLastChanged; }
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

            public bool ShowBrightnessLabel { get; set; }
        }

        #endregion

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
                var width = 512;
                var height = 512;

                var scaleWidth = bounds.Width;
                var scaleHeight = bounds.Height;

                if (scaleHeight > scaleWidth) scaleHeight = scaleWidth;
                if (scaleWidth > scaleHeight) scaleWidth = scaleHeight;


                gfx.ResetTransform(); //clear any existing transforms
                gfx.SetClip(bounds);
                //set the clipping region on the graphics object to our render rectangle's boundaries
                gfx.FillRectangle(Brushes.Black, bounds);

                float translateX = 0;
                float translateY = 0;
                if (scaleWidth < bounds.Width)
                {
                    translateX = (bounds.Width - scaleWidth)/2.0f;
                }
                if (scaleHeight < bounds.Height)
                {
                    translateY = (bounds.Height - scaleHeight)/2.0f;
                }
                gfx.TranslateTransform(translateX, translateY);
                gfx.ScaleTransform(scaleWidth/(float) width, scaleHeight/(float) height);
                //set the initial scale transformation 

                gfx.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
                gfx.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
                gfx.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
                gfx.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;

                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = gfx.Save();

                var outerRect = new RectangleF(0, 0, width, height);

                if (InstrumentState.NoDataFlag)
                {
                    DrawNoDataFlag(gfx, outerRect);
                }
                else
                {
                    //draw the compass
                    DrawCompassRose(gfx, outerRect);

                    //draw the desired heading bug
                    DrawDesiredHeadingBug(gfx, outerRect);

                    //draw course deviation needles
                    DrawCourseDeviationNeedles(gfx, outerRect);

                    //draw distance to beacon digits
                    DrawDistanceToBeacon(gfx, outerRect);

                    //draw desired course digits
                    DrawDesiredCourse(gfx, outerRect);

                    //draw heading and course select knob labels
                    DrawHeadingAndCourseAdjustLabels(gfx, outerRect);

                    //draw instrument mode label
                    DrawInstrumentMode(gfx, outerRect);

                    //draw bearing to beacon indicator
                    DrawBearingToBeaconIndicator(gfx, outerRect);

                    //draw INU invalid flag if needed
                    if (InstrumentState.INUInvalidFlag)
                    {
                        var redFlagColor = Color.FromArgb(224, 43, 48);
                        DrawTextWarningFlag(gfx, outerRect, "I\nN\nU", redFlagColor, Color.White);
                    }

                    //draw ATT invalid flag
                    if (InstrumentState.AttitudeFailureFlag)
                    {
                        var yellowFlagColor = Color.FromArgb(244, 240, 55);
                        DrawTextWarningFlag(gfx, outerRect, "A\nT\nT", yellowFlagColor, Color.Black);
                    }

                    //draw airplane symbol
                    DrawAirplaneSymbol(gfx, outerRect);
                }
                if (InstrumentState.ShowBrightnessLabel)
                {
                    DrawBrightnessLabel(gfx, outerRect);
                }

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

        private void DrawBrightnessLabel(Graphics g, RectangleF outerBounds)
        {
            DrawCenterLabel(g, outerBounds, "BRT");
        }

        private void DrawCenterLabel(Graphics g, RectangleF outerBounds, string label)
        {
            var fontFamily = _fonts.Families[0];
            var labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            var labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                            StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;
            var charSize = g.MeasureString("A", labelFont);
            var labelRect = new RectangleF(
                new PointF(
                    0,
                    (((outerBounds.Y + outerBounds.Height) - charSize.Height)/2.0f)
                    ),
                new SizeF(
                    outerBounds.Width,
                    charSize.Height
                    )
                );
            labelRect.Offset(0, -40);

            g.DrawString(label, labelFont, Brushes.White, labelRect, labelStringFormat);
        }

        private void DrawAirplaneSymbol(Graphics g, RectangleF outerBounds)
        {
            var middle = new PointF(outerBounds.X + (outerBounds.Width/2.0f), outerBounds.Y + (outerBounds.Height/2.0f));
            float airplaneFuselageLength = 45;
            float airplaneTailWidth = 23;
            float airplaneWingWidth = 45;
            var gap = airplaneFuselageLength/2.0f;

            var symbolPen = new Pen(Color.White);
            symbolPen.Width = 3;

            //draw wings
            var wingLeft = new PointF(middle.X - (airplaneWingWidth/2.0f), middle.Y - (gap/2.0f));
            var wingRight = new PointF(middle.X + (airplaneWingWidth/2.0f), middle.Y - (gap/2.0f));
            g.DrawLine(symbolPen, wingLeft, wingRight);

            //draw tail
            var tailLeft = new PointF(middle.X - (airplaneTailWidth/2.0f), middle.Y + (gap/2.0f));
            var tailRight = new PointF(middle.X + (airplaneTailWidth/2.0f), middle.Y + (gap/2.0f));
            g.DrawLine(symbolPen, tailLeft, tailRight);

            //draw fuselage
            var fuselageTop = new PointF(middle.X, middle.Y - (airplaneFuselageLength/2.0f));
            var fuselageBottom = new PointF(middle.X, middle.Y + (airplaneFuselageLength/2.0f));
            g.DrawLine(symbolPen, fuselageTop, fuselageBottom);
        }

        private void DrawBearingToBeaconIndicator(Graphics g, RectangleF outerBounds)
        {
            var needleColor = Color.FromArgb(102, 190, 157);
            Brush needleBrush = new SolidBrush(needleColor);
            var needlePen = new Pen(needleColor);
            needlePen.Width = 4;
            var basicState = g.Save();

            g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
            g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(InstrumentState.BearingToBeaconDegrees);
            g.TranslateTransform(-outerBounds.Width/2.0f, -outerBounds.Height/2.0f);


            g.TranslateTransform(outerBounds.Width/2.0f, 0);

            float bearingTriangleWidth = 23;
            float bearingTriangleHeight = 25;

            var bearingTriangleTop = new PointF(0, 5);
            var bearingTriangleLeft = new PointF(-bearingTriangleWidth/2.0f,
                                                 bearingTriangleTop.Y + bearingTriangleHeight);
            var bearingTriangleRight = new PointF(bearingTriangleWidth/2.0f,
                                                  bearingTriangleTop.Y + bearingTriangleHeight);
            g.FillPolygon(needleBrush, new[] {bearingTriangleTop, bearingTriangleLeft, bearingTriangleRight});

            float bearingLineTopHeight = 23;
            var bearingLineTopTop = new PointF(bearingTriangleTop.X, bearingTriangleLeft.Y);
            var bearingLineTopBottom = new PointF(bearingLineTopTop.X, bearingLineTopTop.Y + bearingLineTopHeight);
            g.DrawLine(needlePen, bearingLineTopTop, bearingLineTopBottom);

            float bearingLineBottomHeight = 37;
            var bearingLineBottomTop = new PointF(bearingTriangleTop.X, 455);
            var bearingLineBottomBottom = new PointF(bearingTriangleTop.X,
                                                     bearingLineBottomTop.Y + bearingLineBottomHeight);
            g.DrawLine(needlePen, bearingLineBottomTop, bearingLineBottomBottom);


            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
        }

        private void DrawTextWarningFlag(Graphics g, RectangleF outerBounds, string flagText, Color flagColor,
                                         Color textColor)
        {
            Brush flagBrush = new SolidBrush(flagColor);
            var fontFamily = _fonts.Families[0];
            var inuFlagFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            var inuFlagStringFormat = new StringFormat();
            inuFlagStringFormat.Alignment = StringAlignment.Center;
            inuFlagStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                              StringFormatFlags.NoWrap;
            inuFlagStringFormat.LineAlignment = StringAlignment.Center;
            inuFlagStringFormat.Trimming = StringTrimming.None;

            var flagLocation = new PointF(12, 75);
            var flagSize = new SizeF(25, 75);
            var flagRect = new RectangleF(flagLocation, flagSize);
            g.FillRectangle(flagBrush, flagRect);
            Brush textBrush = new SolidBrush(textColor);
            g.DrawString(flagText, inuFlagFont, textBrush, flagRect, inuFlagStringFormat);
        }

        private void DrawCourseDeviationNeedles(Graphics g, RectangleF outerBounds)
        {
            var initialState = g.Save();
            var redFlagColor = Color.FromArgb(224, 43, 48);
            var needleColor = Color.FromArgb(102, 190, 157);
            Brush redFlagBrush = new SolidBrush(redFlagColor);
            Brush needleBrush = new SolidBrush(needleColor);
            var needlePen = new Pen(needleColor);

            float pointerNeedleThickWidth = 8;
            float pointerNeedleThinWidth = 3;
            float maxDeviationX = 110;
            float dotHeight = 15;
            var dotY = (outerBounds.Height - dotHeight)/2.0f;
            var leftInnerDotX = (-maxDeviationX/2.0f) - (dotHeight/2.0f);
            var leftOuterDotX = -maxDeviationX - (dotHeight/2.0f);
            var rightInnerDotX = (maxDeviationX/2.0f) - (dotHeight/2.0f);
            var rightOuterDotX = maxDeviationX - (dotHeight/2.0f);
            float topIndicatorLineHeight = 50;
            float coursePointerTriangleWidth = 30;
            float coursePointerTriangleHeight = 20;
            float pointerNeedleThickHeightTop = 40;
            float cdiNeedleHeight = 198;
            var deviationTranslateX = maxDeviationX*Math.Sign(InstrumentState.CourseDeviationDegrees)*
                                      (Math.Abs(InstrumentState.CourseDeviationDegrees)/
                                       Math.Abs(InstrumentState.CourseDeviationLimitDegrees));
            float pointerNeedleThickHeightBottom = 60;
            float bottomIndicatorLineHeight = 50;
            var courseNeedleTopLineTop = new PointF(0, 43);
            var courseNeedleTopLineBottom = new PointF(0, courseNeedleTopLineTop.Y + topIndicatorLineHeight);
            var coursePointerTriangleLeft = new PointF(courseNeedleTopLineBottom.X - (coursePointerTriangleWidth/2.0f),
                                                       courseNeedleTopLineBottom.Y + coursePointerTriangleHeight);
            var coursePointerTriangleRight = new PointF(
                courseNeedleTopLineBottom.X + (coursePointerTriangleWidth/2.0f), coursePointerTriangleLeft.Y);
            var coursePointerTriangleTop = new PointF(0, courseNeedleTopLineBottom.Y - 2);
            var pointerNeedleThickTopTop = new PointF(0, coursePointerTriangleLeft.Y);
            var pointerNeedleThickTopBottom = new PointF(0, pointerNeedleThickTopTop.Y + pointerNeedleThickHeightTop);
            var deviationInvalidFlagRect = new RectangleF(new PointF(-80, pointerNeedleThickTopBottom.Y),
                                                          new SizeF(60, 30));
            var toFromFlagHeight = deviationInvalidFlagRect.Height;
            var toFromFlagWidth = deviationInvalidFlagRect.Height;
            var cdiLineTop = new PointF(0, pointerNeedleThickTopBottom.Y + 2);
            var cdiLineBottom = new PointF(0, cdiLineTop.Y + cdiNeedleHeight);
            var pointerNeedleThickBottomTop = new PointF(0, cdiLineBottom.Y + 2);
            var pointerNeedleThickBottomBottom = new PointF(0,
                                                            pointerNeedleThickBottomTop.Y +
                                                            pointerNeedleThickHeightBottom);
            var courseNeedleBottomLineTop = new PointF(0, pointerNeedleThickBottomBottom.Y);
            var courseNeedleBottomLineBottom = new PointF(0, courseNeedleBottomLineTop.Y + bottomIndicatorLineHeight);
            var toFlagTop = new PointF((maxDeviationX/2.0f), pointerNeedleThickTopBottom.Y);
            var toFlagLeft = new PointF(toFlagTop.X - (toFromFlagWidth/2.0f), toFlagTop.Y + toFromFlagHeight);
            var toFlagRight = new PointF(toFlagTop.X + (toFromFlagWidth/2.0f), toFlagTop.Y + toFromFlagHeight);
            var fromFlagBottom = new PointF((maxDeviationX/2.0f), pointerNeedleThickBottomTop.Y);
            var fromFlagLeft = new PointF(fromFlagBottom.X - (toFromFlagWidth/2.0f), fromFlagBottom.Y - toFromFlagHeight);
            var fromFlagRight = new PointF(fromFlagBottom.X + (toFromFlagWidth/2.0f),
                                           fromFlagBottom.Y - toFromFlagHeight);

            g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
            g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(InstrumentState.DesiredCourseDegrees);
            g.TranslateTransform(-outerBounds.Width/2.0f, -outerBounds.Height/2.0f);

            g.TranslateTransform(outerBounds.Width/2.0f, 0);

            //draw deviation dots
            g.FillEllipse(Brushes.White, new RectangleF(leftInnerDotX, dotY, dotHeight, dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(leftOuterDotX, dotY, dotHeight, dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(rightInnerDotX, dotY, dotHeight, dotHeight));
            g.FillEllipse(Brushes.White, new RectangleF(rightOuterDotX, dotY, dotHeight, dotHeight));

            //draw thin line on top of pointer arrow
            needlePen.Width = pointerNeedleThinWidth;
            g.DrawLine(needlePen, courseNeedleTopLineTop, courseNeedleTopLineBottom);

            //draw pointer arrow
            g.FillPolygon(needleBrush,
                          new[] {coursePointerTriangleTop, coursePointerTriangleLeft, coursePointerTriangleRight});

            //draw thick line just below pointer arrow
            needlePen.Width = pointerNeedleThickWidth;
            g.DrawLine(needlePen, pointerNeedleThickTopTop, pointerNeedleThickTopBottom);


            if (InstrumentState.DeviationInvalidFlag)
            {
                //draw deviation invalid flag
                g.FillRectangle(redFlagBrush, deviationInvalidFlagRect);
            }
            //draw CDI needle
            needlePen.Width = pointerNeedleThickWidth;
            if (InstrumentState.DeviationInvalidFlag)
            {
                needlePen.DashPattern = new[] {2, 1.75f};
            }
            g.TranslateTransform(deviationTranslateX, 0);
            g.DrawLine(needlePen, cdiLineTop, cdiLineBottom);
            g.TranslateTransform(-deviationTranslateX, 0);

            needlePen.DashStyle = DashStyle.Solid;

            //draw thick line just below CDI needle  
            needlePen.Width = pointerNeedleThickWidth;
            g.DrawLine(needlePen, pointerNeedleThickBottomTop, pointerNeedleThickBottomBottom);

            //draw thin line indicating reciprocal-of-course
            needlePen.Width = pointerNeedleThinWidth;
            g.DrawLine(needlePen, courseNeedleBottomLineTop, courseNeedleBottomLineBottom);

            if (InstrumentState.ShowToFromFlag)
            {
                if (InstrumentState.ToFlag)
                {
                    g.FillPolygon(Brushes.White, new[] {toFlagTop, toFlagLeft, toFlagRight});
                }
                if (InstrumentState.FromFlag)
                {
                    g.FillPolygon(Brushes.White, new[] {fromFlagBottom, fromFlagLeft, fromFlagRight});
                }
            }


            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }

        private void DrawDesiredHeadingBug(Graphics g, RectangleF outerBounds)
        {
            var basicState = g.Save();
            g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
            g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
            g.RotateTransform(InstrumentState.DesiredHeadingDegrees);
            g.TranslateTransform(-outerBounds.Width/2.0f, -outerBounds.Height/2.0f);

            float headingBugSquareSize = 20;
            float headingBugGapBetweenSquares = 5;
            float headingBugSquareTop = 18;
            var centerX = outerBounds.X + (outerBounds.Width/2.0f);
            var leftHeadingBugSquareLocation =
                new PointF(centerX - headingBugSquareSize - (headingBugGapBetweenSquares/2.0f), headingBugSquareTop);
            var rightHeadingBugSquareLocation = new PointF(centerX + (headingBugGapBetweenSquares/2.0f),
                                                           headingBugSquareTop);
            var headingBugLeftSquare = new RectangleF(leftHeadingBugSquareLocation,
                                                      new SizeF(headingBugSquareSize, headingBugSquareSize));
            var headingBugRightSquare = new RectangleF(rightHeadingBugSquareLocation,
                                                       new SizeF(headingBugSquareSize, headingBugSquareSize));
            var headingBugColor = Color.FromArgb(248, 238, 153);
            Brush headingBugBrush = new SolidBrush(headingBugColor);
            g.FillRectangle(headingBugBrush, headingBugLeftSquare);
            g.FillRectangle(headingBugBrush, headingBugRightSquare);
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
        }

        private void DrawNoDataFlag(Graphics g, RectangleF outerBounds)
        {
            if (InstrumentState.NoDataFlag)
            {
                g.FillRectangle(Brushes.Black, outerBounds);
                if (_noData.MaskedImage != null)
                {
                    g.DrawImage(_noData.MaskedImage, outerBounds,
                                new RectangleF(new Point(0, 0), _noData.MaskedImage.Size), GraphicsUnit.Pixel);
                }
                else
                {
                    _log.Debug("_noData.MaskedImage was null in DrawNoDataFlag");
                }
            }
        }

        private void DrawInstrumentMode(Graphics g, RectangleF outerBounds)
        {
            var fontFamily = _fonts.Families[0];
            var labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            var labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                            StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;

            float letterHeight = 20;
            float margin = 8;
            float labelWidth = 50;

            var howLongSinceInstrumentModeChanged =
                DateTime.Now.Subtract(InstrumentState.WhenInstrumentModeLastChanged);
            if (howLongSinceInstrumentModeChanged.TotalMilliseconds <= 2000)
            {
                var toDisplay = string.Empty;
                switch (InstrumentState.InstrumentMode)
                {
                    case F16EHSIInstrumentState.InstrumentModes.Unknown:
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.PlsTacan:
                        toDisplay = "PLS/TACAN";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.Tacan:
                        toDisplay = "TACAN";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.Nav:
                        toDisplay = "NAV";
                        break;
                    case F16EHSIInstrumentState.InstrumentModes.PlsNav:
                        toDisplay = "PLS/NAV";
                        break;
                    default:
                        break;
                }
                if (!InstrumentState.ShowBrightnessLabel)
                {
                    DrawCenterLabel(g, outerBounds, toDisplay);
                }
            }

            //draw PLS label
            if (
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsNav
                ||
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsTacan
                )
            {
                var plsLabelRect = new RectangleF(outerBounds.Width*0.25f, outerBounds.Height - letterHeight - margin,
                                                  labelWidth, letterHeight);
                g.DrawString("PLS", labelFont, Brushes.White, plsLabelRect, labelStringFormat);
            }

            if (
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsNav
                ||
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.Nav
                )
            {
                var navLabelRect = new RectangleF(outerBounds.Width*0.7f, outerBounds.Height - letterHeight - margin,
                                                  labelWidth, letterHeight);
                g.DrawString("NAV", labelFont, Brushes.White, navLabelRect, labelStringFormat);
            }

            if (
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.PlsTacan
                ||
                InstrumentState.InstrumentMode == F16EHSIInstrumentState.InstrumentModes.Tacan
                )
            {
                var tacanLabelRect = new RectangleF(outerBounds.Width*0.7f, outerBounds.Height - letterHeight - margin,
                                                    labelWidth, letterHeight);
                g.DrawString("TCN", labelFont, Brushes.White, tacanLabelRect, labelStringFormat);
            }
        }

        private void DrawHeadingAndCourseAdjustLabels(Graphics g, RectangleF outerBounds)
        {
            var fontFamily = _fonts.Families[0];
            var labelFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            var labelStringFormat = new StringFormat();
            labelStringFormat.Alignment = StringAlignment.Center;
            labelStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                            StringFormatFlags.NoWrap;
            labelStringFormat.LineAlignment = StringAlignment.Center;
            labelStringFormat.Trimming = StringTrimming.None;

            float letterHeight = 20;
            float letterWidth = 20;
            float toAddX = -5;
            float toAddY = -5;
            float margin = 40;
            //draw HDG label
            var hdgHRect = new RectangleF(margin, outerBounds.Height*0.82f, letterWidth, letterHeight);
            g.DrawString("H", labelFont, Brushes.White, hdgHRect, labelStringFormat);

            var hdgDRect = new RectangleF(hdgHRect.X + hdgHRect.Width + toAddX, hdgHRect.Y + hdgHRect.Height + toAddY,
                                          letterWidth, letterHeight);
            g.DrawString("D", labelFont, Brushes.White, hdgDRect, labelStringFormat);

            var hdgGRect = new RectangleF(hdgDRect.X + hdgDRect.Width + toAddX, hdgDRect.Y + hdgDRect.Height + toAddY,
                                          letterWidth, letterHeight);
            g.DrawString("G", labelFont, Brushes.White, hdgGRect, labelStringFormat);


            //draw CRS label
            var crsCRect = new RectangleF(outerBounds.Width - ((letterWidth + toAddX)*3) - hdgHRect.X, hdgGRect.Y,
                                          letterWidth, letterHeight);
            g.DrawString("C", labelFont, Brushes.White, crsCRect, labelStringFormat);

            var crsRRect = new RectangleF(crsCRect.X + crsCRect.Width + toAddX, hdgDRect.Y, letterWidth, letterHeight);
            g.DrawString("R", labelFont, Brushes.White, crsRRect, labelStringFormat);

            var crsSRect = new RectangleF(crsRRect.X + crsRRect.Width + toAddX, hdgHRect.Y, letterWidth, letterHeight);
            g.DrawString("S", labelFont, Brushes.White, crsSRect, labelStringFormat);
        }

        private void DrawDesiredCourse(Graphics g, RectangleF outerBounds)
        {
            var fontFamily = _fonts.Families[0];
            var digitsFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);
            var crsFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            var desiredCourseDigitStringFormat = new StringFormat();
            desiredCourseDigitStringFormat.Alignment = StringAlignment.Center;
            desiredCourseDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                         StringFormatFlags.NoWrap;
            desiredCourseDigitStringFormat.LineAlignment = StringAlignment.Center;
            desiredCourseDigitStringFormat.Trimming = StringTrimming.None;

            var crsStringFormat = new StringFormat();
            crsStringFormat.Alignment = StringAlignment.Far;
            crsStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                          StringFormatFlags.NoWrap;
            crsStringFormat.LineAlignment = StringAlignment.Center;
            crsStringFormat.Trimming = StringTrimming.None;


            var initialState = g.Save();
            g.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;

            var basicState = g.Save();

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            var desiredCourse = string.Format("{0:000.0}", InstrumentState.DesiredCourseDegrees);
            var hundredsDigit = desiredCourse.Substring(0, 1);
            var tensDigit = desiredCourse.Substring(1, 1);
            var onesDigit = desiredCourse.Substring(2, 1);

            float digitWidth = 22;
            float digitHeight = 32;
            float digitSeparationPixels = -2;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float margin = 8;
            var hundredsRect = new RectangleF(outerBounds.Width - margin - ((digitWidth + digitSeparationPixels)*3),
                                              margin, digitWidth, digitHeight);
            var tensRect = new RectangleF(hundredsRect.X + digitWidth + digitSeparationPixels, hundredsRect.Y,
                                          digitWidth, digitHeight);
            var onesRect = new RectangleF(tensRect.X + digitWidth + digitSeparationPixels, tensRect.Y, digitWidth,
                                          digitHeight);

            g.DrawString(hundredsDigit, digitsFont, Brushes.White, hundredsRect, desiredCourseDigitStringFormat);
            g.DrawString(tensDigit, digitsFont, Brushes.White, tensRect, desiredCourseDigitStringFormat);
            g.DrawString(onesDigit, digitsFont, Brushes.White, onesRect, desiredCourseDigitStringFormat);

            var crsRect = new RectangleF(hundredsRect.X, 45, (digitWidth + digitSeparationPixels)*3, 20);
            g.DrawString("CRS", crsFont, Brushes.White, crsRect, crsStringFormat);

            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }

        private void DrawDistanceToBeacon(Graphics g, RectangleF outerBounds)
        {
            var fontFamily = _fonts.Families[0];
            var digitsFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);
            var nmFont = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Point);

            var distanceDigitStringFormat = new StringFormat();
            distanceDigitStringFormat.Alignment = StringAlignment.Center;
            distanceDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                    StringFormatFlags.NoWrap;
            distanceDigitStringFormat.LineAlignment = StringAlignment.Center;
            distanceDigitStringFormat.Trimming = StringTrimming.None;

            var nmStringFormat = new StringFormat();
            nmStringFormat.Alignment = StringAlignment.Center;
            nmStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                         StringFormatFlags.NoWrap;
            nmStringFormat.LineAlignment = StringAlignment.Center;
            nmStringFormat.Trimming = StringTrimming.None;

            var initialState = g.Save();
            g.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;
            var basicState = g.Save();

            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            var distanceToBeaconString = string.Format("{0:000.0}", InstrumentState.DistanceToBeaconNauticalMiles);
            var hundredsDigit = distanceToBeaconString.Substring(0, 1);
            var tensDigit = distanceToBeaconString.Substring(1, 1);
            var onesDigit = distanceToBeaconString.Substring(2, 1);
            var tenthsDigit = distanceToBeaconString.Substring(4, 1);

            float digitWidth = 22;
            float digitHeight = 32;
            float digitSeparationPixels = -4;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            var hundredsRect = new RectangleF(12, 8, digitWidth, digitHeight);
            var tensRect = new RectangleF(hundredsRect.X + digitWidth + digitSeparationPixels, hundredsRect.Y,
                                          digitWidth, digitHeight);
            var onesRect = new RectangleF(tensRect.X + digitWidth + digitSeparationPixels, tensRect.Y, digitWidth,
                                          digitHeight);
            var tenthsRect = new RectangleF(onesRect.X + digitWidth + 4, onesRect.Y, digitWidth, digitHeight);

            g.DrawString(hundredsDigit, digitsFont, Brushes.White, hundredsRect, distanceDigitStringFormat);
            g.DrawString(tensDigit, digitsFont, Brushes.White, tensRect, distanceDigitStringFormat);
            g.DrawString(onesDigit, digitsFont, Brushes.White, onesRect, distanceDigitStringFormat);

            g.FillRectangle(Brushes.White, tenthsRect);
            g.DrawString(tenthsDigit, digitsFont, Brushes.Black, tenthsRect, distanceDigitStringFormat);

            if (InstrumentState.DmeInvalidFlag)
            {
                var dmeInvalidFlagUpperLeft = new PointF(hundredsRect.X, hundredsRect.Y + 8);
                var dmeInvalidFlagSize = new SizeF((tenthsRect.X + tenthsRect.Width) - hundredsRect.X, 16);
                var dmeInvalidFlagRect = new RectangleF(dmeInvalidFlagUpperLeft, dmeInvalidFlagSize);
                var redFlagColor = Color.FromArgb(224, 43, 48);
                Brush redFlagBrush = new SolidBrush(redFlagColor);
                g.FillRectangle(redFlagBrush, dmeInvalidFlagRect);
            }

            var nmRect = new RectangleF(hundredsRect.X, 45, 30, 20);
            g.DrawString("NM", nmFont, Brushes.White, nmRect, nmStringFormat);

            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }

        private void DrawCompassRose(Graphics g, RectangleF outerBounds)
        {
            var initialState = g.Save();
            g.InterpolationMode = Options.GDIPlusOptions.InterpolationMode;
            g.PixelOffsetMode = Options.GDIPlusOptions.PixelOffsetMode;
            g.SmoothingMode = Options.GDIPlusOptions.SmoothingMode;
            g.TextRenderingHint = Options.GDIPlusOptions.TextRenderingHint;
            var basicState = g.Save();

            var majorHeadingDigitStringFormat = new StringFormat();
            majorHeadingDigitStringFormat.Alignment = StringAlignment.Center;
            majorHeadingDigitStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                                                        StringFormatFlags.NoWrap;
            majorHeadingDigitStringFormat.LineAlignment = StringAlignment.Center;
            majorHeadingDigitStringFormat.Trimming = StringTrimming.None;

            var fontFamily = _fonts.Families[1];

            var majorHeadingDigitFont = new Font(fontFamily, 27.5f, FontStyle.Bold, GraphicsUnit.Point);

            var linePen = new Pen(Color.White);
            linePen.Width = 3;
            GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            float majorHeadingLineLength = 28;
            var minorHeadingLineLength = majorHeadingLineLength/2.0f;
            float majorHeadingLegendLayoutRectangleHeight = 30;
            float majorHeadingLegendLayoutRectangleWidth = 30;
            Brush majorHeadingBrush = new SolidBrush(Color.White);

            var innerBounds = new RectangleF(outerBounds.X, outerBounds.Y, outerBounds.Width, outerBounds.Height);
            var marginWidth = 30f;
            innerBounds.Inflate(-marginWidth, -marginWidth);

            for (var i = 0; i < 360; i += 45)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width/2.0f, innerBounds.Height/2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-innerBounds.Width/2.0f, -innerBounds.Height/2.0f);

                float separationPixels = 2;
                //draw 45-degree outer ticks
                if (i%90 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, -separationPixels),
                               new PointF(innerBounds.Width/2.0f, -((minorHeadingLineLength*1.5f) + separationPixels)));
                }
                else
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, -separationPixels),
                               new PointF(innerBounds.Width/2.0f, -((majorHeadingLineLength) + separationPixels)));
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }


            for (var i = 0; i < 360; i++)
            {
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                g.TranslateTransform(outerBounds.Width/2.0f, outerBounds.Height/2.0f);
                g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                g.TranslateTransform(-outerBounds.Width/2.0f, -outerBounds.Height/2.0f);

                g.TranslateTransform(marginWidth, marginWidth);
                g.TranslateTransform(innerBounds.Width/2.0f, innerBounds.Height/2.0f);
                g.RotateTransform(i);
                g.TranslateTransform(-innerBounds.Width/2.0f, -innerBounds.Height/2.0f);
                if (i%10 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, 0),
                               new PointF(innerBounds.Width/2.0f, majorHeadingLineLength));
                }
                else if (i%5 == 0)
                {
                    g.DrawLine(linePen, new PointF(innerBounds.Width/2.0f, 0),
                               new PointF(innerBounds.Width/2.0f, minorHeadingLineLength));
                }
                if (i%30 == 0)
                {
                    var majorHeadingLegendText = string.Format("{0:##}", i/10);
                    if (i == 90)
                    {
                        majorHeadingLegendText = "E";
                    }
                    else if (i == 180)
                    {
                        majorHeadingLegendText = "S";
                    }
                    else if (i == 270)
                    {
                        majorHeadingLegendText = "W";
                    }
                    else if (i == 0)
                    {
                        majorHeadingLegendText = "N";
                    }

                    var majorHeadingLegendLayoutRectangle =
                        new RectangleF(((innerBounds.Width/2.0f) - majorHeadingLegendLayoutRectangleWidth/2.0f),
                                       (majorHeadingLegendLayoutRectangleHeight/2.0f),
                                       majorHeadingLegendLayoutRectangleWidth, majorHeadingLegendLayoutRectangleHeight);
                    majorHeadingLegendLayoutRectangle.Offset(0, 18);
                    g.DrawString(majorHeadingLegendText, majorHeadingDigitFont, majorHeadingBrush,
                                 majorHeadingLegendLayoutRectangle, majorHeadingDigitStringFormat);
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
            }
            GraphicsUtil.RestoreGraphicsState(g, ref initialState);
        }
   }
}