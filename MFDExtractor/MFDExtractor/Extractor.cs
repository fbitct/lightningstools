using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport.DirectInput;
using Common.InputSupport.UI;
using Common.SimSupport;
using Common.UI;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Process;
using F4Utils.Terrain;
using MFDExtractor.EventSystem;
using MFDExtractor.Networking;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using log4net;
using Constants = Common.Math.Constants;
using Util = Common.Imaging.Util;
using Common.Networking;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor
{
	public class AllForms
	{
		
	}
	public sealed class Extractor : AllForms, IDisposable
    {
        #region Instance Variables

        private const int MIN_RENDERER_PASS_TIME_MILLSECONDS = 0;
        private const int MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER = 0;
        private static readonly ILog _log = LogManager.GetLogger(typeof (Extractor));
        private static Extractor _extractor;
        private bool _disposed;
        private long _renderCycleNum;

	    private readonly IInputControlSelectionSettingReader _inputControlSelectionSettingReader = new InputControlSelectionSettingReader();
		private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache= new InstrumentStateSnapshotCache();
		private readonly IInstrumentFormSettingsReader _instrumentFormSettingsReader = new InstrumentFormSettingsReader();

        private readonly Dictionary<IInstrumentRenderer, InstrumentForm> _outputForms = new Dictionary<IInstrumentRenderer, InstrumentForm>();
	    private KeySettings _keySettings;
	    private IKeySettingsReader _keySettingsReader = new KeySettingsReader();

		private readonly IInstrumentRendererSet _renderers = new InstrumentRendererSet();
        private readonly IRendererSetInitializer _rendererSetInitializer;

        private GdiPlusOptions _gdiPlusOptions = new GdiPlusOptions();


        private Form _applicationForm;
		private InstrumentForms _forms;
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
        private IFlightDataUpdater _flightDataUpdater = new FlightDataUpdater();
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

       

        #endregion

        #region Threads

        private readonly IMediatorStateChangeHandler _mediatorEventHandler;
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

        private Thread _simStatusMonitorThread;
        private Thread _speedbrakeRenderThread;
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;

        private Thread _vviRenderThread;
        private readonly RenderThreadSetupHelper _renderThreadSetupHelper;
        private readonly ThreadAbortion _threadAbortion;
        private readonly BMSSupport _bmsSupport;
        private IRenderThreadWorkHelper _adiRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _backupAdiRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _asiRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _altimeterRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _aoaIndexerRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _aoaIndicatorRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _cautionPanelRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _cmdsPanelRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _compassRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _dedRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _pflRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _epuFuelRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _accelerometerRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _ftit1RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _ftit2RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _fuelFlowRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _isisRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _fuelQuantityRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _hsiRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _ehsiRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _landingGearLightsRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _nwsIndexerRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _nozPos1RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _nozPos2RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _oilGauge1RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _oilGauge2RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _rwrRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _speedbrakeRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _rpm1RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _rpm2RenderThreadWorkHelper;
        private IRenderThreadWorkHelper _vviRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _hydARenderThreadWorkHelper;
        private IRenderThreadWorkHelper _hydBRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _cabinPressRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _rollTrimRenderThreadWorkHelper;
        private IRenderThreadWorkHelper _pitchTrimRenderThreadWorkHelper;
        private readonly DIHotkeyDetection _diHotkeyDetection;
	    private IDirectInputEventHotkeyFilter _directInputEventHotkeyFilter;
	    private IEHSIStateTracker _ehsiStateTracker;

		

	    private readonly IKeyDownEventHandler _keyDownEventHandler;
	    private readonly IKeyUpEventHandler _keyUpEventHandler;
	    private readonly IKeyboardWatcher _keyboardWatcher;
	    private readonly IClientSideIncomingMessageDispatcher _clientSideIncomingMessageDispatcher;
		private readonly IServerSideIncomingMessageDispatcher _serverSideIncomingMessageDispatcher;
	    private readonly IGdiPlusOptionsReader _gdiPlusOptionsReader;
	    private readonly IInputEvents _inputEvents;
	    private readonly InstrumentRenderHelper _instrumentRenderHelper;
        #endregion

        #endregion


        private Extractor(
			IKeyDownEventHandler keyDownEventHandler = null, 
			IKeyDownEventHandler keyUpEventHandler=null,
			IKeyboardWatcher keyboardWatcher = null,
			IDirectInputEventHotkeyFilter directInputEventHotkeyFilter= null, 
			IEHSIStateTracker ehsiStateTracker =null, 
			IClientSideIncomingMessageDispatcher clientSideIncomingMessageDispatcher = null,
			IServerSideIncomingMessageDispatcher serverSideIncomingMessageDispatcher = null, 
			IGdiPlusOptionsReader gdiPlusOptionsReader=null,
			IInputEvents inputEvents = null,
            IInstrumentRenderHelper instrumentRenderHelper = null)
        {
			_forms = new InstrumentForms();
	        _gdiPlusOptionsReader = gdiPlusOptionsReader ?? new GdiPlusOptionsReader();
            LoadSettings();
			_rendererSetInitializer = new RendererSetInitializer(_renderers);
			_rendererSetInitializer.Initialize(_gdiPlusOptions);
			_ehsiStateTracker = ehsiStateTracker ?? new EHSIStateTracker(_renderers.EHSI);
			_directInputEventHotkeyFilter = directInputEventHotkeyFilter ?? new DirectInputEventHotkeyFilter();
            _instrumentRenderHelper = new InstrumentRenderHelper();
			State = new ExtractorState();

			_diHotkeyDetection = new DIHotkeyDetection(Mediator);
	        _mediatorEventHandler =  new MediatorStateChangeHandler(_keySettings, _directInputEventHotkeyFilter,_diHotkeyDetection, _ehsiStateTracker,_inputEvents );
            if (!Settings.Default.DisableDirectInputMediator)
            {
                Mediator = new Mediator(null);
            }
            _renderThreadSetupHelper = new RenderThreadSetupHelper();
            _threadAbortion = new ThreadAbortion();
            _bmsSupport = new BMSSupport();
	        _keyDownEventHandler = keyDownEventHandler ?? new KeyDownEventHandler(_ehsiStateTracker, _inputEvents, _keySettings);

			_keyUpEventHandler = new KeyUpEventHandler(_keySettings, _ehsiStateTracker, _inputEvents);
			_keyboardWatcher = keyboardWatcher ?? new KeyboardWatcher(_keyDownEventHandler, _keyUpEventHandler, _log);
			_clientSideIncomingMessageDispatcher = clientSideIncomingMessageDispatcher ?? new ClientSideIncomingMessageDispatcher(_inputEvents, _client);
			_serverSideIncomingMessageDispatcher = serverSideIncomingMessageDispatcher ?? new ServerSideIncomingMessageDispatcher(_inputEvents);

        }
        private void SetupRenderThreadWorkHelpers()
        {
            _adiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _adiRenderStart, _adiRenderEnd, _renderers.ADI, _forms.ADIForm, () => Settings.Default.ADI_RotateFlipType, () => Settings.Default.ADI_Monochrome, RenderInstrumentImage);
            _backupAdiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _backupAdiRenderStart, _backupAdiRenderEnd, _renderers.BackupADI, _forms.BackupAdiForm, () => Settings.Default.Backup_ADI_RotateFlipType, () => Settings.Default.Backup_ADI_Monochrome, RenderInstrumentImage);
            _asiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _asiRenderStart, _asiRenderEnd, _renderers.ASI, _forms.ASIForm, () => Settings.Default.ASI_RotateFlipType, () => Settings.Default.ASI_Monochrome, RenderInstrumentImage);
            _altimeterRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _altimeterRenderStart, _altimeterRenderEnd, _renderers.Altimeter, _forms.AltimeterForm, () => Settings.Default.Altimeter_RotateFlipType, () => Settings.Default.Altimeter_Monochrome, RenderInstrumentImage);
            _aoaIndexerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _aoaIndexerRenderStart, _aoaIndexerRenderEnd, _renderers.AOAIndexer, _forms.AOAIndexerForm, () => Settings.Default.AOAIndexer_RotateFlipType, () => Settings.Default.AOAIndexer_Monochrome, RenderInstrumentImage);
            _aoaIndicatorRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _aoaIndicatorRenderStart, _aoaIndicatorRenderEnd, _renderers.AOAIndicator, _forms.AOAIndicatorForm, () => Settings.Default.AOAIndicator_RotateFlipType, () => Settings.Default.AOAIndicator_Monochrome, RenderInstrumentImage);
            _cautionPanelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cautionPanelRenderStart, _cautionPanelRenderEnd, _renderers.CautionPanel, _forms.CautionPanelForm, () => Settings.Default.CautionPanel_RotateFlipType, () => Settings.Default.CautionPanel_Monochrome, RenderInstrumentImage);
            _cmdsPanelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cmdsPanelRenderStart, _cmdsPanelRenderEnd, _renderers.CMDSPanel, _forms.CMDSPanelForm, () => Settings.Default.CMDS_RotateFlipType, () => Settings.Default.CMDS_Monochrome, RenderInstrumentImage);
            _compassRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _compassRenderStart, _compassRenderEnd, _renderers.Compass, _forms.CompassForm, () => Settings.Default.Compass_RotateFlipType, () => Settings.Default.Compass_Monochrome, RenderInstrumentImage);
            _dedRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _dedRenderStart, _dedRenderEnd, _renderers.DED, _forms.DEDForm, () => Settings.Default.DED_RotateFlipType, () => Settings.Default.DED_Monochrome, RenderInstrumentImage);
            _pflRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _pflRenderStart, _pflRenderEnd, _renderers.PFL, _forms.PFLForm, () => Settings.Default.PFL_RotateFlipType, () => Settings.Default.PFL_Monochrome, RenderInstrumentImage);
            _epuFuelRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _epuFuelRenderStart, _epuFuelRenderEnd, _renderers.EPUFuel, _forms.EPUFuelForm, () => Settings.Default.EPUFuel_RotateFlipType, () => Settings.Default.EPUFuel_Monochrome, RenderInstrumentImage);
            _accelerometerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _accelerometerRenderStart, _accelerometerRenderEnd, _renderers.Accelerometer, _forms.AccelerometerForm, () => Settings.Default.Accelerometer_RotateFlipType, () => Settings.Default.Accelerometer_Monochrome, RenderInstrumentImage);
            _ftit1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ftit1RenderStart, _ftit1RenderEnd, _renderers.FTIT1, _forms.FTIT1Form, () => Settings.Default.FTIT1_RotateFlipType, () => Settings.Default.FTIT1_Monochrome, RenderInstrumentImage);
            _ftit2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ftit2RenderStart, _ftit2RenderEnd, _renderers.FTIT2, _forms.FTIT2Form, () => Settings.Default.FTIT2_RotateFlipType, () => Settings.Default.FTIT2_Monochrome, RenderInstrumentImage);
            _fuelFlowRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _fuelFlowRenderStart, _fuelFlowRenderEnd, _renderers.FuelFlow, _forms.FuelFlowForm, () => Settings.Default.FuelFlow_RotateFlipType, () => Settings.Default.FuelFlow_Monochrome, RenderInstrumentImage);
            _isisRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _isisRenderStart, _isisRenderEnd, _renderers.ISIS, _forms.ISISForm, () => Settings.Default.ISIS_RotateFlipType, () => Settings.Default.ISIS_Monochrome, RenderInstrumentImage);
            _fuelQuantityRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _fuelQuantityRenderStart, _fuelQuantityRenderEnd, _renderers.FuelQuantity, _forms.FuelQuantityForm, () => Settings.Default.FuelQuantity_RotateFlipType, () => Settings.Default.FuelQuantity_Monochrome, RenderInstrumentImage);
            _hsiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hsiRenderStart, _hsiRenderEnd, _renderers.HSI, _forms.HSIForm, () => Settings.Default.HSI_RotateFlipType, () => Settings.Default.HSI_Monochrome, RenderInstrumentImage);
            _ehsiRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _ehsiRenderStart, _ehsiRenderEnd, _renderers.EHSI, _forms.EHSIForm, () => Settings.Default.EHSI_RotateFlipType, () => Settings.Default.EHSI_Monochrome, RenderInstrumentImage);
            _landingGearLightsRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _landingGearLightsRenderStart, _landingGearLightsRenderEnd, _renderers.LandingGearLights, _forms.LandingGearLightsForm, () => Settings.Default.GearLights_RotateFlipType, () => Settings.Default.GearLights_Monochrome, RenderInstrumentImage);
            _nwsIndexerRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nwsIndexerRenderStart, _nwsIndexerRenderEnd, _renderers.NWSIndexer, _forms.NWSIndexerForm, () => Settings.Default.NWSIndexer_RotateFlipType, () => Settings.Default.NWSIndexer_Monochrome, RenderInstrumentImage);
            _nozPos1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nozPos1RenderStart, _nozPos1RenderEnd, _renderers.NOZ1, _forms.NOZPos1Form, () => Settings.Default.NOZ1_RotateFlipType, () => Settings.Default.NOZ1_Monochrome, RenderInstrumentImage);
            _nozPos2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _nozPos2RenderStart, _nozPos2RenderEnd, _renderers.NOZ2, _forms.NOZPos2Form, () => Settings.Default.NOZ2_RotateFlipType, () => Settings.Default.NOZ2_Monochrome, RenderInstrumentImage);
            _oilGauge1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _oilGauge1RenderStart, _oilGauge1RenderEnd, _renderers.OIL1, _forms.OILGauge1Form, () => Settings.Default.OIL1_RotateFlipType, () => Settings.Default.OIL1_Monochrome, RenderInstrumentImage);
            _oilGauge2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _oilGauge2RenderStart, _oilGauge2RenderEnd, _renderers.OIL2, _forms.OILGauge2Form, () => Settings.Default.OIL2_RotateFlipType, () => Settings.Default.OIL2_Monochrome, RenderInstrumentImage);
            _rwrRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rwrRenderStart, _rwrRenderEnd, _renderers.RWR, _forms.RWRForm, () => Settings.Default.RWR_RotateFlipType, () => Settings.Default.RWR_Monochrome, RenderInstrumentImage);
            _speedbrakeRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _speedbrakeRenderStart, _speedbrakeRenderEnd, _renderers.Speedbrake, _forms.SpeedbrakeForm, () => Settings.Default.Speedbrake_RotateFlipType, () => Settings.Default.Speedbrake_Monochrome, RenderInstrumentImage);
            _rpm1RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rpm1RenderStart, _rpm1RenderEnd, _renderers.RPM1, _forms.RPM1Form, () => Settings.Default.RPM1_RotateFlipType, () => Settings.Default.RPM1_Monochrome, RenderInstrumentImage);
            _rpm2RenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rpm2RenderStart, _rpm2RenderEnd, _renderers.RPM2, _forms.RPM2Form, () => Settings.Default.RPM2_RotateFlipType, () => Settings.Default.RPM2_Monochrome, RenderInstrumentImage);
            _vviRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _vviRenderStart, _vviRenderEnd, _renderers.VVI, _forms.VVIForm, () => Settings.Default.VVI_RotateFlipType, () => Settings.Default.VVI_Monochrome, RenderInstrumentImage);
            _hydARenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hydARenderStart, _hydARenderEnd, _renderers.HYDA, _forms.HydAForm, () => Settings.Default.HYDA_RotateFlipType, () => Settings.Default.HYDA_Monochrome, RenderInstrumentImage);
            _hydBRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _hydBRenderStart, _hydBRenderEnd, _renderers.HYDB, _forms.HydBForm, () => Settings.Default.HYDB_RotateFlipType, () => Settings.Default.HYDB_Monochrome, RenderInstrumentImage);
            _cabinPressRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _cabinPressRenderStart, _cabinPressRenderEnd, _renderers.CabinPress, _forms.CabinPressForm, () => Settings.Default.CabinPress_RotateFlipType, () => Settings.Default.CabinPress_Monochrome, RenderInstrumentImage);
            _rollTrimRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _rollTrimRenderStart, _rollTrimRenderEnd, _renderers.RollTrim, _forms.RollTrimForm, () => Settings.Default.RollTrim_RotateFlipType, () => Settings.Default.RollTrim_Monochrome, RenderInstrumentImage);
            _pitchTrimRenderThreadWorkHelper = new RenderThreadWorkHelper(() => _keepRunning, _pitchTrimRenderStart, _pitchTrimRenderEnd, _renderers.PitchTrim, _forms.PitchTrimForm, () => Settings.Default.PitchTrim_RotateFlipType, () => Settings.Default.PitchTrim_Monochrome, RenderInstrumentImage);

        }
        
        
        public void Start()
        {
            if (_running)
            {
                return;
            }
            KeyFileUtils.ResetCurrentKeyFile();
            if (Starting != null)
            {
                Starting.Invoke(this, new EventArgs());
            }
	        _keySettings = _keySettingsReader.Read();
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged += _mediatorEventHandler.HandleStateChange;
            }
            
            RunThreads();
        }
        public void Stop()
        {
            if (Stopping != null)
            {
                Stopping.Invoke(this, new EventArgs());
            }
            _keepRunning = false;
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged -= _mediatorEventHandler.HandleStateChange;
            }

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

            CloseOutputWindowForms();
            if (_networkMode == NetworkMode.Server)
            {
                TearDownImageServer();
            }
            CloseAndDisposeSharedmemReaders();
            _running = false;
            if (Stopped != null)
            {
                Stopped.Invoke(this, new EventArgs());
            }
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
            var settings = Settings.Default;
	        _keySettings = _keySettingsReader.Read();
            _gdiPlusOptions= _gdiPlusOptionsReader.Read();
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


            _threadPriority = settings.ThreadPriority;
            _compressionType = settings.CompressionType;
            _imageFormat = settings.NetworkImageFormat;
            if (DataChanged != null)
            {
                DataChanged.Invoke(null, new EventArgs());
            }
        }

        #region Public Properties

        
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
		public ExtractorState State { get; set; }
        public Mediator Mediator { get; set; }

        #endregion

        #region Networking Support

        #region Basic Network Client/Server Setup Code

        private void SetupNetworking()
        {
            if (_networkMode == NetworkMode.Client)
            {
                SetupNetworkingClient();
            }
            if (_networkMode == NetworkMode.Server)
            {
                SetupNetworkingServer();
            }
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

        

        #endregion

       

        #region Instrument Renderer Setup


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
                        var falconDataFormat = F4Utils.Process.Util.DetectFalconFormat();

                        //set automatic 3D mode for BMS
                        if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.BMS4) _threeDeeMode = true;

                        bool doMore = true;
                        if (_falconSmReader == null)
                        {
                            _falconSmReader = falconDataFormat.HasValue ? new Reader(falconDataFormat.Value) : new Reader();
                        }
                        else
                        {
                            if (falconDataFormat.HasValue)
                            {
                                if (falconDataFormat.Value != _falconSmReader.DataFormat)
                                {
                                    _falconSmReader = new Reader(falconDataFormat.Value);
                                }
                            }
                            else
                            {
                                doMore = false;
                                Common.Util.DisposeObject(_falconSmReader);
                                _falconSmReader = null;
                                _useBMSAdvancedSharedmemValues = false;
                            }
                        }
                        
                        if (doMore)
                        {
                            toReturn = _falconSmReader.GetCurrentData();

                            bool computeRalt = Settings.Default.EnableISISOutput || NetworkMode == NetworkMode.Server;
                            if (computeRalt)
                            {
                                ComputeRadarAltitude(toReturn);
                            }
                        }
                    }
                    else if (_networkMode == NetworkMode.Client)
                    {
	                    toReturn = _client.GetFlightData();
                    }
                }
            }
            if (toReturn != null) return toReturn;
            toReturn = new FlightData {hsiBits = Int32.MaxValue};
            return toReturn;
        }

	    private void ComputeRadarAltitude(FlightData toReturn)
	    {
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
	                        toReturn = _client.GetMfd4Bitmap();
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
	                        toReturn = _client.GetMfd3Bitmap();
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
	                        toReturn = _client.GetLeftMfdBitmap();
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
	                        toReturn = _client.GetRightMfdBitmap();
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
	                        toReturn = _client.GetHudBitmap();
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
                    ExtractorServer.SetFlightData(flightData);
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
                    ExtractorServer.SetHudBitmap(hudImage);
                }
                if (_forms.HUDForm != null)
                {
                    if (Settings.Default.HUD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        hudImage.RotateFlip(Settings.Default.HUD_RotateFlipType);
                    }
                    using (var graphics = _forms.HUDForm.CreateGraphics())
                    {
                        if (Settings.Default.HUD_Monochrome)
                        {
                            var ia = new ImageAttributes();
							ia.SetColorMatrix(Util.GreyscaleColorMatrix);
                            using (var compatible = Util.CopyBitmap(hudImage))
                            {
                                graphics.DrawImage(compatible, _forms.HUDForm.ClientRectangle, 0, 0, hudImage.Width,
                                                   hudImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(hudImage, _forms.HUDForm.ClientRectangle);
                        }
                    }
                }
                Common.Util.DisposeObject(hudImage);
            }
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
                    ExtractorServer.SetMfd4Bitmap(mfd4Image);
                }
                if (_forms.MFD4Form != null)
                {
                    if (Settings.Default.MFD4_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd4Image.RotateFlip(Settings.Default.MFD4_RotateFlipType);
                    }
                    using (Graphics graphics = _forms.MFD4Form.CreateGraphics())
                    {
                        if (Settings.Default.MFD4_Monochrome)
                        {
                            var ia = new ImageAttributes();
							ia.SetColorMatrix(Util.GreyscaleColorMatrix);
                            using (Image compatible = Util.CopyBitmap(mfd4Image))
                            {
                                graphics.DrawImage(compatible, _forms.MFD4Form.ClientRectangle, 0, 0, mfd4Image.Width,
                                                   mfd4Image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(mfd4Image, _forms.MFD4Form.ClientRectangle);
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
                    ExtractorServer.SetMfd3Bitmap(mfd3Image);
                }
                if (_forms.MFD3Form != null)
                {
                    if (Settings.Default.MFD3_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd3Image.RotateFlip(Settings.Default.MFD3_RotateFlipType);
                    }
                    using (Graphics graphics = _forms.MFD3Form.CreateGraphics())
                    {
                        if (Settings.Default.MFD3_Monochrome)
                        {
                            var ia = new ImageAttributes();
							ia.SetColorMatrix(Util.GreyscaleColorMatrix);
                            using (Image compatible = Util.CopyBitmap(mfd3Image))
                            {
                                graphics.DrawImage(compatible, _forms.MFD3Form.ClientRectangle, 0, 0, mfd3Image.Width,
                                                   mfd3Image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(mfd3Image, _forms.MFD3Form.ClientRectangle);
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
                    ExtractorServer.SetLeftMfdBitmap(leftMfdImage);
                }
                if (_forms.LeftMFDForm != null)
                {
                    if (Settings.Default.LMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        leftMfdImage.RotateFlip(Settings.Default.LMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _forms.LeftMFDForm.CreateGraphics())
                    {
                        if (Settings.Default.LMFD_Monochrome)
                        {
                            var ia = new ImageAttributes();
							ia.SetColorMatrix(Util.GreyscaleColorMatrix);
                            using (Image compatible = Util.CopyBitmap(leftMfdImage))
                            {
                                graphics.DrawImage(compatible, _forms.LeftMFDForm.ClientRectangle, 0, 0, leftMfdImage.Width,
                                                   leftMfdImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(leftMfdImage, _forms.LeftMFDForm.ClientRectangle);
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
                    ExtractorServer.SetRightMfdBitmap(rightMfdImage);
                }
                if (_forms.RightMfdForm != null)
                {
                    if (Settings.Default.RMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        rightMfdImage.RotateFlip(Settings.Default.RMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _forms.RightMfdForm.CreateGraphics())
                    {
                        if (Settings.Default.RMFD_Monochrome)
                        {
                            var ia = new ImageAttributes();
							ia.SetColorMatrix(Util.GreyscaleColorMatrix);
                            using (Image compatible = Util.CopyBitmap(rightMfdImage))
                            {
                                graphics.DrawImage(compatible, _forms.RightMfdForm.ClientRectangle, 0, 0, rightMfdImage.Width,
                                                   rightMfdImage.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            graphics.DrawImage(rightMfdImage, _forms.RightMfdForm.ClientRectangle);
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
            _forms.MFD4Form = SetupInstrumentForm("MFD4","MFD 4",null,_mfd4Form_Disposed,_mfd4BlankImage);
        }

        private void SetupMfd3Form()
        {
            _forms.MFD3Form = SetupInstrumentForm("MFD3","MFD 3",null,_mfd3Form_Disposed,_mfd3BlankImage);
        }

        private void SetupLeftMfdForm()
        {
            _forms.LeftMFDForm = SetupInstrumentForm("LMFD","Left MFD",null,_leftMfdForm_Disposed,_leftMfdBlankImage);
        }

        private void SetupRightMfdForm()
        {
            _forms.RightMfdForm = SetupInstrumentForm("RMFD","Right MFD",null,_rightMfdForm_Disposed,_rightMfdBlankImage);
        }

        private void SetupHudForm()
        {
            _forms.HUDForm = SetupInstrumentForm("HUD","HUD",null,_hudForm_Disposed,_hudBlankImage);
        }

        #endregion

        #region Instruments Forms Setup

        private void SetupVVIForm()
        {
            _forms.VVIForm = SetupInstrumentForm("VVI","VVI",_renderers.VVI,_vviForm_Disposed);
        }
        private InstrumentForm SetupInstrumentForm
        (
			string instrumentName,
            string formCaption,
            IInstrumentRenderer renderer,
            EventHandler disposeHandler,
            Image initialImage=null
        )
        {
	        var currentSettings = _instrumentFormSettingsReader.Read(instrumentName);
            if (!currentSettings.Enabled ) return null;
            Point location;
            Size size;
			var screen = Common.Screen.Util.FindScreen(currentSettings.OutputDisplay);
            var instrumentForm = new InstrumentForm { Text = formCaption, ShowInTaskbar = false, ShowIcon = false };
			if (currentSettings.StretchToFit)
            {
                location = new Point(0, 0);
                size = screen.Bounds.Size;
                instrumentForm.StretchToFill = true;
            }
            else
            {
				location = new Point(currentSettings.ULX, currentSettings.ULY);
				size = new Size(currentSettings.LRX - currentSettings.ULX, currentSettings.LRY - currentSettings.ULY);
                instrumentForm.StretchToFill = false;
            }
			instrumentForm.AlwaysOnTop = currentSettings.AlwaysOnTop;
			instrumentForm.Monochrome = currentSettings.Monochrome;
			instrumentForm.Rotation = currentSettings.RotateFlipType;
            instrumentForm.WindowState = FormWindowState.Normal;
            Common.Screen.Util.OpenFormOnSpecificMonitor(instrumentForm, _applicationForm, screen, location, size, true, true);
            instrumentForm.DataChanged += new InstrumentFormDataChangedHandler(instrumentName,instrumentForm,_extractor).HandleDataChangedEvent;

            instrumentForm.Disposed += disposeHandler;
            if (renderer != null)
            {
                _outputForms.Add(renderer, instrumentForm);
            }
            if (initialImage != null)
            {
                using (var graphics = instrumentForm.CreateGraphics())
                {
                    graphics.DrawImage(initialImage, instrumentForm.ClientRectangle);
                }
            }
	        return instrumentForm;
        }

        private void SetupRPM1Form()
        {
            _forms.RPM1Form = SetupInstrumentForm("RPM1","Engine 1 - RPM",_renderers.RPM1,_rpm1Form_Disposed);
        }

        private void SetupRPM2Form()
        {
            _forms.RPM2Form = SetupInstrumentForm("RPM2","Engine 2 - RPM",_renderers.RPM2,_rpm2Form_Disposed);
        }

        private void SetupSpeedbrakeForm()
        {
            _forms.SpeedbrakeForm = SetupInstrumentForm("Speedbrake","Speedbrake",_renderers.Speedbrake,_speedbrakeForm_Disposed);
        }

        private void SetupRWRForm()
        {
            _forms.RWRForm = SetupInstrumentForm("RWR","RWR",_renderers.RWR,_rwrForm_Disposed);
        }

        private void SetupOIL2Form()
        {
            _forms.OILGauge2Form = SetupInstrumentForm("OIL2","Engine 2 - Oil Pressure Indicator",_renderers.OIL2,_oilGauge2Form_Disposed);
        }

        private void SetupOIL1Form()
        {
            _forms.OILGauge1Form = SetupInstrumentForm("OIL1","Engine 1 - Oil Pressure Indicator",_renderers.OIL1,_oilGauge1Form_Disposed);
        }

        private void SetupNOZ2Form()
        {
            _forms.NOZPos2Form= SetupInstrumentForm("NOZ2","Engine 2 - Nozzle Position Indicator",_renderers.NOZ2,_nozPos2Form_Disposed);
        }

        private void SetupNOZ1Form()
        {
            _forms.NOZPos1Form = SetupInstrumentForm("NOZ1","Engine 1 - Nozzle Position Indicator",_renderers.NOZ1,_nozPos1Form_Disposed);
        }

        private void SetupNWSIndexerForm()
        {
            _forms.NWSIndexerForm = SetupInstrumentForm("NWSIndexer","NWS Indexer",_renderers.NWSIndexer,_nwsIndexerForm_Disposed);
        }

        private void SetupGearLightsForm()
        {
            _forms.LandingGearLightsForm = SetupInstrumentForm("GearLights", "Landing Gear Lights",_renderers.LandingGearLights,_landingGearLightsForm_Disposed);
        }

        private void SetupHSIForm()
        {
            _forms.HSIForm = SetupInstrumentForm("HSI", "Horizontal Situation Indicator",_renderers.HSI,_hsiForm_Disposed);
        }

        private void SetupEHSIForm()
        {
            _forms.EHSIForm = SetupInstrumentForm("EHSI","EHSI",_renderers.EHSI,_ehsiForm_Disposed);
        }

        private void SetupFuelQuantityForm()
        {
            _forms.FuelQuantityForm = SetupInstrumentForm("FuelQuantity","Fuel Quantity",_renderers.FuelQuantity,FuelQuantityForm_Disposed);
        }

        private void SetupFuelFlowForm()
        {
            _forms.FuelFlowForm = SetupInstrumentForm("FuelFlow","Fuel Flow Indicator",_renderers.FuelFlow,_fuelFlowForm_Disposed);
        }

        private void SetupISISForm()
        {
            _forms.ISISForm = SetupInstrumentForm("ISIS","ISIS",_renderers.ISIS,_isisForm_Disposed);
        }

        private void SetupAccelerometerForm()
        {
            _forms.AccelerometerForm = SetupInstrumentForm("Accelerometer","Accelerometer",_renderers.Accelerometer,_accelerometerForm_Disposed);
        }

        private void SetupFTIT2Form()
        {
            _forms.FTIT2Form = SetupInstrumentForm("FTIT2","FTIT 2",_renderers.FTIT2,_ftit2Form_Disposed);
        }

        private void SetupFTIT1Form()
        {
            _forms.FTIT1Form = SetupInstrumentForm("FTIT1","FTIT 1",_renderers.FTIT1,_ftit1Form_Disposed);
        }

        private void SetupEPUFuelForm()
        {
            _forms.EPUFuelForm = SetupInstrumentForm("EPUFuel","EPU Fuel",_renderers.EPUFuel,_epuFuelForm_Disposed);
        }

        private void SetupPFLForm()
        {
            _forms.PFLForm = SetupInstrumentForm("PFL","PFL",_renderers.PFL,_pflForm_Disposed);
        }

        private void SetupDEDForm()
        {
            _forms.DEDForm = SetupInstrumentForm("DED","DED",_renderers.DED,_dedForm_Disposed);
        }

        private void SetupCompassForm()
        {
            _forms.CompassForm = SetupInstrumentForm("Compass","Compass",_renderers.Compass,_compassForm_Disposed);
        }

        private void SetupCMDSPanelForm()
        {
            _forms.CMDSPanelForm = SetupInstrumentForm("CMDS", "CMDS", _renderers.CMDSPanel,_cmdsPanelForm_Disposed);
        }

        private void SetupCautionPanelForm()
        {
            _forms.CautionPanelForm = SetupInstrumentForm("CautionPanel", "Caution Panel",_renderers.CautionPanel,_cautionPanelForm_Disposed);
        }

        private void SetupAOAIndicatorForm()
        {
            _forms.AOAIndicatorForm = SetupInstrumentForm("AOAIndicator","AOA Indicator",_renderers.AOAIndicator,AOAIndicatorForm_Disposed);
        }

		private void SetupAOAIndexerForm()
        {
            _forms.AOAIndexerForm = SetupInstrumentForm("AOAIndexer","AOA Indexer",_renderers.AOAIndexer,AOAIndexerForm_Disposed);
        }

        private void SetupAltimeterForm()
        {
            _forms.AltimeterForm = SetupInstrumentForm("Altimeter","Altimeter",_renderers.Altimeter,_altimeterForm_Disposed);
        }

        private void SetupCabinPressForm()
        {
            _forms.CabinPressForm = SetupInstrumentForm("CabinPress","Cabin Pressure Indicator",_renderers.CabinPress,_cabinPressForm_Disposed);
        }

        private void SetupRollTrimForm()
        {
            _forms.RollTrimForm = SetupInstrumentForm("RollTrim","Roll Trim Indicator",_renderers.RollTrim,_rollTrimForm_Disposed);
        }

        private void SetupPitchTrimForm()
        {
            _forms.PitchTrimForm = SetupInstrumentForm("PitchTrim","Pitch Trim Indicator",_renderers.PitchTrim,_pitchTrimForm_Disposed);
        }

        private void SetupHydAForm()
        {
            _forms.HydAForm = SetupInstrumentForm("HYDA","Hydraulic Pressure Indicator A",_renderers.HYDA,_hydAForm_Disposed);
        }

        private void SetupHydBForm()
        {
            _forms.HydBForm = SetupInstrumentForm("HYDB", "Hydraulic Pressure Indicator B",_renderers.HYDB,_hydBForm_Disposed);
        }

        private void SetupASIForm()
        {
            _forms.ASIForm = SetupInstrumentForm("ASI", "Airspeed Indicator",_renderers.ASI,_asiForm_Disposed);
        }

        private void SetupADIForm()
        {
            _forms.ADIForm = SetupInstrumentForm("ADI","Attitude Indicator",_renderers.ADI,_adiForm_Disposed);
        }

        private void SetupBackupADIForm()
        {
            _forms.BackupAdiForm = SetupInstrumentForm("BackupADI","Standby Attitude Indicator",_renderers.BackupADI,_backupAdiForm_Disposed);
        }

        #endregion

        #endregion

        #region Instrument Form Disposal Event handlers

        private void _vviForm_Disposed(object sender, EventArgs e)
        {
            _forms.VVIForm = null;
        }

        private void _rpm2Form_Disposed(object sender, EventArgs e)
        {
            _forms.RPM2Form = null;
        }

        private void _rpm1Form_Disposed(object sender, EventArgs e)
        {
            _forms.RPM1Form = null;
        }

        private void _speedbrakeForm_Disposed(object sender, EventArgs e)
        {
            _forms.SpeedbrakeForm = null;
        }

        private void _rwrForm_Disposed(object sender, EventArgs e)
        {
            _forms.RWRForm = null;
        }

        private void _landingGearLightsForm_Disposed(object sender, EventArgs e)
        {
            _forms.LandingGearLightsForm = null;
        }

        private void _oilGauge1Form_Disposed(object sender, EventArgs e)
        {
            _forms.OILGauge1Form = null;
        }

        private void _oilGauge2Form_Disposed(object sender, EventArgs e)
        {
            _forms.OILGauge2Form = null;
        }

        private void _nozPos1Form_Disposed(object sender, EventArgs e)
        {
            _forms.NOZPos1Form = null;
        }

        private void _nozPos2Form_Disposed(object sender, EventArgs e)
        {
            _forms.NOZPos2Form = null;
        }

        private void _nwsIndexerForm_Disposed(object sender, EventArgs e)
        {
            _forms.NWSIndexerForm = null;
        }

        private void _hsiForm_Disposed(object sender, EventArgs e)
        {
            _forms.HSIForm = null;
        }

        private void _ehsiForm_Disposed(object sender, EventArgs e)
        {
            _forms.EHSIForm = null;
        }

        private void FuelQuantityForm_Disposed(object sender, EventArgs e)
        {
            _forms.FuelQuantityForm = null;
        }

        private void _fuelFlowForm_Disposed(object sender, EventArgs e)
        {
            _forms.FuelFlowForm = null;
        }

        private void _isisForm_Disposed(object sender, EventArgs e)
        {
            _forms.ISISForm = null;
        }

        private void _accelerometerForm_Disposed(object sender, EventArgs e)
        {
            _forms.AccelerometerForm = null;
        }
        private void _ftit2Form_Disposed(object sender, EventArgs e)
        {
            _forms.FTIT2Form = null;
        }

        private void _ftit1Form_Disposed(object sender, EventArgs e)
        {
            _forms.FTIT1Form = null;
        }

        private void _epuFuelForm_Disposed(object sender, EventArgs e)
        {
            _forms.EPUFuelForm = null;
        }

        private void _pflForm_Disposed(object sender, EventArgs e)
        {
            _forms.PFLForm = null;
        }

        private void _dedForm_Disposed(object sender, EventArgs e)
        {
            _forms.DEDForm = null;
        }

        private void _compassForm_Disposed(object sender, EventArgs e)
        {
            _forms.CompassForm = null;
        }

        private void _cmdsPanelForm_Disposed(object sender, EventArgs e)
        {
            _forms.CMDSPanelForm = null;
        }

        private void _cautionPanelForm_Disposed(object sender, EventArgs e)
        {
            _forms.CautionPanelForm = null;
        }

        private void AOAIndicatorForm_Disposed(object sender, EventArgs e)
        {
            _forms.AOAIndicatorForm = null;
        }

        private void AOAIndexerForm_Disposed(object sender, EventArgs e)
        {
            _forms.AOAIndexerForm = null;
        }

        private void _altimeterForm_Disposed(object sender, EventArgs e)
        {
            _forms.AltimeterForm = null;
        }

        private void _asiForm_Disposed(object sender, EventArgs e)
        {
            _forms.ASIForm = null;
        }

        private void _adiForm_Disposed(object sender, EventArgs e)
        {
            _forms.ADIForm = null;
        }

        private void _backupAdiForm_Disposed(object sender, EventArgs e)
        {
            _forms.BackupAdiForm = null;
        }

        private void _hydAForm_Disposed(object sender, EventArgs e)
        {
            _forms.HydAForm = null;
        }

        private void _hydBForm_Disposed(object sender, EventArgs e)
        {
            _forms.HydBForm = null;
        }

        private void _cabinPressForm_Disposed(object sender, EventArgs e)
        {
            _forms.CabinPressForm = null;
        }

        private void _rollTrimForm_Disposed(object sender, EventArgs e)
        {
            _forms.RollTrimForm = null;
        }

        private void _pitchTrimForm_Disposed(object sender, EventArgs e)
        {
            _forms.PitchTrimForm = null;
        }

        private void _mfd4Form_Disposed(object sender, EventArgs e)
        {
            _forms.MFD4Form = null;
        }

        private void _mfd3Form_Disposed(object sender, EventArgs e)
        {
            _forms.MFD3Form = null;
        }

        private void _leftMfdForm_Disposed(object sender, EventArgs e)
        {
            _forms.LeftMFDForm = null;
        }

        private void _rightMfdForm_Disposed(object sender, EventArgs e)
        {
            _forms.RightMfdForm = null;
        }

        private void _hudForm_Disposed(object sender, EventArgs e)
        {
            _forms.HUDForm = null;
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
            RecoverInstrumentForm(_forms.ADIForm, screen);
        }

        public void RecoverBackupADIWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.BackupAdiForm, screen);
        }

        public void RecoverASIWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.ASIForm, screen);
        }

        public void RecoverCabinPressWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.CabinPressForm, screen);
        }

        public void RecoverRollTrimWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.RollTrimForm, screen);
        }

        public void RecoverPitchTrimWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.PitchTrimForm, screen);
        }

        public void RecoverAltimeterWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.AltimeterForm, screen);
        }

        public void RecoverAOAIndexerWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.AOAIndexerForm, screen);
        }

        public void RecoverAOAIndicatorWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.AOAIndicatorForm, screen);
        }

        public void RecoverCautionPanelWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.CautionPanelForm, screen);
        }

        public void RecoverCMDSPanelWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.CMDSPanelForm, screen);
        }

        public void RecoverCompassWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.CompassForm, screen);
        }

        public void RecoverDEDWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.DEDForm, screen);
        }

        public void RecoverPFLWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.PFLForm, screen);
        }

        public void RecoverEPUFuelWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.EPUFuelForm, screen);
        }

        public void RecoverFTIT1Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.FTIT1Form, screen);
        }

        public void RecoverAccelerometerWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.AccelerometerForm, screen);
        }

        public void RecoverFTIT2Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.FTIT2Form, screen);
        }

        public void RecoverFuelFlowWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.FuelFlowForm, screen);
        }

        public void RecoverISISWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.ISISForm, screen);
        }

        public void RecoverFuelQuantityWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.FuelQuantityForm, screen);
        }

        public void RecoverHSIWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.HSIForm, screen);
        }

        public void RecoverEHSIWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.EHSIForm, screen);
        }

        public void RecoverLandingGearLightsWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.LandingGearLightsForm, screen);
        }

        public void RecoverNWSWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.NWSIndexerForm, screen);
        }

        public void RecoverNOZ1Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.NOZPos1Form, screen);
        }

        public void RecoverNOZ2Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.NOZPos2Form, screen);
        }

        public void RecoverOil1Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.OILGauge1Form, screen);
        }

        public void RecoverOil2Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.OILGauge2Form, screen);
        }

        public void RecoverAzimuthIndicatorWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.RWRForm, screen);
        }

        public void RecoverSpeedbrakeWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.SpeedbrakeForm, screen);
        }

        public void RecoverRPM1Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.RPM1Form, screen);
        }

        public void RecoverRPM2Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.RPM2Form, screen);
        }

        public void RecoverVVIWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.VVIForm, screen);
        }

        public void RecoverMfd4Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.MFD4Form, screen);
        }

        public void RecoverMfd3Window(Screen screen)
        {
            RecoverInstrumentForm(_forms.MFD3Form, screen);
        }

        public void RecoverLeftMfdWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.LeftMFDForm, screen);
        }

        public void RecoverRightMfdWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.RightMfdForm, screen);
        }

        public void RecoverHudWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.HUDForm, screen);
        }

        public void RecoverHydAWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.HydAForm, screen);
        }

        public void RecoverHydBWindow(Screen screen)
        {
            RecoverInstrumentForm(_forms.HydBForm, screen);
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
            CloseOutputWindowForm(_forms.ADIForm);
            CloseOutputWindowForm(_forms.BackupAdiForm);
            CloseOutputWindowForm(_forms.ASIForm);
            CloseOutputWindowForm(_forms.AltimeterForm);
            CloseOutputWindowForm(_forms.AOAIndexerForm);
            CloseOutputWindowForm(_forms.AOAIndicatorForm);
            CloseOutputWindowForm(_forms.CautionPanelForm);
            CloseOutputWindowForm(_forms.CMDSPanelForm);
            CloseOutputWindowForm(_forms.CompassForm);
            CloseOutputWindowForm(_forms.DEDForm);
            CloseOutputWindowForm(_forms.PFLForm);
            CloseOutputWindowForm(_forms.EPUFuelForm);
            CloseOutputWindowForm(_forms.AccelerometerForm);
            CloseOutputWindowForm(_forms.FTIT1Form);
            CloseOutputWindowForm(_forms.FTIT2Form);
            CloseOutputWindowForm(_forms.FuelFlowForm);
            CloseOutputWindowForm(_forms.ISISForm);
            CloseOutputWindowForm(_forms.FuelQuantityForm);
            CloseOutputWindowForm(_forms.HSIForm);
            CloseOutputWindowForm(_forms.EHSIForm);
            CloseOutputWindowForm(_forms.LandingGearLightsForm);
            CloseOutputWindowForm(_forms.NWSIndexerForm);
            CloseOutputWindowForm(_forms.NOZPos1Form);
            CloseOutputWindowForm(_forms.NOZPos2Form);
            CloseOutputWindowForm(_forms.OILGauge1Form);
            CloseOutputWindowForm(_forms.OILGauge2Form);
            CloseOutputWindowForm(_forms.RWRForm);
            CloseOutputWindowForm(_forms.SpeedbrakeForm);
            CloseOutputWindowForm(_forms.RPM1Form);
            CloseOutputWindowForm(_forms.RPM2Form);
            CloseOutputWindowForm(_forms.VVIForm);
            CloseOutputWindowForm(_forms.HydAForm);
            CloseOutputWindowForm(_forms.HydBForm);
            CloseOutputWindowForm(_forms.CabinPressForm);
            CloseOutputWindowForm(_forms.RollTrimForm);
            CloseOutputWindowForm(_forms.PitchTrimForm);


            CloseOutputWindowForm(_forms.MFD4Form);
            CloseOutputWindowForm(_forms.MFD3Form);
            CloseOutputWindowForm(_forms.LeftMFDForm);
            CloseOutputWindowForm(_forms.RightMfdForm);
            CloseOutputWindowForm(_forms.HUDForm);
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
                _rendererSetInitializer.Initialize(_gdiPlusOptions);
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

       

        private void CaptureOrchestrationThreadWork()
        {
            var toWait = new List<WaitHandle>();
            try
            {
                while (_keepRunning)
                {
                    _windowSizingOrMoving = WindowSizingOrMovingBeingAttemptedOnAnyOutputWindow();
                    Application.DoEvents();
                    if (_renderCycleNum < long.MaxValue)
                    {
                        _renderCycleNum++;
                    }
                    else
                    {
                        _renderCycleNum = 0;
                    }
                    var thisLoopStartTime = DateTime.Now;
                    var setNullImages = true;

                    if (NetworkMode == NetworkMode.Client)
                    {
						_clientSideIncomingMessageDispatcher.ProcessPendingMessages();
                    }
                    else if (NetworkMode == NetworkMode.Server)
                    {
                        _serverSideIncomingMessageDispatcher.ProcessPendingMessages();
                    }


                    if (_simRunning || _testMode || NetworkMode == NetworkMode.Client)
                    {
                        var currentFlightData = GetFlightData();
                        SetFlightData(currentFlightData);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, currentFlightData, _simRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _networkMode);
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
                        var flightDataToSet = new FlightData();
                        flightDataToSet.hsiBits = Int32.MaxValue;
                        SetFlightData(flightDataToSet);
						_flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, flightDataToSet, _simRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _networkMode);
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
                    if ((!_simRunning && _networkMode != NetworkMode.Client) && !_testMode)
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RWR)) ||
                    (_forms.RWRForm != null && _forms.RWRForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RWR_RenderEveryN == Settings.Default.RWR_RenderOnN - 1) ||
                        (_forms.RWRForm != null && _forms.RWRForm.RenderImmediately))
                    {
                        if (_forms.RWRForm != null)
                        {
                            _forms.RWRForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CMDSPanel)) ||
                    (_forms.CMDSPanelForm != null && _forms.CMDSPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CMDS_RenderEveryN == Settings.Default.CMDS_RenderOnN - 1) ||
                        (_forms.CMDSPanelForm != null && _forms.CMDSPanelForm.RenderImmediately))
                    {
                        if (_forms.CMDSPanelForm != null)
                        {
                            _forms.CMDSPanelForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CautionPanel)) ||
                    (_forms.CautionPanelForm != null && _forms.CautionPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CautionPanel_RenderEveryN ==
                         Settings.Default.CautionPanel_RenderOnN - 1) ||
                        (_forms.CautionPanelForm != null && _forms.CautionPanelForm.RenderImmediately))
                    {
                        if (_forms.CautionPanelForm != null)
                        {
                            _forms.CautionPanelForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.LandingGearLights)) ||
                    (_forms.LandingGearLightsForm != null && _forms.LandingGearLightsForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.GearLights_RenderEveryN ==
                         Settings.Default.GearLights_RenderOnN - 1) ||
                        (_forms.LandingGearLightsForm != null && _forms.LandingGearLightsForm.RenderImmediately))
                    {
                        if (_forms.LandingGearLightsForm != null)
                        {
                            _forms.LandingGearLightsForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Speedbrake)) ||
                    (_forms.SpeedbrakeForm != null && _forms.SpeedbrakeForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Speedbrake_RenderEveryN ==
                         Settings.Default.Speedbrake_RenderOnN - 1) ||
                        (_forms.SpeedbrakeForm != null && _forms.SpeedbrakeForm.RenderImmediately))
                    {
                        if (_forms.SpeedbrakeForm != null)
                        {
                            _forms.SpeedbrakeForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.DED) ||
                     (_forms.DEDForm != null && _forms.DEDForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.DED_RenderEveryN == Settings.Default.DED_RenderOnN - 1) ||
                        (_forms.DEDForm != null && _forms.DEDForm.RenderImmediately))
                    {
                        if (_forms.DEDForm != null)
                        {
                            _forms.DEDForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.PFL)) ||
                    (_forms.PFLForm != null && _forms.PFLForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.PFL_RenderEveryN == Settings.Default.PFL_RenderOnN - 1) ||
                        (_forms.PFLForm != null && _forms.PFLForm.RenderImmediately))
                    {
                        if (_forms.PFLForm != null)
                        {
                            _forms.PFLForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FuelFlow)) ||
                    (_forms.FuelFlowForm != null && _forms.FuelFlowForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FuelFlow_RenderEveryN ==
                         Settings.Default.FuelFlow_RenderOnN - 1) ||
                        (_forms.FuelFlowForm != null && _forms.FuelFlowForm.RenderImmediately))
                    {
                        if (_forms.FuelFlowForm != null)
                        {
                            _forms.FuelFlowForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FuelQuantity)) ||
                    (_forms.FuelQuantityForm != null && _forms.FuelQuantityForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FuelQuantity_RenderEveryN ==
                         Settings.Default.FuelQuantity_RenderOnN - 1) ||
                        (_forms.FuelQuantityForm != null && _forms.FuelQuantityForm.RenderImmediately))
                    {
                        if (_forms.FuelQuantityForm != null)
                        {
                            _forms.FuelQuantityForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.EPUFuel)) ||
                    (_forms.EPUFuelForm != null && _forms.EPUFuelForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.EPUFuel_RenderEveryN == Settings.Default.EPUFuel_RenderOnN - 1) ||
                        (_forms.EPUFuelForm != null && _forms.EPUFuelForm.RenderImmediately))
                    {
                        if (_forms.EPUFuelForm != null)
                        {
                            _forms.EPUFuelForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.AOAIndexer)) ||
                    (_forms.AOAIndexerForm != null && _forms.AOAIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.AOAIndexer_RenderEveryN ==
                         Settings.Default.AOAIndexer_RenderOnN - 1) ||
                        (_forms.AOAIndexerForm != null && _forms.AOAIndexerForm.RenderImmediately))
                    {
                        if (_forms.AOAIndexerForm != null)
                        {
                            _forms.AOAIndexerForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NWSIndexer)) ||
                    (_forms.NWSIndexerForm != null && _forms.NWSIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NWSIndexer_RenderEveryN ==
                         Settings.Default.NWSIndexer_RenderOnN - 1) ||
                        (_forms.NWSIndexerForm != null && _forms.NWSIndexerForm.RenderImmediately))
                    {
                        if (_forms.NWSIndexerForm != null)
                        {
                            _forms.NWSIndexerForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FTIT1)) ||
                    (_forms.FTIT1Form != null && _forms.FTIT1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FTIT1_RenderEveryN == Settings.Default.FTIT1_RenderOnN - 1) ||
                        (_forms.FTIT1Form != null && _forms.FTIT1Form.RenderImmediately))
                    {
                        if (_forms.FTIT1Form != null)
                        {
                            _forms.FTIT1Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NOZ1)) ||
                    (_forms.NOZPos1Form != null && _forms.NOZPos1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NOZ1_RenderEveryN == Settings.Default.NOZ1_RenderOnN - 1) ||
                        (_forms.NOZPos1Form != null && _forms.NOZPos1Form.RenderImmediately))
                    {
                        if (_forms.NOZPos1Form != null)
                        {
                            _forms.NOZPos1Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.OIL1)) ||
                    (_forms.OILGauge1Form != null && _forms.OILGauge1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.OIL1_RenderEveryN == Settings.Default.OIL1_RenderOnN - 1) ||
                        (_forms.OILGauge1Form != null && _forms.OILGauge1Form.RenderImmediately))
                    {
                        if (_forms.OILGauge1Form != null)
                        {
                            _forms.OILGauge1Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RPM1)) ||
                    (_forms.RPM1Form != null && _forms.RPM1Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RPM1_RenderEveryN == Settings.Default.RPM1_RenderOnN - 1) ||
                        (_forms.RPM1Form != null && _forms.RPM1Form.RenderImmediately))
                    {
                        if (_forms.RPM1Form != null)
                        {
                            _forms.RPM1Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FTIT2)) ||
                    (_forms.FTIT2Form != null && _forms.FTIT2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.FTIT2_RenderEveryN == Settings.Default.FTIT2_RenderOnN - 1) ||
                        (_forms.FTIT2Form != null && _forms.FTIT2Form.RenderImmediately))
                    {
                        if (_forms.FTIT2Form != null)
                        {
                            _forms.FTIT2Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NOZ2)) ||
                    (_forms.NOZPos2Form != null && _forms.NOZPos2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.NOZ2_RenderEveryN == Settings.Default.NOZ2_RenderOnN - 1) ||
                        (_forms.NOZPos2Form != null && _forms.NOZPos2Form.RenderImmediately))
                    {
                        if (_forms.NOZPos2Form != null)
                        {
                            _forms.NOZPos2Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.OIL2)) ||
                    (_forms.OILGauge2Form != null && _forms.OILGauge2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.OIL2_RenderEveryN == Settings.Default.OIL2_RenderOnN - 1) ||
                        (_forms.OILGauge2Form != null && _forms.OILGauge2Form.RenderImmediately))
                    {
                        if (_forms.OILGauge2Form != null)
                        {
                            _forms.OILGauge2Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RPM2)) ||
                    (_forms.RPM2Form != null && _forms.RPM2Form.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RPM2_RenderEveryN == Settings.Default.RPM2_RenderOnN - 1) ||
                        (_forms.RPM2Form != null && _forms.RPM2Form.RenderImmediately))
                    {
                        if (_forms.RPM2Form != null)
                        {
                            _forms.RPM2Form.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HYDA)) ||
                    (_forms.HydAForm != null && _forms.HydAForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.HYDA_RenderEveryN == Settings.Default.HYDA_RenderOnN - 1) ||
                        (_forms.HydAForm != null && _forms.HydAForm.RenderImmediately))
                    {
                        if (_forms.HydAForm != null)
                        {
                            _forms.HydAForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HYDB)) ||
                    (_forms.HydBForm != null && _forms.HydBForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.HYDB_RenderEveryN == Settings.Default.HYDB_RenderOnN - 1) ||
                        (_forms.HydBForm != null && _forms.HydBForm.RenderImmediately))
                    {
                        if (_forms.HydBForm != null)
                        {
                            _forms.HydBForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CabinPress)) ||
                    (_forms.CabinPressForm != null && _forms.CabinPressForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.CabinPress_RenderEveryN ==
                         Settings.Default.CabinPress_RenderOnN - 1) ||
                        (_forms.CabinPressForm != null && _forms.CabinPressForm.RenderImmediately))
                    {
                        if (_forms.CabinPressForm != null)
                        {
                            _forms.CabinPressForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RollTrim)) ||
                    (_forms.RollTrimForm != null && _forms.RollTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.RollTrim_RenderEveryN ==
                         Settings.Default.RollTrim_RenderOnN - 1) ||
                        (_forms.RollTrimForm != null && _forms.RollTrimForm.RenderImmediately))
                    {
                        if (_forms.RollTrimForm != null)
                        {
                            _forms.RollTrimForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.PitchTrim)) ||
                    (_forms.PitchTrimForm != null && _forms.PitchTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.PitchTrim_RenderEveryN ==
                         Settings.Default.PitchTrim_RenderOnN - 1) ||
                        (_forms.PitchTrimForm != null && _forms.PitchTrimForm.RenderImmediately))
                    {
                        if (_forms.PitchTrimForm != null)
                        {
                            _forms.PitchTrimForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ADI) ||
                     (_forms.ADIForm != null && _forms.ADIForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.ADI_RenderEveryN == Settings.Default.ADI_RenderOnN - 1) ||
                        (_forms.ADIForm != null && _forms.ADIForm.RenderImmediately))
                    {
                        if (_forms.ADIForm != null)
                        {
                            _forms.ADIForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ISIS)) ||
                    (_forms.ISISForm != null && _forms.ISISForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.ISIS_RenderEveryN == Settings.Default.ISIS_RenderOnN - 1) ||
                        (_forms.ISISForm != null && _forms.ISISForm.RenderImmediately))
                    {
                        if (_forms.ISISForm != null)
                        {
                            _forms.ISISForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HSI) ||
                     (_forms.HSIForm != null && _forms.HSIForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.HSI_RenderEveryN == Settings.Default.HSI_RenderOnN - 1) ||
                        (_forms.HSIForm != null && _forms.HSIForm.RenderImmediately))
                    {
                        if (_forms.HSIForm != null)
                        {
                            _forms.HSIForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.EHSI)) ||
                    (_forms.EHSIForm != null && _forms.EHSIForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.EHSI_RenderEveryN == Settings.Default.EHSI_RenderOnN - 1) ||
                        (_forms.EHSIForm != null && _forms.EHSIForm.RenderImmediately))
                    {
                        if (_forms.EHSIForm != null)
                        {
                            _forms.EHSIForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Altimeter)) ||
                    (_forms.AltimeterForm != null && _forms.AltimeterForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Altimeter_RenderEveryN ==
                         Settings.Default.Altimeter_RenderOnN - 1) ||
                        (_forms.AltimeterForm != null && _forms.AltimeterForm.RenderImmediately))
                    {
                        if (_forms.AltimeterForm != null)
                        {
                            _forms.AltimeterForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ASI) ||
                     (_forms.ASIForm != null && _forms.ASIForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.ASI_RenderEveryN == Settings.Default.ASI_RenderOnN - 1) ||
                        (_forms.ASIForm != null && _forms.ASIForm.RenderImmediately))
                    {
                        if (_forms.ASIForm != null)
                        {
                            _forms.ASIForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.BackupADI)) ||
                    (_forms.BackupAdiForm != null && _forms.BackupAdiForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Backup_ADI_RenderEveryN ==
                         Settings.Default.Backup_ADI_RenderOnN - 1) ||
                        (_forms.BackupAdiForm != null && _forms.BackupAdiForm.RenderImmediately))
                    {
                        if (_forms.BackupAdiForm != null)
                        {
                            _forms.BackupAdiForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.VVI) ||
                     (_forms.VVIForm != null && _forms.VVIForm.RenderImmediately)))
                {
                    if ((_renderCycleNum%Settings.Default.VVI_RenderEveryN == Settings.Default.VVI_RenderOnN - 1) ||
                        (_forms.VVIForm != null && _forms.VVIForm.RenderImmediately))
                    {
                        if (_forms.VVIForm != null)
                        {
                            _forms.VVIForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.AOAIndicator)) ||
                    (_forms.AOAIndicatorForm != null && _forms.AOAIndicatorForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.AOAIndicator_RenderEveryN ==
                         Settings.Default.AOAIndicator_RenderOnN - 1) ||
                        (_forms.AOAIndicatorForm != null && _forms.AOAIndicatorForm.RenderImmediately))
                    {
                        if (_forms.AOAIndicatorForm != null)
                        {
                            _forms.AOAIndicatorForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Compass)) ||
                    (_forms.CompassForm != null && _forms.CompassForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Compass_RenderEveryN == Settings.Default.Compass_RenderOnN - 1) ||
                        (_forms.CompassForm != null && _forms.CompassForm.RenderImmediately))
                    {
                        if (_forms.CompassForm != null)
                        {
                            _forms.CompassForm.RenderImmediately = false;
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
                     IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Accelerometer)) ||
                    (_forms.AccelerometerForm != null && _forms.AccelerometerForm.RenderImmediately))
                {
                    if ((_renderCycleNum%Settings.Default.Accelerometer_RenderEveryN ==
                         Settings.Default.Accelerometer_RenderOnN - 1) ||
                        (_forms.AccelerometerForm != null && _forms.AccelerometerForm.RenderImmediately))
                    {
                        if (_forms.AccelerometerForm != null)
                        {
                            _forms.AccelerometerForm.RenderImmediately = false;
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
                var count = 0;

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

                            try
                            {
                                _simRunning = NetworkMode == NetworkMode.Client ||
                                              F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex.Message, ex);
                            }
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

            SetupRenderThreadWorkHelpers();
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
			_keyboardWatcherThread = new Thread(_keyboardWatcher.Start);
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


        private float GetIndicatedAltitude(float trueAltitude, float baroPressure, bool pressureInInchesOfMercury)
        {
            var baroPressureInchesOfMercury = baroPressure;
            if (!pressureInInchesOfMercury)
            {
                baroPressureInchesOfMercury = baroPressure/Constants.INCHES_MERCURY_TO_HECTOPASCALS;
            }
            var baroDifference = baroPressureInchesOfMercury - 29.92f;
            var baroChangePerThousandFeet = 1.08f;
            var altitudeCorrection = (baroDifference/baroChangePerThousandFeet)*1000.0f;
            return altitudeCorrection + trueAltitude;
        }

       

        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(IInstrumentRenderer renderer)
        {
            var instrumentForm = GetFormForRenderer(renderer);
	        var stateIsStale = _instrumentStateSnapshotCache.CaptureInstrumentStateSnapshotAndCheckIfStale(renderer, instrumentForm);
            return stateIsStale || HighlightingBorderShouldBeDisplayedOnTargetForm(instrumentForm);
        }


        private void RenderInstrumentImage(IInstrumentRenderer renderer, InstrumentForm targetForm,
                                           RotateFlipType rotation, bool monochrome)
        {
            var startTime = DateTime.Now;
            if (renderer == null || targetForm == null) return;
            if (DateTime.Now.Subtract(targetForm.LastRenderedOn).TotalMilliseconds < Settings.Default.PollingDelay)
            {
                return;
            }
            _instrumentRenderHelper.Render(renderer, targetForm, rotation, monochrome, HighlightingBorderShouldBeDisplayedOnTargetForm(targetForm), _nightMode);
            var endTime = DateTime.Now;
            var elapsed = endTime.Subtract(startTime);
            if (!(elapsed.TotalMilliseconds < MIN_RENDERER_PASS_TIME_MILLSECONDS)) return;
            var toWait = new TimeSpan(0, 0, 0, 0,
                (int) (MIN_RENDERER_PASS_TIME_MILLSECONDS - elapsed.TotalMilliseconds));
            if (toWait.TotalMilliseconds < MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER)
            {
                toWait = new TimeSpan(0, 0, 0, 0, MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER);
            }
            Thread.Sleep(toWait);
        }


	    private static bool HighlightingBorderShouldBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
			return targetForm.SizingOrMovingCursorsAreDisplayed  && Settings.Default.HighlightOutputWindows;
        }

        private static bool WindowSizingOrMovingBeingAttemptedOnAnyOutputWindow()
        {
            return MouseButtonDown && Application.OpenForms.OfType<InstrumentForm>().Any(x => x.Visible && x.SizingOrMovingCursorsAreDisplayed);
        }

	    private static bool MouseButtonDown
	    {
	        get 
            { 
                return 
                (
                    ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left) 
                    || 
                    ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                ); 
            }
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
                    Common.Util.DisposeObject(_forms.ASIForm);
                    Common.Util.DisposeObject(_forms.ADIForm);
                    Common.Util.DisposeObject(_forms.BackupAdiForm);
                    Common.Util.DisposeObject(_forms.AltimeterForm);
                    Common.Util.DisposeObject(_forms.AOAIndexerForm);
                    Common.Util.DisposeObject(_forms.AOAIndicatorForm);
                    Common.Util.DisposeObject(_forms.CautionPanelForm);
                    Common.Util.DisposeObject(_forms.CMDSPanelForm);
                    Common.Util.DisposeObject(_forms.CompassForm);
                    Common.Util.DisposeObject(_forms.DEDForm);
                    Common.Util.DisposeObject(_forms.PFLForm);
                    Common.Util.DisposeObject(_forms.EPUFuelForm);
                    Common.Util.DisposeObject(_forms.AccelerometerForm);
                    Common.Util.DisposeObject(_forms.FTIT1Form);
                    Common.Util.DisposeObject(_forms.FTIT2Form);
                    Common.Util.DisposeObject(_forms.FuelFlowForm);
                    Common.Util.DisposeObject(_forms.ISISForm);
                    Common.Util.DisposeObject(_forms.FuelQuantityForm);
                    Common.Util.DisposeObject(_forms.HSIForm);
                    Common.Util.DisposeObject(_forms.EHSIForm);
                    Common.Util.DisposeObject(_forms.LandingGearLightsForm);
                    Common.Util.DisposeObject(_forms.NWSIndexerForm);
                    Common.Util.DisposeObject(_forms.NOZPos1Form);
                    Common.Util.DisposeObject(_forms.NOZPos2Form);
                    Common.Util.DisposeObject(_forms.OILGauge1Form);
                    Common.Util.DisposeObject(_forms.OILGauge2Form);
                    Common.Util.DisposeObject(_forms.RWRForm);
                    Common.Util.DisposeObject(_forms.SpeedbrakeForm);
                    Common.Util.DisposeObject(_forms.RPM1Form);
                    Common.Util.DisposeObject(_forms.RPM2Form);
                    Common.Util.DisposeObject(_forms.VVIForm);
                    Common.Util.DisposeObject(_forms.HydAForm);
                    Common.Util.DisposeObject(_forms.HydBForm);
                    Common.Util.DisposeObject(_forms.CabinPressForm);
                    Common.Util.DisposeObject(_forms.RollTrimForm);
                    Common.Util.DisposeObject(_forms.PitchTrimForm);

                    Common.Util.DisposeObject(_forms.MFD4Form);
                    Common.Util.DisposeObject(_forms.MFD3Form);
                    Common.Util.DisposeObject(_forms.LeftMFDForm);
                    Common.Util.DisposeObject(_forms.RightMfdForm);
                    Common.Util.DisposeObject(_forms.HUDForm);
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