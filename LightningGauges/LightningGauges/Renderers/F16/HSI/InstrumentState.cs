using System;
using Common.SimSupport;

namespace LightningGauges.Renderers.F16.HSI
{
    [Serializable]
    public class InstrumentState : InstrumentStateBase
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
                if (Single.IsNaN(desiredCourse) || Single.IsInfinity(desiredCourse)) desiredCourse = 0;
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
                if (Single.IsNaN(desiredHeading) || Single.IsInfinity(desiredHeading)) desiredHeading = 0;
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
                if (Single.IsNaN(courseDeviation) || Single.IsInfinity(courseDeviation)) courseDeviation = 0;
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
    }
}