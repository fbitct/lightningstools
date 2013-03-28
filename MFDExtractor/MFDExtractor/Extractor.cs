using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport;
using Common.InputSupport.DirectInput;
using Common.InputSupport.UI;
using Common.SimSupport;
using Common.UI;
using Common.Win32;
using F4KeyFile;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Process;
using F4Utils.SimSupport;
using F4Utils.Terrain;
using LightningGauges.Renderers;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using Microsoft.DirectX.DirectInput;
using log4net;
using Constants = Common.Math.Constants;
using Message = MFDExtractor.Message;
using Util = Common.Imaging.Util;
using Common.Networking;

namespace MFDExtractor
{
    public sealed class Extractor : IDisposable
    {
        #region Instance Variables

        private const int MIN_RENDERER_PASS_TIME_MILLSECONDS = 0;
        private const int MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER = 0;
        private static readonly ILog _log = LogManager.GetLogger(typeof (Extractor));
        private static Extractor _extractor;
        private bool _disposed;
        private long _renderCycleNum;

        private Screen _hudOutputScreen;
        private Screen _leftMfdOutputScreen;
        private Screen _mfd3OutputScreen;
        private Screen _mfd4OutputScreen;
        private Screen _rightMfdOutputScreen;


        private readonly Dictionary<IInstrumentRenderer, InstrumentForm> _outputForms =
            new Dictionary<IInstrumentRenderer, InstrumentForm>();
        private InstrumentForm _accelerometerForm;
        private InstrumentForm _adiForm;
        private InstrumentForm _altimeterForm;
        private InstrumentForm _aoaIndexerForm;
        private InstrumentForm _aoaIndicatorForm;
        private InstrumentForm _asiForm;
        private InstrumentForm _backupAdiForm;
        private InstrumentForm _cabinPressForm;
        private InstrumentForm _cautionPanelForm;
        private InstrumentForm _cmdsPanelForm;
        private InstrumentForm _compassForm;
        private InstrumentForm _dedForm;
        private InstrumentForm _ehsiForm;
        private InstrumentForm _epuFuelForm;
        private InstrumentForm _ftit1Form;
        private InstrumentForm _ftit2Form;
        private InstrumentForm _fuelFlowForm;
        private InstrumentForm _fuelQuantityForm;
        private InstrumentForm _hsiForm;
        private InstrumentForm _hudForm;
        private InstrumentForm _hydAForm;
        private InstrumentForm _hydBForm;
        private InstrumentForm _isisForm;
        private InstrumentForm _landingGearLightsForm;
        private InstrumentForm _leftMfdForm;
        private InstrumentForm _mfd3Form;
        private InstrumentForm _mfd4Form;
        private InstrumentForm _nozPos1Form;
        private InstrumentForm _nozPos2Form;
        private InstrumentForm _nwsIndexerForm;
        private InstrumentForm _oilGauge1Form;
        private InstrumentForm _oilGauge2Form;
        private InstrumentForm _pflForm;
        private InstrumentForm _pitchTrimForm;
        private InstrumentForm _rightMfdForm;
        private InstrumentForm _rollTrimForm;
        private InstrumentForm _rpm1Form;
        private InstrumentForm _rpm2Form;
        private InstrumentForm _rwrForm;
        private InstrumentForm _speedbrakeForm;
        private InstrumentForm _vviForm;

        private IInstrumentRenderer _accelerometerRenderer;
        private IInstrumentRenderer _adiRenderer;
        private IInstrumentRenderer _altimeterRenderer;
        private IInstrumentRenderer _aoaIndexerRenderer;
        private IInstrumentRenderer _aoaIndicatorRenderer;
        private IInstrumentRenderer _asiRenderer;
        private IInstrumentRenderer _backupAdiRenderer;
        private IInstrumentRenderer _cabinPressRenderer;
        private IInstrumentRenderer _cautionPanelRenderer;
        private IInstrumentRenderer _cmdsPanelRenderer;
        private IInstrumentRenderer _compassRenderer;
        private IInstrumentRenderer _dedRenderer;
        private IInstrumentRenderer _ehsiRenderer;
        private IInstrumentRenderer _epuFuelRenderer;
        private IInstrumentRenderer _ftit1Renderer;
        private IInstrumentRenderer _ftit2Renderer;
        private IInstrumentRenderer _fuelFlowRenderer;
        private IInstrumentRenderer _fuelQuantityRenderer;
        private IInstrumentRenderer _hsiRenderer;
        private IInstrumentRenderer _hydARenderer;
        private IInstrumentRenderer _hydBRenderer;
        private IInstrumentRenderer _isisRenderer;
        private IInstrumentRenderer _landingGearLightsRenderer;
        private IInstrumentRenderer _nozPos1Renderer;
        private IInstrumentRenderer _nozPos2Renderer;
        private IInstrumentRenderer _nwsIndexerRenderer;
        private IInstrumentRenderer _oilGauge1Renderer;
        private IInstrumentRenderer _oilGauge2Renderer;
        private IInstrumentRenderer _pflRenderer;
        private IInstrumentRenderer _pitchTrimRenderer;
        private IInstrumentRenderer _rollTrimRenderer;
        private IInstrumentRenderer _rpm1Renderer;
        private IInstrumentRenderer _rpm2Renderer;
        private IInstrumentRenderer _rwrRenderer;
        private IInstrumentRenderer _speedbrakeRenderer;
        private IInstrumentRenderer _vviRenderer;


        private GDIPlusOptions _gdiPlusOptions = new GDIPlusOptions();


        private Form _applicationForm;
        private volatile bool _keepRunning;
        private volatile bool _nightMode;
        private volatile bool _running;
        private volatile bool _testMode;
        private volatile bool _threeDeeMode;
        private volatile bool _twoDeePrimaryView = true;
        private volatile bool _windowSizingOrMoving;

        #region Capture Coordinates

        #region Primary 2D Mode Capture Coordinates

        private Rectangle _primaryHud2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _primaryLeftMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _primaryMfd3_2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _primaryMfd4_2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _primaryRightMfd2DInputRect = new Rectangle(0, 0, 0, 0);

        #endregion

        #region Secondary 2D Mode Capture Coordinates

        private Rectangle _secondaryHud2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _secondaryLeftMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _secondaryMfd3_2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _secondaryMfd4_2DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _secondaryRightMfd2DInputRect = new Rectangle(0, 0, 0, 0);

        #endregion

        #region 3D Mode Image Source Coordinates

        private Rectangle _hud3DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _leftMfd3DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _mfd3_3DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _mfd4_3DInputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _rightMfd3DInputRect = new Rectangle(0, 0, 0, 0);

        #endregion

        #endregion

        #region Output Window Coordinates

        public static bool _simRunning = false;
        private readonly object _texSmReaderLock = new object();
        private Reader _falconSmReader;
        private FlightData _flightData;
        private Rectangle _hudOutputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _leftMfdOutputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _mfd3_OutputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _mfd4_OutputRect = new Rectangle(0, 0, 0, 0);
        private Rectangle _rightMfdOutputRect = new Rectangle(0, 0, 0, 0);
        private bool _sim3DDataAvailable;

        #endregion

        #region Falcon 4 Sharedmem Readers & status flags
        private TerrainBrowser _terrainBrowser = new TerrainBrowser(false);

        private F4TexSharedMem.Reader _texSmReader = new F4TexSharedMem.Reader();
        private F4TexSharedMem.Reader _texSmStatusReader = new F4TexSharedMem.Reader();
        private bool _useBMSAdvancedSharedmemValues;
        #endregion

        #region Blank Images

        private readonly Image _hudBlankImage = Util.CloneBitmap(Resources.hudBlankImage);
        private readonly Image _leftMfdBlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage);
        private readonly Image _mfd3BlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage); //TODO: change to MFD3
        private readonly Image _mfd4BlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage); //TODO: change to MFD4
        private readonly Image _rightMfdBlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage);

        #endregion

        #region Test/Alignment Images

        private readonly Image _hudTestAlignmentImage = Util.CloneBitmap(Resources.hudTestAlignmentImage);
        private readonly Image _leftMfdTestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        private readonly Image _mfd3TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        private readonly Image _mfd4TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        private readonly Image _rightMfdTestAlignmentImage = Util.CloneBitmap(Resources.rightMFDTestAlignmentImage);

        #endregion

        #region Network Configuration

        private const string _serviceName = "MFDExtractorService";
        private ExtractorClient _client;
        private string _compressionType = "None";
        private string _imageFormat = "PNG";
        private NetworkMode _networkMode = NetworkMode.Standalone;
        private IPEndPoint _serverEndpoint;

        #endregion

        #region Public Events

        public event EventHandler DataChanged;
        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;
        public event EventHandler Starting;

        #endregion

        #region Thread Synchronization Signals

        private readonly AutoResetEvent _accelerometerRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _accelerometerRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _adiRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _adiRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _altimeterRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _altimeterRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _aoaIndexerRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _aoaIndexerRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _aoaIndicatorRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _aoaIndicatorRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _asiRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _asiRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _backupAdiRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _backupAdiRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _cabinPressRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _cabinPressRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _cautionPanelRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _cautionPanelRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _cmdsPanelRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _cmdsPanelRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _compassRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _compassRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _dedRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _dedRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _ehsiRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _ehsiRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _epuFuelRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _epuFuelRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _ftit1RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _ftit1RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _ftit2RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _ftit2RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _fuelFlowRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _fuelFlowRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _fuelQuantityRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _fuelQuantityRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _hsiRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _hsiRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _hudCaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _hydARenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _hydARenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _hydBRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _hydBRenderStart = new AutoResetEvent(false);

        private readonly Dictionary<IInstrumentRenderer, InstrumentStateSnapshot> _instrumentStates =
            new Dictionary<IInstrumentRenderer, Extractor.InstrumentStateSnapshot>();

        private readonly AutoResetEvent _isisRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _isisRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _landingGearLightsRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _landingGearLightsRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _leftMfdCaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _mfd3CaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _mfd4CaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _nozPos1RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _nozPos1RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _nozPos2RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _nozPos2RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _nwsIndexerRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _nwsIndexerRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _oilGauge1RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _oilGauge1RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _oilGauge2RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _oilGauge2RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _pflRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _pflRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _pitchTrimRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _pitchTrimRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rightMfdCaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rollTrimRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _rollTrimRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rpm1RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _rpm1RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rpm2RenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _rpm2RenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rwrRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _rwrRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _speedbrakeRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _speedbrakeRenderStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _vviRenderEnd = new AutoResetEvent(false);
        private readonly AutoResetEvent _vviRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _hudCaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _leftMfdCaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _mfd3CaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _mfd4CaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _rightMfdCaptureEnd = new AutoResetEvent(false);

        private struct InstrumentStateSnapshot
        {
            public DateTime DateTime;
            public int HashCode;
        }

        #endregion

        #region Threads

        private readonly Mediator.PhysicalControlStateChangedEventHandler _mediatorEventHandler;
        private readonly BackgroundWorker _settingsLoaderAsyncWorker = new BackgroundWorker();
        private readonly BackgroundWorker _settingsSaverAsyncWorker = new BackgroundWorker();
        private Thread _accelerometerRenderThread;
        private InputControlSelection _accelerometerResetKey;

        private Thread _adiRenderThread;
        private InputControlSelection _airspeedIndexDecreaseKey;
        private InputControlSelection _airspeedIndexIncreaseKey;
        private Thread _altimeterRenderThread;
        private Thread _aoaIndexerRenderThread;
        private Thread _aoaIndicatorRenderThread;
        private Thread _asiRenderThread;
        private InputControlSelection _azimuthIndicatorBrightnessDecreaseKey;
        private InputControlSelection _azimuthIndicatorBrightnessIncreaseKey;
        private Thread _backupAdiRenderThread;
        private Thread _cabinPressRenderThread;

        private Thread _captureOrchestrationThread;

        private Thread _cautionPanelRenderThread;
        private Thread _cmdsPanelRenderThread;
        private Thread _compassRenderThread;
        private Thread _dedRenderThread;
        private InputControlSelection _ehsiCourseDecreaseKey;
        private InputControlSelection _ehsiCourseDepressedKey;
        private InputControlSelection _ehsiCourseIncreaseKey;
        private InputControlSelection _ehsiHeadingDecreaseKey;
        private InputControlSelection _ehsiHeadingIncreaseKey;
        private InputControlSelection _ehsiMenuButtonDepressedKey;
        private Thread _ehsiRenderThread;
        private DateTime? _ehsiRightKnobDepressedTime;
        private DateTime? _ehsiRightKnobLastActivityTime;
        private DateTime? _ehsiRightKnobReleasedTime;
        private Thread _epuFuelRenderThread;
        private Thread _ftit1RenderThread;
        private Thread _ftit2RenderThread;
        private Thread _fuelFlowRenderThread;
        private Thread _fuelQuantityRenderThread;
        private Thread _hsiRenderThread;
        private Thread _hudCaptureThread;
        private Thread _hydARenderThread;
        private Thread _hydBRenderThread;
        private InputControlSelection _isisBrightButtonKey;
        private Thread _isisRenderThread;
        private InputControlSelection _isisStandardButtonKey;
        private bool _keySettingsLoaded;
        private Thread _keyboardWatcherThread;
        private Thread _landingGearLightsRenderThread;
        private Thread _leftMfdCaptureThread;
        private Thread _mfd3CaptureThread;
        private Thread _mfd4CaptureThread;
        private Thread _nozPos1RenderThread;
        private Thread _nozPos2RenderThread;
        private InputControlSelection _nvisKey;
        private Thread _nwsIndexerRenderThread;
        private Thread _oilGauge1RenderThread;
        private Thread _oilGauge2RenderThread;
        private Thread _pflRenderThread;
        private Thread _pitchTrimRenderThread;
        private Thread _rightMfdCaptureThread;
        private Thread _rollTrimRenderThread;
        private Thread _rpm1RenderThread;
        private Thread _rpm2RenderThread;
        private Thread _rwrRenderThread;
        private volatile bool _settingsLoadScheduled;
        private volatile bool _settingsSaveScheduled;

        private Thread _simStatusMonitorThread;
        private Thread _speedbrakeRenderThread;
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;

        private Thread _vviRenderThread;
        private readonly RenderThreadSetupHelper _renderThreadSetupHelper;
        private readonly ThreadAbortion _threadAbortion;
        private readonly BMSSupport _bmsSupport;
        private readonly IRenderThreadWorkHelper _adiRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _backupAdiRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _asiRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _altimeterRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _aoaIndexerRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _aoaIndicatorRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _cautionPanelRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _cmdsPanelRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _compassRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _dedRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _pflRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _epuFuelRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _accelerometerRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _ftit1RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _ftit2RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _fuelFlowRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _isisRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _fuelQuantityRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _hsiRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _ehsiRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _landingGearLightsRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _nwsIndexerRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _nozPos1RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _nozPos2RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _oilGauge1RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _oilGauge2RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _rwrRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _speedbrakeRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _rpm1RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _rpm2RenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _vviRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _hydARenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _hydBRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _cabinPressRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _rollTrimRenderThreadWorkHelper;
        private readonly IRenderThreadWorkHelper _pitchTrimRenderThreadWorkHelper;
        private readonly DIHotkeyDetection _diHotkeyDetection;

        #endregion

        #endregion


        private Extractor()
        {
            LoadSettings();
            _mediatorEventHandler = Mediator_PhysicalControlStateChanged;
            if (!Settings.Default.DisableDirectInputMediator)
            {
                Mediator = new Mediator(null);
            }
            _settingsSaverAsyncWorker.DoWork += _settingsSaverAsyncWorker_DoWork;
            _settingsLoaderAsyncWorker.DoWork += _settingsLoaderAsyncWorker_DoWork;
            _renderThreadSetupHelper = new RenderThreadSetupHelper();
            _threadAbortion = new ThreadAbortion();
            _bmsSupport = new BMSSupport();
            _adiRenderThreadWorkHelper = new RenderThreadWorkHelper(()=>_keepRunning, _adiRenderStart, _adiRenderEnd, _adiRenderer, _adiForm, ()=>Settings.Default.ADI_RotateFlipType,()=>Settings.Default.ADI_Monochrome, RenderInstrumentImage);
            _backupAdiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _backupAdiRenderStart, _backupAdiRenderEnd, _backupAdiRenderer, _backupAdiForm, () => Settings.Default.Backup_ADI_RotateFlipType, () => Settings.Default.Backup_ADI_Monochrome, RenderInstrumentImage);
            _asiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _asiRenderStart, _asiRenderEnd, _asiRenderer, _asiForm, () => Settings.Default.ASI_RotateFlipType, () => Settings.Default.ASI_Monochrome, RenderInstrumentImage);
            _altimeterRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _altimeterRenderStart, _altimeterRenderEnd, _altimeterRenderer, _altimeterForm, () => Settings.Default.Altimeter_RotateFlipType, () => Settings.Default.Altimeter_Monochrome, RenderInstrumentImage);
            _aoaIndexerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _aoaIndexerRenderStart, _aoaIndexerRenderEnd, _aoaIndexerRenderer, _aoaIndexerForm, () => Settings.Default.AOAIndexer_RotateFlipType, () => Settings.Default.AOAIndexer_Monochrome, RenderInstrumentImage);
            _aoaIndicatorRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _aoaIndicatorRenderStart, _aoaIndicatorRenderEnd, _aoaIndicatorRenderer, _aoaIndicatorForm, () => Settings.Default.AOAIndicator_RotateFlipType, () => Settings.Default.AOAIndicator_Monochrome, RenderInstrumentImage);
            _cautionPanelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cautionPanelRenderStart, _cautionPanelRenderEnd, _cautionPanelRenderer, _cautionPanelForm, () => Settings.Default.CautionPanel_RotateFlipType, () => Settings.Default.CautionPanel_Monochrome, RenderInstrumentImage);
            _cmdsPanelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cmdsPanelRenderStart, _cmdsPanelRenderEnd, _cmdsPanelRenderer, _cmdsPanelForm, () => Settings.Default.CMDS_RotateFlipType, () => Settings.Default.CMDS_Monochrome, RenderInstrumentImage);
            _compassRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _compassRenderStart, _compassRenderEnd, _compassRenderer, _compassForm, () => Settings.Default.Compass_RotateFlipType, () => Settings.Default.Compass_Monochrome, RenderInstrumentImage);
            _dedRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _dedRenderStart, _dedRenderEnd, _dedRenderer, _dedForm, () => Settings.Default.DED_RotateFlipType, () => Settings.Default.DED_Monochrome, RenderInstrumentImage);
            _pflRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _pflRenderStart, _pflRenderEnd, _pflRenderer, _pflForm, () => Settings.Default.PFL_RotateFlipType, () => Settings.Default.PFL_Monochrome, RenderInstrumentImage);
            _epuFuelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _epuFuelRenderStart, _epuFuelRenderEnd, _epuFuelRenderer, _epuFuelForm, () => Settings.Default.EPUFuel_RotateFlipType, () => Settings.Default.EPUFuel_Monochrome, RenderInstrumentImage);
            _accelerometerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _accelerometerRenderStart, _accelerometerRenderEnd, _accelerometerRenderer, _accelerometerForm, () => Settings.Default.Accelerometer_RotateFlipType, () => Settings.Default.Accelerometer_Monochrome, RenderInstrumentImage);
            _ftit1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ftit1RenderStart, _ftit1RenderEnd, _ftit1Renderer, _ftit1Form, () => Settings.Default.FTIT1_RotateFlipType, () => Settings.Default.FTIT1_Monochrome, RenderInstrumentImage);
            _ftit2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ftit2RenderStart, _ftit2RenderEnd, _ftit2Renderer, _ftit2Form, () => Settings.Default.FTIT2_RotateFlipType, () => Settings.Default.FTIT2_Monochrome, RenderInstrumentImage);
            _fuelFlowRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _fuelFlowRenderStart, _fuelFlowRenderEnd, _fuelFlowRenderer, _fuelFlowForm, () => Settings.Default.FuelFlow_RotateFlipType, () => Settings.Default.FuelFlow_Monochrome, RenderInstrumentImage);
            _isisRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _isisRenderStart, _isisRenderEnd, _isisRenderer, _isisForm, () => Settings.Default.ISIS_RotateFlipType, () => Settings.Default.ISIS_Monochrome, RenderInstrumentImage);
            _fuelQuantityRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _fuelQuantityRenderStart, _fuelQuantityRenderEnd, _fuelQuantityRenderer, _fuelQuantityForm, () => Settings.Default.FuelQuantity_RotateFlipType, () => Settings.Default.FuelQuantity_Monochrome, RenderInstrumentImage);
            _hsiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hsiRenderStart, _hsiRenderEnd, _hsiRenderer, _hsiForm, () => Settings.Default.HSI_RotateFlipType, () => Settings.Default.HSI_Monochrome, RenderInstrumentImage);
            _ehsiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ehsiRenderStart, _ehsiRenderEnd, _ehsiRenderer, _ehsiForm, () => Settings.Default.EHSI_RotateFlipType, () => Settings.Default.EHSI_Monochrome, RenderInstrumentImage);
            _landingGearLightsRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _landingGearLightsRenderStart, _landingGearLightsRenderEnd, _landingGearLightsRenderer, _landingGearLightsForm, () => Settings.Default.GearLights_RotateFlipType, () => Settings.Default.GearLights_Monochrome, RenderInstrumentImage);
            _nwsIndexerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nwsIndexerRenderStart, _nwsIndexerRenderEnd, _nwsIndexerRenderer, _nwsIndexerForm, () => Settings.Default.NWSIndexer_RotateFlipType, () => Settings.Default.NWSIndexer_Monochrome, RenderInstrumentImage);
            _nozPos1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nozPos1RenderStart, _nozPos1RenderEnd, _nozPos1Renderer, _nozPos1Form, () => Settings.Default.NOZ1_RotateFlipType, () => Settings.Default.NOZ1_Monochrome, RenderInstrumentImage);
            _nozPos2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nozPos2RenderStart, _nozPos2RenderEnd, _nozPos2Renderer, _nozPos2Form, () => Settings.Default.NOZ2_RotateFlipType, () => Settings.Default.NOZ2_Monochrome, RenderInstrumentImage);
            _oilGauge1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _oilGauge1RenderStart, _oilGauge1RenderEnd, _oilGauge1Renderer, _oilGauge1Form, () => Settings.Default.OIL1_RotateFlipType, () => Settings.Default.OIL1_Monochrome, RenderInstrumentImage);
            _oilGauge2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _oilGauge2RenderStart, _oilGauge2RenderEnd, _oilGauge2Renderer, _oilGauge2Form, () => Settings.Default.OIL2_RotateFlipType, () => Settings.Default.OIL2_Monochrome, RenderInstrumentImage);
            _rwrRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rwrRenderStart, _rwrRenderEnd, _rwrRenderer, _rwrForm, () => Settings.Default.RWR_RotateFlipType, () => Settings.Default.RWR_Monochrome, RenderInstrumentImage);
            _speedbrakeRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _speedbrakeRenderStart, _speedbrakeRenderEnd, _speedbrakeRenderer, _speedbrakeForm, () => Settings.Default.Speedbrake_RotateFlipType, () => Settings.Default.Speedbrake_Monochrome, RenderInstrumentImage);
            _rpm1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rpm1RenderStart, _rpm1RenderEnd, _rpm1Renderer, _rpm1Form, () => Settings.Default.RPM1_RotateFlipType, () => Settings.Default.RPM1_Monochrome, RenderInstrumentImage);
            _rpm2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rpm2RenderStart, _rpm2RenderEnd, _rpm2Renderer, _rpm2Form, () => Settings.Default.RPM2_RotateFlipType, () => Settings.Default.RPM2_Monochrome, RenderInstrumentImage);
            _vviRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _vviRenderStart, _vviRenderEnd, _vviRenderer, _vviForm, () => Settings.Default.VVI_RotateFlipType, () => Settings.Default.VVI_Monochrome, RenderInstrumentImage);
            _hydARenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hydARenderStart, _hydARenderEnd, _hydARenderer, _hydAForm, () => Settings.Default.HYDA_RotateFlipType, () => Settings.Default.HYDA_Monochrome, RenderInstrumentImage);
            _hydBRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hydBRenderStart, _hydBRenderEnd, _hydBRenderer, _hydBForm, () => Settings.Default.HYDB_RotateFlipType, () => Settings.Default.HYDB_Monochrome, RenderInstrumentImage);
            _cabinPressRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cabinPressRenderStart, _cabinPressRenderEnd, _cabinPressRenderer, _cabinPressForm, () => Settings.Default.CabinPress_RotateFlipType, () => Settings.Default.CabinPress_Monochrome, RenderInstrumentImage);
            _rollTrimRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rollTrimRenderStart, _rollTrimRenderEnd, _rollTrimRenderer, _rollTrimForm, () => Settings.Default.RollTrim_RotateFlipType, () => Settings.Default.RollTrim_Monochrome, RenderInstrumentImage);
            _pitchTrimRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _pitchTrimRenderStart, _pitchTrimRenderEnd, _pitchTrimRenderer, _pitchTrimForm, () => Settings.Default.PitchTrim_RotateFlipType, () => Settings.Default.PitchTrim_Monochrome, RenderInstrumentImage);
            _diHotkeyDetection = new DIHotkeyDetection(Mediator);
        }
        private void ProcessKeyUpEvent(KeyEventArgs e)
        {
            if (_keySettingsLoaded == false) LoadKeySettings();
            if (_ehsiCourseDepressedKey == null) return;
            Keys modifiersPressedRightNow = UpdateKeyEventArgsWithExtendedKeyInfo(Keys.None);
            Keys modifiersInHotkey = (_ehsiCourseDepressedKey.Keys & Keys.Modifiers);

            if (
                EHSIRightKnobIsCurrentlyDepressed()
                &&
                (_ehsiCourseDepressedKey.ControlType == ControlType.Key)
                &&
                (
                    (e.KeyData & Keys.KeyCode) == (_ehsiCourseDepressedKey.Keys & Keys.KeyCode)
                    ||
                    ((e.KeyData & Keys.KeyCode) & ~Keys.LControlKey & ~Keys.RControlKey & ~Keys.LShiftKey & ~Keys.LMenu &
                     ~Keys.RMenu) == Keys.None
                )
                &&
                (
                    (
                        modifiersInHotkey == Keys.None
                    )
                    ||
                    (
                        ((modifiersInHotkey & Keys.Alt) == Keys.Alt && (modifiersPressedRightNow & Keys.Alt) != Keys.Alt)
                        ||
                        ((modifiersInHotkey & Keys.Control) == Keys.Control &&
                         (modifiersPressedRightNow & Keys.Control) != Keys.Control)
                        ||
                        ((modifiersInHotkey & Keys.Shift) == Keys.Shift &&
                         (modifiersPressedRightNow & Keys.Shift) != Keys.Shift)
                    )
                )
                )
            {
                NotifyEHSIRightKnobReleased(true);
            }
        }
        private void ProcessKeyDownEvent(KeyEventArgs e)
        {
            if (_keySettingsLoaded == false) LoadKeySettings();
            Keys keys = UpdateKeyEventArgsWithExtendedKeyInfo(e.KeyData);

            if (KeyIsHotkey(_nvisKey, keys))
            {
                NotifyNightModeIsToggled(true);
            }
            else if (KeyIsHotkey(_airspeedIndexIncreaseKey, keys))
            {
                NotifyAirspeedIndexIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_airspeedIndexDecreaseKey, keys))
            {
                NotifyAirspeedIndexDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_ehsiHeadingDecreaseKey, keys))
            {
                NotifyEHSILeftKnobDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_ehsiHeadingIncreaseKey, keys))
            {
                NotifyEHSILeftKnobIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_ehsiCourseDecreaseKey, keys))
            {
                NotifyEHSIRightKnobDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_ehsiCourseIncreaseKey, keys))
            {
                NotifyEHSIRightKnobIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_ehsiCourseDepressedKey, keys) && !EHSIRightKnobIsCurrentlyDepressed())
            {
                NotifyEHSIRightKnobDepressed(true);
            }
            else if (KeyIsHotkey(_ehsiMenuButtonDepressedKey, keys))
            {
                NotifyEHSIMenuButtonDepressed(true);
            }
            else if (KeyIsHotkey(_isisBrightButtonKey, keys))
            {
                NotifyISISBrightButtonDepressed(true);
            }
            else if (KeyIsHotkey(_isisStandardButtonKey, keys))
            {
                NotifyISISStandardButtonDepressed(true);
            }
            else if (KeyIsHotkey(_azimuthIndicatorBrightnessIncreaseKey, keys))
            {
                NotifyAzimuthIndicatorBrightnessIncreased(true);
            }
            else if (KeyIsHotkey(_azimuthIndicatorBrightnessDecreaseKey, keys))
            {
                NotifyAzimuthIndicatorBrightnessDecreased(true);
            }
            else if (KeyIsHotkey(_accelerometerResetKey, keys))
            {
                NotifyAccelerometerIsReset(true);
            }
        }
        private Keys UpdateKeyEventArgsWithExtendedKeyInfo(Keys keys)
        {
            if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
            {
                keys |= Keys.Shift;
                //SHIFT is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
            {
                keys |= Keys.Control;
                //CONTROL is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
            {
                keys |= Keys.Alt;
                //ALT is pressed
            }
            return keys;
        }
        private void NotifyAzimuthIndicatorBrightnessIncreased(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Brightness +
                ((((F16AzimuthIndicator) _rwrRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
            ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Brightness = newBrightness;
            Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.AzimuthIndicatorBrightnessIncrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyAzimuthIndicatorBrightnessDecreased(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Brightness -
                ((((F16AzimuthIndicator) _rwrRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
            ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Brightness = newBrightness;
            Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.AzimuthIndicatorBrightnessDecrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyISISBrightButtonDepressed(bool relayToListeners)
        {
            int newBrightness = ((F16ISIS) _isisRenderer).InstrumentState.MaxBrightness;
            ((F16ISIS) _isisRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.ISISBrightButtonDepressed.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyISISStandardButtonDepressed(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                (((F16ISIS) _isisRenderer).InstrumentState.MaxBrightness)*0.5f
                                          );
            ((F16ISIS) _isisRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.ISISStandardButtonDepressed.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSILeftKnobIncreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
            bool useIncrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                KeyBinding incByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiHdgIncBy1");
                if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                {
                    useIncrementByOne = true;
                }
            }
            if (useIncrementByOne)
            {
                KeyFileUtils.SendCallbackToFalcon("SimHsiHdgIncBy1");
            }
            else
            {
                KeyFileUtils.SendCallbackToFalcon("SimHsiHeadingInc");
            }
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSILeftKnobIncrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSILeftKnobDecreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
            bool useDecrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                KeyBinding decByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiHdgDecBy1");
                if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                {
                    useDecrementByOne = true;
                }
            }
            if (useDecrementByOne)
            {
                KeyFileUtils.SendCallbackToFalcon("SimHsiHdgDecBy1");
            }
            else
            {
                KeyFileUtils.SendCallbackToFalcon("SimHsiHeadingDec");
            }
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSILeftKnobDecrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIRightKnobIncreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI) _ehsiRenderer).InstrumentState.ShowBrightnessLabel)
            {
                var newBrightness = (int) Math.Floor(
                    ((F16EHSI) _ehsiRenderer).InstrumentState.Brightness +
                    ((((F16EHSI) _ehsiRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
                ((F16EHSI) _ehsiRenderer).InstrumentState.Brightness = newBrightness;
                Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {
                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useIncrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    KeyBinding incByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiCrsIncBy1");
                    if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                    {
                        useIncrementByOne = true;
                    }
                }
                if (useIncrementByOne)
                {
                    KeyFileUtils.SendCallbackToFalcon("SimHsiCrsIncBy1");
                }
                else
                {
                    KeyFileUtils.SendCallbackToFalcon("SimHsiCourseInc");
                }
            }
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSIRightKnobIncrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIRightKnobDecreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI) _ehsiRenderer).InstrumentState.ShowBrightnessLabel)
            {
                var newBrightness = (int) Math.Floor(
                    ((F16EHSI) _ehsiRenderer).InstrumentState.Brightness -
                    ((((F16EHSI) _ehsiRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
                ((F16EHSI) _ehsiRenderer).InstrumentState.Brightness = newBrightness;
                Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {
                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useDecrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    KeyBinding decByOneCallback = KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
                    if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                    {
                        useDecrementByOne = true;
                    }
                }
                if (useDecrementByOne)
                {
                    KeyFileUtils.SendCallbackToFalcon("SimHsiCrsDecBy1");
                }
                else
                {
                    KeyFileUtils.SendCallbackToFalcon("SimHsiCourseDec");
                }
            }
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSIRightKnobDecrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIRightKnobDepressed(bool relayToListeners)
        {
            _ehsiRightKnobDepressedTime = DateTime.Now;
            _ehsiRightKnobReleasedTime = null;
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSIRightKnobDepressed.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIRightKnobReleased(bool relayToListeners)
        {
            _ehsiRightKnobDepressedTime = null;
            _ehsiRightKnobReleasedTime = DateTime.Now;
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSIRightKnobReleased.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIMenuButtonDepressed(bool relayToListeners)
        {
            F16EHSI.F16EHSIInstrumentState.InstrumentModes currentMode =
                ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode;
            F16EHSI.F16EHSIInstrumentState.InstrumentModes? newMode = null;
            switch (currentMode)
            {
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown:
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                    break;
                default:
                    break;
            }
            if (newMode.HasValue)
            {
                ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode = newMode.Value;
            }
            if (NetworkMode == NetworkMode.Standalone || NetworkMode == NetworkMode.Server)
            {
                KeyFileUtils.SendCallbackToFalcon("SimStepHSIMode");
            }


            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.EHSIMenuButtonDepressed.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyAirspeedIndexDecreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator) _asiRenderer).InstrumentState.AirspeedIndexKnots -= 2.5F;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.AirspeedIndexDecrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyAirspeedIndexIncreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator) _asiRenderer).InstrumentState.AirspeedIndexKnots += 2.5F;
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.AirspeedIndexIncrease.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }

        private void ProcessPendingMessagesToServerFromClient()
        {
            if (NetworkMode != NetworkMode.Server) return;
            Message pendingMessage = ExtractorServer.GetNextPendingMessageToServerFromClient();
            while (pendingMessage != null)
            {
                var messageType = (MessageTypes) Enum.Parse(typeof (MessageTypes), pendingMessage.MessageType);
                switch (messageType)
                {
                    case MessageTypes.ToggleNightMode:
                        NotifyNightModeIsToggled(false);
                        break;
                    case MessageTypes.AirspeedIndexIncrease:
                        NotifyAirspeedIndexIncreasedByOne(false);
                        break;
                    case MessageTypes.AirspeedIndexDecrease:
                        NotifyAirspeedIndexDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobIncrease:
                        NotifyEHSILeftKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobDecrease:
                        NotifyEHSILeftKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobIncrease:
                        NotifyEHSIRightKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDecrease:
                        NotifyEHSIRightKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDepressed:
                        NotifyEHSIRightKnobDepressed(false);
                        break;
                    case MessageTypes.EHSIRightKnobReleased:
                        NotifyEHSIRightKnobReleased(false);
                        break;
                    case MessageTypes.EHSIMenuButtonDepressed:
                        NotifyEHSIMenuButtonDepressed(false);
                        break;
                    case MessageTypes.AccelerometerIsReset:
                        NotifyAccelerometerIsReset(false);
                        break;
                    default:
                        break;
                }
                pendingMessage = ExtractorServer.GetNextPendingMessageToServerFromClient();
            }
        }
        private void ProcessPendingMessagesToClientFromServer()
        {
            if (NetworkMode != NetworkMode.Client || _client == null) return;
            Message pendingMessage = _client.GetNextMessageToClientFromServer();
            while (pendingMessage != null)
            {
                var messageType = (MessageTypes) Enum.Parse(typeof (MessageTypes), pendingMessage.MessageType);
                switch (messageType)
                {
                    case MessageTypes.ToggleNightMode:
                        NotifyNightModeIsToggled(false);
                        break;
                    case MessageTypes.AirspeedIndexIncrease:
                        NotifyAirspeedIndexIncreasedByOne(false);
                        break;
                    case MessageTypes.AirspeedIndexDecrease:
                        NotifyAirspeedIndexDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobIncrease:
                        NotifyEHSILeftKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobDecrease:
                        NotifyEHSILeftKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobIncrease:
                        NotifyEHSIRightKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDecrease:
                        NotifyEHSIRightKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDepressed:
                        NotifyEHSIRightKnobDepressed(false);
                        break;
                    case MessageTypes.EHSIRightKnobReleased:
                        NotifyEHSIRightKnobReleased(false);
                        break;
                    case MessageTypes.EHSIMenuButtonDepressed:
                        NotifyEHSIMenuButtonDepressed(false);
                        break;
                    case MessageTypes.AccelerometerIsReset:
                        NotifyAccelerometerIsReset(false);
                        break;
                    case MessageTypes.EnableBMSAdvancedSharedmemValues:
                        _useBMSAdvancedSharedmemValues = true;
                        break;
                    case MessageTypes.DisableBMSAdvancedSharedmemValues:
                        _useBMSAdvancedSharedmemValues = false;
                        break;
                    default:
                        break;
                }
                pendingMessage = _client.GetNextMessageToClientFromServer();
            }
        }
        private void NotifyAccelerometerIsReset(bool relayToListeners)
        {
            ((F16Accelerometer) _accelerometerRenderer).InstrumentState.ResetMinAndMaxGs();
            if (relayToListeners)
            {
                var msg = new Message(MessageTypes.AccelerometerIsReset.ToString(), null);
                if (NetworkMode == NetworkMode.Server)
                {
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyNightModeIsToggled(bool relayToListeners)
        {
            NightMode = !NightMode;
            if (relayToListeners)
            {
                if (NetworkMode == NetworkMode.Server)
                {
                    var msg = new Message(MessageTypes.ToggleNightMode.ToString(), null);
                    ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
            }
        }
        private void Mediator_PhysicalControlStateChanged(object sender, PhysicalControlStateChangedEventArgs e)
        {
            if (_keySettingsLoaded == false) LoadKeySettings();
            if (DirectInputEventIsHotkey(e, _nvisKey))
            {
                NotifyNightModeIsToggled(true);
            }
            else if (DirectInputEventIsHotkey(e, _airspeedIndexIncreaseKey))
            {
                NotifyAirspeedIndexIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _airspeedIndexDecreaseKey))
            {
                NotifyAirspeedIndexDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiHeadingDecreaseKey))
            {
                NotifyEHSILeftKnobDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiHeadingIncreaseKey))
            {
                NotifyEHSILeftKnobIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiCourseDecreaseKey))
            {
                NotifyEHSIRightKnobDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiCourseIncreaseKey))
            {
                NotifyEHSIRightKnobIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiCourseDepressedKey))
            {
                NotifyEHSIRightKnobDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _ehsiMenuButtonDepressedKey))
            {
                NotifyEHSIMenuButtonDepressed(true);
            }
            else if (
                !_diHotkeyDetection.DirectInputHotkeyIsTriggering(_ehsiCourseDepressedKey)
                &&
                EHSIRightKnobIsCurrentlyDepressed()
                )
            {
                NotifyEHSIRightKnobReleased(true);
            }
            else if (DirectInputEventIsHotkey(e, _isisBrightButtonKey))
            {
                NotifyISISBrightButtonDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _isisStandardButtonKey))
            {
                NotifyISISStandardButtonDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _azimuthIndicatorBrightnessIncreaseKey))
            {
                NotifyAzimuthIndicatorBrightnessIncreased(true);
            }
            else if (DirectInputEventIsHotkey(e, _azimuthIndicatorBrightnessDecreaseKey))
            {
                NotifyAzimuthIndicatorBrightnessDecreased(true);
            }
            else if (DirectInputEventIsHotkey(e, _accelerometerResetKey))
            {
                NotifyAccelerometerIsReset(true);
            }
        }
        private bool EHSIRightKnobIsCurrentlyDepressed()
        {
            return _ehsiRightKnobDepressedTime.HasValue;
        }

        private bool DirectInputEventIsHotkey(PhysicalControlStateChangedEventArgs diEvent, InputControlSelection hotkey)
        {
            if (diEvent == null) return false;
            if (diEvent.Control == null) return false;
            if (hotkey == null) return false;
            if (
                hotkey.ControlType != ControlType.Axis
                &&
                hotkey.ControlType != ControlType.Button
                &&
                hotkey.ControlType != ControlType.Pov
                )
            {
                return false;
            }
            if (hotkey.DirectInputControl == null) return false;
            if (hotkey.DirectInputDevice == null) return false;

            if (
                diEvent.Control.ControlType == hotkey.DirectInputControl.ControlType
                &&
                diEvent.Control.ControlNum == hotkey.DirectInputControl.ControlNum
                &&
                (
                    (diEvent.Control.ControlType == ControlType.Axis &&
                     diEvent.Control.AxisType == hotkey.DirectInputControl.AxisType)
                    ||
                    (diEvent.Control.ControlType != ControlType.Axis)
                )
                &&
                Equals(diEvent.Control.Parent.Key, hotkey.DirectInputDevice.Key)
                &&
                (
                    diEvent.Control.ControlType != ControlType.Pov
                    ||
                    (
                        hotkey.ControlType == ControlType.Pov
                        &&
                        hotkey.PovDirection == Common.InputSupport.Util.GetPovDirection(diEvent.CurrentState)
                    )
                )
                &&
                (
                    diEvent.Control.ControlType != ControlType.Button
                    ||
                    (
                        diEvent.Control.ControlType == ControlType.Button
                        &&
                        diEvent.CurrentState == 1
                    )
                )
                )
            {
                return true;
            }
            return false;
        }
        private bool KeyIsHotkey(InputControlSelection hotkey, Keys keyPressed)
        {
            if (hotkey == null) return false;
            if (hotkey.ControlType == ControlType.Key)
            {
                if (
                    (hotkey.Keys & Keys.KeyCode) == (keyPressed & Keys.KeyCode)
                    &&
                    (hotkey.Keys & Keys.Modifiers) == (keyPressed & Keys.Modifiers)
                    )
                {
                    return true;
                }
            }
            return false;
        }
        private void LoadKeySettings()
        {
            LoadNvisKeySetting();
            LoadAirspeedIndexIncreaseKeySetting();
            LoadAirspeedIndexDecreaseKeySetting();
            LoadEHSILeftKnobIncreaseKeySetting();
            LoadEHSILeftKnobDecreaseKeySetting();
            LoadEHSIRightKnobIncreaseKeySetting();
            LoadEHSIRightKnobDecreaseKeySetting();
            LoadEHSIRightKnobDepressedKeySetting();
            LoadEHSIMenuButtonDepressedKeySetting();
            LoadISISBrightButtonKeySetting();
            LoadISISStandardButtonKeySetting();
            LoadAzimuthIndicatorBrightnessIncreaseKeySetting();
            LoadAzimuthIndicatorBrightnessDecreaseKeySetting();
            LoadAccelerometerResetKeySetting();
            _keySettingsLoaded = true;
        }
        private void LoadEHSIMenuButtonDepressedKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSIMenuButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiMenuButtonDepressedKey = keyFromSettings;
                }
                else
                {
                    _ehsiMenuButtonDepressedKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadEHSIRightKnobDepressedKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSICourseKnobDepressedKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiCourseDepressedKey = keyFromSettings;
                }
                else
                {
                    _ehsiCourseDepressedKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadEHSIRightKnobIncreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSICourseIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiCourseIncreaseKey = keyFromSettings;
                }
                else
                {
                    _ehsiCourseIncreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadEHSIRightKnobDecreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSICourseDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiCourseDecreaseKey = keyFromSettings;
                }
                else
                {
                    _ehsiCourseDecreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadEHSILeftKnobIncreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSIHeadingIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiHeadingIncreaseKey = keyFromSettings;
                }
                else
                {
                    _ehsiHeadingIncreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadEHSILeftKnobDecreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.EHSIHeadingDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _ehsiHeadingDecreaseKey = keyFromSettings;
                }
                else
                {
                    _ehsiHeadingDecreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadAirspeedIndexIncreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.AirspeedIndexIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _airspeedIndexIncreaseKey = keyFromSettings;
                }
                else
                {
                    _airspeedIndexIncreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadAirspeedIndexDecreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.AirspeedIndexDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _airspeedIndexDecreaseKey = keyFromSettings;
                }
                else
                {
                    _airspeedIndexDecreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadAccelerometerResetKeySetting()
        {
            string keyFromSettingsString = Settings.Default.AccelerometerResetKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _accelerometerResetKey = keyFromSettings;
                }
                else
                {
                    _accelerometerResetKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadNvisKeySetting()
        {
            string keyFromSettingsString = Settings.Default.NVISKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _nvisKey = keyFromSettings;
                }
                else
                {
                    _nvisKey = new InputControlSelection {ControlType = ControlType.Unknown, Keys = Keys.None};
                }
            }
        }
        private void LoadISISBrightButtonKeySetting()
        {
            string keyFromSettingsString = Settings.Default.ISISBrightButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _isisBrightButtonKey = keyFromSettings;
                }
                else
                {
                    _isisBrightButtonKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadISISStandardButtonKeySetting()
        {
            string keyFromSettingsString = Settings.Default.ISISStandardButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _isisStandardButtonKey = keyFromSettings;
                }
                else
                {
                    _isisStandardButtonKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadAzimuthIndicatorBrightnessIncreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.AzimuthIndicatorBrightnessIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _azimuthIndicatorBrightnessIncreaseKey = keyFromSettings;
                }
                else
                {
                    _azimuthIndicatorBrightnessIncreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }
        private void LoadAzimuthIndicatorBrightnessDecreaseKeySetting()
        {
            string keyFromSettingsString = Settings.Default.AzimuthIndicatorBrightnessDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings =
                        (InputControlSelection)
                        Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString,
                                                                     typeof (InputControlSelection));
                }
                catch (Exception e)
                {
                }
                if (keyFromSettings != null)
                {
                    _azimuthIndicatorBrightnessDecreaseKey = keyFromSettings;
                }
                else
                {
                    _azimuthIndicatorBrightnessDecreaseKey = new InputControlSelection
                        {
                            ControlType = ControlType.Unknown,
                            Keys = Keys.None
                        };
                }
            }
        }

        public void Start()
        {
            //if we're already running, just ignore the Start command
            if (_running)
            {
                return;
            }
            KeyFileUtils.ResetCurrentKeyFile();
            DateTime startingTime = DateTime.Now;
            _log.DebugFormat("Starting the extractor at : {0}", startingTime.ToString());

            DateTime beginStartingEventTime = DateTime.Now;
            //Fire the Starting event to all listeners
            if (Starting != null)
            {
                Starting.Invoke(this, new EventArgs());
            }
            DateTime endStartingEventTime = DateTime.Now;
            TimeSpan startingEventTimeElapsed = endStartingEventTime.Subtract(beginStartingEventTime);
            _log.DebugFormat("Total time taken to invoke the Starting event on the extractor: {0}",
                             startingEventTimeElapsed.TotalMilliseconds);

            if (_keySettingsLoaded == false) LoadKeySettings();
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged += _mediatorEventHandler;
            }
            DateTime beginSendNullImagesTime = DateTime.Now;
            SendMfd4Image(null);
            SendMfd3Image(null);
            SendLeftMfdImage(null);
            SendRightMfdImage(null);
            SendHudImage(null);
            DateTime endSendNullImagesTime = DateTime.Now;
            TimeSpan sendNullImagesTimeElapsed = endSendNullImagesTime.Subtract(beginSendNullImagesTime);
            _log.DebugFormat("Total time taken to send null images on the extractor: {0}",
                             sendNullImagesTimeElapsed.TotalMilliseconds);

            DateTime beginRunThreadsTime = DateTime.Now;
            _log.DebugFormat("About to call RunThreads() at: {0}", beginRunThreadsTime.ToString());
            //launch worker thread(s) to do the extraction process
            RunThreads();
            DateTime endRunThreadsTime = DateTime.Now;
            _log.DebugFormat("Finished calling RunThreads() at: {0}", endRunThreadsTime.ToString());
            TimeSpan runThreadsTotalTime = endRunThreadsTime.Subtract(beginRunThreadsTime);
            _log.DebugFormat("Time taken calling RunThreads(): {0}", runThreadsTotalTime.TotalMilliseconds);


            DateTime finishedTime = DateTime.Now;
            _log.DebugFormat("Finished starting the extractor at : {0}", finishedTime.ToString());
            TimeSpan timeElapsed = finishedTime.Subtract(startingTime);
            _log.DebugFormat("Total time taken to start the extractor: {0}", timeElapsed.TotalMilliseconds);
        }
        public void Stop()
        {
            _log.DebugFormat("Stopping the extractor at : {0}", DateTime.Now.ToString());

            DateTime beginStoppingTime = DateTime.Now;

            DateTime beginStoppingEventInvokeTime = DateTime.Now;
            //Raise the Stopping event to signal listeners that the Extractor engine is stopping.
            if (Stopping != null) //verify that event listeners exist for the Stopping event
            {
                Stopping.Invoke(this, new EventArgs()); //fire the Stopping event to all listeners
            }
            DateTime endStoppingEventInvokeTime = DateTime.Now;
            TimeSpan stoppingEventTimeElapsed = endStoppingEventInvokeTime.Subtract(beginStoppingEventInvokeTime);
            _log.DebugFormat("Total time taken to invoke the Stopping event on the extractor: {0}",
                             stoppingEventTimeElapsed.TotalMilliseconds);


            //clear global flag that worker threads use to determine if their work loops should continue
            _keepRunning = false;
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged -= _mediatorEventHandler;
            }
            _keySettingsLoaded = false;

            DateTime beginEndingThreadsTime = DateTime.Now;

            var threadsToKill = new[]
                {
                    _captureOrchestrationThread,
                    _simStatusMonitorThread,
                    _keyboardWatcherThread,
                    _rwrRenderThread,
                    _mfd4CaptureThread,
                    _mfd3CaptureThread,
                    _leftMfdCaptureThread,
                    _rightMfdCaptureThread,
                    _hudCaptureThread,
                    _adiRenderThread,
                    _backupAdiRenderThread,
                    _asiRenderThread,
                    _altimeterRenderThread,
                    _aoaIndexerRenderThread,
                    _aoaIndicatorRenderThread,
                    _cautionPanelRenderThread,
                    _cmdsPanelRenderThread,
                    _compassRenderThread,
                    _dedRenderThread,
                    _pflRenderThread,
                    _epuFuelRenderThread,
                    _accelerometerRenderThread,
                    _ftit1RenderThread,
                    _ftit2RenderThread,
                    _fuelFlowRenderThread,
                    _isisRenderThread,
                    _fuelQuantityRenderThread,
                    _hsiRenderThread,
                    _ehsiRenderThread,
                    _landingGearLightsRenderThread,
                    _nwsIndexerRenderThread,
                    _nozPos1RenderThread,
                    _nozPos2RenderThread,
                    _oilGauge1RenderThread,
                    _oilGauge2RenderThread,
                    _speedbrakeRenderThread,
                    _rpm1RenderThread,
                    _rpm2RenderThread,
                    _vviRenderThread,
                    _hydARenderThread,
                    _hydBRenderThread,
                    _cabinPressRenderThread,
                    _rollTrimRenderThread,
                    _pitchTrimRenderThread
                };

            WaitForThreadEndThenAbort(ref threadsToKill, new TimeSpan(0, 0, 1));

            DateTime endEndingThreadsTime = DateTime.Now;
            TimeSpan endingThreadsTimeElapsed = endEndingThreadsTime.Subtract(beginEndingThreadsTime);
            _log.DebugFormat("Total time taken to end threads on the extractor: {0}",
                             endingThreadsTimeElapsed.TotalMilliseconds);


            DateTime beginClosingOutputWindowFormsTime = DateTime.Now;
            CloseOutputWindowForms();
            DateTime endClosingOutputWindowFormsTime = DateTime.Now;
            TimeSpan closingOutputWindowFormsTimeElapsed =
                endClosingOutputWindowFormsTime.Subtract(beginClosingOutputWindowFormsTime);
            _log.DebugFormat("Total time taken to close output windows on the extractor: {0}",
                             closingOutputWindowFormsTimeElapsed.TotalMilliseconds);


            DateTime beginTearDownImageServerTime = DateTime.Now;
            //if we're in Server mode, tear down the .NET Remoting channel
            if (_networkMode == NetworkMode.Server)
            {
                TearDownImageServer();
            }
            DateTime endTearDownImageServerTime = DateTime.Now;
            TimeSpan tearDownImageServerTimeElapsed = endTearDownImageServerTime.Subtract(beginTearDownImageServerTime);
            _log.DebugFormat("Total time taken to tear down the image server on the extractor: {0}",
                             tearDownImageServerTimeElapsed.TotalMilliseconds);

            CloseAndDisposeSharedmemReaders();

            //set the Running flag to false
            _running = false;

            DateTime beginStoppedEventTime = DateTime.Now;
            //fire the Stopped event to all listeners
            if (Stopped != null)
            {
                Stopped.Invoke(this, new EventArgs());
            }
            DateTime endStoppedEventTime = DateTime.Now;
            TimeSpan stoppedEventElapsedTime = endStoppedEventTime.Subtract(beginStoppedEventTime);
            _log.DebugFormat("Total time taken to invoke the Stopped event on the extractor: {0}",
                             stoppedEventElapsedTime.TotalMilliseconds);


            DateTime endStoppingTime = DateTime.Now;
            TimeSpan totalElapsed = endStoppingTime.Subtract(beginStoppingTime);
            _log.DebugFormat("Extractor engine stopped at : {0}", DateTime.Now.ToString());
            _log.DebugFormat("Total time taken to stop the extractor engine (in milliseconds): {0}",
                             totalElapsed.TotalMilliseconds);
        }
        public static Extractor GetInstance()
        {
            if (_extractor == null)
            {
                _extractor = new Extractor();
            }
            return _extractor;
        }
        public void LoadSettings()
        {
            Settings settings = Settings.Default;
            LoadGDIPlusSettings();
            _networkMode = (NetworkMode) settings.NetworkingMode;
            if (_networkMode == NetworkMode.Server)
            {
                _serverEndpoint = new IPEndPoint(IPAddress.Any, settings.ServerUsePortNumber);
            }
            else if (_networkMode == NetworkMode.Client)
            {
                _serverEndpoint = new IPEndPoint(IPAddress.Parse(settings.ClientUseServerIpAddress),
                                                 settings.ClientUseServerPortNum);
            }
            if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
            {
                _primaryMfd4_2DInputRect = Rectangle.FromLTRB(settings.Primary_MFD4_2D_ULX, settings.Primary_MFD4_2D_ULY,
                                                              settings.Primary_MFD4_2D_LRX, settings.Primary_MFD4_2D_LRY);
                _primaryMfd3_2DInputRect = Rectangle.FromLTRB(settings.Primary_MFD3_2D_ULX, settings.Primary_MFD3_2D_ULY,
                                                              settings.Primary_MFD3_2D_LRX, settings.Primary_MFD3_2D_LRY);
                _primaryLeftMfd2DInputRect = Rectangle.FromLTRB(settings.Primary_LMFD_2D_ULX,
                                                                settings.Primary_LMFD_2D_ULY,
                                                                settings.Primary_LMFD_2D_LRX,
                                                                settings.Primary_LMFD_2D_LRY);
                _primaryRightMfd2DInputRect = Rectangle.FromLTRB(settings.Primary_RMFD_2D_ULX,
                                                                 settings.Primary_RMFD_2D_ULY,
                                                                 settings.Primary_RMFD_2D_LRX,
                                                                 settings.Primary_RMFD_2D_LRY);
                _primaryHud2DInputRect = Rectangle.FromLTRB(settings.Primary_HUD_2D_ULX, settings.Primary_HUD_2D_ULY,
                                                            settings.Primary_HUD_2D_LRX, settings.Primary_HUD_2D_LRY);

                _secondaryMfd4_2DInputRect = Rectangle.FromLTRB(settings.Secondary_MFD4_2D_ULX,
                                                                settings.Secondary_MFD4_2D_ULY,
                                                                settings.Secondary_MFD4_2D_LRX,
                                                                settings.Secondary_MFD4_2D_LRY);
                _secondaryMfd3_2DInputRect = Rectangle.FromLTRB(settings.Secondary_MFD3_2D_ULX,
                                                                settings.Secondary_MFD3_2D_ULY,
                                                                settings.Secondary_MFD3_2D_LRX,
                                                                settings.Secondary_MFD3_2D_LRY);
                _secondaryLeftMfd2DInputRect = Rectangle.FromLTRB(settings.Secondary_LMFD_2D_ULX,
                                                                  settings.Secondary_LMFD_2D_ULY,
                                                                  settings.Secondary_LMFD_2D_LRX,
                                                                  settings.Secondary_LMFD_2D_LRY);
                _secondaryRightMfd2DInputRect = Rectangle.FromLTRB(settings.Secondary_RMFD_2D_ULX,
                                                                   settings.Secondary_RMFD_2D_ULY,
                                                                   settings.Secondary_RMFD_2D_LRX,
                                                                   settings.Secondary_RMFD_2D_LRY);
                _secondaryHud2DInputRect = Rectangle.FromLTRB(settings.Secondary_HUD_2D_ULX,
                                                              settings.Secondary_HUD_2D_ULY,
                                                              settings.Secondary_HUD_2D_LRX,
                                                              settings.Secondary_HUD_2D_LRY);
            }
            _mfd4_OutputRect = Rectangle.FromLTRB(settings.MFD4_OutULX, settings.MFD4_OutULY, settings.MFD4_OutLRX,
                                                  settings.MFD4_OutLRY);
            _mfd3_OutputRect = Rectangle.FromLTRB(settings.MFD3_OutULX, settings.MFD3_OutULY, settings.MFD3_OutLRX,
                                                  settings.MFD3_OutLRY);
            _leftMfdOutputRect = Rectangle.FromLTRB(settings.LMFD_OutULX, settings.LMFD_OutULY, settings.LMFD_OutLRX,
                                                    settings.LMFD_OutLRY);
            _rightMfdOutputRect = Rectangle.FromLTRB(settings.RMFD_OutULX, settings.RMFD_OutULY, settings.RMFD_OutLRX,
                                                     settings.RMFD_OutLRY);
            _hudOutputRect = Rectangle.FromLTRB(settings.HUD_OutULX, settings.HUD_OutULY, settings.HUD_OutLRX,
                                                settings.HUD_OutLRY);

            _mfd4OutputScreen = Common.Screen.Util.FindScreen(settings.MFD4_OutputDisplay);
            _mfd3OutputScreen = Common.Screen.Util.FindScreen(settings.MFD3_OutputDisplay);
            _leftMfdOutputScreen = Common.Screen.Util.FindScreen(settings.LMFD_OutputDisplay);
            _rightMfdOutputScreen = Common.Screen.Util.FindScreen(settings.RMFD_OutputDisplay);
            _hudOutputScreen = Common.Screen.Util.FindScreen(settings.HUD_OutputDisplay);
            _testMode = settings.TestMode;
            _threadPriority = settings.ThreadPriority;
            _compressionType = settings.CompressionType;
            _imageFormat = settings.NetworkImageFormat;
            if (DataChanged != null)
            {
                DataChanged.Invoke(null, new EventArgs());
            }
        }

        #region Public Properties

        public Form ApplicationForm
        {
            get { return _applicationForm; }
            set { _applicationForm = value; }
        }
        public IPEndPoint ServerEndpoint
        {
            get { return _serverEndpoint; }
            set { _serverEndpoint = value; }
        }
        public NetworkMode NetworkMode
        {
            get { return _networkMode; }
            set { _networkMode = value; }
        }
        public bool Running
        {
            get { return _running; }
        }
        public bool TestMode
        {
            get { return _testMode; }
            set
            {
                _testMode = value;
                Settings.Default.TestMode = _testMode;
            }
        }
        public bool TwoDeePrimaryView
        {
            get { return _twoDeePrimaryView; }
            set { _twoDeePrimaryView = value; }
        }
        public bool ThreeDeeMode
        {
            get { return _threeDeeMode; }
            set { _threeDeeMode = value; }
        }
        public bool NightMode
        {
            get { return _nightMode; }
            set { _nightMode = value; }
        }
        public Mediator Mediator { get; set; }

        #endregion

        #region Networking Support

        #region Basic Network Client/Server Setup Code

        private void SetupNetworking()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting setting up networking at: {0}", startTime.ToString());
            if (_networkMode == NetworkMode.Client)
            {
                SetupNetworkingClient();
            }
            if (_networkMode == NetworkMode.Server)
            {
                SetupNetworkingServer();
            }
            DateTime endTime = DateTime.Now;
            _log.DebugFormat("Finished setting up networking at: {0}", endTime.ToString());
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Time elapsed setting up networking: {0}", elapsed.TotalMilliseconds);
        }

        private void SetupNetworkingClient()
        {
            try
            {
                _client = new ExtractorClient(_serverEndpoint, _serviceName);
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
            }
        }

        private void SetupNetworkingServer()
        {
            ExtractorServer.CreateService(_serviceName, _serverEndpoint.Port, _compressionType, _imageFormat);
        }

        private void TearDownImageServer()
        {
            ExtractorServer.TearDownService(_serverEndpoint.Port);
        }

        #endregion

        #region MFD Network Image Transfer Code

        #region Outbound Transfer

        private void SendFlightData(FlightData flightData)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetFlightData(flightData);
            }
        }

        private void SendMfd4Image(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetMfd4Bitmap(image);
            }
        }

        private void SendMfd3Image(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetMfd3Bitmap(image);
            }
        }

        private void SendLeftMfdImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetLeftMfdBitmap(image);
            }
        }

        private void SendRightMfdImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetRightMfdBitmap(image);
            }
        }

        private void SendHudImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetHudBitmap(image);
            }
        }

        #endregion

        #region Inbound Transfer

        private FlightData ReadFlightDataFromNetwork()
        {
            FlightData retrieved = null;
            try
            {
                retrieved = _client.GetFlightData();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        private Image ReadMfd4FromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetMfd4Bitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        private Image ReadMfd3FromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetMfd3Bitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        private Image ReadLeftMfdFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetLeftMfdBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        private Image ReadRightMfdFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetRightMfdBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        private Image ReadHudFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetHudBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return retrieved;
        }

        #endregion

        #endregion

        #endregion

        #region Settings Loaders and Savers

        private void LoadGDIPlusSettings()
        {
            _gdiPlusOptions = new GDIPlusOptions();
            _gdiPlusOptions.CompositingQuality = Settings.Default.CompositingQuality;
            _gdiPlusOptions.InterpolationMode = Settings.Default.InterpolationMode;
            _gdiPlusOptions.PixelOffsetMode = Settings.Default.PixelOffsetMode;
            _gdiPlusOptions.SmoothingMode = Settings.Default.SmoothingMode;
            _gdiPlusOptions.TextRenderingHint = Settings.Default.TextRenderingHint;
        }

        private void LoadSettingsAsync()
        {
            if (_settingsLoaderAsyncWorker.IsBusy)
            {
                _settingsLoadScheduled = true;
            }
            else
            {
                _settingsLoaderAsyncWorker.RunWorkerAsync();
            }
        }

        private void SaveSettingsAsync()
        {
            if (_settingsSaverAsyncWorker.IsBusy)
            {
                _settingsSaveScheduled = true;
            }
            else
            {
                _settingsSaverAsyncWorker.RunWorkerAsync();
            }
        }

        private void _settingsLoaderAsyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadSettings();
            _settingsLoadScheduled = false;
        }

        private void _settingsSaverAsyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Settings.Default.Save();
            _settingsSaveScheduled = false;
        }

        #endregion

        #region Instrument Renderer Setup

        private void SetupInstrumentRenderers()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting setting up instrument renderers at: {0}", startTime.ToString());
            bool createAllRenderers = true;
            if (createAllRenderers || Settings.Default.EnableADIOutput)
            {
                SetupADIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableBackupADIOutput)
            {
                SetupBackupADIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableASIOutput)
            {
                SetupASIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableAltimeterOutput)
            {
                SetupAltimeterRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableAOAIndexerOutput)
            {
                SetupAOAIndexerRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableAOAIndicatorOutput)
            {
                SetupAOAIndicatorRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableCautionPanelOutput)
            {
                SetupCautionPanelRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableCMDSOutput)
            {
                SetupCMDSPanelRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableCompassOutput)
            {
                SetupCompassRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableDEDOutput)
            {
                SetupDEDRenderer();
            }
            if (createAllRenderers || Settings.Default.EnablePFLOutput)
            {
                SetupPFLRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableEPUFuelOutput)
            {
                SetupEPUFuelRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableAccelerometerOutput)
            {
                SetupAccelerometerRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableFTIT1Output)
            {
                SetupFTIT1Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableFTIT2Output)
            {
                SetupFTIT2Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableFuelFlowOutput)
            {
                SetupFuelFlowRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableISISOutput)
            {
                SetupISISRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableFuelQuantityOutput)
            {
                SetupFuelQuantityRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableHSIOutput)
            {
                SetupHSIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableEHSIOutput)
            {
                SetupEHSIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableGearLightsOutput)
            {
                SetupLandingGearLightsRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableNWSIndexerOutput)
            {
                SetupNWSIndexerRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableNOZ1Output)
            {
                SetupNOZ1Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableNOZ2Output)
            {
                SetupNOZ2Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableOIL1Output)
            {
                SetupOil1Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableOIL2Output)
            {
                SetupOil2Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableRWROutput)
            {
                SetupRWRRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableSpeedbrakeOutput)
            {
                SetupSpeedbrakeRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableRPM1Output)
            {
                SetupRPM1Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableRPM2Output)
            {
                SetupRPM2Renderer();
            }
            if (createAllRenderers || Settings.Default.EnableVVIOutput)
            {
                SetupVVIRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableHYDAOutput)
            {
                SetupHydARenderer();
            }
            if (createAllRenderers || Settings.Default.EnableHYDBOutput)
            {
                SetupHydBRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableCabinPressOutput)
            {
                SetupCabinPressRenderer();
            }
            if (createAllRenderers || Settings.Default.EnableRollTrimOutput)
            {
                SetupRollTrimRenderer();
            }
            if (createAllRenderers || Settings.Default.EnablePitchTrimOutput)
            {
                SetupPitchTrimRenderer();
            }
            DateTime endTime = DateTime.Now;
            _log.DebugFormat("Finished setting up instrument renderers at: {0}", endTime.ToString());
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Time taken setting up instrument renderers: {0}", elapsed.TotalMilliseconds);
        }

        private void SetupVVIRenderer()
        {
            string vviStyleString = Settings.Default.VVI_Style;
            var vviStyle = (VVIStyles) Enum.Parse(typeof (VVIStyles), vviStyleString);
            switch (vviStyle)
            {
                case VVIStyles.Tape:
                    _vviRenderer = new F16VerticalVelocityIndicatorUSA();
                    break;
                case VVIStyles.Needle:
                    _vviRenderer = new F16VerticalVelocityIndicatorEU();
                    break;
                default:
                    break;
            }
        }

        private void SetupRPM2Renderer()
        {
            _rpm2Renderer = new F16Tachometer();
            ((F16Tachometer) _rpm2Renderer).Options.IsSecondary = true;
        }

        private void SetupRPM1Renderer()
        {
            _rpm1Renderer = new F16Tachometer();
            ((F16Tachometer) _rpm1Renderer).Options.IsSecondary = false;
        }

        private void SetupSpeedbrakeRenderer()
        {
            _speedbrakeRenderer = new F16SpeedbrakeIndicator();
        }

        private void SetupRWRRenderer()
        {
            _rwrRenderer = new F16AzimuthIndicator();
            string styleString = Settings.Default.AzimuthIndicatorType;
            var style =
                (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)
                Enum.Parse(typeof (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), styleString);
            ((F16AzimuthIndicator) _rwrRenderer).Options.Style = style;
            ((F16AzimuthIndicator) _rwrRenderer).Options.HideBezel = !Settings.Default.AzimuthIndicator_ShowBezel;
            ((F16AzimuthIndicator) _rwrRenderer).Options.GDIPlusOptions = _gdiPlusOptions;
        }

        private void SetupOil2Renderer()
        {
            _oilGauge2Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge) _oilGauge2Renderer).Options.IsSecondary = true;
        }

        private void SetupOil1Renderer()
        {
            _oilGauge1Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge) _oilGauge1Renderer).Options.IsSecondary = false;
        }

        private void SetupNOZ2Renderer()
        {
            _nozPos2Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator) _nozPos2Renderer).Options.IsSecondary = true;
        }

        private void SetupNOZ1Renderer()
        {
            _nozPos1Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator) _nozPos1Renderer).Options.IsSecondary = false;
        }

        private void SetupNWSIndexerRenderer()
        {
            _nwsIndexerRenderer = new F16NosewheelSteeringIndexer();
        }

        private void SetupLandingGearLightsRenderer()
        {
            _landingGearLightsRenderer = new F16LandingGearWheelsLights();
        }

        private void SetupHSIRenderer()
        {
            _hsiRenderer = new F16HorizontalSituationIndicator();
        }

        private void SetupEHSIRenderer()
        {
            _ehsiRenderer = new F16EHSI();
            ((F16EHSI) _ehsiRenderer).Options.GDIPlusOptions = _gdiPlusOptions;
        }

        private void SetupFuelQuantityRenderer()
        {
            _fuelQuantityRenderer = new F16FuelQuantityIndicator();
            if (Settings.Default.FuelQuantityIndicator_NeedleCModel)
            {
                ((F16FuelQuantityIndicator) _fuelQuantityRenderer).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel;
            }
            else
            {
                ((F16FuelQuantityIndicator) _fuelQuantityRenderer).Options.NeedleType =
                    F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel;
            }
        }

        private void SetupFuelFlowRenderer()
        {
            _fuelFlowRenderer = new F16FuelFlow();
        }

        private void SetupISISRenderer()
        {
            _isisRenderer = new F16ISIS();
            string pressureUnitsString = Settings.Default.ISIS_PressureUnits;
            if (!string.IsNullOrEmpty(pressureUnitsString))
            {
                try
                {
                    ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                        (F16ISIS.F16ISISOptions.PressureUnits)
                        Enum.Parse(typeof (F16ISIS.F16ISISOptions.PressureUnits), pressureUnitsString);
                }
                catch (Exception e)
                {
                    ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                        F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                }
            }
            ((F16ISIS) _isisRenderer).Options.GDIPlusOptions = _gdiPlusOptions;
        }

        private void SetupAccelerometerRenderer()
        {
            _accelerometerRenderer = new F16Accelerometer();
        }

        private void SetupFTIT2Renderer()
        {
            _ftit2Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature) _ftit2Renderer).Options.IsSecondary = true;
        }

        private void SetupFTIT1Renderer()
        {
            _ftit1Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature) _ftit1Renderer).Options.IsSecondary = false;
        }

        private void SetupEPUFuelRenderer()
        {
            _epuFuelRenderer = new F16EPUFuelGauge();
        }

        private void SetupPFLRenderer()
        {
            _pflRenderer = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupDEDRenderer()
        {
            _dedRenderer = new F16DataEntryDisplayPilotFaultList();
        }

        private void SetupCompassRenderer()
        {
            _compassRenderer = new F16Compass();
        }

        private void SetupCMDSPanelRenderer()
        {
            _cmdsPanelRenderer = new F16CMDSPanel();
        }

        private void SetupCautionPanelRenderer()
        {
            _cautionPanelRenderer = new F16CautionPanel();
        }

        private void SetupAOAIndicatorRenderer()
        {
            _aoaIndicatorRenderer = new F16AngleOfAttackIndicator();
        }

        private void SetupAOAIndexerRenderer()
        {
            _aoaIndexerRenderer = new F16AngleOfAttackIndexer();
        }

        private void SetupAltimeterRenderer()
        {
            _altimeterRenderer = new F16Altimeter();

            string altimeterSyleString = Settings.Default.Altimeter_Style;
            var altimeterStyle =
                (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterSyleString);
            ((F16Altimeter) _altimeterRenderer).Options.Style = altimeterStyle;

            string pressureUnitsString = Settings.Default.Altimeter_PressureUnits;
            var pressureUnits =
                (F16Altimeter.F16AltimeterOptions.PressureUnits)
                Enum.Parse(typeof (F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            ((F16Altimeter) _altimeterRenderer).Options.PressureAltitudeUnits = pressureUnits;
        }

        private void SetupASIRenderer()
        {
            _asiRenderer = new F16AirspeedIndicator();
        }

        private void SetupADIRenderer()
        {
            _adiRenderer = new F16ADI();
        }

        private void SetupBackupADIRenderer()
        {
            _backupAdiRenderer = new F16StandbyADI();
        }

        private void SetupHydARenderer()
        {
            _hydARenderer = new F16HydraulicPressureGauge();
        }

        private void SetupHydBRenderer()
        {
            _hydBRenderer = new F16HydraulicPressureGauge();
        }

        private void SetupCabinPressRenderer()
        {
            _cabinPressRenderer = new F16CabinPressureAltitudeIndicator();
        }

        private void SetupRollTrimRenderer()
        {
            _rollTrimRenderer = new F16RollTrimIndicator();
        }

        private void SetupPitchTrimRenderer()
        {
            _pitchTrimRenderer = new F16PitchTrimIndicator();
        }

        #endregion

        #region MFD Capture Code

        #region Capture Strategy Orchestration Methods

        private FlightData GetFlightData()
        {
            FlightData toReturn = null;
            if (!_testMode)
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                    {
                        FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
#if (ALLIEDFORCE)
                        format = FalconDataFormats.AlliedForce;
#endif
                        //set automatic 3D mode for BMS
                        if (format.HasValue && format.Value == FalconDataFormats.BMS4) _threeDeeMode = true;

                        bool doMore = true;
                        bool newReader = false;
                        if (_falconSmReader == null)
                        {
                            if (format.HasValue)
                            {
                                _falconSmReader = new Reader(format.Value);
                                newReader = true;
                            }
                            else
                            {
                                _falconSmReader = new Reader();
                                newReader = true;
                            }
                        }
                        else
                        {
                            if (format.HasValue)
                            {
                                if (format.Value != _falconSmReader.DataFormat)
                                {
                                    _falconSmReader = new Reader(format.Value);
                                    newReader = true;
                                }
                            }
                            else
                            {
                                doMore = false;
                                Common.Util.DisposeObject(_falconSmReader);
                                _falconSmReader = null;
                                _useBMSAdvancedSharedmemValues = false;
                                newReader = false;
                            }
                        }
                        if (newReader)
                        {
                            string exePath = F4Utils.Process.Util.GetFalconExePath();
                            FileVersionInfo verInfo = null;
                            if (exePath != null) verInfo = FileVersionInfo.GetVersionInfo(exePath);
                            //12-08-12 Falcas change verInfo.ProductMinorPart >= 6826 to verInfo.ProductMinorPart >= 32
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4 && verInfo != null &&
                                ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >= 32) ||
                                 (verInfo.ProductMajorPart > 4)))
                            {
                                EnableBMSAdvancedSharedmemValues();
                            }
                            else
                            {
                                DisableBMSAdvancedSharedmemValues();
                            }
                        }
                        if (doMore)
                        {
                            toReturn = _falconSmReader.GetCurrentData();

                            bool computeRalt = false;
                            if (Settings.Default.EnableISISOutput || NetworkMode == NetworkMode.Server)
                            {
                                computeRalt = true;
                            }
                            if (computeRalt)
                            {
                                if (_terrainBrowser == null)
                                {
                                    _terrainBrowser = new TerrainBrowser(false);
                                    _terrainBrowser.LoadCurrentTheaterTerrainDatabase();
                                }
                                if (_terrainBrowser != null && toReturn != null)
                                {
                                    var extensionData = new FlightDataExtension();
                                    float terrainHeight = _terrainBrowser.GetTerrainHeight(toReturn.x, toReturn.y);
                                    float ralt = -toReturn.z - terrainHeight;

                                    //reset AGL altitude to zero if we're on the ground
                                    if (
                                        ((toReturn.lightBits & (int) LightBits.WOW) == (int) LightBits.WOW)
                                        ||
                                        (
                                            ((toReturn.lightBits3 & (int) Bms4LightBits3.OnGround) ==
                                             (int) Bms4LightBits3.OnGround)
                                            &&
                                            toReturn.DataFormat == FalconDataFormats.BMS4
                                        )
                                        )
                                    {
                                        ralt = 0;
                                    }

                                    if (ralt < 0)
                                    {
                                        ralt = 0;
                                    }
                                    extensionData.RadarAltitudeFeetAGL = ralt;
                                    toReturn.ExtensionData = extensionData;
                                }
                            }
                        }
                    }
                    else if (_networkMode == NetworkMode.Client)
                    {
                        toReturn = ReadFlightDataFromNetwork();
                    }
                }
            }
            if (toReturn == null)
            {
                toReturn = new FlightData();
                toReturn.hsiBits = Int32.MaxValue;
            }
            return toReturn;
        }

        private void DisableBMSAdvancedSharedmemValues()
        {
            _useBMSAdvancedSharedmemValues = false;
            if (NetworkMode == NetworkMode.Server)
            {
                var msg = new Message(MessageTypes.DisableBMSAdvancedSharedmemValues.ToString(), null);
                ExtractorServer.SubmitMessageToClientFromServer(msg);
            }
        }

        private void EnableBMSAdvancedSharedmemValues()
        {
            _useBMSAdvancedSharedmemValues = true;
            if (NetworkMode == NetworkMode.Server)
            {
                var msg = new Message(MessageTypes.EnableBMSAdvancedSharedmemValues.ToString(), null);
                ExtractorServer.SubmitMessageToClientFromServer(msg);
            }
        }

        private Image GetMfd4Bitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(_mfd4TestAlignmentImage);
            }
            else
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                    {
                        toReturn = Get3DMFD4();
                    }
                    else
                    {
                        if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                        {
                            if (_twoDeePrimaryView)
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_primaryMfd4_2DInputRect);
                            }
                            else
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_secondaryMfd4_2DInputRect);
                            }
                        }
                        else if (_networkMode == NetworkMode.Client)
                        {
                            toReturn = ReadMfd4FromNetwork();
                        }
                    }
                }
            }
            return toReturn;
        }

        private Image GetMfd3Bitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(_mfd3TestAlignmentImage);
            }
            else
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                    {
                        toReturn = Get3DMFD3();
                    }
                    else
                    {
                        if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                        {
                            if (_twoDeePrimaryView)
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_primaryMfd3_2DInputRect);
                            }
                            else
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_secondaryMfd3_2DInputRect);
                            }
                        }
                        else if (_networkMode == NetworkMode.Client)
                        {
                            toReturn = ReadMfd3FromNetwork();
                        }
                    }
                }
            }
            return toReturn;
        }

        private Image GetLeftMfdBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(_leftMfdTestAlignmentImage);
            }
            else
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                    {
                        toReturn = Get3DLeftMFD();
                    }
                    else
                    {
                        if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                        {
                            if (_twoDeePrimaryView)
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_primaryLeftMfd2DInputRect);
                            }
                            else
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_secondaryLeftMfd2DInputRect);
                            }
                        }
                        else if (_networkMode == NetworkMode.Client)
                        {
                            toReturn = ReadLeftMfdFromNetwork();
                        }
                    }
                }
            }
            return toReturn;
        }

        private Image GetRightMfdBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(_rightMfdTestAlignmentImage);
            }
            else
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                    {
                        toReturn = Get3DRightMFD();
                    }
                    else
                    {
                        if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                        {
                            if (_twoDeePrimaryView)
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_primaryRightMfd2DInputRect);
                            }
                            else
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_secondaryRightMfd2DInputRect);
                            }
                        }
                        else if (_networkMode == NetworkMode.Client)
                        {
                            toReturn = ReadRightMfdFromNetwork();
                        }
                    }
                }
            }
            return toReturn;
        }

        private Image GetHudBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(_hudTestAlignmentImage);
            }
            else
            {
                if (_simRunning || _networkMode == NetworkMode.Client)
                {
                    if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                    {
                        toReturn = Get3DHud();
                    }
                    else
                    {
                        if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                        {
                            if (_twoDeePrimaryView)
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_primaryHud2DInputRect);
                            }
                            else
                            {
                                toReturn = Common.Screen.Util.CaptureScreenRectangle(_secondaryHud2DInputRect);
                            }
                        }
                        else if (_networkMode == NetworkMode.Client)
                        {
                            toReturn = ReadHudFromNetwork();
                        }
                    }
                }
            }
            return toReturn;
        }

        private Image Get3DHud()
        {
            Image toReturn = null;
            if (_keepRunning && (_simRunning && _sim3DDataAvailable) && _hud3DInputRect != Rectangle.Empty)
            {
                try
                {
                    lock (_texSmReaderLock)
                    {
                        if (_texSmReader != null)
                        {
                            toReturn = Util.CloneBitmap(_texSmReader.GetImage(_hud3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        private Image Get3DMFD4()
        {
            Image toReturn = null;
            if (_keepRunning && (_simRunning && _sim3DDataAvailable) && _mfd4_3DInputRect != Rectangle.Empty)
            {
                try
                {
                    lock (_texSmReaderLock)
                    {
                        if (_texSmReader != null)
                        {
                            toReturn = Util.CloneBitmap(_texSmReader.GetImage(_mfd4_3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        private Image Get3DMFD3()
        {
            Image toReturn = null;
            if (_keepRunning && (_simRunning && _sim3DDataAvailable) && _mfd3_3DInputRect != Rectangle.Empty)
            {
                try
                {
                    lock (_texSmReaderLock)
                    {
                        if (_texSmReader != null)
                        {
                            toReturn = Util.CloneBitmap(_texSmReader.GetImage(_mfd3_3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        private Image Get3DLeftMFD()
        {
            Image toReturn = null;
            if (_keepRunning && (_simRunning && _sim3DDataAvailable) && _leftMfd3DInputRect != Rectangle.Empty)
            {
                try
                {
                    lock (_texSmReaderLock)
                    {
                        if (_texSmReader != null)
                        {
                            toReturn = Util.CloneBitmap(_texSmReader.GetImage(_leftMfd3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        private Image Get3DRightMFD()
        {
            Image toReturn = null;
            if (_keepRunning && (_simRunning && _sim3DDataAvailable) && _rightMfd3DInputRect != Rectangle.Empty)
            {
                try
                {
                    lock (_texSmReaderLock)
                    {
                        if (_texSmReader != null)
                        {
                            toReturn = Util.CloneBitmap(_texSmReader.GetImage(_rightMfd3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        #endregion

        #region MFD Capturing implementation methods

        private void CaptureMfd4()
        {
            if (Settings.Default.EnableMfd4Output || _networkMode == NetworkMode.Server)
            {
                Image mfd4Image = null;
                try
                {
                    mfd4Image = GetMfd4Bitmap();
                    if (mfd4Image == null)
                    {
                        mfd4Image = Util.CloneBitmap(_mfd4BlankImage);
                    }
                    SetMfd4Image(mfd4Image);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                finally
                {
                    Common.Util.DisposeObject(mfd4Image);
                }
            }
        }

        private void CaptureMfd3()
        {
            if (Settings.Default.EnableMfd3Output || _networkMode == NetworkMode.Server)
            {
                Image mfd3Image = null;
                try
                {
                    mfd3Image = GetMfd3Bitmap();
                    if (mfd3Image == null)
                    {
                        mfd3Image = Util.CloneBitmap(_mfd3BlankImage);
                    }
                    SetMfd3Image(mfd3Image);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                finally
                {
                    Common.Util.DisposeObject(mfd3Image);
                }
            }
        }

        private void CaptureLeftMfd()
        {
            if (Settings.Default.EnableLeftMFDOutput || _networkMode == NetworkMode.Server)
            {
                Image leftMfdImage = null;
                try
                {
                    leftMfdImage = GetLeftMfdBitmap();
                    if (leftMfdImage == null)
                    {
                        leftMfdImage = Util.CloneBitmap(_leftMfdBlankImage);
                    }
                    SetLeftMfdImage(leftMfdImage);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                finally
                {
                    Common.Util.DisposeObject(leftMfdImage);
                }
            }
        }

        private void CaptureRightMfd()
        {
            if (Settings.Default.EnableRightMFDOutput || _networkMode == NetworkMode.Server)
            {
                Image rightMfdImage = null;
                try
                {
                    rightMfdImage = GetRightMfdBitmap();
                    if (rightMfdImage == null)
                    {
                        rightMfdImage = Util.CloneBitmap(_rightMfdBlankImage);
                    }
                    SetRightMfdImage(rightMfdImage);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                finally
                {
                    Common.Util.DisposeObject(rightMfdImage);
                }
            }
        }

        private void CaptureHud()
        {
            if (Settings.Default.EnableHudOutput || _networkMode == NetworkMode.Server)
            {
                Image hudImage = null;
                try
                {
                    hudImage = GetHudBitmap();
                    if (hudImage == null)
                    {
                        hudImage = Util.CloneBitmap(_hudBlankImage);
                    }
                    SetHudImage(hudImage);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
                finally
                {
                    Common.Util.DisposeObject(hudImage);
                }
            }
        }

        #endregion

        #region MFD & HUD Image Swapping

        private void SetFlightData(FlightData flightData)
        {
            if (flightData == null) return;
            lock (flightData)
            {
                _flightData = flightData;
                if (_networkMode == NetworkMode.Server)
                {
                    SendFlightData(flightData);
                }
            }
        }

        private void SetHudImage(Image hudImage)
        {
            if (hudImage == null) return;
            lock (hudImage)
            {
                if (_networkMode == NetworkMode.Server)
                {
                    SendHudImage(hudImage);
                }
                if (_hudForm != null)
                {
                    if (Settings.Default.HUD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        hudImage.RotateFlip(Settings.Default.HUD_RotateFlipType);
                    }
                    using (Graphics graphics = _hudForm.CreateGraphics())
                    {
                        if (Settings.Default.HUD_Monochrome)
                        {
                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (Image compatible = Util.CopyBitmap(hudImage))
                            {
                                graphics.DrawImage(compatible, _hudForm.ClientRectangle, 0, 0, hudImage.Width,
                                                   hudImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(hudImage, _hudForm.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(hudImage);
            }
        }

        private ColorMatrix GetGreyscaleColorMatrix()
        {
            //ColorMatrix cm = new ColorMatrix(new float[][]{   new float[]{0.3f,0.3f,0.3f,0,0},
            //                      new float[]{0.59f,0.59f,0.59f,0,0},
            //                      new float[]{0.11f,0.11f,0.11f,0,0},
            //                      new float[]{0,0,0,1,0,0},
            //                      new float[]{0,0,0,0,1,0},
            //                      new float[]{0,0,0,0,0,1}});
            var cm = new ColorMatrix(new[]
                {
                    new[] {0.33f, 0.33f, 0.33f, 0, 0},
                    new[] {0.33f, 0.33f, 0.33f, 0, 0},
                    new[] {0.33f, 0.33f, 0.33f, 0, 0},
                    new float[] {0, 0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 0, 1}
                });
            return cm;
        }

        /// <summary>
        ///     Stores the current MFD #4 image and makes it available over the network to remote clients
        /// </summary>
        /// <param name="mfd4Image">the current MFD #4 image to store/send</param>
        private void SetMfd4Image(Image mfd4Image)
        {
            if (mfd4Image == null) return;
            lock (mfd4Image)
            {
                if (_networkMode == NetworkMode.Server)
                {
                    SendMfd4Image(mfd4Image);
                }
                if (_mfd4Form != null)
                {
                    if (Settings.Default.MFD4_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd4Image.RotateFlip(Settings.Default.MFD4_RotateFlipType);
                    }
                    using (Graphics graphics = _mfd4Form.CreateGraphics())
                    {
                        if (Settings.Default.MFD4_Monochrome)
                        {
                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (Image compatible = Util.CopyBitmap(mfd4Image))
                            {
                                graphics.DrawImage(compatible, _mfd4Form.ClientRectangle, 0, 0, mfd4Image.Width,
                                                   mfd4Image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(mfd4Image, _mfd4Form.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(mfd4Image);
            }
        }

        /// <summary>
        ///     Stores the current MFD #3 image and makes it available over the network to remote clients
        /// </summary>
        /// <param name="mfd3Image">the current MFD #3 image to store/send</param>
        private void SetMfd3Image(Image mfd3Image)
        {
            if (mfd3Image == null) return;
            lock (mfd3Image)
            {
                if (_networkMode == NetworkMode.Server)
                {
                    SendMfd3Image(mfd3Image);
                }
                if (_mfd3Form != null)
                {
                    if (Settings.Default.MFD3_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd3Image.RotateFlip(Settings.Default.MFD3_RotateFlipType);
                    }
                    using (Graphics graphics = _mfd3Form.CreateGraphics())
                    {
                        if (Settings.Default.MFD3_Monochrome)
                        {
                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (Image compatible = Util.CopyBitmap(mfd3Image))
                            {
                                graphics.DrawImage(compatible, _mfd3Form.ClientRectangle, 0, 0, mfd3Image.Width,
                                                   mfd3Image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(mfd3Image, _mfd3Form.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(mfd3Image);
            }
        }

        /// <summary>
        ///     Stores the current Left MFD image and makes it available over the network to remote clients
        /// </summary>
        /// <param name="leftMfdImage">the current Left MFD image to store/send</param>
        private void SetLeftMfdImage(Image leftMfdImage)
        {
            if (leftMfdImage == null) return;
            lock (leftMfdImage)
            {
                if (_networkMode == NetworkMode.Server)
                {
                    SendLeftMfdImage(leftMfdImage);
                }
                if (_leftMfdForm != null)
                {
                    if (Settings.Default.LMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        leftMfdImage.RotateFlip(Settings.Default.LMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _leftMfdForm.CreateGraphics())
                    {
                        if (Settings.Default.LMFD_Monochrome)
                        {
                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (Image compatible = Util.CopyBitmap(leftMfdImage))
                            {
                                graphics.DrawImage(compatible, _leftMfdForm.ClientRectangle, 0, 0, leftMfdImage.Width,
                                                   leftMfdImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(leftMfdImage, _leftMfdForm.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(leftMfdImage);
            }
        }

        /// <summary>
        ///     Stores the current Right MFD image and makes it available over the network to remote clients
        /// </summary>
        /// <param name="rightMfdImage">the current Right MFD image to store/send</param>
        private void SetRightMfdImage(Image rightMfdImage)
        {
            if (rightMfdImage == null) return;
            lock (rightMfdImage)
            {
                if (_networkMode == NetworkMode.Server)
                {
                    SendRightMfdImage(rightMfdImage);
                }
                if (_rightMfdForm != null)
                {
                    if (Settings.Default.RMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        rightMfdImage.RotateFlip(Settings.Default.RMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _rightMfdForm.CreateGraphics())
                    {
                        if (Settings.Default.RMFD_Monochrome)
                        {
                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (Image compatible = Util.CopyBitmap(rightMfdImage))
                            {
                                graphics.DrawImage(compatible, _rightMfdForm.ClientRectangle, 0, 0, rightMfdImage.Width,
                                                   rightMfdImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(rightMfdImage, _rightMfdForm.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(rightMfdImage);
            }
        }

        #endregion

        #endregion

        #region Forms Management

        #region Forms Setup

        private void SetupOutputForms()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Started setting up output forms on the extractor at: {0}", startTime.ToString());
            if (Settings.Default.EnableMfd4Output || NetworkMode == NetworkMode.Server)
            {
                SetupMfd4Form();
            }
            if (Settings.Default.EnableMfd3Output || NetworkMode == NetworkMode.Server)
            {
                SetupMfd3Form();
            }
            if (Settings.Default.EnableLeftMFDOutput || NetworkMode == NetworkMode.Server)
            {
                SetupLeftMfdForm();
            }

            if (Settings.Default.EnableRightMFDOutput || NetworkMode == NetworkMode.Server)
            {
                SetupRightMfdForm();
            }

            if (Settings.Default.EnableNWSIndexerOutput)
            {
                SetupNWSIndexerForm();
            }
            if (Settings.Default.EnableAOAIndexerOutput)
            {
                SetupAOAIndexerForm();
            }
            if (Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server)
            {
                SetupHudForm();
            }
            if (Settings.Default.EnableAOAIndicatorOutput)
            {
                SetupAOAIndicatorForm();
            }
            if (Settings.Default.EnableVVIOutput)
            {
                SetupVVIForm();
            }
            if (Settings.Default.EnableADIOutput)
            {
                SetupADIForm();
            }
            if (Settings.Default.EnableBackupADIOutput)
            {
                SetupBackupADIForm();
            }
            if (Settings.Default.EnableASIOutput)
            {
                SetupASIForm();
            }
            if (Settings.Default.EnableAltimeterOutput)
            {
                SetupAltimeterForm();
            }
            if (Settings.Default.EnableCautionPanelOutput)
            {
                SetupCautionPanelForm();
            }
            if (Settings.Default.EnableCMDSOutput)
            {
                SetupCMDSPanelForm();
            }
            if (Settings.Default.EnableCompassOutput)
            {
                SetupCompassForm();
            }
            if (Settings.Default.EnableDEDOutput)
            {
                SetupDEDForm();
            }
            if (Settings.Default.EnablePFLOutput)
            {
                SetupPFLForm();
            }
            if (Settings.Default.EnableEPUFuelOutput)
            {
                SetupEPUFuelForm();
            }
            if (Settings.Default.EnableAccelerometerOutput)
            {
                SetupAccelerometerForm();
            }
            if (Settings.Default.EnableFTIT1Output)
            {
                SetupFTIT1Form();
            }
            if (Settings.Default.EnableFTIT2Output)
            {
                SetupFTIT2Form();
            }
            if (Settings.Default.EnableFuelFlowOutput)
            {
                SetupFuelFlowForm();
            }
            if (Settings.Default.EnableISISOutput)
            {
                SetupISISForm();
            }
            if (Settings.Default.EnableFuelQuantityOutput)
            {
                SetupFuelQuantityForm();
            }
            if (Settings.Default.EnableHSIOutput)
            {
                SetupHSIForm();
            }
            if (Settings.Default.EnableEHSIOutput)
            {
                SetupEHSIForm();
            }
            if (Settings.Default.EnableGearLightsOutput)
            {
                SetupGearLightsForm();
            }
            if (Settings.Default.EnableNOZ1Output)
            {
                SetupNOZ1Form();
            }
            if (Settings.Default.EnableNOZ2Output)
            {
                SetupNOZ2Form();
            }
            if (Settings.Default.EnableOIL1Output)
            {
                SetupOIL1Form();
            }
            if (Settings.Default.EnableOIL2Output)
            {
                SetupOIL2Form();
            }
            if (Settings.Default.EnableRWROutput)
            {
                SetupRWRForm();
            }
            if (Settings.Default.EnableSpeedbrakeOutput)
            {
                SetupSpeedbrakeForm();
            }
            if (Settings.Default.EnableRPM1Output)
            {
                SetupRPM1Form();
            }
            if (Settings.Default.EnableRPM2Output)
            {
                SetupRPM2Form();
            }
            if (Settings.Default.EnableHYDAOutput)
            {
                SetupHydAForm();
            }
            if (Settings.Default.EnableHYDBOutput)
            {
                SetupHydBForm();
            }
            if (Settings.Default.EnableCabinPressOutput)
            {
                SetupCabinPressForm();
            }
            if (Settings.Default.EnableRollTrimOutput)
            {
                SetupRollTrimForm();
            }
            if (Settings.Default.EnablePitchTrimOutput)
            {
                SetupPitchTrimForm();
            }
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Finished setting up output forms on the extractor at: {0}", endTime.ToString());
            _log.DebugFormat("Time taken to set up output forms on the extractor: {0}", elapsed.TotalMilliseconds);
        }

        #region MFD Forms Setup

        private void SetupMfd4Form()
        {
            if (Settings.Default.EnableMfd4Output)
            {
                Point location;
                var size = new Size();
                _mfd4Form = new InstrumentForm();
                _mfd4Form.ShowInTaskbar = false;
                _mfd4Form.ShowIcon = false;
                _mfd4Form.Text = "MFD #4";
                if (Settings.Default.MFD4_StretchToFit)
                {
                    _mfd4Form.Size = _mfd4_OutputRect.Size;
                    _mfd4Form.Location = _mfd4_OutputRect.Location;
                    _mfd4Form.StretchToFill = true;
                    location = new Point(0, 0);
                    size = _mfd4OutputScreen.Bounds.Size;
                }
                else
                {
                    location = _mfd4_OutputRect.Location;
                    size = _mfd4_OutputRect.Size;
                }
                _mfd4Form.AlwaysOnTop = Settings.Default.MFD4_AlwaysOnTop;
                _mfd4Form.Monochrome = Settings.Default.MFD4_Monochrome;
                _mfd4Form.Rotation = Settings.Default.MFD4_RotateFlipType;
                _mfd4Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_mfd4Form, _applicationForm, _mfd4OutputScreen, location,
                                                             size, true, true);
                using (Graphics graphics = _mfd4Form.CreateGraphics())
                {
                    graphics.DrawImage(_mfd4BlankImage, _mfd4Form.ClientRectangle);
                }
                _mfd4Form.DataChanged += _mfd4Form_DataChanged;
                _mfd4Form.Disposed += _mfd4Form_Disposed;
            }
        }

        private void SetupMfd3Form()
        {
            if (Settings.Default.EnableMfd3Output)
            {
                Point location;
                var size = new Size();
                _mfd3Form = new InstrumentForm();
                _mfd3Form.Text = "MFD #3";
                _mfd3Form.ShowInTaskbar = false;
                _mfd3Form.ShowIcon = false;
                if (Settings.Default.MFD3_StretchToFit)
                {
                    _mfd3Form.Size = _mfd3_OutputRect.Size;
                    _mfd3Form.Location = _mfd3_OutputRect.Location;
                    _mfd3Form.StretchToFill = true;
                    location = new Point(0, 0);
                    size = _mfd3OutputScreen.Bounds.Size;
                }
                else
                {
                    location = _mfd3_OutputRect.Location;
                    size = _mfd3_OutputRect.Size;
                }
                _mfd3Form.AlwaysOnTop = Settings.Default.MFD3_AlwaysOnTop;
                _mfd3Form.Monochrome = Settings.Default.MFD3_Monochrome;
                _mfd3Form.Rotation = Settings.Default.MFD3_RotateFlipType;
                _mfd3Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_mfd3Form, _applicationForm, _mfd3OutputScreen, location,
                                                             size, true, true);
                using (Graphics graphics = _mfd3Form.CreateGraphics())
                {
                    graphics.DrawImage(_mfd3BlankImage, _mfd3Form.ClientRectangle);
                }
                _mfd3Form.DataChanged += _mfd3Form_DataChanged;
                _mfd3Form.Disposed += _mfd3Form_Disposed;
            }
        }

        private void SetupLeftMfdForm()
        {
            if (Settings.Default.EnableLeftMFDOutput)
            {
                Point location;
                var size = new Size();
                _leftMfdForm = new InstrumentForm();
                _leftMfdForm.Text = "Left MFD";
                _leftMfdForm.ShowInTaskbar = false;
                _leftMfdForm.ShowIcon = false;
                if (Settings.Default.LMFD_StretchToFit)
                {
                    _leftMfdForm.Size = _leftMfdOutputRect.Size;
                    _leftMfdForm.Location = _leftMfdOutputRect.Location;
                    _leftMfdForm.StretchToFill = true;
                    location = new Point(0, 0);
                    size = _leftMfdOutputScreen.Bounds.Size;
                }
                else
                {
                    location = _leftMfdOutputRect.Location;
                    size = _leftMfdOutputRect.Size;
                }
                _leftMfdForm.AlwaysOnTop = Settings.Default.LMFD_AlwaysOnTop;
                _leftMfdForm.Monochrome = Settings.Default.LMFD_Monochrome;
                _leftMfdForm.Rotation = Settings.Default.LMFD_RotateFlipType;
                _leftMfdForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_leftMfdForm, _applicationForm, _leftMfdOutputScreen,
                                                             location, size, true, true);
                using (Graphics graphics = _leftMfdForm.CreateGraphics())
                {
                    graphics.DrawImage(_leftMfdBlankImage, _leftMfdForm.ClientRectangle);
                }
                _leftMfdForm.DataChanged += _leftMfdForm_DataChanged;
                _leftMfdForm.Disposed += _leftMfdForm_Disposed;
            }
        }

        private void SetupRightMfdForm()
        {
            if (Settings.Default.EnableRightMFDOutput)
            {
                Point location;
                var size = new Size();
                _rightMfdForm = new InstrumentForm();
                _rightMfdForm.Text = "Right MFD";
                _rightMfdForm.ShowInTaskbar = false;
                _rightMfdForm.ShowIcon = false;
                if (Settings.Default.RMFD_StretchToFit)
                {
                    _rightMfdForm.Size = _rightMfdOutputRect.Size;
                    _rightMfdForm.Location = _rightMfdOutputRect.Location;
                    _rightMfdForm.StretchToFill = true;
                    location = new Point(0, 0);
                    size = _rightMfdOutputScreen.Bounds.Size;
                }
                else
                {
                    location = _rightMfdOutputRect.Location;
                    size = _rightMfdOutputRect.Size;
                }
                _rightMfdForm.AlwaysOnTop = Settings.Default.RMFD_AlwaysOnTop;
                _rightMfdForm.Monochrome = Settings.Default.RMFD_Monochrome;
                _rightMfdForm.Rotation = Settings.Default.RMFD_RotateFlipType;
                _rightMfdForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rightMfdForm, _applicationForm, _rightMfdOutputScreen,
                                                             location, size, true, true);
                using (Graphics graphics = _rightMfdForm.CreateGraphics())
                {
                    graphics.DrawImage(_rightMfdBlankImage, _rightMfdForm.ClientRectangle);
                }
                _rightMfdForm.DataChanged += _rightMfdForm_DataChanged;
                _rightMfdForm.Disposed += _rightMfdForm_Disposed;
            }
        }

        private void SetupHudForm()
        {
            if (Settings.Default.EnableHudOutput)
            {
                Point location;
                var size = new Size();
                _hudForm = new InstrumentForm();
                _hudForm.Text = "HUD";
                _hudForm.ShowInTaskbar = false;
                _hudForm.ShowIcon = false;
                if (Settings.Default.HUD_StretchToFit)
                {
                    _hudForm.Size = _hudOutputRect.Size;
                    _hudForm.Location = _hudOutputRect.Location;
                    _hudForm.StretchToFill = true;
                    location = new Point(0, 0);
                    size = _hudForm.Bounds.Size;
                }
                else
                {
                    location = _hudOutputRect.Location;
                    size = _hudOutputRect.Size;
                }
                _hudForm.AlwaysOnTop = Settings.Default.HudAlwaysOnTop;
                _hudForm.Monochrome = Settings.Default.HUD_Monochrome;
                _hudForm.Rotation = Settings.Default.HUD_RotateFlipType;
                _hudForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hudForm, _applicationForm, _hudOutputScreen, location,
                                                             size, true, true);
                using (Graphics graphics = _hudForm.CreateGraphics())
                {
                    graphics.DrawImage(_hudBlankImage, _hudForm.ClientRectangle);
                }
                _hudForm.DataChanged += _hudForm_DataChanged;
                _hudForm.Disposed += _hudForm_Disposed;
            }
        }

        #endregion

        #region Instruments Forms Setup

        private void SetupVVIForm()
        {
            if (Settings.Default.EnableVVIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.VVI_OutputDisplay);
                _vviForm = new InstrumentForm();
                _vviForm.Text = "Vertical Velocity Indicator (VVI)";
                _vviForm.ShowInTaskbar = false;
                _vviForm.ShowIcon = false;
                if (Settings.Default.VVI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _vviForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.VVI_OutULX, Settings.Default.VVI_OutULY);
                    size = new Size(Settings.Default.VVI_OutLRX - Settings.Default.VVI_OutULX,
                                    Settings.Default.VVI_OutLRY - Settings.Default.VVI_OutULY);
                    _vviForm.StretchToFill = false;
                }
                _vviForm.AlwaysOnTop = Settings.Default.VVI_AlwaysOnTop;
                _vviForm.Monochrome = Settings.Default.VVI_Monochrome;
                _vviForm.Rotation = Settings.Default.VVI_RotateFlipType;
                _vviForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_vviForm, _applicationForm, screen, location, size, true,
                                                             true);
                _vviForm.DataChanged += _vviForm_DataChanged;
                _vviForm.Disposed += _vviForm_Disposed;
                _outputForms.Add(_vviRenderer, _vviForm);
            }
        }

        private void SetupRPM1Form()
        {
            if (Settings.Default.EnableRPM1Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.RPM1_OutputDisplay);
                _rpm1Form = new InstrumentForm();
                _rpm1Form.Text = "Engine 1 - RPM";
                _rpm1Form.ShowInTaskbar = false;
                _rpm1Form.ShowIcon = false;
                if (Settings.Default.RPM1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rpm1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.RPM1_OutULX, Settings.Default.RPM1_OutULY);
                    size = new Size(Settings.Default.RPM1_OutLRX - Settings.Default.RPM1_OutULX,
                                    Settings.Default.RPM1_OutLRY - Settings.Default.RPM1_OutULY);
                    _rpm1Form.StretchToFill = false;
                }
                _rpm1Form.AlwaysOnTop = Settings.Default.RPM1_AlwaysOnTop;
                _rpm1Form.Monochrome = Settings.Default.RPM1_Monochrome;
                _rpm1Form.Rotation = Settings.Default.RPM1_RotateFlipType;
                _rpm1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rpm1Form, _applicationForm, screen, location, size, true,
                                                             true);
                _rpm1Form.DataChanged += _rpm1Form_DataChanged;
                _rpm1Form.Disposed += _rpm1Form_Disposed;
                _outputForms.Add(_rpm1Renderer, _rpm1Form);
            }
        }

        private void SetupRPM2Form()
        {
            if (Settings.Default.EnableRPM2Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.RPM2_OutputDisplay);
                _rpm2Form = new InstrumentForm();
                _rpm2Form.Text = "Engine 2 - RPM";
                _rpm2Form.ShowInTaskbar = false;
                _rpm2Form.ShowIcon = false;
                if (Settings.Default.RPM2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rpm2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.RPM2_OutULX, Settings.Default.RPM2_OutULY);
                    size = new Size(Settings.Default.RPM2_OutLRX - Settings.Default.RPM2_OutULX,
                                    Settings.Default.RPM2_OutLRY - Settings.Default.RPM2_OutULY);
                    _rpm2Form.StretchToFill = false;
                }
                _rpm2Form.AlwaysOnTop = Settings.Default.RPM2_AlwaysOnTop;
                _rpm2Form.Monochrome = Settings.Default.RPM2_Monochrome;
                _rpm2Form.Rotation = Settings.Default.RPM2_RotateFlipType;
                _rpm2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rpm2Form, _applicationForm, screen, location, size, true,
                                                             true);
                _rpm2Form.DataChanged += _rpm2Form_DataChanged;
                _rpm2Form.Disposed += _rpm2Form_Disposed;
                _outputForms.Add(_rpm2Renderer, _rpm2Form);
            }
        }

        private void SetupSpeedbrakeForm()
        {
            if (Settings.Default.EnableSpeedbrakeOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.Speedbrake_OutputDisplay);
                _speedbrakeForm = new InstrumentForm();
                _speedbrakeForm.Text = "Speedbrake";
                _speedbrakeForm.ShowInTaskbar = false;
                _speedbrakeForm.ShowIcon = false;
                if (Settings.Default.Speedbrake_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _speedbrakeForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.Speedbrake_OutULX, Settings.Default.Speedbrake_OutULY);
                    size = new Size(Settings.Default.Speedbrake_OutLRX - Settings.Default.Speedbrake_OutULX,
                                    Settings.Default.Speedbrake_OutLRY - Settings.Default.Speedbrake_OutULY);
                    _speedbrakeForm.StretchToFill = false;
                }
                _speedbrakeForm.AlwaysOnTop = Settings.Default.Speedbrake_AlwaysOnTop;
                _speedbrakeForm.Monochrome = Settings.Default.Speedbrake_Monochrome;
                _speedbrakeForm.Rotation = Settings.Default.Speedbrake_RotateFlipType;
                _speedbrakeForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_speedbrakeForm, _applicationForm, screen, location, size,
                                                             true, true);
                _speedbrakeForm.DataChanged += _speedbrakeForm_DataChanged;
                _speedbrakeForm.Disposed += _speedbrakeForm_Disposed;
                _outputForms.Add(_speedbrakeRenderer, _speedbrakeForm);
            }
        }

        private void SetupRWRForm()
        {
            if (Settings.Default.EnableRWROutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.RWR_OutputDisplay);
                _rwrForm = new InstrumentForm();
                _rwrForm.Text = "RWR/Azimuth Indicator";
                _rwrForm.ShowInTaskbar = false;
                _rwrForm.ShowIcon = false;
                if (Settings.Default.RWR_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rwrForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.RWR_OutULX, Settings.Default.RWR_OutULY);
                    size = new Size(Settings.Default.RWR_OutLRX - Settings.Default.RWR_OutULX,
                                    Settings.Default.RWR_OutLRY - Settings.Default.RWR_OutULY);
                    _rwrForm.StretchToFill = false;
                }
                _rwrForm.AlwaysOnTop = Settings.Default.RWR_AlwaysOnTop;
                _rwrForm.Monochrome = Settings.Default.RWR_Monochrome;
                _rwrForm.Rotation = Settings.Default.RWR_RotateFlipType;
                _rwrForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rwrForm, _applicationForm, screen, location, size, true,
                                                             true);
                _rwrForm.DataChanged += _rwrForm_DataChanged;
                _rwrForm.Disposed += _rwrForm_Disposed;
                _outputForms.Add(_rwrRenderer, _rwrForm);
            }
        }

        private void SetupOIL2Form()
        {
            if (Settings.Default.EnableOIL2Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.OIL2_OutputDisplay);
                _oilGauge2Form = new InstrumentForm();
                _oilGauge2Form.Text = "Engine 2 - Oil Pressure Indicator";
                _oilGauge2Form.ShowInTaskbar = false;
                _oilGauge2Form.ShowIcon = false;
                if (Settings.Default.OIL2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _oilGauge2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.OIL2_OutULX, Settings.Default.OIL2_OutULY);
                    size = new Size(Settings.Default.OIL2_OutLRX - Settings.Default.OIL2_OutULX,
                                    Settings.Default.OIL2_OutLRY - Settings.Default.OIL2_OutULY);
                    _oilGauge2Form.StretchToFill = false;
                }
                _oilGauge2Form.AlwaysOnTop = Settings.Default.OIL2_AlwaysOnTop;
                _oilGauge2Form.Monochrome = Settings.Default.OIL2_Monochrome;
                _oilGauge2Form.Rotation = Settings.Default.OIL2_RotateFlipType;
                _oilGauge2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_oilGauge2Form, _applicationForm, screen, location, size,
                                                             true, true);
                _oilGauge2Form.DataChanged += _oilGauge2Form_DataChanged;
                _oilGauge2Form.Disposed += _oilGauge2Form_Disposed;
                _outputForms.Add(_oilGauge2Renderer, _oilGauge2Form);
            }
        }

        private void SetupOIL1Form()
        {
            if (Settings.Default.EnableOIL1Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.OIL1_OutputDisplay);
                _oilGauge1Form = new InstrumentForm();
                _oilGauge1Form.Text = "Engine 1 - Oil Pressure Indicator";
                _oilGauge1Form.ShowInTaskbar = false;
                _oilGauge1Form.ShowIcon = false;
                if (Settings.Default.OIL1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _oilGauge1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.OIL1_OutULX, Settings.Default.OIL1_OutULY);
                    size = new Size(Settings.Default.OIL1_OutLRX - Settings.Default.OIL1_OutULX,
                                    Settings.Default.OIL1_OutLRY - Settings.Default.OIL1_OutULY);
                    _oilGauge1Form.StretchToFill = false;
                }
                _oilGauge1Form.AlwaysOnTop = Settings.Default.OIL1_AlwaysOnTop;
                _oilGauge1Form.Monochrome = Settings.Default.OIL1_Monochrome;
                _oilGauge1Form.Rotation = Settings.Default.OIL1_RotateFlipType;
                _oilGauge1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_oilGauge1Form, _applicationForm, screen, location, size,
                                                             true, true);
                _oilGauge1Form.DataChanged += _oilGauge1Form_DataChanged;
                _oilGauge1Form.Disposed += _oilGauge1Form_Disposed;
                _outputForms.Add(_oilGauge1Renderer, _oilGauge1Form);
            }
        }

        private void SetupNOZ2Form()
        {
            if (Settings.Default.EnableNOZ2Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.NOZ2_OutputDisplay);
                _nozPos2Form = new InstrumentForm();
                _nozPos2Form.Text = "Engine 2 - Nozzle Position Indicator";
                _nozPos2Form.ShowInTaskbar = false;
                _nozPos2Form.ShowIcon = false;
                if (Settings.Default.NOZ2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nozPos2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.NOZ2_OutULX, Settings.Default.NOZ2_OutULY);
                    size = new Size(Settings.Default.NOZ2_OutLRX - Settings.Default.NOZ2_OutULX,
                                    Settings.Default.NOZ2_OutLRY - Settings.Default.NOZ2_OutULY);
                    _nozPos2Form.StretchToFill = false;
                }
                _nozPos2Form.AlwaysOnTop = Settings.Default.NOZ2_AlwaysOnTop;
                _nozPos2Form.Monochrome = Settings.Default.NOZ2_Monochrome;
                _nozPos2Form.Rotation = Settings.Default.NOZ2_RotateFlipType;
                _nozPos2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nozPos2Form, _applicationForm, screen, location, size,
                                                             true, true);
                _nozPos2Form.DataChanged += _nozPos2Form_DataChanged;
                _nozPos2Form.Disposed += _nozPos2Form_Disposed;
                _outputForms.Add(_nozPos2Renderer, _nozPos2Form);
            }
        }

        private void SetupNOZ1Form()
        {
            if (Settings.Default.EnableNOZ1Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.NOZ1_OutputDisplay);
                _nozPos1Form = new InstrumentForm();
                _nozPos1Form.Text = "Engine 1 - Nozzle Position Indicator";
                _nozPos1Form.ShowInTaskbar = false;
                _nozPos1Form.ShowIcon = false;
                if (Settings.Default.NOZ1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nozPos1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.NOZ1_OutULX, Settings.Default.NOZ1_OutULY);
                    size = new Size(Settings.Default.NOZ1_OutLRX - Settings.Default.NOZ1_OutULX,
                                    Settings.Default.NOZ1_OutLRY - Settings.Default.NOZ1_OutULY);
                    _nozPos1Form.StretchToFill = false;
                }
                _nozPos1Form.AlwaysOnTop = Settings.Default.NOZ1_AlwaysOnTop;
                _nozPos1Form.Monochrome = Settings.Default.NOZ1_Monochrome;
                _nozPos1Form.Rotation = Settings.Default.NOZ1_RotateFlipType;
                _nozPos1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nozPos1Form, _applicationForm, screen, location, size,
                                                             true, true);
                _nozPos1Form.DataChanged += _nozPos1Form_DataChanged;
                _nozPos1Form.Disposed += _nozPos1Form_Disposed;
                _outputForms.Add(_nozPos1Renderer, _nozPos1Form);
            }
        }

        private void SetupNWSIndexerForm()
        {
            if (Settings.Default.EnableNWSIndexerOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.NWSIndexer_OutputDisplay);
                _nwsIndexerForm = new InstrumentForm();
                _nwsIndexerForm.Text = "NWS Indexer";
                _nwsIndexerForm.ShowInTaskbar = false;
                _nwsIndexerForm.ShowIcon = false;
                if (Settings.Default.NWSIndexer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nwsIndexerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.NWSIndexer_OutULX, Settings.Default.NWSIndexer_OutULY);
                    size = new Size(Settings.Default.NWSIndexer_OutLRX - Settings.Default.NWSIndexer_OutULX,
                                    Settings.Default.NWSIndexer_OutLRY - Settings.Default.NWSIndexer_OutULY);
                    _nwsIndexerForm.StretchToFill = false;
                }
                _nwsIndexerForm.AlwaysOnTop = Settings.Default.NWSIndexer_AlwaysOnTop;
                _nwsIndexerForm.Monochrome = Settings.Default.NWSIndexer_Monochrome;
                _nwsIndexerForm.Rotation = Settings.Default.NWSIndexer_RotateFlipType;
                _nwsIndexerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nwsIndexerForm, _applicationForm, screen, location, size,
                                                             true, true);
                _nwsIndexerForm.DataChanged += _nwsIndexerForm_DataChanged;
                _nwsIndexerForm.Disposed += _nwsIndexerForm_Disposed;
                _outputForms.Add(_nwsIndexerRenderer, _nwsIndexerForm);
            }
        }

        private void SetupGearLightsForm()
        {
            if (Settings.Default.EnableGearLightsOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.GearLights_OutputDisplay);
                _landingGearLightsForm = new InstrumentForm();
                _landingGearLightsForm.Text = "Landing Gear Lights";
                _landingGearLightsForm.ShowInTaskbar = false;
                _landingGearLightsForm.ShowIcon = false;
                if (Settings.Default.GearLights_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _landingGearLightsForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.GearLights_OutULX, Settings.Default.GearLights_OutULY);
                    size = new Size(Settings.Default.GearLights_OutLRX - Settings.Default.GearLights_OutULX,
                                    Settings.Default.GearLights_OutLRY - Settings.Default.GearLights_OutULY);
                    _landingGearLightsForm.StretchToFill = false;
                }
                _landingGearLightsForm.AlwaysOnTop = Settings.Default.GearLights_AlwaysOnTop;
                _landingGearLightsForm.Monochrome = Settings.Default.GearLights_Monochrome;
                _landingGearLightsForm.Rotation = Settings.Default.GearLights_RotateFlipType;
                _landingGearLightsForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_landingGearLightsForm, _applicationForm, screen, location,
                                                             size, true, true);
                _landingGearLightsForm.DataChanged += _landingGearLightsForm_DataChanged;
                _landingGearLightsForm.Disposed += _landingGearLightsForm_Disposed;
                _outputForms.Add(_landingGearLightsRenderer, _landingGearLightsForm);
            }
        }

        private void SetupHSIForm()
        {
            if (Settings.Default.EnableHSIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.HSI_OutputDisplay);
                _hsiForm = new InstrumentForm();
                _hsiForm.Text = "HSI";
                _hsiForm.ShowInTaskbar = false;
                _hsiForm.ShowIcon = false;
                if (Settings.Default.HSI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hsiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.HSI_OutULX, Settings.Default.HSI_OutULY);
                    size = new Size(Settings.Default.HSI_OutLRX - Settings.Default.HSI_OutULX,
                                    Settings.Default.HSI_OutLRY - Settings.Default.HSI_OutULY);
                    _hsiForm.StretchToFill = false;
                }
                _hsiForm.AlwaysOnTop = Settings.Default.HSI_AlwaysOnTop;
                _hsiForm.Monochrome = Settings.Default.HSI_Monochrome;
                _hsiForm.Rotation = Settings.Default.HSI_RotateFlipType;
                _hsiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hsiForm, _applicationForm, screen, location, size, true,
                                                             true);
                _hsiForm.DataChanged += _hsiForm_DataChanged;
                _hsiForm.Disposed += _hsiForm_Disposed;
                _outputForms.Add(_hsiRenderer, _hsiForm);
            }
        }

        private void SetupEHSIForm()
        {
            if (Settings.Default.EnableEHSIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.EHSI_OutputDisplay);
                _ehsiForm = new InstrumentForm();
                _ehsiForm.Text = "EHSI";
                _ehsiForm.ShowInTaskbar = false;
                _ehsiForm.ShowIcon = false;
                if (Settings.Default.EHSI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ehsiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.EHSI_OutULX, Settings.Default.EHSI_OutULY);
                    size = new Size(Settings.Default.EHSI_OutLRX - Settings.Default.EHSI_OutULX,
                                    Settings.Default.EHSI_OutLRY - Settings.Default.EHSI_OutULY);
                    _ehsiForm.StretchToFill = false;
                }
                _ehsiForm.AlwaysOnTop = Settings.Default.EHSI_AlwaysOnTop;
                _ehsiForm.Monochrome = Settings.Default.EHSI_Monochrome;
                _ehsiForm.Rotation = Settings.Default.EHSI_RotateFlipType;
                _ehsiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ehsiForm, _applicationForm, screen, location, size, true,
                                                             true);
                _ehsiForm.DataChanged += _ehsiForm_DataChanged;
                _ehsiForm.Disposed += _ehsiForm_Disposed;
                _outputForms.Add(_ehsiRenderer, _ehsiForm);
            }
        }

        private void SetupFuelQuantityForm()
        {
            if (Settings.Default.EnableFuelQuantityOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.FuelQuantity_OutputDisplay);
                _fuelQuantityForm = new InstrumentForm();
                _fuelQuantityForm.Text = "Fuel Quantity";
                _fuelQuantityForm.ShowInTaskbar = false;
                _fuelQuantityForm.ShowIcon = false;
                if (Settings.Default.FuelQuantity_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _fuelQuantityForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.FuelQuantity_OutULX, Settings.Default.FuelQuantity_OutULY);
                    size = new Size(Settings.Default.FuelQuantity_OutLRX - Settings.Default.FuelQuantity_OutULX,
                                    Settings.Default.FuelQuantity_OutLRY - Settings.Default.FuelQuantity_OutULY);
                    _fuelQuantityForm.StretchToFill = false;
                }
                _fuelQuantityForm.AlwaysOnTop = Settings.Default.FuelQuantity_AlwaysOnTop;
                _fuelQuantityForm.Monochrome = Settings.Default.FuelQuantity_Monochrome;
                _fuelQuantityForm.Rotation = Settings.Default.FuelQuantity_RotateFlipType;
                _fuelQuantityForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_fuelQuantityForm, _applicationForm, screen, location, size,
                                                             true, true);
                _fuelQuantityForm.DataChanged += _fuelQuantityForm_DataChanged;
                _fuelQuantityForm.Disposed += _fuelQuantityForm_Disposed;
                _outputForms.Add(_fuelQuantityRenderer, _fuelQuantityForm);
            }
        }

        private void SetupFuelFlowForm()
        {
            if (Settings.Default.EnableFuelFlowOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.FuelFlow_OutputDisplay);
                _fuelFlowForm = new InstrumentForm();
                _fuelFlowForm.Text = "Fuel Flow Indicator";
                _fuelFlowForm.ShowInTaskbar = false;
                _fuelFlowForm.ShowIcon = false;
                if (Settings.Default.FuelFlow_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _fuelFlowForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.FuelFlow_OutULX, Settings.Default.FuelFlow_OutULY);
                    size = new Size(Settings.Default.FuelFlow_OutLRX - Settings.Default.FuelFlow_OutULX,
                                    Settings.Default.FuelFlow_OutLRY - Settings.Default.FuelFlow_OutULY);
                    _fuelFlowForm.StretchToFill = false;
                }
                _fuelFlowForm.AlwaysOnTop = Settings.Default.FuelFlow_AlwaysOnTop;
                _fuelFlowForm.Monochrome = Settings.Default.FuelFlow_Monochrome;
                _fuelFlowForm.Rotation = Settings.Default.FuelFlow_RotateFlipType;
                _fuelFlowForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_fuelFlowForm, _applicationForm, screen, location, size,
                                                             true, true);
                _fuelFlowForm.DataChanged += _fuelFlowForm_DataChanged;
                _fuelFlowForm.Disposed += _fuelFlowForm_Disposed;
                _outputForms.Add(_fuelFlowRenderer, _fuelFlowForm);
            }
        }

        private void SetupISISForm()
        {
            if (Settings.Default.EnableISISOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.ISIS_OutputDisplay);
                _isisForm = new InstrumentForm();
                _isisForm.Text = "ISIS";
                _isisForm.ShowInTaskbar = false;
                _isisForm.ShowIcon = false;
                if (Settings.Default.ISIS_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _isisForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.ISIS_OutULX, Settings.Default.ISIS_OutULY);
                    size = new Size(Settings.Default.ISIS_OutLRX - Settings.Default.ISIS_OutULX,
                                    Settings.Default.ISIS_OutLRY - Settings.Default.ISIS_OutULY);
                    _isisForm.StretchToFill = false;
                }
                _isisForm.AlwaysOnTop = Settings.Default.ISIS_AlwaysOnTop;
                _isisForm.Monochrome = Settings.Default.ISIS_Monochrome;
                _isisForm.Rotation = Settings.Default.ISIS_RotateFlipType;
                _isisForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_isisForm, _applicationForm, screen, location, size, true,
                                                             true);
                _isisForm.DataChanged += _isisForm_DataChanged;
                _isisForm.Disposed += _isisForm_Disposed;
                _outputForms.Add(_isisRenderer, _isisForm);
            }
        }

        private void SetupAccelerometerForm()
        {
            if (Settings.Default.EnableAccelerometerOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.Accelerometer_OutputDisplay);
                _accelerometerForm = new InstrumentForm();
                _accelerometerForm.Text = "Accelerometer (G-Meter)";
                _accelerometerForm.ShowInTaskbar = false;
                _accelerometerForm.ShowIcon = false;
                if (Settings.Default.Accelerometer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _accelerometerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.Accelerometer_OutULX, Settings.Default.Accelerometer_OutULY);
                    size = new Size(Settings.Default.Accelerometer_OutLRX - Settings.Default.Accelerometer_OutULX,
                                    Settings.Default.Accelerometer_OutLRY - Settings.Default.Accelerometer_OutULY);
                    _accelerometerForm.StretchToFill = false;
                }
                _accelerometerForm.AlwaysOnTop = Settings.Default.Accelerometer_AlwaysOnTop;
                _accelerometerForm.Monochrome = Settings.Default.Accelerometer_Monochrome;
                _accelerometerForm.Rotation = Settings.Default.Accelerometer_RotateFlipType;
                _accelerometerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_accelerometerForm, _applicationForm, screen, location,
                                                             size, true, true);
                _accelerometerForm.DataChanged += _accelerometerForm_DataChanged;
                _accelerometerForm.Disposed += _accelerometerForm_Disposed;
                _outputForms.Add(_accelerometerRenderer, _accelerometerForm);
            }
        }

        private void SetupFTIT2Form()
        {
            if (Settings.Default.EnableFTIT2Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.FTIT2_OutputDisplay);
                _ftit2Form = new InstrumentForm();
                _ftit2Form.Text = "Engine 2 - FTIT";
                _ftit2Form.ShowInTaskbar = false;
                _ftit2Form.ShowIcon = false;
                if (Settings.Default.FTIT2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ftit2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.FTIT2_OutULX, Settings.Default.FTIT2_OutULY);
                    size = new Size(Settings.Default.FTIT2_OutLRX - Settings.Default.FTIT2_OutULX,
                                    Settings.Default.FTIT2_OutLRY - Settings.Default.FTIT2_OutULY);
                    _ftit2Form.StretchToFill = false;
                }
                _ftit2Form.AlwaysOnTop = Settings.Default.FTIT2_AlwaysOnTop;
                _ftit2Form.Monochrome = Settings.Default.FTIT2_Monochrome;
                _ftit2Form.Rotation = Settings.Default.FTIT2_RotateFlipType;
                _ftit2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ftit2Form, _applicationForm, screen, location, size, true,
                                                             true);
                _ftit2Form.DataChanged += _ftit2Form_DataChanged;
                _ftit2Form.Disposed += _ftit2Form_Disposed;
                _outputForms.Add(_ftit2Renderer, _ftit2Form);
            }
        }

        private void SetupFTIT1Form()
        {
            if (Settings.Default.EnableFTIT1Output)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.FTIT1_OutputDisplay);
                _ftit1Form = new InstrumentForm();
                _ftit1Form.Text = "Engine 1 - FTIT";
                _ftit1Form.ShowInTaskbar = false;
                _ftit1Form.ShowIcon = false;
                if (Settings.Default.FTIT1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ftit1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.FTIT1_OutULX, Settings.Default.FTIT1_OutULY);
                    size = new Size(Settings.Default.FTIT1_OutLRX - Settings.Default.FTIT1_OutULX,
                                    Settings.Default.FTIT1_OutLRY - Settings.Default.FTIT1_OutULY);
                    _ftit1Form.StretchToFill = false;
                }
                _ftit1Form.AlwaysOnTop = Settings.Default.FTIT1_AlwaysOnTop;
                _ftit1Form.Monochrome = Settings.Default.FTIT1_Monochrome;
                _ftit1Form.Rotation = Settings.Default.FTIT1_RotateFlipType;
                _ftit1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ftit1Form, _applicationForm, screen, location, size, true,
                                                             true);
                _ftit1Form.DataChanged += _ftit1Form_DataChanged;
                _ftit1Form.Disposed += _ftit1Form_Disposed;
                _outputForms.Add(_ftit1Renderer, _ftit1Form);
            }
        }

        private void SetupEPUFuelForm()
        {
            if (Settings.Default.EnableEPUFuelOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.EPUFuel_OutputDisplay);
                _epuFuelForm = new InstrumentForm();
                _epuFuelForm.Text = "EPU Fuel";
                _epuFuelForm.ShowInTaskbar = false;
                _epuFuelForm.ShowIcon = false;
                if (Settings.Default.EPUFuel_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _epuFuelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.EPUFuel_OutULX, Settings.Default.EPUFuel_OutULY);
                    size = new Size(Settings.Default.EPUFuel_OutLRX - Settings.Default.EPUFuel_OutULX,
                                    Settings.Default.EPUFuel_OutLRY - Settings.Default.EPUFuel_OutULY);
                    _epuFuelForm.StretchToFill = false;
                }
                _epuFuelForm.AlwaysOnTop = Settings.Default.EPUFuel_AlwaysOnTop;
                _epuFuelForm.Monochrome = Settings.Default.EPUFuel_Monochrome;
                _epuFuelForm.Rotation = Settings.Default.EPUFuel_RotateFlipType;
                _epuFuelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_epuFuelForm, _applicationForm, screen, location, size,
                                                             true, true);
                _epuFuelForm.DataChanged += _epuFuelForm_DataChanged;
                _epuFuelForm.Disposed += _epuFuelForm_Disposed;
                _outputForms.Add(_epuFuelRenderer, _epuFuelForm);
            }
        }

        private void SetupPFLForm()
        {
            if (Settings.Default.EnablePFLOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.PFL_OutputDisplay);
                _pflForm = new InstrumentForm();
                _pflForm.Text = "Pilot Fault List (PFL)";
                _pflForm.ShowInTaskbar = false;
                _pflForm.ShowIcon = false;
                if (Settings.Default.PFL_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _pflForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.PFL_OutULX, Settings.Default.PFL_OutULY);
                    size = new Size(Settings.Default.PFL_OutLRX - Settings.Default.PFL_OutULX,
                                    Settings.Default.PFL_OutLRY - Settings.Default.PFL_OutULY);
                    _pflForm.StretchToFill = false;
                }
                _pflForm.AlwaysOnTop = Settings.Default.PFL_AlwaysOnTop;
                _pflForm.Monochrome = Settings.Default.PFL_Monochrome;
                _pflForm.Rotation = Settings.Default.PFL_RotateFlipType;
                _pflForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_pflForm, _applicationForm, screen, location, size, true,
                                                             true);
                _pflForm.DataChanged += _pflForm_DataChanged;
                _pflForm.Disposed += _pflForm_Disposed;
                _outputForms.Add(_pflRenderer, _pflForm);
            }
        }

        private void SetupDEDForm()
        {
            if (Settings.Default.EnableDEDOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.DED_OutputDisplay);
                _dedForm = new InstrumentForm();
                _dedForm.Text = "Data Entry Display (DED)";
                _dedForm.ShowInTaskbar = false;
                _dedForm.ShowIcon = false;
                if (Settings.Default.DED_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _dedForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.DED_OutULX, Settings.Default.DED_OutULY);
                    size = new Size(Settings.Default.DED_OutLRX - Settings.Default.DED_OutULX,
                                    Settings.Default.DED_OutLRY - Settings.Default.DED_OutULY);
                    _dedForm.StretchToFill = false;
                }
                _dedForm.AlwaysOnTop = Settings.Default.DED_AlwaysOnTop;
                _dedForm.Monochrome = Settings.Default.DED_Monochrome;
                _dedForm.Rotation = Settings.Default.DED_RotateFlipType;
                _dedForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_dedForm, _applicationForm, screen, location, size, true,
                                                             true);
                _dedForm.DataChanged += _dedForm_DataChanged;
                _dedForm.Disposed += _dedForm_Disposed;
                _outputForms.Add(_dedRenderer, _dedForm);
            }
        }

        private void SetupCompassForm()
        {
            if (Settings.Default.EnableCompassOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.Compass_OutputDisplay);
                _compassForm = new InstrumentForm();
                _compassForm.Text = "Compass";
                _compassForm.ShowInTaskbar = false;
                _compassForm.ShowIcon = false;
                if (Settings.Default.Compass_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _compassForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.Compass_OutULX, Settings.Default.Compass_OutULY);
                    size = new Size(Settings.Default.Compass_OutLRX - Settings.Default.Compass_OutULX,
                                    Settings.Default.Compass_OutLRY - Settings.Default.Compass_OutULY);
                    _compassForm.StretchToFill = false;
                }
                _compassForm.AlwaysOnTop = Settings.Default.Compass_AlwaysOnTop;
                _compassForm.Monochrome = Settings.Default.Compass_Monochrome;
                _compassForm.Rotation = Settings.Default.Compass_RotateFlipType;
                _compassForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_compassForm, _applicationForm, screen, location, size,
                                                             true, true);
                _compassForm.DataChanged += _compassForm_DataChanged;
                _compassForm.Disposed += _compassForm_Disposed;
                _outputForms.Add(_compassRenderer, _compassForm);
            }
        }

        private void SetupCMDSPanelForm()
        {
            if (Settings.Default.EnableCMDSOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.CMDS_OutputDisplay);
                _cmdsPanelForm = new InstrumentForm();
                _cmdsPanelForm.Text = "CMDS Panel";
                _cmdsPanelForm.ShowInTaskbar = false;
                _cmdsPanelForm.ShowIcon = false;
                if (Settings.Default.CMDS_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cmdsPanelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.CMDS_OutULX, Settings.Default.CMDS_OutULY);
                    size = new Size(Settings.Default.CMDS_OutLRX - Settings.Default.CMDS_OutULX,
                                    Settings.Default.CMDS_OutLRY - Settings.Default.CMDS_OutULY);
                    _cmdsPanelForm.StretchToFill = false;
                }
                _cmdsPanelForm.AlwaysOnTop = Settings.Default.CMDS_AlwaysOnTop;
                _cmdsPanelForm.Monochrome = Settings.Default.CMDS_Monochrome;
                _cmdsPanelForm.Rotation = Settings.Default.CMDS_RotateFlipType;
                _cmdsPanelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cmdsPanelForm, _applicationForm, screen, location, size,
                                                             true, true);
                _cmdsPanelForm.DataChanged += _cmdsPanelForm_DataChanged;
                _cmdsPanelForm.Disposed += _cmdsPanelForm_Disposed;
                _outputForms.Add(_cmdsPanelRenderer, _cmdsPanelForm);
            }
        }

        private void SetupCautionPanelForm()
        {
            if (Settings.Default.EnableCautionPanelOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.CautionPanel_OutputDisplay);
                _cautionPanelForm = new InstrumentForm();
                _cautionPanelForm.Text = "Caution Panel";
                _cautionPanelForm.ShowInTaskbar = false;
                _cautionPanelForm.ShowIcon = false;
                if (Settings.Default.CautionPanel_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cautionPanelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.CautionPanel_OutULX, Settings.Default.CautionPanel_OutULY);
                    size = new Size(Settings.Default.CautionPanel_OutLRX - Settings.Default.CautionPanel_OutULX,
                                    Settings.Default.CautionPanel_OutLRY - Settings.Default.CautionPanel_OutULY);
                    _cautionPanelForm.StretchToFill = false;
                }
                _cautionPanelForm.AlwaysOnTop = Settings.Default.CautionPanel_AlwaysOnTop;
                _cautionPanelForm.Monochrome = Settings.Default.CautionPanel_Monochrome;
                _cautionPanelForm.Rotation = Settings.Default.CautionPanel_RotateFlipType;
                _cautionPanelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cautionPanelForm, _applicationForm, screen, location, size,
                                                             true, true);
                _cautionPanelForm.DataChanged += _cautionPanelForm_DataChanged;
                _cautionPanelForm.Disposed += _cautionPanelForm_Disposed;
                _outputForms.Add(_cautionPanelRenderer, _cautionPanelForm);
            }
        }

        private void SetupAOAIndicatorForm()
        {
            if (Settings.Default.EnableAOAIndicatorOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.AOAIndicator_OutputDisplay);
                _aoaIndicatorForm = new InstrumentForm();
                _aoaIndicatorForm.Text = "AOA Indicator";
                _aoaIndicatorForm.ShowInTaskbar = false;
                _aoaIndicatorForm.ShowIcon = false;
                if (Settings.Default.AOAIndicator_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _aoaIndicatorForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.AOAIndicator_OutULX, Settings.Default.AOAIndicator_OutULY);
                    size = new Size(Settings.Default.AOAIndicator_OutLRX - Settings.Default.AOAIndicator_OutULX,
                                    Settings.Default.AOAIndicator_OutLRY - Settings.Default.AOAIndicator_OutULY);
                    _aoaIndicatorForm.StretchToFill = false;
                }
                _aoaIndicatorForm.AlwaysOnTop = Settings.Default.AOAIndicator_AlwaysOnTop;
                _aoaIndicatorForm.Monochrome = Settings.Default.AOAIndicator_Monochrome;
                _aoaIndicatorForm.Rotation = Settings.Default.AOAIndicator_RotateFlipType;
                _aoaIndicatorForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_aoaIndicatorForm, _applicationForm, screen, location, size,
                                                             true, true);
                _aoaIndicatorForm.DataChanged += _aoaIndicatorForm_DataChanged;
                _aoaIndicatorForm.Disposed += _aoaIndicatorForm_Disposed;
                _outputForms.Add(_aoaIndicatorRenderer, _aoaIndicatorForm);
            }
        }

        private void SetupAOAIndexerForm()
        {
            if (Settings.Default.EnableAOAIndexerOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.AOAIndexer_OutputDisplay);
                _aoaIndexerForm = new InstrumentForm();
                _aoaIndexerForm.Text = "AOA Indexer";
                _aoaIndexerForm.ShowInTaskbar = false;
                _aoaIndexerForm.ShowIcon = false;
                if (Settings.Default.AOAIndexer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _aoaIndexerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.AOAIndexer_OutULX, Settings.Default.AOAIndexer_OutULY);
                    size = new Size(Settings.Default.AOAIndexer_OutLRX - Settings.Default.AOAIndexer_OutULX,
                                    Settings.Default.AOAIndexer_OutLRY - Settings.Default.AOAIndexer_OutULY);
                    _aoaIndexerForm.StretchToFill = false;
                }
                _aoaIndexerForm.AlwaysOnTop = Settings.Default.AOAIndexer_AlwaysOnTop;
                _aoaIndexerForm.Monochrome = Settings.Default.AOAIndexer_Monochrome;
                _aoaIndexerForm.Rotation = Settings.Default.AOAIndexer_RotateFlipType;
                _aoaIndexerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_aoaIndexerForm, _applicationForm, screen, location, size,
                                                             true, true);
                _aoaIndexerForm.DataChanged += _aoaIndexerForm_DataChanged;
                _aoaIndexerForm.Disposed += _aoaIndexerForm_Disposed;
                _outputForms.Add(_aoaIndexerRenderer, _aoaIndexerForm);
            }
        }

        private void SetupAltimeterForm()
        {
            if (Settings.Default.EnableAltimeterOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.Altimeter_OutputDisplay);
                _altimeterForm = new InstrumentForm();
                _altimeterForm.Text = "Altimeter";
                _altimeterForm.ShowInTaskbar = false;
                _altimeterForm.ShowIcon = false;
                if (Settings.Default.Altimeter_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _altimeterForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.Altimeter_OutULX, Settings.Default.Altimeter_OutULY);
                    size = new Size(Settings.Default.Altimeter_OutLRX - Settings.Default.Altimeter_OutULX,
                                    Settings.Default.Altimeter_OutLRY - Settings.Default.Altimeter_OutULY);
                    _altimeterForm.StretchToFill = false;
                }
                _altimeterForm.AlwaysOnTop = Settings.Default.Altimeter_AlwaysOnTop;
                _altimeterForm.Monochrome = Settings.Default.Altimeter_Monochrome;
                _altimeterForm.Rotation = Settings.Default.Altimeter_RotateFlipType;
                _altimeterForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_altimeterForm, _applicationForm, screen, location, size,
                                                             true, true);
                _altimeterForm.DataChanged += _altimeterForm_DataChanged;
                _altimeterForm.Disposed += _altimeterForm_Disposed;
                _outputForms.Add(_altimeterRenderer, _altimeterForm);
            }
        }

        private void SetupCabinPressForm()
        {
            if (Settings.Default.EnableCabinPressOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.CabinPress_OutputDisplay);
                _cabinPressForm = new InstrumentForm();
                _cabinPressForm.Text = "Cabin Pressure Altitude Indicator";
                _cabinPressForm.ShowInTaskbar = false;
                _cabinPressForm.ShowIcon = false;
                if (Settings.Default.CabinPress_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cabinPressForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.CabinPress_OutULX, Settings.Default.CabinPress_OutULY);
                    size = new Size(Settings.Default.CabinPress_OutLRX - Settings.Default.CabinPress_OutULX,
                                    Settings.Default.CabinPress_OutLRY - Settings.Default.CabinPress_OutULY);
                    _cabinPressForm.StretchToFill = false;
                }
                _cabinPressForm.AlwaysOnTop = Settings.Default.CabinPress_AlwaysOnTop;
                _cabinPressForm.Monochrome = Settings.Default.CabinPress_Monochrome;
                _cabinPressForm.Rotation = Settings.Default.CabinPress_RotateFlipType;
                _cabinPressForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cabinPressForm, _applicationForm, screen, location, size,
                                                             true, true);
                _cabinPressForm.DataChanged += _cabinPressForm_DataChanged;
                _cabinPressForm.Disposed += _cabinPressForm_Disposed;
                _outputForms.Add(_cabinPressRenderer, _cabinPressForm);
            }
        }

        private void SetupRollTrimForm()
        {
            if (Settings.Default.EnableRollTrimOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.RollTrim_OutputDisplay);
                _rollTrimForm = new InstrumentForm();
                _rollTrimForm.Text = "Roll Trim Indicator";
                _rollTrimForm.ShowInTaskbar = false;
                _rollTrimForm.ShowIcon = false;
                if (Settings.Default.RollTrim_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rollTrimForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.RollTrim_OutULX, Settings.Default.RollTrim_OutULY);
                    size = new Size(Settings.Default.RollTrim_OutLRX - Settings.Default.RollTrim_OutULX,
                                    Settings.Default.RollTrim_OutLRY - Settings.Default.RollTrim_OutULY);
                    _rollTrimForm.StretchToFill = false;
                }
                _rollTrimForm.AlwaysOnTop = Settings.Default.RollTrim_AlwaysOnTop;
                _rollTrimForm.Monochrome = Settings.Default.RollTrim_Monochrome;
                _rollTrimForm.Rotation = Settings.Default.RollTrim_RotateFlipType;
                _rollTrimForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rollTrimForm, _applicationForm, screen, location, size,
                                                             true, true);
                _rollTrimForm.DataChanged += _rollTrimForm_DataChanged;
                _rollTrimForm.Disposed += _rollTrimForm_Disposed;
                _outputForms.Add(_rollTrimRenderer, _rollTrimForm);
            }
        }

        private void SetupPitchTrimForm()
        {
            if (Settings.Default.EnablePitchTrimOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.PitchTrim_OutputDisplay);
                _pitchTrimForm = new InstrumentForm();
                _pitchTrimForm.Text = "Pitch Trim Indicator";
                _pitchTrimForm.ShowInTaskbar = false;
                _pitchTrimForm.ShowIcon = false;
                if (Settings.Default.PitchTrim_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _pitchTrimForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.PitchTrim_OutULX, Settings.Default.PitchTrim_OutULY);
                    size = new Size(Settings.Default.PitchTrim_OutLRX - Settings.Default.PitchTrim_OutULX,
                                    Settings.Default.PitchTrim_OutLRY - Settings.Default.PitchTrim_OutULY);
                    _pitchTrimForm.StretchToFill = false;
                }
                _pitchTrimForm.AlwaysOnTop = Settings.Default.PitchTrim_AlwaysOnTop;
                _pitchTrimForm.Monochrome = Settings.Default.PitchTrim_Monochrome;
                _pitchTrimForm.Rotation = Settings.Default.PitchTrim_RotateFlipType;
                _pitchTrimForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_pitchTrimForm, _applicationForm, screen, location, size,
                                                             true, true);
                _pitchTrimForm.DataChanged += _pitchTrimForm_DataChanged;
                _pitchTrimForm.Disposed += _pitchTrimForm_Disposed;
                _outputForms.Add(_pitchTrimRenderer, _pitchTrimForm);
            }
        }

        private void SetupHydAForm()
        {
            if (Settings.Default.EnableHYDAOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.HYDA_OutputDisplay);
                _hydAForm = new InstrumentForm();
                _hydAForm.Text = "Hydraulic Pressure Indicator A";
                _hydAForm.ShowInTaskbar = false;
                _hydAForm.ShowIcon = false;
                if (Settings.Default.HYDA_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hydAForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.HYDA_OutULX, Settings.Default.HYDA_OutULY);
                    size = new Size(Settings.Default.HYDA_OutLRX - Settings.Default.HYDA_OutULX,
                                    Settings.Default.HYDA_OutLRY - Settings.Default.HYDA_OutULY);
                    _hydAForm.StretchToFill = false;
                }
                _hydAForm.AlwaysOnTop = Settings.Default.HYDA_AlwaysOnTop;
                _hydAForm.Monochrome = Settings.Default.HYDA_Monochrome;
                _hydAForm.Rotation = Settings.Default.HYDA_RotateFlipType;
                _hydAForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hydAForm, _applicationForm, screen, location, size, true,
                                                             true);
                _hydAForm.DataChanged += _hydAForm_DataChanged;
                _hydAForm.Disposed += _hydAForm_Disposed;
                _outputForms.Add(_hydARenderer, _hydAForm);
            }
        }

        private void SetupHydBForm()
        {
            if (Settings.Default.EnableHYDBOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.HYDB_OutputDisplay);
                _hydBForm = new InstrumentForm();
                _hydBForm.Text = "Hydraulic Pressure Indicator B";
                _hydBForm.ShowInTaskbar = false;
                _hydBForm.ShowIcon = false;
                if (Settings.Default.HYDB_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hydBForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.HYDB_OutULX, Settings.Default.HYDB_OutULY);
                    size = new Size(Settings.Default.HYDB_OutLRX - Settings.Default.HYDB_OutULX,
                                    Settings.Default.HYDB_OutLRY - Settings.Default.HYDB_OutULY);
                    _hydBForm.StretchToFill = false;
                }
                _hydBForm.AlwaysOnTop = Settings.Default.HYDB_AlwaysOnTop;
                _hydBForm.Monochrome = Settings.Default.HYDB_Monochrome;
                _hydBForm.Rotation = Settings.Default.HYDB_RotateFlipType;
                _hydBForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hydBForm, _applicationForm, screen, location, size, true,
                                                             true);
                _hydBForm.DataChanged += _hydBForm_DataChanged;
                _hydBForm.Disposed += _hydBForm_Disposed;
                _outputForms.Add(_hydBRenderer, _hydBForm);
            }
        }

        private void SetupASIForm()
        {
            if (Settings.Default.EnableASIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.ASI_OutputDisplay);
                _asiForm = new InstrumentForm();
                _asiForm.Text = "Airspeed Indicator";
                _asiForm.ShowInTaskbar = false;
                _asiForm.ShowIcon = false;
                if (Settings.Default.ASI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _asiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.ASI_OutULX, Settings.Default.ASI_OutULY);
                    size = new Size(Settings.Default.ASI_OutLRX - Settings.Default.ASI_OutULX,
                                    Settings.Default.ASI_OutLRY - Settings.Default.ASI_OutULY);
                    _asiForm.StretchToFill = false;
                }
                _asiForm.AlwaysOnTop = Settings.Default.ASI_AlwaysOnTop;
                _asiForm.Monochrome = Settings.Default.ASI_Monochrome;
                _asiForm.Rotation = Settings.Default.ASI_RotateFlipType;
                _asiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_asiForm, _applicationForm, screen, location, size, true,
                                                             true);
                _asiForm.DataChanged += _asiForm_DataChanged;
                _asiForm.Disposed += _asiForm_Disposed;
                _outputForms.Add(_asiRenderer, _asiForm);
            }
        }

        private void SetupADIForm()
        {
            if (Settings.Default.EnableADIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.ADI_OutputDisplay);
                _adiForm = new InstrumentForm();
                _adiForm.Text = "Attitude Indicator";
                _adiForm.ShowInTaskbar = false;
                _adiForm.ShowIcon = false;
                if (Settings.Default.ADI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _adiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.ADI_OutULX, Settings.Default.ADI_OutULY);
                    size = new Size(Settings.Default.ADI_OutLRX - Settings.Default.ADI_OutULX,
                                    Settings.Default.ADI_OutLRY - Settings.Default.ADI_OutULY);
                    _adiForm.StretchToFill = false;
                }
                _adiForm.AlwaysOnTop = Settings.Default.ADI_AlwaysOnTop;
                _adiForm.Monochrome = Settings.Default.ADI_Monochrome;
                _adiForm.Rotation = Settings.Default.ADI_RotateFlipType;
                _adiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_adiForm, _applicationForm, screen, location, size, true,
                                                             true);
                _adiForm.DataChanged += _adiForm_DataChanged;
                _adiForm.Disposed += _adiForm_Disposed;
                _outputForms.Add(_adiRenderer, _adiForm);
            }
        }

        private void SetupBackupADIForm()
        {
            if (Settings.Default.EnableBackupADIOutput)
            {
                Point location;
                var size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Settings.Default.Backup_ADI_OutputDisplay);
                _backupAdiForm = new InstrumentForm();
                _backupAdiForm.Text = "Standby Attitude Indicator";
                _backupAdiForm.ShowInTaskbar = false;
                _backupAdiForm.ShowIcon = false;
                if (Settings.Default.Backup_ADI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _backupAdiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Settings.Default.Backup_ADI_OutULX, Settings.Default.Backup_ADI_OutULY);
                    size = new Size(Settings.Default.Backup_ADI_OutLRX - Settings.Default.Backup_ADI_OutULX,
                                    Settings.Default.Backup_ADI_OutLRY - Settings.Default.Backup_ADI_OutULY);
                    _backupAdiForm.StretchToFill = false;
                }
                _backupAdiForm.AlwaysOnTop = Settings.Default.Backup_ADI_AlwaysOnTop;
                _backupAdiForm.Monochrome = Settings.Default.Backup_ADI_Monochrome;
                _backupAdiForm.Rotation = Settings.Default.Backup_ADI_RotateFlipType;
                _backupAdiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_backupAdiForm, _applicationForm, screen, location, size,
                                                             true, true);
                _backupAdiForm.DataChanged += _backupAdiForm_DataChanged;
                _backupAdiForm.Disposed += _backupAdiForm_Disposed;
                _outputForms.Add(_backupAdiRenderer, _backupAdiForm);
            }
        }

        #endregion

        #endregion

        #region Instrument Form Disposal Event handlers

        private void _vviForm_Disposed(object sender, EventArgs e)
        {
            _vviForm = null;
        }

        private void _rpm2Form_Disposed(object sender, EventArgs e)
        {
            _rpm2Form = null;
        }

        private void _rpm1Form_Disposed(object sender, EventArgs e)
        {
            _rpm1Form = null;
        }

        private void _speedbrakeForm_Disposed(object sender, EventArgs e)
        {
            _speedbrakeForm = null;
        }

        private void _rwrForm_Disposed(object sender, EventArgs e)
        {
            _rwrForm = null;
        }

        private void _landingGearLightsForm_Disposed(object sender, EventArgs e)
        {
            _landingGearLightsForm = null;
        }

        private void _oilGauge1Form_Disposed(object sender, EventArgs e)
        {
            _oilGauge1Form = null;
        }

        private void _oilGauge2Form_Disposed(object sender, EventArgs e)
        {
            _oilGauge2Form = null;
        }

        private void _nozPos1Form_Disposed(object sender, EventArgs e)
        {
            _nozPos1Form = null;
        }

        private void _nozPos2Form_Disposed(object sender, EventArgs e)
        {
            _nozPos2Form = null;
        }

        private void _nwsIndexerForm_Disposed(object sender, EventArgs e)
        {
            _nwsIndexerForm = null;
        }

        private void _hsiForm_Disposed(object sender, EventArgs e)
        {
            _hsiForm = null;
        }

        private void _ehsiForm_Disposed(object sender, EventArgs e)
        {
            _ehsiForm = null;
        }

        private void _fuelQuantityForm_Disposed(object sender, EventArgs e)
        {
            _fuelQuantityForm = null;
        }

        private void _fuelFlowForm_Disposed(object sender, EventArgs e)
        {
            _fuelFlowForm = null;
        }

        private void _isisForm_Disposed(object sender, EventArgs e)
        {
            _isisForm = null;
        }

        private void _accelerometerForm_Disposed(object sender, EventArgs e)
        {
            _accelerometerForm = null;
        }
        private void _ftit2Form_Disposed(object sender, EventArgs e)
        {
            _ftit2Form = null;
        }

        private void _ftit1Form_Disposed(object sender, EventArgs e)
        {
            _ftit1Form = null;
        }

        private void _epuFuelForm_Disposed(object sender, EventArgs e)
        {
            _epuFuelForm = null;
        }

        private void _pflForm_Disposed(object sender, EventArgs e)
        {
            _pflForm = null;
        }

        private void _dedForm_Disposed(object sender, EventArgs e)
        {
            _dedForm = null;
        }

        private void _compassForm_Disposed(object sender, EventArgs e)
        {
            _compassForm = null;
        }

        private void _cmdsPanelForm_Disposed(object sender, EventArgs e)
        {
            _cmdsPanelForm = null;
        }

        private void _cautionPanelForm_Disposed(object sender, EventArgs e)
        {
            _cautionPanelForm = null;
        }

        private void _aoaIndicatorForm_Disposed(object sender, EventArgs e)
        {
            _aoaIndicatorForm = null;
        }

        private void _aoaIndexerForm_Disposed(object sender, EventArgs e)
        {
            _aoaIndexerForm = null;
        }

        private void _altimeterForm_Disposed(object sender, EventArgs e)
        {
            _altimeterForm = null;
        }

        private void _asiForm_Disposed(object sender, EventArgs e)
        {
            _asiForm = null;
        }

        private void _adiForm_Disposed(object sender, EventArgs e)
        {
            _adiForm = null;
        }

        private void _backupAdiForm_Disposed(object sender, EventArgs e)
        {
            _backupAdiForm = null;
        }

        private void _hydAForm_Disposed(object sender, EventArgs e)
        {
            _hydAForm = null;
        }

        private void _hydBForm_Disposed(object sender, EventArgs e)
        {
            _hydBForm = null;
        }

        private void _cabinPressForm_Disposed(object sender, EventArgs e)
        {
            _cabinPressForm = null;
        }

        private void _rollTrimForm_Disposed(object sender, EventArgs e)
        {
            _rollTrimForm = null;
        }

        private void _pitchTrimForm_Disposed(object sender, EventArgs e)
        {
            _pitchTrimForm = null;
        }

        private void _mfd4Form_Disposed(object sender, EventArgs e)
        {
            _mfd4Form = null;
        }

        private void _mfd3Form_Disposed(object sender, EventArgs e)
        {
            _mfd3Form = null;
        }

        private void _leftMfdForm_Disposed(object sender, EventArgs e)
        {
            _leftMfdForm = null;
        }

        private void _rightMfdForm_Disposed(object sender, EventArgs e)
        {
            _rightMfdForm = null;
        }

        private void _hudForm_Disposed(object sender, EventArgs e)
        {
            _hudForm = null;
        }

        #endregion

        #region Output Window Recovery Methods

        private void RecoverInstrumentForm(InstrumentForm form, Screen screen)
        {
            if (form != null)
            {
                form.StretchToFill = false;
                form.Location = screen.Bounds.Location;
                form.BringToFront();
            }
        }

        public void RecoverADIWindow(Screen screen)
        {
            RecoverInstrumentForm(_adiForm, screen);
        }

        public void RecoverBackupADIWindow(Screen screen)
        {
            RecoverInstrumentForm(_backupAdiForm, screen);
        }

        public void RecoverASIWindow(Screen screen)
        {
            RecoverInstrumentForm(_asiForm, screen);
        }

        public void RecoverCabinPressWindow(Screen screen)
        {
            RecoverInstrumentForm(_cabinPressForm, screen);
        }

        public void RecoverRollTrimWindow(Screen screen)
        {
            RecoverInstrumentForm(_rollTrimForm, screen);
        }

        public void RecoverPitchTrimWindow(Screen screen)
        {
            RecoverInstrumentForm(_pitchTrimForm, screen);
        }

        public void RecoverAltimeterWindow(Screen screen)
        {
            RecoverInstrumentForm(_altimeterForm, screen);
        }

        public void RecoverAOAIndexerWindow(Screen screen)
        {
            RecoverInstrumentForm(_aoaIndexerForm, screen);
        }

        public void RecoverAOAIndicatorWindow(Screen screen)
        {
            RecoverInstrumentForm(_aoaIndicatorForm, screen);
        }

        public void RecoverCautionPanelWindow(Screen screen)
        {
            RecoverInstrumentForm(_cautionPanelForm, screen);
        }

        public void RecoverCMDSPanelWindow(Screen screen)
        {
            RecoverInstrumentForm(_cmdsPanelForm, screen);
        }

        public void RecoverCompassWindow(Screen screen)
        {
            RecoverInstrumentForm(_compassForm, screen);
        }

        public void RecoverDEDWindow(Screen screen)
        {
            RecoverInstrumentForm(_dedForm, screen);
        }

        public void RecoverPFLWindow(Screen screen)
        {
            RecoverInstrumentForm(_pflForm, screen);
        }

        public void RecoverEPUFuelWindow(Screen screen)
        {
            RecoverInstrumentForm(_epuFuelForm, screen);
        }

        public void RecoverFTIT1Window(Screen screen)
        {
            RecoverInstrumentForm(_ftit1Form, screen);
        }

        public void RecoverAccelerometerWindow(Screen screen)
        {
            RecoverInstrumentForm(_accelerometerForm, screen);
        }

        public void RecoverFTIT2Window(Screen screen)
        {
            RecoverInstrumentForm(_ftit2Form, screen);
        }

        public void RecoverFuelFlowWindow(Screen screen)
        {
            RecoverInstrumentForm(_fuelFlowForm, screen);
        }

        public void RecoverISISWindow(Screen screen)
        {
            RecoverInstrumentForm(_isisForm, screen);
        }

        public void RecoverFuelQuantityWindow(Screen screen)
        {
            RecoverInstrumentForm(_fuelQuantityForm, screen);
        }

        public void RecoverHSIWindow(Screen screen)
        {
            RecoverInstrumentForm(_hsiForm, screen);
        }

        public void RecoverEHSIWindow(Screen screen)
        {
            RecoverInstrumentForm(_ehsiForm, screen);
        }

        public void RecoverLandingGearLightsWindow(Screen screen)
        {
            RecoverInstrumentForm(_landingGearLightsForm, screen);
        }

        public void RecoverNWSWindow(Screen screen)
        {
            RecoverInstrumentForm(_nwsIndexerForm, screen);
        }

        public void RecoverNOZ1Window(Screen screen)
        {
            RecoverInstrumentForm(_nozPos1Form, screen);
        }

        public void RecoverNOZ2Window(Screen screen)
        {
            RecoverInstrumentForm(_nozPos2Form, screen);
        }

        public void RecoverOil1Window(Screen screen)
        {
            RecoverInstrumentForm(_oilGauge1Form, screen);
        }

        public void RecoverOil2Window(Screen screen)
        {
            RecoverInstrumentForm(_oilGauge2Form, screen);
        }

        public void RecoverAzimuthIndicatorWindow(Screen screen)
        {
            RecoverInstrumentForm(_rwrForm, screen);
        }

        public void RecoverSpeedbrakeWindow(Screen screen)
        {
            RecoverInstrumentForm(_speedbrakeForm, screen);
        }

        public void RecoverRPM1Window(Screen screen)
        {
            RecoverInstrumentForm(_rpm1Form, screen);
        }

        public void RecoverRPM2Window(Screen screen)
        {
            RecoverInstrumentForm(_rpm2Form, screen);
        }

        public void RecoverVVIWindow(Screen screen)
        {
            RecoverInstrumentForm(_vviForm, screen);
        }

        public void RecoverMfd4Window(Screen screen)
        {
            RecoverInstrumentForm(_mfd4Form, screen);
        }

        public void RecoverMfd3Window(Screen screen)
        {
            RecoverInstrumentForm(_mfd3Form, screen);
        }

        public void RecoverLeftMfdWindow(Screen screen)
        {
            RecoverInstrumentForm(_leftMfdForm, screen);
        }

        public void RecoverRightMfdWindow(Screen screen)
        {
            RecoverInstrumentForm(_rightMfdForm, screen);
        }

        public void RecoverHudWindow(Screen screen)
        {
            RecoverInstrumentForm(_hudForm, screen);
        }

        public void RecoverHydAWindow(Screen screen)
        {
            RecoverInstrumentForm(_hydAForm, screen);
        }

        public void RecoverHydBWindow(Screen screen)
        {
            RecoverInstrumentForm(_hydBForm, screen);
        }

        #endregion

        #region Instrument Form Data-Changed Event Handlers

        private void _speedbrakeForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _speedbrakeForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_speedbrakeForm.DesktopBounds);
            settings.Speedbrake_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_speedbrakeForm.StretchToFill)
            {
                settings.Speedbrake_StretchToFit = true;
            }
            else
            {
                settings.Speedbrake_StretchToFit = false;
                Size size = _speedbrakeForm.Size;
                settings.Speedbrake_OutULX = location.X - screen.Bounds.Location.X;
                settings.Speedbrake_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.Speedbrake_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.Speedbrake_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableSpeedbrakeOutput = _speedbrakeForm.Visible;
            settings.Speedbrake_RotateFlipType = _speedbrakeForm.Rotation;
            settings.Speedbrake_AlwaysOnTop = _speedbrakeForm.AlwaysOnTop;
            settings.Speedbrake_Monochrome = _speedbrakeForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _rwrForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _rwrForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_rwrForm.DesktopBounds);
            settings.RWR_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_rwrForm.StretchToFill)
            {
                settings.RWR_StretchToFit = true;
            }
            else
            {
                settings.RWR_StretchToFit = false;
                Size size = _rwrForm.Size;
                settings.RWR_OutULX = location.X - screen.Bounds.Location.X;
                settings.RWR_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.RWR_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.RWR_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableRWROutput = _rwrForm.Visible;
            settings.RWR_RotateFlipType = _rwrForm.Rotation;
            settings.RWR_AlwaysOnTop = _rwrForm.AlwaysOnTop;
            settings.RWR_Monochrome = _rwrForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _rpm2Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _rpm2Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_rpm2Form.DesktopBounds);
            settings.RPM2_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_rpm2Form.StretchToFill)
            {
                settings.RPM2_StretchToFit = true;
            }
            else
            {
                settings.RPM2_StretchToFit = false;
                Size size = _rpm2Form.Size;
                settings.RPM2_OutULX = location.X - screen.Bounds.Location.X;
                settings.RPM2_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.RPM2_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.RPM2_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableRPM2Output = _rpm2Form.Visible;
            settings.RPM2_RotateFlipType = _rpm2Form.Rotation;
            settings.RPM2_AlwaysOnTop = _rpm2Form.AlwaysOnTop;
            settings.RPM2_Monochrome = _rpm2Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _vviForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _vviForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_vviForm.DesktopBounds);
            settings.VVI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_vviForm.StretchToFill)
            {
                settings.VVI_StretchToFit = true;
            }
            else
            {
                settings.VVI_StretchToFit = false;
                Size size = _vviForm.Size;
                settings.VVI_OutULX = location.X - screen.Bounds.Location.X;
                settings.VVI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.VVI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.VVI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableVVIOutput = _vviForm.Visible;
            settings.VVI_RotateFlipType = _vviForm.Rotation;
            settings.VVI_AlwaysOnTop = _vviForm.AlwaysOnTop;
            settings.VVI_Monochrome = _vviForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _rpm1Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _rpm1Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_rpm1Form.DesktopBounds);
            settings.RPM1_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_rpm1Form.StretchToFill)
            {
                settings.RPM1_StretchToFit = true;
            }
            else
            {
                settings.RPM1_StretchToFit = false;
                Size size = _rpm1Form.Size;
                settings.RPM1_OutULX = location.X - screen.Bounds.Location.X;
                settings.RPM1_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.RPM1_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.RPM1_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableRPM1Output = _rpm1Form.Visible;
            settings.RPM1_RotateFlipType = _rpm1Form.Rotation;
            settings.RPM1_AlwaysOnTop = _rpm1Form.AlwaysOnTop;
            settings.RPM1_Monochrome = _rpm1Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _oilGauge1Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _oilGauge1Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_oilGauge1Form.DesktopBounds);
            settings.OIL1_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_oilGauge1Form.StretchToFill)
            {
                settings.OIL1_StretchToFit = true;
            }
            else
            {
                settings.OIL1_StretchToFit = false;
                Size size = _oilGauge1Form.Size;
                settings.OIL1_OutULX = location.X - screen.Bounds.Location.X;
                settings.OIL1_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.OIL1_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.OIL1_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableOIL1Output = _oilGauge1Form.Visible;
            settings.OIL1_RotateFlipType = _oilGauge1Form.Rotation;
            settings.OIL1_AlwaysOnTop = _oilGauge1Form.AlwaysOnTop;
            settings.OIL1_Monochrome = _oilGauge1Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _oilGauge2Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _oilGauge2Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_oilGauge2Form.DesktopBounds);
            settings.OIL2_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_oilGauge2Form.StretchToFill)
            {
                settings.OIL2_StretchToFit = true;
            }
            else
            {
                settings.OIL2_StretchToFit = false;
                Size size = _oilGauge2Form.Size;
                settings.OIL2_OutULX = location.X - screen.Bounds.Location.X;
                settings.OIL2_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.OIL2_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.OIL2_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableOIL2Output = _oilGauge2Form.Visible;
            settings.OIL2_RotateFlipType = _oilGauge2Form.Rotation;
            settings.OIL2_AlwaysOnTop = _oilGauge2Form.AlwaysOnTop;
            settings.OIL2_Monochrome = _oilGauge2Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _nozPos2Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _nozPos2Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_nozPos2Form.DesktopBounds);
            settings.NOZ2_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_nozPos2Form.StretchToFill)
            {
                settings.NOZ2_StretchToFit = true;
            }
            else
            {
                settings.NOZ2_StretchToFit = false;
                Size size = _nozPos2Form.Size;
                settings.NOZ2_OutULX = location.X - screen.Bounds.Location.X;
                settings.NOZ2_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.NOZ2_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.NOZ2_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableNOZ2Output = _nozPos2Form.Visible;
            settings.NOZ2_RotateFlipType = _nozPos2Form.Rotation;
            settings.NOZ2_AlwaysOnTop = _nozPos2Form.AlwaysOnTop;
            settings.NOZ2_Monochrome = _nozPos2Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _nozPos1Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _nozPos1Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_nozPos1Form.DesktopBounds);
            settings.NOZ1_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_nozPos1Form.StretchToFill)
            {
                settings.NOZ1_StretchToFit = true;
            }
            else
            {
                settings.NOZ1_StretchToFit = false;
                Size size = _nozPos1Form.Size;
                settings.NOZ1_OutULX = location.X - screen.Bounds.Location.X;
                settings.NOZ1_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.NOZ1_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.NOZ1_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableNOZ1Output = _nozPos1Form.Visible;
            settings.NOZ1_RotateFlipType = _nozPos1Form.Rotation;
            settings.NOZ1_AlwaysOnTop = _nozPos1Form.AlwaysOnTop;
            settings.NOZ1_Monochrome = _nozPos1Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _nwsIndexerForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _nwsIndexerForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_nwsIndexerForm.DesktopBounds);
            settings.NWSIndexer_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_nwsIndexerForm.StretchToFill)
            {
                settings.NWSIndexer_StretchToFit = true;
            }
            else
            {
                settings.NWSIndexer_StretchToFit = false;
                Size size = _nwsIndexerForm.Size;
                settings.NWSIndexer_OutULX = location.X - screen.Bounds.Location.X;
                settings.NWSIndexer_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.NWSIndexer_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.NWSIndexer_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableNWSIndexerOutput = _nwsIndexerForm.Visible;
            settings.NWSIndexer_RotateFlipType = _nwsIndexerForm.Rotation;
            settings.NWSIndexer_AlwaysOnTop = _nwsIndexerForm.AlwaysOnTop;
            settings.NWSIndexer_Monochrome = _nwsIndexerForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _fuelQuantityForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _fuelQuantityForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_fuelQuantityForm.DesktopBounds);
            settings.FuelQuantity_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_fuelQuantityForm.StretchToFill)
            {
                settings.FuelQuantity_StretchToFit = true;
            }
            else
            {
                settings.FuelQuantity_StretchToFit = false;
                Size size = _fuelQuantityForm.Size;
                settings.FuelQuantity_OutULX = location.X - screen.Bounds.Location.X;
                settings.FuelQuantity_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.FuelQuantity_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.FuelQuantity_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableFuelQuantityOutput = _fuelQuantityForm.Visible;
            settings.FuelQuantity_RotateFlipType = _fuelQuantityForm.Rotation;
            settings.FuelQuantity_AlwaysOnTop = _fuelQuantityForm.AlwaysOnTop;
            settings.FuelQuantity_Monochrome = _fuelQuantityForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _landingGearLightsForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _landingGearLightsForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_landingGearLightsForm.DesktopBounds);
            settings.GearLights_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_landingGearLightsForm.StretchToFill)
            {
                settings.GearLights_StretchToFit = true;
            }
            else
            {
                settings.GearLights_StretchToFit = false;
                Size size = _landingGearLightsForm.Size;
                settings.GearLights_OutULX = location.X - screen.Bounds.Location.X;
                settings.GearLights_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.GearLights_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.GearLights_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableGearLightsOutput = _landingGearLightsForm.Visible;
            settings.GearLights_RotateFlipType = _landingGearLightsForm.Rotation;
            settings.GearLights_AlwaysOnTop = _landingGearLightsForm.AlwaysOnTop;
            settings.GearLights_Monochrome = _landingGearLightsForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _hsiForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _hsiForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_hsiForm.DesktopBounds);
            settings.HSI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_hsiForm.StretchToFill)
            {
                settings.HSI_StretchToFit = true;
            }
            else
            {
                settings.HSI_StretchToFit = false;
                Size size = _hsiForm.Size;
                settings.HSI_OutULX = location.X - screen.Bounds.Location.X;
                settings.HSI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.HSI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.HSI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableHSIOutput = _hsiForm.Visible;
            settings.HSI_RotateFlipType = _hsiForm.Rotation;
            settings.HSI_AlwaysOnTop = _hsiForm.AlwaysOnTop;
            settings.HSI_Monochrome = _hsiForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _ehsiForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _ehsiForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_ehsiForm.DesktopBounds);
            settings.EHSI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_ehsiForm.StretchToFill)
            {
                settings.EHSI_StretchToFit = true;
            }
            else
            {
                settings.EHSI_StretchToFit = false;
                Size size = _ehsiForm.Size;
                settings.EHSI_OutULX = location.X - screen.Bounds.Location.X;
                settings.EHSI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.EHSI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.EHSI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableEHSIOutput = _ehsiForm.Visible;
            settings.EHSI_RotateFlipType = _ehsiForm.Rotation;
            settings.EHSI_AlwaysOnTop = _ehsiForm.AlwaysOnTop;
            settings.EHSI_Monochrome = _ehsiForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _fuelFlowForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _fuelFlowForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_fuelFlowForm.DesktopBounds);
            settings.FuelFlow_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_fuelFlowForm.StretchToFill)
            {
                settings.FuelFlow_StretchToFit = true;
            }
            else
            {
                settings.FuelFlow_StretchToFit = false;
                Size size = _fuelFlowForm.Size;
                settings.FuelFlow_OutULX = location.X - screen.Bounds.Location.X;
                settings.FuelFlow_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.FuelFlow_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.FuelFlow_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableFuelFlowOutput = _fuelFlowForm.Visible;
            settings.FuelFlow_RotateFlipType = _fuelFlowForm.Rotation;
            settings.FuelFlow_AlwaysOnTop = _fuelFlowForm.AlwaysOnTop;
            settings.FuelFlow_Monochrome = _fuelFlowForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _isisForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _isisForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_isisForm.DesktopBounds);
            settings.ISIS_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_isisForm.StretchToFill)
            {
                settings.ISIS_StretchToFit = true;
            }
            else
            {
                settings.ISIS_StretchToFit = false;
                Size size = _isisForm.Size;
                settings.ISIS_OutULX = location.X - screen.Bounds.Location.X;
                settings.ISIS_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.ISIS_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.ISIS_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableISISOutput = _isisForm.Visible;
            settings.ISIS_RotateFlipType = _isisForm.Rotation;
            settings.ISIS_AlwaysOnTop = _isisForm.AlwaysOnTop;
            settings.ISIS_Monochrome = _isisForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _accelerometerForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _accelerometerForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_accelerometerForm.DesktopBounds);
            settings.Accelerometer_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_accelerometerForm.StretchToFill)
            {
                settings.Accelerometer_StretchToFit = true;
            }
            else
            {
                settings.Accelerometer_StretchToFit = false;
                Size size = _accelerometerForm.Size;
                settings.Accelerometer_OutULX = location.X - screen.Bounds.Location.X;
                settings.Accelerometer_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.Accelerometer_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.Accelerometer_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableAccelerometerOutput = _accelerometerForm.Visible;
            settings.Accelerometer_RotateFlipType = _accelerometerForm.Rotation;
            settings.Accelerometer_AlwaysOnTop = _accelerometerForm.AlwaysOnTop;
            settings.Accelerometer_Monochrome = _accelerometerForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _ftit2Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _ftit2Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_ftit2Form.DesktopBounds);
            settings.FTIT2_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_ftit2Form.StretchToFill)
            {
                settings.FTIT2_StretchToFit = true;
            }
            else
            {
                settings.FTIT2_StretchToFit = false;
                Size size = _ftit2Form.Size;
                settings.FTIT2_OutULX = location.X - screen.Bounds.Location.X;
                settings.FTIT2_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.FTIT2_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.FTIT2_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableFTIT2Output = _ftit2Form.Visible;
            settings.FTIT2_RotateFlipType = _ftit2Form.Rotation;
            settings.FTIT2_AlwaysOnTop = _ftit2Form.AlwaysOnTop;
            settings.FTIT2_Monochrome = _ftit2Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _ftit1Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _ftit1Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_ftit1Form.DesktopBounds);
            settings.FTIT1_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_ftit1Form.StretchToFill)
            {
                settings.FTIT1_StretchToFit = true;
            }
            else
            {
                settings.FTIT1_StretchToFit = false;
                Size size = _ftit1Form.Size;
                settings.FTIT1_OutULX = location.X - screen.Bounds.Location.X;
                settings.FTIT1_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.FTIT1_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.FTIT1_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableFTIT1Output = _ftit1Form.Visible;
            settings.FTIT1_RotateFlipType = _ftit1Form.Rotation;
            settings.FTIT1_AlwaysOnTop = _ftit1Form.AlwaysOnTop;
            settings.FTIT1_Monochrome = _ftit1Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _epuFuelForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _epuFuelForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_epuFuelForm.DesktopBounds);
            settings.EPUFuel_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_epuFuelForm.StretchToFill)
            {
                settings.EPUFuel_StretchToFit = true;
            }
            else
            {
                settings.EPUFuel_StretchToFit = false;
                Size size = _epuFuelForm.Size;
                settings.EPUFuel_OutULX = location.X - screen.Bounds.Location.X;
                settings.EPUFuel_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.EPUFuel_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.EPUFuel_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableEPUFuelOutput = _epuFuelForm.Visible;
            settings.EPUFuel_RotateFlipType = _epuFuelForm.Rotation;
            settings.EPUFuel_AlwaysOnTop = _epuFuelForm.AlwaysOnTop;
            settings.EPUFuel_Monochrome = _epuFuelForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _pflForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _pflForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_pflForm.DesktopBounds);
            settings.PFL_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_pflForm.StretchToFill)
            {
                settings.PFL_StretchToFit = true;
            }
            else
            {
                settings.PFL_StretchToFit = false;
                Size size = _pflForm.Size;
                settings.PFL_OutULX = location.X - screen.Bounds.Location.X;
                settings.PFL_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.PFL_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.PFL_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnablePFLOutput = _pflForm.Visible;
            settings.PFL_RotateFlipType = _pflForm.Rotation;
            settings.PFL_AlwaysOnTop = _pflForm.AlwaysOnTop;
            settings.PFL_Monochrome = _pflForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _dedForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _dedForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_dedForm.DesktopBounds);
            settings.DED_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_dedForm.StretchToFill)
            {
                settings.DED_StretchToFit = true;
            }
            else
            {
                settings.DED_StretchToFit = false;
                Size size = _dedForm.Size;
                settings.DED_OutULX = location.X - screen.Bounds.Location.X;
                settings.DED_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.DED_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.DED_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableDEDOutput = _dedForm.Visible;
            settings.DED_RotateFlipType = _dedForm.Rotation;
            settings.DED_AlwaysOnTop = _dedForm.AlwaysOnTop;
            settings.DED_Monochrome = _dedForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _compassForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _compassForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_compassForm.DesktopBounds);
            settings.Compass_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_compassForm.StretchToFill)
            {
                settings.Compass_StretchToFit = true;
            }
            else
            {
                settings.Compass_StretchToFit = false;
                Size size = _compassForm.Size;
                settings.Compass_OutULX = location.X - screen.Bounds.Location.X;
                settings.Compass_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.Compass_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.Compass_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableCompassOutput = _compassForm.Visible;
            settings.Compass_RotateFlipType = _compassForm.Rotation;
            settings.Compass_AlwaysOnTop = _compassForm.AlwaysOnTop;
            settings.Compass_Monochrome = _compassForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _cmdsPanelForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _cmdsPanelForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_cmdsPanelForm.DesktopBounds);
            settings.CMDS_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_cmdsPanelForm.StretchToFill)
            {
                settings.CMDS_StretchToFit = true;
            }
            else
            {
                settings.CMDS_StretchToFit = false;
                Size size = _cmdsPanelForm.Size;
                settings.CMDS_OutULX = location.X - screen.Bounds.Location.X;
                settings.CMDS_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.CMDS_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.CMDS_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableCMDSOutput = _cmdsPanelForm.Visible;
            settings.CMDS_RotateFlipType = _cmdsPanelForm.Rotation;
            settings.CMDS_AlwaysOnTop = _cmdsPanelForm.AlwaysOnTop;
            settings.CMDS_Monochrome = _cmdsPanelForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _cautionPanelForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _cautionPanelForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_cautionPanelForm.DesktopBounds);
            settings.CautionPanel_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_cautionPanelForm.StretchToFill)
            {
                settings.CautionPanel_StretchToFit = true;
            }
            else
            {
                settings.CautionPanel_StretchToFit = false;
                Size size = _cautionPanelForm.Size;
                settings.CautionPanel_OutULX = location.X - screen.Bounds.Location.X;
                settings.CautionPanel_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.CautionPanel_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.CautionPanel_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableCautionPanelOutput = _cautionPanelForm.Visible;
            settings.CautionPanel_RotateFlipType = _cautionPanelForm.Rotation;
            settings.CautionPanel_AlwaysOnTop = _cautionPanelForm.AlwaysOnTop;
            settings.CautionPanel_Monochrome = _cautionPanelForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _aoaIndicatorForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _aoaIndicatorForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_aoaIndicatorForm.DesktopBounds);
            settings.AOAIndicator_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_aoaIndicatorForm.StretchToFill)
            {
                settings.AOAIndicator_StretchToFit = true;
            }
            else
            {
                settings.AOAIndicator_StretchToFit = false;
                Size size = _aoaIndicatorForm.Size;
                settings.AOAIndicator_OutULX = location.X - screen.Bounds.Location.X;
                settings.AOAIndicator_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.AOAIndicator_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.AOAIndicator_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableAOAIndicatorOutput = _aoaIndicatorForm.Visible;
            settings.AOAIndicator_RotateFlipType = _aoaIndicatorForm.Rotation;
            settings.AOAIndicator_AlwaysOnTop = _aoaIndicatorForm.AlwaysOnTop;
            settings.AOAIndicator_Monochrome = _aoaIndicatorForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _aoaIndexerForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _aoaIndexerForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_aoaIndexerForm.DesktopBounds);
            settings.AOAIndexer_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_aoaIndexerForm.StretchToFill)
            {
                settings.AOAIndexer_StretchToFit = true;
            }
            else
            {
                settings.AOAIndexer_StretchToFit = false;
                Size size = _aoaIndexerForm.Size;
                settings.AOAIndexer_OutULX = location.X - screen.Bounds.Location.X;
                settings.AOAIndexer_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.AOAIndexer_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.AOAIndexer_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableAOAIndexerOutput = _aoaIndexerForm.Visible;
            settings.AOAIndexer_RotateFlipType = _aoaIndexerForm.Rotation;
            settings.AOAIndexer_AlwaysOnTop = _aoaIndexerForm.AlwaysOnTop;
            settings.AOAIndexer_Monochrome = _aoaIndexerForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _altimeterForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _altimeterForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_altimeterForm.DesktopBounds);
            settings.Altimeter_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_altimeterForm.StretchToFill)
            {
                settings.Altimeter_StretchToFit = true;
            }
            else
            {
                settings.Altimeter_StretchToFit = false;
                Size size = _altimeterForm.Size;
                settings.Altimeter_OutULX = location.X - screen.Bounds.Location.X;
                settings.Altimeter_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.Altimeter_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.Altimeter_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableAltimeterOutput = _altimeterForm.Visible;
            settings.Altimeter_RotateFlipType = _altimeterForm.Rotation;
            settings.Altimeter_AlwaysOnTop = _altimeterForm.AlwaysOnTop;
            settings.Altimeter_Monochrome = _altimeterForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _asiForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _asiForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_asiForm.DesktopBounds);
            settings.ASI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_asiForm.StretchToFill)
            {
                settings.ASI_StretchToFit = true;
            }
            else
            {
                settings.ASI_StretchToFit = false;
                Size size = _asiForm.Size;
                settings.ASI_OutULX = location.X - screen.Bounds.Location.X;
                settings.ASI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.ASI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.ASI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableASIOutput = _asiForm.Visible;
            settings.ASI_RotateFlipType = _asiForm.Rotation;
            settings.ASI_AlwaysOnTop = _asiForm.AlwaysOnTop;
            settings.ASI_Monochrome = _asiForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _adiForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _adiForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_adiForm.DesktopBounds);
            settings.ADI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_adiForm.StretchToFill)
            {
                settings.ADI_StretchToFit = true;
            }
            else
            {
                settings.ADI_StretchToFit = false;
                Size size = _adiForm.Size;
                settings.ADI_OutULX = location.X - screen.Bounds.Location.X;
                settings.ADI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.ADI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.ADI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableADIOutput = _adiForm.Visible;
            settings.ADI_RotateFlipType = _adiForm.Rotation;
            settings.ADI_AlwaysOnTop = _adiForm.AlwaysOnTop;
            settings.ADI_Monochrome = _adiForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _backupAdiForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _backupAdiForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_backupAdiForm.DesktopBounds);
            settings.Backup_ADI_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_backupAdiForm.StretchToFill)
            {
                settings.Backup_ADI_StretchToFit = true;
            }
            else
            {
                settings.Backup_ADI_StretchToFit = false;
                Size size = _backupAdiForm.Size;
                settings.Backup_ADI_OutULX = location.X - screen.Bounds.Location.X;
                settings.Backup_ADI_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.Backup_ADI_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.Backup_ADI_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableBackupADIOutput = _backupAdiForm.Visible;
            settings.Backup_ADI_RotateFlipType = _backupAdiForm.Rotation;
            settings.Backup_ADI_AlwaysOnTop = _backupAdiForm.AlwaysOnTop;
            settings.Backup_ADI_Monochrome = _backupAdiForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _hydAForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _hydAForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_hydAForm.DesktopBounds);
            settings.HYDA_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_hydAForm.StretchToFill)
            {
                settings.HYDA_StretchToFit = true;
            }
            else
            {
                settings.HYDA_StretchToFit = false;
                Size size = _hydAForm.Size;
                settings.HYDA_OutULX = location.X - screen.Bounds.Location.X;
                settings.HYDA_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.HYDA_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.HYDA_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableHYDAOutput = _hydAForm.Visible;
            settings.HYDA_RotateFlipType = _hydAForm.Rotation;
            settings.HYDA_AlwaysOnTop = _hydAForm.AlwaysOnTop;
            settings.HYDA_Monochrome = _hydAForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _hydBForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _hydBForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_hydBForm.DesktopBounds);
            settings.HYDB_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_hydBForm.StretchToFill)
            {
                settings.HYDB_StretchToFit = true;
            }
            else
            {
                settings.HYDB_StretchToFit = false;
                Size size = _hydBForm.Size;
                settings.HYDB_OutULX = location.X - screen.Bounds.Location.X;
                settings.HYDB_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.HYDB_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.HYDB_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableHYDBOutput = _hydBForm.Visible;
            settings.HYDB_RotateFlipType = _hydBForm.Rotation;
            settings.HYDB_AlwaysOnTop = _hydBForm.AlwaysOnTop;
            settings.HYDB_Monochrome = _hydBForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _cabinPressForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _cabinPressForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_cabinPressForm.DesktopBounds);
            settings.CabinPress_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_cabinPressForm.StretchToFill)
            {
                settings.CabinPress_StretchToFit = true;
            }
            else
            {
                settings.CabinPress_StretchToFit = false;
                Size size = _cabinPressForm.Size;
                settings.CabinPress_OutULX = location.X - screen.Bounds.Location.X;
                settings.CabinPress_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.CabinPress_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.CabinPress_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableCabinPressOutput = _cabinPressForm.Visible;
            settings.CabinPress_RotateFlipType = _cabinPressForm.Rotation;
            settings.CabinPress_AlwaysOnTop = _cabinPressForm.AlwaysOnTop;
            settings.CabinPress_Monochrome = _cabinPressForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }


        private void _rollTrimForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _rollTrimForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_rollTrimForm.DesktopBounds);
            settings.RollTrim_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_rollTrimForm.StretchToFill)
            {
                settings.RollTrim_StretchToFit = true;
            }
            else
            {
                settings.RollTrim_StretchToFit = false;
                Size size = _rollTrimForm.Size;
                settings.RollTrim_OutULX = location.X - screen.Bounds.Location.X;
                settings.RollTrim_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.RollTrim_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.RollTrim_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableRollTrimOutput = _rollTrimForm.Visible;
            settings.RollTrim_RotateFlipType = _rollTrimForm.Rotation;
            settings.RollTrim_AlwaysOnTop = _rollTrimForm.AlwaysOnTop;
            settings.RollTrim_Monochrome = _rollTrimForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _pitchTrimForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _pitchTrimForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_pitchTrimForm.DesktopBounds);
            settings.PitchTrim_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_pitchTrimForm.StretchToFill)
            {
                settings.PitchTrim_StretchToFit = true;
            }
            else
            {
                settings.PitchTrim_StretchToFit = false;
                Size size = _pitchTrimForm.Size;
                settings.PitchTrim_OutULX = location.X - screen.Bounds.Location.X;
                settings.PitchTrim_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.PitchTrim_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.PitchTrim_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnablePitchTrimOutput = _pitchTrimForm.Visible;
            settings.PitchTrim_RotateFlipType = _pitchTrimForm.Rotation;
            settings.PitchTrim_AlwaysOnTop = _pitchTrimForm.AlwaysOnTop;
            settings.PitchTrim_Monochrome = _pitchTrimForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _mfd4Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _mfd4Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_mfd4Form.DesktopBounds);
            settings.MFD4_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_mfd4Form.StretchToFill)
            {
                settings.MFD4_StretchToFit = true;
            }
            else
            {
                settings.MFD4_StretchToFit = false;
                Size size = _mfd4Form.Size;
                settings.MFD4_OutULX = location.X - screen.Bounds.Location.X;
                settings.MFD4_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.MFD4_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.MFD4_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableMfd4Output = _mfd4Form.Visible;
            settings.MFD4_RotateFlipType = _mfd4Form.Rotation;
            settings.MFD4_AlwaysOnTop = _mfd4Form.AlwaysOnTop;
            settings.MFD4_Monochrome = _mfd4Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _mfd3Form_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _mfd3Form.DesktopLocation;
            Screen screen = Screen.FromRectangle(_mfd3Form.DesktopBounds);
            settings.MFD3_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_mfd3Form.StretchToFill)
            {
                settings.MFD3_StretchToFit = true;
            }
            else
            {
                settings.MFD3_StretchToFit = false;
                Size size = _mfd3Form.Size;
                settings.MFD3_OutULX = location.X - screen.Bounds.Location.X;
                settings.MFD3_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.MFD3_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.MFD3_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableMfd3Output = _mfd3Form.Visible;
            settings.MFD3_RotateFlipType = _mfd3Form.Rotation;
            settings.MFD3_AlwaysOnTop = _mfd3Form.AlwaysOnTop;
            settings.MFD3_Monochrome = _mfd3Form.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _leftMfdForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _leftMfdForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_leftMfdForm.DesktopBounds);
            settings.LMFD_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_leftMfdForm.StretchToFill)
            {
                settings.LMFD_StretchToFit = true;
            }
            else
            {
                settings.LMFD_StretchToFit = false;
                Size size = _leftMfdForm.Size;
                settings.LMFD_OutULX = location.X - screen.Bounds.Location.X;
                settings.LMFD_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.LMFD_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.LMFD_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableLeftMFDOutput = _leftMfdForm.Visible;
            settings.LMFD_RotateFlipType = _leftMfdForm.Rotation;
            settings.LMFD_AlwaysOnTop = _leftMfdForm.AlwaysOnTop;
            settings.LMFD_Monochrome = _leftMfdForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _rightMfdForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _rightMfdForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_rightMfdForm.DesktopBounds);
            Size size = _rightMfdForm.Size;
            settings.RMFD_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_rightMfdForm.StretchToFill)
            {
                settings.RMFD_StretchToFit = true;
            }
            else
            {
                settings.RMFD_StretchToFit = false;
                settings.RMFD_OutULX = location.X - screen.Bounds.Location.X;
                settings.RMFD_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.RMFD_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.RMFD_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableRightMFDOutput = _rightMfdForm.Visible;
            settings.RMFD_RotateFlipType = _rightMfdForm.Rotation;
            settings.RMFD_AlwaysOnTop = _rightMfdForm.AlwaysOnTop;
            settings.RMFD_Monochrome = _rightMfdForm.Monochrome;
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        private void _hudForm_DataChanged(object sender, EventArgs e)
        {
            Settings settings = Settings.Default;
            Point location = _hudForm.DesktopLocation;
            Screen screen = Screen.FromRectangle(_hudForm.DesktopBounds);
            Size size = _hudForm.Size;
            settings.HUD_OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_hudForm.StretchToFill)
            {
                settings.HUD_StretchToFit = true;
            }
            else
            {
                settings.HUD_StretchToFit = false;
                settings.HUD_OutULX = location.X - screen.Bounds.Location.X;
                settings.HUD_OutULY = location.Y - screen.Bounds.Location.Y;
                settings.HUD_OutLRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.HUD_OutLRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
            settings.EnableHudOutput = _hudForm.Visible;
            settings.HUD_RotateFlipType = _hudForm.Rotation;
            settings.HudAlwaysOnTop = _hudForm.AlwaysOnTop;
            settings.HUD_Monochrome = _hudForm.Monochrome;

            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
        }

        #endregion

        #region Form and Renderer Matching

        private InstrumentForm GetFormForRenderer(IInstrumentRenderer renderer)
        {
            if (renderer == null) return null;
            if (_outputForms.ContainsKey(renderer))
            {
                return _outputForms[renderer];
            }
            return null;
        }

        #endregion

        #region Form Teardown

        private void CloseOutputWindowForm(Form form)
        {
            if (form != null)
            {
                try
                {
                    form.Close();
                }
                catch (InvalidOperationException e)
                {
                    _log.Error(e.Message, e);
                }
            }
        }

        /// <summary>
        ///     Close all the output window forms
        /// </summary>
        private void CloseOutputWindowForms()
        {
            CloseOutputWindowForm(_adiForm);
            CloseOutputWindowForm(_backupAdiForm);
            CloseOutputWindowForm(_asiForm);
            CloseOutputWindowForm(_altimeterForm);
            CloseOutputWindowForm(_aoaIndexerForm);
            CloseOutputWindowForm(_aoaIndicatorForm);
            CloseOutputWindowForm(_cautionPanelForm);
            CloseOutputWindowForm(_cmdsPanelForm);
            CloseOutputWindowForm(_compassForm);
            CloseOutputWindowForm(_dedForm);
            CloseOutputWindowForm(_pflForm);
            CloseOutputWindowForm(_epuFuelForm);
            CloseOutputWindowForm(_accelerometerForm);
            CloseOutputWindowForm(_ftit1Form);
            CloseOutputWindowForm(_ftit2Form);
            CloseOutputWindowForm(_fuelFlowForm);
            CloseOutputWindowForm(_isisForm);
            CloseOutputWindowForm(_fuelQuantityForm);
            CloseOutputWindowForm(_hsiForm);
            CloseOutputWindowForm(_ehsiForm);
            CloseOutputWindowForm(_landingGearLightsForm);
            CloseOutputWindowForm(_nwsIndexerForm);
            CloseOutputWindowForm(_nozPos1Form);
            CloseOutputWindowForm(_nozPos2Form);
            CloseOutputWindowForm(_oilGauge1Form);
            CloseOutputWindowForm(_oilGauge2Form);
            CloseOutputWindowForm(_rwrForm);
            CloseOutputWindowForm(_speedbrakeForm);
            CloseOutputWindowForm(_rpm1Form);
            CloseOutputWindowForm(_rpm2Form);
            CloseOutputWindowForm(_vviForm);
            CloseOutputWindowForm(_hydAForm);
            CloseOutputWindowForm(_hydBForm);
            CloseOutputWindowForm(_cabinPressForm);
            CloseOutputWindowForm(_rollTrimForm);
            CloseOutputWindowForm(_pitchTrimForm);


            CloseOutputWindowForm(_mfd4Form);
            CloseOutputWindowForm(_mfd3Form);
            CloseOutputWindowForm(_leftMfdForm);
            CloseOutputWindowForm(_rightMfdForm);
            CloseOutputWindowForm(_hudForm);
        }

        #endregion

        #endregion

        #region Thread Management

        /// <summary>
        ///     Sets up all worker threads and output forms and starts the worker threads running
        /// </summary>
        private void RunThreads()
        {
            if (!_running)
            {
                _running = true;
                SetupNetworking();
                _keepRunning = true;
                SetupInstrumentRenderers();
                SetupOutputForms();
                SetupThreads();
                StartThreads();

                DateTime startingEventStartTime = DateTime.Now;
                _log.DebugFormat("About to invoke the Starting event at {0}", startingEventStartTime.ToString());
                if (Started != null)
                {
                    Started.Invoke(this, new EventArgs());
                }
                DateTime startingEventFinishTime = DateTime.Now;
                _log.DebugFormat("Finished invoking the Starting event at {0}", startingEventFinishTime.ToString());
                TimeSpan startingEventTimeTaken = startingEventFinishTime.Subtract(startingEventStartTime);
                _log.DebugFormat("Time taken to invoke the Starting event: {0}",
                                 startingEventTimeTaken.TotalMilliseconds);
            }
        }

        private void StartThreads()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting threads on the extractor at: {0}", startTime.ToString());

            StartThread(_mfd4CaptureThread);
            StartThread(_mfd3CaptureThread);
            StartThread(_leftMfdCaptureThread);
            StartThread(_rightMfdCaptureThread);
            StartThread(_hudCaptureThread);
            StartThread(_rwrRenderThread);
            StartThread(_adiRenderThread);
            StartThread(_backupAdiRenderThread);
            StartThread(_asiRenderThread);
            StartThread(_altimeterRenderThread);
            StartThread(_aoaIndexerRenderThread);
            StartThread(_aoaIndicatorRenderThread);
            StartThread(_cautionPanelRenderThread);
            StartThread(_cmdsPanelRenderThread);
            StartThread(_compassRenderThread);
            StartThread(_dedRenderThread);
            StartThread(_pflRenderThread);
            StartThread(_epuFuelRenderThread);
            StartThread(_accelerometerRenderThread);
            StartThread(_ftit1RenderThread);
            StartThread(_ftit2RenderThread);
            StartThread(_fuelFlowRenderThread);
            StartThread(_isisRenderThread);
            StartThread(_fuelQuantityRenderThread);
            StartThread(_hsiRenderThread);
            StartThread(_ehsiRenderThread);
            StartThread(_landingGearLightsRenderThread);
            StartThread(_nwsIndexerRenderThread);
            StartThread(_nozPos1RenderThread);
            StartThread(_nozPos2RenderThread);
            StartThread(_oilGauge1RenderThread);
            StartThread(_oilGauge2RenderThread);
            StartThread(_speedbrakeRenderThread);
            StartThread(_rpm1RenderThread);
            StartThread(_rpm2RenderThread);
            StartThread(_vviRenderThread);
            StartThread(_hydARenderThread);
            StartThread(_hydBRenderThread);
            StartThread(_cabinPressRenderThread);
            StartThread(_rollTrimRenderThread);
            StartThread(_pitchTrimRenderThread);

            StartThread(_simStatusMonitorThread);
            StartThread(_captureOrchestrationThread);
            StartThread(_keyboardWatcherThread);

            DateTime finishedTime = DateTime.Now;
            TimeSpan elapsed = finishedTime.Subtract(startTime);

            _log.DebugFormat("Took {0} milliseconds for all threads to start.", elapsed.TotalMilliseconds);
            _log.DebugFormat("Finished starting threads on the extractor at: {0}", finishedTime.ToString());
        }

        private void StartThread(Thread t)
        {
            if (t == null) return;
            try
            {
                t.Start();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void KeyboardWatcherThreadWork()
        {
            AutoResetEvent resetEvent = null;
            Device device = null;
            try
            {
                resetEvent = new AutoResetEvent(false);
                device = new Device(SystemGuid.Keyboard);
                device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                device.SetEventNotification(resetEvent);
                device.Properties.BufferSize = 255;
                device.Acquire();
                var lastKeyboardState = new bool[Enum.GetValues(typeof (Key)).Length];
                var currentKeyboardState = new bool[Enum.GetValues(typeof (Key)).Length];
                while (_keepRunning)
                {
                    try
                    {
                        resetEvent.WaitOne(50);
                    }
                    catch (TimeoutException)
                    {
                    }
                    try
                    {
                        KeyboardState curState = device.GetCurrentKeyboardState();
                        Array possibleKeys = Enum.GetValues(typeof (Key));

                        int i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            currentKeyboardState[i] = curState[thisKey];
                            i++;
                        }

                        i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            bool isPressedNow = currentKeyboardState[i];
                            bool wasPressedBefore = lastKeyboardState[i];
                            var winFormsKey =
                                (Keys) NativeMethods.MapVirtualKey((uint) thisKey, NativeMethods.MAPVK_VSC_TO_VK_EX);
                            if (isPressedNow && !wasPressedBefore)
                            {
                                ProcessKeyDownEvent(new KeyEventArgs(winFormsKey));
                            }
                            else if (wasPressedBefore && !isPressedNow)
                            {
                                ProcessKeyUpEvent(new KeyEventArgs(winFormsKey));
                            }
                            i++;
                        }
                        Array.Copy(currentKeyboardState, lastKeyboardState, currentKeyboardState.Length);
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            finally
            {
                if (device != null)
                {
                    device.Unacquire();
                }
                Common.Util.DisposeObject(device);
                device = null;
            }
        }

        private void CaptureOrchestrationThreadWork()
        {
            var toWait = new List<WaitHandle>();
            try
            {
                while (_keepRunning)
                {
                    _windowSizingOrMoving = WindowSizingOrMovingBeingAttemptedOnAnyOutputWindow();
                    Application.DoEvents();
                    if (_settingsSaveScheduled && !_windowSizingOrMoving)
                    {
                        SaveSettingsAsync();
                    }
                    if (_settingsLoadScheduled && !_windowSizingOrMoving)
                    {
                        LoadSettingsAsync();
                    }
                    if (_renderCycleNum < long.MaxValue)
                    {
                        _renderCycleNum++;
                    }
                    else
                    {
                        _renderCycleNum = 0;
                    }
                    DateTime thisLoopStartTime = DateTime.Now;
                    bool setNullImages = true;

                    if (NetworkMode == NetworkMode.Client)
                    {
                        ProcessPendingMessagesToClientFromServer();
                    }
                    else if (NetworkMode == NetworkMode.Server)
                    {
                        ProcessPendingMessagesToServerFromClient();
                    }


                    if (_simRunning || _testMode || NetworkMode == NetworkMode.Client)
                    {
                        FlightData current = GetFlightData();
                        SetFlightData(current);
                        UpdateRendererStatesFromFlightData();
                        try
                        {
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                        catch (ThreadAbortException)
                        {
                        }
                        catch (Exception e)
                        {
                            _log.Error(e.Message, e);
                        }
                    }
                    else
                    {
                        var toSet = new FlightData();
                        toSet.hsiBits = Int32.MaxValue;
                        SetFlightData(toSet);
                        UpdateRendererStatesFromFlightData();
                        SetMfd4Image(Util.CloneBitmap(_mfd4BlankImage));
                        SetMfd3Image(Util.CloneBitmap(_mfd3BlankImage));
                        SetLeftMfdImage(Util.CloneBitmap(_leftMfdBlankImage));
                        SetRightMfdImage(Util.CloneBitmap(_rightMfdBlankImage));
                        SetHudImage(Util.CloneBitmap(_hudBlankImage));
                        setNullImages = false;
                    }

                    try
                    {
                        toWait = new List<WaitHandle>();
                        SignalMFDAndHudThreadsToStart();
                        //performance group 0
                        SignalRwrRenderThreadToStart(toWait);
                        SignalPrimaryFlightInstrumentRenderThreadsToStart(toWait);
                        //                            WaitAllAndClearList(toWait, 1000);
                        SignalFuelInstrumentsRenderThreadsToStart(toWait);
                        SignalIndexerRenderThreadsToStart(toWait);
                        SignalEngine1GaugesRenderThreadsToStart(toWait);
                        SignalEngine2GaugesRenderThreadsToStart(toWait);
                        SignalRightAuxEmulatedGaugesRenderThreadsToStart(toWait);
                        SignalTrimIndicatorRenderThreadsToStart(toWait);
                        SignalDEDPFLRenderThreadsToStart(toWait);
                        SignalCautionPanelRenderThreadToStart(toWait);
                        SignalCMDSRenderThreadToStart(toWait);
                        SignalGearPanelAccessoryRenderThreadsToStart(toWait);
                        WaitAllAndClearList(toWait, 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }

                    DateTime thisLoopFinishTime = DateTime.Now;
                    TimeSpan timeElapsed = thisLoopFinishTime.Subtract(thisLoopStartTime);
                    int millisToSleep = Settings.Default.PollingDelay - ((int) timeElapsed.TotalMilliseconds);
                    if (_testMode) millisToSleep = 500;
                    DateTime sleepUntil = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, millisToSleep));
                    while (DateTime.Now < sleepUntil)
                    {
                        var millisRemaining = (int) Math.Floor(DateTime.Now.Subtract(sleepUntil).TotalMilliseconds);
                        int millisWaited = millisRemaining >= 5 ? 5 : 1;
                        Thread.Sleep(millisWaited);
                        Application.DoEvents();
                    }
                    Application.DoEvents();
                    if ((!_simRunning && !(_networkMode == NetworkMode.Client)) && !_testMode)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5);
                            //sleep an additional half-second or so here if we're not a client and there's no sim running and we're not in test mode
                    }
                    else if (_testMode)
                    {
                        Application.DoEvents();
                        Thread.Sleep(50);
                    }
                }
                Debug.WriteLine("CaptureThreadWork has exited.");
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
            finally
            {
                _windowSizingOrMoving = false;
            }
        }

        private void SignalRwrRenderThreadToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }

            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableRWROutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rwrRenderer)) ||
                    (_rwrForm != null && _rwrForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RWR_RenderEveryN == Settings.Default.RWR_RenderOnN - 1) ||
                        (_rwrForm != null && _rwrForm.RenderImmediately))
                    {
                        if (_rwrForm != null)
                        {
                            _rwrForm.RenderImmediately = false;
                        }
                        if (_rwrRenderStart != null)
                        {
                            _rwrRenderStart.Set();
                        }
                        if (_rwrRenderEnd != null)
                        {
                            toWait.Add(_rwrRenderEnd);
                        }
                    }
                }
            }
        }

        private static void WaitAllAndClearList(List<WaitHandle> toWait, int millisecondsTimeout)
        {
            if (toWait != null && toWait.Count > 0)
            {
                try
                {
                    WaitHandle[] handles = toWait.ToArray();
                    if (handles != null && handles.Length > 0)
                    {
                        WaitHandle.WaitAll(handles, millisecondsTimeout);
                    }
                }
                catch (TimeoutException)
                {
                }
                catch (DuplicateWaitObjectException) //this can happen somehow if our list is not cleared 
                {
                }
            }
            toWait.Clear();
        }

        private void SignalCMDSRenderThreadToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableCMDSOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cmdsPanelRenderer)) ||
                    (_cmdsPanelForm != null && _cmdsPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CMDS_RenderEveryN == Settings.Default.CMDS_RenderOnN - 1) ||
                        (_cmdsPanelForm != null && _cmdsPanelForm.RenderImmediately))
                    {
                        if (_cmdsPanelForm != null)
                        {
                            _cmdsPanelForm.RenderImmediately = false;
                        }
                        if (_cmdsPanelRenderStart != null)
                        {
                            _cmdsPanelRenderStart.Set();
                        }
                        if (_cmdsPanelRenderEnd != null)
                        {
                            toWait.Add(_cmdsPanelRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalCautionPanelRenderThreadToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableCautionPanelOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cautionPanelRenderer)) ||
                    (_cautionPanelForm != null && _cautionPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CautionPanel_RenderEveryN ==
                         Settings.Default.CautionPanel_RenderOnN - 1) ||
                        (_cautionPanelForm != null && _cautionPanelForm.RenderImmediately))
                    {
                        if (_cautionPanelForm != null)
                        {
                            _cautionPanelForm.RenderImmediately = false;
                        }
                        if (_cautionPanelRenderStart != null)
                        {
                            _cautionPanelRenderStart.Set();
                        }
                        if (_cautionPanelRenderEnd != null)
                        {
                            toWait.Add(_cautionPanelRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalGearPanelAccessoryRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableGearLightsOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_landingGearLightsRenderer)) ||
                    (_landingGearLightsForm != null && _landingGearLightsForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.GearLights_RenderEveryN ==
                         Settings.Default.GearLights_RenderOnN - 1) ||
                        (_landingGearLightsForm != null && _landingGearLightsForm.RenderImmediately))
                    {
                        if (_landingGearLightsForm != null)
                        {
                            _landingGearLightsForm.RenderImmediately = false;
                        }
                        if (_landingGearLightsRenderStart != null)
                        {
                            _landingGearLightsRenderStart.Set();
                        }
                        if (_landingGearLightsRenderEnd != null)
                        {
                            toWait.Add(_landingGearLightsRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableSpeedbrakeOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_speedbrakeRenderer)) ||
                    (_speedbrakeForm != null && _speedbrakeForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Speedbrake_RenderEveryN ==
                         Settings.Default.Speedbrake_RenderOnN - 1) ||
                        (_speedbrakeForm != null && _speedbrakeForm.RenderImmediately))
                    {
                        if (_speedbrakeForm != null)
                        {
                            _speedbrakeForm.RenderImmediately = false;
                        }
                        if (_speedbrakeRenderStart != null)
                        {
                            _speedbrakeRenderStart.Set();
                        }
                        if (_speedbrakeRenderEnd != null)
                        {
                            toWait.Add(_speedbrakeRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalDEDPFLRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableDEDOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_dedRenderer) ||
                     (_dedForm != null && _dedForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.DED_RenderEveryN == Settings.Default.DED_RenderOnN - 1) ||
                        (_dedForm != null && _dedForm.RenderImmediately))
                    {
                        if (_dedForm != null)
                        {
                            _dedForm.RenderImmediately = false;
                        }
                        if (_dedRenderStart != null)
                        {
                            _dedRenderStart.Set();
                        }
                        if (_dedRenderEnd != null)
                        {
                            toWait.Add(_dedRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnablePFLOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_pflRenderer)) ||
                    (_pflForm != null && _pflForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.PFL_RenderEveryN == Settings.Default.PFL_RenderOnN - 1) ||
                        (_pflForm != null && _pflForm.RenderImmediately))
                    {
                        if (_pflForm != null)
                        {
                            _pflForm.RenderImmediately = false;
                        }
                        if (_pflRenderStart != null)
                        {
                            _pflRenderStart.Set();
                        }
                        if (_pflRenderEnd != null)
                        {
                            toWait.Add(_pflRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalFuelInstrumentsRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableFuelFlowOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_fuelFlowRenderer)) ||
                    (_fuelFlowForm != null && _fuelFlowForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FuelFlow_RenderEveryN ==
                         Settings.Default.FuelFlow_RenderOnN - 1) ||
                        (_fuelFlowForm != null && _fuelFlowForm.RenderImmediately))
                    {
                        if (_fuelFlowForm != null)
                        {
                            _fuelFlowForm.RenderImmediately = false;
                        }
                        if (_fuelFlowRenderStart != null)
                        {
                            _fuelFlowRenderStart.Set();
                        }
                        if (_fuelFlowRenderEnd != null)
                        {
                            toWait.Add(_fuelFlowRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableFuelQuantityOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_fuelQuantityRenderer)) ||
                    (_fuelQuantityForm != null && _fuelQuantityForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FuelQuantity_RenderEveryN ==
                         Settings.Default.FuelQuantity_RenderOnN - 1) ||
                        (_fuelQuantityForm != null && _fuelQuantityForm.RenderImmediately))
                    {
                        if (_fuelQuantityForm != null)
                        {
                            _fuelQuantityForm.RenderImmediately = false;
                        }
                        if (_fuelQuantityRenderStart != null)
                        {
                            _fuelQuantityRenderStart.Set();
                        }
                        if (_fuelQuantityRenderEnd != null)
                        {
                            toWait.Add(_fuelQuantityRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableEPUFuelOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_epuFuelRenderer)) ||
                    (_epuFuelForm != null && _epuFuelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.EPUFuel_RenderEveryN == Settings.Default.EPUFuel_RenderOnN - 1) ||
                        (_epuFuelForm != null && _epuFuelForm.RenderImmediately))
                    {
                        if (_epuFuelForm != null)
                        {
                            _epuFuelForm.RenderImmediately = false;
                        }
                        if (_epuFuelRenderStart != null)
                        {
                            _epuFuelRenderStart.Set();
                        }
                        if (_epuFuelRenderEnd != null)
                        {
                            toWait.Add(_epuFuelRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalIndexerRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableAOAIndexerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_aoaIndexerRenderer)) ||
                    (_aoaIndexerForm != null && _aoaIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.AOAIndexer_RenderEveryN ==
                         Settings.Default.AOAIndexer_RenderOnN - 1) ||
                        (_aoaIndexerForm != null && _aoaIndexerForm.RenderImmediately))
                    {
                        if (_aoaIndexerForm != null)
                        {
                            _aoaIndexerForm.RenderImmediately = false;
                        }
                        if (_aoaIndexerRenderStart != null)
                        {
                            _aoaIndexerRenderStart.Set();
                        }
                        if (_aoaIndexerRenderEnd != null)
                        {
                            toWait.Add(_aoaIndexerRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableNWSIndexerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nwsIndexerRenderer)) ||
                    (_nwsIndexerForm != null && _nwsIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NWSIndexer_RenderEveryN ==
                         Settings.Default.NWSIndexer_RenderOnN - 1) ||
                        (_nwsIndexerForm != null && _nwsIndexerForm.RenderImmediately))
                    {
                        if (_nwsIndexerForm != null)
                        {
                            _nwsIndexerForm.RenderImmediately = false;
                        }
                        if (_nwsIndexerRenderStart != null)
                        {
                            _nwsIndexerRenderStart.Set();
                        }
                        if (_nwsIndexerRenderEnd != null)
                        {
                            toWait.Add(_nwsIndexerRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalEngine1GaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableFTIT1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ftit1Renderer)) ||
                    (_ftit1Form != null && _ftit1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FTIT1_RenderEveryN == Settings.Default.FTIT1_RenderOnN - 1) ||
                        (_ftit1Form != null && _ftit1Form.RenderImmediately))
                    {
                        if (_ftit1Form != null)
                        {
                            _ftit1Form.RenderImmediately = false;
                        }
                        if (_ftit1RenderStart != null)
                        {
                            _ftit1RenderStart.Set();
                        }
                        if (_ftit1RenderEnd != null)
                        {
                            toWait.Add(_ftit1RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableNOZ1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nozPos1Renderer)) ||
                    (_nozPos1Form != null && _nozPos1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NOZ1_RenderEveryN == Settings.Default.NOZ1_RenderOnN - 1) ||
                        (_nozPos1Form != null && _nozPos1Form.RenderImmediately))
                    {
                        if (_nozPos1Form != null)
                        {
                            _nozPos1Form.RenderImmediately = false;
                        }
                        if (_nozPos1RenderStart != null)
                        {
                            _nozPos1RenderStart.Set();
                        }
                        if (_nozPos1RenderEnd != null)
                        {
                            toWait.Add(_nozPos1RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableOIL1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_oilGauge1Renderer)) ||
                    (_oilGauge1Form != null && _oilGauge1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.OIL1_RenderEveryN == Settings.Default.OIL1_RenderOnN - 1) ||
                        (_oilGauge1Form != null && _oilGauge1Form.RenderImmediately))
                    {
                        if (_oilGauge1Form != null)
                        {
                            _oilGauge1Form.RenderImmediately = false;
                        }
                        if (_oilGauge1RenderStart != null)
                        {
                            _oilGauge1RenderStart.Set();
                        }
                        if (_oilGauge1RenderEnd != null)
                        {
                            toWait.Add(_oilGauge1RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableRPM1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rpm1Renderer)) ||
                    (_rpm1Form != null && _rpm1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RPM1_RenderEveryN == Settings.Default.RPM1_RenderOnN - 1) ||
                        (_rpm1Form != null && _rpm1Form.RenderImmediately))
                    {
                        if (_rpm1Form != null)
                        {
                            _rpm1Form.RenderImmediately = false;
                        }
                        if (_rpm1RenderStart != null)
                        {
                            _rpm1RenderStart.Set();
                        }
                        if (_rpm1RenderEnd != null)
                        {
                            toWait.Add(_rpm1RenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalEngine2GaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableFTIT2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ftit2Renderer)) ||
                    (_ftit2Form != null && _ftit2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FTIT2_RenderEveryN == Settings.Default.FTIT2_RenderOnN - 1) ||
                        (_ftit2Form != null && _ftit2Form.RenderImmediately))
                    {
                        if (_ftit2Form != null)
                        {
                            _ftit2Form.RenderImmediately = false;
                        }
                        if (_ftit2RenderStart != null)
                        {
                            _ftit2RenderStart.Set();
                        }
                        if (_ftit2RenderEnd != null)
                        {
                            toWait.Add(_ftit2RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableNOZ2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nozPos2Renderer)) ||
                    (_nozPos2Form != null && _nozPos2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NOZ2_RenderEveryN == Settings.Default.NOZ2_RenderOnN - 1) ||
                        (_nozPos2Form != null && _nozPos2Form.RenderImmediately))
                    {
                        if (_nozPos2Form != null)
                        {
                            _nozPos2Form.RenderImmediately = false;
                        }
                        if (_nozPos2RenderStart != null)
                        {
                            _nozPos2RenderStart.Set();
                        }
                        if (_nozPos2RenderEnd != null)
                        {
                            toWait.Add(_nozPos2RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableOIL2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_oilGauge2Renderer)) ||
                    (_oilGauge2Form != null && _oilGauge2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.OIL2_RenderEveryN == Settings.Default.OIL2_RenderOnN - 1) ||
                        (_oilGauge2Form != null && _oilGauge2Form.RenderImmediately))
                    {
                        if (_oilGauge2Form != null)
                        {
                            _oilGauge2Form.RenderImmediately = false;
                        }
                        if (_oilGauge2RenderStart != null)
                        {
                            _oilGauge2RenderStart.Set();
                        }
                        if (_oilGauge2RenderEnd != null)
                        {
                            toWait.Add(_oilGauge2RenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableRPM2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rpm2Renderer)) ||
                    (_rpm2Form != null && _rpm2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RPM2_RenderEveryN == Settings.Default.RPM2_RenderOnN - 1) ||
                        (_rpm2Form != null && _rpm2Form.RenderImmediately))
                    {
                        if (_rpm2Form != null)
                        {
                            _rpm2Form.RenderImmediately = false;
                        }
                        if (_rpm2RenderStart != null)
                        {
                            _rpm2RenderStart.Set();
                        }
                        if (_rpm2RenderEnd != null)
                        {
                            toWait.Add(_rpm2RenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalRightAuxEmulatedGaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableHYDAOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hydARenderer)) ||
                    (_hydAForm != null && _hydAForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.HYDA_RenderEveryN == Settings.Default.HYDA_RenderOnN - 1) ||
                        (_hydAForm != null && _hydAForm.RenderImmediately))
                    {
                        if (_hydAForm != null)
                        {
                            _hydAForm.RenderImmediately = false;
                        }
                        if (_hydARenderStart != null)
                        {
                            _hydARenderStart.Set();
                        }
                        if (_hydARenderEnd != null)
                        {
                            toWait.Add(_hydARenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableHYDBOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hydBRenderer)) ||
                    (_hydBForm != null && _hydBForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.HYDB_RenderEveryN == Settings.Default.HYDB_RenderOnN - 1) ||
                        (_hydBForm != null && _hydBForm.RenderImmediately))
                    {
                        if (_hydBForm != null)
                        {
                            _hydBForm.RenderImmediately = false;
                        }
                        if (_hydBRenderStart != null)
                        {
                            _hydBRenderStart.Set();
                        }
                        if (_hydBRenderEnd != null)
                        {
                            toWait.Add(_hydBRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableCabinPressOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cabinPressRenderer)) ||
                    (_cabinPressForm != null && _cabinPressForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CabinPress_RenderEveryN ==
                         Settings.Default.CabinPress_RenderOnN - 1) ||
                        (_cabinPressForm != null && _cabinPressForm.RenderImmediately))
                    {
                        if (_cabinPressForm != null)
                        {
                            _cabinPressForm.RenderImmediately = false;
                        }
                        if (_cabinPressRenderStart != null)
                        {
                            _cabinPressRenderStart.Set();
                        }
                        if (_cabinPressRenderEnd != null)
                        {
                            toWait.Add(_cabinPressRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalTrimIndicatorRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableRollTrimOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rollTrimRenderer)) ||
                    (_rollTrimForm != null && _rollTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RollTrim_RenderEveryN ==
                         Settings.Default.RollTrim_RenderOnN - 1) ||
                        (_rollTrimForm != null && _rollTrimForm.RenderImmediately))
                    {
                        if (_rollTrimForm != null)
                        {
                            _rollTrimForm.RenderImmediately = false;
                        }
                        if (_rollTrimRenderStart != null)
                        {
                            _rollTrimRenderStart.Set();
                        }
                        if (_rollTrimRenderEnd != null)
                        {
                            toWait.Add(_rollTrimRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnablePitchTrimOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_pitchTrimRenderer)) ||
                    (_pitchTrimForm != null && _pitchTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.PitchTrim_RenderEveryN ==
                         Settings.Default.PitchTrim_RenderOnN - 1) ||
                        (_pitchTrimForm != null && _pitchTrimForm.RenderImmediately))
                    {
                        if (_pitchTrimForm != null)
                        {
                            _pitchTrimForm.RenderImmediately = false;
                        }
                        if (_pitchTrimRenderStart != null)
                        {
                            _pitchTrimRenderStart.Set();
                        }
                        if (_pitchTrimRenderEnd != null)
                        {
                            toWait.Add(_pitchTrimRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalPrimaryFlightInstrumentRenderThreadsToStart(List<WaitHandle> toWait)
        {
            if (!(_running && _keepRunning))
            {
                return;
            }

            bool renderOnlyOnStateChanges = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Settings.Default.EnableADIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_adiRenderer) ||
                     (_adiForm != null && _adiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.ADI_RenderEveryN == Settings.Default.ADI_RenderOnN - 1) ||
                        (_adiForm != null && _adiForm.RenderImmediately))
                    {
                        if (_adiForm != null)
                        {
                            _adiForm.RenderImmediately = false;
                        }
                        if (_adiRenderStart != null)
                        {
                            _adiRenderStart.Set();
                        }
                        if (_adiRenderEnd != null)
                        {
                            toWait.Add(_adiRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableISISOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_isisRenderer)) ||
                    (_isisForm != null && _isisForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.ISIS_RenderEveryN == Settings.Default.ISIS_RenderOnN - 1) ||
                        (_isisForm != null && _isisForm.RenderImmediately))
                    {
                        if (_isisForm != null)
                        {
                            _isisForm.RenderImmediately = false;
                        }
                        if (_isisRenderStart != null)
                        {
                            _isisRenderStart.Set();
                        }
                        if (_isisRenderEnd != null)
                        {
                            toWait.Add(_isisRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableHSIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hsiRenderer) ||
                     (_hsiForm != null && _hsiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.HSI_RenderEveryN == Settings.Default.HSI_RenderOnN - 1) ||
                        (_hsiForm != null && _hsiForm.RenderImmediately))
                    {
                        if (_hsiForm != null)
                        {
                            _hsiForm.RenderImmediately = false;
                        }
                        if (_hsiRenderStart != null)
                        {
                            _hsiRenderStart.Set();
                        }
                        if (_hsiRenderEnd != null)
                        {
                            toWait.Add(_hsiRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableEHSIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ehsiRenderer)) ||
                    (_ehsiForm != null && _ehsiForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.EHSI_RenderEveryN == Settings.Default.EHSI_RenderOnN - 1) ||
                        (_ehsiForm != null && _ehsiForm.RenderImmediately))
                    {
                        if (_ehsiForm != null)
                        {
                            _ehsiForm.RenderImmediately = false;
                        }
                        if (_ehsiRenderStart != null)
                        {
                            _ehsiRenderStart.Set();
                        }
                        if (_ehsiRenderEnd != null)
                        {
                            toWait.Add(_ehsiRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableAltimeterOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_altimeterRenderer)) ||
                    (_altimeterForm != null && _altimeterForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Altimeter_RenderEveryN ==
                         Settings.Default.Altimeter_RenderOnN - 1) ||
                        (_altimeterForm != null && _altimeterForm.RenderImmediately))
                    {
                        if (_altimeterForm != null)
                        {
                            _altimeterForm.RenderImmediately = false;
                        }
                        if (_altimeterRenderStart != null)
                        {
                            _altimeterRenderStart.Set();
                        }
                        if (_altimeterRenderEnd != null)
                        {
                            toWait.Add(_altimeterRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableASIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_asiRenderer) ||
                     (_asiForm != null && _asiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.ASI_RenderEveryN == Settings.Default.ASI_RenderOnN - 1) ||
                        (_asiForm != null && _asiForm.RenderImmediately))
                    {
                        if (_asiForm != null)
                        {
                            _asiForm.RenderImmediately = false;
                        }
                        if (_asiRenderStart != null)
                        {
                            _asiRenderStart.Set();
                        }
                        if (_asiRenderEnd != null)
                        {
                            toWait.Add(_asiRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableBackupADIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_backupAdiRenderer)) ||
                    (_backupAdiForm != null && _backupAdiForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Backup_ADI_RenderEveryN ==
                         Settings.Default.Backup_ADI_RenderOnN - 1) ||
                        (_backupAdiForm != null && _backupAdiForm.RenderImmediately))
                    {
                        if (_backupAdiForm != null)
                        {
                            _backupAdiForm.RenderImmediately = false;
                        }
                        if (_backupAdiRenderStart != null)
                        {
                            _backupAdiRenderStart.Set();
                        }
                        if (_backupAdiRenderEnd != null)
                        {
                            toWait.Add(_backupAdiRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableVVIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_vviRenderer) ||
                     (_vviForm != null && _vviForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.VVI_RenderEveryN == Settings.Default.VVI_RenderOnN - 1) ||
                        (_vviForm != null && _vviForm.RenderImmediately))
                    {
                        if (_vviForm != null)
                        {
                            _vviForm.RenderImmediately = false;
                        }
                        if (_vviRenderStart != null)
                        {
                            _vviRenderStart.Set();
                        }
                        if (_vviRenderEnd != null)
                        {
                            toWait.Add(_vviRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableAOAIndicatorOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_aoaIndicatorRenderer)) ||
                    (_aoaIndicatorForm != null && _aoaIndicatorForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.AOAIndicator_RenderEveryN ==
                         Settings.Default.AOAIndicator_RenderOnN - 1) ||
                        (_aoaIndicatorForm != null && _aoaIndicatorForm.RenderImmediately))
                    {
                        if (_aoaIndicatorForm != null)
                        {
                            _aoaIndicatorForm.RenderImmediately = false;
                        }
                        if (_aoaIndicatorRenderStart != null)
                        {
                            _aoaIndicatorRenderStart.Set();
                        }
                        if (_aoaIndicatorRenderEnd != null)
                        {
                            toWait.Add(_aoaIndicatorRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableCompassOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_compassRenderer)) ||
                    (_compassForm != null && _compassForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Compass_RenderEveryN == Settings.Default.Compass_RenderOnN - 1) ||
                        (_compassForm != null && _compassForm.RenderImmediately))
                    {
                        if (_compassForm != null)
                        {
                            _compassForm.RenderImmediately = false;
                        }
                        if (_compassRenderStart != null)
                        {
                            _compassRenderStart.Set();
                        }
                        if (_compassRenderEnd != null)
                        {
                            toWait.Add(_compassRenderEnd);
                        }
                    }
                }
            }
            if (Settings.Default.EnableAccelerometerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges ||
                    (renderOnlyOnStateChanges &&
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_accelerometerRenderer)) ||
                    (_accelerometerForm != null && _accelerometerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Accelerometer_RenderEveryN ==
                         Settings.Default.Accelerometer_RenderOnN - 1) ||
                        (_accelerometerForm != null && _accelerometerForm.RenderImmediately))
                    {
                        if (_accelerometerForm != null)
                        {
                            _accelerometerForm.RenderImmediately = false;
                        }
                        if (_accelerometerRenderStart != null)
                        {
                            _accelerometerRenderStart.Set();
                        }
                        if (_accelerometerRenderEnd != null)
                        {
                            toWait.Add(_accelerometerRenderEnd);
                        }
                    }
                }
            }
        }

        private void SignalMFDAndHudThreadsToStart()
        {
            if (!(_running && _keepRunning))
            {
                return;
            }
            if (Settings.Default.EnableMfd4Output || _networkMode == NetworkMode.Server)
            {
                _mfd4CaptureStart.Set();
            }
            if (Settings.Default.EnableMfd3Output || _networkMode == NetworkMode.Server)
            {
                _mfd3CaptureStart.Set();
            }
            if (Settings.Default.EnableLeftMFDOutput || _networkMode == NetworkMode.Server)
            {
                _leftMfdCaptureStart.Set();
            }
            if (Settings.Default.EnableRightMFDOutput || _networkMode == NetworkMode.Server)
            {
                _rightMfdCaptureStart.Set();
            }
            if (Settings.Default.EnableHudOutput || _networkMode == NetworkMode.Server)
            {
                _hudCaptureStart.Set();
            }
        }

        private void SimStatusMonitorThreadWork()
        {
            try
            {
                int count = 0;

                while (_keepRunning)
                {
                    count++;
                    if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
                    {
                        bool simWasRunning = _simRunning;

                        //TODO:make this check optional via the user-config file
                        if (count%1 == 0)
                        {
                            count = 0;
                            Common.Util.DisposeObject(_texSmStatusReader);
                            _texSmStatusReader = new F4TexSharedMem.Reader();

#if SIMRUNNING
                            _simRunning = true;
#else
                            try
                            {
                                _simRunning = NetworkMode == NetworkMode.Client ||
                                              F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex.Message, ex);
                            }
#endif
                            _sim3DDataAvailable = _simRunning &&
                                                  (NetworkMode == NetworkMode.Client ||
                                                   _texSmStatusReader.IsDataAvailable);

                            if (_sim3DDataAvailable)
                            {
                                try
                                {
                                    if (_threeDeeMode)
                                    {
                                        lock (_texSmReaderLock)
                                        {
                                            if (_texSmReader == null) _texSmReader = new F4TexSharedMem.Reader();
                                        }
                                        if ((Settings.Default.EnableLeftMFDOutput ||
                                             Settings.Default.EnableRightMFDOutput ||
                                             Settings.Default.EnableMfd3Output ||
                                             Settings.Default.EnableMfd4Output ||
                                             Settings.Default.EnableHudOutput ||
                                             NetworkMode == NetworkMode.Server))
                                        {
                                            if ((_hud3DInputRect == Rectangle.Empty) ||
                                                (_leftMfd3DInputRect == Rectangle.Empty) ||
                                                (_rightMfd3DInputRect == Rectangle.Empty) ||
                                                (_mfd3_3DInputRect == Rectangle.Empty) ||
                                                (_mfd3_3DInputRect == Rectangle.Empty))
                                            {
                                                BMSSupport.Read3DCoordinatesFromCurrentBmsDatFile(ref _mfd4_3DInputRect,
                                                                                       ref _mfd3_3DInputRect,
                                                                                       ref _leftMfd3DInputRect,
                                                                                       ref _rightMfd3DInputRect,
                                                                                       ref _hud3DInputRect);
                                            }
                                        }
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                }
                            }
                            else
                            {
                                _mfd4_3DInputRect = Rectangle.Empty;
                                _mfd3_3DInputRect = Rectangle.Empty;
                                _leftMfd3DInputRect = Rectangle.Empty;
                                _rightMfd3DInputRect = Rectangle.Empty;
                                _hud3DInputRect = Rectangle.Empty;
                            }
                            if (simWasRunning && !_simRunning)
                            {
                                CloseAndDisposeSharedmemReaders();

                                if (_networkMode == NetworkMode.Server)
                                {
                                    TearDownImageServer();
                                }
                            }
                            if (_networkMode == NetworkMode.Server && (!simWasRunning && _simRunning))
                            {
                                SetupNetworkingServer();
                            }
                        }
                    }
                    Thread.Sleep(500);
                    //System.GC.Collect();
                }
                Debug.WriteLine("SimStatusMonitorThreadWork has exited.");
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void CloseAndDisposeSharedmemReaders()
        {
            Common.Util.DisposeObject(_terrainBrowser);
            _terrainBrowser = null;

            Common.Util.DisposeObject(_texSmStatusReader);
            _texSmStatusReader = null;

            Common.Util.DisposeObject(_texSmReader);
            _texSmReader = null;

            Common.Util.DisposeObject(_falconSmReader);
            _falconSmReader = null;
        }

        #region Thread Teardown

        private void WaitForThreadEndThenAbort(ref Thread[] threads, TimeSpan timeout)
        {
            if (threads == null) return;
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < threads.Length; i++)
            {
                Thread t = threads[i];

                if (t == null)
                {
                    continue;
                }

                string threadName = t.Name ?? "unknown";

                //interrupt the threads
                try
                {
                    t.Interrupt();
                }
                catch (Exception)
                {
                }
            }

            for (int i = 0; i < threads.Length; i++)
            {
                Thread t = threads[i];

                if (t == null)
                {
                    continue;
                }

                string threadName = t.Name ?? "unknown";

                DateTime now = DateTime.Now;
                TimeSpan elapsed = now.Subtract(startTime);
                TimeSpan timeRemaining = elapsed.Subtract(timeout);
                if (timeRemaining.TotalMilliseconds <= 0) timeRemaining = new TimeSpan(0, 0, 0, 0, 1);

                try
                {
                    t.Join(timeRemaining);
                    t = null;
                    threads[i] = null;
                }
                catch (Exception e)
                {
                }
            }

            for (int i = 0; i < threads.Length; i++)
            {
                Thread t = threads[i];

                if (t == null)
                {
                    continue;
                }

                string threadName = t.Name ?? "unknown";

                if (t != null)
                {
                    try
                    {
                        _threadAbortion.AbortThread(ref t);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        #endregion

        #region Thread Setup

        private void SetupThreads()
        {
            SetupSimStatusMonitorThread();
            SetupCaptureOrchestrationThread();
            SetupKeyboardWatcherThread();

            SetupMFD4CaptureThread();
            SetupMFD3CaptureThread();
            SetupLeftMFDCaptureThread();
            SetupRightMFDCaptureThread();
            SetupHUDCaptureThread();
            _renderThreadSetupHelper.SetupThread(ref _adiRenderThread, _threadPriority, "ADIRenderThread", () => Settings.Default.EnableADIOutput, _adiRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _backupAdiRenderThread, _threadPriority, "StandbyADIRenderThread", () => Settings.Default.EnableBackupADIOutput, _backupAdiRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _asiRenderThread, _threadPriority, "ASIRenderThread", () => Settings.Default.EnableASIOutput, _asiRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _altimeterRenderThread, _threadPriority, "AltimeterRenderThread", () => Settings.Default.EnableAltimeterOutput, _altimeterRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _aoaIndexerRenderThread, _threadPriority, "AOAIndexerRenderThread", () => Settings.Default.EnableAOAIndexerOutput, _aoaIndexerRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _aoaIndicatorRenderThread, _threadPriority, "AOAIndicatorRenderThread", () => Settings.Default.EnableAOAIndicatorOutput, _aoaIndicatorRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _cautionPanelRenderThread, _threadPriority, "CautionPanelRenderThread", () => Settings.Default.EnableCautionPanelOutput, _cautionPanelRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _cmdsPanelRenderThread, _threadPriority, "CMDSPanelRenderThread", () => Settings.Default.EnableCMDSOutput, _cmdsPanelRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _compassRenderThread, _threadPriority, "CompassRenderThread", () => Settings.Default.EnableCompassOutput, _compassRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _dedRenderThread, _threadPriority, "DEDRenderThread", () => Settings.Default.EnableDEDOutput, _dedRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _pflRenderThread, _threadPriority, "PFLRenderThread", () => Settings.Default.EnablePFLOutput, _pflRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _epuFuelRenderThread, _threadPriority, "EPUFuelRenderThread", () => Settings.Default.EnableEPUFuelOutput, _epuFuelRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _accelerometerRenderThread, _threadPriority, "AccelerometerRenderThread", () => Settings.Default.EnableAccelerometerOutput, _accelerometerRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _ftit1RenderThread, _threadPriority, "FTIT1RenderThread", () => Settings.Default.EnableFTIT1Output, _ftit1RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _ftit2RenderThread, _threadPriority, "FTIT2RenderThread", () => Settings.Default.EnableFTIT2Output, _ftit2RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _fuelFlowRenderThread, _threadPriority, "FuelFlowRenderThread", () => Settings.Default.EnableFuelFlowOutput, _fuelFlowRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _isisRenderThread, _threadPriority, "ISISRenderThread", () => Settings.Default.EnableISISOutput, _isisRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _fuelQuantityRenderThread, _threadPriority, "FuelQuantityRenderThread", () => Settings.Default.EnableFuelQuantityOutput, _fuelQuantityRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _hsiRenderThread, _threadPriority, "HSIRenderThread", () => Settings.Default.EnableHSIOutput, _hsiRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _ehsiRenderThread, _threadPriority, "EHSIRenderThread", () => Settings.Default.EnableEHSIOutput, _ehsiRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _landingGearLightsRenderThread, _threadPriority, "LandingGearLightsRenderThread", () => Settings.Default.EnableGearLightsOutput, _landingGearLightsRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _nwsIndexerRenderThread, _threadPriority, "NWSIndexerRenderThread", () => Settings.Default.EnableNWSIndexerOutput, _nwsIndexerRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _nozPos1RenderThread, _threadPriority, "NOZ1RenderThread", () => Settings.Default.EnableNOZ1Output, _nozPos1RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _nozPos2RenderThread, _threadPriority, "NOZ2RenderThread", () => Settings.Default.EnableNOZ2Output, _nozPos2RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _oilGauge1RenderThread, _threadPriority, "OIL1RenderThread", () => Settings.Default.EnableOIL1Output, _oilGauge1RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _oilGauge2RenderThread, _threadPriority, "OIL2RenderThread", () => Settings.Default.EnableOIL2Output, _oilGauge2RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _rwrRenderThread, _threadPriority, "RWRRenderThread", () => Settings.Default.EnableRWROutput, _rwrRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _speedbrakeRenderThread, _threadPriority, "SpeedbrakeRenderThread", () => Settings.Default.EnableSpeedbrakeOutput, _speedbrakeRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _rpm1RenderThread, _threadPriority, "RPM1RenderThread", () => Settings.Default.EnableRPM1Output, _rpm1RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _rpm2RenderThread, _threadPriority, "RPM2RenderThread", () => Settings.Default.EnableRPM2Output, _rpm2RenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _vviRenderThread, _threadPriority, "VVIRenderThread", () => Settings.Default.EnableVVIOutput, _vviRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _hydARenderThread, _threadPriority, "HYDARenderThread", () => Settings.Default.EnableHYDAOutput, _hydARenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _hydBRenderThread, _threadPriority, "HYDBRenderThread", () => Settings.Default.EnableHYDBOutput, _hydBRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _cabinPressRenderThread, _threadPriority, "CabinPressRenderThread", () => Settings.Default.EnableCabinPressOutput, _cabinPressRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _rollTrimRenderThread, _threadPriority, "RollTrimRenderThread", () => Settings.Default.EnableRollTrimOutput, _rollTrimRenderThreadWorkHelper.DoWork);
            _renderThreadSetupHelper.SetupThread(ref _pitchTrimRenderThread, _threadPriority, "PitchTrimRenderThread", () => Settings.Default.EnablePitchTrimOutput, _pitchTrimRenderThreadWorkHelper.DoWork);

        }

        private void SetupKeyboardWatcherThread()
        {
            _threadAbortion.AbortThread(ref _keyboardWatcherThread);
            _keyboardWatcherThread = new Thread(KeyboardWatcherThreadWork);
            _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
            _keyboardWatcherThread.Priority = ThreadPriority.Highest;
            _keyboardWatcherThread.IsBackground = true;
            _keyboardWatcherThread.Name = "KeyboardWatcherThread";
        }

        private void SetupCaptureOrchestrationThread()
        {
            _threadAbortion.AbortThread(ref _captureOrchestrationThread);
            _captureOrchestrationThread = new Thread(CaptureOrchestrationThreadWork);
            _captureOrchestrationThread.Priority = _threadPriority;
            _captureOrchestrationThread.IsBackground = true;
            _captureOrchestrationThread.Name = "CaptureOrchestrationThread";
        }


        private void SetupHUDCaptureThread()
        {
            _threadAbortion.AbortThread(ref _hudCaptureThread);
            if (Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server)
            {
                _hudCaptureThread = new Thread(HudCaptureThreadWork);
                _hudCaptureThread.Priority = _threadPriority;
                _hudCaptureThread.IsBackground = true;
                _hudCaptureThread.Name = "HudCaptureThread";
            }
        }

        private void SetupRightMFDCaptureThread()
        {
            _threadAbortion.AbortThread(ref _rightMfdCaptureThread);
            if (Settings.Default.EnableRightMFDOutput || NetworkMode == NetworkMode.Server)
            {
                _rightMfdCaptureThread = new Thread(RightMfdCaptureThreadWork);
                _rightMfdCaptureThread.Priority = _threadPriority;
                _rightMfdCaptureThread.IsBackground = true;
                _rightMfdCaptureThread.Name = "RightMfdCaptureThread";
            }
        }

        private void SetupLeftMFDCaptureThread()
        {
            _threadAbortion.AbortThread(ref _leftMfdCaptureThread);
            if (Settings.Default.EnableLeftMFDOutput || NetworkMode == NetworkMode.Server)
            {
                _leftMfdCaptureThread = new Thread(LeftMfdCaptureThreadWork);
                _leftMfdCaptureThread.Priority = _threadPriority;
                _leftMfdCaptureThread.IsBackground = true;
                _leftMfdCaptureThread.Name = "LeftMfdCaptureThread";
            }
        }

        private void SetupMFD3CaptureThread()
        {
            _threadAbortion.AbortThread(ref _mfd3CaptureThread);
            if (Settings.Default.EnableMfd3Output || NetworkMode == NetworkMode.Server)
            {
                _mfd3CaptureThread = new Thread(Mfd3CaptureThreadWork);
                _mfd3CaptureThread.Priority = _threadPriority;
                _mfd3CaptureThread.IsBackground = true;
                _mfd3CaptureThread.Name = "Mfd3CaptureThread";
            }
        }

        private void SetupMFD4CaptureThread()
        {
            _threadAbortion.AbortThread(ref _mfd4CaptureThread);
            if (Settings.Default.EnableMfd4Output || NetworkMode == NetworkMode.Server)
            {
                _mfd4CaptureThread = new Thread(Mfd4CaptureThreadWork);
                _mfd4CaptureThread.Priority = _threadPriority;
                _mfd4CaptureThread.IsBackground = true;
                _mfd4CaptureThread.Name = "Mfd4CaptureThread";
            }
        }

        private void SetupSimStatusMonitorThread()
        {
            _threadAbortion.AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork);
            _simStatusMonitorThread.Priority = ThreadPriority.BelowNormal;
            _simStatusMonitorThread.IsBackground = true;
            _simStatusMonitorThread.Name = "SimStatusMonitorThread";
        }

        #endregion

        #region Gauges rendering thread-work methods

        private void UpdateRendererStatesFromFlightData()
        {
            if (_flightData == null || (NetworkMode != NetworkMode.Client && !_simRunning))
            {
                _flightData = new FlightData();
                _flightData.hsiBits = Int32.MaxValue;
            }

            FlightData fromFalcon = _flightData;
            var extensionData = ((FlightDataExtension) fromFalcon.ExtensionData);

            if (_simRunning || NetworkMode == NetworkMode.Client)
            {
                var hsibits = ((HsiBits) fromFalcon.hsiBits);
                var altbits = ((AltBits) fromFalcon.altBits); //12-08-12 added by Falcas 
                bool commandBarsOn = false;

                //*** UPDATE ISIS ***
                ((F16ISIS) _isisRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && _useBMSAdvancedSharedmemValues)
                {
                    ((F16ISIS) _isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                    //((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (-fromFalcon.z, ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure, ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);

                    if (fromFalcon.VersionNum >= 111)
                    {
                        if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                        {
                            ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                                F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                        }
                        else
                        {
                            ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                                F16ISIS.F16ISISOptions.PressureUnits.Millibars;
                        }

                        ((F16ISIS) _isisRenderer).InstrumentState.BarometricPressure = fromFalcon.AltCalReading;
                            //13-08-12 added by Falcas
                    }
                    else
                    {
                        ((F16ISIS) _isisRenderer).InstrumentState.BarometricPressure = 2992f;
                            //14-0-12 Falcas removed the point
                        ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                            F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                    }
                }
                else
                {
                    //((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure, ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);
                    ((F16ISIS) _isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                    ((F16ISIS) _isisRenderer).InstrumentState.BarometricPressure = 2992f;
                        //14-0-12 Falcas removed the point
                    ((F16ISIS) _isisRenderer).Options.PressureAltitudeUnits =
                        F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                }
                if (extensionData != null)
                {
                    ((F16ISIS) _isisRenderer).InstrumentState.RadarAltitudeAGL = extensionData.RadarAltitudeFeetAGL;
                }
                ((F16ISIS) _isisRenderer).InstrumentState.MachNumber = fromFalcon.mach;
                ((F16ISIS) _isisRenderer).InstrumentState.MagneticHeadingDegrees = (360 +
                                                                                    (fromFalcon.yaw/
                                                                                     Constants.RADIANS_PER_DEGREE))%360;
                ((F16ISIS) _isisRenderer).InstrumentState.NeverExceedSpeedKnots = 850;

                ((F16ISIS) _isisRenderer).InstrumentState.PitchDegrees = ((fromFalcon.pitch/Constants.RADIANS_PER_DEGREE));
                ((F16ISIS) _isisRenderer).InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));
                ((F16ISIS) _isisRenderer).InstrumentState.VerticalVelocityFeetPerMinute = -fromFalcon.zDot*60.0f;
                ((F16ISIS) _isisRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                ((F16ISIS) _isisRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
                ((F16ISIS) _isisRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
                ((F16ISIS) _isisRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) ==
                                                                           HsiBits.ADI_LOC);


                // *** UPDATE VVI ***
                float verticalVelocity = 0;
                if (((hsibits & HsiBits.VVI) == HsiBits.VVI))
                {
                    verticalVelocity = 0;
                }
                else
                {
                    verticalVelocity = -fromFalcon.zDot*60.0f;
                }

                if (_vviRenderer is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU) _vviRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) ==
                                                                                               HsiBits.VVI);
                    ((F16VerticalVelocityIndicatorEU) _vviRenderer).InstrumentState.VerticalVelocityFeet =
                        verticalVelocity;
                }
                else if (_vviRenderer is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA) _vviRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) ==
                                                                                                HsiBits.VVI);
                    ((F16VerticalVelocityIndicatorUSA) _vviRenderer).InstrumentState.VerticalVelocityFeetPerMinute =
                        verticalVelocity;
                }

                //*********************

                // *** UPDATE ALTIMETER ***
                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && _useBMSAdvancedSharedmemValues)
                {
                    if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                    {
                        ((F16Altimeter) _altimeterRenderer).Options.PressureAltitudeUnits =
                            F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury;
                    }
                    else
                    {
                        ((F16Altimeter) _altimeterRenderer).Options.PressureAltitudeUnits =
                            F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars;
                    }
                    //((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (- fromFalcon.z,((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure, ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury) ;
                    ((F16Altimeter) _altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                    if (fromFalcon.VersionNum >= 111)
                    {
                        ((F16Altimeter) _altimeterRenderer).InstrumentState.BarometricPressure =
                            fromFalcon.AltCalReading; //12-08-12 added by Falcas
                        ((F16Altimeter) _altimeterRenderer).InstrumentState.PneumaticModeFlag = ((altbits &
                                                                                                  AltBits.PneuFlag) ==
                                                                                                 AltBits.PneuFlag);
                            //12-08-12 added by Falcas
                    }
                    else
                    {
                        ((F16Altimeter) _altimeterRenderer).InstrumentState.BarometricPressure = 2992f;
                            //12-08-12 added by Falcas
                        ((F16Altimeter) _altimeterRenderer).InstrumentState.PneumaticModeFlag = false;
                            //12-08-12 added by Falcas
                        ((F16Altimeter) _altimeterRenderer).Options.PressureAltitudeUnits =
                            F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
                    }
                }
                else
                {
                    //((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure, ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury);
                    ((F16Altimeter) _altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                    ((F16Altimeter) _altimeterRenderer).InstrumentState.BarometricPressure = 2992f;
                        //12-08-12 added by Falcas
                    ((F16Altimeter) _altimeterRenderer).InstrumentState.PneumaticModeFlag = false;
                        //12-08-12 added by Falcas
                    ((F16Altimeter) _altimeterRenderer).Options.PressureAltitudeUnits =
                        F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
                }
                //*************************

                //*** UPDATE ASI ***
                ((F16AirspeedIndicator) _asiRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;
                ((F16AirspeedIndicator) _asiRenderer).InstrumentState.MachNumber = fromFalcon.mach;
                //*************************

                //**** UPDATE COMPASS
                ((F16Compass) _compassRenderer).InstrumentState.MagneticHeadingDegrees = (360 +
                                                                                          (fromFalcon.yaw/
                                                                                           Constants.RADIANS_PER_DEGREE))%
                                                                                         360;
                //*******************

                //**** UPDATE AOA INDICATOR***
                if (((hsibits & HsiBits.AOA) == HsiBits.AOA))
                {
                    ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.OffFlag = true;
                    ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees = 0;
                }
                else
                {
                    ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.OffFlag = false;
                    ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees =
                        fromFalcon.alpha;
                }
                //*******************

                //**** UPDATE AOA INDEXER***
                float aoa = ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees;
                ((F16AngleOfAttackIndexer) _aoaIndexerRenderer).InstrumentState.AoaBelow = ((fromFalcon.lightBits &
                                                                                             (int) LightBits.AOABelow) ==
                                                                                            (int) LightBits.AOABelow);
                ((F16AngleOfAttackIndexer) _aoaIndexerRenderer).InstrumentState.AoaOn = ((fromFalcon.lightBits &
                                                                                          (int) LightBits.AOAOn) ==
                                                                                         (int) LightBits.AOAOn);
                ((F16AngleOfAttackIndexer) _aoaIndexerRenderer).InstrumentState.AoaAbove = ((fromFalcon.lightBits &
                                                                                             (int) LightBits.AOAAbove) ==
                                                                                            (int) LightBits.AOAAbove);
                //**************************


                //***** UPDATE ADI *****
                ((F16ADI) _adiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                ((F16ADI) _adiRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
                ((F16ADI) _adiRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
                ((F16ADI) _adiRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
                //**********************

                //***** UPDATE BACKUP ADI *****
                ((F16StandbyADI) _backupAdiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) ==
                                                                                HsiBits.BUP_ADI_OFF);
                //**********************

                //***** UPDATE HSI ***** 
                ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.HSI_OFF) ==
                                                                                            HsiBits.HSI_OFF);
                ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.MagneticHeadingDegrees = (360 +
                                                                                                           (fromFalcon
                                                                                                                .yaw/
                                                                                                            Constants
                                                                                                                .RADIANS_PER_DEGREE))%
                                                                                                          360;
                ((F16EHSI) _ehsiRenderer).InstrumentState.NoDataFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
                ((F16EHSI) _ehsiRenderer).InstrumentState.MagneticHeadingDegrees = (360 +
                                                                                    (fromFalcon.yaw/
                                                                                     Constants.RADIANS_PER_DEGREE))%360;
                //**********************

                if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
                {
                    //if the standby ADI is off
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.RollDegrees = 0;
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.OffFlag = true;
                }
                else
                {
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.PitchDegrees = ((fromFalcon.pitch/
                                                                                          Constants.RADIANS_PER_DEGREE));
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.RollDegrees = ((fromFalcon.roll/
                                                                                         Constants.RADIANS_PER_DEGREE));
                    ((F16StandbyADI) _backupAdiRenderer).InstrumentState.OffFlag = false;
                }


                //***** UPDATE SOME COMPLEX HSI/ADI VARIABLES
                if (((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF))
                {
                    //if the ADI is off
                    ((F16ADI) _adiRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16ADI) _adiRenderer).InstrumentState.RollDegrees = 0;
                    ((F16ADI) _adiRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
                    ((F16ADI) _adiRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
                    ((F16ADI) _adiRenderer).InstrumentState.ShowCommandBars = false;

                    ((F16ISIS) _isisRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16ISIS) _isisRenderer).InstrumentState.RollDegrees = 0;
                    ((F16ISIS) _isisRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
                    ((F16ISIS) _isisRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
                    ((F16ISIS) _isisRenderer).InstrumentState.ShowCommandBars = false;
                }
                else
                {
                    ((F16ADI) _adiRenderer).InstrumentState.PitchDegrees = ((fromFalcon.pitch/
                                                                             Constants.RADIANS_PER_DEGREE));
                    ((F16ADI) _adiRenderer).InstrumentState.RollDegrees = ((fromFalcon.roll/Constants.RADIANS_PER_DEGREE));

                    ((F16ISIS) _isisRenderer).InstrumentState.PitchDegrees = ((fromFalcon.pitch/
                                                                               Constants.RADIANS_PER_DEGREE));
                    ((F16ISIS) _isisRenderer).InstrumentState.RollDegrees = ((fromFalcon.roll/
                                                                              Constants.RADIANS_PER_DEGREE));

                    //The following floating data is also crossed up in the flightData.h File:
                    //float AdiIlsHorPos;       // Position of horizontal ILS bar ----Vertical
                    //float AdiIlsVerPos;       // Position of vertical ILS bar-----horizontal
                    commandBarsOn = ((float) (Math.Abs(Math.Round(fromFalcon.AdiIlsHorPos, 4))) != 0.1745f);
                    if (
                        (Math.Abs((fromFalcon.AdiIlsVerPos/Constants.RADIANS_PER_DEGREE)) > 1.0f)
                        ||
                        (Math.Abs((fromFalcon.AdiIlsHorPos/Constants.RADIANS_PER_DEGREE)) > 5.0f)
                        )
                    {
                        commandBarsOn = false;
                    }
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = true;

                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & HsiBits.TotalFlags) != HsiBits.TotalFlags)
                    {
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
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
                            ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = true;
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
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                    }
                    if (commandBarsOn)
                    {
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                    }

                    ((F16ADI) _adiRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ADI) _adiRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos/
                                                                                         Constants.RADIANS_PER_DEGREE;
                    ((F16ADI) _adiRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos/
                                                                                        Constants.RADIANS_PER_DEGREE;

                    ((F16ISIS) _isisRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ISIS) _isisRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos/
                                                                                           Constants.RADIANS_PER_DEGREE;
                    ((F16ISIS) _isisRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos/
                                                                                          Constants.RADIANS_PER_DEGREE;
                }

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && _useBMSAdvancedSharedmemValues)
                {
                    /*
                    This value is called navMode and is unsigned char type with 4 possible values: ILS_TACAN = 0, and TACAN = 1,
                    NAV = 2, ILS_NAV = 3
                    */

                    byte bmsNavMode = fromFalcon.navMode;
                    switch (bmsNavMode)
                    {
                        case 0: //NavModes.PlsTcn:
                            ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode =
                                F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                            break;
                        case 1: //NavModes.Tcn:
                            ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode =
                                F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                            ((F16ADI) _adiRenderer).InstrumentState.ShowCommandBars = false;
                            ((F16ISIS) _isisRenderer).InstrumentState.ShowCommandBars = false;
                            break;
                        case 2: //NavModes.Nav:
                            ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode =
                                F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                            ((F16ADI) _adiRenderer).InstrumentState.ShowCommandBars = false;
                            ((F16ISIS) _isisRenderer).InstrumentState.ShowCommandBars = false;
                            break;
                        case 3: //NavModes.PlsNav:
                            ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode =
                                F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    ((F16EHSI) _ehsiRenderer).InstrumentState.InstrumentMode =
                        F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown;
                }
                if (((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF))
                {
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DmeInvalidFlag = true;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DeviationInvalidFlag = false;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationDegrees = 0;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.BearingToBeaconDegrees = 0;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DmeInvalidFlag = true;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DeviationInvalidFlag = false;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.CourseDeviationDegrees = 0;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.BearingToBeaconDegrees = 0;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
                }
                else
                {
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DmeInvalidFlag = ((hsibits &
                                                                                                        HsiBits
                                                                                                            .CourseWarning) ==
                                                                                                       HsiBits
                                                                                                           .CourseWarning);
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits &
                                                                                                              HsiBits
                                                                                                                  .IlsWarning) ==
                                                                                                             HsiBits
                                                                                                                 .IlsWarning);
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationLimitDegrees =
                        fromFalcon.deviationLimit;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationDegrees =
                        fromFalcon.courseDeviation;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DesiredCourseDegrees =
                        (int) fromFalcon.desiredCourse;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DesiredHeadingDegrees =
                        (int) fromFalcon.desiredHeading;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.BearingToBeaconDegrees =
                        fromFalcon.bearingToBeacon;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles =
                        fromFalcon.distanceToBeacon;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) ==
                                                                                HsiBits.CourseWarning);
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) ==
                                                                                      HsiBits.IlsWarning);
                    ((F16EHSI) _ehsiRenderer).InstrumentState.CourseDeviationLimitDegrees = fromFalcon.deviationLimit;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.CourseDeviationDegrees = fromFalcon.courseDeviation;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DesiredCourseDegrees = (int) fromFalcon.desiredCourse;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DesiredHeadingDegrees = (int) fromFalcon.desiredHeading;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.BearingToBeaconDegrees = fromFalcon.bearingToBeacon;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles =
                        fromFalcon.distanceToBeacon;
                }


                {
                    //compute course deviation and TO/FROM
                    float deviationLimitDecimalDegrees =
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationLimitDegrees%180;
                    float desiredCourseInDegrees =
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.DesiredCourseDegrees;
                    float courseDeviationDecimalDegrees =
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationDegrees;
                    float bearingToBeaconInDegrees =
                        ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.BearingToBeaconDegrees;
                    float myCourseDeviationDecimalDegrees = Common.Math.Util.AngleDelta(desiredCourseInDegrees,
                                                                                        bearingToBeaconInDegrees);
                    bool toFlag = false;
                    bool fromFlag = false;
                    if (Math.Abs(courseDeviationDecimalDegrees) <= 90)
                    {
                        toFlag = true;
                        fromFlag = false;
                    }
                    else
                    {
                        toFlag = false;
                        fromFlag = true;
                    }

                    if (courseDeviationDecimalDegrees < -90)
                    {
                        courseDeviationDecimalDegrees =
                            Common.Math.Util.AngleDelta(Math.Abs(courseDeviationDecimalDegrees), 180)%180;
                    }
                    else if (courseDeviationDecimalDegrees > 90)
                    {
                        courseDeviationDecimalDegrees =
                            -Common.Math.Util.AngleDelta(courseDeviationDecimalDegrees, 180)%180;
                    }
                    else
                    {
                        courseDeviationDecimalDegrees = -courseDeviationDecimalDegrees;
                    }
                    if (Math.Abs(courseDeviationDecimalDegrees) > deviationLimitDecimalDegrees)
                        courseDeviationDecimalDegrees = Math.Sign(courseDeviationDecimalDegrees)*
                                                        deviationLimitDecimalDegrees;

                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.CourseDeviationDegrees =
                        courseDeviationDecimalDegrees;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.ToFlag = toFlag;
                    ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.FromFlag = fromFlag;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.ToFlag = toFlag;
                    ((F16EHSI) _ehsiRenderer).InstrumentState.FromFlag = fromFlag;
                }

                //**************************


                //*** UPDATE EHSI **********
                UpdateEHSIBrightnessLabelVisibility();
                //**************************


                //**  UPDATE HYDA/HYDB****
                float rpm = fromFalcon.rpm;
                bool mainGen = ((fromFalcon.lightBits3 & (int) LightBits3.MainGen) == (int) LightBits3.MainGen);
                bool stbyGen = ((fromFalcon.lightBits3 & (int) LightBits3.StbyGen) == (int) LightBits3.StbyGen);
                bool epuGen = ((fromFalcon.lightBits3 & (int) LightBits3.EpuGen) == (int) LightBits3.EpuGen);
                bool epuOn = ((fromFalcon.lightBits2 & (int) LightBits2.EPUOn) == (int) LightBits2.EPUOn);
                float epuFuel = fromFalcon.epuFuel;
                ((F16HydraulicPressureGauge) _hydARenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch =
                    NonImplementedGaugeCalculations.HydA(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
                ((F16HydraulicPressureGauge) _hydBRenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch =
                    NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
                //**************************

                //**  UPDATE CABIN PRESSURE ALTITUDE INDICATOR****
                float z = fromFalcon.z;
                float origCabinAlt =
                    ((F16CabinPressureAltitudeIndicator) _cabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet;
                bool pressurization = ((fromFalcon.lightBits & (int) LightBits.CabinPress) == (int) LightBits.CabinPress);
                ((F16CabinPressureAltitudeIndicator) _cabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet =
                    NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
                //**************************

                //**  UPDATE ROLL TRIM INDICATOR****
                float rolltrim = fromFalcon.TrimRoll;
                ((F16RollTrimIndicator) _rollTrimRenderer).InstrumentState.RollTrimPercent = rolltrim*2.0f*100.0f;
                //**************************

                //**  UPDATE PITCH TRIM INDICATOR****
                float pitchTrim = fromFalcon.TrimPitch;
                ((F16PitchTrimIndicator) _pitchTrimRenderer).InstrumentState.PitchTrimPercent = pitchTrim*2.0f*100.0f;
                //**************************


                //**  UPDATE RWR ****
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.MagneticHeadingDegrees = (360 +
                                                                                               (fromFalcon.yaw/
                                                                                                Constants
                                                                                                    .RADIANS_PER_DEGREE))%
                                                                                              360;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.RollDegrees = ((fromFalcon.roll/
                                                                                     Constants.RADIANS_PER_DEGREE));
                int rwrObjectCount = fromFalcon.RwrObjectCount;
                if (fromFalcon.RWRsymbol != null)
                {
                    var blips =
                        new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[fromFalcon.RWRsymbol.Length];
                    ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Blips = blips;
                    //for (int i = 0; i < rwrObjectCount; i++)
                    if (fromFalcon.RWRsymbol != null)
                    {
                        for (int i = 0; i < fromFalcon.RWRsymbol.Length; i++)
                        {
                            var thisBlip = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip();
                            if (i < rwrObjectCount) thisBlip.Visible = true;
                            if (fromFalcon.bearing != null)
                            {
                                thisBlip.BearingDegrees = (fromFalcon.bearing[i]/Constants.RADIANS_PER_DEGREE);
                            }
                            if (fromFalcon.lethality != null)
                            {
                                thisBlip.Lethality = fromFalcon.lethality[i];
                            }
                            if (fromFalcon.missileActivity != null)
                            {
                                thisBlip.MissileActivity = fromFalcon.missileActivity[i];
                            }
                            if (fromFalcon.missileLaunch != null)
                            {
                                thisBlip.MissileLaunch = fromFalcon.missileLaunch[i];
                            }
                            if (fromFalcon.newDetection != null)
                            {
                                thisBlip.NewDetection = fromFalcon.newDetection[i];
                            }
                            if (fromFalcon.selected != null)
                            {
                                thisBlip.Selected = fromFalcon.selected[i];
                            }
                            thisBlip.SymbolID = fromFalcon.RWRsymbol[i];
                            blips[i] = thisBlip;
                        }
                    }
                }
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Activity = ((fromFalcon.lightBits2 &
                                                                                  (int) LightBits2.AuxAct) ==
                                                                                 (int) LightBits2.AuxAct);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.ChaffCount = (int) fromFalcon.ChaffCount;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 &
                                                                                  (int) LightBits2.ChaffLo) ==
                                                                                 (int) LightBits2.ChaffLo);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.EWSDegraded = ((fromFalcon.lightBits2 &
                                                                                     (int) LightBits2.Degr) ==
                                                                                    (int) LightBits2.Degr);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.EWSDispenseReady = ((fromFalcon.lightBits2 &
                                                                                          (int) LightBits2.Rdy) ==
                                                                                         (int) LightBits2.Rdy);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.EWSNoGo = (
                                                                                   ((fromFalcon.lightBits2 &
                                                                                     (int) LightBits2.NoGo) ==
                                                                                    (int) LightBits2.NoGo)
                                                                                   ||
                                                                                   ((fromFalcon.lightBits2 &
                                                                                     (int) LightBits2.Degr) ==
                                                                                    (int) LightBits2.Degr)
                                                                               );
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.EWSGo =
                    (
                        ((fromFalcon.lightBits2 & (int) LightBits2.Go) == (int) LightBits2.Go)
                        &&
                        !(
                             ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                             ||
                             ((fromFalcon.lightBits2 & (int) LightBits2.Degr) == (int) LightBits2.Degr)
                             ||
                             ((fromFalcon.lightBits2 & (int) LightBits2.Rdy) == (int) LightBits2.Rdy)
                         )
                    );


                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.FlareCount = (int) fromFalcon.FlareCount;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 &
                                                                                  (int) LightBits2.FlareLo) ==
                                                                                 (int) LightBits2.FlareLo);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Handoff = ((fromFalcon.lightBits2 &
                                                                                 (int) LightBits2.HandOff) ==
                                                                                (int) LightBits2.HandOff);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Launch = ((fromFalcon.lightBits2 &
                                                                                (int) LightBits2.Launch) ==
                                                                               (int) LightBits2.Launch);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.LowAltitudeMode = ((fromFalcon.lightBits2 &
                                                                                         (int) LightBits2.AuxLow) ==
                                                                                        (int) LightBits2.AuxLow);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.NavalMode = ((fromFalcon.lightBits2 &
                                                                                   (int) LightBits2.Naval) ==
                                                                                  (int) LightBits2.Naval);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Other1Count = 0;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Other1Low = true;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Other2Count = 0;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.Other2Low = true;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.RWRPowerOn = ((fromFalcon.lightBits2 &
                                                                                    (int) LightBits2.AuxPwr) ==
                                                                                   (int) LightBits2.AuxPwr);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.PriorityMode = ((fromFalcon.lightBits2 &
                                                                                      (int) LightBits2.PriMode) ==
                                                                                     (int) LightBits2.PriMode);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.SearchMode = ((fromFalcon.lightBits2 &
                                                                                    (int) LightBits2.AuxSrch) ==
                                                                                   (int) LightBits2.AuxSrch);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.SeparateMode = ((fromFalcon.lightBits2 &
                                                                                      (int) LightBits2.TgtSep) ==
                                                                                     (int) LightBits2.TgtSep);
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.UnknownThreatScanMode = ((fromFalcon.lightBits2 &
                                                                                               (int) LightBits2.Unk) ==
                                                                                              (int) LightBits2.Unk);
                //********************

                //** UPDATE CAUTION PANEL
                //TODO: implement all-lights-on when test is detected
                F16CautionPanel.F16CautionPanelInstrumentState myState =
                    ((F16CautionPanel) _cautionPanelRenderer).InstrumentState;
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.AftFuelLow = ((fromFalcon.lightBits2 &
                                                                                         (int) LightBits2.AftFuelLow) ==
                                                                                        (int) LightBits2.AftFuelLow);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.AntiSkid = ((fromFalcon.lightBits2 &
                                                                                       (int) LightBits2.ANTI_SKID) ==
                                                                                      (int) LightBits2.ANTI_SKID);
                //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ATFNotEngaged = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED) == (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.AvionicsFault = ((fromFalcon.lightBits &
                                                                                            (int) LightBits.Avionics) ==
                                                                                           (int) LightBits.Avionics);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.BUC = ((fromFalcon.lightBits2 &
                                                                                  (int) LightBits2.BUC) ==
                                                                                 (int) LightBits2.BUC);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.CabinPress = ((fromFalcon.lightBits &
                                                                                         (int) LightBits.CabinPress) ==
                                                                                        (int) LightBits.CabinPress);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.CADC = ((fromFalcon.lightBits3 &
                                                                                   (int) Bms4LightBits3.cadc) ==
                                                                                  (int) Bms4LightBits3.cadc);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.ECM = ((fromFalcon.lightBits &
                                                                                  (int) LightBits.ECM) ==
                                                                                 (int) LightBits.ECM);
                //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.EEC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ee) == (int)F4SharedMem.Headers.LightBits.ECM);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.ElecSys = ((fromFalcon.lightBits3 &
                                                                                      (int) LightBits3.Elec_Fault) ==
                                                                                     (int) LightBits3.Elec_Fault);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.EngineFault = ((fromFalcon.lightBits &
                                                                                          (int) LightBits.EngineFault) ==
                                                                                         (int) LightBits.EngineFault);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.EquipHot = ((fromFalcon.lightBits &
                                                                                       (int) LightBits.EQUIP_HOT) ==
                                                                                      (int) LightBits.EQUIP_HOT);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.FLCSFault = ((fromFalcon.lightBits &
                                                                                        (int) LightBits.FltControlSys) ==
                                                                                       (int) LightBits.FltControlSys);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.FuelOilHot = ((fromFalcon.lightBits2 &
                                                                                         (int) LightBits2.FUEL_OIL_HOT) ==
                                                                                        (int) LightBits2.FUEL_OIL_HOT);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.FwdFuelLow = ((fromFalcon.lightBits2 &
                                                                                         (int) LightBits2.FwdFuelLow) ==
                                                                                        (int) LightBits2.FwdFuelLow);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.Hook = ((fromFalcon.lightBits &
                                                                                   (int) LightBits.Hook) ==
                                                                                  (int) LightBits.Hook);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.IFF = ((fromFalcon.lightBits &
                                                                                  (int) LightBits.IFF) ==
                                                                                 (int) LightBits.IFF);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.NWSFail = ((fromFalcon.lightBits &
                                                                                      (int) LightBits.NWSFail) ==
                                                                                     (int) LightBits.NWSFail);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.Overheat = ((fromFalcon.lightBits &
                                                                                       (int) LightBits.Overheat) ==
                                                                                      (int) LightBits.Overheat);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.OxyLow = ((fromFalcon.lightBits2 &
                                                                                     (int) LightBits2.OXY_LOW) ==
                                                                                    (int) LightBits2.OXY_LOW);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.ProbeHeat = ((fromFalcon.lightBits2 &
                                                                                        (int) LightBits2.PROBEHEAT) ==
                                                                                       (int) LightBits2.PROBEHEAT);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.RadarAlt = ((fromFalcon.lightBits &
                                                                                       (int) LightBits.RadarAlt) ==
                                                                                      (int) LightBits.RadarAlt);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.SeatNotArmed = ((fromFalcon.lightBits2 &
                                                                                           (int) LightBits2.SEAT_ARM) ==
                                                                                          (int) LightBits2.SEAT_ARM);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.SEC = ((fromFalcon.lightBits2 &
                                                                                  (int) LightBits2.SEC) ==
                                                                                 (int) LightBits2.SEC);
                ((F16CautionPanel) _cautionPanelRenderer).InstrumentState.StoresConfig = ((fromFalcon.lightBits &
                                                                                           (int) LightBits.CONFIG) ==
                                                                                          (int) LightBits.CONFIG);

                //TODO: implement MLU cautions

                //***********************

                //**  UPDATE CMDS PANEL
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Degraded = ((fromFalcon.lightBits2 &
                                                                                 (int) LightBits2.Degr) ==
                                                                                (int) LightBits2.Degr);
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.ChaffCount = (int) fromFalcon.ChaffCount;
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 &
                                                                                 (int) LightBits2.ChaffLo) ==
                                                                                (int) LightBits2.ChaffLo);
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.DispenseReady = ((fromFalcon.lightBits2 &
                                                                                      (int) LightBits2.Rdy) ==
                                                                                     (int) LightBits2.Rdy);
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.FlareCount = (int) fromFalcon.FlareCount;
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 &
                                                                                 (int) LightBits2.FlareLo) ==
                                                                                (int) LightBits2.FlareLo);
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Go =
                    (
                        ((fromFalcon.lightBits2 & (int) LightBits2.Go) == (int) LightBits2.Go)
                    //Falcas 04/09/2012 to match what you see in BMS
                    //    &&
                    //!(
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                    //             ||
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    //             ||
                    //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                    //)
                    );

                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.NoGo =
                    (
                        ((fromFalcon.lightBits2 & (int) LightBits2.NoGo) == (int) LightBits2.NoGo)
                    //Falcas 04/09/2012 to match what you see in BMS
                    //         ||
                    //((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    );
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Other1Count = 0;
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Other1Low = true;
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Other2Count = 0;
                ((F16CMDSPanel) _cmdsPanelRenderer).InstrumentState.Other2Low = true;
                //**********************

                //** UPDATE DED 
                if (fromFalcon.DEDLines != null)
                {
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line1 =
                        Encoding.Default.GetBytes(fromFalcon.DEDLines[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line2 =
                        Encoding.Default.GetBytes(fromFalcon.DEDLines[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line3 =
                        Encoding.Default.GetBytes(fromFalcon.DEDLines[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line4 =
                        Encoding.Default.GetBytes(fromFalcon.DEDLines[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line5 =
                        Encoding.Default.GetBytes(fromFalcon.DEDLines[4] ?? "");
                }
                if (fromFalcon.Invert != null)
                {
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line1Invert =
                        Encoding.Default.GetBytes(fromFalcon.Invert[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line2Invert =
                        Encoding.Default.GetBytes(fromFalcon.Invert[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line3Invert =
                        Encoding.Default.GetBytes(fromFalcon.Invert[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line4Invert =
                        Encoding.Default.GetBytes(fromFalcon.Invert[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _dedRenderer).InstrumentState.Line5Invert =
                        Encoding.Default.GetBytes(fromFalcon.Invert[4] ?? "");
                }
                //*************************


                //** UPDATE PFL
                if (fromFalcon.PFLLines != null)
                {
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line1 =
                        Encoding.Default.GetBytes(fromFalcon.PFLLines[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line2 =
                        Encoding.Default.GetBytes(fromFalcon.PFLLines[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line3 =
                        Encoding.Default.GetBytes(fromFalcon.PFLLines[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line4 =
                        Encoding.Default.GetBytes(fromFalcon.PFLLines[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line5 =
                        Encoding.Default.GetBytes(fromFalcon.PFLLines[4] ?? "");
                }
                if (fromFalcon.PFLInvert != null)
                {
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line1Invert =
                        Encoding.Default.GetBytes(fromFalcon.PFLInvert[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line2Invert =
                        Encoding.Default.GetBytes(fromFalcon.PFLInvert[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line3Invert =
                        Encoding.Default.GetBytes(fromFalcon.PFLInvert[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line4Invert =
                        Encoding.Default.GetBytes(fromFalcon.PFLInvert[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList) _pflRenderer).InstrumentState.Line5Invert =
                        Encoding.Default.GetBytes(fromFalcon.PFLInvert[4] ?? "");
                }
                //*************************

                //** UPDATE EPU FUEL
                ((F16EPUFuelGauge) _epuFuelRenderer).InstrumentState.FuelRemainingPercent = fromFalcon.epuFuel;
                //******************

                //** UPDATE FUEL FLOW
                ((F16FuelFlow) _fuelFlowRenderer).InstrumentState.FuelFlowPoundsPerHour = fromFalcon.fuelFlow;
                //******************

                //** UPDATE FUEL QTY
                ((F16FuelQuantityIndicator) _fuelQuantityRenderer).InstrumentState.AftLeftFuelQuantityPounds =
                    fromFalcon.aft/10.0f;
                ((F16FuelQuantityIndicator) _fuelQuantityRenderer).InstrumentState.ForeRightFuelQuantityPounds =
                    fromFalcon.fwd/10.0f;
                ((F16FuelQuantityIndicator) _fuelQuantityRenderer).InstrumentState.TotalFuelQuantityPounds =
                    fromFalcon.total;
                //******************

                //** UPDATE LANDING GEAR LIGHTS

                if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon ||
                    fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.LeftGearDown =
                        ((fromFalcon.lightBits3 & (int) LightBits3.LeftGearDown) == (int) LightBits3.LeftGearDown);
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.NoseGearDown =
                        ((fromFalcon.lightBits3 & (int) LightBits3.NoseGearDown) == (int) LightBits3.NoseGearDown);
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.RightGearDown =
                        ((fromFalcon.lightBits3 & (int) LightBits3.RightGearDown) == (int) LightBits3.RightGearDown);
                }
                else
                {
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.LeftGearDown =
                        ((fromFalcon.LeftGearPos == 1.0f));
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.NoseGearDown =
                        ((fromFalcon.NoseGearPos == 1.0f));
                    ((F16LandingGearWheelsLights) _landingGearLightsRenderer).InstrumentState.RightGearDown =
                        ((fromFalcon.RightGearPos == 1.0f));
                }


                //******************

                //** UPDATE NWS
                ((F16NosewheelSteeringIndexer) _nwsIndexerRenderer).InstrumentState.DISC = ((fromFalcon.lightBits &
                                                                                             (int) LightBits.RefuelDSC) ==
                                                                                            (int) LightBits.RefuelDSC);
                ((F16NosewheelSteeringIndexer) _nwsIndexerRenderer).InstrumentState.AR_NWS = ((fromFalcon.lightBits &
                                                                                               (int) LightBits.RefuelAR) ==
                                                                                              (int) LightBits.RefuelAR);
                ((F16NosewheelSteeringIndexer) _nwsIndexerRenderer).InstrumentState.RDY = ((fromFalcon.lightBits &
                                                                                            (int) LightBits.RefuelRDY) ==
                                                                                           (int) LightBits.RefuelRDY);
                //******************

                //** UPDATE SPEEDBRAKE
                ((F16SpeedbrakeIndicator) _speedbrakeRenderer).InstrumentState.PercentOpen = fromFalcon.speedBrake*
                                                                                             100.0f;

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    ((F16SpeedbrakeIndicator) _speedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 &
                                                                                                 (int)
                                                                                                 Bms4LightBits3
                                                                                                     .Power_Off) ==
                                                                                                (int)
                                                                                                Bms4LightBits3.Power_Off);
                }
                else
                {
                    ((F16SpeedbrakeIndicator) _speedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 &
                                                                                                 (int)
                                                                                                 LightBits3.Power_Off) ==
                                                                                                (int)
                                                                                                LightBits3.Power_Off);
                }
                //******************

                //** UPDATE RPM1
                ((F16Tachometer) _rpm1Renderer).InstrumentState.RPMPercent = fromFalcon.rpm;
                //******************

                //** UPDATE RPM2
                ((F16Tachometer) _rpm2Renderer).InstrumentState.RPMPercent = fromFalcon.rpm2;
                //******************

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    //Only BMS4 has a valid FTIT value in sharedmem
                    //** UPDATE FTIT1
                    ((F16FanTurbineInletTemperature) _ftit1Renderer).InstrumentState.InletTemperatureDegreesCelcius =
                        fromFalcon.ftit*100.0f;
                    //******************
                    //** UPDATE FTIT2
                    ((F16FanTurbineInletTemperature) _ftit2Renderer).InstrumentState.InletTemperatureDegreesCelcius =
                        fromFalcon.ftit2*100.0f;
                    //******************
                }
                else
                {
                    //FTIT is hosed in AF, RedViper, FF5, OF
                    //** UPDATE FTIT1
                    ((F16FanTurbineInletTemperature) _ftit1Renderer).InstrumentState.InletTemperatureDegreesCelcius =
                        NonImplementedGaugeCalculations.Ftit(
                            ((F16FanTurbineInletTemperature) _ftit1Renderer).InstrumentState
                                                                            .InletTemperatureDegreesCelcius,
                            fromFalcon.rpm);
                    //******************
                    //** UPDATE FTIT2
                    ((F16FanTurbineInletTemperature) _ftit2Renderer).InstrumentState.InletTemperatureDegreesCelcius =
                        NonImplementedGaugeCalculations.Ftit(
                            ((F16FanTurbineInletTemperature) _ftit2Renderer).InstrumentState
                                                                            .InletTemperatureDegreesCelcius,
                            fromFalcon.rpm2);
                    //******************
                }

                if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
                {
                    //NOZ is hosed in OF
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator) _nozPos1Renderer).InstrumentState.NozzlePositionPercent =
                        NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator) _nozPos2Renderer).InstrumentState.NozzlePositionPercent =
                        NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
                    //******************
                }
                else if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator) _nozPos1Renderer).InstrumentState.NozzlePositionPercent =
                        fromFalcon.nozzlePos*100.0f;
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator) _nozPos2Renderer).InstrumentState.NozzlePositionPercent =
                        fromFalcon.nozzlePos2*100.0f;
                    //******************
                }
                else
                {
                    //NOZ is OK in AF, RedViper, FF5
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator) _nozPos1Renderer).InstrumentState.NozzlePositionPercent =
                        fromFalcon.nozzlePos;
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator) _nozPos2Renderer).InstrumentState.NozzlePositionPercent =
                        fromFalcon.nozzlePos2;
                    //******************
                }

                //** UPDATE OIL1
                ((F16OilPressureGauge) _oilGauge1Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure;
                //******************

                //** UPDATE OIL2
                ((F16OilPressureGauge) _oilGauge2Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure2;
                //******************

                //** UPDATE ACCELEROMETER
                float gs = fromFalcon.gs;
                if (gs == 0) //ignore exactly zero g's
                {
                    gs = 1;
                }
                ((F16Accelerometer) _accelerometerRenderer).InstrumentState.AccelerationInGs = gs;
                //******************
            }
            else //Falcon's not running
            {
                if (_vviRenderer is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU) _vviRenderer).InstrumentState.OffFlag = true;
                }
                else if (_vviRenderer is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA) _vviRenderer).InstrumentState.OffFlag = true;
                }
                ((F16AngleOfAttackIndicator) _aoaIndicatorRenderer).InstrumentState.OffFlag = true;
                ((F16HorizontalSituationIndicator) _hsiRenderer).InstrumentState.OffFlag = true;
                ((F16EHSI) _ehsiRenderer).InstrumentState.NoDataFlag = true;
                ((F16ADI) _adiRenderer).InstrumentState.OffFlag = true;
                ((F16StandbyADI) _backupAdiRenderer).InstrumentState.OffFlag = true;
                ((F16AzimuthIndicator) _rwrRenderer).InstrumentState.RWRPowerOn = false;
                ((F16ISIS) _isisRenderer).InstrumentState.RadarAltitudeAGL = 0;
                ((F16ISIS) _isisRenderer).InstrumentState.OffFlag = true;
                UpdateEHSIBrightnessLabelVisibility();
            }
        }

        private float GetIndicatedAltitude(float trueAltitude, float baroPressure, bool pressureInInchesOfMercury)
        {
            float baroPressureInchesOfMercury = baroPressure;
            if (!pressureInInchesOfMercury)
            {
                baroPressureInchesOfMercury = baroPressure/Constants.INCHES_MERCURY_TO_HECTOPASCALS;
            }
            float baroDifference = baroPressureInchesOfMercury - 29.92f;
            float baroChangePerThousandFeet = 1.08f;
            float altitudeCorrection = (baroDifference/baroChangePerThousandFeet)*1000.0f;
            return altitudeCorrection + trueAltitude;
        }

        private void UpdateEHSIBrightnessLabelVisibility()
        {
            bool showBrightnessLabel = false;
            if (EHSIRightKnobIsCurrentlyDepressed())
            {
                DateTime? whenPressed = _ehsiRightKnobDepressedTime;
                if (whenPressed.HasValue)
                {
                    TimeSpan howLongPressed = DateTime.Now.Subtract(whenPressed.Value);
                    if (howLongPressed.TotalMilliseconds > 2000)
                    {
                        showBrightnessLabel = true;
                    }
                }
            }
            else
            {
                DateTime? whenReleased = _ehsiRightKnobReleasedTime;
                DateTime? lastActivity = _ehsiRightKnobLastActivityTime;
                if (whenReleased.HasValue && lastActivity.HasValue)
                {
                    TimeSpan howLongAgoReleased = DateTime.Now.Subtract(whenReleased.Value);
                    TimeSpan howLongAgoLastActivity = DateTime.Now.Subtract(lastActivity.Value);
                    if (howLongAgoReleased.TotalMilliseconds < 2000 || howLongAgoLastActivity.TotalMilliseconds < 2000)
                    {
                        showBrightnessLabel = ((F16EHSI) _ehsiRenderer).InstrumentState.ShowBrightnessLabel;
                    }
                }
            }
            ((F16EHSI) _ehsiRenderer).InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
        }

        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(IInstrumentRenderer renderer)
        {
            int staleDataTimeout = 500; //Timeout.Infinite;
            var baseRenderer = renderer as InstrumentRendererBase;
            if (baseRenderer == null) return true;
            int oldStateHash = 0;
            DateTime oldStateDateTime = DateTime.MinValue;
            bool oldHashWasFound = false;
            if (_instrumentStates.ContainsKey(baseRenderer))
            {
                oldStateHash = _instrumentStates[baseRenderer].HashCode;
                oldStateDateTime = _instrumentStates[baseRenderer].DateTime;
                oldHashWasFound = true;
            }
            InstrumentStateBase newState = baseRenderer.GetState();
            int newStateHash = newState != null ? newState.GetHashCode() : 0;
            DateTime newStateDateTime = DateTime.Now;

            bool hashesAreDifferent = !oldHashWasFound || (oldStateHash != newStateHash);

            int timeSinceHashChanged = Int32.MaxValue;
            if (oldStateDateTime != DateTime.MinValue)
            {
                timeSinceHashChanged = (int) Math.Floor(DateTime.Now.Subtract(oldStateDateTime).TotalMilliseconds);
            }
            bool stateIsStaleOrChanged = (hashesAreDifferent ||
                                          (timeSinceHashChanged > staleDataTimeout &&
                                           staleDataTimeout != Timeout.Infinite));
            if (stateIsStaleOrChanged)
            {
                var toStore = new Extractor.InstrumentStateSnapshot {DateTime = newStateDateTime, HashCode = newStateHash};
                if (_instrumentStates.ContainsKey(baseRenderer))
                {
                    _instrumentStates[baseRenderer] = toStore;
                }
                else
                {
                    _instrumentStates.Add(baseRenderer, toStore);
                }
            }
            InstrumentForm form = GetFormForRenderer(renderer);
            if (ShouldHighlightingBorderBeDisplayedOnTargetForm(form)) return true;
            return stateIsStaleOrChanged;
        }


        private void RenderInstrumentImage(IInstrumentRenderer renderer, InstrumentForm targetForm,
                                           RotateFlipType rotation, bool monochrome)
        {
            DateTime startTime = DateTime.Now;
            if (renderer == null || targetForm == null) return;
            if (DateTime.Now.Subtract(targetForm.LastRenderedOn).TotalMilliseconds < Settings.Default.PollingDelay)
            {
                return;
            }
            Bitmap image = null;
            try
            {
                if (targetForm.Rotation.ToString().Contains("90") || targetForm.Rotation.ToString().Contains("270"))
                {
                    image = new Bitmap(targetForm.ClientRectangle.Height, targetForm.ClientRectangle.Width,
                                       PixelFormat.Format32bppPArgb);
                }
                else
                {
                    image = new Bitmap(targetForm.ClientRectangle.Width, targetForm.ClientRectangle.Height,
                                       PixelFormat.Format32bppPArgb);
                }
                using (Graphics g = Graphics.FromImage(image))
                {
                    try
                    {
                        renderer.Render(g, new Rectangle(0, 0, image.Width, image.Height));
                        targetForm.LastRenderedOn = DateTime.Now;
                        if (ShouldHighlightingBorderBeDisplayedOnTargetForm(targetForm))
                        {
                            Color scopeGreenColor = Color.FromArgb(255, 63, 250, 63);
                            var scopeGreenPen = new Pen(scopeGreenColor);
                            scopeGreenPen.Width = 5;
                            g.DrawRectangle(scopeGreenPen, new Rectangle(new Point(0, 0), image.Size));
                            targetForm.RenderImmediately = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            _log.Error("An error occurred while rendering " + renderer.GetType(), e);
                        }
                        catch (NullReferenceException ex)
                        {
                        }
                    }
                }
                if (rotation != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(rotation);
                }
                using (Graphics graphics = targetForm.CreateGraphics())
                {
                    if (NightMode)
                    {
                        var nvisImageAttribs = new ImageAttributes();
                        ColorMatrix cm = Util.GetNVISColorMatrix(255, 255);
                        nvisImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                        graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height,
                                           GraphicsUnit.Pixel, nvisImageAttribs);
                    }
                    else if (monochrome)
                    {
                        var monochromeImageAttribs = new ImageAttributes();
                        ColorMatrix cm = GetGreyscaleColorMatrix();
                        monochromeImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                        graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height,
                                           GraphicsUnit.Pixel, monochromeImageAttribs);
                    }
                    else
                    {
                        graphics.DrawImageUnscaled(image, 0, 0, image.Width, image.Height);
                    }
                }
            }
            catch (ExternalException)
            {
                //GDI+ error message we don't care about
            }
            catch (ObjectDisposedException)
            {
                //GDI+ error message thrown on operations on disposed images -- can happen when one thread disposes while shutting-down thread tries to render
            }
            catch (ArgumentException)
            {
                //GDI+ error message we don't care about
            }
            catch (OutOfMemoryException)
            {
                //bullshit OOM messages from GDI+
            }
            catch (InvalidOperationException)
            {
                //GDI+ error message we don't care about
            }
            finally
            {
                Common.Util.DisposeObject(image);
            }
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            if (elapsed.TotalMilliseconds < MIN_RENDERER_PASS_TIME_MILLSECONDS)
            {
                var toWait = new TimeSpan(0, 0, 0, 0,
                                          (int) (MIN_RENDERER_PASS_TIME_MILLSECONDS - elapsed.TotalMilliseconds));
                if (toWait.TotalMilliseconds < MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER)
                {
                    toWait = new TimeSpan(0, 0, 0, 0, MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER);
                }
                Thread.Sleep(toWait);
            }
        }

        private static bool ShouldHighlightingBorderBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return SizingOrMovingCursorsAreDisplayed(targetForm)
                   &&
                   Settings.Default.HighlightOutputWindows
                ;
        }

        private bool WindowSizingOrMovingBeingAttemptedOnAnyOutputWindow()
        {
            bool retVal = false;
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    var iForm = form as InstrumentForm;
                    if (iForm != null)
                    {
                        if (
                            iForm.Visible && SizingOrMovingCursorsAreDisplayed(iForm)
                            &&
                            (
                                ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                                ||
                                ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                            )
                            )
                        {
                            retVal = true;
                            break;
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
                //if the OpenForms collection is modified during our loop (by the application shutting down, etc), we need to swallow this here
            {
            }
            return retVal;
        }

        private static bool SizingOrMovingCursorsAreDisplayed(InstrumentForm targetForm)
        {
            return (
                       (
                           targetForm.Cursor == Cursors.SizeAll
                           ||
                           targetForm.Cursor == Cursors.SizeNESW
                           ||
                           targetForm.Cursor == Cursors.SizeNS
                           ||
                           targetForm.Cursor == Cursors.SizeNWSE
                           ||
                           targetForm.Cursor == Cursors.SizeWE
                       )
                       ||
                       new Rectangle(targetForm.Location, targetForm.Size).Contains(Cursor.Position)
                   );
        }

        #endregion

        #region MFD rendering thread-work methods

        private void LeftMfdCaptureThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _leftMfdCaptureStart.WaitOne();
                    CaptureLeftMfd();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void RightMfdCaptureThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _rightMfdCaptureStart.WaitOne();
                    CaptureRightMfd();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void HudCaptureThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _hudCaptureStart.WaitOne();
                    CaptureHud();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void Mfd3CaptureThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _mfd3CaptureStart.WaitOne();
                    CaptureMfd3();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void Mfd4CaptureThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _mfd4CaptureStart.WaitOne();
                    CaptureMfd4();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        #endregion

        #endregion

        #region Object Disposal & Destructors

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                    _settingsSaverAsyncWorker.DoWork -= _settingsSaverAsyncWorker_DoWork;
                    _settingsLoaderAsyncWorker.DoWork -= _settingsLoaderAsyncWorker_DoWork;
                    Common.Util.DisposeObject(_asiForm);
                    Common.Util.DisposeObject(_adiForm);
                    Common.Util.DisposeObject(_backupAdiForm);
                    Common.Util.DisposeObject(_altimeterForm);
                    Common.Util.DisposeObject(_aoaIndexerForm);
                    Common.Util.DisposeObject(_aoaIndicatorForm);
                    Common.Util.DisposeObject(_cautionPanelForm);
                    Common.Util.DisposeObject(_cmdsPanelForm);
                    Common.Util.DisposeObject(_compassForm);
                    Common.Util.DisposeObject(_dedForm);
                    Common.Util.DisposeObject(_pflForm);
                    Common.Util.DisposeObject(_epuFuelForm);
                    Common.Util.DisposeObject(_accelerometerForm);
                    Common.Util.DisposeObject(_ftit1Form);
                    Common.Util.DisposeObject(_ftit2Form);
                    Common.Util.DisposeObject(_fuelFlowForm);
                    Common.Util.DisposeObject(_isisForm);
                    Common.Util.DisposeObject(_fuelQuantityForm);
                    Common.Util.DisposeObject(_hsiForm);
                    Common.Util.DisposeObject(_ehsiForm);
                    Common.Util.DisposeObject(_landingGearLightsForm);
                    Common.Util.DisposeObject(_nwsIndexerForm);
                    Common.Util.DisposeObject(_nozPos1Form);
                    Common.Util.DisposeObject(_nozPos2Form);
                    Common.Util.DisposeObject(_oilGauge1Form);
                    Common.Util.DisposeObject(_oilGauge2Form);
                    Common.Util.DisposeObject(_rwrForm);
                    Common.Util.DisposeObject(_speedbrakeForm);
                    Common.Util.DisposeObject(_rpm1Form);
                    Common.Util.DisposeObject(_rpm2Form);
                    Common.Util.DisposeObject(_vviForm);
                    Common.Util.DisposeObject(_hydAForm);
                    Common.Util.DisposeObject(_hydBForm);
                    Common.Util.DisposeObject(_cabinPressForm);
                    Common.Util.DisposeObject(_rollTrimForm);
                    Common.Util.DisposeObject(_pitchTrimForm);

                    Common.Util.DisposeObject(_mfd4Form);
                    Common.Util.DisposeObject(_mfd3Form);
                    Common.Util.DisposeObject(_leftMfdForm);
                    Common.Util.DisposeObject(_rightMfdForm);
                    Common.Util.DisposeObject(_hudForm);
                    Common.Util.DisposeObject(_texSmReader);
                    Common.Util.DisposeObject(_texSmStatusReader);
                    Common.Util.DisposeObject(_falconSmReader);
                    Common.Util.DisposeObject(_mfd4BlankImage);
                    Common.Util.DisposeObject(_mfd3BlankImage);
                    Common.Util.DisposeObject(_leftMfdBlankImage);
                    Common.Util.DisposeObject(_rightMfdBlankImage);
                    Common.Util.DisposeObject(_hudBlankImage);
                    Common.Util.DisposeObject(_mfd4TestAlignmentImage);
                    Common.Util.DisposeObject(_mfd3TestAlignmentImage);
                    Common.Util.DisposeObject(_leftMfdTestAlignmentImage);
                    Common.Util.DisposeObject(_rightMfdTestAlignmentImage);
                    Common.Util.DisposeObject(_hudTestAlignmentImage);
                }
            }
            _disposed = true;
        }

        internal static void DisposeInstance()
        {
            if (_extractor != null)
            {
                Common.Util.DisposeObject(_extractor);
                _extractor = null;
            }
        }

        #endregion
    }
}