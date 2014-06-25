using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Common.Imaging;
using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16HorizontalSituationIndicator : IInstrumentRenderer
    {
        F16HorizontalSituationIndicator.F16HSIInstrumentState InstrumentState { get; set; }
    }

    public class F16HorizontalSituationIndicator : InstrumentRendererBase, IF16HorizontalSituationIndicator
    {
        #region Image Location Constants

        private const string HSI_BACKGROUND_IMAGE_FILENAME = "hsi.bmp";
        private const string HSI_BACKGROUND_MASK_FILENAME = "hsi_mask.bmp";
        private const string HSI_BEARING_TO_BEACON_NEEDLE_IMAGE_FILENAME = "hsibeac.bmp";
        private const string HSI_BEARING_TO_BEACON_NEEDLE_MASK_FILENAME = "hsibeac_mask.bmp";
        private const string HSI_CDI_FLAG_IMAGE_FILENAME = "hsicdflag.bmp";
        private const string HSI_CDI_FLAG_MASK_FILENAME = "hsicdflag_mask.bmp";
        private const string HSI_COMPASS_ROSE_IMAGE_FILENAME = "hsicomp.bmp";
        private const string HSI_COMPASS_ROSE_MASK_FILENAME = "hsicomp_mask.bmp";
        private const string HSI_COURSE_DEVIATION_INDICATOR_IMAGE_FILENAME = "hsicorsdev.bmp";
        private const string HSI_COURSE_DEVIATION_INDICATOR_MASK_FILENAME = "hsicorsdev_mask.bmp";
        private const string HSI_HEADING_BUG_IMAGE_FILENAME = "hsiheadref.bmp";
        private const string HSI_HEADING_BUG_MASK_FILENAME = "hsiheadref_mask.bmp";
        private const string HSI_INNER_WHEEL_IMAGE_FILENAME = "hsiinner.bmp";
        private const string HSI_INNER_WHEEL_MASK_FILENAME = "hsiinner_mask.bmp";
        private const string HSI_AIRPLANE_SYMBOL_IMAGE_FILENAME = "hsiplane.bmp";
        private const string HSI_AIRPLANE_SYMBOL_MASK_FILENAME = "hsiplane_mask.bmp";
        private const string HSI_RANGE_FLAG_IMAGE_FILENAME = "hsirangeflag.bmp";
        private const string HSI_RANGE_FLAG_MASK_FILENAME = "hsirangeflag_mask.bmp";
        private const string HSI_TO_FLAG_IMAGE_FILENAME = "hsitotrue.bmp";
        private const string HSI_TO_FLAG_MASK_FILENAME = "hsitotrue_mask.bmp";
        private const string HSI_FROM_FLAG_IMAGE_FILENAME = "hsitofalse.bmp";
        private const string HSI_FROM_FLAG_MASK_FILENAME = "hsitofalse_mask.bmp";
        private const string HSI_OFF_FLAG_IMAGE_FILENAME = "adioff.bmp";
        private const string HSI_OFF_FLAG_MASK_FILENAME = "adiflags_mask.bmp";

        private const string HSI_RANGE_FONT_FILENAME = "font1.bmp";

        private static readonly string IMAGES_FOLDER_NAME =
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName +
            Path.DirectorySeparatorChar + "images";

        #endregion

        #region Instance variables

        private static readonly object _imagesLock = new object();
        private static ImageMaskPair _hsiBackground;
        private static ImageMaskPair _hsiBearingToBeaconNeedle;
        private static ImageMaskPair _hsiCDIFlag;
        private static ImageMaskPair _compassRose;
        private static ImageMaskPair _hsiCourseDeviationIndicator;
        private static ImageMaskPair _hsiHeadingBug;
        private static ImageMaskPair _hsiInnerWheel;
        private static ImageMaskPair _airplaneSymbol;
        private static ImageMaskPair _hsiRangeFlag;
        private static ImageMaskPair _toFlag;
        private static ImageMaskPair _fromFlag;
        private static ImageMaskPair _hsiOffFlag;

        private static FontGraphic _rangeFont;
        private static bool _imagesLoaded;

        #endregion

        public F16HorizontalSituationIndicator()
        {
            InstrumentState = new F16HSIInstrumentState();
        }

        #region Initialization Code

        private void LoadImageResources()
        {
            if (_hsiBackground == null)
            {
                _hsiBackground = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BACKGROUND_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BACKGROUND_MASK_FILENAME
                    );
            }
            if (_hsiBearingToBeaconNeedle == null)
            {
                _hsiBearingToBeaconNeedle = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BEARING_TO_BEACON_NEEDLE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_BEARING_TO_BEACON_NEEDLE_MASK_FILENAME
                    );
                _hsiBearingToBeaconNeedle.Use1BitAlpha = true;
            }
            if (_hsiCDIFlag == null)
            {
                _hsiCDIFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_CDI_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_CDI_FLAG_MASK_FILENAME
                    );
                _hsiCDIFlag.Use1BitAlpha = true;
            }
            if (_compassRose == null)
            {
                _compassRose = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COMPASS_ROSE_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COMPASS_ROSE_MASK_FILENAME
                    );
            }
            if (_hsiCourseDeviationIndicator == null)
            {
                _hsiCourseDeviationIndicator = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COURSE_DEVIATION_INDICATOR_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_COURSE_DEVIATION_INDICATOR_MASK_FILENAME
                    );
                _hsiCourseDeviationIndicator.Use1BitAlpha = true;
            }
            if (_hsiHeadingBug == null)
            {
                _hsiHeadingBug = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_HEADING_BUG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_HEADING_BUG_MASK_FILENAME
                    );
                _hsiHeadingBug.Use1BitAlpha = true;
            }
            if (_hsiInnerWheel == null)
            {
                _hsiInnerWheel = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_INNER_WHEEL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_INNER_WHEEL_MASK_FILENAME
                    );
            }
            if (_airplaneSymbol == null)
            {
                _airplaneSymbol = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_AIRPLANE_SYMBOL_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_AIRPLANE_SYMBOL_MASK_FILENAME
                    );
                _airplaneSymbol.Use1BitAlpha = true;
            }
            if (_hsiRangeFlag == null)
            {
                _hsiRangeFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FLAG_MASK_FILENAME
                    );
                _hsiRangeFlag.Use1BitAlpha = true;
            }
            if (_toFlag == null)
            {
                _toFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_TO_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_TO_FLAG_MASK_FILENAME
                    );
                _toFlag.Use1BitAlpha = true;
            }
            if (_fromFlag == null)
            {
                _fromFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_FROM_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_FROM_FLAG_MASK_FILENAME
                    );
                _fromFlag.Use1BitAlpha = true;
            }
            if (_hsiOffFlag == null)
            {
                _hsiOffFlag = ImageMaskPair.CreateFromFiles(
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_OFF_FLAG_IMAGE_FILENAME,
                    IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_OFF_FLAG_MASK_FILENAME
                    );
            }

            if (_rangeFont == null)
            {
                _rangeFont = new FontGraphic(IMAGES_FOLDER_NAME + Path.DirectorySeparatorChar + HSI_RANGE_FONT_FILENAME);
            }
            _imagesLoaded = true;
        }

        #endregion

        public F16HSIInstrumentState InstrumentState { get; set; }

        #region Instrument State

        [Serializable]
        public class F16HSIInstrumentState : InstrumentStateBase
        {
            private const float DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES = 5.0F;
            private const float MAX_RANGE = 999.9F;
            private float _bearingToBeaconDegrees;
            private float _courseDeviationDegrees;
            private float _courseDeviationLimitDegrees = DEFAULT_COURSE_DEVIATION_LIMIT_DEGREES;
            private int _desiredCourseDegrees;
            private int _desiredHeadingDegrees;
            private float _distanceToBeaconNauticalMiles;
            private float _magneticHeadingDegrees;

            public F16HSIInstrumentState()
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
                OffFlag = false;
            }

            public bool OffFlag { get; set; }
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
                    if (float.IsNaN(desiredCourse) || float.IsInfinity(desiredCourse)) desiredCourse = 0;
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
                    if (float.IsNaN(desiredHeading) || float.IsInfinity(desiredHeading)) desiredHeading = 0;
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
                    if (float.IsNaN(courseDeviation) || float.IsInfinity(courseDeviation)) courseDeviation = 0;
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
        }

        #endregion


        public override void Render(Graphics g, Rectangle bounds)
        {
            if (!_imagesLoaded)
            {
                LoadImageResources();
            }
            lock (_imagesLock)
            {
                //store the canvas's transform and clip settings so we can restore them later
                var initialState = g.Save();

                //set up the canvas scale and clipping region
                var width = 0;
                var height = 0;

                width = _hsiBackground.Image.Width - 47;
                height = _hsiBackground.Image.Height - 49;

                g.ResetTransform(); //clear any existing transforms
                g.SetClip(bounds); //set the clipping region on the graphics object to our render rectangle's boundaries
                g.FillRectangle(Brushes.Black, bounds);
                g.ScaleTransform(bounds.Width/(float) width, bounds.Height/(float) height);
                //set the initial scale transformation 

                g.TranslateTransform(-24, -14);
                //save the basic canvas transform and clip settings so we can revert to them later, as needed
                var basicState = g.Save();

                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, -11);
                g.DrawImage(_hsiBackground.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                float centerX = 128;
                float centerY = 128;


                //draw compass rose
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                g.TranslateTransform(-centerX, -centerY);
                g.DrawImage(_compassRose.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw inner wheel
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                g.RotateTransform(InstrumentState.DesiredCourseDegrees);
                g.TranslateTransform(-centerX, -centerY);
                g.TranslateTransform(0.5f, -2.0f);
                g.DrawImage(_hsiInnerWheel.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw heading bug
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(InstrumentState.DesiredHeadingDegrees - InstrumentState.MagneticHeadingDegrees);
                g.TranslateTransform(-centerX, -centerY);
                g.DrawImage(_hsiHeadingBug.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);


                //draw the bearing to beacon indicator needle
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(-(InstrumentState.MagneticHeadingDegrees - InstrumentState.BearingToBeaconDegrees));
                g.TranslateTransform(-centerX, -centerY);
                g.TranslateTransform(1, 0);
                g.DrawImage(_hsiBearingToBeaconNeedle.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the CDI flag
                if (InstrumentState.DeviationInvalidFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                    g.RotateTransform(InstrumentState.DesiredCourseDegrees);
                    g.TranslateTransform(-centerX, -centerY);
                    g.DrawImage(_hsiCDIFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the range to the beacon
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    var distanceToBeacon = InstrumentState.DistanceToBeaconNauticalMiles;
                    if (distanceToBeacon > 999.9) distanceToBeacon = 999.9f;
                    var distanceToBeaconString = string.Format("{0:000.0}", distanceToBeacon);
                    var distanceToBeaconHundreds = distanceToBeaconString[0];
                    var distanceToBeaconTens = distanceToBeaconString[1];
                    var distanceToBeaconOnes = distanceToBeaconString[2];
                    var distanceToBeaconHundredsImage = _rangeFont.GetCharImage(distanceToBeaconHundreds);
                    var distanceToBeaconTensImage = _rangeFont.GetCharImage(distanceToBeaconTens);
                    var distanceToBeaconOnesImage = _rangeFont.GetCharImage(distanceToBeaconOnes);

                    var currentX = 0;
                    var y = 0;
                    currentX = 29;
                    y = 45;
                    var spacingPixels = -5;
                    g.DrawImage(distanceToBeaconHundredsImage, new Point(currentX, y));
                    currentX += distanceToBeaconHundredsImage.Width + spacingPixels;
                    g.DrawImage(distanceToBeaconTensImage, new Point(currentX, y));
                    currentX += distanceToBeaconTensImage.Width + spacingPixels;
                    g.DrawImage(distanceToBeaconOnesImage, new Point(currentX, y));
                    currentX += distanceToBeaconOnesImage.Width + spacingPixels;
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw desired course
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    var desiredCourseString = string.Format("{0:000}", InstrumentState.DesiredCourseDegrees);
                    var desiredCourseHundreds = desiredCourseString[0];
                    var desiredCourseTens = desiredCourseString[1];
                    var desiredCourseOnes = desiredCourseString[2];
                    var desiredCourseHundredsImage = _rangeFont.GetCharImage(desiredCourseHundreds);
                    var desiredCourseTensImage = _rangeFont.GetCharImage(desiredCourseTens);
                    var desiredCourseOnesImage = _rangeFont.GetCharImage(desiredCourseOnes);

                    var currentX = 0;
                    var y = 0;
                    currentX = 182;
                    y = 45;
                    var spacingPixels = -5;
                    g.DrawImage(desiredCourseHundredsImage, new Point(currentX, y));
                    currentX += desiredCourseHundredsImage.Width + spacingPixels;
                    g.DrawImage(desiredCourseTensImage, new Point(currentX, y));
                    currentX += desiredCourseTensImage.Width + spacingPixels;
                    g.DrawImage(desiredCourseOnesImage, new Point(currentX, y));
                    currentX += desiredCourseOnesImage.Width + spacingPixels;

                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the TO flag 
                if (InstrumentState.ShowToFromFlag && InstrumentState.ToFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                    g.RotateTransform(InstrumentState.DesiredCourseDegrees);
                    g.TranslateTransform(-centerX, -centerY);
                    g.DrawImage(_toFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the FROM flag 
                if (InstrumentState.ShowToFromFlag && InstrumentState.FromFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                    g.RotateTransform(InstrumentState.DesiredCourseDegrees);
                    g.TranslateTransform(-centerX, -centerY);
                    g.DrawImage(_fromFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw the range flag
                if (InstrumentState.DmeInvalidFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.DrawImage(_hsiRangeFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //draw course deviation indicator
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(-InstrumentState.MagneticHeadingDegrees);
                g.RotateTransform(InstrumentState.DesiredCourseDegrees);
                g.TranslateTransform(-centerX, -centerY);
                var cdiPct = InstrumentState.CourseDeviationDegrees/InstrumentState.CourseDeviationLimitDegrees;
                var cdiRange = 46.0f;
                var cdiPos = cdiPct*cdiRange;
                g.TranslateTransform(cdiPos, -2);
                try
                {
                    g.DrawImage(_hsiCourseDeviationIndicator.MaskedImage, new Point(0, 0));
                }
                catch (OverflowException)
                {
                }
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw airplane symbol
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                g.TranslateTransform(0, 5);
                g.DrawImage(_airplaneSymbol.MaskedImage, new Point(0, 0));
                GraphicsUtil.RestoreGraphicsState(g, ref basicState);

                //draw the OFF flag
                if (InstrumentState.OffFlag)
                {
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                    g.RotateTransform(-25);
                    g.TranslateTransform(20, 50);
                    g.DrawImage(_hsiOffFlag.MaskedImage, new Point(0, 0));
                    GraphicsUtil.RestoreGraphicsState(g, ref basicState);
                }

                //restore the canvas's transform and clip settings to what they were when we entered this method
                g.Restore(initialState);
            }
        }
    }
}