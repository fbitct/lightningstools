using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.Math;
using Common.SimSupport;
using F4SharedMem;
using F4SharedMem.Headers;
using Util = F4Utils.Process.Util;
using log4net;
using System.Text;

namespace F4Utils.SimSupport
{
    public class Falcon4SimSupportModule : SimSupportModule
    {
        public const float DEGREES_PER_RADIAN = 57.2957795f;
        public const float FEET_PER_SECOND_PER_KNOT = 1.68780986f;
        private const float GLIDESLOPE_DEVIATION_LIMIT_DEGREES = 1.0F;
        private const float LOCALIZER_DEVIATION_LIMIT_DEGREES = 5.0F;
        private const float SIDESLIP_ANGLE_LIMIT_DEGREES = 5;
        private readonly Dictionary<string, ISimOutput> _simOutputs = new Dictionary<string, ISimOutput>();
        private FlightData _lastFlightData;
        private Reader _smReader;
        private IIndicatedRateOfTurnCalculator _rateOfTurnCalculator = new IndicatedRateOfTurnCalculator();
        private static readonly ILog _log = LogManager.GetLogger (typeof(Falcon4SimSupportModule));
        private MorseCode _morseCodeGenerator = new MorseCode();
        private bool _disposed = false;
        private bool _markerBeaconMorseCodeState = false;
        public Falcon4SimSupportModule()
        {
            _morseCodeGenerator.CharactersPerMinute = 53;
            _morseCodeGenerator.UnitTimeTick += _morseCodeGenerator_UnitTimeTick;
            EnsureSharedmemReaderIsCreated();
            CreateSimOutputsList();
        }

        public override string FriendlyName
        {
            get { return "Falcon BMS"; }
        }

        private DateTime? _lastFalconSimRunningCheckTime;
        private bool _falconWasRunningOnLastCheck = false;
        public override bool IsSimRunning
        {
            get
            {
                if (!_lastFalconSimRunningCheckTime.HasValue ||  DateTime.Now.Subtract(_lastFalconSimRunningCheckTime.Value).Duration().TotalMilliseconds > 500)
                {
                    _lastFalconSimRunningCheckTime = DateTime.Now;
                    _falconWasRunningOnLastCheck = Util.IsFalconRunning();
                }
                return _falconWasRunningOnLastCheck; 
            }
        }

        public override Dictionary<string, ISimOutput> SimOutputs
        {
            get { return _simOutputs; }
        }

        public override Dictionary<string, SimCommand> SimCommands
        {
            get { return new Dictionary<string, SimCommand>(); }
        }

        private void EnsureSharedmemReaderIsCreated()
        {
           
            if (_smReader == null || !_smReader.IsFalconRunning)
            {
                Common.Util.DisposeObject(_smReader);
                _smReader = new Reader();
            }
        }

        private void GetNextFlightDataFromSharedMem()
        {
            if (_smReader == null) return;
            _lastFlightData = _smReader.GetCurrentData();
        }


        private void UpdateSimOutputValues()
        {

            if (_simOutputs == null) return;
            GetNextFlightDataFromSharedMem();
            var showToFromFlag = false;
            var showCommandBars = false;
            DetermineWhetherToShowILSCommandBarsAndToFromFlags(_lastFlightData, out showToFromFlag, out showCommandBars);
            float courseDeviationDegrees;
            float deviationLimitDegrees;
            UpdateHsiData(_lastFlightData, out courseDeviationDegrees, out deviationLimitDegrees);

            if (_lastFlightData == null) return;
            foreach (var output in _simOutputs.Values)
            {
                F4SimOutputs? simOutputEnumMatch = null;
                F4SimOutputs triedParse;
                var key = ((Signal) output).Id;
                var firstBracketLocation = key.IndexOf("[", StringComparison.OrdinalIgnoreCase);
                if (firstBracketLocation > 0)
                {
                    key = key.Substring(0, firstBracketLocation);
                }
                var success = Common.Util.EnumTryParse(key.Substring(3, key.Length - 3), out triedParse);
                if (success) simOutputEnumMatch = triedParse;
                if (!simOutputEnumMatch.HasValue) continue;
                switch (simOutputEnumMatch.Value)
                {
                    case F4SimOutputs.MAP__GROUND_POSITION__FEET_NORTH_OF_MAP_ORIGIN:
                        ((AnalogSignal) output).State = _lastFlightData.x;
                        break;
                    case F4SimOutputs.MAP__GROUND_POSITION__FEET_EAST_OF_MAP_ORIGIN:
                        ((AnalogSignal) output).State = _lastFlightData.y;
                        break;
                    case F4SimOutputs.MAP__GROUND_SPEED_VECTOR__NORTH_COMPONENT_FPS:
                        ((AnalogSignal) output).State = _lastFlightData.xDot;
                        break;
                    case F4SimOutputs.MAP__GROUND_SPEED_VECTOR__EAST_COMPONENT_FPS:
                        ((AnalogSignal) output).State = _lastFlightData.yDot;
                        break;
                    case F4SimOutputs.MAP__GROUND_SPEED_KNOTS:
                        ((AnalogSignal) output).State =
                            Math.Sqrt((_lastFlightData.xDot*_lastFlightData.xDot) +
                                      (_lastFlightData.yDot*_lastFlightData.yDot))/FEET_PER_SECOND_PER_KNOT;
                        break;
                    case F4SimOutputs.ALTIMETER__INDICATED_ALTITUDE__MSL:
                        ((AnalogSignal) output).State = -_lastFlightData.aauz;
                        break;
                    case F4SimOutputs.ALTIMETER__BAROMETRIC_PRESSURE_INCHES_HG:
                        ((AnalogSignal)output).State = _lastFlightData.AltCalReading /100.0f;
                        break;
                    case F4SimOutputs.TRUE_ALTITUDE__MSL:
                        ((AnalogSignal) output).State = -_lastFlightData.z;
                        break;
                    case F4SimOutputs.VVI__VERTICAL_VELOCITY_FPM:
                        ((AnalogSignal) output).State = -_lastFlightData.zDot*60;
                        break;
                    case F4SimOutputs.FLIGHT_DYNAMICS__SIDESLIP_ANGLE_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.beta;
                        break;
                    case F4SimOutputs.FLIGHT_DYNAMICS__CLIMBDIVE_ANGLE_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.gamma*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.FLIGHT_DYNAMICS__OWNSHIP_NORMAL_GS:
                        ((AnalogSignal) output).State = _lastFlightData.gs;
                        break;
                    case F4SimOutputs.AIRSPEED_MACH_INDICATOR__MACH_NUMBER:
                        ((AnalogSignal) output).State = _lastFlightData.mach;
                        break;
                    case F4SimOutputs.AIRSPEED_MACH_INDICATOR__INDICATED_AIRSPEED_KNOTS:
                        ((AnalogSignal) output).State = _lastFlightData.kias;
                        break;
                    case F4SimOutputs.AIRSPEED_MACH_INDICATOR__TRUE_AIRSPEED_KNOTS:
                        ((AnalogSignal) output).State = _lastFlightData.vt/FEET_PER_SECOND_PER_KNOT;
                        break;
                    case F4SimOutputs.HUD__WIND_DELTA_TO_FLIGHT_PATH_MARKER_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.windOffset*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.NOZ_POS1__NOZZLE_PERCENT_OPEN:
                        ((AnalogSignal) output).State = _lastFlightData.nozzlePos;
                        break;
                    case F4SimOutputs.NOZ_POS2__NOZZLE_PERCENT_OPEN:
                        ((AnalogSignal) output).State = _lastFlightData.nozzlePos2;
                        break;
                    case F4SimOutputs.HYD_PRESSURE_A__PSI:
                        ((AnalogSignal)output).State = _lastFlightData.hydPressureA;
                        break;
                    case F4SimOutputs.HYD_PRESSURE_B__PSI:
                        ((AnalogSignal)output).State = _lastFlightData.hydPressureB;
                        break;

                    case F4SimOutputs.FUEL_QTY__INTERNAL_FUEL_POUNDS:
                        ((AnalogSignal) output).State = _lastFlightData.internalFuel;
                        break;
                    case F4SimOutputs.FUEL_QTY__EXTERNAL_FUEL_POUNDS:
                        ((AnalogSignal) output).State = _lastFlightData.externalFuel;
                        break;
                    case F4SimOutputs.FUEL_FLOW1__FUEL_FLOW_POUNDS_PER_HOUR:
                        ((AnalogSignal) output).State = _lastFlightData.fuelFlow;
                        break;
                    case F4SimOutputs.FUEL_FLOW2__FUEL_FLOW_POUNDS_PER_HOUR:
                        ((AnalogSignal)output).State = _lastFlightData.fuelFlow2;
                        break;
                    case F4SimOutputs.RPM1__RPM_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.rpm;
                        break;
                    case F4SimOutputs.RPM2__RPM_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.rpm2;
                        break;
                    case F4SimOutputs.FTIT1__FTIT_TEMP_DEG_CELCIUS:
                        ((AnalogSignal) output).State = _lastFlightData.ftit * 100;
                        break;
                    case F4SimOutputs.FTIT2__FTIT_TEMP_DEG_CELCIUS:
                        ((AnalogSignal) output).State = _lastFlightData.ftit2 * 100;
                        break;
                    case F4SimOutputs.SPEED_BRAKE__POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.speedBrake;
                        break;
                    case F4SimOutputs.SPEED_BRAKE__NOT_STOWED_FLAG:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3) 
                            & LightBits3.SpeedBrake) == LightBits3.SpeedBrake;
                        break;
                    case F4SimOutputs.EPU_FUEL__EPU_FUEL_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.epuFuel;
                        break;
                    case F4SimOutputs.OIL_PRESS1__OIL_PRESS_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.oilPressure;
                        break;
                    case F4SimOutputs.OIL_PRESS2__OIL_PRESS_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.oilPressure2;
                        break;
                    case F4SimOutputs.CABIN_PRESS__CABIN_PRESS_FEET_MSL:
                        ((AnalogSignal)output).State = _lastFlightData.cabinAlt;
                        break;
                    case F4SimOutputs.COMPASS__MAGNETIC_HEADING_DEGREES:
                        ((AnalogSignal)output).State = (360 + (_lastFlightData.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                        break;
                    case F4SimOutputs.GEAR_PANEL__GEAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.gearPos;
                        break;
                    case F4SimOutputs.GEAR_PANEL__NOSE_GEAR_DOWN_LIGHT:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.NoseGearDown) == LightBits3.NoseGearDown);
                        break;
                    case F4SimOutputs.GEAR_PANEL__LEFT_GEAR_DOWN_LIGHT:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.LeftGearDown) == LightBits3.LeftGearDown);
                        break;
                    case F4SimOutputs.GEAR_PANEL__RIGHT_GEAR_DOWN_LIGHT:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.RightGearDown) == LightBits3.RightGearDown);
                        break;
                    case F4SimOutputs.GEAR_PANEL__NOSE_GEAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.NoseGearPos;
                        break;
                    case F4SimOutputs.GEAR_PANEL__LEFT_GEAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.LeftGearPos;
                        break;
                    case F4SimOutputs.GEAR_PANEL__RIGHT_GEAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.RightGearPos;
                        break;
                    case F4SimOutputs.GEAR_PANEL__GEAR_HANDLE_LIGHT:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.GEARHANDLE) == LightBits2.GEARHANDLE);
                        break;
                    case F4SimOutputs.GEAR_PANEL__PARKING_BRAKE_ENGAGED_FLAG:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.ParkBrakeOn) == LightBits3.ParkBrakeOn);
                        break;
                    case F4SimOutputs.ADI__PITCH_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.pitch*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.ADI__ROLL_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.roll*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.ADI__ILS_SHOW_COMMAND_BARS:
                        ((DigitalSignal)output).State = showCommandBars;
                        break;
                    case F4SimOutputs.ADI__ILS_HORIZONTAL_BAR_POSITION:
                        ((AnalogSignal)output).State = (_lastFlightData.AdiIlsVerPos * DEGREES_PER_RADIAN) / GLIDESLOPE_DEVIATION_LIMIT_DEGREES;
                        break;
                    case F4SimOutputs.ADI__ILS_VERTICAL_BAR_POSITION:
                        ((AnalogSignal)output).State =(_lastFlightData.AdiIlsHorPos * DEGREES_PER_RADIAN) / LOCALIZER_DEVIATION_LIMIT_DEGREES;
                        break;
                    case F4SimOutputs.ADI__RATE_OF_TURN_INDICATOR_POSITION:
                        var rateOfTurn = _rateOfTurnCalculator.DetermineIndicatedRateOfTurn(_lastFlightData.yaw * DEGREES_PER_RADIAN);
                        var percentDeflection = rateOfTurn / IndicatedRateOfTurnCalculator.MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 1.5f;
                        if (percentDeflection > 1.0f) percentDeflection = 1.0f;
                        if (percentDeflection < -1.0f) percentDeflection = -1.0f;
                        ((AnalogSignal)output).State = percentDeflection;
                        break;
                    case F4SimOutputs.ADI__INCLINOMETER_POSITION:
                        ((AnalogSignal)output).State = _lastFlightData.beta / SIDESLIP_ANGLE_LIMIT_DEGREES;
                        break;

                    case F4SimOutputs.ADI__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.ADI_OFF) ==
                                                          HsiBits.ADI_OFF);
                        break;
                    case F4SimOutputs.ADI__AUX_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.ADI_AUX) ==
                                                          HsiBits.ADI_AUX);
                        break;
                    case F4SimOutputs.ADI__GS_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.ADI_GS) ==
                                                          HsiBits.ADI_GS);
                        break;
                    case F4SimOutputs.ADI__LOC_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.ADI_LOC) ==
                                                          HsiBits.ADI_LOC);
                        break;
                    case F4SimOutputs.STBY_ADI__PITCH_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.pitch*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.STBY_ADI__ROLL_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.roll*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.STBY_ADI__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.BUP_ADI_OFF) ==
                                                          HsiBits.BUP_ADI_OFF);
                        break;
                    case F4SimOutputs.VVI__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.VVI) ==
                                                          HsiBits.VVI);
                        break;
                    case F4SimOutputs.AOA_INDICATOR__AOA_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.alpha;
                        break;
                    case F4SimOutputs.AOA_INDICATOR__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.AOA) ==
                                                          HsiBits.AOA);
                        break;


                    case F4SimOutputs.HSI__COURSE_DEVIATION_INVALID_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.IlsWarning) ==
                                                          HsiBits.IlsWarning);
                        break;
                    case F4SimOutputs.HSI__DISTANCE_INVALID_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.CourseWarning) ==
                                                          HsiBits.CourseWarning);
                        break;
                    case F4SimOutputs.HSI__DESIRED_COURSE_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.desiredCourse;
                        break;
                    case F4SimOutputs.HSI__COURSE_DEVIATION_DEGREES:
                        ((AnalogSignal) output).State = courseDeviationDegrees;
                        break;
                    case F4SimOutputs.HSI__COURSE_DEVIATION_LIMIT_DEGREES:
                        ((AnalogSignal)output).State = deviationLimitDegrees;
                        break;
                    case F4SimOutputs.HSI__DISTANCE_TO_BEACON_NAUTICAL_MILES:
                        ((AnalogSignal) output).State = _lastFlightData.distanceToBeacon;
                        break;
                    case F4SimOutputs.HSI__BEARING_TO_BEACON_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.bearingToBeacon;
                        break;
                    case F4SimOutputs.HSI__CURRENT_HEADING_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.currentHeading;
                        break;
                    case F4SimOutputs.HSI__DESIRED_HEADING_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.desiredHeading;
                        break;
                    case F4SimOutputs.HSI__LOCALIZER_COURSE_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.localizerCourse;
                        break;
                    case F4SimOutputs.HSI__AIRBASE_FEET_NORTH_OF_MAP_ORIGIN:
                        ((AnalogSignal)output).State = _lastFlightData.airbaseX;
                        break;
                    case F4SimOutputs.HSI__AIRBASE_FEET_EAST_OF_MAP_ORIGIN:
                        ((AnalogSignal)output).State = _lastFlightData.airbaseY;
                        break;
                    case F4SimOutputs.HSI__TO_FLAG:
                        {
                            var myCourseDeviationDecimalDegrees =
                                Common.Math.Util.AngleDelta(_lastFlightData.desiredCourse,
                                                            _lastFlightData.bearingToBeacon);
                            ((DigitalSignal) output).State = Math.Abs(myCourseDeviationDecimalDegrees) <= 90 && showToFromFlag;
                        }
                        break;
                    case F4SimOutputs.HSI__FROM_FLAG:
                        {
                            var myCourseDeviationDecimalDegrees =
                                Common.Math.Util.AngleDelta(_lastFlightData.desiredCourse,
                                                            _lastFlightData.bearingToBeacon);
                            ((DigitalSignal) output).State = Math.Abs(myCourseDeviationDecimalDegrees) > 90 && showToFromFlag;
                        }
                        break;
                    case F4SimOutputs.HSI__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.HSI_OFF) ==
                                                          HsiBits.HSI_OFF);
                        break;
                    case F4SimOutputs.HSI__HSI_MODE:
                        ((AnalogSignal) output).State = _lastFlightData.navMode;
                        break;
                    case F4SimOutputs.TRIM__PITCH_TRIM:
                        ((AnalogSignal) output).State = _lastFlightData.TrimPitch*2;
                        break;
                    case F4SimOutputs.TRIM__ROLL_TRIM:
                        ((AnalogSignal) output).State = _lastFlightData.TrimRoll*2;
                        break;
                    case F4SimOutputs.TRIM__YAW_TRIM:
                        ((AnalogSignal) output).State = _lastFlightData.TrimYaw*2;
                        break;
                    case F4SimOutputs.DED__LINES:
                        {
                            string thisLine = null;
                            if (_lastFlightData.DEDLines != null)
                                thisLine = _lastFlightData.DEDLines[((Signal) output).Index.Value];
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.DED__INVERT_LINES:
                        {
                            string thisLine = null;
                            if (_lastFlightData.Invert != null)
                                thisLine = _lastFlightData.Invert[((Signal) output).Index.Value];
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.PFL__LINES:
                        {
                            string thisLine = null;
                            if (_lastFlightData.PFLLines != null)
                                thisLine = _lastFlightData.PFLLines[((Signal) output).Index.Value];
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.PFL__INVERT_LINES:
                        {
                            string thisLine = null;
                            if (_lastFlightData.PFLInvert != null)
                                thisLine = _lastFlightData.PFLInvert[((Signal) output).Index.Value];
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.UFC__TACAN_CHANNEL:
                        ((AnalogSignal) output).State = _lastFlightData.UFCTChan;
                        break;
                    case F4SimOutputs.UFC__TACAN_BAND_IS_X:
                        ((DigitalSignal) output).State = _lastFlightData.UfcTacanIsX;
                        break;
                    case F4SimOutputs.UFC__TACAN_MODE_IS_AA:
                        ((DigitalSignal) output).State = _lastFlightData.UfcTacanIsAA;
                        break;

                    case F4SimOutputs.AUX_COMM__TACAN_CHANNEL:
                        ((AnalogSignal) output).State = _lastFlightData.AUXTChan;
                        break;
                    case F4SimOutputs.AUX_COMM__TACAN_BAND_IS_X:
                        ((DigitalSignal) output).State = _lastFlightData.AuxTacanIsX;
                        break;
                    case F4SimOutputs.AUX_COMM__TACAN_MODE_IS_AA:
                        ((DigitalSignal) output).State = _lastFlightData.AuxTacanIsAA;
                        break;
                    case F4SimOutputs.AUX_COMM__UHF_PRESET:
                        ((AnalogSignal)output).State = _lastFlightData.BupUhfPreset;
                        break;
                    case F4SimOutputs.AUX_COMM__UHF_FREQUENCY:
                        ((AnalogSignal)output).State = _lastFlightData.BupUhfFreq;
                        break;

                    case F4SimOutputs.FUEL_QTY__FOREWARD_QTY_LBS:
                        ((AnalogSignal) output).State = _lastFlightData.fwd;
                        break;
                    case F4SimOutputs.FUEL_QTY__AFT_QTY_LBS:
                        ((AnalogSignal) output).State = _lastFlightData.aft;
                        break;
                    case F4SimOutputs.FUEL_QTY__TOTAL_FUEL_LBS:
                        ((AnalogSignal) output).State = _lastFlightData.total;
                        break;
                    case F4SimOutputs.LMFD__OSB_LABEL_LINES1:
                        {
                            string thisLine = null;
                            if (_lastFlightData.leftMFD != null)
                                thisLine = _lastFlightData.leftMFD[((Signal) output).Index.Value].Line1;
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.LMFD__OSB_LABEL_LINES2:
                        {
                            string thisLine = null;
                            if (_lastFlightData.leftMFD != null)
                                thisLine = _lastFlightData.leftMFD[((Signal) output).Index.Value].Line2;
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.LMFD__OSB_INVERTED_FLAGS:
                        {
                            var thisLine = false;
                            if (_lastFlightData.leftMFD != null)
                                thisLine = _lastFlightData.leftMFD[((Signal) output).Index.Value].Inverted;
                            ((DigitalSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.RMFD__OSB_LABEL_LINES1:
                        {
                            string thisLine = null;
                            if (_lastFlightData.rightMFD != null)
                                thisLine = _lastFlightData.rightMFD[((Signal) output).Index.Value].Line1;
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.RMFD__OSB_LABEL_LINES2:
                        {
                            string thisLine = null;
                            if (_lastFlightData.rightMFD != null)
                                thisLine = _lastFlightData.rightMFD[((Signal) output).Index.Value].Line2;
                            ((TextSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.RMFD__OSB_INVERTED_FLAGS:
                        {
                            var thisLine = false;
                            if (_lastFlightData.rightMFD != null)
                                thisLine = _lastFlightData.rightMFD[((Signal) output).Index.Value].Inverted;
                            ((DigitalSignal) output).State = thisLine;
                        }
                        break;
                    case F4SimOutputs.MASTER_CAUTION_LIGHT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits &
                                                           LightBits.MasterCaution) == LightBits.MasterCaution);
                        break;
                    case F4SimOutputs.MASTER_CAUTION_ANNOUNCED:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 &
                                                           LightBits3.MCAnnounced) == LightBits3.MCAnnounced);
                        break;
                    case F4SimOutputs.LEFT_EYEBROW_LIGHTS__TFFAIL:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.TF) ==
                                                          LightBits.TF);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__ENGFIRE:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.ENG_FIRE) ==
                                                          LightBits.ENG_FIRE);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__ENGINE:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.ENGINE) ==
                                                          LightBits2.ENGINE);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__HYDOIL:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.HYD) ==
                                                          LightBits.HYD);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__FLCS:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.FLCS) ==
                                                          LightBits.FLCS);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__CANOPY:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.CAN) ==
                                                          LightBits.CAN);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__TO_LDG_CONFIG:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.T_L_CFG) ==
                                                          LightBits.T_L_CFG);
                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__OXY_LOW:
                        ((DigitalSignal)output).State =
                                                       (
                                                        (((LightBits)_lastFlightData.lightBits & LightBits.OXY_BROW) ==
                                                          LightBits.OXY_BROW)
                                                                ||
                                                            (
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.OXY_BROW) ==
                                                              BlinkBits.OXY_BROW) 
                                                                    &&
                                                                DateTime.Now.Millisecond %500 <250
                                                            )
                                                        );

                        break;
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__DBU_ON:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 & LightBits3.DbuWarn) ==
                                                          LightBits3.DbuWarn);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__FLCS_FAULT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits &
                                                           LightBits.FltControlSys) == LightBits.FltControlSys);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__LE_FLAPS:
                        ((DigitalSignal) output).State =
                            (((LightBits) _lastFlightData.lightBits & LightBits.LEFlaps) == LightBits.LEFlaps)
                            ||
                            (((LightBits3) _lastFlightData.lightBits3 & LightBits3.Lef_Fault) == LightBits3.Lef_Fault);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__ELEC_SYS:
                        ((DigitalSignal) output).State = 
                                                        (
                                                            (((LightBits3) _lastFlightData.lightBits3 &
                                                               LightBits3.Elec_Fault) == LightBits3.Elec_Fault)
                                                                ||
                                                            (
                                                                (((BlinkBits) _lastFlightData.blinkBits &
                                                                   BlinkBits.Elec_Fault) == BlinkBits.Elec_Fault)
                                                                        &&
                                                                DateTime.Now.Millisecond % 250 < 125
                                                            )
                                                        );
                        break;
                    case F4SimOutputs.CAUTION_PANEL__ENGINE_FAULT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.EngineFault) ==
                                                          LightBits.EngineFault);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__SEC:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.SEC) ==
                                                          LightBits2.SEC);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__FWD_FUEL_LOW:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.FwdFuelLow) == LightBits2.FwdFuelLow);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__AFT_FUEL_LOW:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.AftFuelLow) == LightBits2.AftFuelLow);
                        //TODO: what about standalone Fuel Low bit?
                        break;
                    case F4SimOutputs.CAUTION_PANEL__OVERHEAT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.Overheat) ==
                                                          LightBits.Overheat);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__BUC:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.BUC) ==
                                                          LightBits2.BUC);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__FUEL_OIL_HOT:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.FUEL_OIL_HOT) == LightBits2.FUEL_OIL_HOT);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__SEAT_NOT_ARMED:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.SEAT_ARM) ==
                                                          LightBits2.SEAT_ARM);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__AVIONICS_FAULT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.Avionics) ==
                                                          LightBits.Avionics);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__RADAR_ALT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.RadarAlt) ==
                                                          LightBits.RadarAlt);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__EQUIP_HOT:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.EQUIP_HOT) ==
                                                          LightBits.EQUIP_HOT);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__ECM:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.ECM) ==
                                                          LightBits.ECM);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__STORES_CONFIG:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.CONFIG) ==
                                                          LightBits.CONFIG);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__ANTI_SKID:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.ANTI_SKID) == LightBits2.ANTI_SKID);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__HOOK:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.Hook) ==
                                                          LightBits.Hook);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__NWS_FAIL:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.NWSFail) ==
                                                          LightBits.NWSFail);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__CABIN_PRESS:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.CabinPress) ==
                                                          LightBits.CabinPress);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__OXY_LOW:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.OXY_LOW) ==
                                                          LightBits2.OXY_LOW);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__PROBE_HEAT:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.PROBEHEAT) == LightBits2.PROBEHEAT) 
                                                                ||
                                                        (
                                                           (((BlinkBits)_lastFlightData.blinkBits &
                                                            BlinkBits.PROBEHEAT) == BlinkBits.PROBEHEAT) 
                                                                && 
                                                           DateTime.Now.Millisecond % 250 < 125
                                                        );
                        break;
                    case F4SimOutputs.CAUTION_PANEL__FUEL_LOW:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.FuelLow) ==
                                                          LightBits.FuelLow);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__IFF:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.IFF) ==
                                                          LightBits.IFF);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__C_ADC:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.cadc) == LightBits3.cadc);
                        break;
                    case F4SimOutputs.CAUTION_PANEL__ATF_NOT_ENGAGED:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 &
                                                           LightBits3.ATF_Not_Engaged) == LightBits3.ATF_Not_Engaged);
                        break;
                    case F4SimOutputs.AOA_INDEXER__AOA_TOO_HIGH:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.AOAAbove) ==
                                                          LightBits.AOAAbove);
                        break;
                    case F4SimOutputs.AOA_INDEXER__AOA_IDEAL:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.AOAOn) ==
                                                          LightBits.AOAOn);
                        break;
                    case F4SimOutputs.AOA_INDEXER__AOA_TOO_LOW:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.AOABelow) ==
                                                          LightBits.AOABelow);
                        break;
                    case F4SimOutputs.NWS_INDEXER__RDY:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.RefuelRDY) ==
                                                          LightBits.RefuelRDY);
                        break;
                    case F4SimOutputs.NWS_INDEXER__AR_NWS:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.RefuelAR) ==
                                                          LightBits.RefuelAR);
                        break;
                    case F4SimOutputs.NWS_INDEXER__DISC:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.RefuelDSC) ==
                                                          LightBits.RefuelDSC);
                        break;
                    case F4SimOutputs.TWP__HANDOFF:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.HandOff) ==
                                                          LightBits2.HandOff);
                        break;
                    case F4SimOutputs.TWP__MISSILE_LAUNCH:
                        ((DigitalSignal)output).State = (((LightBits2)_lastFlightData.lightBits2 & LightBits2.Launch) ==
                                                          LightBits2.Launch)
                                                            ||
                                                        (
                                                          (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.Launch) ==
                                                          BlinkBits.Launch) 
                                                              && 
                                                          DateTime.Now.Millisecond % 250 < 125
                                                        );
                        break;
                    case F4SimOutputs.TWP__PRIORITY_MODE:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.PriMode) ==
                                                          LightBits2.PriMode)
                                                          ||
                                                          (
                                                              (((LightBits2)_lastFlightData.lightBits2 & LightBits2.PriMode) ==
                                                              LightBits2.PriMode)
                                                                &&
                                                              (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.PriMode) ==
                                                              BlinkBits.PriMode)
                                                                &&
                                                              DateTime.Now.Millisecond % 250 < 125
                                                            );
                        break;
                    case F4SimOutputs.TWP__UNKNOWN:
                        ((DigitalSignal)output).State = (((LightBits2)_lastFlightData.lightBits2 & LightBits2.Unk) ==
                                                          LightBits2.Unk)
                                                          ||
                                                          (
                                                              (((LightBits2)_lastFlightData.lightBits2 & LightBits2.Unk) ==
                                                              LightBits2.Unk)
                                                                &&
                                                              (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.Unk) ==
                                                              BlinkBits.Unk)
                                                                &&
                                                              DateTime.Now.Millisecond % 250 < 125
                                                            );
                        break;
                    case F4SimOutputs.TWP__NAVAL:
                        ((DigitalSignal)output).State = (((LightBits2)_lastFlightData.lightBits2 & LightBits2.Naval) ==
                                                          LightBits2.Naval);
                        break;
                    case F4SimOutputs.TWP__TARGET_SEP:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.TgtSep) ==
                                                          LightBits2.TgtSep);
                        break;
                    case F4SimOutputs.TWP__SYS_TEST:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 & LightBits3.SysTest) ==
                                                          LightBits3.SysTest);
                        break;
                    case F4SimOutputs.TWA__SEARCH:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.AuxSrch) ==
                                                          LightBits2.AuxSrch) 
                                                                ||
                                                        (
                                                          (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.AuxSrch) ==
                                                          BlinkBits.AuxSrch) 
                                                            && 
                                                          DateTime.Now.Millisecond % 250 < 125
                                                        );
                        break;
                    case F4SimOutputs.TWA__ACTIVITY_POWER:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.AuxAct) ==
                                                          LightBits2.AuxAct);
                        break;
                    case F4SimOutputs.TWA__LOW_ALTITUDE:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.AuxLow) ==
                                                          LightBits2.AuxLow);
                        break;
                    case F4SimOutputs.TWA__SYSTEM_POWER:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.AuxPwr) ==
                                                          LightBits2.AuxPwr);
                        break;
                    case F4SimOutputs.ECM__POWER:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.EcmPwr) ==
                                                          LightBits2.EcmPwr);
                        break;
                    case F4SimOutputs.ECM__FAIL:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.EcmFail) ==
                                                          LightBits2.EcmFail);
                        break;
                    case F4SimOutputs.MISC__ADV_MODE_ACTIVE:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 &
                                                           LightBits2.TFR_ENGAGED) == LightBits2.TFR_ENGAGED);
                        break;
                    case F4SimOutputs.MISC__ADV_MODE_STBY:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.TFR_STBY) ==
                                                          LightBits.TFR_STBY);
                        break;
                    case F4SimOutputs.MISC__AUTOPILOT_ENGAGED:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.AutoPilotOn) ==
                                                          LightBits.AutoPilotOn);
                        break;
                    case F4SimOutputs.CHAFF_FLARE__CHAFF_COUNT:
                        ((AnalogSignal) output).State = _lastFlightData.ChaffCount;
                        break;
                    case F4SimOutputs.CHAFF_FLARE__FLARE_COUNT:
                        ((AnalogSignal) output).State = _lastFlightData.FlareCount;
                        break;
                    case F4SimOutputs.CMDS__GO:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Go) ==
                                                          LightBits2.Go);
                        break;
                    case F4SimOutputs.CMDS__NOGO:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.NoGo) ==
                                                          LightBits2.NoGo);
                        break;
                    case F4SimOutputs.CMDS__AUTO_DEGR:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Degr) ==
                                                          LightBits2.Degr);
                        break;
                    case F4SimOutputs.CMDS__DISPENSE_RDY:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Rdy) ==
                                                          LightBits2.Rdy);
                        break;
                    case F4SimOutputs.CMDS__CHAFF_LO:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.ChaffLo) ==
                                                          LightBits2.ChaffLo);
                        break;
                    case F4SimOutputs.CMDS__FLARE_LO:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.FlareLo) ==
                                                          LightBits2.FlareLo);
                        break;
                    case F4SimOutputs.CMDS__MODE:
                        ((AnalogSignal)output).State = _lastFlightData.cmdsMode;
                        break;
                    case F4SimOutputs.ELEC__FLCS_PMG:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.FlcsPmg) ==
                                                          LightBits3.FlcsPmg);
                        break;
                    case F4SimOutputs.ELEC__MAIN_GEN:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.MainGen) ==
                                                          LightBits3.MainGen);
                        break;
                    case F4SimOutputs.ELEC__STBY_GEN:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.StbyGen) ==
                                                          LightBits3.StbyGen);
                        break;
                    case F4SimOutputs.ELEC__EPU_GEN:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.EpuGen) ==
                                                          LightBits3.EpuGen);
                        break;
                    case F4SimOutputs.ELEC__EPU_PMG:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.EpuPmg) ==
                                                          LightBits3.EpuPmg);
                        break;
                    case F4SimOutputs.ELEC__TO_FLCS:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.ToFlcs) ==
                                                          LightBits3.ToFlcs);
                        break;
                    case F4SimOutputs.ELEC__FLCS_RLY:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.FlcsRly) ==
                                                          LightBits3.FlcsRly);
                        break;
                    case F4SimOutputs.ELEC__BATT_FAIL:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.BatFail) ==
                                                          LightBits3.BatFail);
                        break;
                    case F4SimOutputs.TEST__ABCD:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.Flcs_ABCD) ==
                                                          LightBits.Flcs_ABCD);
                        break;
                    case F4SimOutputs.EPU__HYDRAZN:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.Hydrazine) == LightBits3.Hydrazine);
                        break;
                    case F4SimOutputs.EPU__AIR:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.Air) ==
                                                          LightBits3.Air);
                        break;
                    case F4SimOutputs.EPU__RUN:
                        ((DigitalSignal)output).State =
                                                       (
                                                        (((LightBits2)_lastFlightData.lightBits2 & LightBits2.EPUOn) ==
                                                          LightBits2.EPUOn)
                                                                ||
                                                            (
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.EPUOn) ==
                                                              BlinkBits.EPUOn)
                                                                &&
                                                                DateTime.Now.Millisecond % 250 < 125
                                                            )
                                                        );
                        break;
                    case F4SimOutputs.AVTR__AVTR:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.AVTR) ==
                                                          HsiBits.AVTR);
                        break;
                    case F4SimOutputs.JFS__RUN:
                        ((DigitalSignal)output).State =
                                                    (
                                                        (((LightBits2)_lastFlightData.lightBits2 & LightBits2.JFSOn) ==
                                                          LightBits2.JFSOn)
                                                                ||
                                                        (
                                                            (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.JFSOn_Slow) ==
                                                              BlinkBits.JFSOn_Slow)
                                                                &&
                                                            DateTime.Now.Millisecond % 1000 < 500
                                                        )
                                                                ||
                                                        (
                                                            (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.JFSOn_Fast) ==
                                                              BlinkBits.JFSOn_Fast)
                                                                &&
                                                            DateTime.Now.Millisecond % 250 < 125
                                                        )
                                                    );
                        break;
                    case F4SimOutputs.MARKER_BEACON__MRK_BCN_LIGHT:
                        if (
                            (((HsiBits)_lastFlightData.hsiBits & HsiBits.OuterMarker) ==
                                                          HsiBits.OuterMarker)
                        )
                        {
                            _morseCodeGenerator.PlainText = "T";
                        }
                        else if ((((HsiBits)_lastFlightData.hsiBits & HsiBits.MiddleMarker) ==
                                                          HsiBits.MiddleMarker))
                        {
                            _morseCodeGenerator.PlainText = "A";
                        }
                        

                        ((DigitalSignal)output).State = (
                                                             //either outer or middle marker blink bit is set and morse code signal state is ON
                                                            (
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.OuterMarker) ==
                                                                  BlinkBits.OuterMarker)
                                                                    ||
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.MiddleMarker) ==
                                                                  BlinkBits.MiddleMarker)
                                                            )
                                                                  && _markerBeaconMorseCodeState
                                                        ) 
                                                             ||
                                                        (
                                                                //neither outer or middle marker blink bit is set but either the standard outer or middle marker bit are set (could be because of test bits)
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.OuterMarker) !=
                                                                  BlinkBits.OuterMarker)
                                                                  &&
                                                                (((BlinkBits)_lastFlightData.blinkBits & BlinkBits.MiddleMarker) !=
                                                                  BlinkBits.MiddleMarker)
                                                                  && 
                                                                  (
                                                                      (((HsiBits)_lastFlightData.hsiBits & HsiBits.OuterMarker) ==
                                                                        HsiBits.OuterMarker)
                                                                        ||
                                                                      (((HsiBits)_lastFlightData.hsiBits & HsiBits.MiddleMarker) ==
                                                                        HsiBits.MiddleMarker)
                                                                  )
                                                        );
                        break;
                    case F4SimOutputs.MARKER_BEACON__OUTER_MARKER_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.OuterMarker) ==
                                                          HsiBits.OuterMarker);

                        break;
                    case F4SimOutputs.MARKER_BEACON__MIDDLE_MARKER_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.MiddleMarker) ==
                                                          HsiBits.MiddleMarker);
                        break;
                    case F4SimOutputs.AIRCRAFT__ONGROUND:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.ONGROUND) ==
                                                          LightBits.ONGROUND) 
                                                          ||
                                                           (((LightBits3)_lastFlightData.lightBits3 & LightBits3.OnGround) ==
                                                          LightBits3.OnGround);
                        break;
                    case F4SimOutputs.AIRCRAFT__MAIN_LANDING_GEAR__WEIGHT_ON_WHEELS:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 & LightBits3.MLGWOW) ==
                                                          LightBits3.MLGWOW);
                        break;
                    case F4SimOutputs.AIRCRAFT__NOSE_LANDING_GEAR__WEIGHT_ON_WHEELS:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 & LightBits3.NLGWOW) ==
                                                          LightBits3.NLGWOW);
                        break;
                    case F4SimOutputs.AIRCRAFT__LEADING_EDGE_FLAPS_POSITION:
                        ((AnalogSignal)output).State = _lastFlightData.lefPos;
                        break;
                    case F4SimOutputs.AIRCRAFT__TRAILING_EDGE_FLAPS_POSITION:
                        ((AnalogSignal)output).State = _lastFlightData.tefPos;
                        break;
                    case F4SimOutputs.AIRCRAFT__VTOL_POSITION:
                        ((AnalogSignal)output).State = _lastFlightData.vtolPos;
                        break;

                    case F4SimOutputs.POWER__ELEC_POWER_OFF:
                        ((DigitalSignal)output).State = (((LightBits3)_lastFlightData.lightBits3 &
                                                           LightBits3.Power_Off) == LightBits3.Power_Off);
                        break;
                    case F4SimOutputs.POWER__MAIN_POWER:
                        ((AnalogSignal)output).State = _lastFlightData.MainPower;
                        break;
                    case F4SimOutputs.POWER__BUS_POWER_BATTERY:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.BusPowerBattery) == PowerBits.BusPowerBattery);
                        break;
                    case F4SimOutputs.POWER__BUS_POWER_EMERGENCY:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.BusPowerEmergency) == PowerBits.BusPowerEmergency);
                        break;
                    case F4SimOutputs.POWER__BUS_POWER_ESSENTIAL:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.BusPowerEssential) == PowerBits.BusPowerEssential);
                        break;
                    case F4SimOutputs.POWER__BUS_POWER_NON_ESSENTIAL:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.BusPowerNonEssential) == PowerBits.BusPowerNonEssential);
                        break;
                    case F4SimOutputs.POWER__MAIN_GENERATOR:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.MainGenerator) == PowerBits.MainGenerator);
                        break;
                    case F4SimOutputs.POWER__STANDBY_GENERATOR:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.StandbyGenerator) == PowerBits.StandbyGenerator);
                        break;
                    case F4SimOutputs.POWER__JET_FUEL_STARTER:
                        ((DigitalSignal)output).State = (((PowerBits)_lastFlightData.powerBits &
                                                           PowerBits.JetFuelStarter) == PowerBits.JetFuelStarter);
                        break;

                    case F4SimOutputs.SIM__BMS_PLAYER_IS_FLYING:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.Flying) ==
                                                          HsiBits.Flying);
                        break;
                    case F4SimOutputs.SIM__FLIGHTDATA_VERSION_NUM:
                        ((AnalogSignal) output).State = _lastFlightData.VersionNum;
                        break;
                    case F4SimOutputs.SIM__FLIGHTDATA2_VERSION_NUM:
                        ((AnalogSignal)output).State = _lastFlightData.VersionNum2;
                        break;
                    case F4SimOutputs.SIM__CURRENT_TIME:
                        ((AnalogSignal)output).State = _lastFlightData.currentTime;
                        break;
                    case F4SimOutputs.SIM__VEHICLE_ACD:
                        ((AnalogSignal)output).State = _lastFlightData.vehicleACD;
                        break;
                    case F4SimOutputs.SIM__PILOT_CALLSIGN:
                        {
                            string callsign = null;
                            if (_lastFlightData.pilotsCallsign != null && ((Signal)output).Index.Value <= _lastFlightData.pilotsOnline)
                            {
                                callsign = (_lastFlightData.pilotsCallsign[((Signal)output).Index.Value] ?? string.Empty).TrimEnd();
                            }
                            else
                            {
                                callsign = string.Empty;
                            }
                            ((TextSignal)output).State = callsign;
                        }
                        break;
                    case F4SimOutputs.SIM__PILOT_STATUS:
                        {
                            byte status;
                            if (_lastFlightData.pilotsStatus != null && ((Signal)output).Index.Value <= _lastFlightData.pilotsOnline)
                            {
                                status = _lastFlightData.pilotsStatus[((Signal)output).Index.Value];
                            }
                            else
                            {
                                status = 0;
                            }
                            ((AnalogSignal)output).State = status;
                        }
                        break;
                    case F4SimOutputs.SIM__AA_MISSILE_FIRED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.AAMissileFired;
                        break;
                    case F4SimOutputs.SIM__AG_MISSILE_FIRED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.AGMissileFired;
                        break;
                    case F4SimOutputs.SIM__BOMB_DROPPED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.BombDropped;
                        break;
                    case F4SimOutputs.SIM__FLARE_DROPPED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.FlareDropped;
                        break;
                    case F4SimOutputs.SIM__CHAFF_DROPPED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.ChaffDropped;
                        break;
                    case F4SimOutputs.SIM__BULLETS_FIRED:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.BulletsFired;
                        break;
                    case F4SimOutputs.SIM__COLLISION_COUNTER:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.CollisionCounter;
                        break;
                    case F4SimOutputs.SIM__GFORCE:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.Gforce;
                        break;
                    case F4SimOutputs.SIM__LAST_DAMAGE:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.lastdamage;
                        break;
                    case F4SimOutputs.SIM__DAMAGE_FORCE:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.damageforce;
                        break;
                    case F4SimOutputs.SIM__WHEN_DAMAGE:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.whendamage;
                        break;
                    case F4SimOutputs.SIM__EYE_X:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.eyex;
                        break;
                    case F4SimOutputs.SIM__EYE_Y:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.eyey;
                        break;
                    case F4SimOutputs.SIM__EYE_Z:
                        ((AnalogSignal)output).State = _lastFlightData.IntellivibeData.eyez;
                        break;
                    case F4SimOutputs.SIM__IS_FIRING_GUN:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsFiringGun;
                        break;
                    case F4SimOutputs.SIM__IS_END_FLIGHT:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsEndFlight;
                        break;
                    case F4SimOutputs.SIM__IS_EJECTING:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsEjecting;
                        break;
                    case F4SimOutputs.SIM__IN_3D:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.In3D;
                        break;
                    case F4SimOutputs.SIM__IS_PAUSED:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsPaused;
                        break;
                    case F4SimOutputs.SIM__IS_FROZEN:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsFrozen;
                        break;
                    case F4SimOutputs.SIM__IS_OVER_G:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsOverG;
                        break;
                    case F4SimOutputs.SIM__IS_ON_GROUND:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsOnGround;
                        break;
                    case F4SimOutputs.SIM__IS_EXIT_GAME:
                        ((DigitalSignal)output).State = _lastFlightData.IntellivibeData.IsExitGame;
                        break;

                    case F4SimOutputs.PILOT__HEADX_OFFSET:
                        ((AnalogSignal) output).State = _lastFlightData.headX;
                        break;
                    case F4SimOutputs.PILOT__HEADY_OFFSET:
                        ((AnalogSignal) output).State = _lastFlightData.headY;
                        break;
                    case F4SimOutputs.PILOT__HEADZ_OFFSET:
                        ((AnalogSignal) output).State = _lastFlightData.headZ;
                        break;
                    case F4SimOutputs.PILOT__HEAD_PITCH_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.headPitch;
                        break;
                    case F4SimOutputs.PILOT__HEAD_ROLL_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.headRoll;
                        break;
                    case F4SimOutputs.PILOT__HEAD_YAW_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.headYaw;
                        break;
                    case F4SimOutputs.FLIGHT_CONTROL__RUN:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.FlcsBitRun) == LightBits3.FlcsBitRun);
                        break;
                    case F4SimOutputs.FLIGHT_CONTROL__FAIL:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.FlcsBitFail) == LightBits3.FlcsBitFail);
                        break;
                    case F4SimOutputs.RWR__OBJECT_COUNT:
                        ((AnalogSignal) output).State = _lastFlightData.RwrObjectCount;
                        break;
                    case F4SimOutputs.RWR__SYMBOL_ID:
                        ((AnalogSignal) output).State = _lastFlightData.RWRsymbol.Length > ((Signal) output).Index
                                                            ? _lastFlightData.RWRsymbol[((Signal) output).Index.Value]
                                                            : 0;
                        break;
                    case F4SimOutputs.RWR__BEARING_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.bearing.Length > ((Signal) output).Index
                                                            ? _lastFlightData.bearing[((Signal) output).Index.Value] * DEGREES_PER_RADIAN
                                                            : 0;
                        break;
                    case F4SimOutputs.RWR__MISSILE_ACTIVITY_FLAG:
                        if (_lastFlightData.missileActivity.Length > ((Signal) output).Index)
                        {
                            ((DigitalSignal) output).State =
                                _lastFlightData.missileActivity[((Signal) output).Index.Value] == 1;
                        }
                        else
                        {
                            ((DigitalSignal) output).State = false;
                        }
                        break;
                    case F4SimOutputs.RWR__MISSILE_LAUNCH_FLAG:
                        if (_lastFlightData.missileLaunch.Length > ((Signal) output).Index)
                        {
                            ((DigitalSignal) output).State =
                                _lastFlightData.missileLaunch[((Signal) output).Index.Value] == 1;
                        }
                        else
                        {
                            ((DigitalSignal) output).State = false;
                        }
                        break;
                    case F4SimOutputs.RWR__SELECTED_FLAG:
                        if (_lastFlightData.selected.Length > ((Signal) output).Index)
                        {
                            ((DigitalSignal) output).State = _lastFlightData.selected[((Signal) output).Index.Value] ==
                                                             1;
                        }
                        else
                        {
                            ((DigitalSignal) output).State = false;
                        }
                        break;
                    case F4SimOutputs.RWR__LETHALITY:
                        ((AnalogSignal) output).State = _lastFlightData.lethality.Length > ((Signal) output).Index
                                                            ? _lastFlightData.lethality[((Signal) output).Index.Value]
                                                            : 0;
                        break;
                    case F4SimOutputs.RWR__NEWDETECTION_FLAG:
                        if (_lastFlightData.newDetection != null)
                        {
                            if (_lastFlightData.newDetection.Length > ((Signal) output).Index)
                            {
                                ((DigitalSignal) output).State =
                                    _lastFlightData.newDetection[((Signal) output).Index.Value] == 1;
                            }
                            else
                            {
                                ((DigitalSignal) output).State = false;
                            }
                        }
                        break;
                    case F4SimOutputs.RWR__ADDITIONAL_INFO:
                        {
                            string rwrInfo=string.Empty;
                            if (_lastFlightData.RwrInfo != null)
                            {
                                rwrInfo = Encoding.Default.GetString(_lastFlightData.RwrInfo);
                            }
                            ((TextSignal)output).State = rwrInfo;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private ISimOutput CreateNewF4SimOutput(string collectionName, string friendlyName, int simOutputEnumVal,
                                                int? index, Type dataType)
        {
            var indexString = index >= 0 ? "[" + index + "]" : string.Empty;

            if (dataType.IsAssignableFrom(typeof (string)))
            {
                return new TextSimOutput
                           {
                               CollectionName = collectionName,
                               FriendlyName = friendlyName,
                               Id = "F4_" + Enum.GetName(typeof (F4SimOutputs), simOutputEnumVal) + indexString,
                               Index = index,
                               PublisherObject = this,
                               Source =  (object) _smReader,
                               SourceFriendlyName = "Falcon 4"
                           };
            }
            if (dataType.IsAssignableFrom(typeof (bool)))
            {
                return new DigitalSimOutput
                           {
                               CollectionName = collectionName,
                               FriendlyName = friendlyName,
                               Id = "F4_" + Enum.GetName(typeof (F4SimOutputs), simOutputEnumVal) + indexString,
                               Index = index,
                               PublisherObject = this,
                               Source = (object) _smReader,
                               SourceFriendlyName = "Falcon 4"
                           };
            }
            return new AnalogSimOutput
                       {
                           CollectionName = collectionName,
                           FriendlyName = friendlyName,
                           Id = "F4_" + Enum.GetName(typeof (F4SimOutputs), simOutputEnumVal) + indexString,
                           Index = index,
                           PublisherObject = this,
                           Source = (object) _smReader,
                           SourceFriendlyName = "Falcon 4",
                       };
        }

        private ISimOutput CreateNewF4SimOutput(string collectionName, string friendlyName, int simOutputEnumVal,
                                                Type dataType)
        {
            return CreateNewF4SimOutput(collectionName, friendlyName, simOutputEnumVal, null, dataType);
        }

        private void AddF4SimOutput(ISimOutput output)
        {
            _simOutputs.Add(output.Id, output);
        }

        private void CreateSimOutputsList()
        {
            _simOutputs.Clear();

            AddF4SimOutput(CreateNewF4SimOutput("Map", "Ground Position: Feet North of Map Origin",
                                                (int) F4SimOutputs.MAP__GROUND_POSITION__FEET_NORTH_OF_MAP_ORIGIN,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Map", "Ground Position: Feet East of Map Origin)",
                                                (int) F4SimOutputs.MAP__GROUND_POSITION__FEET_EAST_OF_MAP_ORIGIN,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Map", "Ground Speed Vector: North Component (feet/sec)",
                                                (int) F4SimOutputs.MAP__GROUND_SPEED_VECTOR__NORTH_COMPONENT_FPS,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Map", "Ground Speed Vector: East Component (feet/sec)",
                                                (int) F4SimOutputs.MAP__GROUND_SPEED_VECTOR__EAST_COMPONENT_FPS,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Map", "Ground Speed (knots)",
                                                (int) F4SimOutputs.MAP__GROUND_SPEED_KNOTS, typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("Altimeter", "Indicated Altitude (feet MSL)",
                                                (int) F4SimOutputs.ALTIMETER__INDICATED_ALTITUDE__MSL, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Altimeter", "Barometric Pressure (inches Hg)",
                                               (int)F4SimOutputs.ALTIMETER__BAROMETRIC_PRESSURE_INCHES_HG, typeof(float)));

            AddF4SimOutput(CreateNewF4SimOutput("Vertical Velocity Indicator (VVI)", "Vertical Velocity (feet/min)",
                                                (int) F4SimOutputs.VVI__VERTICAL_VELOCITY_FPM, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Vertical Velocity Indicator (VVI)", "OFF flag",
                                                (int) F4SimOutputs.VVI__OFF_FLAG, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Flight dynamics", "Sideslip angle [beta] (degrees)",
                                                (int) F4SimOutputs.FLIGHT_DYNAMICS__SIDESLIP_ANGLE_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Flight dynamics", "Climb/Dive Angle [gamma] (degrees)",
                                                (int) F4SimOutputs.FLIGHT_DYNAMICS__CLIMBDIVE_ANGLE_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Flight dynamics", "Ownship Normal Gs",
                                                (int) F4SimOutputs.FLIGHT_DYNAMICS__OWNSHIP_NORMAL_GS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Flight dynamics", "True Airspeed (knots)",
                                                (int) F4SimOutputs.AIRSPEED_MACH_INDICATOR__TRUE_AIRSPEED_KNOTS,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Flight dynamics", "True Altitude (feet MSL)",
                                                (int) F4SimOutputs.TRUE_ALTITUDE__MSL, typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("Airspeed indicator/Machmeter", "Mach number",
                                                (int) F4SimOutputs.AIRSPEED_MACH_INDICATOR__MACH_NUMBER, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Airspeed indicator/Machmeter", "Indicated Airspeed (knots)",
                                                (int) F4SimOutputs.AIRSPEED_MACH_INDICATOR__INDICATED_AIRSPEED_KNOTS,
                                                typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("HUD", "Wind delta to flight path marker (degrees)",
                                                (int) F4SimOutputs.HUD__WIND_DELTA_TO_FLIGHT_PATH_MARKER_DEGREES,
                                                typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("NOZ POS 1", "Engine 1 Nozzle Percent Open",
                                                (int) F4SimOutputs.NOZ_POS1__NOZZLE_PERCENT_OPEN, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("NOZ POS 2", "Engine 2 Nozzle Percent Open",
                                                (int) F4SimOutputs.NOZ_POS2__NOZZLE_PERCENT_OPEN, typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("HYD A", "Hydraulic Pressure A (Pounds per Square Inch)",
                                    (int)F4SimOutputs.HYD_PRESSURE_A__PSI, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("HYD B", "Hydraulic Pressure B (Pounds per Square Inch)",
                                                (int)F4SimOutputs.HYD_PRESSURE_B__PSI, typeof(float)));


            AddF4SimOutput(CreateNewF4SimOutput("FUEL FLOW 1", "Fuel flow (pounds/hour)",
                                                (int) F4SimOutputs.FUEL_FLOW1__FUEL_FLOW_POUNDS_PER_HOUR, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FUEL FLOW 2", "Fuel flow (pounds/hour)",
                                                (int)F4SimOutputs.FUEL_FLOW2__FUEL_FLOW_POUNDS_PER_HOUR, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("RPM 1", "Engine 1 RPM (Percent 0-103)",
                                                (int) F4SimOutputs.RPM1__RPM_PERCENT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("RPM 2", "Engine 2 RPM(Percent 0-103)",
                                                (int) F4SimOutputs.RPM2__RPM_PERCENT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FTIT 1", "Engine 1 Forward Turbine Inlet Temp (Degrees C)",
                                                (int) F4SimOutputs.FTIT1__FTIT_TEMP_DEG_CELCIUS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FTIT 2", "Engine 2 Forward Turbine Inlet Temp (Degrees C)",
                                                (int) F4SimOutputs.FTIT2__FTIT_TEMP_DEG_CELCIUS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("SPEED BRAKE", "Speedbrake position (0 = closed, 1 = 60 Degrees open)",
                                                (int) F4SimOutputs.SPEED_BRAKE__POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("SPEED BRAKE", "Speedbrake Not Stowed Flag",
                                                (int)F4SimOutputs.SPEED_BRAKE__NOT_STOWED_FLAG, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("EPU FUEL", "EPU fuel (Percent 0-100)",
                                                (int) F4SimOutputs.EPU_FUEL__EPU_FUEL_PERCENT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("OIL PRESS 1", "Engine 1 Oil Pressure (Percent 0-100)",
                                                (int) F4SimOutputs.OIL_PRESS1__OIL_PRESS_PERCENT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("OIL PRESS 2", "Engine 2 Oil Pressure (Percent 0-100)",
                                                (int) F4SimOutputs.OIL_PRESS2__OIL_PRESS_PERCENT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("CABIN PRESS", "Cabin Pressure Altitude (in Feet MSL)",
                                                (int) F4SimOutputs.CABIN_PRESS__CABIN_PRESS_FEET_MSL, typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("Compass", "Magnetic Heading (degrees)",
                                                (int) F4SimOutputs.COMPASS__MAGNETIC_HEADING_DEGREES, typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Gear position (0 = up, 1 = down)",
                                                (int) F4SimOutputs.GEAR_PANEL__GEAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Nose Gear Down Light",
                                                (int) F4SimOutputs.GEAR_PANEL__NOSE_GEAR_DOWN_LIGHT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Left Gear Down Light",
                                                (int) F4SimOutputs.GEAR_PANEL__LEFT_GEAR_DOWN_LIGHT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Right Gear Down Light",
                                                (int) F4SimOutputs.GEAR_PANEL__RIGHT_GEAR_DOWN_LIGHT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Nose Gear Position",
                                                (int) F4SimOutputs.GEAR_PANEL__NOSE_GEAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Left Gear Position",
                                                (int) F4SimOutputs.GEAR_PANEL__LEFT_GEAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Right Gear Position",
                                                (int) F4SimOutputs.GEAR_PANEL__RIGHT_GEAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Gear handle 'Lollipop' light",
                                                (int) F4SimOutputs.GEAR_PANEL__GEAR_HANDLE_LIGHT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("GEAR Panel", "Parking brake engaged flag",
                                                (int) F4SimOutputs.GEAR_PANEL__PARKING_BRAKE_ENGAGED_FLAG, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Pitch (degrees)", (int) F4SimOutputs.ADI__PITCH_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Roll (degrees)", (int) F4SimOutputs.ADI__ROLL_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Glideslope and localizer ILS command bars enabled flag",
                                                (int)F4SimOutputs.ADI__ILS_SHOW_COMMAND_BARS, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Position of glideslope ILS bar",
                                                (int) F4SimOutputs.ADI__ILS_HORIZONTAL_BAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Position of localizer ILS bar",
                                                (int) F4SimOutputs.ADI__ILS_VERTICAL_BAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Rate of Turn Indicator Position",
                                                (int)F4SimOutputs.ADI__RATE_OF_TURN_INDICATOR_POSITION, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Inclinometer Position",
                                                (int)F4SimOutputs.ADI__INCLINOMETER_POSITION, typeof(float)));
            
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "OFF flag", (int) F4SimOutputs.ADI__OFF_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "AUX flag", (int) F4SimOutputs.ADI__AUX_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "GS flag", (int) F4SimOutputs.ADI__GS_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "LOC flag", (int) F4SimOutputs.ADI__LOC_FLAG, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("STBY ADI", "ADI OFF flag", (int) F4SimOutputs.STBY_ADI__OFF_FLAG,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("STBY ADI", "Pitch (degrees)",
                                                (int) F4SimOutputs.STBY_ADI__PITCH_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("STBY ADI", "Roll (degrees)", (int) F4SimOutputs.STBY_ADI__ROLL_DEGREES,
                                                typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("AOA INDICATOR", "Angle of Attack [alpha] (degrees)",
                                                (int) F4SimOutputs.AOA_INDICATOR__AOA_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("AOA INDICATOR", "OFF flag", (int) F4SimOutputs.AOA_INDICATOR__OFF_FLAG,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Course deviation invalid flag",
                                                (int) F4SimOutputs.HSI__COURSE_DEVIATION_INVALID_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Distance invalid flag",
                                                (int) F4SimOutputs.HSI__DISTANCE_INVALID_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Desired course (degrees)",
                                                (int) F4SimOutputs.HSI__DESIRED_COURSE_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Course deviation (degrees)",
                                                (int) F4SimOutputs.HSI__COURSE_DEVIATION_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Course deviation limit (degrees)",
                                                (int)F4SimOutputs.HSI__COURSE_DEVIATION_LIMIT_DEGREES, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Distance to beacon (nautical miles)",
                                                (int) F4SimOutputs.HSI__DISTANCE_TO_BEACON_NAUTICAL_MILES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Bearing to beacon (degrees)",
                                                (int) F4SimOutputs.HSI__BEARING_TO_BEACON_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Current heading (degrees)",
                                                (int) F4SimOutputs.HSI__CURRENT_HEADING_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Desired heading (degrees)",
                                                (int) F4SimOutputs.HSI__DESIRED_HEADING_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Localizer course (degrees)",
                                                (int) F4SimOutputs.HSI__LOCALIZER_COURSE_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Airbase Position: Feet North of Map Origin",
                                                (int)F4SimOutputs.HSI__AIRBASE_FEET_NORTH_OF_MAP_ORIGIN,
                                                typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Airbase Position: Feet East of Map Origin",
                                                (int)F4SimOutputs.HSI__AIRBASE_FEET_EAST_OF_MAP_ORIGIN,
                                                typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "TO Flag", (int)F4SimOutputs.HSI__TO_FLAG, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "FROM Flag", (int) F4SimOutputs.HSI__FROM_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "OFF Flag", (int) F4SimOutputs.HSI__OFF_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "HSI mode (0=ILS/TCN, 1=TACAN, 2=NAV, 3=ILS/NAV)",
                                                (int) F4SimOutputs.HSI__HSI_MODE, typeof (int)));

            AddF4SimOutput(CreateNewF4SimOutput("Trim", "Pitch trim (-1 to +1)", (int) F4SimOutputs.TRIM__PITCH_TRIM,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Trim", "Roll trim (-1 to +1)", (int) F4SimOutputs.TRIM__ROLL_TRIM,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Trim", "Yaw trim (-1 to +1)", (int) F4SimOutputs.TRIM__YAW_TRIM,
                                                typeof (float)));

            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 1", (int) F4SimOutputs.DED__LINES, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 2", (int) F4SimOutputs.DED__LINES, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 3", (int) F4SimOutputs.DED__LINES, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 4", (int) F4SimOutputs.DED__LINES, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 5", (int) F4SimOutputs.DED__LINES, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 1 Inverse Characters",
                                                (int) F4SimOutputs.DED__INVERT_LINES, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 2 Inverse Characters",
                                                (int) F4SimOutputs.DED__INVERT_LINES, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 3 Inverse Characters",
                                                (int) F4SimOutputs.DED__INVERT_LINES, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 4 Inverse Characters",
                                                (int) F4SimOutputs.DED__INVERT_LINES, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("DED", "DED Line 5 Inverse Characters",
                                                (int) F4SimOutputs.DED__INVERT_LINES, 4, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 1", (int) F4SimOutputs.PFL__LINES, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 2", (int) F4SimOutputs.PFL__LINES, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 3", (int) F4SimOutputs.PFL__LINES, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 4", (int) F4SimOutputs.PFL__LINES, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 5", (int) F4SimOutputs.PFL__LINES, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 1 Inverse Characters",
                                                (int) F4SimOutputs.PFL__INVERT_LINES, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 2 Inverse Characters",
                                                (int) F4SimOutputs.PFL__INVERT_LINES, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 3 Inverse Characters",
                                                (int) F4SimOutputs.PFL__INVERT_LINES, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 4 Inverse Characters",
                                                (int) F4SimOutputs.PFL__INVERT_LINES, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("PFL", "PFL Line 5 Inverse Characters",
                                                (int) F4SimOutputs.PFL__INVERT_LINES, 4, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("Up-Front Controls", "TACAN Channel",
                                                (int) F4SimOutputs.UFC__TACAN_CHANNEL, typeof (int)));
            AddF4SimOutput(CreateNewF4SimOutput("Up-Front Controls", "Tacan Band is X",
                                                (int) F4SimOutputs.UFC__TACAN_BAND_IS_X, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Up-Front Controls", "Tacan Mode is AA",
                                                (int) F4SimOutputs.UFC__TACAN_MODE_IS_AA, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "TACAN Channel",
                                                (int) F4SimOutputs.AUX_COMM__TACAN_CHANNEL, typeof (int)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "Tacan Band is X",
                                                (int) F4SimOutputs.AUX_COMM__TACAN_BAND_IS_X, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "Tacan Mode is AA",
                                                (int) F4SimOutputs.AUX_COMM__TACAN_MODE_IS_AA, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "UHF Preset",
                                                (int)F4SimOutputs.AUX_COMM__UHF_PRESET, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "UHF Frequency",
                                                (int)F4SimOutputs.AUX_COMM__UHF_FREQUENCY, typeof(int)));


            AddF4SimOutput(CreateNewF4SimOutput("FUEL QTY Panel", "Internal fuel (pounds)",
                                                (int) F4SimOutputs.FUEL_QTY__INTERNAL_FUEL_POUNDS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FUEL QTY Panel", "External fuel (pounds)",
                                                (int) F4SimOutputs.FUEL_QTY__EXTERNAL_FUEL_POUNDS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FUEL QTY Panel", "Foreward fuel quantity (lbs)",
                                                (int) F4SimOutputs.FUEL_QTY__FOREWARD_QTY_LBS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FUEL QTY Panel", "Aft fuel quantity (lbs) ",
                                                (int) F4SimOutputs.FUEL_QTY__AFT_QTY_LBS, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("FUEL QTY Panel", "Total fuel (lbs)",
                                                (int) F4SimOutputs.FUEL_QTY__TOTAL_FUEL_LBS, typeof (float)));


            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 1 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 2 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 3 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 4 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 5 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 6 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 5, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 7 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 6, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 8 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 7, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 9 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 8, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 10 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 9, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 11 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 10, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 12 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 11, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 13 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 12, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 14 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 13, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 15 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 14, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 16 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 15, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 17 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 16, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 18 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 17, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 19 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 18, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 20 Label Line 1",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES1, 19, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 1 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 2 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 3 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 4 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 5 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 6 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 5, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 7 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 6, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 8 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 7, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 9 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 8, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 10 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 9, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 11 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 10, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 12 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 11, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 13 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 12, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 14 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 13, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 15 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 14, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 16 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 15, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 17 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 16, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 18 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 17, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 19 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 18, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 20 Label Line 2",
                                                (int) F4SimOutputs.LMFD__OSB_LABEL_LINES2, 19, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 1 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 0, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 2 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 1, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 3 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 2, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 4 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 3, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 5 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 4, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 6 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 5, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 7 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 6, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 8 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 7, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 9 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 8, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 10 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 9, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 11 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 10, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 12 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 11, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 13 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 12, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 14 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 13, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 15 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 14, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 16 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 15, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 17 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 16, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 18 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 17, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 19 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 18, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left MFD", "OSB 20 Label Inverted Flag",
                                                (int) F4SimOutputs.LMFD__OSB_INVERTED_FLAGS, 19, typeof (bool)));


            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 1 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 2 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 3 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 4 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 5 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 6 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 5, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 7 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 6, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 8 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 7, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 9 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 8, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 10 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 9, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 11 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 10, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 12 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 11, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 13 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 12, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 14 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 13, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 15 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 14, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 16 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 15, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 17 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 16, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 18 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 17, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 19 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 18, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 20 Label Line 1",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES1, 19, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 1 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 0, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 2 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 1, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 3 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 2, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 4 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 3, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 5 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 4, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 6 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 5, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 7 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 6, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 8 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 7, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 9 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 8, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 10 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 9, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 11 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 10, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 12 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 11, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 13 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 12, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 14 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 13, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 15 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 14, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 16 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 15, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 17 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 16, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 18 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 17, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 19 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 18, typeof (string)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 20 Label Line 2",
                                                (int) F4SimOutputs.RMFD__OSB_LABEL_LINES2, 19, typeof (string)));

            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 1 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 0, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 2 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 1, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 3 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 2, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 4 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 3, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 5 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 4, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 6 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 5, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 7 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 6, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 8 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 7, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 9 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 8, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 10 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 9, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 11 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 10, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 12 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 11, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 13 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 12, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 14 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 13, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 15 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 14, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 16 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 15, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 17 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 16, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 18 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 17, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 19 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 18, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right MFD", "OSB 20 Label Inverted Flag",
                                                (int) F4SimOutputs.RMFD__OSB_INVERTED_FLAGS, 19, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Right Aux Console", "Master Caution Light",
                                                (int) F4SimOutputs.MASTER_CAUTION_LIGHT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Aux Console", "Master Caution Announced",
                                                (int)F4SimOutputs.MASTER_CAUTION_ANNOUNCED, typeof(bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Left Eyebrow Lights", "TF-FAIL Light",
                                                (int) F4SimOutputs.LEFT_EYEBROW_LIGHTS__TFFAIL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left Eyebrow Lights", "ALT LOW Light",
                                                (int) F4SimOutputs.LEFT_EYEBROW_LIGHTS__ALTLOW, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Left Eyebrow Lights", "OBS WRN Light",
                                                (int) F4SimOutputs.LEFT_EYEBROW_LIGHTS__OBSWRN, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "ENG FIRE Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__ENGFIRE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "ENGINE Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__ENGINE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "HYD/OIL PRESS Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__HYDOIL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "DUAL FC Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__DUALFC, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "FLCS Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__FLCS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "CANOPY Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__CANOPY, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "TO/LDG CONFIG Light",
                                                (int) F4SimOutputs.RIGHT_EYEBROW_LIGHTS__TO_LDG_CONFIG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "OXY LOW Light",
                                                (int)F4SimOutputs.RIGHT_EYEBROW_LIGHTS__OXY_LOW, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Right Eyebrow Lights", "DBU ON Light",
                                                (int)F4SimOutputs.RIGHT_EYEBROW_LIGHTS__DBU_ON, typeof(bool)));
            

            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "FLCS FAULT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__FLCS_FAULT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "LE FLAPS Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__LE_FLAPS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "ELEC SYS Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__ELEC_SYS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "ENGINE FAULT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__ENGINE_FAULT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "SEC Light", (int) F4SimOutputs.CAUTION_PANEL__SEC,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "FWD FUEL LOW Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__FWD_FUEL_LOW, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "AFT FUEL LOW Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__AFT_FUEL_LOW, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "OVERHEAT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__OVERHEAT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "BUC Light", (int) F4SimOutputs.CAUTION_PANEL__BUC,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "FUEL OIL HOT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__FUEL_OIL_HOT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "SEAT NOT ARMED Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__SEAT_NOT_ARMED, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "AVIONICS FAULT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__AVIONICS_FAULT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "RADAR ALT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__RADAR_ALT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "EQUIP HOT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__EQUIP_HOT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "ECM Light", (int) F4SimOutputs.CAUTION_PANEL__ECM,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "STORES CONFIG Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__STORES_CONFIG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "ANTI SKID Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__ANTI_SKID, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "HOOK Light", (int) F4SimOutputs.CAUTION_PANEL__HOOK,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "NWS FAIL Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__NWS_FAIL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "CABIN PRESS Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__CABIN_PRESS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "OXY LOW Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__OXY_LOW, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "PROBE HEAT Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__PROBE_HEAT, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "FUEL LOW Light",
                                                (int) F4SimOutputs.CAUTION_PANEL__FUEL_LOW, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "IFF Light", (int) F4SimOutputs.CAUTION_PANEL__IFF,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "C ADC Light", (int) F4SimOutputs.CAUTION_PANEL__C_ADC,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Caution Panel", "ATF NOT ENGAGED Light", (int)F4SimOutputs.CAUTION_PANEL__ATF_NOT_ENGAGED,
                                                typeof(bool)));

            /*
             * MISSING CAUTION PANEL FLAGS
            ADC
            INLET ICING
            EEC
            NUCLEAR
             */

            /*
            // Caution panel
        Lef_Fault = 0x800,  //TODO: WHAT IS THIS?? LE FLAPS IS ALREADY A FLAG??
             * 
            */


            AddF4SimOutput(CreateNewF4SimOutput("AOA Indexer", "AOA Too-High Light",
                                                (int) F4SimOutputs.AOA_INDEXER__AOA_TOO_HIGH, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("AOA Indexer", "AOA Ideal Light",
                                                (int) F4SimOutputs.AOA_INDEXER__AOA_IDEAL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("AOA Indexer", "AOA Too-Low Light",
                                                (int) F4SimOutputs.AOA_INDEXER__AOA_TOO_LOW, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Refuel/NWS Indexer", "RDY Light", (int) F4SimOutputs.NWS_INDEXER__RDY,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Refuel/NWS Indexer", "AR NWS Light",
                                                (int) F4SimOutputs.NWS_INDEXER__AR_NWS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Refuel/NWS Indexer", "DISC Light", (int) F4SimOutputs.NWS_INDEXER__DISC,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "HANDOFF Indicator Light",
                                                (int) F4SimOutputs.TWP__HANDOFF, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "MISSILE LAUNCH Indicator Light",
                                                (int) F4SimOutputs.TWP__MISSILE_LAUNCH, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "PRIORITY MODE OPEN Indicator Light",
                                                (int) F4SimOutputs.TWP__PRIORITY_MODE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "UNKNOWN Indicator Light",
                                                (int) F4SimOutputs.TWP__UNKNOWN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "NAVAL Indicator Light",
                                                (int) F4SimOutputs.TWP__NAVAL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "TGT SEP Indicator Light",
                                                (int) F4SimOutputs.TWP__TARGET_SEP, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "SYS TEST Indicator Light",
                                                (int)F4SimOutputs.TWP__SYS_TEST, typeof(bool)));


            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Aux Panel", "SEARCH Indicator Light",
                                                (int) F4SimOutputs.TWA__SEARCH, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Aux Panel", "ACTIVITY/POWER Indicator Light",
                                                (int) F4SimOutputs.TWA__ACTIVITY_POWER, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Aux Panel", "LOW ALTITUDE Indicator Light",
                                                (int) F4SimOutputs.TWA__LOW_ALTITUDE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Aux Panel", "SYSTEM POWER Indicator Light",
                                                (int) F4SimOutputs.TWA__SYSTEM_POWER, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("ECM", "POWER Flag", (int) F4SimOutputs.ECM__POWER, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ECM", "FAIL Flag", (int) F4SimOutputs.ECM__FAIL, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("MISC Panel", "ADV MODE ACTIVE Light",
                                                (int) F4SimOutputs.MISC__ADV_MODE_ACTIVE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("MISC Panel", "ADV MODE STBY Light",
                                                (int) F4SimOutputs.MISC__ADV_MODE_STBY, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("MISC Panel", "Autopilot engaged flag",
                                                (int) F4SimOutputs.MISC__AUTOPILOT_ENGAGED, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Chaff/Flare Panel", "Chaff Count (# remaining)",
                                                (int) F4SimOutputs.CHAFF_FLARE__CHAFF_COUNT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("Chaff/Flare Panel", "Flare Count (# remaining)",
                                                (int) F4SimOutputs.CHAFF_FLARE__FLARE_COUNT, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "GO Flag", (int) F4SimOutputs.CMDS__GO, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "NO GO Flag", (int) F4SimOutputs.CMDS__NOGO, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "AUTO DEGR Flag", (int) F4SimOutputs.CMDS__AUTO_DEGR,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "DISPENSE RDY Flag", (int) F4SimOutputs.CMDS__DISPENSE_RDY,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "CHAFF LO Flag", (int) F4SimOutputs.CMDS__CHAFF_LO,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "FLARE LO Flag", (int) F4SimOutputs.CMDS__FLARE_LO,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("CMDS Panel", "MODE", (int)F4SimOutputs.CMDS__MODE,
                                                typeof(int)));

            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "FLCS PMG Indicator Light",
                                                (int) F4SimOutputs.ELEC__FLCS_PMG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "MAIN GEN Indicator Light",
                                                (int) F4SimOutputs.ELEC__MAIN_GEN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "STBY GEN Indicator Light",
                                                (int) F4SimOutputs.ELEC__STBY_GEN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "EPU GEN Indicator Light",
                                                (int) F4SimOutputs.ELEC__EPU_GEN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "EPU PMG Indicator Light",
                                                (int) F4SimOutputs.ELEC__EPU_PMG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "TO FLCS Indicator Light",
                                                (int) F4SimOutputs.ELEC__TO_FLCS, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "FLCS RLY Indicator Light",
                                                (int) F4SimOutputs.ELEC__FLCS_RLY, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ELEC Panel", "BATT FAIL Indicator Light",
                                                (int) F4SimOutputs.ELEC__BATT_FAIL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("TEST Panel", "FLCS ABCD Indicator Light", (int) F4SimOutputs.TEST__ABCD,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("EPU Panel", "HYDRAZN Indicator Light", (int) F4SimOutputs.EPU__HYDRAZN,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("EPU Panel", "AIR Indicator Light", (int) F4SimOutputs.EPU__AIR,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("EPU Panel", "RUN Indicator Light", (int) F4SimOutputs.EPU__RUN,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("AVTR Panel", "AVTR Indicator Light", (int) F4SimOutputs.AVTR__AVTR,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("JFS Panel", "RUN Indicator Light", (int) F4SimOutputs.JFS__RUN,
                                                typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("MARKER BEACON", "MRK BCN Light",
                                                (int)F4SimOutputs.MARKER_BEACON__MRK_BCN_LIGHT, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("MARKER BEACON", "Outer marker flag",
                                                (int) F4SimOutputs.MARKER_BEACON__OUTER_MARKER_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("MARKER BEACON", "Middle marker flag",
                                                (int) F4SimOutputs.MARKER_BEACON__MIDDLE_MARKER_FLAG, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "On Ground", (int) F4SimOutputs.AIRCRAFT__ONGROUND,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Main Landing Gear - Weight on Wheels", (int)F4SimOutputs.AIRCRAFT__MAIN_LANDING_GEAR__WEIGHT_ON_WHEELS,
                                                typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Nose Landing Gear - Weight on Wheels", (int)F4SimOutputs.AIRCRAFT__NOSE_LANDING_GEAR__WEIGHT_ON_WHEELS,
                                                typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Engine 2 Fire flag",
                                                (int) F4SimOutputs.AIRCRAFT__ENGINE_2_FIRE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Leading edge flaps position", (int)F4SimOutputs.AIRCRAFT__LEADING_EDGE_FLAPS_POSITION,
                        typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Trailing edge flaps position", (int)F4SimOutputs.AIRCRAFT__TRAILING_EDGE_FLAPS_POSITION,
                                    typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "VTOL position", (int)F4SimOutputs.AIRCRAFT__VTOL_POSITION,
                                    typeof(float)));

            AddF4SimOutput(CreateNewF4SimOutput("Power", "Electrical Power OFF flag",
                                                (int)F4SimOutputs.POWER__ELEC_POWER_OFF, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Main Power flag", (int)F4SimOutputs.POWER__MAIN_POWER,
                                                typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Bus Power - Battery Power flag",
                                                (int)F4SimOutputs.POWER__BUS_POWER_BATTERY, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Bus Power - Emergency Power flag",
                                                (int)F4SimOutputs.POWER__BUS_POWER_EMERGENCY, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Bus Power - Essential Power flag",
                                                (int)F4SimOutputs.POWER__BUS_POWER_ESSENTIAL, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Bus Power - Non-Essential Power flag",
                                                (int)F4SimOutputs.POWER__BUS_POWER_NON_ESSENTIAL, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Main Generator flag",
                                                (int)F4SimOutputs.POWER__MAIN_GENERATOR, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Standby Generator flag",
                                                (int)F4SimOutputs.POWER__STANDBY_GENERATOR, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Power", "Jet Fuel Starter flag",
                                                (int)F4SimOutputs.POWER__JET_FUEL_STARTER, typeof(bool)));

            AddF4SimOutput(CreateNewF4SimOutput("SIM", "'Pilot Is Flying' flag",
                                                (int) F4SimOutputs.SIM__BMS_PLAYER_IS_FLYING, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Shared Memory Area \"FlightData\" Version Number",
                                                (int) F4SimOutputs.SIM__FLIGHTDATA_VERSION_NUM, typeof (int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Shared Memory Area \"FlightData2\" Version Number",
                                                (int)F4SimOutputs.SIM__FLIGHTDATA2_VERSION_NUM, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Current Time",
                                                (int)F4SimOutputs.SIM__CURRENT_TIME, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Vehicle ACD",
                                                (int)F4SimOutputs.SIM__VEHICLE_ACD, typeof(short)));
            for (var i = 0; i < 32; i++)
            {
                AddF4SimOutput(CreateNewF4SimOutput("SIM", "Pilot Callsign", (int)F4SimOutputs.SIM__PILOT_CALLSIGN, i, typeof(string)));
                AddF4SimOutput(CreateNewF4SimOutput("SIM", "Pilot Status", (int)F4SimOutputs.SIM__PILOT_STATUS, i, typeof(int)));
            }
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "AA Missile Fired",
                                                (int)F4SimOutputs.SIM__AA_MISSILE_FIRED, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Bomb Dropped",
                                                (int)F4SimOutputs.SIM__BOMB_DROPPED, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Flare Dropped",
                                                (int)F4SimOutputs.SIM__FLARE_DROPPED, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Chaff Dropped",
                                                (int)F4SimOutputs.SIM__CHAFF_DROPPED, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Bullets Fired",
                                                (int)F4SimOutputs.SIM__BULLETS_FIRED, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Collision Counter",
                                                (int)F4SimOutputs.SIM__COLLISION_COUNTER, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "g Force",
                                                (int)F4SimOutputs.SIM__GFORCE, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Last Damage",
                                                (int)F4SimOutputs.SIM__LAST_DAMAGE, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Damage Force",
                                                (int)F4SimOutputs.SIM__DAMAGE_FORCE, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "When Damage",
                                                (int)F4SimOutputs.SIM__WHEN_DAMAGE, typeof(int)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Eye X",
                                                (int)F4SimOutputs.SIM__EYE_X, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Eye Y",
                                                (int)F4SimOutputs.SIM__EYE_Y, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Eye Z",
                                                (int)F4SimOutputs.SIM__EYE_Z, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Firing Gun flag",
                                                (int)F4SimOutputs.SIM__IS_FIRING_GUN, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is End Flight flag",
                                                (int)F4SimOutputs.SIM__IS_END_FLIGHT, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Ejecting flag",
                                                (int)F4SimOutputs.SIM__IS_EJECTING, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "In 3D flag",
                                                (int)F4SimOutputs.SIM__IN_3D, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Paused flag",
                                                (int)F4SimOutputs.SIM__IS_PAUSED, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Frozen flag",
                                                (int)F4SimOutputs.SIM__IS_FROZEN, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Over G flag",
                                                (int)F4SimOutputs.SIM__IS_OVER_G, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is On Ground flag",
                                                (int)F4SimOutputs.SIM__IS_ON_GROUND, typeof(bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Is Exit Game flag",
                                                (int)F4SimOutputs.SIM__IS_EXIT_GAME, typeof(bool)));

            /*
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head X offset from design eye (feet)",(int)F4SimOutputs.PILOT__HEADX_OFFSET,  typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head Y offset from design eye (feet)", (int)F4SimOutputs.PILOT__HEADY_OFFSET, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head Z offset from design eye (feet)", (int)F4SimOutputs.PILOT__HEADZ_OFFSET,  typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head pitch offset from design eye (degrees)", (int)F4SimOutputs.PILOT__HEAD_PITCH_DEGREES,  typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head roll offset from design eye (degrees)", (int)F4SimOutputs.PILOT__HEAD_ROLL_DEGREES, typeof(float)));
            AddF4SimOutput(CreateNewF4SimOutput("Pilot", "Head yaw offset from design eye (degrees)", (int)F4SimOutputs.PILOT__HEAD_YAW_DEGREES, typeof(float)));
            */

            AddF4SimOutput(CreateNewF4SimOutput("FLIGHT CONTROL Panel", "RUN Indicator Light",
                                                (int) F4SimOutputs.FLIGHT_CONTROL__RUN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("FLIGHT CONTROL Panel", "FAIL Indicator Light",
                                                (int) F4SimOutputs.FLIGHT_CONTROL__FAIL, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("RWR", "Object Count", (int) F4SimOutputs.RWR__OBJECT_COUNT,
                                                typeof (int)));
            for (var i = 0; i < 64; i++)
            {
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Symbol ID",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__SYMBOL_ID, i, typeof (int)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Bearing (Degrees)",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__BEARING_DEGREES, i, typeof (float)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Missile Activity Flag",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__MISSILE_ACTIVITY_FLAG, i, typeof (bool)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Missile Launch Flag",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__MISSILE_LAUNCH_FLAG, i, typeof (bool)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Selected Flag",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__SELECTED_FLAG, i, typeof (bool)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} Lethality",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__LETHALITY, i, typeof (float)));
                AddF4SimOutput(CreateNewF4SimOutput("RWR",
                                                    string.Format("Threat #{0} New Detection Flag",
                                                                  string.Format("{0:00}", i + 1)),
                                                    (int) F4SimOutputs.RWR__NEWDETECTION_FLAG, i, typeof (bool)));
            }
            AddF4SimOutput(CreateNewF4SimOutput("RWR", "Additional Info", (int)F4SimOutputs.RWR__ADDITIONAL_INFO,
                                                typeof(string)));
        }

        public override void Update()
        {
            GetNextFlightDataFromSharedMem();
            UpdateSimOutputValues();
        }

        private void UpdateHsiData(FlightData flightData, out float courseDeviationDecimalDegrees, out float deviationLimitDecimalDegrees )
        {
            deviationLimitDecimalDegrees = flightData.deviationLimit  % 180;
            courseDeviationDecimalDegrees = flightData.courseDeviation;

            if (courseDeviationDecimalDegrees < -90)
            {
                courseDeviationDecimalDegrees = Common.Math.Util.AngleDelta(Math.Abs(courseDeviationDecimalDegrees), 180) % 180;
            }
            else if (courseDeviationDecimalDegrees > 90)
            {
                courseDeviationDecimalDegrees = -Common.Math.Util.AngleDelta(courseDeviationDecimalDegrees, 180) % 180;
            }
            else
            {
                courseDeviationDecimalDegrees = -courseDeviationDecimalDegrees;
            }
            if (Math.Abs(courseDeviationDecimalDegrees) > deviationLimitDecimalDegrees)
            {
                courseDeviationDecimalDegrees = Math.Sign(courseDeviationDecimalDegrees) * deviationLimitDecimalDegrees;
            }

        }
        private void DetermineWhetherToShowILSCommandBarsAndToFromFlags(FlightData flightData, out bool showToFromFlag, out bool showCommandBars)
        {
            showToFromFlag = true;
            showCommandBars =
                           ((Math.Abs((flightData.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE)) <= (flightData.deviationLimit / 5.0f))
                               &&
                           (Math.Abs((flightData.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE)) <= flightData.deviationLimit))
                               &&
                           !(((HsiBits)flightData.hsiBits & HsiBits.ADI_GS) == HsiBits.ADI_GS)
                           &&
                           !(((HsiBits)flightData.hsiBits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC)
                           &&
                           !(((HsiBits)flightData.hsiBits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
            
            switch (flightData.navMode)
            {
                case 0: //NavModes.PlsTcn:
                    showToFromFlag = false;
                    break;
                case 1: //NavModes.Tcn:
                    showToFromFlag = true;
                    showCommandBars = false;
                    break;
                case 2: //NavModes.Nav:
                    showToFromFlag = false;
                    showCommandBars = false;
                    break;
                case 3: //NavModes.PlsNav:
                    showToFromFlag = false;
                    break;
            }

            if (showCommandBars)
            {
                showToFromFlag = false;
            }
        }

        private void _morseCodeGenerator_UnitTimeTick(object sender, UnitTimeTickEventArgs e)
        {
            _markerBeaconMorseCodeState = e.CurrentSignalLineState;
        }


        public override void Dispose()
        {
            Dispose(true);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_morseCodeGenerator.Sending)
                {
                    _morseCodeGenerator.StopSending();
                }
                _morseCodeGenerator.UnitTimeTick -= _morseCodeGenerator_UnitTimeTick;
                Common.Util.DisposeObject(_morseCodeGenerator);
                _disposed = true;
            }
        }
        ~Falcon4SimSupportModule()
        {
            Dispose(false);
        }
    }


}