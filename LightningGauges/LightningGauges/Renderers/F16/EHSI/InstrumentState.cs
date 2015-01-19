using System;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16.EHSI
{
    [Serializable]
    public class InstrumentState : InstrumentStateBase
    {
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

        public InstrumentState()
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
            NoPowerFlag = false; //Added by Falcas 28-10-2012
        }

        public bool NoPowerFlag { get; set; }
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
                if (Single.IsNaN(distance) || Single.IsNegativeInfinity(distance))
                {
                    distance = 0;
                }
                if (Single.IsPositiveInfinity(distance))
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
                if (Single.IsInfinity(courseDeviation) || Single.IsNaN(courseDeviation))
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
                if (Single.IsInfinity(courseDeviationLimit) || Single.IsNaN(courseDeviationLimit) ||
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
                if (Single.IsNaN(heading) || Single.IsInfinity(heading))
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
                if (Single.IsInfinity(bearingToBeacon) || Single.IsNaN(bearingToBeacon))
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
}