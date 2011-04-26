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
    [Serializable]
    public enum TacanChannelSource
    {
        Ufc,
        Backup
    }

    [Serializable]
    public enum TacanBand
    {
        X,
        Y
    }

    //TODO: PRIO create configurable reset key
    //TODO: PRIO blank RALTs in certain attitudes
    //TODO: PRIO is there a way to read initial switch state in falcon and synchronize to that?
    internal sealed class Falcon4Support : ISimSupportModule, IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Falcon4Support));

        #region Instance variables

        private readonly object _mapImageLock = new object();
        private readonly MorseCode _morseCodeGenerator;
        private const TacanBand _backupTacanBand = TacanBand.X;
        private const bool _cpdPowerOn = true;
        private FalconDataFormats? _curFalconDataFormat;
        private bool _isDisposed;
        private bool _isSendingInput;
        private bool _isSimRunning;
        private KeyFile _keyFile;
        private TimestampedFloatValue _lastHeadingSample;
        private List<TimestampedFloatValue> _lastInstantaneousRatesOfTurn = new List<TimestampedFloatValue>();
        private float _lastMapScale;
        private int _lastRangeRingDiameterNauticalMiles;
        private Bitmap _lastRenderedMapImage;
        private Bitmap _mapAirplaneBitmap;
        private int _mapRenderProgress;
        private BackgroundWorker _mapRenderingBackgroundWorker;
        private DoWorkEventHandler _mapRenderingBackgroundWorkerDoWorkDelegate;
        private Mediator _mediator;
        private bool _morseCodeSignalLineValue;
        private KeyWithModifiers _pendingComboKeys;
        private Queue<bool> _pendingMorseCodeUnits = new Queue<bool>();
        private Reader _sharedMemReader;
        private TacanChannelSource _tacanChannelSource = TacanChannelSource.Ufc;
        private TerrainBrowser _terrainBrowser;
        private Bitmap _theaterMapImage;

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
        }

        #region ISimSupportModule Members

        public void InitializeTestMode()
        {
            if (!Settings.Default.RunAsClient)
            {
                LoadCurrentKeyFile();
                EnsureTerrainBrowserLoaded();
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
            bool nextUnit = false;
            if (_pendingMorseCodeUnits.Count > 0)
            {
                nextUnit = _pendingMorseCodeUnits.Dequeue();
                _morseCodeSignalLineValue = nextUnit;
            }
            if (_pendingMorseCodeUnits.Count > 1000)
            {
                bool[] units = _pendingMorseCodeUnits.ToArray();
                _pendingMorseCodeUnits.Clear();
                var newUnits = new Queue<bool>();
                for (int i = 0; i < 100; i++)
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
            FlightData flightData = Manager.FlightData;
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
            _lastInstantaneousRatesOfTurn = new List<TimestampedFloatValue>();
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

            FlightData flightData = Manager.FlightData;

            _curFalconDataFormat = DetectFalconFormat();
            string exePath = F4Utils.Process.Util.GetFalconExePath();
            CreateSharedMemReaderIfNotExists();
            F4SharedMem.FlightData fromFalcon = ReadF4SharedMem();

            if (!Settings.Default.RunAsClient)
            {
                if (_keyFile == null)
                {
                    LoadCurrentKeyFile();
                }
                EnsureTerrainBrowserLoaded();
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
                //flightData.PfdOffFlag = (flightData.VviOffFlag && flightData.AoaOffFlag && flightData.HsiOffFlag && flightData.AdiOffFlag);
                flightData.PfdOffFlag = false;

                if (_curFalconDataFormat.HasValue && _curFalconDataFormat.Value == FalconDataFormats.BMS4)
                {
                    //&& ((fromFalcon.hsiBits & (int)F4SharedMem.Headers.HsiBits.Flying) == (int)F4SharedMem.Headers.HsiBits.Flying) 
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
                    float terrainHeight = _terrainBrowser.GetTerrainHeight(fromFalcon.x, fromFalcon.y);
                    float agl = -fromFalcon.z - terrainHeight;

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
                int newAlow = flightData.AutomaticLowAltitudeWarningInFeet;
                bool foundNewAlow = CheckDED_ALOW(fromFalcon, out newAlow);
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
                    bool commandBarsOn = ((float) (Math.Abs(Math.Round(fromFalcon.AdiIlsHorPos, 4))) != 0.1745f);
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

                        byte bmsNavMode = fromFalcon.navMode;
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

                DetermineIndicatedRateOfTurn(flightData);

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
                _terrainBrowser.CalculateLatLong(fromFalcon.x, fromFalcon.y, out latWholeDegrees, out latMinutes,
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
                Common.Util.DisposeObject(_terrainBrowser);
                _keyFile = null;
                _terrainBrowser = null;
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
            FlightData existingFlightData = Manager.FlightData;
            newServerFlightData.AltimeterMode = existingFlightData.AltimeterMode;
            newServerFlightData.BarometricPressureInDecimalInchesOfMercury =
                existingFlightData.BarometricPressureInDecimalInchesOfMercury;
            newServerFlightData.TransitionAltitudeInFeet = existingFlightData.TransitionAltitudeInFeet;
        }

        private static void ResetBaroPressureSettingToStandard(FlightData flightData)
        {
            flightData.BarometricPressureInDecimalInchesOfMercury = 29.92f;
        }

        private void EnsureTerrainBrowserLoaded()
        {
            if (_terrainBrowser == null)
            {
                _terrainBrowser = new TerrainBrowser(true);
                _terrainBrowser.LoadCurrentTheaterTerrainDatabase();
                _terrainBrowser.LoadFarTilesAsync();
            }
        }

        private static bool CheckDED_ALOW(F4SharedMem.FlightData fromFalcon, out int newAlow)
        {
            string alowString = fromFalcon.DEDLines[1];
            string alowInverseString = fromFalcon.Invert[1];
            bool anyCharsHighlighted = false;
            if (alowString.Contains("CARA ALOW"))
            {
                string newAlowString = "";
                for (int i = 0; i < alowString.Length; i++)
                {
                    char someChar = alowString[i];
                    char inverseChar = alowInverseString[i];
                    int tryParse;
                    if (Int32.TryParse(new String(someChar, 1), out tryParse))
                    {
                        if (inverseChar != ' ')
                        {
                            anyCharsHighlighted = true;
                            break;
                        }
                        newAlowString += someChar;
                    }
                }
                if (anyCharsHighlighted)
                {
                    newAlow = -1;
                    return false;
                }
                bool success = Int32.TryParse(newAlowString, out newAlow);
                if (!success)
                {
                    newAlow = -1;
                }
                return success;
            }
            newAlow = -1;
            return false;
        }

        /// <summary>
        /// Determines the indicated rate of turn 
        /// </summary>
        /// <param name="flightData"></param>
        private void DetermineIndicatedRateOfTurn(FlightData flightData)
        {
            //capture the current time
            DateTime curTime = DateTime.Now;

            //determine how many seconds it's been since our last "current heading" datum snapshot?
            var dT = (float) ((curTime.Subtract(_lastHeadingSample.Timestamp)).TotalMilliseconds);

            //determine the change in heading since our last snapshot
            float currentHeading = flightData.MagneticHeadingInDecimalDegrees;
            float headingDelta = Common.Math.Util.AngleDelta(_lastHeadingSample.Value, currentHeading);

            //now calculate the instantaneous rate of turn
            float currentInstantaneousRateOfTurn = (headingDelta/dT)*1000;
            if (Math.Abs(currentInstantaneousRateOfTurn) > 30 || float.IsInfinity(currentInstantaneousRateOfTurn) ||
                float.IsNaN(currentInstantaneousRateOfTurn)) currentInstantaneousRateOfTurn = 0; //noise
            if (Math.Abs(currentInstantaneousRateOfTurn) >
                (Pfd.MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 0.5f))
            {
                currentInstantaneousRateOfTurn = (Pfd.MAX_INDICATED_RATE_OF_TURN_DECIMAL_DEGREES_PER_SECOND + 0.5f)*
                                                 Math.Sign(currentInstantaneousRateOfTurn);
            }

            var sample = new TimestampedFloatValue {Timestamp = curTime, Value = currentInstantaneousRateOfTurn};

            //cull historic rate-of-turn samples older than n seconds
            var replacementList = new List<TimestampedFloatValue>();
            for (int i = 0; i < _lastInstantaneousRatesOfTurn.Count; i++)
            {
                if (!(Math.Abs(curTime.Subtract(_lastInstantaneousRatesOfTurn[i].Timestamp).TotalMilliseconds) > 1000))
                {
                    replacementList.Add(_lastInstantaneousRatesOfTurn[i]);
                }
            }
            _lastInstantaneousRatesOfTurn = replacementList;

            _lastInstantaneousRatesOfTurn.Add(sample);

            var medianRateOfTurn = (float) Math.Round(MedianSampleValue(_lastInstantaneousRatesOfTurn), 1);
            const float minIncrement = 0.1f;
            while (medianRateOfTurn < flightData.RateOfTurnInDecimalDegreesPerSecond - minIncrement)
            {
                flightData.RateOfTurnInDecimalDegreesPerSecond -= minIncrement;
            }
            while (medianRateOfTurn > flightData.RateOfTurnInDecimalDegreesPerSecond + minIncrement)
            {
                flightData.RateOfTurnInDecimalDegreesPerSecond += minIncrement;
            }

            if (Math.Round(medianRateOfTurn, 1) == 0)
            {
                flightData.RateOfTurnInDecimalDegreesPerSecond = 0;
            }
            else if (medianRateOfTurn == flightData.RateOfTurnInDecimalDegreesPerSecond - minIncrement)
            {
                flightData.RateOfTurnInDecimalDegreesPerSecond = medianRateOfTurn;
            }
            else if (medianRateOfTurn == flightData.RateOfTurnInDecimalDegreesPerSecond + minIncrement)
            {
                flightData.RateOfTurnInDecimalDegreesPerSecond = medianRateOfTurn;
            }

            _lastHeadingSample = new TimestampedFloatValue
                                     {
                                         Timestamp = curTime,
                                         Value = flightData.MagneticHeadingInDecimalDegrees
                                     };
        }

        #endregion

        #region Math utility functions

        private float AverageSampleValue(List<TimestampedFloatValue> values)
        {
            float sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i].Value;
            }

            float avg = values.Count > 0 ? sum/values.Count : 0;
            return avg;
        }

        private static float MedianSampleValue(List<TimestampedFloatValue> values)
        {
            if (values.Count == 0)
            {
                return 0;
            }

            var justTheValues = new float[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                justTheValues[i] = values[i].Value;
            }

            Array.Sort(justTheValues);

            int itemIndex = justTheValues.Length/2;

            if (justTheValues.Length%2 == 0)
            {
                // Even number of items.
                return (justTheValues[itemIndex] + justTheValues[itemIndex - 1])/2;
            }
            // Odd number of items.
            return justTheValues[itemIndex];
        }

        #endregion

        #region Network messaging event handlers

        public bool ProcessPendingMessageToServerFromClient(Message pendingMessage)
        {
            if (!Settings.Default.RunAsServer) return false;
            bool toReturn = false;
            if (pendingMessage != null)
            {
                string messageType = pendingMessage.MessageType;
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
            bool toReturn = false;
            if (pendingMessage != null)
            {
                string messageType = pendingMessage.MessageType;
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
            string functionName = button.FunctionName;
            if (!String.IsNullOrEmpty(functionName))
            {
                switch (functionName)
                {
                    case "CourseSelectIncrease":
                        {
                            FalconDataFormats? format = _curFalconDataFormat;
                            bool useIncrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsIncBy1");
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
                            FalconDataFormats? format = _curFalconDataFormat;
                            bool useDecrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsDecBy1");
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
                            FalconDataFormats? format = _curFalconDataFormat;
                            bool useIncrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgIncBy1");
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
                            FalconDataFormats? format = _curFalconDataFormat;
                            bool useDecrementByOne = false;
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                            {
                                KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgDecBy1");
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
            return F4Utils.Process.Util.FindKeyBinding(callback);
        }


        private void SendCallbackToFalcon(string callback)
        {
            if (!Settings.Default.RunAsClient)
            {
                DateTime startTime = DateTime.Now;
                _log.Debug("Sending callback:" + callback + " to Falcon.");
                SendCallbackToFalconLocal(callback);
                _log.Debug("Finished sending callback:" + callback + " to Falcon.");
                DateTime endTime = DateTime.Now;
                TimeSpan elapsed = endTime.Subtract(startTime);
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
            F4Utils.Process.Util.SendCallbackToFalcon(callback);
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
                _keyFile = F4Utils.Process.Util.GetCurrentKeyFile();
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

        #region Map Rendering Code

        public void RenderMap(Graphics g, Rectangle renderRectangle, float mapScale,
                              int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode)
        {
            //set up a background worker to do map rendering, if we haven't already set one up
            if (_mapRenderingBackgroundWorker == null)
            {
                _mapRenderingBackgroundWorker = new BackgroundWorker();
                if (_mapRenderingBackgroundWorkerDoWorkDelegate == null)
                {
                    _mapRenderingBackgroundWorkerDoWorkDelegate =
                        new DoWorkEventHandler(MapRenderingBackgroundWorkerDoWork);
                }
                _mapRenderingBackgroundWorker.DoWork += _mapRenderingBackgroundWorkerDoWorkDelegate;
                _mapRenderingBackgroundWorker.WorkerSupportsCancellation = true;
                _mapRenderingBackgroundWorker.WorkerReportsProgress = true;
                _mapRenderingBackgroundWorker.ProgressChanged += MapRenderingBackgroundWorkerProgressChanged;
            }
            if (_lastMapScale != mapScale)
            {
                if (_mapRenderingBackgroundWorker.IsBusy)
                {
                    _mapRenderingBackgroundWorker.CancelAsync();
                }
            }
            _lastMapScale = mapScale;

            /*
            if (_lastRangeRingDiameterNauticalMiles != rangeRingDiameterInNauticalMiles)
            {
                if (_mapRenderingBackgroundWorker.IsBusy)
                {
                    _mapRenderingBackgroundWorker.CancelAsync();
                }
            }*/
            _lastRangeRingDiameterNauticalMiles = rangeRingDiameterInNauticalMiles;

            //TODO: break on changes in rotation mode, centering mode, etc.

            //if the background worker is not busy, have it go render another map for us
            if (!_mapRenderingBackgroundWorker.IsBusy)
            {
                var args = new MapRenderAsyncArguments
                               {
                                   RenderRectangle = renderRectangle,
                                   MapScale = mapScale,
                                   RangeRingDiameterInNauticalMiles = rangeRingDiameterInNauticalMiles,
                                   RotationMode = rotationMode
                               };
                _mapRenderingBackgroundWorker.RunWorkerAsync(args);
            }
            lock (_mapImageLock)
            {
                if (_lastRenderedMapImage != null)
                {
                    //in the meantime go render the last drawn map
                    g.DrawImage(_lastRenderedMapImage, renderRectangle,
                                new Rectangle(new Point(0, 0), _lastRenderedMapImage.Size), GraphicsUnit.Pixel);
                }
                else
                {
                    Matrix gTransform = g.Transform;
                    g.ResetTransform();
                    string toDisplay = String.Format("LOADING: {0}%", _mapRenderProgress);
                    Brush greenBrush = Brushes.Green;
                    var path = new GraphicsPath();
                    var sf = new StringFormat(StringFormatFlags.NoWrap)
                                 {
                                     Alignment = StringAlignment.Center,
                                     LineAlignment = StringAlignment.Center
                                 };
                    var f = new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold);
                    SizeF textSize = g.MeasureString(toDisplay, f, 1, sf);
                    int leftX = (((renderRectangle.Width - ((int) textSize.Width))/2));
                    int topY = (((renderRectangle.Height - ((int) textSize.Height))/2));
                    var target = new Rectangle(leftX, topY, (int) textSize.Width, (int) textSize.Height);
                    path.AddString(toDisplay, f.FontFamily, (int) f.Style, f.Size, target.Location, sf);
                    g.FillPath(greenBrush, path);
                    g.Transform = gTransform;
                }
            }
        }

        private void MapRenderingBackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _mapRenderProgress = e.ProgressPercentage;
        }

        private void MapRenderingBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var args = (MapRenderAsyncArguments) e.Argument;
            var renderSurface = new Bitmap(args.RenderRectangle.Width, args.RenderRectangle.Height,
                                           PixelFormat.Format16bppRgb565);
            bool success;
            using (Graphics g = Graphics.FromImage(renderSurface))
            {
                success = RenderMapAsync(g, args.RenderRectangle, args.MapScale, args.RangeRingDiameterInNauticalMiles,
                                         args.RotationMode, e);
            }
            if (success)
            {
                lock (_mapImageLock)
                {
                    Common.Util.DisposeObject(_lastRenderedMapImage);
                    _lastRenderedMapImage = renderSurface;
                }
            }
            else
            {
                lock (_mapImageLock)
                {
                    Common.Util.DisposeObject(_lastRenderedMapImage);
                    _lastRenderedMapImage = null;
                }
            }
        }

        public bool RenderMapAsync(Graphics g, Rectangle renderRectangle, float mapScale,
                                   int rangeRingDiameterInNauticalMiles, MapRotationMode rotationMode, DoWorkEventArgs e)
        {
            if (_terrainBrowser == null || _terrainBrowser.DetectCurrentTheaterName() == null) return false;
            _mapRenderingBackgroundWorker.ReportProgress(0);
            //define original screen size in pixels and inches
            var originalRenderSizeInPixels = new Size(Constants.I_NATIVE_RES_WIDTH, Constants.I_NATIVE_RES_HEIGHT);
            var originalRenderSizeInScreenInches = new SizeF(6.24f, 8.32f);

            var originalRenderDiameterPixels =
                (float)
                Math.Sqrt((originalRenderSizeInPixels.Width*originalRenderSizeInPixels.Width) +
                          (originalRenderSizeInPixels.Height*originalRenderSizeInPixels.Height));
            var originalRenderDiameterInScreenInches =
                (float)
                Math.Sqrt((originalRenderSizeInScreenInches.Width*originalRenderSizeInScreenInches.Width) +
                          (originalRenderSizeInScreenInches.Height*originalRenderSizeInScreenInches.Height));

            //calculate how much terrain distance to render at specified scale in order to fill the screen 
            float terrainWidthToRenderInNauticalMiles = (originalRenderDiameterInScreenInches/(1.0000000000f/mapScale))/
                                                        Constants.INCHES_PER_NAUTICAL_MILE;
            float terrainWidthToRenderInFeet = terrainWidthToRenderInNauticalMiles*Constants.FEET_PER_NM;

            float outerMapRingDiameterPixelsUnscaled = ((rangeRingDiameterInNauticalMiles)/
                                                        terrainWidthToRenderInNauticalMiles)*
                                                       originalRenderDiameterPixels;


            //calculate number of elevation posts involved in rendering that amount of terrain distance
            float feetBetweenL0ElevationPosts = _terrainBrowser.CurrentTheaterDotMapFileInfo.FeetBetweenL0Posts;
            var numL0ElevationPostsToRender =
                (int) (Math.Ceiling(terrainWidthToRenderInFeet/feetBetweenL0ElevationPosts));
            if (numL0ElevationPostsToRender < 1) return false;
            float posX = Manager.FlightData.MapCoordinateFeetEast;
            float posY = Manager.FlightData.MapCoordinateFeetNorth;

            //determine which Level of Detail to use for rendering the map
            //start with the lowest Level of Detail available (i.e. highest-resolution map) and work upward (i.e. toward lower-resolution maps covering greater areas)
            uint lod = 0;
            float feetBetweenElevationPosts = feetBetweenL0ElevationPosts;
            float numThisLodElevationPostsToRender = numL0ElevationPostsToRender;
            int thisLodDetailTextureWidthPixels = 64;

            const int numAdditionalElevationPostsToRender = 1;

            while ((numThisLodElevationPostsToRender*thisLodDetailTextureWidthPixels) > originalRenderDiameterPixels)
                //choose LoD that requires fewest unnecessary pixels to be rendered
            {
                if (lod + 1 > _terrainBrowser.CurrentTheaterDotMapFileInfo.LastFarTiledLOD)
                {
                    break;
                }
                lod++;
                feetBetweenElevationPosts *= 2.0f;
                numThisLodElevationPostsToRender /= 2.0f;
                if (lod > _terrainBrowser.CurrentTheaterDotMapFileInfo.LastNearTiledLOD)
                {
                    thisLodDetailTextureWidthPixels = 32;
                }
                else
                {
                    Bitmap sample = _terrainBrowser.GetDetailTextureForElevationPost(0, 0, lod);
                    thisLodDetailTextureWidthPixels = sample != null ? sample.Width : 1;
                }
            }

            //there's no such thing as a fractional elevation post, so round up to the next integral number of elevation posts
            numThisLodElevationPostsToRender = (float) Math.Ceiling(numThisLodElevationPostsToRender);
            numThisLodElevationPostsToRender += numAdditionalElevationPostsToRender;

            //determine which elevation post represents the map's center
            var centerXElevationPost = (int) Math.Floor(posX/feetBetweenElevationPosts);
            var centerYElevationPost = (int) Math.Floor(posY/feetBetweenElevationPosts);

            //now do the same thing but allow for the concept of fractional elevation posts
            float centerXElevationPostF = (posX/feetBetweenElevationPosts);
            float centerYElevationPostF = (posY/feetBetweenElevationPosts);

            //now calculate the difference between the fractional number and the integral number, to determine how far of an offset into a single elevation post's detail texture, we should take as
            //being the exact center of the map (we'll use these offsets to crop a sub-square of terrain out of a larger square of rendered terrain later in the process)
            var xOffset =
                (int) Math.Floor((((centerXElevationPostF - centerXElevationPost)*thisLodDetailTextureWidthPixels)));
            int yOffset =
                -(int) Math.Floor((((centerYElevationPostF - centerYElevationPost)*thisLodDetailTextureWidthPixels)));

            //determine the boundaries of the map section that we'll be rendering
            var leftXPost =
                (int)
                Math.Floor(
                    (decimal) (centerXElevationPost - (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var rightXPost =
                (int)
                Math.Floor(
                    (decimal) (centerXElevationPost + (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var topYPost =
                (int)
                Math.Floor(
                    (decimal) (centerYElevationPost + (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            var bottomYPost =
                (int)
                Math.Floor(
                    (decimal) (centerYElevationPost - (int) Math.Floor(((numThisLodElevationPostsToRender/2.0f)))));
            int clampLeftXPost = leftXPost;
            int clampRightXPost = rightXPost;
            int clampTopYPost = topYPost;
            int clampBottomYPost = bottomYPost;
            _terrainBrowser.ClampElevationPostCoordinates(ref clampLeftXPost, ref clampTopYPost, lod);
            _terrainBrowser.ClampElevationPostCoordinates(ref clampRightXPost, ref clampBottomYPost, lod);

            //now store those boundaries in a Size object for convenience
            var elevationPostsToRenderBoundsSize = new Size((Math.Abs(rightXPost - leftXPost)),
                                                            (Math.Abs(bottomYPost - topYPost)));
            var clampedElevationPostsToRenderBoundsSize = new Size((Math.Abs(clampRightXPost - clampLeftXPost)),
                                                                   (Math.Abs(clampBottomYPost - clampTopYPost)));
            //determine the bounds of the cropped section of the render target that we'll be using 
            int cropWidth = ((elevationPostsToRenderBoundsSize.Width + 1 - numAdditionalElevationPostsToRender)*
                             thisLodDetailTextureWidthPixels);
            float toScale = 1.0f;
            if (cropWidth > originalRenderDiameterPixels)
            {
                var newCropWidth = (int) Math.Floor(originalRenderDiameterPixels);
                toScale = newCropWidth/(float) cropWidth;
                cropWidth = newCropWidth;
            }
            float scaleFactor = cropWidth/originalRenderDiameterPixels;

            //create the render target and render the overall (larger) map section to the render target
            try
            {
                using (var renderTarget = new Bitmap(cropWidth, cropWidth, PixelFormat.Format16bppRgb565))
                {
                    using (Graphics h = Graphics.FromImage(renderTarget))
                    {
                        Matrix origHTransform = h.Transform;
                        //h.CompositingQuality = CompositingQuality.HighQuality;
                        //h.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        //h.SmoothingMode = SmoothingMode.HighQuality;
                        h.PixelOffsetMode = PixelOffsetMode.Half;
                        if (rotationMode == MapRotationMode.CurrentHeadingOnTop)
                        {
                            h.TranslateTransform(cropWidth/2.0f, cropWidth/2.0f);
                            h.RotateTransform(-(Manager.FlightData.MagneticHeadingInDecimalDegrees));
                            h.TranslateTransform(-cropWidth/2.0f, -cropWidth/2.0f);
                        }

                        h.ScaleTransform(toScale, toScale);
                        h.TranslateTransform(-xOffset, -yOffset);

                        Bitmap crapMap =
                            _terrainBrowser.GetTheaterMap(_terrainBrowser.CurrentTheaterDotMapFileInfo.NumLODs - 1);
                        if (crapMap != null)
                        {
                            h.Clear(crapMap.GetPixel(crapMap.Width - 1, crapMap.Height - 1));
                        }
                        int numPostsRendered = 0;
                        for (int thisElevationPostY = clampBottomYPost;
                             thisElevationPostY <= clampTopYPost;
                             thisElevationPostY++)
                        {
                            for (int thisElevationPostX = clampLeftXPost;
                                 thisElevationPostX <= clampRightXPost;
                                 thisElevationPostX++)
                            {
                                //retrieve the detail texture corresponding to the current elevation post offset 

                                if (_mapRenderingBackgroundWorker.CancellationPending)
                                {
                                    _mapRenderProgress = 0;
                                    e.Cancel = true;
                                    return false;
                                }
                                Bitmap thisElevationPostDetailTexture =
                                    _terrainBrowser.GetDetailTextureForElevationPost(thisElevationPostX,
                                                                                     thisElevationPostY, lod);
                                if (thisElevationPostDetailTexture == null) continue;
                                //now draw the detail texture onto the render target
                                var sourceRect = new Rectangle(0, 0, thisElevationPostDetailTexture.Width,
                                                               thisElevationPostDetailTexture.Height);
                                //determine the upper-left pixel at which to place this detail texture on the render target
                                var destPoint = new Point(
                                    (thisElevationPostX - leftXPost)*thisLodDetailTextureWidthPixels,
                                    (topYPost - thisElevationPostY - 1)*thisLodDetailTextureWidthPixels
                                    );
                                //calculate the destination rectangle (in pixels) on the render target, that we'll be placing the detail texture inside of
                                var destRect = new Rectangle(destPoint,
                                                             new Size(thisLodDetailTextureWidthPixels + 2,
                                                                      thisLodDetailTextureWidthPixels + 2));
                                if (_mapRenderingBackgroundWorker.CancellationPending)
                                {
                                    _mapRenderProgress = 0;
                                    e.Cancel = true;
                                    return false;
                                }

                                h.DrawImage(thisElevationPostDetailTexture, destRect, sourceRect, GraphicsUnit.Pixel);
                                //h.DrawImageUnscaled(thisElevationPostDetailTexture, destRect);
                                numPostsRendered++;
                            }
                            _mapRenderingBackgroundWorker.ReportProgress(
                                (int) Math.Floor((
                                                     numPostsRendered/
                                                     (float) (
                                                                 clampedElevationPostsToRenderBoundsSize.Width
                                                                 *
                                                                 clampedElevationPostsToRenderBoundsSize.Height
                                                             )
                                                 )*100.0f)
                                );
                            Application.DoEvents();
                        }
                    }
                    var clipRectangle =
                        new Rectangle((cropWidth/2) - (int) ((originalRenderSizeInPixels.Width*scaleFactor)/2),
                                      (cropWidth/2) - (int) ((originalRenderSizeInPixels.Height*scaleFactor)/2),
                                      (int) (originalRenderSizeInPixels.Width*scaleFactor),
                                      (int) (originalRenderSizeInPixels.Height*scaleFactor));
                    g.DrawImage(
                        renderTarget,
                        renderRectangle,
                        clipRectangle,
                        GraphicsUnit.Pixel
                        );
                    if (_mapAirplaneBitmap == null)
                    {
                        _mapAirplaneBitmap = (Bitmap) Resources.F16Symbol.Clone();
                        _mapAirplaneBitmap.MakeTransparent(Color.FromArgb(255, 0, 255));
                        _mapAirplaneBitmap =
                            (Bitmap)
                            Common.Imaging.Util.ResizeBitmap(_mapAirplaneBitmap,
                                                             new Size(
                                                                 (int) Math.Floor(((float) _mapAirplaneBitmap.Width)),
                                                                 (int) Math.Floor(((float) _mapAirplaneBitmap.Height))));
                    }
                    g.DrawImage(_mapAirplaneBitmap, (((renderRectangle.Width - _mapAirplaneBitmap.Width)/2)),
                                (((renderRectangle.Height - _mapAirplaneBitmap.Height)/2)));

                    float renderRectangleScaleFactor =
                        (float)
                        Math.Sqrt((renderRectangle.Width*renderRectangle.Width) +
                                  (renderRectangle.Height*renderRectangle.Height))/originalRenderDiameterPixels;


                    var mapRingPen = new Pen(Color.Magenta);
                    Brush mapRingBrush = new SolidBrush(Color.Magenta);
                    mapRingPen.Width = 1;
                    const int mapRingLineWidths = 25;

                    Matrix originalGTransform = g.Transform;

                    g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
                    g.RotateTransform(-Manager.FlightData.MagneticHeadingInDecimalDegrees);
                    g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

                    //rotate 45 degrees before drawing outer map range circle
                    Matrix preRotate = g.Transform;
                    //capture current rotation so we can set it back before drawing inner map range circle
                    g.TranslateTransform(renderRectangle.Width/2.0f, renderRectangle.Height/2.0f);
                    g.RotateTransform(-45);
                    g.TranslateTransform(-renderRectangle.Width/2.0f, -renderRectangle.Height/2.0f);

                    //now draw outer map range circle
                    var outerMapRingDiameterPixelsScaled =
                        (int) Math.Floor(outerMapRingDiameterPixelsUnscaled*renderRectangleScaleFactor);
                    var outerMapRingBoundingRect =
                        new Rectangle(((renderRectangle.Width - outerMapRingDiameterPixelsScaled)/2),
                                      ((renderRectangle.Height - outerMapRingDiameterPixelsScaled)/2),
                                      outerMapRingDiameterPixelsScaled, outerMapRingDiameterPixelsScaled);
                    g.DrawEllipse(mapRingPen, outerMapRingBoundingRect);
                    int outerMapRingBoundingRectMiddleX = outerMapRingBoundingRect.X +
                                                          (int) (Math.Floor(outerMapRingBoundingRect.Width/(float) 2));
                    int outerMapRingBoundingRectMiddleY = outerMapRingBoundingRect.Y +
                                                          (int) (Math.Floor(outerMapRingBoundingRect.Height/(float) 2));
                    g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Top),
                               new Point(outerMapRingBoundingRectMiddleX,
                                         outerMapRingBoundingRect.Top + mapRingLineWidths));
                    g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRect.X, outerMapRingBoundingRectMiddleY),
                               new Point(outerMapRingBoundingRect.X + mapRingLineWidths, outerMapRingBoundingRectMiddleY));
                    g.DrawLine(mapRingPen,
                               new Point(outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width,
                                         outerMapRingBoundingRectMiddleY),
                               new Point(
                                   outerMapRingBoundingRect.X + outerMapRingBoundingRect.Width - mapRingLineWidths,
                                   outerMapRingBoundingRectMiddleY));
                    g.DrawLine(mapRingPen, new Point(outerMapRingBoundingRectMiddleX, outerMapRingBoundingRect.Bottom),
                               new Point(outerMapRingBoundingRectMiddleX,
                                         outerMapRingBoundingRect.Bottom - mapRingLineWidths));

                    //set rotation back before drawing inner map range circle
                    g.Transform = preRotate;

                    //draw inner map range circle
                    var innerMapRingDiameterPixelsScaled = (int) (Math.Floor(outerMapRingDiameterPixelsScaled/2.0f));
                    var innerMapRingBoundingRect =
                        new Rectangle(((renderRectangle.Width - innerMapRingDiameterPixelsScaled)/2),
                                      ((renderRectangle.Height - innerMapRingDiameterPixelsScaled)/2),
                                      innerMapRingDiameterPixelsScaled, innerMapRingDiameterPixelsScaled);
                    g.DrawEllipse(mapRingPen, innerMapRingBoundingRect);
                    int innerMapRingBoundingRectMiddleX = innerMapRingBoundingRect.X +
                                                          (int) (Math.Floor(innerMapRingBoundingRect.Width/(float) 2));
                    int innerMapRingBoundingRectMiddleY = innerMapRingBoundingRect.Y +
                                                          (int) (Math.Floor(innerMapRingBoundingRect.Height/(float) 2));
                    g.DrawLine(mapRingPen, new Point(innerMapRingBoundingRect.X, innerMapRingBoundingRectMiddleY),
                               new Point(innerMapRingBoundingRect.X + mapRingLineWidths, innerMapRingBoundingRectMiddleY));
                    g.DrawLine(mapRingPen,
                               new Point(innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width,
                                         innerMapRingBoundingRectMiddleY),
                               new Point(
                                   innerMapRingBoundingRect.X + innerMapRingBoundingRect.Width - mapRingLineWidths,
                                   innerMapRingBoundingRectMiddleY));
                    g.DrawLine(mapRingPen, new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Bottom),
                               new Point(innerMapRingBoundingRectMiddleX,
                                         innerMapRingBoundingRect.Bottom - mapRingLineWidths));

                    //draw north marker on inner map range circle
                    var northMarkerPoints = new Point[3];
                    northMarkerPoints[0] = new Point(innerMapRingBoundingRectMiddleX, innerMapRingBoundingRect.Top - 15);
                    northMarkerPoints[1] = new Point(innerMapRingBoundingRectMiddleX - 12,
                                                     innerMapRingBoundingRect.Top + 1);
                    northMarkerPoints[2] = new Point(innerMapRingBoundingRectMiddleX + 12,
                                                     innerMapRingBoundingRect.Top + 1);
                    g.FillPolygon(mapRingBrush, northMarkerPoints);

                    g.Transform = originalGTransform;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
            return true;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~Falcon4Support()
        {
            Dispose();
        }

        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
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
                    Common.Util.DisposeObject(_terrainBrowser);
                    _terrainBrowser = null;
                    Common.Util.DisposeObject(_mapRenderingBackgroundWorkerDoWorkDelegate);
                    _mapRenderingBackgroundWorkerDoWorkDelegate = null;
                    Common.Util.DisposeObject(_mapRenderingBackgroundWorker);
                    _mapRenderingBackgroundWorker = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion

        #region Nested type: MapRenderAsyncArguments

        private struct MapRenderAsyncArguments
        {
            public float MapScale;
            public int RangeRingDiameterInNauticalMiles;
            public Rectangle RenderRectangle;
            public MapRotationMode RotationMode;
        }

        #endregion

        #region Nested type: TimestampedFloatValue

        [Serializable]
        public struct TimestampedFloatValue
        {
            public DateTime Timestamp;
            public float Value;
        }

        #endregion
    }
}