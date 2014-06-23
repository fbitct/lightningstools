using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Common.InputSupport.DirectInput;
using F16CPD.FlightInstruments;
using F16CPD.Mfd.Controls;
using F16CPD.Networking;
using F16CPD.Properties;
using F4KeyFile;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Terrain;
using log4net;
using Message = F16CPD.Networking.Message;

namespace F16CPD.SimSupport.Falcon4
{
    //TODO: PRIO create configurable reset key
    //TODO: PRIO blank RALTs in certain attitudes
    //TODO: PRIO is there a way to read initial switch state in falcon and synchronize to that?
    internal sealed class Falcon4Support : ISimSupportModule, IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Falcon4Support));

        #region Instance variables

        private const TacanBand _backupTacanBand = TacanBand.X;
        private const bool _cpdPowerOn = true;
        private readonly MorseCode _morseCodeGenerator;
        private FalconDataFormats? _curFalconDataFormat;
        private bool _isDisposed;
        private bool _isSendingInput;
        private bool _isSimRunning;
        private KeyFile _keyFile;
        private Mediator _mediator;
        private bool _morseCodeSignalLineValue;
        private KeyWithModifiers _pendingComboKeys;
        private Queue<bool> _pendingMorseCodeUnits = new Queue<bool>();
        private Reader _sharedMemReader;
        private TacanChannelSource _tacanChannelSource = TacanChannelSource.Ufc;
        private TerrainDB _terrainDB;
        private Bitmap _theaterMapImage;
        private ITerrainHeightCalculator _terrainHeightCalulator = new TerrainHeightCalculator();
        private ILatLongCalculator _latLongCalculator = new LatLongCalculator();
        private ITerrainDBFactory _terrainDBFactory = new TerrainDBFactory();
        private IElevationPostCoordinateClamper _elevationPostCoordinateClamper = new ElevationPostCoordinateClamper();
        private IMovingMap _movingMap;
        private IDEDAlowReader _dedAlowReader;
        private IIndicatedRateOfTurnCalculator _indicatedRateOfTurnCalculator = new IndicatedRateOfTurnCalculator();
        #endregion

        public Falcon4Support(F16CpdMfdManager manager, Mediator mediator)
        {
            _mediator = mediator;
            Manager = manager;
            
            InitializeFlightData();
            /* REMOVED BECAUSE WE ARE NOT WATCHING FOR FALCON CALLBACKS AT THE MOMENT
            if (!Properties.Settings.Default.RunAsClient)
            {
                SetupDirectInputMonitoring();
            }
             */
            _morseCodeGenerator = new MorseCode {CharactersPerMinute = 53};
            _morseCodeGenerator.UnitTimeTick += MorseCodeUnitTimeTick;
            _dedAlowReader = new DEDAlowReader();
        }

        #region ISimSupportModule Members

        public void InitializeTestMode()
        {
            if (!Settings.Default.RunAsClient)
            {
                LoadCurrentKeyFile();
                EnsureTerrainIsLoaded();
            }
        }

        public bool IsSendingInput
        {
            get { return _isSendingInput; }
        }

        public F16CpdMfdManager Manager { get; set; }

        public bool IsSimRunning
        {
            get { return _isSimRunning; }
        }

        #endregion

        private void MorseCodeUnitTimeTick(object sender, UnitTimeTickEventArgs e)
        {
            _pendingMorseCodeUnits.Enqueue(e.CurrentSignalLineState);
        }

        private bool GetNextMorseCodeUnit()
        {
            var nextUnit = false;
            if (_pendingMorseCodeUnits.Count > 0)
            {
                nextUnit = _pendingMorseCodeUnits.Dequeue();
                _morseCodeSignalLineValue = nextUnit;
            }
            if (_pendingMorseCodeUnits.Count > 1000)
            {
                var units = _pendingMorseCodeUnits.ToArray();
                _pendingMorseCodeUnits.Clear();
                var newUnits = new Queue<bool>();
                for (var i = 0; i < 100; i++)
                {
                    newUnits.Enqueue(units[i]);
                }
                _pendingMorseCodeUnits = newUnits;
            }
            return nextUnit;
        }

        #region Flight Data/State Management

        public void InitializeFlightData()
        {
            var flightData = Manager.FlightData;
            flightData.AltimeterMode = AltimeterMode.Electronic;
            flightData.AutomaticLowAltitudeWarningInFeet = 300;
            flightData.BarometricPressureInDecimalInchesOfMercury = 29.92f;
            flightData.HsiCourseDeviationLimitInDecimalDegrees = 10;
            flightData.HsiDesiredCourseInDegrees = 0;
            flightData.HsiDesiredHeadingInDegrees = 0;
            flightData.HsiDeviationInvalidFlag = false;
            flightData.TransitionAltitudeInFeet = 18000;
            flightData.RateOfTurnInDecimalDegreesPerSecond = 0;
            flightData.VviOffFlag = false;
            flightData.AoaOffFlag = false;
            flightData.HsiOffFlag = false;
            flightData.AdiOffFlag = false;
            flightData.PfdOffFlag = false;
            flightData.RadarAltimeterOffFlag = false;
            flightData.CpdPowerOnFlag = true;
            flightData.MarkerBeaconOuterMarkerFlag = false;
            flightData.MarkerBeaconMiddleMarkerFlag = false;
            flightData.AdiEnableCommandBars = false;
            flightData.TacanChannel = "106X";
            _morseCodeSignalLineValue = false;
            _indicatedRateOfTurnCalculator.Reset();
            _isSimRunning = false;
        }

        public void UpdateManagerFlightData()
        {
            bool outerMarkerFromFalcon;
            bool middleMarkerFromFalcon;
            GetNextMorseCodeUnit();

            if (Settings.Default.RunAsClient)
            {
                try
                {
                    var serializedFlightData = (string) Manager.Client.GetSimProperty("F4FlightData");
                    FlightData fromServer = null;
                    if (!String.IsNullOrEmpty(serializedFlightData))
                    {
                        _isSimRunning = true;
                        fromServer = (FlightData) Common.Serialization.Util.FromRawBytes(serializedFlightData);
                        UpdateNewServerFlightDataWithCertainExistingClientFlightData(fromServer);
                        Manager.FlightData = fromServer;

                        outerMarkerFromFalcon = Manager.FlightData.MarkerBeaconOuterMarkerFlag;
                        middleMarkerFromFalcon = Manager.FlightData.MarkerBeaconMiddleMarkerFlag;

                        Manager.FlightData.MarkerBeaconOuterMarkerFlag &= _morseCodeSignalLineValue;
                        Manager.FlightData.MarkerBeaconMiddleMarkerFlag &= _morseCodeSignalLineValue;

                        if (outerMarkerFromFalcon)
                        {
                            if (_morseCodeGenerator != null)
                                if (_morseCodeGenerator.PlainText != "T")
                                {
                                    _morseCodeGenerator.PlainText = "T"; //dot
                                }
                        }
                        else if (middleMarkerFromFalcon)
                        {
                            if (_morseCodeGenerator != null)
                                if (_morseCodeGenerator.PlainText != "A")
                                {
                                    _morseCodeGenerator.PlainText = "A"; //dot-dash
                                }
                        }
                        if (_morseCodeGenerator != null)
                            if ((outerMarkerFromFalcon || middleMarkerFromFalcon) && !_morseCodeGenerator.Sending)
                            {
                                if (!_morseCodeGenerator.KeepSending)
                                {
                                    _morseCodeGenerator.KeepSending = true;
                                }
                                if (!_morseCodeGenerator.Sending)
                                {
                                    _pendingMorseCodeUnits.Clear();
                                    _morseCodeGenerator.StartSending();
                                }
                            }
                            else if (!outerMarkerFromFalcon && !middleMarkerFromFalcon)
                            {
                                _morseCodeGenerator.StopSending();
                                _pendingMorseCodeUnits.Clear();
                            }
                    }
                    else
                    {
                        _isSimRunning = false;
                    }
                    if (fromServer == null)
                    {
                        fromServer = new FlightData();
                        InitializeFlightData();
                        Manager.FlightData = fromServer;
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                return;
            }

            var flightData = Manager.FlightData;

            _curFalconDataFormat = DetectFalconFormat();
            var exePath = F4Utils.Process.Util.GetFalconExePath();
            CreateSharedMemReaderIfNotExists();
            var fromFalcon = ReadF4SharedMem();

            if (!Settings.Default.RunAsClient)
            {
                if (_keyFile == null)
                {
                    LoadCurrentKeyFile();
                }
                EnsureTerrainIsLoaded();
            }

            if (exePath != null && ((_sharedMemReader != null && _sharedMemReader.IsFalconRunning)))
            {
                _isSimRunning = true;
                if (fromFalcon == null) fromFalcon = new F4SharedMem.FlightData();
                var hsibits = ((HsiBits) fromFalcon.hsiBits);

                flightData.VviOffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                flightData.AoaOffFlag = ((hsibits & HsiBits.AOA) == HsiBits.AOA);
                flightData.HsiOffFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
                flightData.AdiOffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                flightData.PfdOffFlag = false;

                if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.BMS4)
                {
                    flightData.CpdPowerOnFlag = _cpdPowerOn &&
                                                ((fromFalcon.lightBits3 & (int) Bms4LightBits3.Power_Off) !=
                                                 (int) Bms4LightBits3.Power_Off);
                }
                else
                {
                    flightData.CpdPowerOnFlag = _cpdPowerOn;
                }

                flightData.RadarAltimeterOffFlag = ((fromFalcon.lightBits & (int) LightBits.RadarAlt) ==
                                                    (int) LightBits.RadarAlt);

                if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.BMS4)
                {
                    flightData.IndicatedAltitudeAboveMeanSeaLevelInDecimalFeet = -fromFalcon.aauz;
                }
                else
                {
                    flightData.IndicatedAltitudeAboveMeanSeaLevelInDecimalFeet = -fromFalcon.z;
                }
                try
                {
                    var terrainHeight = _terrainHeightCalulator.CalculateTerrainHeight (fromFalcon.x, fromFalcon.y, _terrainDB );
                    var agl = -fromFalcon.z - terrainHeight;

                    //reset AGL altitude to zero if we're on the ground
                    if (
                        ((fromFalcon.lightBits & (int) LightBits.WOW) == (int) LightBits.WOW)
                        ||
                        (
                            ((fromFalcon.lightBits3 & (int) Bms4LightBits3.OnGround) == (int) Bms4LightBits3.OnGround)
                            &&
                            _curFalconDataFormat == FalconDataFormats.BMS4
                        )
                        )
                    {
                        agl = 0;
                    }
                    if (agl < 0) agl = 0;
                    flightData.AltitudeAboveGroundLevelInDecimalFeet = agl;
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                    flightData.AltitudeAboveGroundLevelInDecimalFeet =
                        flightData.TrueAltitudeAboveMeanSeaLevelInDecimalFeet;
                }
                if (Settings.Default.UseAsiLockoutSpeed && (fromFalcon.kias < Settings.Default.AsiLockoutSpeedKnots))
                {
                    //lockout airspeed if under 60 knots
                    {
                        flightData.IndicatedAirspeedInDecimalFeetPerSecond = 0;
                    }
                }
                else
                {
                    flightData.IndicatedAirspeedInDecimalFeetPerSecond = fromFalcon.kias*Constants.FPS_PER_KNOT;
                }
                var newAlow = flightData.AutomaticLowAltitudeWarningInFeet;
                var foundNewAlow = _dedAlowReader.CheckDED_ALOW(fromFalcon, out newAlow);
                if (foundNewAlow)
                {
                    flightData.AutomaticLowAltitudeWarningInFeet = newAlow;
                }

                flightData.TrueAirspeedInDecimalFeetPerSecond = fromFalcon.vt;
                flightData.MachNumber = fromFalcon.mach;
                flightData.GroundSpeedInDecimalFeetPerSecond =
                    (float) Math.Sqrt((fromFalcon.xDot*fromFalcon.xDot) + (fromFalcon.yDot*fromFalcon.yDot));

                flightData.MagneticHeadingInDecimalDegrees = (360 +
                                                              (fromFalcon.yaw/Common.Math.Constants.RADIANS_PER_DEGREE))%
                                                             360;


                if (((hsibits & HsiBits.VVI) == HsiBits.VVI))
                {
                    flightData.VerticalVelocityInDecimalFeetPerSecond = 0;
                }
                else
                {
                    flightData.VerticalVelocityInDecimalFeetPerSecond = -fromFalcon.zDot;
                }

                flightData.AngleOfAttackInDegrees = ((hsibits & HsiBits.AOA) == HsiBits.AOA) ? 0 : fromFalcon.alpha;

                flightData.AdiAuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
                flightData.AdiGlideslopeInvalidFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
                flightData.AdiLocalizerInvalidFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);

                outerMarkerFromFalcon = ((fromFalcon.hsiBits & (int) HsiBits.OuterMarker) == (int) HsiBits.OuterMarker);
                middleMarkerFromFalcon = ((fromFalcon.hsiBits & (int) HsiBits.MiddleMarker) ==
                                          (int) HsiBits.MiddleMarker);

                if (Settings.Default.RunAsServer)
                {
                    flightData.MarkerBeaconOuterMarkerFlag = outerMarkerFromFalcon;
                    flightData.MarkerBeaconMiddleMarkerFlag = middleMarkerFromFalcon;
                }
                else
                {
                    flightData.MarkerBeaconOuterMarkerFlag = outerMarkerFromFalcon & _morseCodeSignalLineValue;
                    flightData.MarkerBeaconMiddleMarkerFlag = middleMarkerFromFalcon & _morseCodeSignalLineValue;

                    if (outerMarkerFromFalcon)
                    {
                        _morseCodeGenerator.PlainText = "T"; //dot
                    }
                    else if (middleMarkerFromFalcon)
                    {
                        _morseCodeGenerator.PlainText = "A"; //dot-dash
                    }
                    if ((outerMarkerFromFalcon || middleMarkerFromFalcon) && !_morseCodeGenerator.Sending)
                    {
                        _morseCodeGenerator.KeepSending = true;
                        _morseCodeGenerator.StartSending();
                    }
                    else if (!outerMarkerFromFalcon && !middleMarkerFromFalcon)
                    {
                        _morseCodeGenerator.StopSending();
                    }
                }

                if (((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF))
                {
                    //if the ADI is off
                    flightData.PitchAngleInDecimalDegrees = 0;
                    flightData.RollAngleInDecimalDegrees = 0;
                    flightData.BetaAngleInDecimalDegrees = 0;
                    flightData.GammaAngleInDecimalDegrees = 0;
                    flightData.AdiIlsGlideslopeDeviationInDecimalDegrees = 0;
                    flightData.AdiIlsLocalizerDeviationInDecimalDegrees = 0;
                    flightData.AdiEnableCommandBars = false;
                    flightData.WindOffsetToFlightPathMarkerInDecimalDegrees = 0;
                }
                else
                {
                    flightData.PitchAngleInDecimalDegrees = ((fromFalcon.pitch/Common.Math.Constants.RADIANS_PER_DEGREE));
                    flightData.RollAngleInDecimalDegrees = fromFalcon.roll/Common.Math.Constants.RADIANS_PER_DEGREE;
                    flightData.BetaAngleInDecimalDegrees = fromFalcon.beta;
                    flightData.GammaAngleInDecimalDegrees = fromFalcon.gamma/Common.Math.Constants.RADIANS_PER_DEGREE;
                    flightData.WindOffsetToFlightPathMarkerInDecimalDegrees = fromFalcon.windOffset/
                                                                              Common.Math.Constants.RADIANS_PER_DEGREE;

                    //The following floating data is also crossed up in the flightData.h File:
                    //float AdiIlsHorPos;       // Position of horizontal ILS bar ----Vertical
                    //float AdiIlsVerPos;       // Position of vertical ILS bar-----horizontal
                    var commandBarsOn = ((float) (Math.Abs(Math.Round(fromFalcon.AdiIlsHorPos, 4))) != 0.1745f);
                    if (
                        (Math.Abs((fromFalcon.AdiIlsVerPos/Common.Math.Constants.RADIANS_PER_DEGREE)) >
                         Pfd.ADI_ILS_GLIDESLOPE_DEVIATION_LIMIT_DECIMAL_DEGREES)
                        ||
                        (Math.Abs((fromFalcon.AdiIlsHorPos/Common.Math.Constants.RADIANS_PER_DEGREE)) >
                         Pfd.ADI_ILS_LOCALIZER_DEVIATION_LIMIT_DECIMAL_DEGREES)
                        )
                    {
                        commandBarsOn = false;
                    }
                    flightData.HsiDisplayToFromFlag = true;


                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & HsiBits.TotalFlags) != HsiBits.TotalFlags)
                    {
                        flightData.HsiDisplayToFromFlag = false;
                    }
                        //if the TO/FROM flag is showing in shared memory, then we are most likely in TACAN mode (except in F4AF which always has the bit turned on)
                    else if (
                        (
                            ((hsibits & HsiBits.ToTrue) == HsiBits.ToTrue)
                            ||
                            ((hsibits & HsiBits.FromTrue) == HsiBits.FromTrue)
                        )
                        )
                    {
                        if (!commandBarsOn) //better make sure we're not in any ILS mode too though
                        {
                            flightData.HsiDisplayToFromFlag = true;
                        }
                    }


                    //if the glideslope or localizer flags on the ADI are turned on, then we must be in an ILS mode and therefore we 
                    //know we don't need to show the HSI TO/FROM flags.
                    if (
                        ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS)
                        ||
                        ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC)
                        )
                    {
                        flightData.HsiDisplayToFromFlag = false;
                    }
                    if (commandBarsOn) flightData.HsiDisplayToFromFlag = false;

                    flightData.AdiEnableCommandBars = commandBarsOn;
                    flightData.AdiIlsGlideslopeDeviationInDecimalDegrees = fromFalcon.AdiIlsVerPos/
                                                                           Common.Math.Constants.RADIANS_PER_DEGREE;
                    flightData.AdiIlsLocalizerDeviationInDecimalDegrees = fromFalcon.AdiIlsHorPos/
                                                                          Common.Math.Constants.RADIANS_PER_DEGREE;


                    if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.BMS4)
                    {
                        /*
                        This value is called navMode and is unsigned char type with 4 possible values: ILS/TCN=0, TCN=1, NAV=2, ILS/NAV=3
                        */

                        var bmsNavMode = fromFalcon.navMode;
                        switch (bmsNavMode)
                        {
                            case 0: //NavModes.PlsTcn:
                                flightData.HsiDisplayToFromFlag = false;
                                break;
                            case 1: //NavModes.Tcn:
                                flightData.HsiDisplayToFromFlag = true;
                                flightData.AdiEnableCommandBars = false;
                                break;
                            case 2: //NavModes.Nav:
                                flightData.HsiDisplayToFromFlag = false;
                                flightData.AdiEnableCommandBars = false;
                                break;
                            case 3: //NavModes.PlsNav:
                                flightData.HsiDisplayToFromFlag = false;
                                break;
                            default:
                                break;
                        }
                    }
                }


                if (((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF))
                {
                    flightData.HsiDistanceInvalidFlag = true;
                    flightData.HsiDeviationInvalidFlag = false;
                    flightData.HsiCourseDeviationLimitInDecimalDegrees = 0;
                    flightData.HsiCourseDeviationInDecimalDegrees = 0;
                    flightData.HsiLocalizerDeviationInDecimalDegrees = 0;
                    flightData.HsiBearingToBeaconInDecimalDegrees = 0;
                    flightData.HsiDistanceToBeaconInNauticalMiles = 0;
                }
                else
                {
                    flightData.HsiDistanceInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
                    flightData.HsiDeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
                    flightData.HsiCourseDeviationLimitInDecimalDegrees = fromFalcon.deviationLimit;
                    flightData.HsiCourseDeviationInDecimalDegrees = fromFalcon.courseDeviation;
                    flightData.HsiLocalizerDeviationInDecimalDegrees = fromFalcon.localizerCourse;
                    flightData.HsiDesiredCourseInDegrees = (int) fromFalcon.desiredCourse;
                    flightData.HsiDesiredHeadingInDegrees = (int) fromFalcon.desiredHeading;
                    flightData.HsiBearingToBeaconInDecimalDegrees = fromFalcon.bearingToBeacon;
                    flightData.HsiDistanceToBeaconInNauticalMiles = fromFalcon.distanceToBeacon;
                }

                _indicatedRateOfTurnCalculator.DetermineIndicatedRateOfTurn(flightData);

                if (flightData.VerticalVelocityInDecimalFeetPerSecond > 0 &&
                    flightData.IndicatedAltitudeAboveMeanSeaLevelInDecimalFeet > flightData.TransitionAltitudeInFeet)
                {
                    ResetBaroPressureSettingToStandard(flightData);
                }

                if (_tacanChannelSource == TacanChannelSource.Backup)
                {
                    flightData.TacanChannel = fromFalcon.AUXTChan + Enum.GetName(typeof (TacanBand), _backupTacanBand);
                }
                else if (_tacanChannelSource == TacanChannelSource.Ufc)
                {
                    flightData.TacanChannel = fromFalcon.UFCTChan.ToString();
                }
                int latWholeDegrees;
                float latMinutes;
                int longWholeDegrees;
                float longMinutes;
                _latLongCalculator.CalculateLatLong(_terrainDB, fromFalcon.x, fromFalcon.y, out latWholeDegrees, out latMinutes,
                                                 out longWholeDegrees, out longMinutes);
                flightData.LatitudeInDecimalDegrees = latWholeDegrees + (latMinutes/60.0f);
                flightData.LongitudeInDecimalDegrees = longWholeDegrees + (longMinutes/60.0f);
                flightData.MapCoordinateFeetEast = fromFalcon.y;
                flightData.MapCoordinateFeetNorth = fromFalcon.x;
            }
            else //Falcon's not running
            {
                _isSimRunning = false;
                if (Settings.Default.ShutoffIfFalconNotRunning)
                {
                    flightData.VviOffFlag = true;
                    flightData.AoaOffFlag = true;
                    flightData.HsiOffFlag = true;
                    flightData.AdiOffFlag = true;
                    flightData.CpdPowerOnFlag = false;
                    flightData.RadarAltimeterOffFlag = true;
                    flightData.PfdOffFlag = true;
                }
                if (Settings.Default.RunAsServer)
                {
                    F16CPDServer.SetSimProperty("SimName", null);
                    F16CPDServer.SetSimProperty("SimVersion", null);
                    F16CPDServer.SetSimProperty("F4FlightData", null);
                }

                if (_sharedMemReader != null)
                {
                    Common.Util.DisposeObject(_sharedMemReader);
                    _sharedMemReader = null;
                }

                Common.Util.DisposeObject(_keyFile);
                Common.Util.DisposeObject(_terrainDB);
                _keyFile = null;
                _terrainDB = null;
            }

            //if running in server mode, send updated flight data to client 
            if (Settings.Default.RunAsServer)
            {
                F16CPDServer.SetSimProperty("F4FlightData", Common.Serialization.Util.ToRawBytes(flightData));
            }
        }

        private void UpdateNewServerFlightDataWithCertainExistingClientFlightData(FlightData newServerFlightData)
        {
            //TODO: move all these variables to private state inside the manager
            var existingFlightData = Manager.FlightData;
            newServerFlightData.AltimeterMode = existingFlightData.AltimeterMode;
            newServerFlightData.BarometricPressureInDecimalInchesOfMercury =
                existingFlightData.BarometricPressureInDecimalInchesOfMercury;
            newServerFlightData.TransitionAltitudeInFeet = existingFlightData.TransitionAltitudeInFeet;
        }

        private static void ResetBaroPressureSettingToStandard(FlightData flightData)
        {
            flightData.BarometricPressureInDecimalInchesOfMercury = 29.92f;
        }

        private void EnsureTerrainIsLoaded()
        {
            if (_terrainDB == null)
            {
                _terrainDB = _terrainDBFactory.Create(true);
            }
        }





        #endregion


        #region Network messaging event handlers

        public bool ProcessPendingMessageToServerFromClient(Message pendingMessage)
        {
            if (!Settings.Default.RunAsServer) return false;
            var toReturn = false;
            if (pendingMessage != null)
            {
                var messageType = pendingMessage.MessageType;
                if (messageType != null)
                {
                    switch (messageType)
                    {
                        case "Falcon4SendCallbackMessage":
                            var callback = (string) pendingMessage.Payload;
                            SendCallbackToFalcon(callback);
                            toReturn = true;
                            break;
                        case "Falcon4IncreaseALOW":
                            {
                                IncreaseAlow();
                                toReturn = true;
                            }
                            break;

                        case "Falcon4DecreaseALOW":
                            {
                                DecreaseAlow();
                                toReturn = true;
                            }
                            break;
                        case "Falcon4IncreaseBaro":
                            {
                                IncreaseBaro();
                                toReturn = true;
                            }
                            break;

                        case "Falcon4DecreaseBaro":
                            {
                                DecreaseBaro();
                                toReturn = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return toReturn;
        }

        public bool ProcessPendingMessageToClientFromServer(Message pendingMessage)
        {
            if (!Settings.Default.RunAsClient) return false;
            var toReturn = false;
            if (pendingMessage != null)
            {
                var messageType = pendingMessage.MessageType;
                if (messageType != null)
                {
                    switch (messageType)
                    {
                        case "Falcon4CallbackOccurredMessage":
                            var callback = (string) pendingMessage.Payload;
                            ProcessDetectedCallback(callback);
                            toReturn = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            return toReturn;
        }

        private static void ProcessDetectedCallback(string callback)
        {
            if (String.IsNullOrEmpty(callback) || String.IsNullOrEmpty(callback.Trim())) return;
            /*
            if (callback.ToLowerInvariant().Trim() == "SimHSIModeInc".ToLowerInvariant() )
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.Nav;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimStepHSIMode".ToLowerInvariant())
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.Nav;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIModeDec".ToLowerInvariant())
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.Nav;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIIlsNav".ToLowerInvariant())
            {
                _currentNavMode = NavModes.PlsNav;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIIlsTcn".ToLowerInvariant())
            {
                _currentNavMode = NavModes.PlsTcn;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSITcn".ToLowerInvariant())
            {
                _currentNavMode = NavModes.Tcn;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSINav".ToLowerInvariant())
            {
                _currentNavMode = NavModes.Nav;
            }
            */
            /*
            if (callback.ToLowerInvariant().Trim() == "SimAuxComBackup".ToLowerInvariant())
            {
                _tacanChannelSource = TacanChannelSource.Backup;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimAuxComUFC".ToLowerInvariant())
            {
                _tacanChannelSource = TacanChannelSource.Ufc;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimToggleAuxComMaster".ToLowerInvariant())
            {
                if (_tacanChannelSource == TacanChannelSource.Backup)
                {
                    _tacanChannelSource = TacanChannelSource.Ufc;
                }
                else
                {
                    _tacanChannelSource = TacanChannelSource.Backup;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimCycleBandAuxComDigit".ToLowerInvariant())
            {
                if (_backupTacanBand == TacanBand.X)
                {
                    _backupTacanBand = TacanBand.Y;
                }
                else
                {
                    _backupTacanBand = TacanBand.X;
                }
            }
            */
            if (Settings.Default.RunAsServer)
            {
                var message = new Message("Falcon4CallbackOccurredMessage", callback.Trim());
                F16CPDServer.SubmitMessageToClient(message);
            }
        }

        #endregion

        #region CPD Physical Input Control handlesr

        #region CPD Input Control Event Handler Dispatch Routines

        public void HandleInputControlEvent(CpdInputControls eventSource, MfdInputControl control)
        {
            OptionSelectButton button;
            switch (eventSource)
            {
                case CpdInputControls.OsbButton1:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton2:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton3:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton4:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton5:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton6:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton7:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton8:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton9:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton10:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton11:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton12:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton13:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton14:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton15:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton16:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton17:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton18:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton19:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton20:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton21:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton22:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton23:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton24:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton25:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.OsbButton26:
                    button = control as OptionSelectButton;
                    HandleOptionSelectButtonPress(button);
                    break;
                case CpdInputControls.HsiModeTcn:
                    SetHsiModeTcn();
                    break;
                case CpdInputControls.HsiModeIlsTcn:
                    SetHsiModePlsTcn();
                    break;
                case CpdInputControls.HsiModeNav:
                    SetHsiModeNav();
                    break;
                case CpdInputControls.HsiModeIlsNav:
                    SetHsiModePlsNav();
                    break;
                case CpdInputControls.ParameterAdjustKnobIncrease:
                    break;
                case CpdInputControls.ParameterAdjustKnobDecrease:
                    break;
                case CpdInputControls.FuelSelectTest:
                    SetFuelSelectTest();
                    break;
                case CpdInputControls.FuelSelectNorm:
                    SetFuelSelectNorm();
                    break;
                case CpdInputControls.FuelSelectRsvr:
                    SetFuelSelectRsvr();
                    break;
                case CpdInputControls.FuelSelectIntWing:
                    SetFuelSelectIntWing();
                    break;
                case CpdInputControls.FuelSelectExtWing:
                    SetFuelSelectExtWing();
                    break;
                case CpdInputControls.FuelSelectExtCtr:
                    SetFuelSelectExtCtr();
                    break;
                case CpdInputControls.ExtFuelSwitchTransNorm:
                    SetExtFuelSwitchTransNorm();
                    break;
                case CpdInputControls.ExtFuelSwitchTransWingFirst:
                    SetExtFuelSwitchTransWingFirst();
                    break;
            }
        }

        public void HandleOptionSelectButtonPress(OptionSelectButton button)
        {
            var functionName = button.FunctionName;
            if (!String.IsNullOrEmpty(functionName))
            {
                switch (functionName)
                {
                    case "CourseSelectIncrease":
                        {
                            var format = _curFalconDataFormat;
                            var useIncrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsIncBy1");
                                if (incByOneCallback != null &&
                                    incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                                {
                                    useIncrementByOne = true;
                                }
                            }
                            SendCallbackToFalcon(useIncrementByOne ? "SimHsiCrsIncBy1" : "SimHsiCourseInc");
                        }
                        break;
                    case "CourseSelectDecrease":
                        {
                            var format = _curFalconDataFormat;
                            var useDecrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
                                if (decByOneCallback != null &&
                                    decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                                {
                                    useDecrementByOne = true;
                                }
                            }
                            SendCallbackToFalcon(useDecrementByOne ? "SimHsiCrsDecBy1" : "SimHsiCourseDec");
                        }
                        break;
                    case "HeadingSelectIncrease":
                        {
                            var format = _curFalconDataFormat;
                            var useIncrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgIncBy1");
                                if (incByOneCallback != null &&
                                    incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                                {
                                    useIncrementByOne = true;
                                }
                            }
                            SendCallbackToFalcon(useIncrementByOne ? "SimHsiHdgIncBy1" : "SimHsiHeadingInc");
                        }

                        break;
                    case "HeadingSelectDecrease":
                        {
                            var format = _curFalconDataFormat;
                            var useDecrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                var decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgDecBy1");
                                if (decByOneCallback != null &&
                                    decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                                {
                                    useDecrementByOne = true;
                                }
                            }
                            SendCallbackToFalcon(useDecrementByOne ? "SimHsiHdgDecBy1" : "SimHsiHeadingDec");
                        }
                        break;
                    case "BarometricPressureSettingIncrease":
                        IncreaseBaro();
                        break;
                    case "BarometricPressureSettingDecrease":
                        DecreaseBaro();
                        break;
                    case "LowAltitudeWarningThresholdIncrease":
                        IncreaseAlow();
                        break;
                    case "LowAltitudeWarningThresholdDecrease":
                        DecreaseAlow();
                        break;
                    case "AcknowledgeMessage":
                        //SendCallbackToFalcon("SimICPFAck");
                        break;
                    default:
                        break;
                }
            }
        }

        private void DecreaseAlow()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4DecreaseALOW", Manager.FlightData.AutomaticLowAltitudeWarningInFeet);
                Manager.Client.SendMessageToServer(message);
            }
            else
            {
                if (Manager.FlightData.AutomaticLowAltitudeWarningInFeet > 1000)
                {
                    Manager.FlightData.AutomaticLowAltitudeWarningInFeet -= 1000;
                }
                else
                {
                    Manager.FlightData.AutomaticLowAltitudeWarningInFeet -= 100;
                }
                SendCallbackToFalcon("DecreaseAlow");
            }
        }

        private void IncreaseAlow()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4IncreaseALOW", Manager.FlightData.AutomaticLowAltitudeWarningInFeet);
                Manager.Client.SendMessageToServer(message);
            }
            else
            {
                if (Manager.FlightData.AutomaticLowAltitudeWarningInFeet < 1000)
                {
                    Manager.FlightData.AutomaticLowAltitudeWarningInFeet += 100;
                }
                else
                {
                    Manager.FlightData.AutomaticLowAltitudeWarningInFeet += 1000;
                }
                SendCallbackToFalcon("IncreaseAlow");
            }
        }

        private void IncreaseBaro()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4IncreaseBaro",
                                          Manager.FlightData.BarometricPressureInDecimalInchesOfMercury);
                Manager.Client.SendMessageToServer(message);
            }
            else
            {
                Manager.FlightData.BarometricPressureInDecimalInchesOfMercury += 0.01f;
                SendCallbackToFalcon("SimAltPressInc");
            }
        }

        private void DecreaseBaro()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4DecreaseBaro",
                                          Manager.FlightData.BarometricPressureInDecimalInchesOfMercury);
                Manager.Client.SendMessageToServer(message);
            }
            else
            {
                Manager.FlightData.BarometricPressureInDecimalInchesOfMercury -= 0.01f;
                SendCallbackToFalcon("SimAltPressDec");
            }
        }

        #endregion

        #region CPD Input Control Event Handler Worker Routines

        private void SetExtFuelSwitchTransWingFirst()
        {
            SendCallbackToFalcon("SimFuelTransWing");
        }

        private void SetExtFuelSwitchTransNorm()
        {
            SendCallbackToFalcon("SimFuelTransNorm");
        }

        private void SetFuelSelectExtCtr()
        {
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelCenterExt");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchCenterExt");
            }
        }

        private void SetFuelSelectExtWing()
        {
            //TODO: PRIO check these and all other callback names against FreeFalcon 5
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelWingExt");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchWingExt");
            }
        }

        private void SetFuelSelectIntWing()
        {
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelWingInt");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchWingInt");
            }
        }

        private void SetFuelSelectRsvr()
        {
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelResv");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchResv");
            }
        }

        private void SetFuelSelectNorm()
        {
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelNorm");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchNorm");
            }
        }

        private void SetFuelSelectTest()
        {
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimFuelTest");
            }
            else
            {
                SendCallbackToFalcon("SimFuelSwitchTest");
            }
        }

        private void SetHsiModePlsNav()
        {
            //_currentNavMode = NavModes.PlsNav;
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimIlsNav");
            }
            else
            {
                SendCallbackToFalcon("SimHSIIlsNav");
            }
        }

        private void SetHsiModeNav()
        {
            //_currentNavMode = NavModes.Nav;
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimNav");
            }
            else
            {
                SendCallbackToFalcon("SimHSINav");
            }
        }

        private void SetHsiModePlsTcn()
        {
            //_currentNavMode = NavModes.PlsTcn;
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimIlsTcn");
            }
            else
            {
                SendCallbackToFalcon("SimHSIIlsTcn");
            }
        }

        private void SetHsiModeTcn()
        {
            //_currentNavMode = NavModes.Tcn;
            if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                SendCallbackToFalcon("SimTcn");
            }
            else
            {
                SendCallbackToFalcon("SimHSITcn");
            }
        }

        #endregion

        #endregion

        #region Falcon Process Detection and Manipulation Functions

        private KeyBinding FindKeyBinding(string callback)
        {
            if (Settings.Default.RunAsServer || (!Settings.Default.RunAsServer && !Settings.Default.RunAsClient))
            {
                return FindKeyBindingLocal(callback);
            }
            return null;
        }

        private static KeyBinding FindKeyBindingLocal(string callback)
        {
            return F4Utils.Process.KeyFileUtils.FindKeyBinding(callback);
        }


        private void SendCallbackToFalcon(string callback)
        {
            if (!Settings.Default.RunAsClient)
            {
                var startTime = DateTime.Now;
                _log.Debug("Sending callback:" + callback + " to Falcon.");
                SendCallbackToFalconLocal(callback);
                _log.Debug("Finished sending callback:" + callback + " to Falcon.");
                var endTime = DateTime.Now;
                var elapsed = endTime.Subtract(startTime);
                _log.Debug(string.Format("Time taken to send callback to Falcon: {0} milliseconds",
                                         elapsed.TotalMilliseconds));
            }
            else if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4SendCallbackMessage", callback);
                Manager.Client.SendMessageToServer(message);
            }
        }

        private void SendCallbackToFalconLocal(string callback)
        {
            _isSendingInput = true;
            F4Utils.Process.KeyFileUtils.SendCallbackToFalcon(callback);
            _isSendingInput = false;
        }

        private F4SharedMem.FlightData ReadF4SharedMem()
        {
            var toReturn = new F4SharedMem.FlightData();

            CreateSharedMemReaderIfNotExists();
            if (_sharedMemReader != null)
            {
                toReturn = _sharedMemReader.GetCurrentData();
            }
            return toReturn;
        }

        private void CreateSharedMemReaderIfNotExists()
        {
            if (_sharedMemReader == null && !Settings.Default.RunAsClient)
            {
                _curFalconDataFormat = DetectFalconFormat();
                if (_curFalconDataFormat.HasValue)
                {
                    _sharedMemReader = new Reader(_curFalconDataFormat.Value);
                }
            }
        }

        private void LoadCurrentKeyFile()
        {
            if (!Settings.Default.RunAsClient)
            {
                _keyFile = F4Utils.Process.KeyFileUtils.GetCurrentKeyFile();
            }
        }

        private FalconDataFormats? DetectFalconFormat()
        {
            FalconDataFormats? toReturn = null;
            //if we're running as the server or we're running in standalone mode
            if (Settings.Default.RunAsServer || (!Settings.Default.RunAsServer && !Settings.Default.RunAsClient))
            {
                toReturn = F4Utils.Process.Util.DetectFalconFormat();
                if (Settings.Default.RunAsServer)
                {
                    F16CPDServer.SetSimProperty("SimName", "Falcon4");
                    F16CPDServer.SetSimProperty("SimVersion",
                                                toReturn.HasValue
                                                    ? Enum.GetName(typeof (FalconDataFormats), toReturn)
                                                    : null);
                }
            }
            else if (Settings.Default.RunAsClient)
            {
                var simName = (string) Manager.Client.GetSimProperty("SimName");
                var simVersion = (string) Manager.Client.GetSimProperty("SimVersion");
                if (simName != null && simName.ToLowerInvariant() == "falcon4")
                {
                    toReturn = (FalconDataFormats) Enum.Parse(typeof (FalconDataFormats), simVersion);
                }
            }

            return toReturn;
        }

        #endregion
        public void RenderMap(Graphics g, Rectangle renderRect, float mapScale, int rangeRingDiameterInNauticalMiles,
               MapRotationMode rotationMode)
        {
            if (_movingMap == null)
            {
                _movingMap = new MovingMap(Manager, _terrainDB);
            }
            _movingMap.RenderMap(g, renderRect, mapScale, rangeRingDiameterInNauticalMiles, rotationMode);
        }


        #region Destructors

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Falcon4Support()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(_keyFile);
                    _keyFile = null;
                    Common.Util.DisposeObject(_sharedMemReader);
                    _sharedMemReader = null;
                    Common.Util.DisposeObject(_pendingComboKeys);
                    _pendingComboKeys = null;
                    Common.Util.DisposeObject(_terrainDB);
                    _terrainDB = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion


    }
}