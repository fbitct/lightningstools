using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport.DirectInput;
using Common.InputSupport.UI;
using Common.SimSupport;
using Common.UI;
using F4SharedMem;
using F4Utils.Process;
using F4Utils.Terrain;
using MFDExtractor.EventSystem;
using MFDExtractor.Networking;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using log4net;
using Util = Common.Imaging.Util;
using Common.Networking;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor
{
	public sealed class Extractor : IDisposable
    {
        #region Instance Variables

        private static readonly ILog Log = LogManager.GetLogger(typeof (Extractor));
        private static Extractor _extractor;
        private bool _disposed;
        private long _renderCycleNum;

	    private readonly IInputControlSelectionSettingReader _inputControlSelectionSettingReader = new InputControlSelectionSettingReader();
		private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache= new InstrumentStateSnapshotCache();

        private readonly Dictionary<IInstrumentRenderer, InstrumentForm> _outputForms = new Dictionary<IInstrumentRenderer, InstrumentForm>();
	    private KeySettings _keySettings;
	    private readonly IKeySettingsReader _keySettingsReader = new KeySettingsReader();

		private readonly IInstrumentRendererSet _renderers = new InstrumentRendererSet();
        private readonly IRendererSetInitializer _rendererSetInitializer;

        private GdiPlusOptions _gdiPlusOptions = new GdiPlusOptions();


		private readonly InstrumentForms _forms;
        private bool _keepRunning;
        private bool _nightMode =false;
        private bool _running;
        private bool _testMode=false;
        private bool _threeDeeMode;
        private bool _twoDeePrimaryView = true;

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

        public static bool SimRunning = false;
        private readonly object _texSmReaderLock = new object();
        private Reader _falconSmReader;
        private readonly IFlightDataUpdater _flightDataUpdater = new FlightDataUpdater();
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

        private const string ServiceName = "MFDExtractorService";
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
        private InputControlSelection _accelerometerResetKey;
        private InputControlSelection _airspeedIndexDecreaseKey;
        private InputControlSelection _airspeedIndexIncreaseKey;
        private InputControlSelection _azimuthIndicatorBrightnessDecreaseKey;
        private InputControlSelection _azimuthIndicatorBrightnessIncreaseKey;
        private InputControlSelection _ehsiCourseDecreaseKey;
        private InputControlSelection _ehsiCourseDepressedKey;
        private InputControlSelection _ehsiCourseIncreaseKey;
        private InputControlSelection _ehsiHeadingDecreaseKey;
        private InputControlSelection _ehsiHeadingIncreaseKey;
        private InputControlSelection _ehsiMenuButtonDepressedKey;
        private InputControlSelection _isisBrightButtonKey;
        private InputControlSelection _isisStandardButtonKey;
        private InputControlSelection _nvisKey;

        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;
        private Thread _accelerometerRenderThread;
        private Thread _adiRenderThread;
        private Thread _altimeterRenderThread;
        private Thread _aoaIndexerRenderThread;
        private Thread _aoaIndicatorRenderThread;
        private Thread _asiRenderThread;
        private Thread _backupAdiRenderThread;
        private Thread _cabinPressRenderThread;
        private Thread _captureOrchestrationThread;
        private Thread _cautionPanelRenderThread;
        private Thread _cmdsPanelRenderThread;
        private Thread _compassRenderThread;
        private Thread _dedRenderThread;
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
        private Thread _isisRenderThread;
        private Thread _keyboardWatcherThread;
        private Thread _landingGearLightsRenderThread;
        private Thread _leftMfdCaptureThread;
        private Thread _mfd3CaptureThread;
        private Thread _mfd4CaptureThread;
        private Thread _nozPos1RenderThread;
        private Thread _nozPos2RenderThread;
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
        private Thread _vviRenderThread;

        private readonly RenderThreadSetupHelper _renderThreadSetupHelper;
        private readonly ThreadAbortion _threadAbortion;

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
	    private readonly IEHSIStateTracker _ehsiStateTracker;

		

	    private readonly IKeyDownEventHandler _keyDownEventHandler;
	    private readonly IKeyUpEventHandler _keyUpEventHandler;
	    private readonly IKeyboardWatcher _keyboardWatcher;
	    private readonly IClientSideIncomingMessageDispatcher _clientSideIncomingMessageDispatcher;
		private readonly IServerSideIncomingMessageDispatcher _serverSideIncomingMessageDispatcher;
	    private readonly IGdiPlusOptionsReader _gdiPlusOptionsReader;
	    private readonly IInputEvents _inputEvents;
	    private readonly IInstrumentRenderHelper _instrumentRenderHelper;
        private readonly IRenderStartHelper _renderStartHelper;
	    private readonly IInstrumentFormFactory _instrumentFormFactory;
	    private readonly IRadarAltitudeCalculator _radarAltitudeCalculator;
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
            IInstrumentRenderHelper instrumentRenderHelper = null,
            IRenderStartHelper renderStartHelper = null,
            IInstrumentFormFactory instrumentFormFactory=null,
            IRadarAltitudeCalculator radarAltitudeCalculator=null)
        {
            _instrumentFormFactory = instrumentFormFactory ?? new InstrumentFormFactory();
			_forms = new InstrumentForms();
            _renderStartHelper = renderStartHelper ?? new RenderStartHelper();
	        _gdiPlusOptionsReader = gdiPlusOptionsReader ?? new GdiPlusOptionsReader();
            LoadSettings();
			_rendererSetInitializer = new RendererSetInitializer(_renderers);
			_rendererSetInitializer.Initialize(_gdiPlusOptions);
			_ehsiStateTracker = ehsiStateTracker ?? new EHSIStateTracker(_renderers.EHSI);
			_directInputEventHotkeyFilter = directInputEventHotkeyFilter ?? new DirectInputEventHotkeyFilter();
            _instrumentRenderHelper = instrumentRenderHelper ?? new InstrumentRenderHelper();
			State = new ExtractorState();

			_diHotkeyDetection = new DIHotkeyDetection(Mediator);
            _inputEvents = inputEvents ?? new InputEvents(_renderers, _ehsiStateTracker, this);
	        _mediatorEventHandler =  new MediatorStateChangeHandler(_keySettings, _directInputEventHotkeyFilter,_diHotkeyDetection, _ehsiStateTracker,_inputEvents );
            if (!Settings.Default.DisableDirectInputMediator)
            {
                Mediator = new Mediator(null);
            }
            _renderThreadSetupHelper = new RenderThreadSetupHelper();
            _threadAbortion = new ThreadAbortion();
	        _keyDownEventHandler = keyDownEventHandler ?? new KeyDownEventHandler(_ehsiStateTracker, _inputEvents, _keySettings);

			_keyUpEventHandler = new KeyUpEventHandler(_keySettings, _ehsiStateTracker, _inputEvents);
			_keyboardWatcher = keyboardWatcher ?? new KeyboardWatcher(_keyDownEventHandler, _keyUpEventHandler, Log);
			_clientSideIncomingMessageDispatcher = clientSideIncomingMessageDispatcher ?? new ClientSideIncomingMessageDispatcher(_inputEvents, _client);
			_serverSideIncomingMessageDispatcher = serverSideIncomingMessageDispatcher ?? new ServerSideIncomingMessageDispatcher(_inputEvents);
            _radarAltitudeCalculator = new RadarAltitudeCalculator(_terrainBrowser);
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
            OnStarting();
	        _keySettings = _keySettingsReader.Read();
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged += _mediatorEventHandler.HandleStateChange;
            }
            
            RunThreads();
        }

	    private void OnStarting()
	    {
	        if (Starting != null)
	        {
	            Starting.Invoke(this, new EventArgs());
	        }
	    }

	    public void Stop()
        {
            OnStopping();
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
            OnStopped();
        }

	    private void OnStopped()
	    {
	        if (Stopped != null)
	        {
	            Stopped.Invoke(this, new EventArgs());
	        }
	    }

	    private void OnStopping()
	    {
	        if (Stopping != null)
	        {
	            Stopping.Invoke(this, new EventArgs());
	        }
	    }

	    public static Extractor GetInstance()
        {
            return _extractor ?? (_extractor = new Extractor());
        }

	    public void LoadSettings()
        {
            var settings = Settings.Default;
	        _keySettings = _keySettingsReader.Read();
            _gdiPlusOptions= _gdiPlusOptionsReader.Read();
            _networkMode = (NetworkMode) settings.NetworkingMode;
            switch (_networkMode)
            {
                case NetworkMode.Server:
                    _serverEndpoint = new IPEndPoint(IPAddress.Any, settings.ServerUsePortNumber);
                    break;
                case NetworkMode.Client:
                    _serverEndpoint = new IPEndPoint(IPAddress.Parse(settings.ClientUseServerIpAddress),
                        settings.ClientUseServerPortNum);
                    break;
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
                _client = new ExtractorClient(_serverEndpoint, ServiceName);
            }
            catch {}
        }

        private void SetupNetworkingServer()
        {
            ExtractorServer.CreateService(ServiceName, _serverEndpoint.Port, _compressionType, _imageFormat);
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
            if (_testMode)
            {
            }
            else
            {
                if (SimRunning || _networkMode == NetworkMode.Client)
                {
                    switch (_networkMode)
                    {
                        case NetworkMode.Standalone:
                        case NetworkMode.Server:
                        {
                            var falconDataFormat = F4Utils.Process.Util.DetectFalconFormat();

                            //set automatic 3D mode for BMS
                            if (falconDataFormat.HasValue && falconDataFormat.Value == FalconDataFormats.BMS4)
                                _threeDeeMode = true;

                            bool doMore = true;
                            if (_falconSmReader == null)
                            {
                                _falconSmReader = falconDataFormat.HasValue
                                    ? new Reader(falconDataFormat.Value)
                                    : new Reader();
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
                                if (NetworkMode == NetworkMode.Server)
                                {
                                    _radarAltitudeCalculator.ComputeRadarAltitude(toReturn);
                                }
                            }
                        }
                            break;
                        case NetworkMode.Client:
                            toReturn = _client.GetFlightData();
                            break;
                    }
                }
            }
            return toReturn ?? new FlightData {hsiBits = Int32.MaxValue};
        }

        private Image GetImage(Image testAlignmentImage,
            Func<Image> threeDeeModeLocalCaptureFunc,
            Func<Image> remoteImageRequestFunc,
            Rectangle twoDeePrimaryCaptureRectangle,
            Rectangle twoDeeSecondaryCaptureRectangle)
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Util.CloneBitmap(testAlignmentImage);
            }
            else
            {
                if (!SimRunning && _networkMode != NetworkMode.Client) return null;
                if (_threeDeeMode && (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone))
                {
                    toReturn = threeDeeModeLocalCaptureFunc();
                }
                else
                {
                    switch (_networkMode)
                    {
                        case NetworkMode.Standalone: //fallthrough
                        case NetworkMode.Server:
                            toReturn = Common.Screen.Util.CaptureScreenRectangle(_twoDeePrimaryView
                                ? twoDeePrimaryCaptureRectangle 
                                : twoDeeSecondaryCaptureRectangle);
                            break;
                        case NetworkMode.Client:
                            toReturn = remoteImageRequestFunc();
                            break;
                    }
                }
            }
            return toReturn;

        }
	    private Image GetMfd4Bitmap()
	    {
            return GetImage(_mfd4TestAlignmentImage, Get3DMFD4, () => _client != null ? _client.GetMfd4Bitmap() : null, _primaryMfd4_2DInputRect, _secondaryMfd4_2DInputRect);
        }

        private Image GetMfd3Bitmap()
        {
            return GetImage(_mfd3TestAlignmentImage, Get3DMFD3, () => _client != null ? _client.GetMfd3Bitmap():null, _primaryMfd3_2DInputRect, _secondaryMfd3_2DInputRect);
        }

        private Image GetLeftMfdBitmap()
        {
            return GetImage(_leftMfdTestAlignmentImage, Get3DLeftMFD, () => _client != null ? _client.GetLeftMfdBitmap():null, _primaryLeftMfd2DInputRect, _secondaryLeftMfd2DInputRect);
        }

	    private Image GetRightMfdBitmap()
	    {
            return GetImage(_rightMfdTestAlignmentImage, Get3DRightMFD, () => _client != null ? _client.GetRightMfdBitmap():null, _primaryRightMfd2DInputRect, _secondaryRightMfd2DInputRect);
	    }

	    private Image GetHudBitmap()
        {
            return GetImage(_hudTestAlignmentImage, Get3DHud, () => _client != null ? _client.GetHudBitmap():null, _primaryHud2DInputRect, _secondaryHud2DInputRect);
        }

        private Image Get3D(Rectangle rttInputRectangle)
        {
            if (!_keepRunning || (!SimRunning || !_sim3DDataAvailable) || rttInputRectangle == Rectangle.Empty)
            {
                return null;
            }

            try
            {
                lock (_texSmReaderLock)
                {
                    if (_texSmReader != null)
                    {
                        return Util.CloneBitmap(_texSmReader.GetImage(rttInputRectangle));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            return null;
        }
        private Image Get3DHud()
        {
            return Get3D(_hud3DInputRect);
        }

        private Image Get3DMFD4()
        {
            return Get3D(_mfd4_3DInputRect);
        }

        private Image Get3DMFD3()
        {
            return Get3D(_mfd3_3DInputRect);
        }

        private Image Get3DLeftMFD()
        {
            return Get3D(_leftMfd3DInputRect);
        }

        private Image Get3DRightMFD()
        {
            return Get3D(_rightMfd3DInputRect);
        }

        #endregion

        #region MFD Capturing implementation methods

        private void CaptureAndUpdateOutput(bool instrumentEnabled, Func<Image> getterFunc, Action<Image> setterFunc, Image blankVersion )
        {
            if (!instrumentEnabled && _networkMode != NetworkMode.Server) return;

            Image image = null;
            try
            {
                image = getterFunc() ?? Util.CloneBitmap(blankVersion);
                setterFunc(image);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            finally
            {
                Common.Util.DisposeObject(image);
            }
        }

	    private void CaptureMfd4()
	    {
	        CaptureAndUpdateOutput(Settings.Default.EnableMfd4Output, GetMfd4Bitmap, SetMfd4Image, _mfd4BlankImage);
	    }

        private void CaptureMfd3()
        {
            CaptureAndUpdateOutput(Settings.Default.EnableMfd3Output, GetMfd3Bitmap, SetMfd3Image, _mfd3BlankImage);
        }

        private void CaptureLeftMfd()
        {
            CaptureAndUpdateOutput(Settings.Default.EnableLeftMFDOutput, GetLeftMfdBitmap, SetLeftMfdImage, _leftMfdBlankImage);
        }

        private void CaptureRightMfd()
        {
            CaptureAndUpdateOutput(Settings.Default.EnableRightMFDOutput, GetRightMfdBitmap, SetRightMfdImage, _rightMfdBlankImage);
        }

        private void CaptureHud()
        {
            CaptureAndUpdateOutput(Settings.Default.EnableHudOutput, GetHudBitmap, SetHudImage, _hudBlankImage);
        }

        #endregion

        #region MFD & HUD Image Swapping

        private void SetFlightData(FlightData flightData)
        {
            if (flightData == null) return;
            if (_networkMode == NetworkMode.Server)
            {
                ExtractorServer.SetFlightData(flightData);
            }
        }
        private void SetAndDisposeImage(Image image, Action<Image> serveImageFunc, RotateFlipType rotateFlipType, InstrumentForm instrumentForm, bool monochrome)
        {
            if (image == null) return;
            if (_networkMode == NetworkMode.Server)
            {
                serveImageFunc(image);
            }
            if (instrumentForm != null)
            {
                if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(rotateFlipType);
                }
                using (var graphics = instrumentForm.CreateGraphics())
                {
                    if (monochrome)
                    {
                        DrawImageToControlMonochrome(image, instrumentForm, graphics);
                    }
                    else
                    {
                        graphics.DrawImage(image, instrumentForm.ClientRectangle);
                    }
                }
            }
            Common.Util.DisposeObject(image);
        }

	    private static void DrawImageToControlMonochrome(Image image, Control instrumentForm, Graphics graphics)
	    {
	        var ia = new ImageAttributes();
	        ia.SetColorMatrix(Util.GreyscaleColorMatrix);
	        using (var compatible = Util.CopyBitmap(image))
	        {
	            graphics.DrawImage(compatible, instrumentForm.ClientRectangle, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, ia);
	        }
	    }

	    private void SetHudImage(Image hudImage)
        {
            SetAndDisposeImage(hudImage, ExtractorServer.SetHudBitmap, Settings.Default.HUD_RotateFlipType, _forms.HUDForm, Settings.Default.HUD_Monochrome);
        }
        private void SetMfd4Image(Image mfd4Image)
        {
            SetAndDisposeImage(mfd4Image, ExtractorServer.SetMfd4Bitmap, Settings.Default.MFD4_RotateFlipType, _forms.MFD4Form, Settings.Default.MFD4_Monochrome);
        }

        private void SetMfd3Image(Image mfd3Image)
        {
            SetAndDisposeImage(mfd3Image, ExtractorServer.SetMfd3Bitmap, Settings.Default.MFD3_RotateFlipType, _forms.MFD3Form, Settings.Default.MFD3_Monochrome);
        }

        private void SetLeftMfdImage(Image leftMfdImage)
        {
            SetAndDisposeImage(leftMfdImage, ExtractorServer.SetLeftMfdBitmap, Settings.Default.LMFD_RotateFlipType, _forms.LeftMFDForm, Settings.Default.LMFD_Monochrome);
        }

        private void SetRightMfdImage(Image rightMfdImage)
        {
            SetAndDisposeImage(rightMfdImage, ExtractorServer.SetRightMfdBitmap, Settings.Default.RMFD_RotateFlipType, _forms.RightMfdForm, Settings.Default.RMFD_Monochrome);
        }

        #endregion

        #endregion

        #region Forms Management

        #region Forms Setup

        private void SetupOutputForms()
        {
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
        }

        #region MFD Forms Setup

        private void SetupMfd4Form()
        {
            _forms.MFD4Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "MFD4","MFD 4",null,_mfd4Form_Disposed,_mfd4BlankImage);
        }

        private void SetupMfd3Form()
        {
            _forms.MFD3Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "MFD3", "MFD 3", null, _mfd3Form_Disposed, _mfd3BlankImage);
        }

        private void SetupLeftMfdForm()
        {
            _forms.LeftMFDForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "LMFD","Left MFD",null,_leftMfdForm_Disposed,_leftMfdBlankImage);
        }

        private void SetupRightMfdForm()
        {
            _forms.RightMfdForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "RMFD", "Right MFD", null, _rightMfdForm_Disposed, _rightMfdBlankImage);
        }

        private void SetupHudForm()
        {
            _forms.HUDForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "HUD", "HUD", null, _hudForm_Disposed, _hudBlankImage);
        }

        #endregion

        #region Instruments Forms Setup

        private void SetupVVIForm()
        {
            _forms.VVIForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "VVI", "VVI", _renderers.VVI, _vviForm_Disposed);
        }

        private void SetupRPM1Form()
        {
            _forms.RPM1Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "RPM1", "Engine 1 - RPM", _renderers.RPM1, _rpm1Form_Disposed);
        }

        private void SetupRPM2Form()
        {
            _forms.RPM2Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "RPM2", "Engine 2 - RPM", _renderers.RPM2, _rpm2Form_Disposed);
        }

        private void SetupSpeedbrakeForm()
        {
            _forms.SpeedbrakeForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "Speedbrake", "Speedbrake", _renderers.Speedbrake, _speedbrakeForm_Disposed);
        }

        private void SetupRWRForm()
        {
            _forms.RWRForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "RWR", "RWR", _renderers.RWR, _rwrForm_Disposed);
        }

        private void SetupOIL2Form()
        {
            _forms.OILGauge2Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "OIL2", "Engine 2 - Oil Pressure Indicator", _renderers.OIL2, _oilGauge2Form_Disposed);
        }

        private void SetupOIL1Form()
        {
            _forms.OILGauge1Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "OIL1", "Engine 1 - Oil Pressure Indicator", _renderers.OIL1, _oilGauge1Form_Disposed);
        }

        private void SetupNOZ2Form()
        {
            _forms.NOZPos2Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "NOZ2", "Engine 2 - Nozzle Position Indicator", _renderers.NOZ2, _nozPos2Form_Disposed);
        }

        private void SetupNOZ1Form()
        {
            _forms.NOZPos1Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "NOZ1", "Engine 1 - Nozzle Position Indicator", _renderers.NOZ1, _nozPos1Form_Disposed);
        }

        private void SetupNWSIndexerForm()
        {
            _forms.NWSIndexerForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "NWSIndexer", "NWS Indexer", _renderers.NWSIndexer, _nwsIndexerForm_Disposed);
        }

        private void SetupGearLightsForm()
        {
            _forms.LandingGearLightsForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "GearLights", "Landing Gear Lights", _renderers.LandingGearLights, _landingGearLightsForm_Disposed);
        }

        private void SetupHSIForm()
        {
            _forms.HSIForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "HSI", "Horizontal Situation Indicator", _renderers.HSI, _hsiForm_Disposed);
        }

        private void SetupEHSIForm()
        {
            _forms.EHSIForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "EHSI", "EHSI", _renderers.EHSI, _ehsiForm_Disposed);
        }

        private void SetupFuelQuantityForm()
        {
            _forms.FuelQuantityForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "FuelQuantity", "Fuel Quantity", _renderers.FuelQuantity, FuelQuantityForm_Disposed);
        }

        private void SetupFuelFlowForm()
        {
            _forms.FuelFlowForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "FuelFlow", "Fuel Flow Indicator", _renderers.FuelFlow, _fuelFlowForm_Disposed);
        }

        private void SetupISISForm()
        {
            _forms.ISISForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "ISIS", "ISIS", _renderers.ISIS, _isisForm_Disposed);
        }

        private void SetupAccelerometerForm()
        {
            _forms.AccelerometerForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "Accelerometer", "Accelerometer", _renderers.Accelerometer, _accelerometerForm_Disposed);
        }

        private void SetupFTIT2Form()
        {
            _forms.FTIT2Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "FTIT2", "FTIT 2", _renderers.FTIT2, _ftit2Form_Disposed);
        }

        private void SetupFTIT1Form()
        {
            _forms.FTIT1Form = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "FTIT1", "FTIT 1", _renderers.FTIT1, _ftit1Form_Disposed);
        }

        private void SetupEPUFuelForm()
        {
            _forms.EPUFuelForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "EPUFuel", "EPU Fuel", _renderers.EPUFuel, _epuFuelForm_Disposed);
        }

        private void SetupPFLForm()
        {
            _forms.PFLForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "PFL", "PFL", _renderers.PFL, _pflForm_Disposed);
        }

        private void SetupDEDForm()
        {
            _forms.DEDForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "DED", "DED", _renderers.DED, _dedForm_Disposed);
        }

        private void SetupCompassForm()
        {
            _forms.CompassForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "Compass", "Compass", _renderers.Compass, _compassForm_Disposed);
        }

        private void SetupCMDSPanelForm()
        {
            _forms.CMDSPanelForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "CMDS", "CMDS", _renderers.CMDSPanel, _cmdsPanelForm_Disposed);
        }

        private void SetupCautionPanelForm()
        {
            _forms.CautionPanelForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "CautionPanel", "Caution Panel", _renderers.CautionPanel, _cautionPanelForm_Disposed);
        }

        private void SetupAOAIndicatorForm()
        {
            _forms.AOAIndicatorForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "AOAIndicator", "AOA Indicator", _renderers.AOAIndicator, AOAIndicatorForm_Disposed);
        }

		private void SetupAOAIndexerForm()
        {
            _forms.AOAIndexerForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "AOAIndexer", "AOA Indexer", _renderers.AOAIndexer, AOAIndexerForm_Disposed);
        }

        private void SetupAltimeterForm()
        {
            _forms.AltimeterForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "Altimeter", "Altimeter", _renderers.Altimeter, _altimeterForm_Disposed);
        }

        private void SetupCabinPressForm()
        {
            _forms.CabinPressForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "CabinPress", "Cabin Pressure Indicator", _renderers.CabinPress, _cabinPressForm_Disposed);
        }

        private void SetupRollTrimForm()
        {
            _forms.RollTrimForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "RollTrim", "Roll Trim Indicator", _renderers.RollTrim, _rollTrimForm_Disposed);
        }

        private void SetupPitchTrimForm()
        {
            _forms.PitchTrimForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "PitchTrim", "Pitch Trim Indicator", _renderers.PitchTrim, _pitchTrimForm_Disposed);
        }

        private void SetupHydAForm()
        {
            _forms.HydAForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "HYDA", "Hydraulic Pressure Indicator A", _renderers.HYDA, _hydAForm_Disposed);
        }

        private void SetupHydBForm()
        {
            _forms.HydBForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "HYDB", "Hydraulic Pressure Indicator B", _renderers.HYDB, _hydBForm_Disposed);
        }

        private void SetupASIForm()
        {
            _forms.ASIForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "ASI", "Airspeed Indicator", _renderers.ASI, _asiForm_Disposed);
        }

        private void SetupADIForm()
        {
            _forms.ADIForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "ADI", "Attitude Indicator", _renderers.ADI, _adiForm_Disposed);
        }

        private void SetupBackupADIForm()
        {
            _forms.BackupAdiForm = _instrumentFormFactory.SetupInstrumentForm(_extractor, _outputForms, "BackupADI", "Standby Attitude Indicator", _renderers.BackupADI, _backupAdiForm_Disposed);
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

        private static void CloseOutputWindowForm(Form form)
        {
            if (form == null) return;
            try
            {
                form.Close();
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e.Message, e);
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
            if (_running) return;
            _running = true;
            SetupNetworking();
            _keepRunning = true;
            _rendererSetInitializer.Initialize(_gdiPlusOptions);
            SetupOutputForms();
            SetupThreads();
            StartThreads();

            if (Started != null)
            {
                Started.Invoke(this, new EventArgs());
            }
        }

        private void StartThreads()
        {
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
        }

        private static void StartThread(Thread t)
        {
            if (t == null) return;
            t.Start();
        }

       

        private void CaptureOrchestrationThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
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

                    switch (NetworkMode)
                    {
                        case NetworkMode.Client:
                            _clientSideIncomingMessageDispatcher.ProcessPendingMessages();
                            break;
                        case NetworkMode.Server:
                            _serverSideIncomingMessageDispatcher.ProcessPendingMessages();
                            break;
                    }


                    if (SimRunning || _testMode || NetworkMode == NetworkMode.Client)
                    {
                        var currentFlightData = GetFlightData();
                        SetFlightData(currentFlightData);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, currentFlightData, SimRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _networkMode);
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
                            Log.Error(e.Message, e);
                        }
                    }
                    else
                    {
                        var flightDataToSet = new FlightData {hsiBits = Int32.MaxValue};
                        SetFlightData(flightDataToSet);
						_flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, flightDataToSet, SimRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _networkMode);
                        SetMfd4Image(Util.CloneBitmap(_mfd4BlankImage));
                        SetMfd3Image(Util.CloneBitmap(_mfd3BlankImage));
                        SetLeftMfdImage(Util.CloneBitmap(_leftMfdBlankImage));
                        SetRightMfdImage(Util.CloneBitmap(_rightMfdBlankImage));
                        SetHudImage(Util.CloneBitmap(_hudBlankImage));
                    }

                    try
                    {
                        var toWait = new List<WaitHandle>();
                        SignalMFDAndHudThreadsToStart();
                        SignalRwrRenderThreadToStart(toWait);
                        SignalPrimaryFlightInstrumentRenderThreadsToStart(toWait);
                        SignalFuelInstrumentsRenderThreadsToStart(toWait);
                        SignalIndexerRenderThreadsToStart(toWait);
                        SignalEngine1GaugesRenderThreadsToStart(toWait);
                        SignalEngine2GaugesRenderThreadsToStart(toWait);
                        SignalRightAuxEmulatedGaugesRenderThreadsToStart(toWait);
                        SignalTrimIndicatorRenderThreadsToStart(toWait);
                        SignalDEDAndPFLRenderThreadsToStart(toWait);
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
                        Log.Error(e.Message, e);
                    }

                    var thisLoopFinishTime = DateTime.Now;
                    var timeElapsed = thisLoopFinishTime.Subtract(thisLoopStartTime);
                    var millisToSleep = Settings.Default.PollingDelay - ((int) timeElapsed.TotalMilliseconds);
                    if (_testMode) millisToSleep = 500;
                    var sleepUntil = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, millisToSleep));
                    while (DateTime.Now < sleepUntil)
                    {
                        var millisRemaining = (int) Math.Floor(DateTime.Now.Subtract(sleepUntil).TotalMilliseconds);
                        var millisWaited = millisRemaining >= 5 ? 5 : 1;
                        Thread.Sleep(millisWaited);
                        Application.DoEvents();
                    }
                    Application.DoEvents();
                    if ((!SimRunning && _networkMode != NetworkMode.Client) && !_testMode)
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
        }

        private void SignalRwrRenderThreadToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableRWROutput, Settings.Default.RWR_RenderEveryN, Settings.Default.RWR_RenderOnN,
                _forms.RWRForm, _rwrRenderStart, _rwrRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RWR));
        }

        private static void WaitAllAndClearList(List<WaitHandle> waitHandles, int millisecondsTimeout)
        {
            if (waitHandles == null || waitHandles.Count <= 0) return;
            try
            {
                WaitHandle.WaitAll(waitHandles.ToArray(), millisecondsTimeout);
            }
            catch (TimeoutException)
            {
            }
            catch (DuplicateWaitObjectException) //this can happen somehow if our list is not cleared 
            {
            }
            waitHandles.Clear();
        }

        private void SignalCMDSRenderThreadToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning, 
                Settings.Default.EnableCMDSOutput, Settings.Default.CMDS_RenderEveryN, Settings.Default.CMDS_RenderOnN,
                _forms.CMDSPanelForm, _cmdsPanelRenderStart, _cmdsPanelRenderEnd, 
                _testMode, _renderCycleNum, 
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CMDSPanel));
        }

        private void SignalCautionPanelRenderThreadToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableCautionPanelOutput, Settings.Default.CautionPanel_RenderEveryN, Settings.Default.CautionPanel_RenderOnN,
                _forms.CautionPanelForm, _cautionPanelRenderStart, _cautionPanelRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CautionPanel));
        }

        private void SignalGearPanelAccessoryRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableGearLightsOutput, Settings.Default.GearLights_RenderEveryN, Settings.Default.GearLights_RenderOnN,
                _forms.LandingGearLightsForm, _landingGearLightsRenderStart, _landingGearLightsRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.LandingGearLights));


            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableSpeedbrakeOutput, Settings.Default.Speedbrake_RenderEveryN, Settings.Default.Speedbrake_RenderOnN,
                _forms.SpeedbrakeForm, _speedbrakeRenderStart, _speedbrakeRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Speedbrake));


        }

        private void SignalDEDAndPFLRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableDEDOutput, Settings.Default.DED_RenderEveryN, Settings.Default.DED_RenderOnN,
                _forms.DEDForm, _dedRenderStart, _dedRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.DED));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnablePFLOutput, Settings.Default.PFL_RenderEveryN, Settings.Default.PFL_RenderOnN,
                _forms.PFLForm, _pflRenderStart, _pflRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.PFL));
            
        }

        private void SignalFuelInstrumentsRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableFuelFlowOutput, Settings.Default.FuelFlow_RenderEveryN, Settings.Default.FuelFlow_RenderOnN,
                _forms.FuelFlowForm, _fuelFlowRenderStart, _fuelFlowRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FuelFlow));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableFuelQuantityOutput, Settings.Default.FuelQuantity_RenderEveryN, Settings.Default.FuelQuantity_RenderOnN,
                _forms.FuelQuantityForm, _fuelQuantityRenderStart, _fuelQuantityRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FuelQuantity));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableEPUFuelOutput, Settings.Default.EPUFuel_RenderEveryN, Settings.Default.EPUFuel_RenderOnN,
                _forms.EPUFuelForm, _epuFuelRenderStart, _epuFuelRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.EPUFuel));

        }

        private void SignalIndexerRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableAOAIndexerOutput, Settings.Default.AOAIndexer_RenderEveryN, Settings.Default.AOAIndexer_RenderOnN,
                _forms.AOAIndexerForm, _aoaIndexerRenderStart, _aoaIndexerRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.AOAIndexer));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableNWSIndexerOutput, Settings.Default.NWSIndexer_RenderEveryN, Settings.Default.NWSIndexer_RenderOnN,
                _forms.NWSIndexerForm, _nwsIndexerRenderStart, _nwsIndexerRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NWSIndexer));
            
        }

        private void SignalEngine1GaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableFTIT1Output, Settings.Default.FTIT1_RenderEveryN, Settings.Default.FTIT1_RenderOnN,
                _forms.FTIT1Form, _ftit1RenderStart, _ftit1RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FTIT1));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableNOZ1Output, Settings.Default.NOZ1_RenderEveryN, Settings.Default.NOZ1_RenderOnN,
                _forms.NOZPos1Form, _nozPos1RenderStart, _nozPos1RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NOZ1));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableOIL1Output, Settings.Default.OIL1_RenderEveryN, Settings.Default.OIL1_RenderOnN,
                _forms.OILGauge1Form, _oilGauge1RenderStart , _oilGauge1RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.OIL1));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableRPM1Output, Settings.Default.RPM1_RenderEveryN, Settings.Default.RPM1_RenderOnN,
                _forms.RPM1Form, _rpm1RenderStart, _rpm1RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RPM1));

        }

        private void SignalEngine2GaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableFTIT2Output, Settings.Default.FTIT2_RenderEveryN, Settings.Default.FTIT2_RenderOnN,
                _forms.FTIT2Form, _ftit2RenderStart, _ftit2RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.FTIT2));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableNOZ2Output, Settings.Default.NOZ2_RenderEveryN, Settings.Default.NOZ2_RenderOnN,
                _forms.NOZPos2Form, _nozPos2RenderStart, _nozPos2RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.NOZ2));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableOIL2Output, Settings.Default.OIL2_RenderEveryN, Settings.Default.OIL2_RenderOnN,
                _forms.OILGauge2Form, _oilGauge2RenderStart, _oilGauge2RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.OIL2));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableRPM2Output, Settings.Default.RPM2_RenderEveryN, Settings.Default.RPM2_RenderOnN,
                _forms.RPM2Form, _rpm2RenderStart, _rpm2RenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RPM2));
        }

        private void SignalRightAuxEmulatedGaugesRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableHYDAOutput, Settings.Default.HYDA_RenderEveryN, Settings.Default.HYDA_RenderOnN,
                _forms.HydAForm, _hydARenderStart, _hydARenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HYDA));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableHYDBOutput, Settings.Default.HYDB_RenderEveryN, Settings.Default.HYDB_RenderOnN,
                _forms.HydBForm, _hydBRenderStart, _hydBRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HYDB));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableCabinPressOutput, Settings.Default.CabinPress_RenderEveryN, Settings.Default.CabinPress_RenderOnN,
                _forms.CabinPressForm, _cabinPressRenderStart, _cabinPressRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.CabinPress));

        }

        private void SignalTrimIndicatorRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableRollTrimOutput, Settings.Default.RollTrim_RenderEveryN, Settings.Default.RollTrim_RenderOnN,
                _forms.RollTrimForm, _rollTrimRenderStart, _rollTrimRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.RollTrim));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnablePitchTrimOutput, Settings.Default.PitchTrim_RenderEveryN, Settings.Default.PitchTrim_RenderOnN,
                _forms.PitchTrimForm, _pitchTrimRenderStart, _pitchTrimRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.PitchTrim));
        }

        private void SignalPrimaryFlightInstrumentRenderThreadsToStart(List<WaitHandle> toWait)
        {
            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableADIOutput, Settings.Default.ADI_RenderEveryN, Settings.Default.ADI_RenderOnN,
                _forms.ADIForm, _adiRenderStart, _adiRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ADI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableISISOutput, Settings.Default.ISIS_RenderEveryN, Settings.Default.ISIS_RenderOnN,
                _forms.ISISForm, _isisRenderStart, _isisRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ISIS));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableHSIOutput, Settings.Default.HSI_RenderEveryN, Settings.Default.HSI_RenderOnN,
                _forms.HSIForm, _hsiRenderStart, _hsiRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.HSI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableEHSIOutput, Settings.Default.EHSI_RenderEveryN, Settings.Default.EHSI_RenderOnN,
                _forms.EHSIForm, _ehsiRenderStart, _ehsiRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.EHSI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableAltimeterOutput, Settings.Default.Altimeter_RenderEveryN, Settings.Default.Altimeter_RenderOnN,
                _forms.AltimeterForm, _altimeterRenderStart, _altimeterRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Altimeter));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableASIOutput, Settings.Default.ASI_RenderEveryN, Settings.Default.ASI_RenderOnN,
                _forms.ASIForm, _asiRenderStart, _asiRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.ASI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableBackupADIOutput, Settings.Default.Backup_ADI_RenderEveryN, Settings.Default.Backup_ADI_RenderOnN,
                _forms.BackupAdiForm, _backupAdiRenderStart, _backupAdiRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.BackupADI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableVVIOutput, Settings.Default.VVI_RenderEveryN, Settings.Default.VVI_RenderOnN,
                _forms.VVIForm, _vviRenderStart, _vviRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.VVI));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableAOAIndicatorOutput, Settings.Default.AOAIndicator_RenderEveryN, Settings.Default.AOAIndicator_RenderOnN,
                _forms.AOAIndicatorForm, _aoaIndicatorRenderStart, _aoaIndicatorRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.AOAIndicator));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableCompassOutput, Settings.Default.Compass_RenderEveryN, Settings.Default.Compass_RenderOnN,
                _forms.CompassForm, _compassRenderStart, _compassRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Compass));

            _renderStartHelper.Start(toWait, _running, _keepRunning,
                Settings.Default.EnableAccelerometerOutput, Settings.Default.Accelerometer_RenderEveryN, Settings.Default.Accelerometer_RenderOnN,
                _forms.AccelerometerForm, _accelerometerRenderStart, _accelerometerRenderEnd,
                _testMode, _renderCycleNum,
                IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_renderers.Accelerometer));

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
                        var simWasRunning = SimRunning;

                        //TODO:make this check optional via the user-config file
                        if (count%1 == 0)
                        {
                            count = 0;
                            Common.Util.DisposeObject(_texSmStatusReader);
                            _texSmStatusReader = new F4TexSharedMem.Reader();

                            try
                            {
                                SimRunning = NetworkMode == NetworkMode.Client ||
                                              F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                            }
                            _sim3DDataAvailable = SimRunning &&
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
                            if (simWasRunning && !SimRunning)
                            {
                                CloseAndDisposeSharedmemReaders();

                                if (_networkMode == NetworkMode.Server)
                                {
                                    TearDownImageServer();
                                }
                            }
                            if (_networkMode == NetworkMode.Server && (!simWasRunning && SimRunning))
                            {
                                SetupNetworkingServer();
                            }
                        }
                    }
                    Thread.Sleep(500);
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
            var startTime = DateTime.Now;

            for (var i = 0; i < threads.Length; i++)
            {
                var t = threads[i];

                if (t == null)
                {
                    continue;
                }
                try { t.Interrupt(); } catch {}
            }

            for (var i = 0; i < threads.Length; i++)
            {
                var thread = threads[i];

                if (thread == null)
                {
                    continue;
                }

                var now = DateTime.Now;
                var elapsed = now.Subtract(startTime);
                var timeRemaining = elapsed.Subtract(timeout);
                if (timeRemaining.TotalMilliseconds <= 0) timeRemaining = new TimeSpan(0, 0, 0, 0, 1);

                try
                {
                    thread.Join(timeRemaining);
                    threads[i] = null;
                }
                catch{}
            }

            for (var i = 0; i < threads.Length; i++)
            {
                var thread = threads[i];

                if (thread == null)
                {
                    continue;
                }

                try
                {
                    _threadAbortion.AbortThread(ref thread);
                }
                catch {}
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
            _captureOrchestrationThread = new Thread(CaptureOrchestrationThreadWork)
            {
                Priority = _threadPriority,
                IsBackground = true,
                Name = "CaptureOrchestrationThread"
            };
        }
        private void SetupCaptureThread(ref Thread thread, Func<bool> predicate, ThreadStart threadStart, string threadName)
        {
            _threadAbortion.AbortThread(ref thread);
            if (predicate())
            {
                thread = new Thread(threadStart)
                {
                    Priority = _threadPriority,
                    IsBackground = true,
                    Name = threadName
                };
            }
        }

        private void SetupHUDCaptureThread()
        {
            SetupCaptureThread(ref _hudCaptureThread, () => Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_hudCaptureStart, CaptureHud), "HudCaptureThread");
        }

        private void SetupRightMFDCaptureThread()
        {
            SetupCaptureThread(ref _rightMfdCaptureThread, () => Settings.Default.EnableRightMFDOutput || NetworkMode == NetworkMode.Server, ()=>CaptureThreadWork(_rightMfdCaptureStart, CaptureRightMfd), "RightMfdCaptureThread");
        }

        private void SetupLeftMFDCaptureThread()
        {
            SetupCaptureThread(ref _leftMfdCaptureThread, () => Settings.Default.EnableLeftMFDOutput || NetworkMode == NetworkMode.Server, ()=>CaptureThreadWork(_leftMfdCaptureStart,CaptureLeftMfd), "LeftMfdCaptureThread");
        }

        private void SetupMFD3CaptureThread()
        {
            SetupCaptureThread(ref _mfd3CaptureThread, () => Settings.Default.EnableMfd3Output || NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_mfd3CaptureStart, CaptureMfd3), "Mfd3CaptureThread");
        }

        private void SetupMFD4CaptureThread()
        {
            SetupCaptureThread(ref _mfd4CaptureThread, () => Settings.Default.EnableMfd4Output || NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_mfd4CaptureStart, CaptureMfd4), "Mfd4CaptureThread");
        }

        private void SetupSimStatusMonitorThread()
        {
            _threadAbortion.AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork)
            {
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true,
                Name = "SimStatusMonitorThread"
            };
        }

        #endregion

        #region Gauges rendering thread-work methods

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
            if (!(elapsed.TotalMilliseconds < 0)) return;
            var toWait = new TimeSpan(0, 0, 0, 0,
                (int) (0 - elapsed.TotalMilliseconds));
            if (toWait.TotalMilliseconds < 0)
            {
                toWait = new TimeSpan(0, 0, 0, 0, 0);
            }
            Thread.Sleep(toWait);
        }


	    private static bool HighlightingBorderShouldBeDisplayedOnTargetForm(InstrumentForm targetForm)
        {
            return targetForm !=null && targetForm.SizingOrMovingCursorsAreDisplayed && Settings.Default.HighlightOutputWindows;
        }

	    #endregion

        private void CaptureThreadWork(WaitHandle waitHandle, Action capture)
        {
            try
            {
                while (_keepRunning)
                {
                    waitHandle.WaitOne();
                    capture();
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
            if (_extractor == null) return;
            Common.Util.DisposeObject(_extractor);
            _extractor = null;
        }

        #endregion
    }
}