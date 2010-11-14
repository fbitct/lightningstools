using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using F16CPD.FlightInstruments;

namespace F16CPD
{
    
    [Serializable]
    public sealed class FlightData
    {
        private int _automaticLowAltitudeWarningInFeet = 0;
        private float _barometricPressureInInchesOfMercury = 29.92f;
        private int _transitionAltitudeInFeet = 18000;
        private NavModes _hsiMode = NavModes.Nav;

        private float _indicatedAltitudeAboveMeanSeaLevelInFeet= 0.0f;
        private float _trueAltitudeAboveMeanSeaLevelInFeet = 0.0f;
        private float _altitudeAboveGroundLevelInFeet = 0.0f;
        private float _angleOfAttackInDegrees = 0.0f;
        private float _pitchAngleInDegrees = 0.0f;
        private float _rollAngleInDegrees = 0.0f;
        private float _betaAngleInDegrees = 0.0f;
        private float _gammaAngleInDegrees = 0.0f;
        private float _windOffsetToFlightPathMarkerDegrees = 0.0f;
        private int _hsiDesiredHeadingInDegrees = 0;

        private float _adiIlsLocalizerDeviationDecimalDegrees = 0.0f;
        private float _adiIlsGlideslopeDeviationDecimalDegrees = 0.0f;
        private float _hsiCourseDeviationInDegrees = 0.0f;
        private float _hsiDistanceToBeaconInNauticalMiles = 0.0f;
        private float _hsiBearingToBeaconInDegrees = 0;
        private float _hsiCourseDeviationLimitInDecimalDegrees = 0.0f;
        private int _hsiDesiredCourseInDegrees = 0;
        private float _hsiLocalizerCourseInDecimalDegrees = 0.0f;
        private float _magneticHeadingInDecimalDegrees = 0.0f;
        private float _rateOfTurnInDegreesPerSecond = 0.0f;
        private string _tacanChannel = "106X";
        private float _indicatedAirspeedFeetPerSecond = 0.0f;
        
        public float MapCoordinateFeetNorth
        {
            get;
            set;
        }
        public float MapCoordinateFeetEast
        {
            get;
            set;
        }
        public bool HsiDisplayToFromFlag
        {
            get;
            set;
        }
        public string TacanChannel
        {
            get
            {
                return _tacanChannel;
            }
            set
            {
                _tacanChannel = value;
            }
        }
        public float LatitudeInDecimalDegrees
        {
            get;
            set;
        }
        public float LongitudeInDecimalDegrees
        {
            get;
            set;
        }
        public int TransitionAltitudeInFeet
        {
            get
            {
                return _transitionAltitudeInFeet;
            }
            set
            {
                if (value < 1000) value = -2500;
                if (value > 20000) value = 20000;

                _transitionAltitudeInFeet = value;
            }
        }
        public float RateOfTurnInDecimalDegreesPerSecond
        {
            get
            {
                return _rateOfTurnInDegreesPerSecond;
            }
            set
            {
                if (!float.IsInfinity(value) && !float.IsNaN(value))
                {
                    _rateOfTurnInDegreesPerSecond = value;
                }
                else
                {
                    _rateOfTurnInDegreesPerSecond = 0.0f;
                }
            }
        }
        public float IndicatedAltitudeAboveMeanSeaLevelInDecimalFeet
        {
            get
            {
                return _indicatedAltitudeAboveMeanSeaLevelInFeet;
            }
            set
            {
                if (value < -4500) value = -4500;
                if (value > 120000) value = 120000;
                _indicatedAltitudeAboveMeanSeaLevelInFeet = value;
            }
        }
        public float TrueAltitudeAboveMeanSeaLevelInDecimalFeet
        {
            get
            {
                return _trueAltitudeAboveMeanSeaLevelInFeet;
            }
            set
            {
                if (value < -2500) value = -2500;
                if (value > 120000) value = 120000;
                _trueAltitudeAboveMeanSeaLevelInFeet = value;
            }
        }
        public float AltitudeAboveGroundLevelInDecimalFeet
        {
            get
            {
                return _altitudeAboveGroundLevelInFeet;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 180000) value = 180000;
                _altitudeAboveGroundLevelInFeet = value;
            }
        }
        public float MachNumber
        {
            get;
            set;
        }
        public float IndicatedAirspeedInDecimalFeetPerSecond
        {
            get
            {
                return _indicatedAirspeedFeetPerSecond;
            }
            set
            {
                _indicatedAirspeedFeetPerSecond = (float)(Math.Round(value, 1));
            }
        }
        public float TrueAirspeedInDecimalFeetPerSecond
        {
            get;
            set;
        }
        public float GroundSpeedInDecimalFeetPerSecond
        {
            get;
            set;
        }
        public int AutomaticLowAltitudeWarningInFeet
        {
            get
            {
                return _automaticLowAltitudeWarningInFeet;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 99999) value = 99999;
                _automaticLowAltitudeWarningInFeet = value;
            }
        }
        public float BarometricPressureInDecimalInchesOfMercury
        {
            get
            {
                return _barometricPressureInInchesOfMercury;
            }
            set
            {
                if (value < 27.97f) value = 27.97f;
                if (value > 31.11f) value = 31.11f;
                _barometricPressureInInchesOfMercury = value;
            }
        }
        public float AngleOfAttackInDegrees
        {
            get
            {
                return _angleOfAttackInDegrees;
            }
            set
            {
                _angleOfAttackInDegrees = value %360;
            }
        }
        public float VerticalVelocityInDecimalFeetPerSecond
        {
            get;
            set;
        }
        public float PitchAngleInDecimalDegrees
        {
            get
            {
                return _pitchAngleInDegrees;
            }
            set
            {
                _pitchAngleInDegrees = value % 360;
                if (_pitchAngleInDegrees > 90) _pitchAngleInDegrees = 90;
                if (_pitchAngleInDegrees < -90) _pitchAngleInDegrees = -90; 
            }
        }
        public float RollAngleInDecimalDegrees
        {
            get
            {
                return _rollAngleInDegrees;
            }
            set
            {
                _rollAngleInDegrees = value % 360;
            }
        }
        public float BetaAngleInDecimalDegrees
        {
            get
            {
                return _betaAngleInDegrees;
            }
            set
            {
                _betaAngleInDegrees = value % 360;
            }
        }
        public float GammaAngleInDecimalDegrees
        {
            get
            {
                return _gammaAngleInDegrees;
            }
            set
            {
                _gammaAngleInDegrees = value % 360;
            }
        }
        public float WindOffsetToFlightPathMarkerInDecimalDegrees
        {
            get
            {
                return _windOffsetToFlightPathMarkerDegrees;
            }
            set
            {
                _windOffsetToFlightPathMarkerDegrees = value % 360;
            }
        }
        public int HsiDesiredCourseInDegrees
        {
            get
            {
                return _hsiDesiredCourseInDegrees;
            }
            set
            {
                _hsiDesiredCourseInDegrees = value;
                if (_hsiDesiredCourseInDegrees < 0) _hsiDesiredCourseInDegrees = 360 - Math.Abs(_hsiDesiredCourseInDegrees);
            }
        }
        public int HsiDesiredHeadingInDegrees
        {
            get
            {
                return _hsiDesiredHeadingInDegrees;
            }
            set
            {
                _hsiDesiredHeadingInDegrees = value;
                if (_hsiDesiredHeadingInDegrees < 0) _hsiDesiredHeadingInDegrees = 360 - Math.Abs(_hsiDesiredHeadingInDegrees);
            }
        }
        public float AdiIlsLocalizerDeviationInDecimalDegrees
        {
            get
            {
                return _adiIlsLocalizerDeviationDecimalDegrees;
            }
            set
            {
                _adiIlsLocalizerDeviationDecimalDegrees = value % 360;
                if (Math.Abs(_adiIlsLocalizerDeviationDecimalDegrees) > Pfd.ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES)
                {
                    _adiIlsLocalizerDeviationDecimalDegrees = (float)(Math.Sign(_adiIlsLocalizerDeviationDecimalDegrees) * Pfd.ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES);
                }
            }
        }
        public float AdiIlsGlideslopeDeviationInDecimalDegrees
        {
            get
            {
                return _adiIlsGlideslopeDeviationDecimalDegrees;
            }
            set
            {
                _adiIlsGlideslopeDeviationDecimalDegrees = value % 360;
                if (Math.Abs(_adiIlsGlideslopeDeviationDecimalDegrees) > Pfd.ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES)
                {
                    _adiIlsGlideslopeDeviationDecimalDegrees = (float)(Math.Sign(_adiIlsGlideslopeDeviationDecimalDegrees) * Pfd.ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES);
                }
            }
        }
        public float HsiCourseDeviationInDecimalDegrees
        {
            get
            {
                return _hsiCourseDeviationInDegrees;
            }
            set
            {
                _hsiCourseDeviationInDegrees = value % 360;
            }
        }
        public float HsiDistanceToBeaconInNauticalMiles
        {
            get
            {
                return _hsiDistanceToBeaconInNauticalMiles;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 3500) value = 3500;
                _hsiDistanceToBeaconInNauticalMiles = value;
            }
        }
        public float HsiBearingToBeaconInDecimalDegrees
        {
            get
            {
                return _hsiBearingToBeaconInDegrees;
            }
            set
            {
                _hsiBearingToBeaconInDegrees = value % 360;
                if (_hsiBearingToBeaconInDegrees < 0) _hsiBearingToBeaconInDegrees = 360 - Math.Abs(_hsiBearingToBeaconInDegrees);
            }
        }
        public float HsiCourseDeviationLimitInDecimalDegrees
        {
            get
            {
                return _hsiCourseDeviationLimitInDecimalDegrees;
            }
            set
            {
                _hsiCourseDeviationLimitInDecimalDegrees = value % 360;
                if (_hsiCourseDeviationLimitInDecimalDegrees < 0) _hsiCourseDeviationLimitInDecimalDegrees = Math.Abs( _hsiCourseDeviationLimitInDecimalDegrees);
            }
        }
        public float HsiLocalizerDeviationInDecimalDegrees
        {
            get
            {
                return _hsiLocalizerCourseInDecimalDegrees;
            }
            set
            {
                _hsiLocalizerCourseInDecimalDegrees = value % 360;
                if (_hsiLocalizerCourseInDecimalDegrees < 0) _hsiLocalizerCourseInDecimalDegrees = 360 - Math.Abs(_hsiLocalizerCourseInDecimalDegrees);
            }
        }
        public float MagneticHeadingInDecimalDegrees
        {
            get
            {
                return _magneticHeadingInDecimalDegrees;
            }
            set
            {
                _magneticHeadingInDecimalDegrees = value % 360;
                if (_magneticHeadingInDecimalDegrees < 0) _magneticHeadingInDecimalDegrees = 360 - Math.Abs(_magneticHeadingInDecimalDegrees);
            }
        }
        public bool HsiDeviationInvalidFlag
        {
            get;
            set;
        }
        public bool HsiDistanceInvalidFlag
        {
            get;
            set;
        }
        public bool AdiAuxFlag
        {
            get;
            set;
        }
        public bool AdiGlideslopeInvalidFlag
        {
            get;
            set;
        }
        public bool AdiLocalizerInvalidFlag
        {
            get;
            set;
        }
        public bool AdiEnableCommandBars
        {
            get;
            set;
        }
        public AltimeterMode AltimeterMode
        {
            get;
            set;
        }
        public bool HsiOffFlag
        {
            get;
            set;
        }
        public bool AdiOffFlag
        {
            get;
            set;
        }
        public bool RadarAltimeterOffFlag
        {
            get;
            set;
        }
        public bool AoaOffFlag
        {
            get;
            set;
        }
        public bool VviOffFlag
        {
            get;
            set;
        }
        public bool CpdPowerOnFlag
        {
            get;
            set;
        }
        public bool PfdOffFlag
        {
            get;
            set;
        }
        public bool MarkerBeaconOuterMarkerFlag
        {
            get;
            set;
        }
        public bool MarkerBeaconMiddleMarkerFlag
        {
            get;
            set;
        }
    }
}
