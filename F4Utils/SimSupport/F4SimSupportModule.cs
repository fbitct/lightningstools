using System;
using System.Collections.Generic;
using Common.MacroProgramming;
using Common.SimSupport;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Process;

namespace F4Utils.SimSupport
{
    public class Falcon4SimSupportModule : SimSupportModule
    {
        public const float DEGREES_PER_RADIAN = 57.2957795f;
        public const float FEET_PER_SECOND_PER_KNOT = 1.68780986f;
        private readonly Dictionary<string, ISimOutput> _simOutputs = new Dictionary<string, ISimOutput>();
        private FlightData _lastFlightData;
        private double _origCabinAlt;
        private Reader _smReader;

        public Falcon4SimSupportModule()
        {
            TryCreateSharedMemReader();
            CreateSimOutputsList();
        }

        public override string FriendlyName
        {
            get { return "Falcon 4"; }
        }

        public override bool IsSimRunning
        {
            get
            {
                if (!TestMode)
                {
                    TryCreateSharedMemReader();
                    if (_smReader == null) return false;
                    return _smReader.IsFalconRunning;
                }
                return true;
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

        private void TryCreateSharedMemReader()
        {
            var dataFormat = Util.DetectFalconFormat();
            if (dataFormat.HasValue)
            {
                if (_smReader != null && _smReader.DataFormat != dataFormat.Value)
                {
                    Common.Util.DisposeObject(_smReader);
                    _smReader = null;
                }
                if (_smReader == null)
                {
                    _smReader = new Reader(dataFormat.Value);
                }
            }
        }

        private void GetNextFlightDataFromSharedMem()
        {
            if (TestMode)
            {
                _lastFlightData = GetFakeFlightData();
            }
            else
            {
                if (_smReader == null) return;
                _lastFlightData = _smReader.GetCurrentData();
            }
        }

        private static FlightData GetFakeFlightData()
        {
            var toReturn = new FlightData();
            var rnd = new Random();
            toReturn.x = (float) (rnd.NextDouble()*10000);
            toReturn.y = (float) (rnd.NextDouble()*10000);
            toReturn.z = (float) (rnd.NextDouble()*10000);
            toReturn.xDot = (float) (rnd.NextDouble()*100);
            toReturn.yDot = (float) (rnd.NextDouble()*100);
            toReturn.zDot = (float) (rnd.NextDouble()*100);
            toReturn.alpha = (float) (rnd.NextDouble()*66) - 33;
            toReturn.beta = 0;
            toReturn.gamma = 0;
            toReturn.alpha = (float) (rnd.NextDouble()*180) - 90;
            toReturn.roll = (float) (rnd.NextDouble()*360) - 180;
            toReturn.yaw = (float) (rnd.NextDouble()*360) - 180;
            toReturn.mach = (float) (rnd.NextDouble()*2.5);
            toReturn.kias = (float) (rnd.NextDouble()*1000);
            toReturn.vt = (float) (rnd.NextDouble()*1000);
            toReturn.gs = (float) (rnd.NextDouble()*9.9);
            toReturn.windOffset = (float) (rnd.NextDouble()*((2*Math.PI)/180)*30);
            toReturn.nozzlePos = (float) (rnd.NextDouble()*100);
            toReturn.nozzlePos2 = (float) (rnd.NextDouble()*100);
            toReturn.internalFuel = (float) (rnd.NextDouble()*10000);
            toReturn.externalFuel = (float) (rnd.NextDouble()*10000);
            toReturn.fuelFlow = (float) (rnd.NextDouble()*99999);
            toReturn.rpm = (float) (rnd.NextDouble()*103);
            toReturn.rpm2 = (float) (rnd.NextDouble()*103);
            toReturn.ftit = (float) (rnd.NextDouble()*700);
            toReturn.ftit2 = (float) (rnd.NextDouble()*700);
            toReturn.gearPos = (float) (rnd.NextDouble()*1);
            toReturn.speedBrake = (float) (rnd.NextDouble()*1);
            toReturn.epuFuel = (float) (rnd.NextDouble()*1);
            toReturn.oilPressure = (float) (rnd.NextDouble()*1);
            toReturn.oilPressure2 = (float) (rnd.NextDouble()*1);
            toReturn.lightBits = (int) (rnd.NextDouble()*0xFFFFFFFF);
            toReturn.lightBits2 = (int) (rnd.NextDouble()*0xFFFFFFFF);
            toReturn.lightBits3 = (int) (rnd.NextDouble()*0xFFFFFFFF);
            toReturn.ChaffCount = (float) (rnd.NextDouble()*99);
            toReturn.FlareCount = (float) (rnd.NextDouble()*99);
            toReturn.NoseGearPos = (float) (rnd.NextDouble()*1);
            toReturn.LeftGearPos = (float) (rnd.NextDouble()*1);
            toReturn.RightGearPos = (float) (rnd.NextDouble()*1);
            toReturn.AdiIlsHorPos = (float) (rnd.NextDouble()*2) - 1;
            toReturn.AdiIlsVerPos = (float) (rnd.NextDouble()*2) - 1;
            toReturn.courseState = (rnd.NextDouble()*1 > 0.50) ? 1 : 0;
            toReturn.headingState = (rnd.NextDouble()*1 > 0.50) ? 1 : 0;
            toReturn.totalStates = (rnd.NextDouble()*1 > 0.50) ? 1 : 0;
            toReturn.courseDeviation = (float) (rnd.NextDouble()*90);
            toReturn.desiredCourse = (float) (rnd.NextDouble()*360);
            toReturn.distanceToBeacon = (float) (rnd.NextDouble()*999);
            toReturn.bearingToBeacon = (float) (rnd.NextDouble()*360);
            toReturn.currentHeading = (float) (rnd.NextDouble()*360);
            toReturn.desiredHeading = (float) (rnd.NextDouble()*360);
            toReturn.deviationLimit = 5;
            toReturn.halfDeviationLimit = 2.5f;
            toReturn.localizerCourse = (float) (rnd.NextDouble()*360);
            toReturn.TrimPitch = ((float) (rnd.NextDouble()*1) - 0.5f)*2;
            toReturn.TrimRoll = ((float) (rnd.NextDouble()*1) - 0.5f)*2;
            toReturn.TrimYaw = ((float) (rnd.NextDouble()*1) - 0.5f)*2;
            toReturn.hsiBits = (int) (rnd.NextDouble()*0xFFFFFFFF);
            toReturn.DEDLines = new[]
                                    {
                                        new string('x', 25), new string('x', 25), new string('x', 25),
                                        new string('x', 25),
                                        new string('x', 25)
                                    };
            toReturn.Invert = new[]
                                  {
                                      new string((char) 2, 25), new string((char) 2, 25), new string((char) 2, 25),
                                      new string((char) 2, 25), new string((char) 2, 25)
                                  };
            toReturn.PFLLines = toReturn.DEDLines;
            toReturn.PFLInvert = toReturn.Invert;
            toReturn.UFCTChan = (int) (rnd.NextDouble()*999);
            toReturn.AUXTChan = (int) (rnd.NextDouble()*999);
            toReturn.RwrObjectCount = (int) (rnd.NextDouble()*40);
            toReturn.RWRsymbol = new int[40];
            toReturn.bearing = new float[40];
            toReturn.missileActivity = new int[40];
            toReturn.missileLaunch = new int[40];
            toReturn.selected = new int[40];
            toReturn.lethality = new float[40];
            toReturn.newDetection = new int[40];
            for (var i = 0; i < toReturn.RwrObjectCount; i++)
            {
                toReturn.bearing[i] = (float) (rnd.NextDouble()*360);
                toReturn.missileActivity[i] = rnd.NextDouble() > 0.5 ? 0 : 1;
                toReturn.missileLaunch[i] = rnd.NextDouble() > 0.5 ? 0 : 1;
                toReturn.selected[i] = rnd.NextDouble() > 0.5 ? 0 : 1;
                toReturn.lethality[i] = (float) (rnd.NextDouble()*2);
            }
            toReturn.fwd = (float) (rnd.NextDouble()*10000);
            toReturn.aft = (float) (rnd.NextDouble()*10000);
            toReturn.total = toReturn.fwd + toReturn.aft;
            toReturn.navMode = (byte) (rnd.NextDouble()*3);
            toReturn.aauz = toReturn.z;
            toReturn.DataFormat = FalconDataFormats.BMS4;
            return toReturn;
        }

        private void UpdateSimOutputValues()
        {
            if (_simOutputs == null) return;
            GetNextFlightDataFromSharedMem();
            if (_lastFlightData == null) return;
            foreach (var output in _simOutputs.Values)
            {
                F4SimOutputs? simOutputEnumMatch = null;
                F4SimOutputs triedParse;
                var key = ((Signal) output).Id;
                var firstBracketLocation = key.IndexOf("[");
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
                        if (_lastFlightData.DataFormat == FalconDataFormats.BMS4)
                        {
                            ((AnalogSignal) output).State = -_lastFlightData.aauz;
                        }
                        else
                        {
                            ((AnalogSignal) output).State = -_lastFlightData.z;
                        }
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
                    case F4SimOutputs.FUEL_QTY__INTERNAL_FUEL_POUNDS:
                        ((AnalogSignal) output).State = _lastFlightData.internalFuel;
                        break;
                    case F4SimOutputs.FUEL_QTY__EXTERNAL_FUEL_POUNDS:
                        ((AnalogSignal) output).State = _lastFlightData.externalFuel;
                        break;
                    case F4SimOutputs.FUEL_FLOW__FUEL_FLOW_POUNDS_PER_HOUR:
                        ((AnalogSignal) output).State = _lastFlightData.fuelFlow;
                        break;
                    case F4SimOutputs.RPM1__RPM_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.rpm;
                        break;
                    case F4SimOutputs.RPM2__RPM_PERCENT:
                        ((AnalogSignal) output).State = _lastFlightData.rpm2;
                        break;
                    case F4SimOutputs.FTIT1__FTIT_TEMP_DEG_CELCIUS:
                        ((AnalogSignal) output).State = _lastFlightData.ftit;
                        break;
                    case F4SimOutputs.FTIT2__FTIT_TEMP_DEG_CELCIUS:
                        ((AnalogSignal) output).State = _lastFlightData.ftit2;
                        break;
                    case F4SimOutputs.SPEED_BRAKE__POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.speedBrake;
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
                        var pressurization = ((_lastFlightData.lightBits & (int) LightBits.CabinPress) ==
                                              (int) LightBits.CabinPress);
                        ((AnalogSignal) output).State = NonImplementedGaugeCalculations.CabinAlt((float) _origCabinAlt,
                                                                                                 _lastFlightData.z,
                                                                                                 pressurization);
                        _origCabinAlt = ((AnalogSignal) output).State;
                        break;
                    case F4SimOutputs.COMPASS__MAGNETIC_HEADING_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.yaw;
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
                        ((DigitalSignal) output).State = (((Bms4LightBits3) _lastFlightData.lightBits3 &
                                                           Bms4LightBits3.ParkBrakeOn) == Bms4LightBits3.ParkBrakeOn);
                        break;
                    case F4SimOutputs.ADI__PITCH_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.pitch*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.ADI__ROLL_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.roll*DEGREES_PER_RADIAN;
                        break;
                    case F4SimOutputs.ADI__ILS_HORIZONTAL_BAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.AdiIlsVerPos;
                        break;
                    case F4SimOutputs.ADI__ILS_VERTICAL_BAR_POSITION:
                        ((AnalogSignal) output).State = _lastFlightData.AdiIlsHorPos;
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
                        ((AnalogSignal) output).State = _lastFlightData.courseDeviation;
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
                    case F4SimOutputs.HSI__LOCALIZER_NEEDLE_LIMIT_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.deviationLimit;
                        break;
                    case F4SimOutputs.HSI__LOCALIZER_NEEDLE_POSITION_DEGREES:
                        ((AnalogSignal) output).State = _lastFlightData.localizerCourse;
                        break;
                    case F4SimOutputs.HSI__TO_FLAG:
                        {
                            var myCourseDeviationDecimalDegrees =
                                Common.Math.Util.AngleDelta(_lastFlightData.desiredCourse,
                                                            _lastFlightData.bearingToBeacon);
                            ((DigitalSignal) output).State = Math.Abs(myCourseDeviationDecimalDegrees) <= 90;
                        }
                        break;
                    case F4SimOutputs.HSI__FROM_FLAG:
                        {
                            var myCourseDeviationDecimalDegrees =
                                Common.Math.Util.AngleDelta(_lastFlightData.desiredCourse,
                                                            _lastFlightData.bearingToBeacon);
                            ((DigitalSignal) output).State = Math.Abs(myCourseDeviationDecimalDegrees) > 90;
                        }
                        break;
                    case F4SimOutputs.HSI__OFF_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.HSI_OFF) ==
                                                          HsiBits.HSI_OFF);
                        break;
                    case F4SimOutputs.HSI__NAVMODE_FLAG:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.TotalFlags) ==
                                                          HsiBits.TotalFlags);
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
                    case F4SimOutputs.LEFT_EYEBROW_LIGHTS__TFFAIL:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.TF) ==
                                                          LightBits.TF);
                        break;
                    case F4SimOutputs.LEFT_EYEBROW_LIGHTS__ALTLOW:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.ALT) ==
                                                          LightBits.ALT);
                        break;
                    case F4SimOutputs.LEFT_EYEBROW_LIGHTS__OBSWRN:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.OBS) ==
                                                          LightBits.OBS);
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
                    case F4SimOutputs.RIGHT_EYEBROW_LIGHTS__DUALFC:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.DUAL) ==
                                                          LightBits.DUAL);
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
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.Elec_Fault) == LightBits3.Elec_Fault);
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
                                                           LightBits2.PROBEHEAT) == LightBits2.PROBEHEAT);
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
                        ((DigitalSignal) output).State = (((Bms4LightBits3) _lastFlightData.lightBits3 &
                                                           Bms4LightBits3.cadc) == Bms4LightBits3.cadc);
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
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Launch) ==
                                                          LightBits2.Launch);
                        break;
                    case F4SimOutputs.TWP__PRIORITY_MODE_OPEN:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.PriMode) ==
                                                          LightBits2.PriMode);
                        break;
                    case F4SimOutputs.TWP__UNKNOWN:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Unk) ==
                                                          LightBits2.Unk);
                        break;
                    case F4SimOutputs.TWP__NAVAL:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.Naval) ==
                                                          LightBits2.Naval);
                        break;
                    case F4SimOutputs.TWP__TARGET_SEP:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.TgtSep) ==
                                                          LightBits2.TgtSep);
                        break;
                    case F4SimOutputs.TWA__SEARCH:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.AuxSrch) ==
                                                          LightBits2.AuxSrch);
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
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.EPUOn) ==
                                                          LightBits2.EPUOn);
                        break;
                    case F4SimOutputs.AVTR__AVTR:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.AVTR) ==
                                                          HsiBits.AVTR);
                        break;
                    case F4SimOutputs.JFS__RUN:
                        ((DigitalSignal) output).State = (((LightBits2) _lastFlightData.lightBits2 & LightBits2.JFSOn) ==
                                                          LightBits2.JFSOn);
                        break;
                    case F4SimOutputs.MARKER_BEACON__OUTER_MARKER:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.OuterMarker) ==
                                                          HsiBits.OuterMarker);

                        //TODO: make blink
                        break;
                    case F4SimOutputs.MARKER_BEACON__MIDDLE_MARKER:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.MiddleMarker) ==
                                                          HsiBits.MiddleMarker);
                        //TODO: make blink
                        break;
                    case F4SimOutputs.AIRCRAFT__WOW:
                        ((DigitalSignal) output).State = (((LightBits) _lastFlightData.lightBits & LightBits.WOW) ==
                                                          LightBits.WOW);
                        break;
                    case F4SimOutputs.AIRCRAFT__ELEC_POWER_OFF:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.Power_Off) == LightBits3.Power_Off);
                        break;
                    case F4SimOutputs.AIRCRAFT__MAIN_POWER:
                        ((AnalogSignal) output).State = _lastFlightData.MainPower;
                        break;
                    case F4SimOutputs.AIRCRAFT__ENGINE_2_FIRE:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 &
                                                           LightBits3.Eng2_Fire) == LightBits3.Eng2_Fire);
                        break;
                    case F4SimOutputs.SIM__SHOOT_CUE:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.Shoot) ==
                                                          LightBits3.Shoot);
                        break;
                    case F4SimOutputs.SIM__LOCK_CUE:
                        ((DigitalSignal) output).State = (((LightBits3) _lastFlightData.lightBits3 & LightBits3.Lock) ==
                                                          LightBits3.Lock);
                        break;
                    case F4SimOutputs.SIM__BMS_PLAYER_IS_FLYING:
                        ((DigitalSignal) output).State = (((HsiBits) _lastFlightData.hsiBits & HsiBits.Flying) ==
                                                          HsiBits.Flying);
                        break;
                    case F4SimOutputs.SIM__SHARED_MEM_VERSION:
                        ((AnalogSignal) output).State = _lastFlightData.VersionNum;
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
                        ((DigitalSignal) output).State = (((Bms4LightBits3) _lastFlightData.lightBits3 &
                                                           Bms4LightBits3.FlcsBitRun) == Bms4LightBits3.FlcsBitRun);
                        break;
                    case F4SimOutputs.FLIGHT_CONTROL__FAIL:
                        ((DigitalSignal) output).State = (((Bms4LightBits3) _lastFlightData.lightBits3 &
                                                           Bms4LightBits3.FlcsBitFail) == Bms4LightBits3.FlcsBitFail);
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
                                                            ? _lastFlightData.bearing[((Signal) output).Index.Value]
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
                               Source = TestMode ? this : (object) _smReader,
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
                               Source = TestMode ? this : (object) _smReader,
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
                           Source = TestMode ? this : (object) _smReader,
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
            AddF4SimOutput(CreateNewF4SimOutput("FUEL FLOW", "Fuel flow (pounds/hour)",
                                                (int) F4SimOutputs.FUEL_FLOW__FUEL_FLOW_POUNDS_PER_HOUR, typeof (float)));
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
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Position of glideslope ILS bar",
                                                (int) F4SimOutputs.ADI__ILS_HORIZONTAL_BAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "Position of localizer ILS bar",
                                                (int) F4SimOutputs.ADI__ILS_VERTICAL_BAR_POSITION, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "OFF flag", (int) F4SimOutputs.ADI__OFF_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "AUX flag", (int) F4SimOutputs.ADI__AUX_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "GS flag", (int) F4SimOutputs.ADI__GS_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("ADI", "LOC flag", (int) F4SimOutputs.ADI__LOC_FLAG, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("STBY ADI", "OFF flag", (int) F4SimOutputs.STBY_ADI__OFF_FLAG,
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
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Distance to beacon (nautical miles)",
                                                (int) F4SimOutputs.HSI__DISTANCE_TO_BEACON_NAUTICAL_MILES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Bearing to beacon (degrees)",
                                                (int) F4SimOutputs.HSI__BEARING_TO_BEACON_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Current heading (degrees)",
                                                (int) F4SimOutputs.HSI__CURRENT_HEADING_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Desired heading (degrees)",
                                                (int) F4SimOutputs.HSI__DESIRED_HEADING_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Localizer needle limit(degrees)",
                                                (int) F4SimOutputs.HSI__LOCALIZER_NEEDLE_LIMIT_DEGREES, typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "Localizer needle position (degrees)",
                                                (int) F4SimOutputs.HSI__LOCALIZER_NEEDLE_POSITION_DEGREES,
                                                typeof (float)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "TO Flag", (int) F4SimOutputs.HSI__TO_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "FROM Flag", (int) F4SimOutputs.HSI__FROM_FLAG, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("HSI", "OFF Flag", (int) F4SimOutputs.HSI__OFF_FLAG, typeof (bool)));
            //            AddF4SimOutput(CreateNewF4SimOutput("HSI", "selected mode is NAV", (int)F4SimOutputs.HSI__SELECTED_MODE_IS_NAV, typeof(bool)));
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
                                                (int) F4SimOutputs.AUX_COMM__TACAN_BAND_IS_X, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Up-Front Controls", "Tacan Mode is AA",
                                                (int) F4SimOutputs.AUX_COMM__TACAN_MODE_IS_AA, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "TACAN Channel",
                                                (int) F4SimOutputs.AUX_COMM__TACAN_CHANNEL, typeof (int)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "Tacan Band is X",
                                                (int) F4SimOutputs.UFC__TACAN_BAND_IS_X, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("AUX COMM Panel", "Tacan Mode is AA",
                                                (int) F4SimOutputs.UFC__TACAN_MODE_IS_AA, typeof (bool)));


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

            /*
             * MISSING CAUTION PANEL FLAGS
            ADC
            ATF NOT ENGAGED
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
                                                (int) F4SimOutputs.TWP__PRIORITY_MODE_OPEN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "UNKNOWN Indicator Light",
                                                (int) F4SimOutputs.TWP__UNKNOWN, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "NAVAL Indicator Light",
                                                (int) F4SimOutputs.TWP__NAVAL, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Threat Warning Prime Panel", "TGT SEP Indicator Light",
                                                (int) F4SimOutputs.TWP__TARGET_SEP, typeof (bool)));


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

            AddF4SimOutput(CreateNewF4SimOutput("MARKER BEACON", "Outer marker flag",
                                                (int) F4SimOutputs.MARKER_BEACON__OUTER_MARKER, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("MARKER BEACON", "Middle marker flag",
                                                (int) F4SimOutputs.MARKER_BEACON__MIDDLE_MARKER, typeof (bool)));

            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Weight-On-Wheels Sensor", (int) F4SimOutputs.AIRCRAFT__WOW,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Electrical Power OFF flag",
                                                (int) F4SimOutputs.AIRCRAFT__ELEC_POWER_OFF, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Main Power flag", (int) F4SimOutputs.AIRCRAFT__MAIN_POWER,
                                                typeof (int)));
            AddF4SimOutput(CreateNewF4SimOutput("Aircraft", "Engine 2 Fire flag",
                                                (int) F4SimOutputs.AIRCRAFT__ENGINE_2_FIRE, typeof (bool)));


            AddF4SimOutput(CreateNewF4SimOutput("SIM", "SHOOT cue light", (int) F4SimOutputs.SIM__SHOOT_CUE,
                                                typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "LOCK cue light", (int) F4SimOutputs.SIM__LOCK_CUE, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "BMS 'PILOT IS FLYING' flag",
                                                (int) F4SimOutputs.SIM__BMS_PLAYER_IS_FLYING, typeof (bool)));
            AddF4SimOutput(CreateNewF4SimOutput("SIM", "Shared Memory Area Data Format Version Number",
                                                (int) F4SimOutputs.SIM__SHARED_MEM_VERSION, typeof (int)));

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
        }

        public override void Update()
        {
            GetNextFlightDataFromSharedMem();
            UpdateSimOutputValues();
        }
    }
}