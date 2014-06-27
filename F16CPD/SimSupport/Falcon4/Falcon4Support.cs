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
using F16CPD.SimSupport.Falcon4.Networking;
using F16CPD.SimSupport.Falcon4.EventHandlers;

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
        private IFalconCallbackSender _falconCallbackSender;
        private IFalconDataFormatDetector _falconDataFormatDetector;
        private IClientSideInboundMessageProcessor _clientSideInboundMessageProcessor;
        private IServerSideInboundMessageProcessor _serverSideInboundMessageProcessor;
        private IInputControlEventHandler _inputControlEventHandler;
        #endregion

        public Falcon4Support(F16CpdMfdManager manager, Mediator mediator)
        {
            _mediator = mediator;
            Manager = manager;
            
            InitializeFlightData();
            _morseCodeGenerator = new MorseCode {CharactersPerMinute = 53};
            _morseCodeGenerator.UnitTimeTick += MorseCodeUnitTimeTick;
            _dedAlowReader = new DEDAlowReader();
            _inputControlEventHandler = new InputControlEventHandler(Manager);
            _falconDataFormatDetector = new FalconDataFormatDetector(Manager);
            _falconCallbackSender = new FalconCallbackSender(Manager);

            _clientSideInboundMessageProcessor = new ClientSideInboundMessageProcessor();
            _serverSideInboundMessageProcessor = new ServerSideInboundMessageProcessor(Manager);
            
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
                PerformClientSideFlightDataUpdates();
                return;
            }

            var flightData = Manager.FlightData;

            _curFalconDataFormat = _falconDataFormatDetector.DetectFalconDataFormat();
            var exePath = F4Utils.Process.Util.GetFalconExePath();
            CreateSharedMemReaderIfNotExists();
            var fromFalcon = ReadF4SharedMem();

            if (_keyFile == null)
            {
                LoadCurrentKeyFile();
            }
            EnsureTerrainIsLoaded();

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
                    TurnOffADI(flightData);
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
                    TurnOffHSI(flightData);
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
                UpdateMapPosition(flightData, fromFalcon);
            }
            else //Falcon's not running
            {
                _isSimRunning = false;
                if (Settings.Default.ShutoffIfFalconNotRunning)
                {
                    TurnOffAllInstruments(flightData);
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

        private void PerformClientSideFlightDataUpdates()
        {
            try
            {
                var serializedFlightData = (string)Manager.Client.GetSimProperty("F4FlightData");
                FlightData fromServer = null;
                if (!String.IsNullOrEmpty(serializedFlightData))
                {
                    _isSimRunning = true;
                    fromServer = (FlightData)Common.Serialization.Util.FromRawBytes(serializedFlightData);
                    UpdateNewServerFlightDataWithCertainExistingClientFlightData(fromServer);
                    Manager.FlightData = fromServer;

                    var outerMarkerFromFalcon = Manager.FlightData.MarkerBeaconOuterMarkerFlag;
                    var middleMarkerFromFalcon = Manager.FlightData.MarkerBeaconMiddleMarkerFlag;

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
        }

        private static void TurnOffAllInstruments(FlightData flightData)
        {
            flightData.VviOffFlag = true;
            flightData.AoaOffFlag = true;
            flightData.HsiOffFlag = true;
            flightData.AdiOffFlag = true;
            flightData.CpdPowerOnFlag = false;
            flightData.RadarAltimeterOffFlag = true;
            flightData.PfdOffFlag = true;
        }

        private void UpdateMapPosition(FlightData flightData, F4SharedMem.FlightData fromFalcon)
        {
            int latWholeDegrees;
            float latMinutes;
            int longWholeDegrees;
            float longMinutes;
            _latLongCalculator.CalculateLatLong(_terrainDB, fromFalcon.x, fromFalcon.y, out latWholeDegrees, out latMinutes,
                                             out longWholeDegrees, out longMinutes);
            flightData.LatitudeInDecimalDegrees = latWholeDegrees + (latMinutes / 60.0f);
            flightData.LongitudeInDecimalDegrees = longWholeDegrees + (longMinutes / 60.0f);
            flightData.MapCoordinateFeetEast = fromFalcon.y;
            flightData.MapCoordinateFeetNorth = fromFalcon.x;
        }

        private static void TurnOffHSI(FlightData flightData)
        {
            flightData.HsiDistanceInvalidFlag = true;
            flightData.HsiDeviationInvalidFlag = false;
            flightData.HsiCourseDeviationLimitInDecimalDegrees = 0;
            flightData.HsiCourseDeviationInDecimalDegrees = 0;
            flightData.HsiLocalizerDeviationInDecimalDegrees = 0;
            flightData.HsiBearingToBeaconInDecimalDegrees = 0;
            flightData.HsiDistanceToBeaconInNauticalMiles = 0;
        }

        private static void TurnOffADI(FlightData flightData)
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
                _curFalconDataFormat = _falconDataFormatDetector.DetectFalconDataFormat();
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

       

        #endregion
        public void HandleInputControlEvent(CpdInputControls eventSource, MfdInputControl control)
        {
            _inputControlEventHandler.HandleInputControlEvent(eventSource, control);
        }

        public bool ProcessPendingMessageToClientFromServer(Message message)
        {
            return _clientSideInboundMessageProcessor.ProcessPendingMessage(message);
        }
        public bool ProcessPendingMessageToServerFromClient(Message message)
        {
            return _serverSideInboundMessageProcessor.ProcessPendingMessage(message);
        }


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