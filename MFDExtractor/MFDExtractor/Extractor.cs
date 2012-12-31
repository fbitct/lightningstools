using System;
using System.Windows.Forms;
using log4net;
using MFDExtractor.UI;
using Common.SimSupport;
using System.Diagnostics;
using Common.UI;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using Common.InputSupport.UI;
using Common.InputSupport.DirectInput;
using Common.InputSupport;
using F4SharedMem;
using System.Drawing.Imaging;
using System.IO;
using Common.Win32;
using LightningGauges.Renderers;
using MFDExtractor.Networking;
using F4SharedMem.Headers;
using F4Utils.SimSupport;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.DirectX.DirectInput;
namespace MFDExtractor
{
    /// <summary>
    /// Enumeration of possible networking modes that an Extractor instance can operate under
    /// </summary>
    public enum NetworkMode
    {
        /// <summary>
        /// Standalone (non-networked) mode
        /// </summary>
        Standalone,
        /// <summary>
        /// Server mode (provides images to remote clients in addition to providing local images)
        /// </summary>
        Server,
        /// <summary>
        /// Client mode (receives images from a remote server)
        /// </summary>
        Client
    }

    public sealed class Extractor : IDisposable
    {
        #region Instance Variables
        private static ILog _log = LogManager.GetLogger(typeof(Extractor));
        private bool _disposed = false;
        /// <summary>
        /// Reference to an instance of this class -- this reference is required so that we
        /// can implement the Singleton pattern, which allows only a single instance of this
        /// class to be created as an object, per app-domain
        /// </summary>
        private static Extractor _extractor;
        private const int MIN_RENDERER_PASS_TIME_MILLSECONDS = 0;//minimum time each instrument render should take per cycle before trying to run again (introduced for throttling purposes)
        private const int MIN_DELAY_AT_END_OF_INSTRUMENT_RENDER = 0;//minimum time after each individual instrument render that should be waited 
        private long _renderCycleNum = 0;

        #region MFD & HUD Output Screens
        /// <summary>
        /// Screen on which to output MFD #4
        /// </summary>
        private Screen _mfd4OutputScreen = null;
        /// <summary>
        /// Screen on which to output MFD #3
        /// </summary>
        private Screen _mfd3OutputScreen = null;
        /// <summary>
        /// Screen on which to output the Left MFD
        /// </summary>
        private Screen _leftMfdOutputScreen = null;
        /// <summary>
        /// Screen on which to output the Right MFD
        /// </summary>
        private Screen _rightMfdOutputScreen = null;
        /// <summary>
        /// Screen on which to output the HUD
        /// </summary>
        private Screen _hudOutputScreen = null;
        #endregion

        #region Instrument Forms
        private InstrumentForm _adiForm = null;
        private InstrumentForm _backupAdiForm = null;
        private InstrumentForm _asiForm = null;
        private InstrumentForm _altimeterForm = null;
        private InstrumentForm _aoaIndexerForm = null;
        private InstrumentForm _aoaIndicatorForm = null;
        private InstrumentForm _cautionPanelForm = null;
        private InstrumentForm _cmdsPanelForm = null;
        private InstrumentForm _compassForm = null;
        private InstrumentForm _dedForm = null;
        private InstrumentForm _pflForm = null;
        private InstrumentForm _epuFuelForm = null;
        private InstrumentForm _accelerometerForm = null;
        private InstrumentForm _ftit1Form = null;
        private InstrumentForm _ftit2Form = null;
        private InstrumentForm _fuelFlowForm = null;
        private InstrumentForm _isisForm = null;
        private InstrumentForm _fuelQuantityForm = null;
        private InstrumentForm _hsiForm = null;
        private InstrumentForm _ehsiForm = null;
        private InstrumentForm _landingGearLightsForm = null;
        private InstrumentForm _nwsIndexerForm = null;
        private InstrumentForm _nozPos1Form = null;
        private InstrumentForm _nozPos2Form = null;
        private InstrumentForm _oilGauge1Form = null;
        private InstrumentForm _oilGauge2Form = null;
        private InstrumentForm _rwrForm = null;
        private InstrumentForm _speedbrakeForm = null;
        private InstrumentForm _rpm1Form = null;
        private InstrumentForm _rpm2Form = null;
        private InstrumentForm _vviForm = null;
        private InstrumentForm _hydAForm = null;
        private InstrumentForm _hydBForm = null;
        private InstrumentForm _cabinPressForm = null;
        private InstrumentForm _rollTrimForm = null;
        private InstrumentForm _pitchTrimForm = null;

        /// <summary>
        /// Form that will display MFD #4 data
        /// </summary>
        private InstrumentForm _mfd4Form = null;
        /// <summary>
        /// Form that will display MFD #3 data
        /// </summary>
        private InstrumentForm _mfd3Form = null;
        /// <summary>
        /// Form that will display the Left MFD data
        /// </summary>
        private InstrumentForm _leftMfdForm = null;
        /// <summary>
        /// Form that will display the Right MFD data
        /// </summary>
        private InstrumentForm _rightMfdForm = null;
        /// <summary>
        /// Form that will display the HUD data
        /// </summary>
        private InstrumentForm _hudForm = null;
        private Dictionary<IInstrumentRenderer, InstrumentForm> _outputForms = new Dictionary<IInstrumentRenderer, InstrumentForm>();
        #endregion

        #region Instrument Renderers
        private IInstrumentRenderer _adiRenderer = null;
        private IInstrumentRenderer _backupAdiRenderer = null;
        private IInstrumentRenderer _asiRenderer = null;
        private IInstrumentRenderer _altimeterRenderer = null;
        private IInstrumentRenderer _aoaIndexerRenderer = null;
        private IInstrumentRenderer _aoaIndicatorRenderer = null;
        private IInstrumentRenderer _cautionPanelRenderer = null;
        private IInstrumentRenderer _cmdsPanelRenderer = null;
        private IInstrumentRenderer _compassRenderer = null;
        private IInstrumentRenderer _dedRenderer = null;
        private IInstrumentRenderer _pflRenderer = null;
        private IInstrumentRenderer _epuFuelRenderer = null;
        private IInstrumentRenderer _accelerometerRenderer = null;
        private IInstrumentRenderer _ftit1Renderer = null;
        private IInstrumentRenderer _ftit2Renderer = null;
        private IInstrumentRenderer _fuelFlowRenderer = null;
        private IInstrumentRenderer _isisRenderer = null;
        private IInstrumentRenderer _fuelQuantityRenderer = null;
        private IInstrumentRenderer _hsiRenderer = null;
        private IInstrumentRenderer _ehsiRenderer = null;
        private IInstrumentRenderer _landingGearLightsRenderer = null;
        private IInstrumentRenderer _nwsIndexerRenderer = null;
        private IInstrumentRenderer _nozPos1Renderer = null;
        private IInstrumentRenderer _nozPos2Renderer = null;
        private IInstrumentRenderer _oilGauge1Renderer = null;
        private IInstrumentRenderer _oilGauge2Renderer = null;
        private IInstrumentRenderer _rwrRenderer = null;
        private IInstrumentRenderer _speedbrakeRenderer = null;
        private IInstrumentRenderer _rpm1Renderer = null;
        private IInstrumentRenderer _rpm2Renderer = null;
        private IInstrumentRenderer _vviRenderer = null;
        private IInstrumentRenderer _hydARenderer = null;
        private IInstrumentRenderer _hydBRenderer = null;
        private IInstrumentRenderer _cabinPressRenderer = null;
        private IInstrumentRenderer _rollTrimRenderer = null;
        private IInstrumentRenderer _pitchTrimRenderer = null;
        #endregion

        #region WMI Performance Counters
        private PerformanceCounter _mfd4PerfCounter = null;
        private PerformanceCounter _mfd3PerfCounter = null;
        private PerformanceCounter _leftMfdPerfCounter = null;
        private PerformanceCounter _rightMfdPerfCounter = null;
        private PerformanceCounter _hudPerfCounter = null;
        private PerformanceCounter _adiCounter = null;
        private PerformanceCounter _backupAdiCounter = null;
        private PerformanceCounter _asiCounter = null;
        private PerformanceCounter _altimeterCounter = null;
        private PerformanceCounter _aoaIndexerCounter = null;
        private PerformanceCounter _aoaIndicatorCounter = null;
        private PerformanceCounter _cautionPanelCounter = null;
        private PerformanceCounter _cmdsPanelCounter = null;
        private PerformanceCounter _compassCounter = null;
        private PerformanceCounter _dedCounter = null;
        private PerformanceCounter _pflCounter = null;
        private PerformanceCounter _epuFuelCounter = null;
        private PerformanceCounter _accelerometerCounter = null;
        private PerformanceCounter _ftit1Counter = null;
        private PerformanceCounter _ftit2Counter = null;
        private PerformanceCounter _fuelFlowCounter = null;
        private PerformanceCounter _isisCounter = null;
        private PerformanceCounter _fuelQuantityCounter = null;
        private PerformanceCounter _hsiCounter = null;
        private PerformanceCounter _ehsiCounter = null;
        private PerformanceCounter _landingGearLightsCounter = null;
        private PerformanceCounter _nwsIndexerCounter = null;
        private PerformanceCounter _nozPos1Counter = null;
        private PerformanceCounter _nozPos2Counter = null;
        private PerformanceCounter _oilGauge1Counter = null;
        private PerformanceCounter _oilGauge2Counter = null;
        private PerformanceCounter _rwrCounter = null;
        private PerformanceCounter _speedbrakeCounter = null;
        private PerformanceCounter _rpm1Counter = null;
        private PerformanceCounter _rpm2Counter = null;
        private PerformanceCounter _vviCounter = null;
        private PerformanceCounter _hydACounter = null;
        private PerformanceCounter _hydBCounter = null;
        private PerformanceCounter _cabinPressCounter = null;
        private PerformanceCounter _rollTrimCounter = null;
        private PerformanceCounter _pitchTrimCounter = null;
        private GDIPlusOptions _gdiPlusOptions = new GDIPlusOptions();
        #endregion

        #region Public Property Backing Fields
        private volatile bool _nightMode = false;
        /// <summary>
        /// 3D-mode flag
        /// </summary>
        private volatile bool _threeDeeMode = false;
        /// <summary>
        /// 2D-mode primary-view flag
        /// </summary>
        private volatile bool _twoDeePrimaryView = true;
        /// <summary>
        /// Flag to indicate if the Extractor engine is currently running
        /// </summary>
        private volatile bool _running = false;
        /// <summary>
        /// Flag to trigger the Extractor engine's worker threads to keep running or stop
        /// </summary>
        private volatile bool _keepRunning = false;
        private volatile bool _testMode = false;
        private volatile bool _windowSizingOrMoving = false;

        /// <summary>
        /// Reference to the application's main form (for supplying to DirectInput)
        /// </summary>
        private Form _applicationForm = null;
        #endregion

        #region Capture Coordinates
        #region Primary 2D Mode Capture Coordinates
        /// <summary>
        /// MFD 4 2D primary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _primaryMfd4_2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// MFD 3 2D primary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _primaryMfd3_2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Left MFD 2D primary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _primaryLeftMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Right MFD 2D primary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _primaryRightMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// HUD 2D primary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _primaryHud2DInputRect = new Rectangle(0, 0, 0, 0);
        #endregion

        #region Secondary 2D Mode Capture Coordinates
        /// <summary>
        /// MFD #4 2D secondary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _secondaryMfd4_2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// MFD #3 2D secondary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _secondaryMfd3_2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Left MFD 2D secondary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _secondaryLeftMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Right MFD 2D secondary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _secondaryRightMfd2DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// HUD 2D secondary-view screen capture coordinates rectangle
        /// </summary>
        private Rectangle _secondaryHud2DInputRect = new Rectangle(0, 0, 0, 0);
        #endregion

        #region 3D Mode Image Source Coordinates
        /// <summary>
        /// MFD #4 3D image coordinates rectangle (with respect to the entire textures shared memory image)
        /// </summary>
        private Rectangle _mfd4_3DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// MFD #3 3D image coordinates rectangle (with respect to the entire textures shared memory image)
        /// </summary>
        private Rectangle _mfd3_3DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Left MFD 3D image coordinates rectangle (with respect to the entire textures shared memory image)
        /// </summary>
        private Rectangle _leftMfd3DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Right MFD 3D image coordinates rectangle (with respect to the entire textures shared memory image)
        /// </summary>
        private Rectangle _rightMfd3DInputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// HUD 3D image coordinates rectangle (with respect to the entire textures shared memory image)
        /// </summary>
        private Rectangle _hud3DInputRect = new Rectangle(0, 0, 0, 0);
        #endregion
        #endregion

        #region Output Window Coordinates
        /// <summary>
        /// MFD #4 output form's screen position coordinates and size (specified as a Rectangle)
        /// </summary>
        private Rectangle _mfd4_OutputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// MFD #3 output form's screen position coordinates and size (specified as a Rectangle)
        /// </summary>
        private Rectangle _mfd3_OutputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Left MFD output form's screen position coordinates and size (specified as a Rectangle)
        /// </summary>
        private Rectangle _leftMfdOutputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Right MFD output form's screen position coordinates and size (specified as a Rectangle)
        /// </summary>
        private Rectangle _rightMfdOutputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// HUD output form's screen position coordinates and size (specified as a Rectangle)
        /// </summary>
        private Rectangle _hudOutputRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// Flag to indicate if the Extractor engine is running in Test Mode
        /// </summary>
        #endregion

        #region Falcon 4 Sharedmem Readers & status flags
        private F4Utils.Terrain.TerrainBrowser _terrainBrowser = new F4Utils.Terrain.TerrainBrowser(false);
        /// <summary>
        /// Reference to a Reader object that can read images from BMS's "textures shared memory" 
        /// area -- this reference is used to perform the actual 3D-mode image extraction
        /// </summary>
        private F4TexSharedMem.Reader _texSmReader = new F4TexSharedMem.Reader();
        private object _texSmReaderLock = new object();
        /// <summary>
        /// Reference to a Reader object that can read images from BMS's "textures shared memory" area 
        /// -- this reference is used to detect whether the 3D-mode shared 
        /// memory images actually exist or not (can be recreated at certain 
        /// intervals without affecting code using the other reference)
        /// </summary>
        private F4TexSharedMem.Reader _texSmStatusReader = new F4TexSharedMem.Reader();
        /// <summary>
        /// Reference to a Reader object that can read values from Falcon's basic (non-textures) shared
        /// memory area.  This is used to detect whether Falcon is running and to provide flight data to rendered instruments
        /// </summary>
        private F4SharedMem.Reader _falconSmReader = null;
        private F4SharedMem.FlightData _flightData = null;
        private bool _useBMSAdvancedSharedmemValues = false;
        /// <summary>
        /// Flag to indicate whether the sim is running
        /// </summary>
        public static bool _simRunning = false;
        /// <summary>
        /// Flag to indicate whether BMS's 3D textures shared memory area is available and has data
        /// </summary>
        private bool _sim3DDataAvailable = false;
        #endregion

        #region Blank Images
        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 image data is not available
        /// </summary>
        private Image _mfd4BlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDBlankImage); //TODO: change to MFD4
        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 image data is not available
        /// </summary>
        private Image _mfd3BlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDBlankImage); //TODO: change to MFD3
        /// <summary>
        /// Reference to a bitmap to display when the Left MFD image data is not available
        /// </summary>
        private Image _leftMfdBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDBlankImage);
        /// <summary>
        /// Reference to a bitmap to display when the Right MFD image data is not available
        /// </summary>
        private Image _rightMfdBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDBlankImage);
        /// <summary>
        /// Reference to a bitmap to display when the HUD image data is not available
        /// </summary>
        private Image _hudBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.hudBlankImage);
        #endregion

        #region Test/Alignment Images
        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 is in test/alignment mode
        /// </summary>
        private Image _mfd4TestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);//TODO: change to MFD4
        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 is in test/alignment mode
        /// </summary>
        private Image _mfd3TestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);//TODO: change to MFD3
        /// <summary>
        /// Reference to a bitmap to display when the Left MFD is in test/alignment mode
        /// </summary>
        private Image _leftMfdTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);
        /// <summary>
        /// Reference to a bitmap to display when the Right MFD is in test/alignment mode
        /// </summary>
        private Image _rightMfdTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDTestAlignmentImage);
        /// <summary>
        /// Reference to a bitmap to display when the HUD is in test/alignment mode
        /// </summary>
        private Image _hudTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.hudTestAlignmentImage);
        #endregion

        #region Network Configuration
        private string _imageFormat = "PNG";
        private string _compressionType = "None";
        /// <summary>
        /// Endpoint address (IP address/port number/service name) of an Extractor engine 
        /// running in Server mode
        /// </summary>
        private IPEndPoint _serverEndpoint = null;
        /// <summary>
        /// Reference to a Client object that can read data from a networked Extractor engine running
        /// in Server mode
        /// </summary>
        private Networking.ExtractorClient _client = null;
        /// <summary>
        /// Setting that indicates which networking mode this instance of the Extractor is configured to 
        /// operate under (server, client, stand-alone)
        /// </summary>
        private NetworkMode _networkMode = NetworkMode.Standalone;
        /// <summary>
        /// Service name to use for this instance of the Extractor engine, if running in Server mode
        /// </summary>
        private const string _serviceName = "MFDExtractorService";
        #endregion

        #region Public Events
        /// <summary>
        /// Event declaration for the DataChanged event
        /// </summary>
        public event EventHandler DataChanged;
        /// <summary>
        /// Event declaration for the Started event
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Event declaration for the Stopping event
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Event declaration for the Stopped event
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// Event declaration for the Starting event
        /// </summary>
        public event EventHandler Starting;
        #endregion

        #region Thread Synchronization Signals
        private AutoResetEvent _mfd4CaptureStart = new AutoResetEvent(false);
        private AutoResetEvent _mfd3CaptureStart = new AutoResetEvent(false);
        private AutoResetEvent _leftMfdCaptureStart = new AutoResetEvent(false);
        private AutoResetEvent _rightMfdCaptureStart = new AutoResetEvent(false);
        private AutoResetEvent _hudCaptureStart = new AutoResetEvent(false);

        private struct InstrumentStateSnapshot
        {
            public int HashCode;
            public DateTime DateTime;
        }
        private Dictionary<IInstrumentRenderer, InstrumentStateSnapshot> _instrumentStates = new Dictionary<IInstrumentRenderer, InstrumentStateSnapshot>();

        private AutoResetEvent _adiRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _backupAdiRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _asiRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _altimeterRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _aoaIndexerRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _aoaIndicatorRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _cautionPanelRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _cmdsPanelRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _compassRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _dedRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _pflRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _epuFuelRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _accelerometerRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _ftit1RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _ftit2RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _fuelFlowRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _isisRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _fuelQuantityRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _hsiRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _ehsiRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _landingGearLightsRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _nwsIndexerRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _nozPos1RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _nozPos2RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _oilGauge1RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _oilGauge2RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _rwrRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _speedbrakeRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _rpm1RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _rpm2RenderStart = new AutoResetEvent(false);
        private AutoResetEvent _vviRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _hydARenderStart = new AutoResetEvent(false);
        private AutoResetEvent _hydBRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _cabinPressRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _rollTrimRenderStart = new AutoResetEvent(false);
        private AutoResetEvent _pitchTrimRenderStart = new AutoResetEvent(false);


        private AutoResetEvent _mfd4CaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _mfd3CaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _leftMfdCaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _rightMfdCaptureEnd = new AutoResetEvent(false);
        private AutoResetEvent _hudCaptureEnd = new AutoResetEvent(false);

        private AutoResetEvent _adiRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _backupAdiRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _asiRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _altimeterRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _aoaIndexerRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _aoaIndicatorRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _cautionPanelRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _cmdsPanelRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _compassRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _dedRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _pflRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _epuFuelRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _accelerometerRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _ftit1RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _ftit2RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _fuelFlowRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _isisRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _fuelQuantityRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _hsiRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _ehsiRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _landingGearLightsRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _nwsIndexerRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _nozPos1RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _nozPos2RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _oilGauge1RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _oilGauge2RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _rwrRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _speedbrakeRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _rpm1RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _rpm2RenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _vviRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _hydARenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _hydBRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _cabinPressRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _rollTrimRenderEnd = new AutoResetEvent(false);
        private AutoResetEvent _pitchTrimRenderEnd = new AutoResetEvent(false);

        #endregion

        #region Threads
        private volatile bool _settingsSaveScheduled = false;
        private volatile bool _settingsLoadScheduled = false;
        private BackgroundWorker _settingsSaverAsyncWorker = new BackgroundWorker();
        private BackgroundWorker _settingsLoaderAsyncWorker = new BackgroundWorker();
        private Thread _keyboardWatcherThread = null;
        /// <summary>
        /// Reference to the sim-is-running status monitor thread 
        /// </summary>
        private Thread _simStatusMonitorThread = null;
        /// <summary>
        /// Referencce to the thread that is responsible for orchestrating the image capture sequence
        /// </summary>
        private Thread _captureOrchestrationThread = null;
        /// <summary>
        /// Thread priority at which the Extractor worker threads should run
        /// </summary>
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;

        private Thread _mfd4CaptureThread = null;
        private Thread _mfd3CaptureThread = null;
        private Thread _leftMfdCaptureThread = null;
        private Thread _rightMfdCaptureThread = null;
        private Thread _hudCaptureThread = null;

        private Thread _adiRenderThread = null;
        private Thread _backupAdiRenderThread = null;
        private Thread _asiRenderThread = null;
        private Thread _altimeterRenderThread = null;
        private Thread _aoaIndexerRenderThread = null;
        private Thread _aoaIndicatorRenderThread = null;
        private Thread _cautionPanelRenderThread = null;
        private Thread _cmdsPanelRenderThread = null;
        private Thread _compassRenderThread = null;
        private Thread _dedRenderThread = null;
        private Thread _pflRenderThread = null;
        private Thread _epuFuelRenderThread = null;
        private Thread _accelerometerRenderThread = null;
        private Thread _ftit1RenderThread = null;
        private Thread _ftit2RenderThread = null;
        private Thread _fuelFlowRenderThread = null;
        private Thread _isisRenderThread = null;
        private Thread _fuelQuantityRenderThread = null;
        private Thread _hsiRenderThread = null;
        private Thread _ehsiRenderThread = null;
        private Thread _landingGearLightsRenderThread = null;
        private Thread _nwsIndexerRenderThread = null;
        private Thread _nozPos1RenderThread = null;
        private Thread _nozPos2RenderThread = null;
        private Thread _oilGauge1RenderThread = null;
        private Thread _oilGauge2RenderThread = null;
        private Thread _rwrRenderThread = null;
        private Thread _speedbrakeRenderThread = null;
        private Thread _rpm1RenderThread = null;
        private Thread _rpm2RenderThread = null;
        private Thread _vviRenderThread = null;
        private Thread _hydARenderThread = null;
        private Thread _hydBRenderThread = null;
        private Thread _cabinPressRenderThread = null;
        private Thread _rollTrimRenderThread = null;
        private Thread _pitchTrimRenderThread = null;
        private bool _keySettingsLoaded = false;
        private InputControlSelection _nvisKey = null;
        private InputControlSelection _airspeedIndexIncreaseKey = null;
        private InputControlSelection _airspeedIndexDecreaseKey = null;
        private InputControlSelection _ehsiMenuButtonDepressedKey = null;
        private InputControlSelection _ehsiHeadingIncreaseKey = null;
        private InputControlSelection _ehsiHeadingDecreaseKey = null;
        private InputControlSelection _ehsiCourseIncreaseKey = null;
        private InputControlSelection _ehsiCourseDecreaseKey = null;
        private InputControlSelection _ehsiCourseDepressedKey = null;
        private DateTime? _ehsiRightKnobDepressedTime = null;
        private DateTime? _ehsiRightKnobReleasedTime = null;
        private DateTime? _ehsiRightKnobLastActivityTime = null;
        private InputControlSelection _isisBrightButtonKey = null;
        private InputControlSelection _isisStandardButtonKey = null;
        private InputControlSelection _azimuthIndicatorBrightnessIncreaseKey = null;
        private InputControlSelection _azimuthIndicatorBrightnessDecreaseKey = null;
        private InputControlSelection _accelerometerResetKey = null;


        private Mediator.PhysicalControlStateChangedEventHandler _mediatorEventHandler = null;
        #endregion

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.  Private modifier hides this constructor, preventing instances
        /// of this class from being created arbitrarily.  User code must call the appropriate 
        /// Factory method (i.e. the .GetInstance() method ) to obtain an object reference.
        /// </summary>
        private Extractor()
            : base()
        {
            //load user settings when an instance of the Extractor engine is created by
            //one of the Factory methods
            LoadSettings();
            CreatePerformanceCounters();
            _mediatorEventHandler = new Mediator.PhysicalControlStateChangedEventHandler(Mediator_PhysicalControlStateChanged);
            if (!Properties.Settings.Default.DisableDirectInputMediator)
            {
                this.Mediator = new Mediator(null);
            }
            _settingsSaverAsyncWorker.DoWork += new DoWorkEventHandler(_settingsSaverAsyncWorker_DoWork);
            _settingsLoaderAsyncWorker.DoWork += new DoWorkEventHandler(_settingsLoaderAsyncWorker_DoWork);

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
                        ((e.KeyData & Keys.KeyCode) & ~Keys.LControlKey & ~Keys.RControlKey & ~Keys.LShiftKey & ~Keys.LMenu & ~Keys.RMenu) == Keys.None
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
                            ((modifiersInHotkey & Keys.Control) == Keys.Control && (modifiersPressedRightNow & Keys.Control) != Keys.Control)
                                ||
                            ((modifiersInHotkey & Keys.Shift) == Keys.Shift && (modifiersPressedRightNow & Keys.Shift) != Keys.Shift)
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
            int newBrightness = (int)Math.Floor(
                (float)((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Brightness +
                ((float)(((F16AzimuthIndicator)_rwrRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.AzimuthIndicatorBrightnessIncrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyAzimuthIndicatorBrightnessDecreased(bool relayToListeners)
        {
            int newBrightness = (int)Math.Floor(
                (float)((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Brightness -
                ((float)(((F16AzimuthIndicator)_rwrRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.AzimuthIndicatorBrightnessDecrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyISISBrightButtonDepressed(bool relayToListeners)
        {
            int newBrightness = ((F16ISIS)_isisRenderer).InstrumentState.MaxBrightness;
            ((F16ISIS)_isisRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.ISISBrightButtonDepressed.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }

        private void NotifyISISStandardButtonDepressed(bool relayToListeners)
        {
            int newBrightness = (int)Math.Floor(
                    ((float)((F16ISIS)_isisRenderer).InstrumentState.MaxBrightness) * 0.5f
                );
            ((F16ISIS)_isisRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.ISISStandardButtonDepressed.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
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
                F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgIncBy1");
                if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useIncrementByOne = true;
                }
            }
            if (useIncrementByOne)
            {
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiHdgIncBy1");
            }
            else
            {
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiHeadingInc");
            }
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSILeftKnobIncrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
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
                F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiHdgDecBy1");
                if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useDecrementByOne = true;
                }
            }
            if (useDecrementByOne)
            {
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiHdgDecBy1");
            }
            else
            {
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiHeadingDec");
            }
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSILeftKnobDecrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIRightKnobIncreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI)_ehsiRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_ehsiRenderer).InstrumentState.Brightness +
                    ((float)(((F16EHSI)_ehsiRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_ehsiRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useIncrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsIncBy1");
                    if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useIncrementByOne = true;
                    }
                }
                if (useIncrementByOne)
                {
                    F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiCrsIncBy1");
                }
                else
                {
                    F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiCourseInc");
                }
            }
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSIRightKnobIncrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }

        }
        private void NotifyEHSIRightKnobDecreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI)_ehsiRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_ehsiRenderer).InstrumentState.Brightness -
                    ((float)(((F16EHSI)_ehsiRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_ehsiRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useDecrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.KeyFileUtils.FindKeyBinding("SimHsiCrsDecBy1");
                    if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useDecrementByOne = true;
                    }
                }
                if (useDecrementByOne)
                {
                    F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiCrsDecBy1");
                }
                else
                {
                    F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimHsiCourseDec");
                }
            }
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSIRightKnobDecrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
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
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSIRightKnobDepressed.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
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
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSIRightKnobReleased.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyEHSIMenuButtonDepressed(bool relayToListeners)
        {
            F16EHSI.F16EHSIInstrumentState.InstrumentModes currentMode = ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode;
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
                ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = newMode.Value;
            }
            if (this.NetworkMode == NetworkMode.Standalone || this.NetworkMode == NetworkMode.Server)
            {
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon("SimStepHSIMode");
            }


            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EHSIMenuButtonDepressed.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyAirspeedIndexDecreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator)_asiRenderer).InstrumentState.AirspeedIndexKnots -= 2.5F;
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.AirspeedIndexDecrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }

        private void NotifyAirspeedIndexIncreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator)_asiRenderer).InstrumentState.AirspeedIndexKnots += 2.5F;
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.AirspeedIndexIncrease.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void ProcessPendingMessagesToServerFromClient()
        {
            if (this.NetworkMode != NetworkMode.Server) return;
            Networking.Message pendingMessage = Networking.ExtractorServer.GetNextPendingMessageToServerFromClient();
            while (pendingMessage != null)
            {
                Networking.MessageTypes messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
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
                pendingMessage = Networking.ExtractorServer.GetNextPendingMessageToServerFromClient();
            }

        }
        private void ProcessPendingMessagesToClientFromServer()
        {
            if (this.NetworkMode != NetworkMode.Client || _client == null) return;
            Networking.Message pendingMessage = _client.GetNextMessageToClientFromServer();
            while (pendingMessage != null)
            {
                Networking.MessageTypes messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
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
            ((F16Accelerometer)_accelerometerRenderer).InstrumentState.ResetMinAndMaxGs();
            if (relayToListeners)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.AccelerometerIsReset.ToString(), null);
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
                else if (this.NetworkMode == NetworkMode.Client && _client != null)
                {
                    _client.SendMessageToServer(msg);
                }
            }
        }
        private void NotifyNightModeIsToggled(bool relayToListeners)
        {
            this.NightMode = !this.NightMode;
            if (relayToListeners)
            {
                if (this.NetworkMode == NetworkMode.Server)
                {
                    Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.ToggleNightMode.ToString(), null);
                    Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
                }
            }
        }

        private void Mediator_PhysicalControlStateChanged(object sender, Common.InputSupport.PhysicalControlStateChangedEventArgs e)
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
                    !DirectInputHotkeyIsTriggering(_ehsiCourseDepressedKey)
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
        private bool DirectInputHotkeyIsTriggering(InputControlSelection hotkey)
        {
            if (hotkey == null || hotkey.DirectInputControl ==null) return false;
            int? currentVal = Mediator.GetPhysicalControlValue(hotkey.DirectInputControl, StateType.Current);
            int? prevVal = Mediator.GetPhysicalControlValue(hotkey.DirectInputControl, StateType.Previous);

            switch (hotkey.ControlType)
            {
                case ControlType.Unknown:
                    break;
                case ControlType.Axis:
                    if (currentVal.HasValue && !prevVal.HasValue)
                    {
                        return true;
                    }
                    else if (!currentVal.HasValue && prevVal.HasValue)
                    {
                        return true;
                    }
                    else
                    {
                        return (currentVal.Value != prevVal.Value);
                    }
                    break;
                case ControlType.Button:
                    return (currentVal.HasValue && currentVal.Value == 1);
                    break;
                case ControlType.Pov:
                    if (currentVal.HasValue)
                    {
                        return Common.InputSupport.Util.GetPovDirection(currentVal.Value) == hotkey.PovDirection;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case ControlType.Key:
                    return (currentVal.HasValue && currentVal.Value == 1);
                    break;
                default:
                    break;
            }
            return false;

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
                        (diEvent.Control.ControlType == ControlType.Axis && diEvent.Control.AxisType == hotkey.DirectInputControl.AxisType)
                            ||
                        (diEvent.Control.ControlType != ControlType.Axis)
                    )
                        &&
                    object.Equals(diEvent.Control.Parent.Key, hotkey.DirectInputDevice.Key)
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
            string keyFromSettingsString = Properties.Settings.Default.EHSIMenuButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiMenuButtonDepressedKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadEHSIRightKnobDepressedKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.EHSICourseKnobDepressedKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiCourseDepressedKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadEHSIRightKnobIncreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.EHSICourseIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiCourseIncreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadEHSIRightKnobDecreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.EHSICourseDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiCourseDecreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadEHSILeftKnobIncreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.EHSIHeadingIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiHeadingIncreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadEHSILeftKnobDecreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.EHSIHeadingDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _ehsiHeadingDecreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadAirspeedIndexIncreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.AirspeedIndexIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _airspeedIndexIncreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadAirspeedIndexDecreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.AirspeedIndexDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _airspeedIndexDecreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }

            }
        }
        private void LoadAccelerometerResetKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.AccelerometerResetKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _accelerometerResetKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadNvisKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.NVISKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _nvisKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }

        private void LoadISISBrightButtonKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.ISISBrightButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _isisBrightButtonKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadISISStandardButtonKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.ISISStandardButtonKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _isisStandardButtonKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadAzimuthIndicatorBrightnessIncreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.AzimuthIndicatorBrightnessIncreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _azimuthIndicatorBrightnessIncreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        private void LoadAzimuthIndicatorBrightnessDecreaseKeySetting()
        {
            string keyFromSettingsString = Properties.Settings.Default.AzimuthIndicatorBrightnessDecreaseKey;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                InputControlSelection keyFromSettings = null;
                try
                {
                    keyFromSettings = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
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
                    _azimuthIndicatorBrightnessDecreaseKey = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the Extractor
        /// </summary>
        public void Start()
        {
            //if we're already running, just ignore the Start command
            if (_running)
            {
                return;
            }
            F4Utils.Process.KeyFileUtils.ResetCurrentKeyFile();
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
            _log.DebugFormat("Total time taken to invoke the Starting event on the extractor: {0}", startingEventTimeElapsed.TotalMilliseconds);

            if (_keySettingsLoaded == false) LoadKeySettings();
            if (this.Mediator != null)
            {
                this.Mediator.PhysicalControlStateChanged += _mediatorEventHandler;
            }
            DateTime beginSendNullImagesTime = DateTime.Now;
            SendMfd4Image(null);
            SendMfd3Image(null);
            SendLeftMfdImage(null);
            SendRightMfdImage(null);
            SendHudImage(null);
            DateTime endSendNullImagesTime = DateTime.Now;
            TimeSpan sendNullImagesTimeElapsed = endSendNullImagesTime.Subtract(beginSendNullImagesTime);
            _log.DebugFormat("Total time taken to send null images on the extractor: {0}", sendNullImagesTimeElapsed.TotalMilliseconds);

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
        /// <summary>
        /// Stops the Extractor engine
        /// </summary>
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
            _log.DebugFormat("Total time taken to invoke the Stopping event on the extractor: {0}", stoppingEventTimeElapsed.TotalMilliseconds);


            //clear global flag that worker threads use to determine if their work loops should continue
            _keepRunning = false;
            if (this.Mediator != null)
            {
                this.Mediator.PhysicalControlStateChanged -= _mediatorEventHandler;
            }
            _keySettingsLoaded = false;

            DateTime beginEndingThreadsTime = DateTime.Now;

            Thread[] threadsToKill = new Thread[] {
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
            _log.DebugFormat("Total time taken to end threads on the extractor: {0}", endingThreadsTimeElapsed.TotalMilliseconds);


            DateTime beginClosingOutputWindowFormsTime = DateTime.Now;
            CloseOutputWindowForms();
            DateTime endClosingOutputWindowFormsTime = DateTime.Now;
            TimeSpan closingOutputWindowFormsTimeElapsed = endClosingOutputWindowFormsTime.Subtract(beginClosingOutputWindowFormsTime);
            _log.DebugFormat("Total time taken to close output windows on the extractor: {0}", closingOutputWindowFormsTimeElapsed.TotalMilliseconds);



            DateTime beginTearDownImageServerTime = DateTime.Now;
            //if we're in Server mode, tear down the .NET Remoting channel
            if (_networkMode == NetworkMode.Server)
            {
                TearDownImageServer();
            }
            DateTime endTearDownImageServerTime = DateTime.Now;
            TimeSpan tearDownImageServerTimeElapsed = endTearDownImageServerTime.Subtract(beginTearDownImageServerTime);
            _log.DebugFormat("Total time taken to tear down the image server on the extractor: {0}", tearDownImageServerTimeElapsed.TotalMilliseconds);

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
            _log.DebugFormat("Total time taken to invoke the Stopped event on the extractor: {0}", stoppedEventElapsedTime.TotalMilliseconds);


            DateTime endStoppingTime = DateTime.Now;
            TimeSpan totalElapsed = endStoppingTime.Subtract(beginStoppingTime);
            _log.DebugFormat("Extractor engine stopped at : {0}", DateTime.Now.ToString());
            _log.DebugFormat("Total time taken to stop the extractor engine (in milliseconds): {0}", totalElapsed.TotalMilliseconds);

        }
        /// <summary>
        /// Calls Dispose() on the current Extractor instance
        /// </summary>
        /// <summary>
        /// Factory Method to create or return an instance of the Extractor.  Only one instance can be active within an AppDomain (Singleton pattern)
        /// </summary>
        /// <returns></returns>
        public static Extractor GetInstance()
        {
            if (_extractor == null)
            {
                _extractor = new Extractor();
            }
            return _extractor;
        }
        /// <summary>
        /// Reads the user settings file from disk or the current in-memory user settings cache
        /// </summary>
        public void LoadSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;
            LoadGDIPlusSettings();
            _networkMode = (NetworkMode)settings.NetworkingMode;
            if (_networkMode == NetworkMode.Server)
            {
                _serverEndpoint = new IPEndPoint(IPAddress.Any, settings.ServerUsePortNumber);
            }
            else if (_networkMode == NetworkMode.Client)
            {
                _serverEndpoint = new IPEndPoint(IPAddress.Parse(settings.ClientUseServerIpAddress), settings.ClientUseServerPortNum);
            }
            if (_networkMode == NetworkMode.Server || _networkMode == NetworkMode.Standalone)
            {
                _primaryMfd4_2DInputRect = Rectangle.FromLTRB(settings.Primary_MFD4_2D_ULX, settings.Primary_MFD4_2D_ULY, settings.Primary_MFD4_2D_LRX, settings.Primary_MFD4_2D_LRY);
                _primaryMfd3_2DInputRect = Rectangle.FromLTRB(settings.Primary_MFD3_2D_ULX, settings.Primary_MFD3_2D_ULY, settings.Primary_MFD3_2D_LRX, settings.Primary_MFD3_2D_LRY);
                _primaryLeftMfd2DInputRect = Rectangle.FromLTRB(settings.Primary_LMFD_2D_ULX, settings.Primary_LMFD_2D_ULY, settings.Primary_LMFD_2D_LRX, settings.Primary_LMFD_2D_LRY);
                _primaryRightMfd2DInputRect = Rectangle.FromLTRB(settings.Primary_RMFD_2D_ULX, settings.Primary_RMFD_2D_ULY, settings.Primary_RMFD_2D_LRX, settings.Primary_RMFD_2D_LRY);
                _primaryHud2DInputRect = Rectangle.FromLTRB(settings.Primary_HUD_2D_ULX, settings.Primary_HUD_2D_ULY, settings.Primary_HUD_2D_LRX, settings.Primary_HUD_2D_LRY);

                _secondaryMfd4_2DInputRect = Rectangle.FromLTRB(settings.Secondary_MFD4_2D_ULX, settings.Secondary_MFD4_2D_ULY, settings.Secondary_MFD4_2D_LRX, settings.Secondary_MFD4_2D_LRY);
                _secondaryMfd3_2DInputRect = Rectangle.FromLTRB(settings.Secondary_MFD3_2D_ULX, settings.Secondary_MFD3_2D_ULY, settings.Secondary_MFD3_2D_LRX, settings.Secondary_MFD3_2D_LRY);
                _secondaryLeftMfd2DInputRect = Rectangle.FromLTRB(settings.Secondary_LMFD_2D_ULX, settings.Secondary_LMFD_2D_ULY, settings.Secondary_LMFD_2D_LRX, settings.Secondary_LMFD_2D_LRY);
                _secondaryRightMfd2DInputRect = Rectangle.FromLTRB(settings.Secondary_RMFD_2D_ULX, settings.Secondary_RMFD_2D_ULY, settings.Secondary_RMFD_2D_LRX, settings.Secondary_RMFD_2D_LRY);
                _secondaryHud2DInputRect = Rectangle.FromLTRB(settings.Secondary_HUD_2D_ULX, settings.Secondary_HUD_2D_ULY, settings.Secondary_HUD_2D_LRX, settings.Secondary_HUD_2D_LRY);
            }
            _mfd4_OutputRect = Rectangle.FromLTRB(settings.MFD4_OutULX, settings.MFD4_OutULY, settings.MFD4_OutLRX, settings.MFD4_OutLRY);
            _mfd3_OutputRect = Rectangle.FromLTRB(settings.MFD3_OutULX, settings.MFD3_OutULY, settings.MFD3_OutLRX, settings.MFD3_OutLRY);
            _leftMfdOutputRect = Rectangle.FromLTRB(settings.LMFD_OutULX, settings.LMFD_OutULY, settings.LMFD_OutLRX, settings.LMFD_OutLRY);
            _rightMfdOutputRect = Rectangle.FromLTRB(settings.RMFD_OutULX, settings.RMFD_OutULY, settings.RMFD_OutLRX, settings.RMFD_OutLRY);
            _hudOutputRect = Rectangle.FromLTRB(settings.HUD_OutULX, settings.HUD_OutULY, settings.HUD_OutLRX, settings.HUD_OutLRY);

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
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets a reference to the application's main form (if there is one) -- required for DirectInput event notifications
        /// </summary>
        public Form ApplicationForm
        {
            get
            {
                return _applicationForm;
            }
            set
            {
                _applicationForm = value;
            }
        }
        /// <summary>
        /// The IP Address of the Server that the Extractor should connect to when running in Client mode
        /// </summary>
        public IPEndPoint ServerEndpoint
        {
            get
            {
                return _serverEndpoint;
            }
            set
            {
                _serverEndpoint = value;
            }
        }
        /// <summary>
        /// A value from the <see cref="NetworkModes"/> enumeration, indicating which network mode the Extractor should run as
        /// </summary>
        public NetworkMode NetworkMode
        {
            get
            {
                return _networkMode;
            }
            set
            {
                _networkMode = value;
            }
        }
        /// <summary>
        /// Indicates whether the Extractor is currently running
        /// </summary>
        public bool Running
        {
            get
            {
                return _running;
            }
        }
        /// <summary>
        /// When read from, this proeprty indicates whether the Extractor is in Test mode; when written to, this property causes the Extrctor to enter Test mode (if <see langword="true"/>), or to exit test mode (if <see langword="false"/>)
        /// </summary>
        public bool TestMode
        {
            get
            {
                return _testMode;
            }
            set
            {
                _testMode = value;
                Properties.Settings.Default.TestMode = _testMode;
            }
        }
        /// <summary>
        /// Controls whether the Extractor (in 2D cockpit extraction mode) uses the primary 2D view capture coordinates or the secondary 2D view capture coordinates
        /// </summary>
        public bool TwoDeePrimaryView
        {
            get { return _twoDeePrimaryView; }
            set { _twoDeePrimaryView = value; }
        }
        /// <summary>
        /// When read from, this proeprty indicates whether the Extractor is in 3D cockpit extraction mode; when written to, this property causes the Extrctor to enter 3D cockpit extraction mode (if <see langword="true"/>), or to exit 3D cockpit extraction mode [switching back to 2D cockpit extraction mode] (if <see langword="false"/>)
        /// </summary>
        public bool ThreeDeeMode
        {
            get
            {
                return _threeDeeMode;
            }
            set
            {
                _threeDeeMode = value;
            }
        }
        /// <summary>
        /// Get/sets Night Vision Mode
        /// </summary>
        public bool NightMode
        {
            get
            {
                return _nightMode;
            }
            set
            {
                _nightMode = value;
            }
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
        /// <summary>
        /// Establishes a .NET Remoting-based connection to a remote MFD Extractor server
        /// </summary>
        private void SetupNetworkingClient()
        {
            try
            {
                _client = new Networking.ExtractorClient(_serverEndpoint, _serviceName);
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
            }
        }
        /// <summary>
        /// Opens a .NET Remoting-based network server channel that remote clients can connect to
        /// </summary>
        private void SetupNetworkingServer()
        {
            Networking.ExtractorServer.CreateService(_serviceName, _serverEndpoint.Port, _compressionType, _imageFormat);
        }
        /// <summary>
        /// Closes the .NET Remoting image server channel
        /// </summary>
        private void TearDownImageServer()
        {
            Networking.ExtractorServer.TearDownService(_serverEndpoint.Port);
        }
        #endregion
        #region MFD Network Image Transfer Code
        #region Outbound Transfer
        private void SendFlightData(FlightData flightData)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetFlightData(flightData);
            }
        }
        /// <summary>
        /// Makes an image representing the current MFD #4 display available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the current contents of MFD #4</param>
        private void SendMfd4Image(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetMfd4Bitmap(image);
            }
        }
        /// <summary>
        /// Makes an image representing the current MFD #3 display available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the current contents of MFD #3</param>
        private void SendMfd3Image(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetMfd3Bitmap(image);
            }
        }
        /// <summary>
        /// Makes an image representing the current Left MFD display available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the current contents of the Left MFD</param>
        private void SendLeftMfdImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetLeftMfdBitmap(image);
            }
        }
        /// <summary>
        /// Makes an image representing the current Right MFD display available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the current contents of the Right MFD</param>
        private void SendRightMfdImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetRightMfdBitmap(image);
            }
        }
        /// <summary>
        /// Makes an image representing the current HUD display available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the current contents of the HUD</param>
        private void SendHudImage(Image image)
        {
            if (_networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.SetHudBitmap(image);
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
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        /// <summary>
        /// Reads the current MFD #4 image from the remote (networked) images server
        /// </summary>
        /// <returns>a Bitmap representing the currently-available MFD #4 image</returns>
        private Image ReadMfd4FromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetMfd4Bitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        /// <summary>
        /// Reads the current MFD #3 image from the remote (networked) images server
        /// </summary>
        /// <returns>a Bitmap representing the currently-available MFD #3 image</returns>
        private Image ReadMfd3FromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetMfd3Bitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        /// <summary>
        /// Reads the current Left MFD image from the remote (networked) images server
        /// </summary>
        /// <returns>a Bitmap representing the currently-available Left MFD image</returns>
        private Image ReadLeftMfdFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetLeftMfdBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        /// <summary>
        /// Reads the current Right MFD image from the remote (networked) images server
        /// </summary>
        /// <returns>a Bitmap representing the currently-available Right MFD image</returns>
        private Image ReadRightMfdFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetRightMfdBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        /// <summary>
        /// Reads the current HUD image from the remote (networked) images server
        /// </summary>
        /// <returns>a Bitmap representing the currently-available HUD image</returns>
        private Image ReadHudFromNetwork()
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetHudBitmap();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
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
            _gdiPlusOptions.CompositingQuality = Properties.Settings.Default.CompositingQuality;
            _gdiPlusOptions.InterpolationMode = Properties.Settings.Default.InterpolationMode;
            _gdiPlusOptions.PixelOffsetMode = Properties.Settings.Default.PixelOffsetMode;
            _gdiPlusOptions.SmoothingMode = Properties.Settings.Default.SmoothingMode;
            _gdiPlusOptions.TextRenderingHint = Properties.Settings.Default.TextRenderingHint;
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
            Properties.Settings.Default.Save();
            _settingsSaveScheduled = false;
        }
        #endregion

        #region Instrument Renderer Setup
        private void SetupInstrumentRenderers()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting setting up instrument renderers at: {0}", startTime.ToString());
            bool createAllRenderers = true;
            if (createAllRenderers || Properties.Settings.Default.EnableADIOutput)
            {
                SetupADIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableBackupADIOutput)
            {
                SetupBackupADIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableASIOutput)
            {
                SetupASIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableAltimeterOutput)
            {
                SetupAltimeterRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableAOAIndexerOutput)
            {
                SetupAOAIndexerRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableAOAIndicatorOutput)
            {
                SetupAOAIndicatorRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableCautionPanelOutput)
            {
                SetupCautionPanelRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableCMDSOutput)
            {
                SetupCMDSPanelRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableCompassOutput)
            {
                SetupCompassRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableDEDOutput)
            {
                SetupDEDRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnablePFLOutput)
            {
                SetupPFLRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableEPUFuelOutput)
            {
                SetupEPUFuelRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableAccelerometerOutput)
            {
                SetupAccelerometerRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableFTIT1Output)
            {
                SetupFTIT1Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableFTIT2Output)
            {
                SetupFTIT2Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableFuelFlowOutput)
            {
                SetupFuelFlowRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableISISOutput)
            {
                SetupISISRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableFuelQuantityOutput)
            {
                SetupFuelQuantityRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableHSIOutput)
            {
                SetupHSIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableEHSIOutput)
            {
                SetupEHSIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableGearLightsOutput)
            {
                SetupLandingGearLightsRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableNWSIndexerOutput)
            {
                SetupNWSIndexerRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableNOZ1Output)
            {
                SetupNOZ1Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableNOZ2Output)
            {
                SetupNOZ2Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableOIL1Output)
            {
                SetupOil1Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableOIL2Output)
            {
                SetupOil2Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableRWROutput)
            {
                SetupRWRRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableSpeedbrakeOutput)
            {
                SetupSpeedbrakeRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableRPM1Output)
            {
                SetupRPM1Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableRPM2Output)
            {
                SetupRPM2Renderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableVVIOutput)
            {
                SetupVVIRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableHYDAOutput)
            {
                SetupHydARenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableHYDBOutput)
            {
                SetupHydBRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableCabinPressOutput)
            {
                SetupCabinPressRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnableRollTrimOutput)
            {
                SetupRollTrimRenderer();
            }
            if (createAllRenderers || Properties.Settings.Default.EnablePitchTrimOutput)
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
            string vviStyleString = Properties.Settings.Default.VVI_Style;
            VVIStyles vviStyle = (VVIStyles)Enum.Parse(typeof(VVIStyles), vviStyleString);
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
            ((F16Tachometer)_rpm2Renderer).Options.IsSecondary = true;
        }
        private void SetupRPM1Renderer()
        {
            _rpm1Renderer = new F16Tachometer();
            ((F16Tachometer)_rpm1Renderer).Options.IsSecondary = false;
        }
        private void SetupSpeedbrakeRenderer()
        {
            _speedbrakeRenderer = new F16SpeedbrakeIndicator();
        }
        private void SetupRWRRenderer()
        {
            _rwrRenderer = new F16AzimuthIndicator();
            string styleString = Properties.Settings.Default.AzimuthIndicatorType;
            F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle style = (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)Enum.Parse(typeof(F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), styleString);
            ((F16AzimuthIndicator)_rwrRenderer).Options.Style = style;
            ((F16AzimuthIndicator)_rwrRenderer).Options.HideBezel = !Properties.Settings.Default.AzimuthIndicator_ShowBezel;
            ((F16AzimuthIndicator)_rwrRenderer).Options.GDIPlusOptions = _gdiPlusOptions;

        }
        private void SetupOil2Renderer()
        {
            _oilGauge2Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge)_oilGauge2Renderer).Options.IsSecondary = true;
        }
        private void SetupOil1Renderer()
        {
            _oilGauge1Renderer = new F16OilPressureGauge();
            ((F16OilPressureGauge)_oilGauge1Renderer).Options.IsSecondary = false;
        }
        private void SetupNOZ2Renderer()
        {
            _nozPos2Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator)_nozPos2Renderer).Options.IsSecondary = true;
        }
        private void SetupNOZ1Renderer()
        {
            _nozPos1Renderer = new F16NozzlePositionIndicator();
            ((F16NozzlePositionIndicator)_nozPos1Renderer).Options.IsSecondary = false;
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
            ((F16EHSI)_ehsiRenderer).Options.GDIPlusOptions = _gdiPlusOptions;
        }
        private void SetupFuelQuantityRenderer()
        {
            _fuelQuantityRenderer = new F16FuelQuantityIndicator();
            if (Properties.Settings.Default.FuelQuantityIndicator_NeedleCModel)
            {
                ((F16FuelQuantityIndicator)_fuelQuantityRenderer).Options.NeedleType = F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.CModel;
            }
            else
            {
                ((F16FuelQuantityIndicator)_fuelQuantityRenderer).Options.NeedleType = F16FuelQuantityIndicator.F16FuelQuantityIndicatorOptions.F16FuelQuantityNeedleType.DModel;
            }

        }
        private void SetupFuelFlowRenderer()
        {
            _fuelFlowRenderer = new F16FuelFlow();
        }
        private void SetupISISRenderer()
        {
            _isisRenderer = new F16ISIS();
            string pressureUnitsString = Properties.Settings.Default.ISIS_PressureUnits;
            if (!string.IsNullOrEmpty(pressureUnitsString))
            {
                try
                {
                    ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = (F16ISIS.F16ISISOptions.PressureUnits)Enum.Parse(typeof(F16ISIS.F16ISISOptions.PressureUnits), pressureUnitsString);
                }
                catch (Exception e)
                {
                    ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                }
            }
            ((F16ISIS)_isisRenderer).Options.GDIPlusOptions = _gdiPlusOptions;

        }
        private void SetupAccelerometerRenderer()
        {
            _accelerometerRenderer = new F16Accelerometer();
        }
        private void SetupFTIT2Renderer()
        {
            _ftit2Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature)_ftit2Renderer).Options.IsSecondary = true;
        }
        private void SetupFTIT1Renderer()
        {
            _ftit1Renderer = new F16FanTurbineInletTemperature();
            ((F16FanTurbineInletTemperature)_ftit1Renderer).Options.IsSecondary = false;
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

            string altimeterSyleString = Properties.Settings.Default.Altimeter_Style;
            F16Altimeter.F16AltimeterOptions.F16AltimeterStyle altimeterStyle = (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)Enum.Parse(typeof(F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterSyleString);
            ((F16Altimeter)_altimeterRenderer).Options.Style = altimeterStyle;

            string pressureUnitsString = Properties.Settings.Default.Altimeter_PressureUnits;
            F16Altimeter.F16AltimeterOptions.PressureUnits pressureUnits = (F16Altimeter.F16AltimeterOptions.PressureUnits)Enum.Parse(typeof(F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits = pressureUnits;
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
                            if (exePath != null) verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);
                            //12-08-12 Falcas change verInfo.ProductMinorPart >= 6826 to verInfo.ProductMinorPart >= 32
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4 && verInfo != null && ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >= 32) || (verInfo.ProductMajorPart > 4)))
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
                            if (Properties.Settings.Default.EnableISISOutput || NetworkMode == NetworkMode.Server)
                            {
                                computeRalt = true;
                            }
                            if (computeRalt)
                            {
                                if (_terrainBrowser == null)
                                {
                                    _terrainBrowser = new F4Utils.Terrain.TerrainBrowser(false);
                                    _terrainBrowser.LoadCurrentTheaterTerrainDatabase();
                                }
                                if (_terrainBrowser != null && toReturn != null)
                                {
                                    FlightDataExtension extensionData = new FlightDataExtension();
                                    float terrainHeight = _terrainBrowser.GetTerrainHeight(toReturn.x, toReturn.y);
                                    float ralt = -toReturn.z - terrainHeight;

                                    //reset AGL altitude to zero if we're on the ground
                                    if (
                                        ((toReturn.lightBits & (int)F4SharedMem.Headers.LightBits.WOW) == (int)F4SharedMem.Headers.LightBits.WOW)
                                          ||
                                          (
                                            ((toReturn.lightBits3 & (int)F4SharedMem.Headers.Bms4LightBits3.OnGround) == (int)F4SharedMem.Headers.Bms4LightBits3.OnGround)
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
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.DisableBMSAdvancedSharedmemValues.ToString(), null);
                Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
            }
        }

        private void EnableBMSAdvancedSharedmemValues()
        {
            _useBMSAdvancedSharedmemValues = true;
            if (NetworkMode == NetworkMode.Server)
            {
                Networking.Message msg = new MFDExtractor.Networking.Message(MessageTypes.EnableBMSAdvancedSharedmemValues.ToString(), null);
                Networking.ExtractorServer.SubmitMessageToClientFromServer(msg);
            }
        }
        /// <summary>
        /// Returns the current MFD #4 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #4 image</returns>
        private Image GetMfd4Bitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(_mfd4TestAlignmentImage);
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
        /// <summary>
        /// Returns the current MFD #3 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #3 image</returns>
        private Image GetMfd3Bitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(_mfd3TestAlignmentImage);
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
        /// <summary>
        /// Returns the current Left MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Left MFD image</returns>
        private Image GetLeftMfdBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(_leftMfdTestAlignmentImage);
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
        /// <summary>
        /// Returns the current Right MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Right MFD image</returns>
        private Image GetRightMfdBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(_rightMfdTestAlignmentImage);
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
        /// <summary>
        /// Returns the current HUD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current HUD image</returns>
        private Image GetHudBitmap()
        {
            Image toReturn = null;
            if (_testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(_hudTestAlignmentImage);
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
        /// <summary>
        /// Reads the current HUD image from BMS's 3D Textures Shared Memory
        /// </summary>
        /// <returns>A Bitmap representing the current HUD image</returns>
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
                            toReturn = Common.Imaging.Util.CloneBitmap(_texSmReader.GetImage(_hud3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
            }
            return toReturn;
        }
        /// <summary>
        /// Reads the current MFD #4 image from BMS's 3D Textures Shared Memory
        /// </summary>
        /// <returns>A Bitmap representing the current MFD #4 image</returns>
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
                            toReturn = Common.Imaging.Util.CloneBitmap(_texSmReader.GetImage(_mfd4_3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
            }
            return toReturn;
        }
        /// <summary>
        /// Reads the current MFD #3 image from BMS's 3D Textures Shared Memory
        /// </summary>
        /// <returns>A Bitmap representing the current MFD #3 image</returns>
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
                            toReturn = Common.Imaging.Util.CloneBitmap(_texSmReader.GetImage(_mfd3_3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
            }
            return toReturn;
        }
        /// <summary>
        /// Reads the current Left MFD image from BMS's 3D Textures Shared Memory
        /// </summary>
        /// <returns>A Bitmap representing the current Left MFD image</returns>
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
                            toReturn = Common.Imaging.Util.CloneBitmap(_texSmReader.GetImage(_leftMfd3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
            }
            return toReturn;
        }
        /// <summary>
        /// Reads the current Right MFD image from BMS's 3D Textures Shared Memory
        /// </summary>
        /// <returns>A Bitmap representing the current Right MFD image</returns>
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
                            toReturn = Common.Imaging.Util.CloneBitmap(_texSmReader.GetImage(_rightMfd3DInputRect));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
            }
            return toReturn;
        }
        #endregion
        #region MFD Capturing implementation methods
        /// <summary>
        /// Captures and stores the current MFD #4 image
        /// </summary>
        private void CaptureMfd4()
        {
            if (Properties.Settings.Default.EnableMfd4Output || _networkMode == NetworkMode.Server)
            {
                Image mfd4Image = null;
                try
                {
                    mfd4Image = GetMfd4Bitmap();
                    if (mfd4Image == null)
                    {
                        mfd4Image = Common.Imaging.Util.CloneBitmap(_mfd4BlankImage);
                    }
                    SetMfd4Image(mfd4Image);
                    if (_mfd4PerfCounter != null)
                    {
                        _mfd4PerfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
                finally
                {
                    Common.Util.DisposeObject(mfd4Image);
                }
            }
        }
        /// <summary>
        /// Captures and stores the current MFD #3 image
        /// </summary>
        private void CaptureMfd3()
        {
            if (Properties.Settings.Default.EnableMfd3Output || _networkMode == NetworkMode.Server)
            {
                Image mfd3Image = null;
                try
                {
                    mfd3Image = GetMfd3Bitmap();
                    if (mfd3Image == null)
                    {
                        mfd3Image = Common.Imaging.Util.CloneBitmap(_mfd3BlankImage);
                    }
                    SetMfd3Image(mfd3Image);
                    if (_mfd3PerfCounter != null)
                    {
                        _mfd3PerfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
                finally
                {
                    Common.Util.DisposeObject(mfd3Image);
                }
            }
        }
        /// <summary>
        /// Captures and stores the current Left MFD image
        /// </summary>
        private void CaptureLeftMfd()
        {
            if (Properties.Settings.Default.EnableLeftMFDOutput || _networkMode == NetworkMode.Server)
            {
                Image leftMfdImage = null;
                try
                {
                    leftMfdImage = GetLeftMfdBitmap();
                    if (leftMfdImage == null)
                    {
                        leftMfdImage = Common.Imaging.Util.CloneBitmap(_leftMfdBlankImage);
                    }
                    SetLeftMfdImage(leftMfdImage);
                    if (_leftMfdPerfCounter != null)
                    {
                        _leftMfdPerfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
                finally
                {
                    Common.Util.DisposeObject(leftMfdImage);
                }
            }
        }
        /// <summary>
        /// Captures and stores the current Right MFD image
        /// </summary>
        private void CaptureRightMfd()
        {
            if (Properties.Settings.Default.EnableRightMFDOutput || _networkMode == NetworkMode.Server)
            {
                Image rightMfdImage = null;
                try
                {
                    rightMfdImage = GetRightMfdBitmap();
                    if (rightMfdImage == null)
                    {
                        rightMfdImage = Common.Imaging.Util.CloneBitmap(_rightMfdBlankImage);
                    }
                    SetRightMfdImage(rightMfdImage);
                    if (_rightMfdPerfCounter != null)
                    {
                        _rightMfdPerfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
                finally
                {
                    Common.Util.DisposeObject(rightMfdImage);
                }
            }
        }
        /// <summary>
        /// Captures and stores the current HUD image 
        /// </summary>
        private void CaptureHud()
        {
            if (Properties.Settings.Default.EnableHudOutput || _networkMode == NetworkMode.Server)
            {
                Image hudImage = null;
                try
                {
                    hudImage = GetHudBitmap();
                    if (hudImage == null)
                    {
                        hudImage = Common.Imaging.Util.CloneBitmap(_hudBlankImage);
                    }
                    SetHudImage(hudImage);
                    if (_hudPerfCounter != null)
                    {
                        _hudPerfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
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
        /// <summary>
        /// Stores the current Left MFD image and makes it available over the network to remote clients
        /// </summary>
        /// <param name="hudMfdImage">the current HUD image to store/send</param>
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
                    if (Properties.Settings.Default.HUD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        hudImage.RotateFlip(Properties.Settings.Default.HUD_RotateFlipType);
                    }
                    using (Graphics graphics = _hudForm.CreateGraphics())
                    {
                        if (Properties.Settings.Default.HUD_Monochrome)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (var compatible = Common.Imaging.Util.CopyBitmap(hudImage))
                            {
                                graphics.DrawImage(compatible, _hudForm.ClientRectangle, 0, 0, hudImage.Width, hudImage.Height, GraphicsUnit.Pixel, ia);
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
            ColorMatrix cm = new ColorMatrix(new float[][]{   
                                  new float[]{0.33f,0.33f,0.33f,0,0},
                                  new float[]{0.33f,0.33f,0.33f,0,0},
                                  new float[]{0.33f,0.33f,0.33f,0,0},
                                  new float[]{0,0,0,1,0,0},
                                  new float[]{0,0,0,0,1,0},
                                  new float[]{0,0,0,0,0,1}});
            return cm;
        }
        /// <summary>
        /// Stores the current MFD #4 image and makes it available over the network to remote clients
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
                    if (Properties.Settings.Default.MFD4_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd4Image.RotateFlip(Properties.Settings.Default.MFD4_RotateFlipType);
                    }
                    using (Graphics graphics = _mfd4Form.CreateGraphics())
                    {
                        if (Properties.Settings.Default.MFD4_Monochrome)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (var compatible = Common.Imaging.Util.CopyBitmap(mfd4Image))
                            {
                                graphics.DrawImage(compatible, _mfd4Form.ClientRectangle, 0, 0, mfd4Image.Width, mfd4Image.Height, GraphicsUnit.Pixel, ia);
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
        /// Stores the current MFD #3 image and makes it available over the network to remote clients
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
                    if (Properties.Settings.Default.MFD3_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        mfd3Image.RotateFlip(Properties.Settings.Default.MFD3_RotateFlipType);
                    }
                    using (Graphics graphics = _mfd3Form.CreateGraphics())
                    {
                        if (Properties.Settings.Default.MFD3_Monochrome)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (var compatible = Common.Imaging.Util.CopyBitmap(mfd3Image))
                            {
                                graphics.DrawImage(compatible, _mfd3Form.ClientRectangle, 0, 0, mfd3Image.Width, mfd3Image.Height, GraphicsUnit.Pixel, ia);
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
        /// Stores the current Left MFD image and makes it available over the network to remote clients
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
                    if (Properties.Settings.Default.LMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        leftMfdImage.RotateFlip(Properties.Settings.Default.LMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _leftMfdForm.CreateGraphics())
                    {
                        if (Properties.Settings.Default.LMFD_Monochrome)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (var compatible = Common.Imaging.Util.CopyBitmap(leftMfdImage))
                            {
                                graphics.DrawImage(compatible, _leftMfdForm.ClientRectangle, 0, 0, leftMfdImage.Width, leftMfdImage.Height, GraphicsUnit.Pixel, ia);
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
        /// Stores the current Right MFD image and makes it available over the network to remote clients
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
                    if (Properties.Settings.Default.RMFD_RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        rightMfdImage.RotateFlip(Properties.Settings.Default.RMFD_RotateFlipType);
                    }
                    using (Graphics graphics = _rightMfdForm.CreateGraphics())
                    {
                        if (Properties.Settings.Default.RMFD_Monochrome)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ia.SetColorMatrix(GetGreyscaleColorMatrix());
                            using (var compatible = Common.Imaging.Util.CopyBitmap(rightMfdImage))
                            {
                                graphics.DrawImage(compatible, _rightMfdForm.ClientRectangle, 0, 0, rightMfdImage.Width, rightMfdImage.Height, GraphicsUnit.Pixel, ia);
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
            if (Properties.Settings.Default.EnableMfd4Output || NetworkMode == NetworkMode.Server)
            {
                SetupMfd4Form();
            }
            if (Properties.Settings.Default.EnableMfd3Output || NetworkMode == NetworkMode.Server)
            {
                SetupMfd3Form();
            }
            if (Properties.Settings.Default.EnableLeftMFDOutput || NetworkMode == NetworkMode.Server)
            {
                SetupLeftMfdForm();
            }

            if (Properties.Settings.Default.EnableRightMFDOutput || NetworkMode == NetworkMode.Server)
            {
                SetupRightMfdForm();
            }

            if (Properties.Settings.Default.EnableNWSIndexerOutput)
            {
                SetupNWSIndexerForm();
            }
            if (Properties.Settings.Default.EnableAOAIndexerOutput)
            {
                SetupAOAIndexerForm();
            }
            if (Properties.Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server)
            {
                SetupHudForm();
            }
            if (Properties.Settings.Default.EnableAOAIndicatorOutput)
            {
                SetupAOAIndicatorForm();
            }
            if (Properties.Settings.Default.EnableVVIOutput)
            {
                SetupVVIForm();
            }
            if (Properties.Settings.Default.EnableADIOutput)
            {
                SetupADIForm();
            }
            if (Properties.Settings.Default.EnableBackupADIOutput)
            {
                SetupBackupADIForm();
            }
            if (Properties.Settings.Default.EnableASIOutput)
            {
                SetupASIForm();
            }
            if (Properties.Settings.Default.EnableAltimeterOutput)
            {
                SetupAltimeterForm();
            }
            if (Properties.Settings.Default.EnableCautionPanelOutput)
            {
                SetupCautionPanelForm();
            }
            if (Properties.Settings.Default.EnableCMDSOutput)
            {
                SetupCMDSPanelForm();
            }
            if (Properties.Settings.Default.EnableCompassOutput)
            {
                SetupCompassForm();
            }
            if (Properties.Settings.Default.EnableDEDOutput)
            {
                SetupDEDForm();
            }
            if (Properties.Settings.Default.EnablePFLOutput)
            {
                SetupPFLForm();
            }
            if (Properties.Settings.Default.EnableEPUFuelOutput)
            {
                SetupEPUFuelForm();
            }
            if (Properties.Settings.Default.EnableAccelerometerOutput)
            {
                SetupAccelerometerForm();
            }
            if (Properties.Settings.Default.EnableFTIT1Output)
            {
                SetupFTIT1Form();
            }
            if (Properties.Settings.Default.EnableFTIT2Output)
            {
                SetupFTIT2Form();
            }
            if (Properties.Settings.Default.EnableFuelFlowOutput)
            {
                SetupFuelFlowForm();
            }
            if (Properties.Settings.Default.EnableISISOutput)
            {
                SetupISISForm();
            }
            if (Properties.Settings.Default.EnableFuelQuantityOutput)
            {
                SetupFuelQuantityForm();
            }
            if (Properties.Settings.Default.EnableHSIOutput)
            {
                SetupHSIForm();
            }
            if (Properties.Settings.Default.EnableEHSIOutput)
            {
                SetupEHSIForm();
            }
            if (Properties.Settings.Default.EnableGearLightsOutput)
            {
                SetupGearLightsForm();
            }
            if (Properties.Settings.Default.EnableNOZ1Output)
            {
                SetupNOZ1Form();
            }
            if (Properties.Settings.Default.EnableNOZ2Output)
            {
                SetupNOZ2Form();
            }
            if (Properties.Settings.Default.EnableOIL1Output)
            {
                SetupOIL1Form();
            }
            if (Properties.Settings.Default.EnableOIL2Output)
            {
                SetupOIL2Form();
            }
            if (Properties.Settings.Default.EnableRWROutput)
            {
                SetupRWRForm();
            }
            if (Properties.Settings.Default.EnableSpeedbrakeOutput)
            {
                SetupSpeedbrakeForm();
            }
            if (Properties.Settings.Default.EnableRPM1Output)
            {
                SetupRPM1Form();
            }
            if (Properties.Settings.Default.EnableRPM2Output)
            {
                SetupRPM2Form();
            }
            if (Properties.Settings.Default.EnableHYDAOutput)
            {
                SetupHydAForm();
            }
            if (Properties.Settings.Default.EnableHYDBOutput)
            {
                SetupHydBForm();
            }
            if (Properties.Settings.Default.EnableCabinPressOutput)
            {
                SetupCabinPressForm();
            }
            if (Properties.Settings.Default.EnableRollTrimOutput)
            {
                SetupRollTrimForm();
            }
            if (Properties.Settings.Default.EnablePitchTrimOutput)
            {
                SetupPitchTrimForm();
            }
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Finished setting up output forms on the extractor at: {0}", endTime.ToString());
            _log.DebugFormat("Time taken to set up output forms on the extractor: {0}", elapsed.TotalMilliseconds);

        }
        #region MFD Forms Setup
        /// <summary>
        /// Sets up the MFD #4 image output window
        /// </summary>
        private void SetupMfd4Form()
        {
            if (Properties.Settings.Default.EnableMfd4Output)
            {
                Point location;
                Size size = new Size();
                _mfd4Form = new InstrumentForm();
                _mfd4Form.ShowInTaskbar = false;
                _mfd4Form.ShowIcon = false;
                _mfd4Form.Text = "MFD #4";
                if (Properties.Settings.Default.MFD4_StretchToFit)
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
                _mfd4Form.AlwaysOnTop = Properties.Settings.Default.MFD4_AlwaysOnTop;
                _mfd4Form.Monochrome = Properties.Settings.Default.MFD4_Monochrome;
                _mfd4Form.Rotation = Properties.Settings.Default.MFD4_RotateFlipType;
                _mfd4Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_mfd4Form, _applicationForm, _mfd4OutputScreen, location, size, true, true);
                using (Graphics graphics = _mfd4Form.CreateGraphics())
                {
                    graphics.DrawImage(_mfd4BlankImage, _mfd4Form.ClientRectangle);
                }
                _mfd4Form.DataChanged += new EventHandler(_mfd4Form_DataChanged);
                _mfd4Form.Disposed += new EventHandler(_mfd4Form_Disposed);
            }
        }
        /// <summary>
        /// Sets up the MFD #3 image output window
        /// </summary>
        private void SetupMfd3Form()
        {
            if (Properties.Settings.Default.EnableMfd3Output)
            {
                Point location;
                Size size = new Size();
                _mfd3Form = new InstrumentForm();
                _mfd3Form.Text = "MFD #3";
                _mfd3Form.ShowInTaskbar = false;
                _mfd3Form.ShowIcon = false;
                if (Properties.Settings.Default.MFD3_StretchToFit)
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
                _mfd3Form.AlwaysOnTop = Properties.Settings.Default.MFD3_AlwaysOnTop;
                _mfd3Form.Monochrome = Properties.Settings.Default.MFD3_Monochrome;
                _mfd3Form.Rotation = Properties.Settings.Default.MFD3_RotateFlipType;
                _mfd3Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_mfd3Form, _applicationForm, _mfd3OutputScreen, location, size, true, true);
                using (Graphics graphics = _mfd3Form.CreateGraphics())
                {
                    graphics.DrawImage(_mfd3BlankImage, _mfd3Form.ClientRectangle);
                }
                _mfd3Form.DataChanged += new EventHandler(_mfd3Form_DataChanged);
                _mfd3Form.Disposed += new EventHandler(_mfd3Form_Disposed);
            }
        }
        /// <summary>
        /// Sets up the Left MFD image output window
        /// </summary>
        private void SetupLeftMfdForm()
        {
            if (Properties.Settings.Default.EnableLeftMFDOutput)
            {
                Point location;
                Size size = new Size();
                _leftMfdForm = new InstrumentForm();
                _leftMfdForm.Text = "Left MFD";
                _leftMfdForm.ShowInTaskbar = false;
                _leftMfdForm.ShowIcon = false;
                if (Properties.Settings.Default.LMFD_StretchToFit)
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
                _leftMfdForm.AlwaysOnTop = Properties.Settings.Default.LMFD_AlwaysOnTop;
                _leftMfdForm.Monochrome = Properties.Settings.Default.LMFD_Monochrome;
                _leftMfdForm.Rotation = Properties.Settings.Default.LMFD_RotateFlipType;
                _leftMfdForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_leftMfdForm, _applicationForm, _leftMfdOutputScreen, location, size, true, true);
                using (Graphics graphics = _leftMfdForm.CreateGraphics())
                {
                    graphics.DrawImage(_leftMfdBlankImage, _leftMfdForm.ClientRectangle);
                }
                _leftMfdForm.DataChanged += new EventHandler(_leftMfdForm_DataChanged);
                _leftMfdForm.Disposed += new EventHandler(_leftMfdForm_Disposed);
            }
        }
        /// <summary>
        /// Sets up the Right MFD image output window
        /// </summary>
        private void SetupRightMfdForm()
        {
            if (Properties.Settings.Default.EnableRightMFDOutput)
            {
                Point location;
                Size size = new Size();
                _rightMfdForm = new InstrumentForm();
                _rightMfdForm.Text = "Right MFD";
                _rightMfdForm.ShowInTaskbar = false;
                _rightMfdForm.ShowIcon = false;
                if (Properties.Settings.Default.RMFD_StretchToFit)
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
                _rightMfdForm.AlwaysOnTop = Properties.Settings.Default.RMFD_AlwaysOnTop;
                _rightMfdForm.Monochrome = Properties.Settings.Default.RMFD_Monochrome;
                _rightMfdForm.Rotation = Properties.Settings.Default.RMFD_RotateFlipType;
                _rightMfdForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rightMfdForm, _applicationForm, _rightMfdOutputScreen, location, size, true, true);
                using (Graphics graphics = _rightMfdForm.CreateGraphics())
                {
                    graphics.DrawImage(_rightMfdBlankImage, _rightMfdForm.ClientRectangle);
                }
                _rightMfdForm.DataChanged += new EventHandler(_rightMfdForm_DataChanged);
                _rightMfdForm.Disposed += new EventHandler(_rightMfdForm_Disposed);

            }

        }
        /// <summary>
        /// Sets up the HUD image output window
        /// </summary>
        private void SetupHudForm()
        {
            if (Properties.Settings.Default.EnableHudOutput)
            {
                Point location;
                Size size = new Size();
                _hudForm = new InstrumentForm();
                _hudForm.Text = "HUD";
                _hudForm.ShowInTaskbar = false;
                _hudForm.ShowIcon = false;
                if (Properties.Settings.Default.HUD_StretchToFit)
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
                _hudForm.AlwaysOnTop = Properties.Settings.Default.HudAlwaysOnTop;
                _hudForm.Monochrome = Properties.Settings.Default.HUD_Monochrome;
                _hudForm.Rotation = Properties.Settings.Default.HUD_RotateFlipType;
                _hudForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hudForm, _applicationForm, _hudOutputScreen, location, size, true, true);
                using (Graphics graphics = _hudForm.CreateGraphics())
                {
                    graphics.DrawImage(_hudBlankImage, _hudForm.ClientRectangle);
                }
                _hudForm.DataChanged += new EventHandler(_hudForm_DataChanged);
                _hudForm.Disposed += new EventHandler(_hudForm_Disposed);
            }

        }
        #endregion
        #region Instruments Forms Setup
        private void SetupVVIForm()
        {
            if (Properties.Settings.Default.EnableVVIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.VVI_OutputDisplay);
                _vviForm = new InstrumentForm();
                _vviForm.Text = "Vertical Velocity Indicator (VVI)";
                _vviForm.ShowInTaskbar = false;
                _vviForm.ShowIcon = false;
                if (Properties.Settings.Default.VVI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _vviForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.VVI_OutULX, Properties.Settings.Default.VVI_OutULY);
                    size = new Size(Properties.Settings.Default.VVI_OutLRX - Properties.Settings.Default.VVI_OutULX, Properties.Settings.Default.VVI_OutLRY - Properties.Settings.Default.VVI_OutULY);
                    _vviForm.StretchToFill = false;
                }
                _vviForm.AlwaysOnTop = Properties.Settings.Default.VVI_AlwaysOnTop;
                _vviForm.Monochrome = Properties.Settings.Default.VVI_Monochrome;
                _vviForm.Rotation = Properties.Settings.Default.VVI_RotateFlipType;
                _vviForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_vviForm, _applicationForm, screen, location, size, true, true);
                _vviForm.DataChanged += new EventHandler(_vviForm_DataChanged);
                _vviForm.Disposed += new EventHandler(_vviForm_Disposed);
                _outputForms.Add(_vviRenderer, _vviForm);
            }

        }
        private void SetupRPM1Form()
        {
            if (Properties.Settings.Default.EnableRPM1Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.RPM1_OutputDisplay);
                _rpm1Form = new InstrumentForm();
                _rpm1Form.Text = "Engine 1 - RPM";
                _rpm1Form.ShowInTaskbar = false;
                _rpm1Form.ShowIcon = false;
                if (Properties.Settings.Default.RPM1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rpm1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.RPM1_OutULX, Properties.Settings.Default.RPM1_OutULY);
                    size = new Size(Properties.Settings.Default.RPM1_OutLRX - Properties.Settings.Default.RPM1_OutULX, Properties.Settings.Default.RPM1_OutLRY - Properties.Settings.Default.RPM1_OutULY);
                    _rpm1Form.StretchToFill = false;
                }
                _rpm1Form.AlwaysOnTop = Properties.Settings.Default.RPM1_AlwaysOnTop;
                _rpm1Form.Monochrome = Properties.Settings.Default.RPM1_Monochrome;
                _rpm1Form.Rotation = Properties.Settings.Default.RPM1_RotateFlipType;
                _rpm1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rpm1Form, _applicationForm, screen, location, size, true, true);
                _rpm1Form.DataChanged += new EventHandler(_rpm1Form_DataChanged);
                _rpm1Form.Disposed += new EventHandler(_rpm1Form_Disposed);
                _outputForms.Add(_rpm1Renderer, _rpm1Form);
            }

        }
        private void SetupRPM2Form()
        {
            if (Properties.Settings.Default.EnableRPM2Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.RPM2_OutputDisplay);
                _rpm2Form = new InstrumentForm();
                _rpm2Form.Text = "Engine 2 - RPM";
                _rpm2Form.ShowInTaskbar = false;
                _rpm2Form.ShowIcon = false;
                if (Properties.Settings.Default.RPM2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rpm2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.RPM2_OutULX, Properties.Settings.Default.RPM2_OutULY);
                    size = new Size(Properties.Settings.Default.RPM2_OutLRX - Properties.Settings.Default.RPM2_OutULX, Properties.Settings.Default.RPM2_OutLRY - Properties.Settings.Default.RPM2_OutULY);
                    _rpm2Form.StretchToFill = false;
                }
                _rpm2Form.AlwaysOnTop = Properties.Settings.Default.RPM2_AlwaysOnTop;
                _rpm2Form.Monochrome = Properties.Settings.Default.RPM2_Monochrome;
                _rpm2Form.Rotation = Properties.Settings.Default.RPM2_RotateFlipType;
                _rpm2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rpm2Form, _applicationForm, screen, location, size, true, true);
                _rpm2Form.DataChanged += new EventHandler(_rpm2Form_DataChanged);
                _rpm2Form.Disposed += new EventHandler(_rpm2Form_Disposed);
                _outputForms.Add(_rpm2Renderer, _rpm2Form);
            }

        }
        private void SetupSpeedbrakeForm()
        {
            if (Properties.Settings.Default.EnableSpeedbrakeOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.Speedbrake_OutputDisplay);
                _speedbrakeForm = new InstrumentForm();
                _speedbrakeForm.Text = "Speedbrake";
                _speedbrakeForm.ShowInTaskbar = false;
                _speedbrakeForm.ShowIcon = false;
                if (Properties.Settings.Default.Speedbrake_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _speedbrakeForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.Speedbrake_OutULX, Properties.Settings.Default.Speedbrake_OutULY);
                    size = new Size(Properties.Settings.Default.Speedbrake_OutLRX - Properties.Settings.Default.Speedbrake_OutULX, Properties.Settings.Default.Speedbrake_OutLRY - Properties.Settings.Default.Speedbrake_OutULY);
                    _speedbrakeForm.StretchToFill = false;
                }
                _speedbrakeForm.AlwaysOnTop = Properties.Settings.Default.Speedbrake_AlwaysOnTop;
                _speedbrakeForm.Monochrome = Properties.Settings.Default.Speedbrake_Monochrome;
                _speedbrakeForm.Rotation = Properties.Settings.Default.Speedbrake_RotateFlipType;
                _speedbrakeForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_speedbrakeForm, _applicationForm, screen, location, size, true, true);
                _speedbrakeForm.DataChanged += new EventHandler(_speedbrakeForm_DataChanged);
                _speedbrakeForm.Disposed += new EventHandler(_speedbrakeForm_Disposed);
                _outputForms.Add(_speedbrakeRenderer, _speedbrakeForm);
            }

        }
        private void SetupRWRForm()
        {
            if (Properties.Settings.Default.EnableRWROutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.RWR_OutputDisplay);
                _rwrForm = new InstrumentForm();
                _rwrForm.Text = "RWR/Azimuth Indicator";
                _rwrForm.ShowInTaskbar = false;
                _rwrForm.ShowIcon = false;
                if (Properties.Settings.Default.RWR_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rwrForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.RWR_OutULX, Properties.Settings.Default.RWR_OutULY);
                    size = new Size(Properties.Settings.Default.RWR_OutLRX - Properties.Settings.Default.RWR_OutULX, Properties.Settings.Default.RWR_OutLRY - Properties.Settings.Default.RWR_OutULY);
                    _rwrForm.StretchToFill = false;
                }
                _rwrForm.AlwaysOnTop = Properties.Settings.Default.RWR_AlwaysOnTop;
                _rwrForm.Monochrome = Properties.Settings.Default.RWR_Monochrome;
                _rwrForm.Rotation = Properties.Settings.Default.RWR_RotateFlipType;
                _rwrForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rwrForm, _applicationForm, screen, location, size, true, true);
                _rwrForm.DataChanged += new EventHandler(_rwrForm_DataChanged);
                _rwrForm.Disposed += new EventHandler(_rwrForm_Disposed);
                _outputForms.Add(_rwrRenderer, _rwrForm);
            }

        }
        private void SetupOIL2Form()
        {
            if (Properties.Settings.Default.EnableOIL2Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.OIL2_OutputDisplay);
                _oilGauge2Form = new InstrumentForm();
                _oilGauge2Form.Text = "Engine 2 - Oil Pressure Indicator";
                _oilGauge2Form.ShowInTaskbar = false;
                _oilGauge2Form.ShowIcon = false;
                if (Properties.Settings.Default.OIL2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _oilGauge2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.OIL2_OutULX, Properties.Settings.Default.OIL2_OutULY);
                    size = new Size(Properties.Settings.Default.OIL2_OutLRX - Properties.Settings.Default.OIL2_OutULX, Properties.Settings.Default.OIL2_OutLRY - Properties.Settings.Default.OIL2_OutULY);
                    _oilGauge2Form.StretchToFill = false;
                }
                _oilGauge2Form.AlwaysOnTop = Properties.Settings.Default.OIL2_AlwaysOnTop;
                _oilGauge2Form.Monochrome = Properties.Settings.Default.OIL2_Monochrome;
                _oilGauge2Form.Rotation = Properties.Settings.Default.OIL2_RotateFlipType;
                _oilGauge2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_oilGauge2Form, _applicationForm, screen, location, size, true, true);
                _oilGauge2Form.DataChanged += new EventHandler(_oilGauge2Form_DataChanged);
                _oilGauge2Form.Disposed += new EventHandler(_oilGauge2Form_Disposed);
                _outputForms.Add(_oilGauge2Renderer, _oilGauge2Form);
            }

        }
        private void SetupOIL1Form()
        {
            if (Properties.Settings.Default.EnableOIL1Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.OIL1_OutputDisplay);
                _oilGauge1Form = new InstrumentForm();
                _oilGauge1Form.Text = "Engine 1 - Oil Pressure Indicator";
                _oilGauge1Form.ShowInTaskbar = false;
                _oilGauge1Form.ShowIcon = false;
                if (Properties.Settings.Default.OIL1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _oilGauge1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.OIL1_OutULX, Properties.Settings.Default.OIL1_OutULY);
                    size = new Size(Properties.Settings.Default.OIL1_OutLRX - Properties.Settings.Default.OIL1_OutULX, Properties.Settings.Default.OIL1_OutLRY - Properties.Settings.Default.OIL1_OutULY);
                    _oilGauge1Form.StretchToFill = false;
                }
                _oilGauge1Form.AlwaysOnTop = Properties.Settings.Default.OIL1_AlwaysOnTop;
                _oilGauge1Form.Monochrome = Properties.Settings.Default.OIL1_Monochrome;
                _oilGauge1Form.Rotation = Properties.Settings.Default.OIL1_RotateFlipType;
                _oilGauge1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_oilGauge1Form, _applicationForm, screen, location, size, true, true);
                _oilGauge1Form.DataChanged += new EventHandler(_oilGauge1Form_DataChanged);
                _oilGauge1Form.Disposed += new EventHandler(_oilGauge1Form_Disposed);
                _outputForms.Add(_oilGauge1Renderer, _oilGauge1Form);
            }

        }
        private void SetupNOZ2Form()
        {
            if (Properties.Settings.Default.EnableNOZ2Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.NOZ2_OutputDisplay);
                _nozPos2Form = new InstrumentForm();
                _nozPos2Form.Text = "Engine 2 - Nozzle Position Indicator";
                _nozPos2Form.ShowInTaskbar = false;
                _nozPos2Form.ShowIcon = false;
                if (Properties.Settings.Default.NOZ2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nozPos2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.NOZ2_OutULX, Properties.Settings.Default.NOZ2_OutULY);
                    size = new Size(Properties.Settings.Default.NOZ2_OutLRX - Properties.Settings.Default.NOZ2_OutULX, Properties.Settings.Default.NOZ2_OutLRY - Properties.Settings.Default.NOZ2_OutULY);
                    _nozPos2Form.StretchToFill = false;
                }
                _nozPos2Form.AlwaysOnTop = Properties.Settings.Default.NOZ2_AlwaysOnTop;
                _nozPos2Form.Monochrome = Properties.Settings.Default.NOZ2_Monochrome;
                _nozPos2Form.Rotation = Properties.Settings.Default.NOZ2_RotateFlipType;
                _nozPos2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nozPos2Form, _applicationForm, screen, location, size, true, true);
                _nozPos2Form.DataChanged += new EventHandler(_nozPos2Form_DataChanged);
                _nozPos2Form.Disposed += new EventHandler(_nozPos2Form_Disposed);
                _outputForms.Add(_nozPos2Renderer, _nozPos2Form);
            }

        }
        private void SetupNOZ1Form()
        {
            if (Properties.Settings.Default.EnableNOZ1Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.NOZ1_OutputDisplay);
                _nozPos1Form = new InstrumentForm();
                _nozPos1Form.Text = "Engine 1 - Nozzle Position Indicator";
                _nozPos1Form.ShowInTaskbar = false;
                _nozPos1Form.ShowIcon = false;
                if (Properties.Settings.Default.NOZ1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nozPos1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.NOZ1_OutULX, Properties.Settings.Default.NOZ1_OutULY);
                    size = new Size(Properties.Settings.Default.NOZ1_OutLRX - Properties.Settings.Default.NOZ1_OutULX, Properties.Settings.Default.NOZ1_OutLRY - Properties.Settings.Default.NOZ1_OutULY);
                    _nozPos1Form.StretchToFill = false;
                }
                _nozPos1Form.AlwaysOnTop = Properties.Settings.Default.NOZ1_AlwaysOnTop;
                _nozPos1Form.Monochrome = Properties.Settings.Default.NOZ1_Monochrome;
                _nozPos1Form.Rotation = Properties.Settings.Default.NOZ1_RotateFlipType;
                _nozPos1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nozPos1Form, _applicationForm, screen, location, size, true, true);
                _nozPos1Form.DataChanged += new EventHandler(_nozPos1Form_DataChanged);
                _nozPos1Form.Disposed += new EventHandler(_nozPos1Form_Disposed);
                _outputForms.Add(_nozPos1Renderer, _nozPos1Form);
            }

        }
        private void SetupNWSIndexerForm()
        {
            if (Properties.Settings.Default.EnableNWSIndexerOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.NWSIndexer_OutputDisplay);
                _nwsIndexerForm = new InstrumentForm();
                _nwsIndexerForm.Text = "NWS Indexer";
                _nwsIndexerForm.ShowInTaskbar = false;
                _nwsIndexerForm.ShowIcon = false;
                if (Properties.Settings.Default.NWSIndexer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _nwsIndexerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.NWSIndexer_OutULX, Properties.Settings.Default.NWSIndexer_OutULY);
                    size = new Size(Properties.Settings.Default.NWSIndexer_OutLRX - Properties.Settings.Default.NWSIndexer_OutULX, Properties.Settings.Default.NWSIndexer_OutLRY - Properties.Settings.Default.NWSIndexer_OutULY);
                    _nwsIndexerForm.StretchToFill = false;
                }
                _nwsIndexerForm.AlwaysOnTop = Properties.Settings.Default.NWSIndexer_AlwaysOnTop;
                _nwsIndexerForm.Monochrome = Properties.Settings.Default.NWSIndexer_Monochrome;
                _nwsIndexerForm.Rotation = Properties.Settings.Default.NWSIndexer_RotateFlipType;
                _nwsIndexerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_nwsIndexerForm, _applicationForm, screen, location, size, true, true);
                _nwsIndexerForm.DataChanged += new EventHandler(_nwsIndexerForm_DataChanged);
                _nwsIndexerForm.Disposed += new EventHandler(_nwsIndexerForm_Disposed);
                _outputForms.Add(_nwsIndexerRenderer, _nwsIndexerForm);
            }

        }
        private void SetupGearLightsForm()
        {
            if (Properties.Settings.Default.EnableGearLightsOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.GearLights_OutputDisplay);
                _landingGearLightsForm = new InstrumentForm();
                _landingGearLightsForm.Text = "Landing Gear Lights";
                _landingGearLightsForm.ShowInTaskbar = false;
                _landingGearLightsForm.ShowIcon = false;
                if (Properties.Settings.Default.GearLights_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _landingGearLightsForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.GearLights_OutULX, Properties.Settings.Default.GearLights_OutULY);
                    size = new Size(Properties.Settings.Default.GearLights_OutLRX - Properties.Settings.Default.GearLights_OutULX, Properties.Settings.Default.GearLights_OutLRY - Properties.Settings.Default.GearLights_OutULY);
                    _landingGearLightsForm.StretchToFill = false;
                }
                _landingGearLightsForm.AlwaysOnTop = Properties.Settings.Default.GearLights_AlwaysOnTop;
                _landingGearLightsForm.Monochrome = Properties.Settings.Default.GearLights_Monochrome;
                _landingGearLightsForm.Rotation = Properties.Settings.Default.GearLights_RotateFlipType;
                _landingGearLightsForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_landingGearLightsForm, _applicationForm, screen, location, size, true, true);
                _landingGearLightsForm.DataChanged += new EventHandler(_landingGearLightsForm_DataChanged);
                _landingGearLightsForm.Disposed += new EventHandler(_landingGearLightsForm_Disposed);
                _outputForms.Add(_landingGearLightsRenderer, _landingGearLightsForm);
            }

        }
        private void SetupHSIForm()
        {
            if (Properties.Settings.Default.EnableHSIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.HSI_OutputDisplay);
                _hsiForm = new InstrumentForm();
                _hsiForm.Text = "HSI";
                _hsiForm.ShowInTaskbar = false;
                _hsiForm.ShowIcon = false;
                if (Properties.Settings.Default.HSI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hsiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.HSI_OutULX, Properties.Settings.Default.HSI_OutULY);
                    size = new Size(Properties.Settings.Default.HSI_OutLRX - Properties.Settings.Default.HSI_OutULX, Properties.Settings.Default.HSI_OutLRY - Properties.Settings.Default.HSI_OutULY);
                    _hsiForm.StretchToFill = false;
                }
                _hsiForm.AlwaysOnTop = Properties.Settings.Default.HSI_AlwaysOnTop;
                _hsiForm.Monochrome = Properties.Settings.Default.HSI_Monochrome;
                _hsiForm.Rotation = Properties.Settings.Default.HSI_RotateFlipType;
                _hsiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hsiForm, _applicationForm, screen, location, size, true, true);
                _hsiForm.DataChanged += new EventHandler(_hsiForm_DataChanged);
                _hsiForm.Disposed += new EventHandler(_hsiForm_Disposed);
                _outputForms.Add(_hsiRenderer, _hsiForm);
            }

        }
        private void SetupEHSIForm()
        {
            if (Properties.Settings.Default.EnableEHSIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.EHSI_OutputDisplay);
                _ehsiForm = new InstrumentForm();
                _ehsiForm.Text = "EHSI";
                _ehsiForm.ShowInTaskbar = false;
                _ehsiForm.ShowIcon = false;
                if (Properties.Settings.Default.EHSI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ehsiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.EHSI_OutULX, Properties.Settings.Default.EHSI_OutULY);
                    size = new Size(Properties.Settings.Default.EHSI_OutLRX - Properties.Settings.Default.EHSI_OutULX, Properties.Settings.Default.EHSI_OutLRY - Properties.Settings.Default.EHSI_OutULY);
                    _ehsiForm.StretchToFill = false;
                }
                _ehsiForm.AlwaysOnTop = Properties.Settings.Default.EHSI_AlwaysOnTop;
                _ehsiForm.Monochrome = Properties.Settings.Default.EHSI_Monochrome;
                _ehsiForm.Rotation = Properties.Settings.Default.EHSI_RotateFlipType;
                _ehsiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ehsiForm, _applicationForm, screen, location, size, true, true);
                _ehsiForm.DataChanged += new EventHandler(_ehsiForm_DataChanged);
                _ehsiForm.Disposed += new EventHandler(_ehsiForm_Disposed);
                _outputForms.Add(_ehsiRenderer, _ehsiForm);
            }

        }
        private void SetupFuelQuantityForm()
        {
            if (Properties.Settings.Default.EnableFuelQuantityOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.FuelQuantity_OutputDisplay);
                _fuelQuantityForm = new InstrumentForm();
                _fuelQuantityForm.Text = "Fuel Quantity";
                _fuelQuantityForm.ShowInTaskbar = false;
                _fuelQuantityForm.ShowIcon = false;
                if (Properties.Settings.Default.FuelQuantity_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _fuelQuantityForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.FuelQuantity_OutULX, Properties.Settings.Default.FuelQuantity_OutULY);
                    size = new Size(Properties.Settings.Default.FuelQuantity_OutLRX - Properties.Settings.Default.FuelQuantity_OutULX, Properties.Settings.Default.FuelQuantity_OutLRY - Properties.Settings.Default.FuelQuantity_OutULY);
                    _fuelQuantityForm.StretchToFill = false;
                }
                _fuelQuantityForm.AlwaysOnTop = Properties.Settings.Default.FuelQuantity_AlwaysOnTop;
                _fuelQuantityForm.Monochrome = Properties.Settings.Default.FuelQuantity_Monochrome;
                _fuelQuantityForm.Rotation = Properties.Settings.Default.FuelQuantity_RotateFlipType;
                _fuelQuantityForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_fuelQuantityForm, _applicationForm, screen, location, size, true, true);
                _fuelQuantityForm.DataChanged += new EventHandler(_fuelQuantityForm_DataChanged);
                _fuelQuantityForm.Disposed += new EventHandler(_fuelQuantityForm_Disposed);
                _outputForms.Add(_fuelQuantityRenderer, _fuelQuantityForm);
            }

        }
        private void SetupFuelFlowForm()
        {
            if (Properties.Settings.Default.EnableFuelFlowOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.FuelFlow_OutputDisplay);
                _fuelFlowForm = new InstrumentForm();
                _fuelFlowForm.Text = "Fuel Flow Indicator";
                _fuelFlowForm.ShowInTaskbar = false;
                _fuelFlowForm.ShowIcon = false;
                if (Properties.Settings.Default.FuelFlow_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _fuelFlowForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.FuelFlow_OutULX, Properties.Settings.Default.FuelFlow_OutULY);
                    size = new Size(Properties.Settings.Default.FuelFlow_OutLRX - Properties.Settings.Default.FuelFlow_OutULX, Properties.Settings.Default.FuelFlow_OutLRY - Properties.Settings.Default.FuelFlow_OutULY);
                    _fuelFlowForm.StretchToFill = false;
                }
                _fuelFlowForm.AlwaysOnTop = Properties.Settings.Default.FuelFlow_AlwaysOnTop;
                _fuelFlowForm.Monochrome = Properties.Settings.Default.FuelFlow_Monochrome;
                _fuelFlowForm.Rotation = Properties.Settings.Default.FuelFlow_RotateFlipType;
                _fuelFlowForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_fuelFlowForm, _applicationForm, screen, location, size, true, true);
                _fuelFlowForm.DataChanged += new EventHandler(_fuelFlowForm_DataChanged);
                _fuelFlowForm.Disposed += new EventHandler(_fuelFlowForm_Disposed);
                _outputForms.Add(_fuelFlowRenderer, _fuelFlowForm);
            }

        }
        private void SetupISISForm()
        {
            if (Properties.Settings.Default.EnableISISOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.ISIS_OutputDisplay);
                _isisForm = new InstrumentForm();
                _isisForm.Text = "ISIS";
                _isisForm.ShowInTaskbar = false;
                _isisForm.ShowIcon = false;
                if (Properties.Settings.Default.ISIS_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _isisForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.ISIS_OutULX, Properties.Settings.Default.ISIS_OutULY);
                    size = new Size(Properties.Settings.Default.ISIS_OutLRX - Properties.Settings.Default.ISIS_OutULX, Properties.Settings.Default.ISIS_OutLRY - Properties.Settings.Default.ISIS_OutULY);
                    _isisForm.StretchToFill = false;
                }
                _isisForm.AlwaysOnTop = Properties.Settings.Default.ISIS_AlwaysOnTop;
                _isisForm.Monochrome = Properties.Settings.Default.ISIS_Monochrome;
                _isisForm.Rotation = Properties.Settings.Default.ISIS_RotateFlipType;
                _isisForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_isisForm, _applicationForm, screen, location, size, true, true);
                _isisForm.DataChanged += new EventHandler(_isisForm_DataChanged);
                _isisForm.Disposed += new EventHandler(_isisForm_Disposed);
                _outputForms.Add(_isisRenderer, _isisForm);
            }

        }
        private void SetupAccelerometerForm()
        {
            if (Properties.Settings.Default.EnableAccelerometerOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.Accelerometer_OutputDisplay);
                _accelerometerForm = new InstrumentForm();
                _accelerometerForm.Text = "Accelerometer (G-Meter)";
                _accelerometerForm.ShowInTaskbar = false;
                _accelerometerForm.ShowIcon = false;
                if (Properties.Settings.Default.Accelerometer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _accelerometerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.Accelerometer_OutULX, Properties.Settings.Default.Accelerometer_OutULY);
                    size = new Size(Properties.Settings.Default.Accelerometer_OutLRX - Properties.Settings.Default.Accelerometer_OutULX, Properties.Settings.Default.Accelerometer_OutLRY - Properties.Settings.Default.Accelerometer_OutULY);
                    _accelerometerForm.StretchToFill = false;
                }
                _accelerometerForm.AlwaysOnTop = Properties.Settings.Default.Accelerometer_AlwaysOnTop;
                _accelerometerForm.Monochrome = Properties.Settings.Default.Accelerometer_Monochrome;
                _accelerometerForm.Rotation = Properties.Settings.Default.Accelerometer_RotateFlipType;
                _accelerometerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_accelerometerForm, _applicationForm, screen, location, size, true, true);
                _accelerometerForm.DataChanged += new EventHandler(_accelerometerForm_DataChanged);
                _accelerometerForm.Disposed += new EventHandler(_accelerometerForm_Disposed);
                _outputForms.Add(_accelerometerRenderer, _accelerometerForm);
            }

        }
        private void SetupFTIT2Form()
        {
            if (Properties.Settings.Default.EnableFTIT2Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.FTIT2_OutputDisplay);
                _ftit2Form = new InstrumentForm();
                _ftit2Form.Text = "Engine 2 - FTIT";
                _ftit2Form.ShowInTaskbar = false;
                _ftit2Form.ShowIcon = false;
                if (Properties.Settings.Default.FTIT2_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ftit2Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.FTIT2_OutULX, Properties.Settings.Default.FTIT2_OutULY);
                    size = new Size(Properties.Settings.Default.FTIT2_OutLRX - Properties.Settings.Default.FTIT2_OutULX, Properties.Settings.Default.FTIT2_OutLRY - Properties.Settings.Default.FTIT2_OutULY);
                    _ftit2Form.StretchToFill = false;
                }
                _ftit2Form.AlwaysOnTop = Properties.Settings.Default.FTIT2_AlwaysOnTop;
                _ftit2Form.Monochrome = Properties.Settings.Default.FTIT2_Monochrome;
                _ftit2Form.Rotation = Properties.Settings.Default.FTIT2_RotateFlipType;
                _ftit2Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ftit2Form, _applicationForm, screen, location, size, true, true);
                _ftit2Form.DataChanged += new EventHandler(_ftit2Form_DataChanged);
                _ftit2Form.Disposed += new EventHandler(_ftit2Form_Disposed);
                _outputForms.Add(_ftit2Renderer, _ftit2Form);
            }

        }
        private void SetupFTIT1Form()
        {
            if (Properties.Settings.Default.EnableFTIT1Output)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.FTIT1_OutputDisplay);
                _ftit1Form = new InstrumentForm();
                _ftit1Form.Text = "Engine 1 - FTIT";
                _ftit1Form.ShowInTaskbar = false;
                _ftit1Form.ShowIcon = false;
                if (Properties.Settings.Default.FTIT1_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _ftit1Form.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.FTIT1_OutULX, Properties.Settings.Default.FTIT1_OutULY);
                    size = new Size(Properties.Settings.Default.FTIT1_OutLRX - Properties.Settings.Default.FTIT1_OutULX, Properties.Settings.Default.FTIT1_OutLRY - Properties.Settings.Default.FTIT1_OutULY);
                    _ftit1Form.StretchToFill = false;
                }
                _ftit1Form.AlwaysOnTop = Properties.Settings.Default.FTIT1_AlwaysOnTop;
                _ftit1Form.Monochrome = Properties.Settings.Default.FTIT1_Monochrome;
                _ftit1Form.Rotation = Properties.Settings.Default.FTIT1_RotateFlipType;
                _ftit1Form.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_ftit1Form, _applicationForm, screen, location, size, true, true);
                _ftit1Form.DataChanged += new EventHandler(_ftit1Form_DataChanged);
                _ftit1Form.Disposed += new EventHandler(_ftit1Form_Disposed);
                _outputForms.Add(_ftit1Renderer, _ftit1Form);
            }

        }
        private void SetupEPUFuelForm()
        {
            if (Properties.Settings.Default.EnableEPUFuelOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.EPUFuel_OutputDisplay);
                _epuFuelForm = new InstrumentForm();
                _epuFuelForm.Text = "EPU Fuel";
                _epuFuelForm.ShowInTaskbar = false;
                _epuFuelForm.ShowIcon = false;
                if (Properties.Settings.Default.EPUFuel_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _epuFuelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.EPUFuel_OutULX, Properties.Settings.Default.EPUFuel_OutULY);
                    size = new Size(Properties.Settings.Default.EPUFuel_OutLRX - Properties.Settings.Default.EPUFuel_OutULX, Properties.Settings.Default.EPUFuel_OutLRY - Properties.Settings.Default.EPUFuel_OutULY);
                    _epuFuelForm.StretchToFill = false;
                }
                _epuFuelForm.AlwaysOnTop = Properties.Settings.Default.EPUFuel_AlwaysOnTop;
                _epuFuelForm.Monochrome = Properties.Settings.Default.EPUFuel_Monochrome;
                _epuFuelForm.Rotation = Properties.Settings.Default.EPUFuel_RotateFlipType;
                _epuFuelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_epuFuelForm, _applicationForm, screen, location, size, true, true);
                _epuFuelForm.DataChanged += new EventHandler(_epuFuelForm_DataChanged);
                _epuFuelForm.Disposed += new EventHandler(_epuFuelForm_Disposed);
                _outputForms.Add(_epuFuelRenderer, _epuFuelForm);
            }

        }
        private void SetupPFLForm()
        {
            if (Properties.Settings.Default.EnablePFLOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.PFL_OutputDisplay);
                _pflForm = new InstrumentForm();
                _pflForm.Text = "Pilot Fault List (PFL)";
                _pflForm.ShowInTaskbar = false;
                _pflForm.ShowIcon = false;
                if (Properties.Settings.Default.PFL_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _pflForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.PFL_OutULX, Properties.Settings.Default.PFL_OutULY);
                    size = new Size(Properties.Settings.Default.PFL_OutLRX - Properties.Settings.Default.PFL_OutULX, Properties.Settings.Default.PFL_OutLRY - Properties.Settings.Default.PFL_OutULY);
                    _pflForm.StretchToFill = false;
                }
                _pflForm.AlwaysOnTop = Properties.Settings.Default.PFL_AlwaysOnTop;
                _pflForm.Monochrome = Properties.Settings.Default.PFL_Monochrome;
                _pflForm.Rotation = Properties.Settings.Default.PFL_RotateFlipType;
                _pflForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_pflForm, _applicationForm, screen, location, size, true, true);
                _pflForm.DataChanged += new EventHandler(_pflForm_DataChanged);
                _pflForm.Disposed += new EventHandler(_pflForm_Disposed);
                _outputForms.Add(_pflRenderer, _pflForm);
            }

        }
        private void SetupDEDForm()
        {
            if (Properties.Settings.Default.EnableDEDOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.DED_OutputDisplay);
                _dedForm = new InstrumentForm();
                _dedForm.Text = "Data Entry Display (DED)";
                _dedForm.ShowInTaskbar = false;
                _dedForm.ShowIcon = false;
                if (Properties.Settings.Default.DED_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _dedForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.DED_OutULX, Properties.Settings.Default.DED_OutULY);
                    size = new Size(Properties.Settings.Default.DED_OutLRX - Properties.Settings.Default.DED_OutULX, Properties.Settings.Default.DED_OutLRY - Properties.Settings.Default.DED_OutULY);
                    _dedForm.StretchToFill = false;
                }
                _dedForm.AlwaysOnTop = Properties.Settings.Default.DED_AlwaysOnTop;
                _dedForm.Monochrome = Properties.Settings.Default.DED_Monochrome;
                _dedForm.Rotation = Properties.Settings.Default.DED_RotateFlipType;
                _dedForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_dedForm, _applicationForm, screen, location, size, true, true);
                _dedForm.DataChanged += new EventHandler(_dedForm_DataChanged);
                _dedForm.Disposed += new EventHandler(_dedForm_Disposed);
                _outputForms.Add(_dedRenderer, _dedForm);
            }

        }
        private void SetupCompassForm()
        {
            if (Properties.Settings.Default.EnableCompassOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.Compass_OutputDisplay);
                _compassForm = new InstrumentForm();
                _compassForm.Text = "Compass";
                _compassForm.ShowInTaskbar = false;
                _compassForm.ShowIcon = false;
                if (Properties.Settings.Default.Compass_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _compassForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.Compass_OutULX, Properties.Settings.Default.Compass_OutULY);
                    size = new Size(Properties.Settings.Default.Compass_OutLRX - Properties.Settings.Default.Compass_OutULX, Properties.Settings.Default.Compass_OutLRY - Properties.Settings.Default.Compass_OutULY);
                    _compassForm.StretchToFill = false;
                }
                _compassForm.AlwaysOnTop = Properties.Settings.Default.Compass_AlwaysOnTop;
                _compassForm.Monochrome = Properties.Settings.Default.Compass_Monochrome;
                _compassForm.Rotation = Properties.Settings.Default.Compass_RotateFlipType;
                _compassForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_compassForm, _applicationForm, screen, location, size, true, true);
                _compassForm.DataChanged += new EventHandler(_compassForm_DataChanged);
                _compassForm.Disposed += new EventHandler(_compassForm_Disposed);
                _outputForms.Add(_compassRenderer, _compassForm);
            }

        }
        private void SetupCMDSPanelForm()
        {
            if (Properties.Settings.Default.EnableCMDSOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.CMDS_OutputDisplay);
                _cmdsPanelForm = new InstrumentForm();
                _cmdsPanelForm.Text = "CMDS Panel";
                _cmdsPanelForm.ShowInTaskbar = false;
                _cmdsPanelForm.ShowIcon = false;
                if (Properties.Settings.Default.CMDS_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cmdsPanelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.CMDS_OutULX, Properties.Settings.Default.CMDS_OutULY);
                    size = new Size(Properties.Settings.Default.CMDS_OutLRX - Properties.Settings.Default.CMDS_OutULX, Properties.Settings.Default.CMDS_OutLRY - Properties.Settings.Default.CMDS_OutULY);
                    _cmdsPanelForm.StretchToFill = false;
                }
                _cmdsPanelForm.AlwaysOnTop = Properties.Settings.Default.CMDS_AlwaysOnTop;
                _cmdsPanelForm.Monochrome = Properties.Settings.Default.CMDS_Monochrome;
                _cmdsPanelForm.Rotation = Properties.Settings.Default.CMDS_RotateFlipType;
                _cmdsPanelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cmdsPanelForm, _applicationForm, screen, location, size, true, true);
                _cmdsPanelForm.DataChanged += new EventHandler(_cmdsPanelForm_DataChanged);
                _cmdsPanelForm.Disposed += new EventHandler(_cmdsPanelForm_Disposed);
                _outputForms.Add(_cmdsPanelRenderer, _cmdsPanelForm);
            }

        }
        private void SetupCautionPanelForm()
        {
            if (Properties.Settings.Default.EnableCautionPanelOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.CautionPanel_OutputDisplay);
                _cautionPanelForm = new InstrumentForm();
                _cautionPanelForm.Text = "Caution Panel";
                _cautionPanelForm.ShowInTaskbar = false;
                _cautionPanelForm.ShowIcon = false;
                if (Properties.Settings.Default.CautionPanel_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cautionPanelForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.CautionPanel_OutULX, Properties.Settings.Default.CautionPanel_OutULY);
                    size = new Size(Properties.Settings.Default.CautionPanel_OutLRX - Properties.Settings.Default.CautionPanel_OutULX, Properties.Settings.Default.CautionPanel_OutLRY - Properties.Settings.Default.CautionPanel_OutULY);
                    _cautionPanelForm.StretchToFill = false;
                }
                _cautionPanelForm.AlwaysOnTop = Properties.Settings.Default.CautionPanel_AlwaysOnTop;
                _cautionPanelForm.Monochrome = Properties.Settings.Default.CautionPanel_Monochrome;
                _cautionPanelForm.Rotation = Properties.Settings.Default.CautionPanel_RotateFlipType;
                _cautionPanelForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cautionPanelForm, _applicationForm, screen, location, size, true, true);
                _cautionPanelForm.DataChanged += new EventHandler(_cautionPanelForm_DataChanged);
                _cautionPanelForm.Disposed += new EventHandler(_cautionPanelForm_Disposed);
                _outputForms.Add(_cautionPanelRenderer, _cautionPanelForm);
            }

        }
        private void SetupAOAIndicatorForm()
        {
            if (Properties.Settings.Default.EnableAOAIndicatorOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.AOAIndicator_OutputDisplay);
                _aoaIndicatorForm = new InstrumentForm();
                _aoaIndicatorForm.Text = "AOA Indicator";
                _aoaIndicatorForm.ShowInTaskbar = false;
                _aoaIndicatorForm.ShowIcon = false;
                if (Properties.Settings.Default.AOAIndicator_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _aoaIndicatorForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.AOAIndicator_OutULX, Properties.Settings.Default.AOAIndicator_OutULY);
                    size = new Size(Properties.Settings.Default.AOAIndicator_OutLRX - Properties.Settings.Default.AOAIndicator_OutULX, Properties.Settings.Default.AOAIndicator_OutLRY - Properties.Settings.Default.AOAIndicator_OutULY);
                    _aoaIndicatorForm.StretchToFill = false;
                }
                _aoaIndicatorForm.AlwaysOnTop = Properties.Settings.Default.AOAIndicator_AlwaysOnTop;
                _aoaIndicatorForm.Monochrome = Properties.Settings.Default.AOAIndicator_Monochrome;
                _aoaIndicatorForm.Rotation = Properties.Settings.Default.AOAIndicator_RotateFlipType;
                _aoaIndicatorForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_aoaIndicatorForm, _applicationForm, screen, location, size, true, true);
                _aoaIndicatorForm.DataChanged += new EventHandler(_aoaIndicatorForm_DataChanged);
                _aoaIndicatorForm.Disposed += new EventHandler(_aoaIndicatorForm_Disposed);
                _outputForms.Add(_aoaIndicatorRenderer, _aoaIndicatorForm);
            }

        }
        private void SetupAOAIndexerForm()
        {
            if (Properties.Settings.Default.EnableAOAIndexerOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.AOAIndexer_OutputDisplay);
                _aoaIndexerForm = new InstrumentForm();
                _aoaIndexerForm.Text = "AOA Indexer";
                _aoaIndexerForm.ShowInTaskbar = false;
                _aoaIndexerForm.ShowIcon = false;
                if (Properties.Settings.Default.AOAIndexer_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _aoaIndexerForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.AOAIndexer_OutULX, Properties.Settings.Default.AOAIndexer_OutULY);
                    size = new Size(Properties.Settings.Default.AOAIndexer_OutLRX - Properties.Settings.Default.AOAIndexer_OutULX, Properties.Settings.Default.AOAIndexer_OutLRY - Properties.Settings.Default.AOAIndexer_OutULY);
                    _aoaIndexerForm.StretchToFill = false;
                }
                _aoaIndexerForm.AlwaysOnTop = Properties.Settings.Default.AOAIndexer_AlwaysOnTop;
                _aoaIndexerForm.Monochrome = Properties.Settings.Default.AOAIndexer_Monochrome;
                _aoaIndexerForm.Rotation = Properties.Settings.Default.AOAIndexer_RotateFlipType;
                _aoaIndexerForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_aoaIndexerForm, _applicationForm, screen, location, size, true, true);
                _aoaIndexerForm.DataChanged += new EventHandler(_aoaIndexerForm_DataChanged);
                _aoaIndexerForm.Disposed += new EventHandler(_aoaIndexerForm_Disposed);
                _outputForms.Add(_aoaIndexerRenderer, _aoaIndexerForm);
            }

        }
        private void SetupAltimeterForm()
        {
            if (Properties.Settings.Default.EnableAltimeterOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.Altimeter_OutputDisplay);
                _altimeterForm = new InstrumentForm();
                _altimeterForm.Text = "Altimeter";
                _altimeterForm.ShowInTaskbar = false;
                _altimeterForm.ShowIcon = false;
                if (Properties.Settings.Default.Altimeter_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _altimeterForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.Altimeter_OutULX, Properties.Settings.Default.Altimeter_OutULY);
                    size = new Size(Properties.Settings.Default.Altimeter_OutLRX - Properties.Settings.Default.Altimeter_OutULX, Properties.Settings.Default.Altimeter_OutLRY - Properties.Settings.Default.Altimeter_OutULY);
                    _altimeterForm.StretchToFill = false;
                }
                _altimeterForm.AlwaysOnTop = Properties.Settings.Default.Altimeter_AlwaysOnTop;
                _altimeterForm.Monochrome = Properties.Settings.Default.Altimeter_Monochrome;
                _altimeterForm.Rotation = Properties.Settings.Default.Altimeter_RotateFlipType;
                _altimeterForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_altimeterForm, _applicationForm, screen, location, size, true, true);
                _altimeterForm.DataChanged += new EventHandler(_altimeterForm_DataChanged);
                _altimeterForm.Disposed += new EventHandler(_altimeterForm_Disposed);
                _outputForms.Add(_altimeterRenderer, _altimeterForm);
            }

        }
        private void SetupCabinPressForm()
        {
            if (Properties.Settings.Default.EnableCabinPressOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.CabinPress_OutputDisplay);
                _cabinPressForm = new InstrumentForm();
                _cabinPressForm.Text = "Cabin Pressure Altitude Indicator";
                _cabinPressForm.ShowInTaskbar = false;
                _cabinPressForm.ShowIcon = false;
                if (Properties.Settings.Default.CabinPress_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _cabinPressForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.CabinPress_OutULX, Properties.Settings.Default.CabinPress_OutULY);
                    size = new Size(Properties.Settings.Default.CabinPress_OutLRX - Properties.Settings.Default.CabinPress_OutULX, Properties.Settings.Default.CabinPress_OutLRY - Properties.Settings.Default.CabinPress_OutULY);
                    _cabinPressForm.StretchToFill = false;
                }
                _cabinPressForm.AlwaysOnTop = Properties.Settings.Default.CabinPress_AlwaysOnTop;
                _cabinPressForm.Monochrome = Properties.Settings.Default.CabinPress_Monochrome;
                _cabinPressForm.Rotation = Properties.Settings.Default.CabinPress_RotateFlipType;
                _cabinPressForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_cabinPressForm, _applicationForm, screen, location, size, true, true);
                _cabinPressForm.DataChanged += new EventHandler(_cabinPressForm_DataChanged);
                _cabinPressForm.Disposed += new EventHandler(_cabinPressForm_Disposed);
                _outputForms.Add(_cabinPressRenderer, _cabinPressForm);
            }

        }
        private void SetupRollTrimForm()
        {
            if (Properties.Settings.Default.EnableRollTrimOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.RollTrim_OutputDisplay);
                _rollTrimForm = new InstrumentForm();
                _rollTrimForm.Text = "Roll Trim Indicator";
                _rollTrimForm.ShowInTaskbar = false;
                _rollTrimForm.ShowIcon = false;
                if (Properties.Settings.Default.RollTrim_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _rollTrimForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.RollTrim_OutULX, Properties.Settings.Default.RollTrim_OutULY);
                    size = new Size(Properties.Settings.Default.RollTrim_OutLRX - Properties.Settings.Default.RollTrim_OutULX, Properties.Settings.Default.RollTrim_OutLRY - Properties.Settings.Default.RollTrim_OutULY);
                    _rollTrimForm.StretchToFill = false;
                }
                _rollTrimForm.AlwaysOnTop = Properties.Settings.Default.RollTrim_AlwaysOnTop;
                _rollTrimForm.Monochrome = Properties.Settings.Default.RollTrim_Monochrome;
                _rollTrimForm.Rotation = Properties.Settings.Default.RollTrim_RotateFlipType;
                _rollTrimForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_rollTrimForm, _applicationForm, screen, location, size, true, true);
                _rollTrimForm.DataChanged += new EventHandler(_rollTrimForm_DataChanged);
                _rollTrimForm.Disposed += new EventHandler(_rollTrimForm_Disposed);
                _outputForms.Add(_rollTrimRenderer, _rollTrimForm);
            }

        }

        private void SetupPitchTrimForm()
        {
            if (Properties.Settings.Default.EnablePitchTrimOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.PitchTrim_OutputDisplay);
                _pitchTrimForm = new InstrumentForm();
                _pitchTrimForm.Text = "Pitch Trim Indicator";
                _pitchTrimForm.ShowInTaskbar = false;
                _pitchTrimForm.ShowIcon = false;
                if (Properties.Settings.Default.PitchTrim_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _pitchTrimForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.PitchTrim_OutULX, Properties.Settings.Default.PitchTrim_OutULY);
                    size = new Size(Properties.Settings.Default.PitchTrim_OutLRX - Properties.Settings.Default.PitchTrim_OutULX, Properties.Settings.Default.PitchTrim_OutLRY - Properties.Settings.Default.PitchTrim_OutULY);
                    _pitchTrimForm.StretchToFill = false;
                }
                _pitchTrimForm.AlwaysOnTop = Properties.Settings.Default.PitchTrim_AlwaysOnTop;
                _pitchTrimForm.Monochrome = Properties.Settings.Default.PitchTrim_Monochrome;
                _pitchTrimForm.Rotation = Properties.Settings.Default.PitchTrim_RotateFlipType;
                _pitchTrimForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_pitchTrimForm, _applicationForm, screen, location, size, true, true);
                _pitchTrimForm.DataChanged += new EventHandler(_pitchTrimForm_DataChanged);
                _pitchTrimForm.Disposed += new EventHandler(_pitchTrimForm_Disposed);
                _outputForms.Add(_pitchTrimRenderer, _pitchTrimForm);
            }

        }

        private void SetupHydAForm()
        {
            if (Properties.Settings.Default.EnableHYDAOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.HYDA_OutputDisplay);
                _hydAForm = new InstrumentForm();
                _hydAForm.Text = "Hydraulic Pressure Indicator A";
                _hydAForm.ShowInTaskbar = false;
                _hydAForm.ShowIcon = false;
                if (Properties.Settings.Default.HYDA_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hydAForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.HYDA_OutULX, Properties.Settings.Default.HYDA_OutULY);
                    size = new Size(Properties.Settings.Default.HYDA_OutLRX - Properties.Settings.Default.HYDA_OutULX, Properties.Settings.Default.HYDA_OutLRY - Properties.Settings.Default.HYDA_OutULY);
                    _hydAForm.StretchToFill = false;
                }
                _hydAForm.AlwaysOnTop = Properties.Settings.Default.HYDA_AlwaysOnTop;
                _hydAForm.Monochrome = Properties.Settings.Default.HYDA_Monochrome;
                _hydAForm.Rotation = Properties.Settings.Default.HYDA_RotateFlipType;
                _hydAForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hydAForm, _applicationForm, screen, location, size, true, true);
                _hydAForm.DataChanged += new EventHandler(_hydAForm_DataChanged);
                _hydAForm.Disposed += new EventHandler(_hydAForm_Disposed);
                _outputForms.Add(_hydARenderer, _hydAForm);
            }
        }
        private void SetupHydBForm()
        {
            if (Properties.Settings.Default.EnableHYDBOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.HYDB_OutputDisplay);
                _hydBForm = new InstrumentForm();
                _hydBForm.Text = "Hydraulic Pressure Indicator B";
                _hydBForm.ShowInTaskbar = false;
                _hydBForm.ShowIcon = false;
                if (Properties.Settings.Default.HYDB_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _hydBForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.HYDB_OutULX, Properties.Settings.Default.HYDB_OutULY);
                    size = new Size(Properties.Settings.Default.HYDB_OutLRX - Properties.Settings.Default.HYDB_OutULX, Properties.Settings.Default.HYDB_OutLRY - Properties.Settings.Default.HYDB_OutULY);
                    _hydBForm.StretchToFill = false;
                }
                _hydBForm.AlwaysOnTop = Properties.Settings.Default.HYDB_AlwaysOnTop;
                _hydBForm.Monochrome = Properties.Settings.Default.HYDB_Monochrome;
                _hydBForm.Rotation = Properties.Settings.Default.HYDB_RotateFlipType;
                _hydBForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_hydBForm, _applicationForm, screen, location, size, true, true);
                _hydBForm.DataChanged += new EventHandler(_hydBForm_DataChanged);
                _hydBForm.Disposed += new EventHandler(_hydBForm_Disposed);
                _outputForms.Add(_hydBRenderer, _hydBForm);
            }

        }
        private void SetupASIForm()
        {
            if (Properties.Settings.Default.EnableASIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.ASI_OutputDisplay);
                _asiForm = new InstrumentForm();
                _asiForm.Text = "Airspeed Indicator";
                _asiForm.ShowInTaskbar = false;
                _asiForm.ShowIcon = false;
                if (Properties.Settings.Default.ASI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _asiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.ASI_OutULX, Properties.Settings.Default.ASI_OutULY);
                    size = new Size(Properties.Settings.Default.ASI_OutLRX - Properties.Settings.Default.ASI_OutULX, Properties.Settings.Default.ASI_OutLRY - Properties.Settings.Default.ASI_OutULY);
                    _asiForm.StretchToFill = false;
                }
                _asiForm.AlwaysOnTop = Properties.Settings.Default.ASI_AlwaysOnTop;
                _asiForm.Monochrome = Properties.Settings.Default.ASI_Monochrome;
                _asiForm.Rotation = Properties.Settings.Default.ASI_RotateFlipType;
                _asiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_asiForm, _applicationForm, screen, location, size, true, true);
                _asiForm.DataChanged += new EventHandler(_asiForm_DataChanged);
                _asiForm.Disposed += new EventHandler(_asiForm_Disposed);
                _outputForms.Add(_asiRenderer, _asiForm);
            }

        }
        private void SetupADIForm()
        {
            if (Properties.Settings.Default.EnableADIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.ADI_OutputDisplay);
                _adiForm = new InstrumentForm();
                _adiForm.Text = "Attitude Indicator";
                _adiForm.ShowInTaskbar = false;
                _adiForm.ShowIcon = false;
                if (Properties.Settings.Default.ADI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _adiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.ADI_OutULX, Properties.Settings.Default.ADI_OutULY);
                    size = new Size(Properties.Settings.Default.ADI_OutLRX - Properties.Settings.Default.ADI_OutULX, Properties.Settings.Default.ADI_OutLRY - Properties.Settings.Default.ADI_OutULY);
                    _adiForm.StretchToFill = false;
                }
                _adiForm.AlwaysOnTop = Properties.Settings.Default.ADI_AlwaysOnTop;
                _adiForm.Monochrome = Properties.Settings.Default.ADI_Monochrome;
                _adiForm.Rotation = Properties.Settings.Default.ADI_RotateFlipType;
                _adiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_adiForm, _applicationForm, screen, location, size, true, true);
                _adiForm.DataChanged += new EventHandler(_adiForm_DataChanged);
                _adiForm.Disposed += new EventHandler(_adiForm_Disposed);
                _outputForms.Add(_adiRenderer, _adiForm);
            }

        }
        private void SetupBackupADIForm()
        {
            if (Properties.Settings.Default.EnableBackupADIOutput)
            {
                Point location;
                Size size = new Size();
                Screen screen = Common.Screen.Util.FindScreen(Properties.Settings.Default.Backup_ADI_OutputDisplay);
                _backupAdiForm = new InstrumentForm();
                _backupAdiForm.Text = "Standby Attitude Indicator";
                _backupAdiForm.ShowInTaskbar = false;
                _backupAdiForm.ShowIcon = false;
                if (Properties.Settings.Default.Backup_ADI_StretchToFit)
                {
                    location = new Point(0, 0);
                    size = screen.Bounds.Size;
                    _backupAdiForm.StretchToFill = true;
                }
                else
                {
                    location = new Point(Properties.Settings.Default.Backup_ADI_OutULX, Properties.Settings.Default.Backup_ADI_OutULY);
                    size = new Size(Properties.Settings.Default.Backup_ADI_OutLRX - Properties.Settings.Default.Backup_ADI_OutULX, Properties.Settings.Default.Backup_ADI_OutLRY - Properties.Settings.Default.Backup_ADI_OutULY);
                    _backupAdiForm.StretchToFill = false;
                }
                _backupAdiForm.AlwaysOnTop = Properties.Settings.Default.Backup_ADI_AlwaysOnTop;
                _backupAdiForm.Monochrome = Properties.Settings.Default.Backup_ADI_Monochrome;
                _backupAdiForm.Rotation = Properties.Settings.Default.Backup_ADI_RotateFlipType;
                _backupAdiForm.WindowState = FormWindowState.Normal;
                Common.Screen.Util.OpenFormOnSpecificMonitor(_backupAdiForm, _applicationForm, screen, location, size, true, true);
                _backupAdiForm.DataChanged += new EventHandler(_backupAdiForm_DataChanged);
                _backupAdiForm.Disposed += new EventHandler(_backupAdiForm_Disposed);
                _outputForms.Add(_backupAdiRenderer, _backupAdiForm);
            }

        }
        #endregion
        #endregion
        #region Instrument Form Disposal Event handlers
        /// <summary>
        /// Event handler for the Disposed event for the VVI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _vviForm_Disposed(object sender, EventArgs e)
        {
            _vviForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the RPM 2 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _rpm2Form_Disposed(object sender, EventArgs e)
        {
            _rpm2Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the RPM 1 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _rpm1Form_Disposed(object sender, EventArgs e)
        {
            _rpm1Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Speedbrake form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _speedbrakeForm_Disposed(object sender, EventArgs e)
        {
            _speedbrakeForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the RWR form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _rwrForm_Disposed(object sender, EventArgs e)
        {
            _rwrForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Gear Lights form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _landingGearLightsForm_Disposed(object sender, EventArgs e)
        {
            _landingGearLightsForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the OIL 1 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _oilGauge1Form_Disposed(object sender, EventArgs e)
        {
            _oilGauge1Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the OIL 2 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _oilGauge2Form_Disposed(object sender, EventArgs e)
        {
            _oilGauge2Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the NOZ POS 1 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _nozPos1Form_Disposed(object sender, EventArgs e)
        {
            _nozPos1Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the NOZ POS 2 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _nozPos2Form_Disposed(object sender, EventArgs e)
        {
            _nozPos2Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the NWS Indexer form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _nwsIndexerForm_Disposed(object sender, EventArgs e)
        {
            _nwsIndexerForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the HSI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _hsiForm_Disposed(object sender, EventArgs e)
        {
            _hsiForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the EHSI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _ehsiForm_Disposed(object sender, EventArgs e)
        {
            _ehsiForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Fuel Quantity form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _fuelQuantityForm_Disposed(object sender, EventArgs e)
        {
            _fuelQuantityForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Fuel Flow form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _fuelFlowForm_Disposed(object sender, EventArgs e)
        {
            _fuelFlowForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the ISIS form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _isisForm_Disposed(object sender, EventArgs e)
        {
            _isisForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Accelerometer form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _accelerometerForm_Disposed(object sender, EventArgs e)
        {
            _accelerometerForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the FTIT 2 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _ftit2Form_Disposed(object sender, EventArgs e)
        {
            _ftit2Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the FTIT 1 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _ftit1Form_Disposed(object sender, EventArgs e)
        {
            _ftit1Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the EPU Fuel form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _epuFuelForm_Disposed(object sender, EventArgs e)
        {
            _epuFuelForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the PFL form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _pflForm_Disposed(object sender, EventArgs e)
        {
            _pflForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the DED form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _dedForm_Disposed(object sender, EventArgs e)
        {
            _dedForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Compass form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _compassForm_Disposed(object sender, EventArgs e)
        {
            _compassForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the CMDS panel form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _cmdsPanelForm_Disposed(object sender, EventArgs e)
        {
            _cmdsPanelForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Caution Panel form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _cautionPanelForm_Disposed(object sender, EventArgs e)
        {
            _cautionPanelForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the AOA Indicator form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _aoaIndicatorForm_Disposed(object sender, EventArgs e)
        {
            _aoaIndicatorForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the AOA Indexer form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _aoaIndexerForm_Disposed(object sender, EventArgs e)
        {
            _aoaIndexerForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Altimeter form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _altimeterForm_Disposed(object sender, EventArgs e)
        {
            _altimeterForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the ASI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _asiForm_Disposed(object sender, EventArgs e)
        {
            _asiForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the ADI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _adiForm_Disposed(object sender, EventArgs e)
        {
            _adiForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Backup ADI form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _backupAdiForm_Disposed(object sender, EventArgs e)
        {
            _backupAdiForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the HYDA form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _hydAForm_Disposed(object sender, EventArgs e)
        {
            _hydAForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the HYDB form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _hydBForm_Disposed(object sender, EventArgs e)
        {
            _hydBForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Cabin Pressure Altitude Indicator form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _cabinPressForm_Disposed(object sender, EventArgs e)
        {
            _cabinPressForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Roll Trim Indicator form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _rollTrimForm_Disposed(object sender, EventArgs e)
        {
            _rollTrimForm = null;
        }

        /// <summary>
        /// Event handler for the Disposed event for the Pitch Trim Indicator form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _pitchTrimForm_Disposed(object sender, EventArgs e)
        {
            _pitchTrimForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the MFD #4 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _mfd4Form_Disposed(object sender, EventArgs e)
        {
            _mfd4Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the MFD #3 form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _mfd3Form_Disposed(object sender, EventArgs e)
        {
            _mfd3Form = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Left MFD form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _leftMfdForm_Disposed(object sender, EventArgs e)
        {
            _leftMfdForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the Right MFD form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
        private void _rightMfdForm_Disposed(object sender, EventArgs e)
        {
            _rightMfdForm = null;
        }
        /// <summary>
        /// Event handler for the Disposed event for the HUD form
        /// </summary>
        /// <param name="sender">Object raising this event</param>
        /// <param name="e">Event arguments for the Form's Disposed event</param>
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
        /// <summary>
        /// Handle change notifications when the Speedbrake output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _speedbrakeForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the RWR output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _rwrForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the RPM 2 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _rpm2Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the VVI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _vviForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the RPM 1 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _rpm1Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Engine 1 - Oil Pressure Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _oilGauge1Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Engine 2 - Oil Pressure Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _oilGauge2Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Engine 2 - Nozzle Position Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _nozPos2Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Engine 1 - Nozzle Position Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _nozPos1Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the NWS Indexer output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _nwsIndexerForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Fuel Quantity output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _fuelQuantityForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Landing Gear Lights output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _landingGearLightsForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the HSI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _hsiForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the EHSI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _ehsiForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Fuel Flow output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _fuelFlowForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the ISIS output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _isisForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Accelerometer output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _accelerometerForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the FTIT 2 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _ftit2Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the FTIT 1 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _ftit1Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the EPU Fuel output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _epuFuelForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the PFL output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _pflForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the DED output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _dedForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Compass output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _compassForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the CMDS Panel output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _cmdsPanelForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Caution Panel output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _cautionPanelForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the AOA Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _aoaIndicatorForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the AOA Indexer output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _aoaIndexerForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Altimeter output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _altimeterForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the ASI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _asiForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the ADI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _adiForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Backup ADI output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _backupAdiForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the HYDA output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _hydAForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the HYDB output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _hydBForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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

        /// <summary>
        /// Handle change notifications when the Cabin Pressure Altitude Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _cabinPressForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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


        /// <summary>
        /// Handle change notifications when the Roll Trim Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _rollTrimForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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

        /// <summary>
        /// Handle change notifications when the Pitch Trim Indicator output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _pitchTrimForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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

        /// <summary>
        /// Handle change notifications when the MFD #4 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _mfd4Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the MFD #3 output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _mfd3Form_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Left MFD output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _leftMfdForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the Right MFD output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _rightMfdForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
        /// <summary>
        /// Handle change notifications when the HUD output window moves or when the user makes changes to the context-menu options for this form
        /// </summary>
        /// <param name="sender">The object raising this event</param>
        /// <param name="e">Event arguments for the DataChanged event</param>
        private void _hudForm_DataChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
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
                    _log.Error(e.Message.ToString(), e);
                }
            }
        }
        /// <summary>
        /// Close all the output window forms
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
        /// Sets up all worker threads and output forms and starts the worker threads running
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
                _log.DebugFormat("Time taken to invoke the Starting event: {0}", startingEventTimeTaken.TotalMilliseconds);
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
        #region Thread Setup
        private void SetupThreads()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting setting up threads at: {0}", startTime.ToString());
            SetupSimStatusMonitorThread();
            SetupCaptureOrchestrationThread();
            SetupKeyboardWatcherThread();

            SetupMFD4CaptureThread();
            SetupMFD3CaptureThread();
            SetupLeftMFDCaptureThread();
            SetupRightMFDCaptureThread();
            SetupHUDCaptureThread();

            SetupADIRenderThread();
            SetupBackupADIRenderThread();
            SetupASIRenderThread();
            SetupAltimeterRenderThread();
            SetupAOAIndexerRenderThread();
            SetupAOAIndicatorRenderThread();
            SetupCautionPanelRenderThread();
            SetupCMDSPanelRenderThread();
            SetupCompassRenderThread();
            SetupDEDRenderThread();
            SetupPFLRenderThread();
            SetupEPUFuelRenderThread();
            SetupAccelerometerRenderThread();
            SetupFTIT1RenderThread();
            SetupFTIT2RenderThread();
            SetupFuelFlowRenderThread();
            SetupISISRenderThread();
            SetupFuelQuantityRenderThread();
            SetupHSIRenderThread();
            SetupEHSIRenderThread();
            SetupLandingGearLightsRenderThread();
            SetupNWSIndexerRenderThread();
            SetupNOZ1RenderThread();
            SetupNOZ2RenderThread();
            SetupOIL1RenderThread();
            SetupOIL2RenderThread();
            SetupRWRRenderThread();
            SetupSpeedbrakeRenderThread();
            SetupRPM1RenderThread();
            SetupRPM2RenderThread();
            SetupVVIRenderThread();
            SetupHYDARenderThread();
            SetupHYDBRenderThread();
            SetupCabinPressRenderThread();
            SetupRollTrimRenderThread();
            SetupPitchTrimRenderThread();

            DateTime endTime = DateTime.Now;
            _log.DebugFormat("Finished setting up threads at: {0}", endTime.ToString());
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Time taken setting up threads: {0}", elapsed.TotalMilliseconds);

        }
        private void AbortThread(ref Thread t)
        {
            if (t == null) return;
            try
            {
                t.Abort();
            }
            catch (Exception e)
            {
            }
            Common.Util.DisposeObject(t);
            t = null;
        }
        private void SetupKeyboardWatcherThread()
        {
            AbortThread(ref _keyboardWatcherThread);
            _keyboardWatcherThread = new Thread(KeyboardWatcherThreadWork);
            _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
            _keyboardWatcherThread.Priority = ThreadPriority.Highest;
            _keyboardWatcherThread.IsBackground = true;
            _keyboardWatcherThread.Name = "KeyboardWatcherThread";
        }
        private void SetupCaptureOrchestrationThread()
        {
            AbortThread(ref _captureOrchestrationThread);
            _captureOrchestrationThread = new Thread(CaptureOrchestrationThreadWork);
            _captureOrchestrationThread.Priority = _threadPriority;
            _captureOrchestrationThread.IsBackground = true;
            _captureOrchestrationThread.Name = "CaptureOrchestrationThread";
        }
        private void SetupVVIRenderThread()
        {
            AbortThread(ref _vviRenderThread);
            if (Properties.Settings.Default.EnableVVIOutput)
            {
                _vviRenderThread = new Thread(VVIRenderThreadWork);
                _vviRenderThread.Priority = _threadPriority;
                _vviRenderThread.IsBackground = true;
                _vviRenderThread.Name = "VVIRenderThread";
            }
        }
        private void SetupRPM2RenderThread()
        {

            AbortThread(ref _rpm2RenderThread);
            if (Properties.Settings.Default.EnableRPM2Output)
            {
                _rpm2RenderThread = new Thread(RPM2RenderThreadWork);
                _rpm2RenderThread.Priority = _threadPriority;
                _rpm2RenderThread.IsBackground = true;
                _rpm2RenderThread.Name = "RPM2RenderThread";
            }
        }
        private void SetupRPM1RenderThread()
        {
            AbortThread(ref _rpm1RenderThread);
            if (Properties.Settings.Default.EnableRPM1Output)
            {
                _rpm1RenderThread = new Thread(RPM1RenderThreadWork);
                _rpm1RenderThread.Priority = _threadPriority;
                _rpm1RenderThread.IsBackground = true;
                _rpm1RenderThread.Name = "RPM1RenderThread";
            }
        }
        private void SetupSpeedbrakeRenderThread()
        {
            AbortThread(ref _speedbrakeRenderThread);
            if (Properties.Settings.Default.EnableSpeedbrakeOutput)
            {
                _speedbrakeRenderThread = new Thread(SpeedbrakeRenderThreadWork);
                _speedbrakeRenderThread.Priority = _threadPriority;
                _speedbrakeRenderThread.IsBackground = true;
                _speedbrakeRenderThread.Name = "SpeedbrakeRenderThread";
            }
        }
        private void SetupRWRRenderThread()
        {
            AbortThread(ref _rwrRenderThread);
            if (Properties.Settings.Default.EnableRWROutput)
            {
                _rwrRenderThread = new Thread(RWRRenderThreadWork);
                _rwrRenderThread.Priority = _threadPriority;
                _rwrRenderThread.IsBackground = true;
                _rwrRenderThread.Name = "RWRRenderThread";
            }
        }
        private void SetupOIL2RenderThread()
        {
            AbortThread(ref _oilGauge2RenderThread);
            if (Properties.Settings.Default.EnableOIL2Output)
            {
                _oilGauge2RenderThread = new Thread(OilGauge2RenderThreadWork);
                _oilGauge2RenderThread.Priority = _threadPriority;
                _oilGauge2RenderThread.IsBackground = true;
                _oilGauge2RenderThread.Name = "OilGauge2RenderThread";
            }
        }
        private void SetupOIL1RenderThread()
        {
            AbortThread(ref _oilGauge1RenderThread);
            if (Properties.Settings.Default.EnableOIL1Output)
            {
                _oilGauge1RenderThread = new Thread(OilGauge1RenderThreadWork);
                _oilGauge1RenderThread.Priority = _threadPriority;
                _oilGauge1RenderThread.IsBackground = true;
                _oilGauge1RenderThread.Name = "OilGauge1RenderThread";
            }
        }
        private void SetupNOZ2RenderThread()
        {
            AbortThread(ref _nozPos2RenderThread);
            if (Properties.Settings.Default.EnableNOZ2Output)
            {
                _nozPos2RenderThread = new Thread(NOZPos2RenderThreadWork);
                _nozPos2RenderThread.Priority = _threadPriority;
                _nozPos2RenderThread.IsBackground = true;
                _nozPos2RenderThread.Name = "NOZPos2RenderThread";
            }
        }
        private void SetupNOZ1RenderThread()
        {
            AbortThread(ref _nozPos1RenderThread);
            if (Properties.Settings.Default.EnableNOZ1Output)
            {
                _nozPos1RenderThread = new Thread(NOZPos1RenderThreadWork);
                _nozPos1RenderThread.Priority = _threadPriority;
                _nozPos1RenderThread.IsBackground = true;
                _nozPos1RenderThread.Name = "NOZPos1RenderThread";
            }
        }
        private void SetupNWSIndexerRenderThread()
        {
            AbortThread(ref _nwsIndexerRenderThread);
            if (Properties.Settings.Default.EnableNWSIndexerOutput)
            {
                _nwsIndexerRenderThread = new Thread(NWSIndexerRenderThreadWork);
                _nwsIndexerRenderThread.Priority = _threadPriority;
                _nwsIndexerRenderThread.IsBackground = true;
                _nwsIndexerRenderThread.Name = "NWSIndexerRenderThread";
            }
        }
        private void SetupLandingGearLightsRenderThread()
        {
            AbortThread(ref _landingGearLightsRenderThread);
            if (Properties.Settings.Default.EnableGearLightsOutput)
            {
                _landingGearLightsRenderThread = new Thread(LandingGearLightsRenderThreadWork);
                _landingGearLightsRenderThread.Priority = _threadPriority;
                _landingGearLightsRenderThread.IsBackground = true;
                _landingGearLightsRenderThread.Name = "LandingGearLightsRenderThread";
            }
        }
        private void SetupHSIRenderThread()
        {
            AbortThread(ref _hsiRenderThread);
            if (Properties.Settings.Default.EnableHSIOutput)
            {
                _hsiRenderThread = new Thread(HSIRenderThreadWork);
                _hsiRenderThread.Priority = _threadPriority;
                _hsiRenderThread.IsBackground = true;
                _hsiRenderThread.Name = "HSIRenderThread";
            }
        }
        private void SetupEHSIRenderThread()
        {
            AbortThread(ref _ehsiRenderThread);
            if (Properties.Settings.Default.EnableEHSIOutput)
            {
                _ehsiRenderThread = new Thread(EHSIRenderThreadWork);
                _ehsiRenderThread.Priority = _threadPriority;
                _ehsiRenderThread.IsBackground = true;
                _ehsiRenderThread.Name = "EHSIRenderThread";
            }
        }
        private void SetupFuelQuantityRenderThread()
        {
            AbortThread(ref _fuelQuantityRenderThread);
            if (Properties.Settings.Default.EnableFuelQuantityOutput)
            {
                _fuelQuantityRenderThread = new Thread(FuelQuantityRenderThreadWork);
                _fuelQuantityRenderThread.Priority = _threadPriority;
                _fuelQuantityRenderThread.IsBackground = true;
                _fuelQuantityRenderThread.Name = "FuelQuantityRenderThread";
            }
        }
        private void SetupFuelFlowRenderThread()
        {
            AbortThread(ref _fuelFlowRenderThread);
            if (Properties.Settings.Default.EnableFuelFlowOutput)
            {
                _fuelFlowRenderThread = new Thread(FuelFlowRenderThreadWork);
                _fuelFlowRenderThread.Priority = _threadPriority;
                _fuelFlowRenderThread.IsBackground = true;
                _fuelFlowRenderThread.Name = "FuelFlowRenderThread";
            }
        }
        private void SetupISISRenderThread()
        {
            AbortThread(ref _isisRenderThread);
            if (Properties.Settings.Default.EnableISISOutput)
            {
                _isisRenderThread = new Thread(ISISRenderThreadWork);
                _isisRenderThread.Priority = _threadPriority;
                _isisRenderThread.IsBackground = true;
                _isisRenderThread.Name = "ISISRenderThread";
            }
        }
        private void SetupAccelerometerRenderThread()
        {
            AbortThread(ref _accelerometerRenderThread);
            if (Properties.Settings.Default.EnableAccelerometerOutput)
            {
                _accelerometerRenderThread = new Thread(AccelerometerRenderThreadWork);
                _accelerometerRenderThread.Priority = _threadPriority;
                _accelerometerRenderThread.IsBackground = true;
                _accelerometerRenderThread.Name = "AccelerometerRenderThread";
            }
        }
        private void SetupFTIT2RenderThread()
        {
            AbortThread(ref _ftit2RenderThread);
            if (Properties.Settings.Default.EnableFTIT2Output)
            {
                _ftit2RenderThread = new Thread(FTIT2RenderThreadWork);
                _ftit2RenderThread.Priority = _threadPriority;
                _ftit2RenderThread.IsBackground = true;
                _ftit2RenderThread.Name = "FTIT2RenderThread";
            }
        }
        private void SetupFTIT1RenderThread()
        {
            AbortThread(ref _ftit1RenderThread);
            if (Properties.Settings.Default.EnableFTIT1Output)
            {
                _ftit1RenderThread = new Thread(FTIT1RenderThreadWork);
                _ftit1RenderThread.Priority = _threadPriority;
                _ftit1RenderThread.IsBackground = true;
                _ftit1RenderThread.Name = "FTIT1RenderThread";
            }
        }
        private void SetupEPUFuelRenderThread()
        {
            AbortThread(ref _epuFuelRenderThread);
            if (Properties.Settings.Default.EnableEPUFuelOutput)
            {
                _epuFuelRenderThread = new Thread(EPUFuelRenderThreadWork);
                _epuFuelRenderThread.Priority = _threadPriority;
                _epuFuelRenderThread.IsBackground = true;
                _epuFuelRenderThread.Name = "EPUFuelRenderThread";
            }
        }
        private void SetupPFLRenderThread()
        {
            AbortThread(ref _pflRenderThread);
            if (Properties.Settings.Default.EnablePFLOutput)
            {
                _pflRenderThread = new Thread(PFLRenderThreadWork);
                _pflRenderThread.Priority = _threadPriority;
                _pflRenderThread.IsBackground = true;
                _pflRenderThread.Name = "PFLRenderThread";
            }
        }
        private void SetupDEDRenderThread()
        {
            AbortThread(ref _dedRenderThread);
            if (Properties.Settings.Default.EnableDEDOutput)
            {
                _dedRenderThread = new Thread(DEDRenderThreadWork);
                _dedRenderThread.Priority = _threadPriority;
                _dedRenderThread.IsBackground = true;
                _dedRenderThread.Name = "DEDRenderThread";
            }
        }
        private void SetupCompassRenderThread()
        {
            AbortThread(ref _compassRenderThread);
            if (Properties.Settings.Default.EnableCompassOutput)
            {
                _compassRenderThread = new Thread(CompassRenderThreadWork);
                _compassRenderThread.Priority = _threadPriority;
                _compassRenderThread.IsBackground = true;
                _compassRenderThread.Name = "CompassRenderThread";
            }
        }
        private void SetupCMDSPanelRenderThread()
        {
            AbortThread(ref _cmdsPanelRenderThread);
            if (Properties.Settings.Default.EnableCMDSOutput)
            {
                _cmdsPanelRenderThread = new Thread(CMDSPanelRenderThreadWork);
                _cmdsPanelRenderThread.Priority = _threadPriority;
                _cmdsPanelRenderThread.IsBackground = true;
                _cmdsPanelRenderThread.Name = "CMDSPanelRenderThread";
            }
        }
        private void SetupCautionPanelRenderThread()
        {
            AbortThread(ref _cautionPanelRenderThread);
            if (Properties.Settings.Default.EnableCautionPanelOutput)
            {
                _cautionPanelRenderThread = new Thread(CautionPanelRenderThreadWork);
                _cautionPanelRenderThread.Priority = _threadPriority;
                _cautionPanelRenderThread.IsBackground = true;
                _cautionPanelRenderThread.Name = "CautionPanelRenderThread";
            }
        }
        private void SetupAOAIndicatorRenderThread()
        {
            AbortThread(ref _aoaIndicatorRenderThread);
            if (Properties.Settings.Default.EnableAOAIndicatorOutput)
            {
                _aoaIndicatorRenderThread = new Thread(AOAIndicatorRenderThreadWork);
                _aoaIndicatorRenderThread.Priority = _threadPriority;
                _aoaIndicatorRenderThread.IsBackground = true;
                _aoaIndicatorRenderThread.Name = "AOAIndicatorRenderThread";
            }
        }
        private void SetupAOAIndexerRenderThread()
        {
            AbortThread(ref _aoaIndexerRenderThread);
            if (Properties.Settings.Default.EnableAOAIndexerOutput)
            {
                _aoaIndexerRenderThread = new Thread(AOAIndexerRenderThreadWork);
                _aoaIndexerRenderThread.Priority = _threadPriority;
                _aoaIndexerRenderThread.IsBackground = true;
                _aoaIndexerRenderThread.Name = "AOAIndexerRenderThread";
            }
        }
        private void SetupAltimeterRenderThread()
        {
            AbortThread(ref _altimeterRenderThread);
            if (Properties.Settings.Default.EnableAltimeterOutput)
            {
                _altimeterRenderThread = new Thread(AltimeterRenderThreadWork);
                _altimeterRenderThread.Priority = _threadPriority;
                _altimeterRenderThread.IsBackground = true;
                _altimeterRenderThread.Name = "AltimeterRenderThread";
            }
        }
        private void SetupASIRenderThread()
        {
            AbortThread(ref _asiRenderThread);
            if (Properties.Settings.Default.EnableASIOutput)
            {
                _asiRenderThread = new Thread(ASIRenderThreadWork);
                _asiRenderThread.Priority = _threadPriority;
                _asiRenderThread.IsBackground = true;
                _asiRenderThread.Name = "ASIRenderThread";
            }
        }
        private void SetupADIRenderThread()
        {
            AbortThread(ref _adiRenderThread);
            if (Properties.Settings.Default.EnableADIOutput)
            {
                _adiRenderThread = new Thread(ADIRenderThreadWork);
                _adiRenderThread.Priority = _threadPriority;
                _adiRenderThread.IsBackground = true;
                _adiRenderThread.Name = "ADIRenderThread";
            }
        }
        private void SetupBackupADIRenderThread()
        {
            AbortThread(ref _backupAdiRenderThread);
            if (Properties.Settings.Default.EnableBackupADIOutput)
            {
                _backupAdiRenderThread = new Thread(BackupADIRenderThreadWork);
                _backupAdiRenderThread.Priority = _threadPriority;
                _backupAdiRenderThread.IsBackground = true;
                _backupAdiRenderThread.Name = "BackupADIRenderThread";
            }
        }
        private void SetupHYDARenderThread()
        {
            AbortThread(ref _hydARenderThread);
            if (Properties.Settings.Default.EnableHYDAOutput)
            {
                _hydARenderThread = new Thread(HYDARenderThreadWork);
                _hydARenderThread.Priority = _threadPriority;
                _hydARenderThread.IsBackground = true;
                _hydARenderThread.Name = "HydARenderThread";
            }
        }
        private void SetupHYDBRenderThread()
        {
            AbortThread(ref _hydBRenderThread);
            if (Properties.Settings.Default.EnableHYDBOutput)
            {
                _hydBRenderThread = new Thread(HYDBRenderThreadWork);
                _hydBRenderThread.Priority = _threadPriority;
                _hydBRenderThread.IsBackground = true;
                _hydBRenderThread.Name = "HydBRenderThread";
            }
        }
        private void SetupCabinPressRenderThread()
        {
            AbortThread(ref _cabinPressRenderThread);
            if (Properties.Settings.Default.EnableCabinPressOutput)
            {
                _cabinPressRenderThread = new Thread(CabinPressRenderThreadWork);
                _cabinPressRenderThread.Priority = _threadPriority;
                _cabinPressRenderThread.IsBackground = true;
                _cabinPressRenderThread.Name = "CabinPressRenderThread";
            }
        }
        private void SetupRollTrimRenderThread()
        {
            AbortThread(ref _rollTrimRenderThread);
            if (Properties.Settings.Default.EnableRollTrimOutput)
            {
                _rollTrimRenderThread = new Thread(RollTrimRenderThreadWork);
                _rollTrimRenderThread.Priority = _threadPriority;
                _rollTrimRenderThread.IsBackground = true;
                _rollTrimRenderThread.Name = "RollTrimRenderThread";
            }
        }
        private void SetupPitchTrimRenderThread()
        {
            AbortThread(ref _pitchTrimRenderThread);
            if (Properties.Settings.Default.EnablePitchTrimOutput)
            {
                _pitchTrimRenderThread = new Thread(PitchTrimRenderThreadWork);
                _pitchTrimRenderThread.Priority = _threadPriority;
                _pitchTrimRenderThread.IsBackground = true;
                _pitchTrimRenderThread.Name = "PitchTrimRenderThread";
            }
        }
        private void SetupHUDCaptureThread()
        {
            AbortThread(ref _hudCaptureThread);
            if (Properties.Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server)
            {
                _hudCaptureThread = new Thread(HudCaptureThreadWork);
                _hudCaptureThread.Priority = _threadPriority;
                _hudCaptureThread.IsBackground = true;
                _hudCaptureThread.Name = "HudCaptureThread";
            }
        }
        private void SetupRightMFDCaptureThread()
        {
            AbortThread(ref _rightMfdCaptureThread);
            if (Properties.Settings.Default.EnableRightMFDOutput || NetworkMode == NetworkMode.Server)
            {
                _rightMfdCaptureThread = new Thread(RightMfdCaptureThreadWork);
                _rightMfdCaptureThread.Priority = _threadPriority;
                _rightMfdCaptureThread.IsBackground = true;
                _rightMfdCaptureThread.Name = "RightMfdCaptureThread";
            }
        }
        private void SetupLeftMFDCaptureThread()
        {
            AbortThread(ref _leftMfdCaptureThread);
            if (Properties.Settings.Default.EnableLeftMFDOutput || NetworkMode == NetworkMode.Server)
            {
                _leftMfdCaptureThread = new Thread(LeftMfdCaptureThreadWork);
                _leftMfdCaptureThread.Priority = _threadPriority;
                _leftMfdCaptureThread.IsBackground = true;
                _leftMfdCaptureThread.Name = "LeftMfdCaptureThread";
            }
        }
        private void SetupMFD3CaptureThread()
        {
            AbortThread(ref _mfd3CaptureThread);
            if (Properties.Settings.Default.EnableMfd3Output || NetworkMode == NetworkMode.Server)
            {
                _mfd3CaptureThread = new Thread(Mfd3CaptureThreadWork);
                _mfd3CaptureThread.Priority = _threadPriority;
                _mfd3CaptureThread.IsBackground = true;
                _mfd3CaptureThread.Name = "Mfd3CaptureThread";
            }
        }
        private void SetupMFD4CaptureThread()
        {
            AbortThread(ref _mfd4CaptureThread);
            if (Properties.Settings.Default.EnableMfd4Output || NetworkMode == NetworkMode.Server)
            {
                _mfd4CaptureThread = new Thread(Mfd4CaptureThreadWork);
                _mfd4CaptureThread.Priority = _threadPriority;
                _mfd4CaptureThread.IsBackground = true;
                _mfd4CaptureThread.Name = "Mfd4CaptureThread";
            }
        }
        private void SetupSimStatusMonitorThread()
        {
            AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork);
            _simStatusMonitorThread.Priority = ThreadPriority.BelowNormal;
            _simStatusMonitorThread.IsBackground = true;
            _simStatusMonitorThread.Name = "SimStatusMonitorThread";
        }
        #endregion
        #region Gauges rendering thread-work methods
        private void UpdateRendererStatesFromFlightData()
        {
            if (_flightData == null || (this.NetworkMode != NetworkMode.Client && !_simRunning))
            {
                _flightData = new FlightData();
                _flightData.hsiBits = Int32.MaxValue;
            }

            FlightData fromFalcon = _flightData;
            FlightDataExtension extensionData = ((FlightDataExtension)fromFalcon.ExtensionData);

            if (_simRunning || NetworkMode == NetworkMode.Client)
            {
                HsiBits hsibits = ((HsiBits)fromFalcon.hsiBits);
                AltBits altbits = ((AltBits)fromFalcon.altBits); //12-08-12 added by Falcas 
                bool commandBarsOn = false;

                //*** UPDATE ISIS ***
                ((F16ISIS)_isisRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && _useBMSAdvancedSharedmemValues)
                {
                    ((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                    //((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (-fromFalcon.z, ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure, ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);

                    if (fromFalcon.VersionNum >= 111)
                    {
                        if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                        {
                            ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury;
                        }
                        else
                        {
                            ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = F16ISIS.F16ISISOptions.PressureUnits.Millibars;
                        }

                        ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure = fromFalcon.AltCalReading; //13-08-12 added by Falcas
                    }
                    else
                    {
                        ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure = 2992f; //14-0-12 Falcas removed the point
                        ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                    }
                }
                else
                {
                    //((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure, ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits == F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury);
                    ((F16ISIS)_isisRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                    ((F16ISIS)_isisRenderer).InstrumentState.BarometricPressure = 2992f; //14-0-12 Falcas removed the point
                    ((F16ISIS)_isisRenderer).Options.PressureAltitudeUnits = F16ISIS.F16ISISOptions.PressureUnits.InchesOfMercury; //14-08-12 added by Falcas
                }
                if (extensionData != null)
                {
                    ((F16ISIS)_isisRenderer).InstrumentState.RadarAltitudeAGL = extensionData.RadarAltitudeFeetAGL;
                }
                ((F16ISIS)_isisRenderer).InstrumentState.MachNumber = fromFalcon.mach;
                ((F16ISIS)_isisRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                ((F16ISIS)_isisRenderer).InstrumentState.NeverExceedSpeedKnots = 850;

                ((F16ISIS)_isisRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
                ((F16ISIS)_isisRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
                ((F16ISIS)_isisRenderer).InstrumentState.VerticalVelocityFeetPerMinute = -fromFalcon.zDot * 60.0f;
                ((F16ISIS)_isisRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                ((F16ISIS)_isisRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
                ((F16ISIS)_isisRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
                ((F16ISIS)_isisRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);


                // *** UPDATE VVI ***
                float verticalVelocity = 0;
                if (((hsibits & HsiBits.VVI) == HsiBits.VVI))
                {
                    verticalVelocity = 0;
                }
                else
                {
                    verticalVelocity = -fromFalcon.zDot * 60.0f;
                }

                if (_vviRenderer is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU)_vviRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                    ((F16VerticalVelocityIndicatorEU)_vviRenderer).InstrumentState.VerticalVelocityFeet = verticalVelocity;
                }
                else if (_vviRenderer is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA)_vviRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.VVI) == HsiBits.VVI);
                    ((F16VerticalVelocityIndicatorUSA)_vviRenderer).InstrumentState.VerticalVelocityFeetPerMinute = verticalVelocity;
                }

                //*********************

                // *** UPDATE ALTIMETER ***
                if (fromFalcon.DataFormat == FalconDataFormats.BMS4 && _useBMSAdvancedSharedmemValues)
                {
                    if (((altbits & AltBits.CalType) == AltBits.CalType)) //13-08-12 added by Falcas
                    {
                        ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury;
                    }
                    else
                    {
                        ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars;
                    }
                    //((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude (- fromFalcon.z,((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure, ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury) ;
                    ((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.aauz;
                    if (fromFalcon.VersionNum >= 111)
                    {
                        ((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure = fromFalcon.AltCalReading; //12-08-12 added by Falcas
                        ((F16Altimeter)_altimeterRenderer).InstrumentState.PneumaticModeFlag = ((altbits & AltBits.PneuFlag) == AltBits.PneuFlag); //12-08-12 added by Falcas
                    }
                    else
                    {
                        ((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure = 2992f; //12-08-12 added by Falcas
                        ((F16Altimeter)_altimeterRenderer).InstrumentState.PneumaticModeFlag = false; //12-08-12 added by Falcas
                        ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
                    }
                }
                else
                {
                    //((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = GetIndicatedAltitude(-fromFalcon.z, ((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure, ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits == F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury);
                    ((F16Altimeter)_altimeterRenderer).InstrumentState.IndicatedAltitudeFeetMSL = -fromFalcon.z;
                    ((F16Altimeter)_altimeterRenderer).InstrumentState.BarometricPressure = 2992f; //12-08-12 added by Falcas
                    ((F16Altimeter)_altimeterRenderer).InstrumentState.PneumaticModeFlag = false; //12-08-12 added by Falcas
                    ((F16Altimeter)_altimeterRenderer).Options.PressureAltitudeUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury; //12-08-12 added by Falcas
                }
                //*************************

                //*** UPDATE ASI ***
                ((F16AirspeedIndicator)_asiRenderer).InstrumentState.AirspeedKnots = fromFalcon.kias;
                ((F16AirspeedIndicator)_asiRenderer).InstrumentState.MachNumber = fromFalcon.mach;
                //*************************

                //**** UPDATE COMPASS
                ((F16Compass)_compassRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                //*******************

                //**** UPDATE AOA INDICATOR***
                if (((hsibits & HsiBits.AOA) == HsiBits.AOA))
                {
                    ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.OffFlag = true;
                    ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees = 0;
                }
                else
                {
                    ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.OffFlag = false;
                    ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees = fromFalcon.alpha;
                }
                //*******************

                //**** UPDATE AOA INDEXER***
                float aoa = ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.AngleOfAttackDegrees;
                ((F16AngleOfAttackIndexer)_aoaIndexerRenderer).InstrumentState.AoaBelow = ((fromFalcon.lightBits & (int)LightBits.AOABelow) == (int)LightBits.AOABelow);
                ((F16AngleOfAttackIndexer)_aoaIndexerRenderer).InstrumentState.AoaOn = ((fromFalcon.lightBits & (int)LightBits.AOAOn) == (int)LightBits.AOAOn);
                ((F16AngleOfAttackIndexer)_aoaIndexerRenderer).InstrumentState.AoaAbove = ((fromFalcon.lightBits & (int)LightBits.AOAAbove) == (int)LightBits.AOAAbove);
                //**************************


                //***** UPDATE ADI *****
                ((F16ADI)_adiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                ((F16ADI)_adiRenderer).InstrumentState.AuxFlag = ((hsibits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX);
                ((F16ADI)_adiRenderer).InstrumentState.GlideslopeFlag = ((hsibits & HsiBits.ADI_GS) == HsiBits.ADI_GS);
                ((F16ADI)_adiRenderer).InstrumentState.LocalizerFlag = ((hsibits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC);
                //**********************

                //***** UPDATE BACKUP ADI *****
                ((F16StandbyADI)_backupAdiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF);
                //**********************

                //***** UPDATE HSI ***** 
                ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.OffFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
                ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                ((F16EHSI)_ehsiRenderer).InstrumentState.NoDataFlag = ((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF);
                ((F16EHSI)_ehsiRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                //**********************

                if (((hsibits & HsiBits.BUP_ADI_OFF) == HsiBits.BUP_ADI_OFF))
                {
                    //if the standby ADI is off
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.RollDegrees = 0;
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.OffFlag = true;
                }
                else
                {
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
                    ((F16StandbyADI)_backupAdiRenderer).InstrumentState.OffFlag = false;
                }


                //***** UPDATE SOME COMPLEX HSI/ADI VARIABLES
                if (((hsibits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF))
                {
                    //if the ADI is off
                    ((F16ADI)_adiRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16ADI)_adiRenderer).InstrumentState.RollDegrees = 0;
                    ((F16ADI)_adiRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
                    ((F16ADI)_adiRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
                    ((F16ADI)_adiRenderer).InstrumentState.ShowCommandBars = false;

                    ((F16ISIS)_isisRenderer).InstrumentState.PitchDegrees = 0;
                    ((F16ISIS)_isisRenderer).InstrumentState.RollDegrees = 0;
                    ((F16ISIS)_isisRenderer).InstrumentState.GlideslopeDeviationDegrees = 0;
                    ((F16ISIS)_isisRenderer).InstrumentState.LocalizerDeviationDegrees = 0;
                    ((F16ISIS)_isisRenderer).InstrumentState.ShowCommandBars = false;
                }
                else
                {
                    ((F16ADI)_adiRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
                    ((F16ADI)_adiRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));

                    ((F16ISIS)_isisRenderer).InstrumentState.PitchDegrees = (float)((fromFalcon.pitch / Common.Math.Constants.RADIANS_PER_DEGREE));
                    ((F16ISIS)_isisRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));

                    //The following floating data is also crossed up in the flightData.h File:
                    //float AdiIlsHorPos;       // Position of horizontal ILS bar ----Vertical
                    //float AdiIlsVerPos;       // Position of vertical ILS bar-----horizontal
                    commandBarsOn = ((float)(Math.Abs(Math.Round(fromFalcon.AdiIlsHorPos, 4))) != 0.1745f);
                    if (
                            (Math.Abs((fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE)) > 1.0f)
                                ||
                            (Math.Abs((fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE)) > 5.0f)
                        )
                    {
                        commandBarsOn = false;
                    }
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = true;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = true;

                    //if the TOTALFLAGS flag is off, then we're most likely in NAV mode
                    if ((hsibits & F4SharedMem.Headers.HsiBits.TotalFlags) != F4SharedMem.Headers.HsiBits.TotalFlags)
                    {
                        ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                    }
                    //if the TO/FROM flag is showing in shared memory, then we are most likely in TACAN mode (except in F4AF which always has the bit turned on)
                    else if (
                                (
                                    ((hsibits & F4SharedMem.Headers.HsiBits.ToTrue) == F4SharedMem.Headers.HsiBits.ToTrue)
                                        ||
                                    ((hsibits & F4SharedMem.Headers.HsiBits.FromTrue) == F4SharedMem.Headers.HsiBits.FromTrue)
                                )
                        )
                    {
                        if (!commandBarsOn) //better make sure we're not in any ILS mode too though
                        {
                            ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = true;
                        }
                    }

                    //if the glideslope or localizer flags on the ADI are turned on, then we must be in an ILS mode and therefore we 
                    //know we don't need to show the HSI TO/FROM flags.
                    if (
                        ((hsibits & F4SharedMem.Headers.HsiBits.ADI_GS) == F4SharedMem.Headers.HsiBits.ADI_GS)
                            ||
                        ((hsibits & F4SharedMem.Headers.HsiBits.ADI_LOC) == F4SharedMem.Headers.HsiBits.ADI_LOC)
                        )
                    {
                        ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                    }
                    if (commandBarsOn)
                    {
                        ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                        ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                    }

                    ((F16ADI)_adiRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ADI)_adiRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE;
                    ((F16ADI)_adiRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE;

                    ((F16ISIS)_isisRenderer).InstrumentState.ShowCommandBars = commandBarsOn;
                    ((F16ISIS)_isisRenderer).InstrumentState.GlideslopeDeviationDegrees = fromFalcon.AdiIlsVerPos / Common.Math.Constants.RADIANS_PER_DEGREE;
                    ((F16ISIS)_isisRenderer).InstrumentState.LocalizerDeviationDegrees = fromFalcon.AdiIlsHorPos / Common.Math.Constants.RADIANS_PER_DEGREE;
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
                            ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                            break;
                        case 1: //NavModes.Tcn:
                            ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = true;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                            ((F16ADI)_adiRenderer).InstrumentState.ShowCommandBars = false;
                            ((F16ISIS)_isisRenderer).InstrumentState.ShowCommandBars = false;
                            break;
                        case 2: //NavModes.Nav:
                            ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                            ((F16ADI)_adiRenderer).InstrumentState.ShowCommandBars = false;
                            ((F16ISIS)_isisRenderer).InstrumentState.ShowCommandBars = false;
                            break;
                        case 3: //NavModes.PlsNav:
                            ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowToFromFlag = false;
                            ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    ((F16EHSI)_ehsiRenderer).InstrumentState.InstrumentMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown;
                }
                if (((hsibits & HsiBits.HSI_OFF) == HsiBits.HSI_OFF))
                {
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DmeInvalidFlag = true;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DeviationInvalidFlag = false;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationDegrees = 0;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.BearingToBeaconDegrees = 0;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DmeInvalidFlag = true;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DeviationInvalidFlag = false;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.CourseDeviationLimitDegrees = 0;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.CourseDeviationDegrees = 0;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.BearingToBeaconDegrees = 0;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = 0;
                }
                else
                {
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationLimitDegrees = fromFalcon.deviationLimit;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationDegrees = fromFalcon.courseDeviation;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DesiredCourseDegrees = (int)fromFalcon.desiredCourse;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DesiredHeadingDegrees = (int)fromFalcon.desiredHeading;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.BearingToBeaconDegrees = fromFalcon.bearingToBeacon;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = fromFalcon.distanceToBeacon;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DmeInvalidFlag = ((hsibits & HsiBits.CourseWarning) == HsiBits.CourseWarning);
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DeviationInvalidFlag = ((hsibits & HsiBits.IlsWarning) == HsiBits.IlsWarning);
                    ((F16EHSI)_ehsiRenderer).InstrumentState.CourseDeviationLimitDegrees = fromFalcon.deviationLimit;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.CourseDeviationDegrees = fromFalcon.courseDeviation;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DesiredCourseDegrees = (int)fromFalcon.desiredCourse;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DesiredHeadingDegrees = (int)fromFalcon.desiredHeading;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.BearingToBeaconDegrees = fromFalcon.bearingToBeacon;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.DistanceToBeaconNauticalMiles = fromFalcon.distanceToBeacon;
                }


                {
                    //compute course deviation and TO/FROM
                    float deviationLimitDecimalDegrees = ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationLimitDegrees % 180;
                    float desiredCourseInDegrees = ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.DesiredCourseDegrees;
                    float courseDeviationDecimalDegrees = ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationDegrees;
                    float bearingToBeaconInDegrees = ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.BearingToBeaconDegrees;
                    float myCourseDeviationDecimalDegrees = Common.Math.Util.AngleDelta(desiredCourseInDegrees, bearingToBeaconInDegrees);
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
                    if (Math.Abs(courseDeviationDecimalDegrees) > deviationLimitDecimalDegrees) courseDeviationDecimalDegrees = Math.Sign(courseDeviationDecimalDegrees) * deviationLimitDecimalDegrees;

                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.ToFlag = toFlag;
                    ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.FromFlag = fromFlag;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.CourseDeviationDegrees = courseDeviationDecimalDegrees;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.ToFlag = toFlag;
                    ((F16EHSI)_ehsiRenderer).InstrumentState.FromFlag = fromFlag;
                }

                //**************************


                //*** UPDATE EHSI **********
                UpdateEHSIBrightnessLabelVisibility();
                //**************************


                //**  UPDATE HYDA/HYDB****
                float rpm = fromFalcon.rpm;
                bool mainGen = ((fromFalcon.lightBits3 & (int)LightBits3.MainGen) == (int)LightBits3.MainGen);
                bool stbyGen = ((fromFalcon.lightBits3 & (int)LightBits3.StbyGen) == (int)LightBits3.StbyGen);
                bool epuGen = ((fromFalcon.lightBits3 & (int)LightBits3.EpuGen) == (int)LightBits3.EpuGen);
                bool epuOn = ((fromFalcon.lightBits2 & (int)LightBits2.EPUOn) == (int)LightBits2.EPUOn);
                float epuFuel = fromFalcon.epuFuel;
                ((F16HydraulicPressureGauge)_hydARenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch = NonImplementedGaugeCalculations.HydA(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
                ((F16HydraulicPressureGauge)_hydBRenderer).InstrumentState.HydraulicPressurePoundsPerSquareInch = NonImplementedGaugeCalculations.HydB(rpm, mainGen, stbyGen, epuGen, epuOn, epuFuel);
                //**************************

                //**  UPDATE CABIN PRESSURE ALTITUDE INDICATOR****
                float z = fromFalcon.z;
                float origCabinAlt = ((F16CabinPressureAltitudeIndicator)_cabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet;
                bool pressurization = ((fromFalcon.lightBits & (int)LightBits.CabinPress) == (int)LightBits.CabinPress);
                ((F16CabinPressureAltitudeIndicator)_cabinPressRenderer).InstrumentState.CabinPressureAltitudeFeet = NonImplementedGaugeCalculations.CabinAlt(origCabinAlt, z, pressurization);
                //**************************

                //**  UPDATE ROLL TRIM INDICATOR****
                float rolltrim = fromFalcon.TrimRoll;
                ((F16RollTrimIndicator)_rollTrimRenderer).InstrumentState.RollTrimPercent = rolltrim * 2.0f * 100.0f;
                //**************************

                //**  UPDATE PITCH TRIM INDICATOR****
                float pitchTrim = fromFalcon.TrimPitch;
                ((F16PitchTrimIndicator)_pitchTrimRenderer).InstrumentState.PitchTrimPercent = pitchTrim * 2.0f * 100.0f;
                //**************************


                //**  UPDATE RWR ****
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.MagneticHeadingDegrees = (360 + (fromFalcon.yaw / Common.Math.Constants.RADIANS_PER_DEGREE)) % 360;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.RollDegrees = (float)((fromFalcon.roll / Common.Math.Constants.RADIANS_PER_DEGREE));
                int rwrObjectCount = fromFalcon.RwrObjectCount;
                if (fromFalcon.RWRsymbol != null)
                {
                    F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[] blips = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip[fromFalcon.RWRsymbol.Length];
                    ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Blips = blips;
                    //for (int i = 0; i < rwrObjectCount; i++)
                    if (fromFalcon.RWRsymbol != null)
                    {
                        for (int i = 0; i < fromFalcon.RWRsymbol.Length; i++)
                        {
                            F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip thisBlip = new F16AzimuthIndicator.F16AzimuthIndicatorInstrumentState.Blip();
                            if (i < rwrObjectCount) thisBlip.Visible = true;
                            if (fromFalcon.bearing != null)
                            {
                                thisBlip.BearingDegrees = (fromFalcon.bearing[i] / Common.Math.Constants.RADIANS_PER_DEGREE);
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
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Activity = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxAct) == (int)F4SharedMem.Headers.LightBits2.AuxAct);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.ChaffCount = (int)fromFalcon.ChaffCount;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ChaffLo) == (int)F4SharedMem.Headers.LightBits2.ChaffLo);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.EWSDegraded = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.EWSDispenseReady = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.EWSNoGo = (
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                                     ||
                            ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                        );
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.EWSGo =
                    (
                        ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Go) == (int)F4SharedMem.Headers.LightBits2.Go)
                                &&
                            !(
                                ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                                         ||
                                ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                                         ||
                                ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                            )
                    );


                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.FlareCount = (int)fromFalcon.FlareCount;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FlareLo) == (int)F4SharedMem.Headers.LightBits2.FlareLo);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Handoff = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.HandOff) == (int)F4SharedMem.Headers.LightBits2.HandOff);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Launch = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Launch) == (int)F4SharedMem.Headers.LightBits2.Launch);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.LowAltitudeMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxLow) == (int)F4SharedMem.Headers.LightBits2.AuxLow);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.NavalMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Naval) == (int)F4SharedMem.Headers.LightBits2.Naval);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Other1Count = 0;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Other1Low = true;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Other2Count = 0;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.Other2Low = true;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.RWRPowerOn = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxPwr) == (int)F4SharedMem.Headers.LightBits2.AuxPwr);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.PriorityMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.PriMode) == (int)F4SharedMem.Headers.LightBits2.PriMode);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.SearchMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AuxSrch) == (int)F4SharedMem.Headers.LightBits2.AuxSrch);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.SeparateMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TgtSep) == (int)F4SharedMem.Headers.LightBits2.TgtSep);
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.UnknownThreatScanMode = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Unk) == (int)F4SharedMem.Headers.LightBits2.Unk);
                //********************

                //** UPDATE CAUTION PANEL
                //TODO: implement all-lights-on when test is detected
                F16CautionPanel.F16CautionPanelInstrumentState myState = ((F16CautionPanel)_cautionPanelRenderer).InstrumentState;
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.AftFuelLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.AftFuelLow) == (int)F4SharedMem.Headers.LightBits2.AftFuelLow);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.AntiSkid = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ANTI_SKID) == (int)F4SharedMem.Headers.LightBits2.ANTI_SKID);
                //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ATFNotEngaged = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED) == (int)F4SharedMem.Headers.LightBits2.TFR_ENGAGED);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.AvionicsFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Avionics) == (int)F4SharedMem.Headers.LightBits.Avionics);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.BUC = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.BUC) == (int)F4SharedMem.Headers.LightBits2.BUC);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.CabinPress = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.CabinPress) == (int)F4SharedMem.Headers.LightBits.CabinPress);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.CADC = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.Bms4LightBits3.cadc) == (int)F4SharedMem.Headers.Bms4LightBits3.cadc);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ECM = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ECM) == (int)F4SharedMem.Headers.LightBits.ECM);
                //((F16CautionPanel)_cautionPanelRenderer).InstrumentState.EEC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.ee) == (int)F4SharedMem.Headers.LightBits.ECM);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ElecSys = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.Elec_Fault) == (int)F4SharedMem.Headers.LightBits3.Elec_Fault);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.EngineFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.EngineFault) == (int)F4SharedMem.Headers.LightBits.EngineFault);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.EquipHot = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.EQUIP_HOT) == (int)F4SharedMem.Headers.LightBits.EQUIP_HOT);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.FLCSFault = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.FltControlSys) == (int)F4SharedMem.Headers.LightBits.FltControlSys);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.FuelOilHot = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FUEL_OIL_HOT) == (int)F4SharedMem.Headers.LightBits2.FUEL_OIL_HOT);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.FwdFuelLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FwdFuelLow) == (int)F4SharedMem.Headers.LightBits2.FwdFuelLow);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.Hook = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Hook) == (int)F4SharedMem.Headers.LightBits.Hook);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.IFF = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.IFF) == (int)F4SharedMem.Headers.LightBits.IFF);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.NWSFail = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.NWSFail) == (int)F4SharedMem.Headers.LightBits.NWSFail);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.Overheat = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.Overheat) == (int)F4SharedMem.Headers.LightBits.Overheat);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.OxyLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.OXY_LOW) == (int)F4SharedMem.Headers.LightBits2.OXY_LOW);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.ProbeHeat = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.PROBEHEAT) == (int)F4SharedMem.Headers.LightBits2.PROBEHEAT);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.RadarAlt = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RadarAlt) == (int)F4SharedMem.Headers.LightBits.RadarAlt);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.SeatNotArmed = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.SEAT_ARM) == (int)F4SharedMem.Headers.LightBits2.SEAT_ARM);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.SEC = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.SEC) == (int)F4SharedMem.Headers.LightBits2.SEC);
                ((F16CautionPanel)_cautionPanelRenderer).InstrumentState.StoresConfig = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.CONFIG) == (int)F4SharedMem.Headers.LightBits.CONFIG);
                
                //TODO: implement MLU cautions

                //***********************

                //**  UPDATE CMDS PANEL
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Degraded = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr);
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.ChaffCount = (int)fromFalcon.ChaffCount;
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.ChaffLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.ChaffLo) == (int)F4SharedMem.Headers.LightBits2.ChaffLo);
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.DispenseReady = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy);
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.FlareCount = (int)fromFalcon.FlareCount;
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.FlareLow = ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.FlareLo) == (int)F4SharedMem.Headers.LightBits2.FlareLo);
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Go =
                    (
                        ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Go) == (int)F4SharedMem.Headers.LightBits2.Go) //Falcas 04/09/2012 to match what you see in BMS
                            //    &&
                            //!(
                            //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo)
                            //             ||
                            //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                            //             ||
                            //    ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Rdy) == (int)F4SharedMem.Headers.LightBits2.Rdy)
                            //)
                    );

                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.NoGo =
                    (
                        ((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.NoGo) == (int)F4SharedMem.Headers.LightBits2.NoGo) //Falcas 04/09/2012 to match what you see in BMS
                        //         ||
                        //((fromFalcon.lightBits2 & (int)F4SharedMem.Headers.LightBits2.Degr) == (int)F4SharedMem.Headers.LightBits2.Degr)
                    );
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Other1Count = 0;
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Other1Low = true;
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Other2Count = 0;
                ((F16CMDSPanel)_cmdsPanelRenderer).InstrumentState.Other2Low = true;
                //**********************

                //** UPDATE DED 
                if (fromFalcon.DEDLines != null)
                {
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line1 = Encoding.Default.GetBytes(fromFalcon.DEDLines[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line2 = Encoding.Default.GetBytes(fromFalcon.DEDLines[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line3 = Encoding.Default.GetBytes(fromFalcon.DEDLines[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line4 = Encoding.Default.GetBytes(fromFalcon.DEDLines[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line5 = Encoding.Default.GetBytes(fromFalcon.DEDLines[4] ?? "");
                }
                if (fromFalcon.Invert != null)
                {
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line1Invert = Encoding.Default.GetBytes(fromFalcon.Invert[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line2Invert = Encoding.Default.GetBytes(fromFalcon.Invert[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line3Invert = Encoding.Default.GetBytes(fromFalcon.Invert[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line4Invert = Encoding.Default.GetBytes(fromFalcon.Invert[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_dedRenderer).InstrumentState.Line5Invert = Encoding.Default.GetBytes(fromFalcon.Invert[4] ?? "");
                }
                //*************************


                //** UPDATE PFL
                if (fromFalcon.PFLLines != null)
                {
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line1 = Encoding.Default.GetBytes(fromFalcon.PFLLines[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line2 = Encoding.Default.GetBytes(fromFalcon.PFLLines[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line3 = Encoding.Default.GetBytes(fromFalcon.PFLLines[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line4 = Encoding.Default.GetBytes(fromFalcon.PFLLines[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line5 = Encoding.Default.GetBytes(fromFalcon.PFLLines[4] ?? "");
                }
                if (fromFalcon.PFLInvert != null)
                {
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line1Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[0] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line2Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[1] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line3Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[2] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line4Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[3] ?? "");
                    ((F16DataEntryDisplayPilotFaultList)_pflRenderer).InstrumentState.Line5Invert = Encoding.Default.GetBytes(fromFalcon.PFLInvert[4] ?? "");
                }
                //*************************

                //** UPDATE EPU FUEL
                ((F16EPUFuelGauge)_epuFuelRenderer).InstrumentState.FuelRemainingPercent = fromFalcon.epuFuel;
                //******************

                //** UPDATE FUEL FLOW
                ((F16FuelFlow)_fuelFlowRenderer).InstrumentState.FuelFlowPoundsPerHour = fromFalcon.fuelFlow;
                //******************

                //** UPDATE FUEL QTY
                ((F16FuelQuantityIndicator)_fuelQuantityRenderer).InstrumentState.AftLeftFuelQuantityPounds = fromFalcon.aft / 10.0f;
                ((F16FuelQuantityIndicator)_fuelQuantityRenderer).InstrumentState.ForeRightFuelQuantityPounds = fromFalcon.fwd / 10.0f;
                ((F16FuelQuantityIndicator)_fuelQuantityRenderer).InstrumentState.TotalFuelQuantityPounds = fromFalcon.total;
                //******************

                //** UPDATE LANDING GEAR LIGHTS

                if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon || fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.LeftGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.LeftGearDown) == (int)F4SharedMem.Headers.LightBits3.LeftGearDown);
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.NoseGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.NoseGearDown) == (int)F4SharedMem.Headers.LightBits3.NoseGearDown);
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.RightGearDown = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.RightGearDown) == (int)F4SharedMem.Headers.LightBits3.RightGearDown);
                }
                else 
                {
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.LeftGearDown = ((fromFalcon.LeftGearPos== 1.0f));
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.NoseGearDown = ((fromFalcon.NoseGearPos== 1.0f));
                    ((F16LandingGearWheelsLights)_landingGearLightsRenderer).InstrumentState.RightGearDown = ((fromFalcon.RightGearPos== 1.0f));
                }

                
                //******************

                //** UPDATE NWS
                ((F16NosewheelSteeringIndexer)_nwsIndexerRenderer).InstrumentState.DISC = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelDSC) == (int)F4SharedMem.Headers.LightBits.RefuelDSC);
                ((F16NosewheelSteeringIndexer)_nwsIndexerRenderer).InstrumentState.AR_NWS = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelAR) == (int)F4SharedMem.Headers.LightBits.RefuelAR);
                ((F16NosewheelSteeringIndexer)_nwsIndexerRenderer).InstrumentState.RDY = ((fromFalcon.lightBits & (int)F4SharedMem.Headers.LightBits.RefuelRDY) == (int)F4SharedMem.Headers.LightBits.RefuelRDY);
                //******************

                //** UPDATE SPEEDBRAKE
                ((F16SpeedbrakeIndicator)_speedbrakeRenderer).InstrumentState.PercentOpen = fromFalcon.speedBrake * 100.0f;

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    ((F16SpeedbrakeIndicator)_speedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.Bms4LightBits3.Power_Off) == (int)F4SharedMem.Headers.Bms4LightBits3.Power_Off);
                }
                else
                {
                    ((F16SpeedbrakeIndicator)_speedbrakeRenderer).InstrumentState.PowerLoss = ((fromFalcon.lightBits3 & (int)F4SharedMem.Headers.LightBits3.Power_Off) == (int)F4SharedMem.Headers.LightBits3.Power_Off);
                }
                //******************

                //** UPDATE RPM1
                ((F16Tachometer)_rpm1Renderer).InstrumentState.RPMPercent = fromFalcon.rpm;
                //******************

                //** UPDATE RPM2
                ((F16Tachometer)_rpm2Renderer).InstrumentState.RPMPercent = fromFalcon.rpm2;
                //******************

                if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    //Only BMS4 has a valid FTIT value in sharedmem
                    //** UPDATE FTIT1
                    ((F16FanTurbineInletTemperature)_ftit1Renderer).InstrumentState.InletTemperatureDegreesCelcius = fromFalcon.ftit * 100.0f;
                    //******************
                    //** UPDATE FTIT2
                    ((F16FanTurbineInletTemperature)_ftit2Renderer).InstrumentState.InletTemperatureDegreesCelcius = fromFalcon.ftit2 * 100.0f;
                    //******************
                }
                else
                {
                    //FTIT is hosed in AF, RedViper, FF5, OF
                    //** UPDATE FTIT1
                    ((F16FanTurbineInletTemperature)_ftit1Renderer).InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(((F16FanTurbineInletTemperature)_ftit1Renderer).InstrumentState.InletTemperatureDegreesCelcius, fromFalcon.rpm);
                    //******************
                    //** UPDATE FTIT2
                    ((F16FanTurbineInletTemperature)_ftit2Renderer).InstrumentState.InletTemperatureDegreesCelcius = NonImplementedGaugeCalculations.Ftit(((F16FanTurbineInletTemperature)_ftit2Renderer).InstrumentState.InletTemperatureDegreesCelcius, fromFalcon.rpm2);
                    //******************
                }

                if (fromFalcon.DataFormat == FalconDataFormats.OpenFalcon)
                {
                    //NOZ is hosed in OF
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator)_nozPos1Renderer).InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm, fromFalcon.z, fromFalcon.fuelFlow);
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator)_nozPos2Renderer).InstrumentState.NozzlePositionPercent = NonImplementedGaugeCalculations.NOZ(fromFalcon.rpm2, fromFalcon.z, fromFalcon.fuelFlow);
                    //******************
                }
                else if (fromFalcon.DataFormat == FalconDataFormats.BMS4)
                {
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator)_nozPos1Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos * 100.0f;
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator)_nozPos2Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos2 * 100.0f;
                    //******************
                }
                else
                {
                    //NOZ is OK in AF, RedViper, FF5
                    //** UPDATE NOZ1
                    ((F16NozzlePositionIndicator)_nozPos1Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos;
                    //******************
                    //** UPDATE NOZ2
                    ((F16NozzlePositionIndicator)_nozPos2Renderer).InstrumentState.NozzlePositionPercent = fromFalcon.nozzlePos2;
                    //******************
                }

                //** UPDATE OIL1
                ((F16OilPressureGauge)_oilGauge1Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure;
                //******************

                //** UPDATE OIL2
                ((F16OilPressureGauge)_oilGauge2Renderer).InstrumentState.OilPressurePercent = fromFalcon.oilPressure2;
                //******************

                //** UPDATE ACCELEROMETER
                float gs = fromFalcon.gs;
                if (gs == 0) //ignore exactly zero g's
                {
                    gs = 1;
                }
                ((F16Accelerometer)_accelerometerRenderer).InstrumentState.AccelerationInGs = gs;
                //******************


            }
            else //Falcon's not running
            {
                if (_vviRenderer is F16VerticalVelocityIndicatorEU)
                {
                    ((F16VerticalVelocityIndicatorEU)_vviRenderer).InstrumentState.OffFlag = true;
                }
                else if (_vviRenderer is F16VerticalVelocityIndicatorUSA)
                {
                    ((F16VerticalVelocityIndicatorUSA)_vviRenderer).InstrumentState.OffFlag = true;
                }
                ((F16AngleOfAttackIndicator)_aoaIndicatorRenderer).InstrumentState.OffFlag = true;
                ((F16HorizontalSituationIndicator)_hsiRenderer).InstrumentState.OffFlag = true;
                ((F16EHSI)_ehsiRenderer).InstrumentState.NoDataFlag = true;
                ((F16ADI)_adiRenderer).InstrumentState.OffFlag = true;
                ((F16StandbyADI)_backupAdiRenderer).InstrumentState.OffFlag = true;
                ((F16AzimuthIndicator)_rwrRenderer).InstrumentState.RWRPowerOn = false;
                ((F16ISIS)_isisRenderer).InstrumentState.RadarAltitudeAGL = 0;
                ((F16ISIS)_isisRenderer).InstrumentState.OffFlag = true;
                UpdateEHSIBrightnessLabelVisibility();
            }

        }
        private float GetIndicatedAltitude(float trueAltitude, float baroPressure, bool pressureInInchesOfMercury)
        {
            float baroPressureInchesOfMercury = baroPressure;
            if (!pressureInInchesOfMercury)
            {
                baroPressureInchesOfMercury = baroPressure / Common.Math.Constants.INCHES_MERCURY_TO_HECTOPASCALS; 
            }
            float baroDifference = baroPressureInchesOfMercury - 29.92f;
            float baroChangePerThousandFeet = 1.08f;
            float altitudeCorrection= (baroDifference / baroChangePerThousandFeet) * 1000.0f;
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
                        showBrightnessLabel = ((F16EHSI)_ehsiRenderer).InstrumentState.ShowBrightnessLabel;
                    }
                }
            }
            ((F16EHSI)_ehsiRenderer).InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
        }
        private bool IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(IInstrumentRenderer renderer)
        {
            int staleDataTimeout = 500;//Timeout.Infinite;
            InstrumentRendererBase baseRenderer = renderer as InstrumentRendererBase;
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
                timeSinceHashChanged = (int)Math.Floor(DateTime.Now.Subtract(oldStateDateTime).TotalMilliseconds);
            }
            bool stateIsStaleOrChanged = (hashesAreDifferent || (timeSinceHashChanged > staleDataTimeout && staleDataTimeout != Timeout.Infinite));
            if (stateIsStaleOrChanged)
            {
                InstrumentStateSnapshot toStore = new InstrumentStateSnapshot() { DateTime = newStateDateTime, HashCode = newStateHash };
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

        private void ADIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _adiRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _adiRenderer,
                        _adiForm,
                        Properties.Settings.Default.ADI_RotateFlipType,
                        Properties.Settings.Default.ADI_Monochrome);
                    if (_adiCounter != null)
                    {
                        _adiCounter.Increment();
                    }
                    _adiRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void BackupADIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _backupAdiRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _backupAdiRenderer,
                        _backupAdiForm,
                        Properties.Settings.Default.Backup_ADI_RotateFlipType,
                        Properties.Settings.Default.Backup_ADI_Monochrome);
                    if (_backupAdiCounter != null)
                    {
                        _backupAdiCounter.Increment();
                    }
                    _backupAdiRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void ASIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _asiRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _asiRenderer,
                        _asiForm,
                        Properties.Settings.Default.ASI_RotateFlipType,
                        Properties.Settings.Default.ASI_Monochrome);
                    if (_asiCounter != null)
                    {
                        _asiCounter.Increment();
                    }
                    _asiRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void HYDARenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _hydARenderStart.WaitOne();
                    RenderInstrumentImage(
                        _hydARenderer,
                        _hydAForm,
                        Properties.Settings.Default.HYDA_RotateFlipType,
                        Properties.Settings.Default.HYDA_Monochrome);
                    if (_hydACounter != null)
                    {
                        _hydACounter.Increment();
                    }
                    _hydARenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void HYDBRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _hydBRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _hydBRenderer,
                        _hydBForm,
                        Properties.Settings.Default.HYDB_RotateFlipType,
                        Properties.Settings.Default.HYDB_Monochrome);
                    if (_hydBCounter != null)
                    {
                        _hydBCounter.Increment();
                    }
                    _hydBRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void CabinPressRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _cabinPressRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _cabinPressRenderer,
                        _cabinPressForm,
                        Properties.Settings.Default.CabinPress_RotateFlipType,
                        Properties.Settings.Default.CabinPress_Monochrome);
                    if (_cabinPressCounter != null)
                    {
                        _cabinPressCounter.Increment();
                    }
                    _cabinPressRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }


        private void RollTrimRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _rollTrimRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _rollTrimRenderer,
                        _rollTrimForm,
                        Properties.Settings.Default.RollTrim_RotateFlipType,
                        Properties.Settings.Default.RollTrim_Monochrome);
                    if (_rollTrimCounter != null)
                    {
                        _rollTrimCounter.Increment();
                    }
                    _rollTrimRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void PitchTrimRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _pitchTrimRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _pitchTrimRenderer,
                        _pitchTrimForm,
                        Properties.Settings.Default.PitchTrim_RotateFlipType,
                        Properties.Settings.Default.PitchTrim_Monochrome);
                    if (_pitchTrimCounter != null)
                    {
                        _pitchTrimCounter.Increment();
                    }
                    _pitchTrimRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void AltimeterRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _altimeterRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _altimeterRenderer,
                        _altimeterForm,
                        Properties.Settings.Default.Altimeter_RotateFlipType,
                        Properties.Settings.Default.Altimeter_Monochrome);
                    if (_altimeterCounter != null)
                    {
                        _altimeterCounter.Increment();
                    }
                    _altimeterRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void AOAIndexerRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _aoaIndexerRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _aoaIndexerRenderer,
                        _aoaIndexerForm,
                        Properties.Settings.Default.AOAIndexer_RotateFlipType,
                        Properties.Settings.Default.AOAIndexer_Monochrome);
                    if (_aoaIndexerCounter != null)
                    {
                        _aoaIndexerCounter.Increment();
                    }
                    _aoaIndexerRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void AOAIndicatorRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _aoaIndicatorRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _aoaIndicatorRenderer,
                        _aoaIndicatorForm,
                        Properties.Settings.Default.AOAIndicator_RotateFlipType,
                        Properties.Settings.Default.AOAIndicator_Monochrome);
                    if (_aoaIndicatorCounter != null)
                    {
                        _aoaIndicatorCounter.Increment();
                    }
                    _aoaIndicatorRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void CautionPanelRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _cautionPanelRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _cautionPanelRenderer,
                        _cautionPanelForm,
                        Properties.Settings.Default.CautionPanel_RotateFlipType,
                        Properties.Settings.Default.CautionPanel_Monochrome);
                    if (_cautionPanelCounter != null)
                    {
                        _cautionPanelCounter.Increment();
                    }
                    _cautionPanelRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void CMDSPanelRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _cmdsPanelRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _cmdsPanelRenderer,
                        _cmdsPanelForm,
                        Properties.Settings.Default.CMDS_RotateFlipType,
                        Properties.Settings.Default.CMDS_Monochrome);
                    if (_cmdsPanelCounter != null)
                    {
                        _cmdsPanelCounter.Increment();
                    }
                    _cmdsPanelRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void CompassRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _compassRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _compassRenderer,
                        _compassForm,
                        Properties.Settings.Default.Compass_RotateFlipType,
                        Properties.Settings.Default.Compass_Monochrome);
                    if (_compassCounter != null)
                    {
                        _compassCounter.Increment();
                    }
                    _compassRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void DEDRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _dedRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _dedRenderer,
                        _dedForm,
                        Properties.Settings.Default.DED_RotateFlipType,
                        Properties.Settings.Default.DED_Monochrome);
                    if (_dedCounter != null)
                    {
                        _dedCounter.Increment();
                    }
                    _dedRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void PFLRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _pflRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _pflRenderer,
                        _pflForm,
                        Properties.Settings.Default.PFL_RotateFlipType,
                        Properties.Settings.Default.PFL_Monochrome);
                    if (_pflCounter != null)
                    {
                        _pflCounter.Increment();
                    }
                    _pflRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void EPUFuelRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _epuFuelRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _epuFuelRenderer,
                        _epuFuelForm,
                        Properties.Settings.Default.EPUFuel_RotateFlipType,
                        Properties.Settings.Default.EPUFuel_Monochrome);
                    if (_epuFuelCounter != null)
                    {
                        _epuFuelCounter.Increment();
                    }
                    _epuFuelRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void AccelerometerRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _accelerometerRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _accelerometerRenderer,
                        _accelerometerForm,
                        Properties.Settings.Default.Accelerometer_RotateFlipType,
                        Properties.Settings.Default.Accelerometer_Monochrome);
                    if (_accelerometerCounter != null)
                    {
                        _accelerometerCounter.Increment();
                    }
                    _accelerometerRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void FTIT1RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _ftit1RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _ftit1Renderer,
                        _ftit1Form,
                        Properties.Settings.Default.FTIT1_RotateFlipType,
                        Properties.Settings.Default.FTIT1_Monochrome);
                    if (_ftit1Counter != null)
                    {
                        _ftit1Counter.Increment();
                    }
                    _ftit1RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void FTIT2RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _ftit2RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _ftit2Renderer,
                        _ftit2Form,
                        Properties.Settings.Default.FTIT2_RotateFlipType,
                        Properties.Settings.Default.FTIT2_Monochrome);
                    if (_ftit2Counter != null)
                    {
                        _ftit2Counter.Increment();
                    }
                    _ftit2RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void FuelFlowRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _fuelFlowRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _fuelFlowRenderer,
                        _fuelFlowForm,
                        Properties.Settings.Default.FuelFlow_RotateFlipType,
                        Properties.Settings.Default.FuelFlow_Monochrome);
                    if (_fuelFlowCounter != null)
                    {
                        _fuelFlowCounter.Increment();
                    }
                    _fuelFlowRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void ISISRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _isisRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _isisRenderer,
                        _isisForm,
                        Properties.Settings.Default.ISIS_RotateFlipType,
                        Properties.Settings.Default.ISIS_Monochrome);
                    if (_isisCounter != null)
                    {
                        _isisCounter.Increment();
                    }
                    _isisRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void FuelQuantityRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _fuelQuantityRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _fuelQuantityRenderer,
                        _fuelQuantityForm,
                        Properties.Settings.Default.FuelQuantity_RotateFlipType,
                        Properties.Settings.Default.FuelQuantity_Monochrome);
                    if (_fuelQuantityCounter != null)
                    {
                        _fuelQuantityCounter.Increment();
                    }
                    _fuelQuantityRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void HSIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _hsiRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _hsiRenderer,
                        _hsiForm,
                        Properties.Settings.Default.HSI_RotateFlipType,
                        Properties.Settings.Default.HSI_Monochrome);
                    if (_hsiCounter != null)
                    {
                        _hsiCounter.Increment();
                    }
                    _hsiRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void EHSIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _ehsiRenderStart.WaitOne();

                    RenderInstrumentImage(
                        _ehsiRenderer,
                        _ehsiForm,
                        Properties.Settings.Default.EHSI_RotateFlipType,
                        Properties.Settings.Default.EHSI_Monochrome);
                    if (_ehsiCounter != null)
                    {
                        _ehsiCounter.Increment();
                    }
                    _ehsiRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void LandingGearLightsRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _landingGearLightsRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _landingGearLightsRenderer,
                        _landingGearLightsForm,
                        Properties.Settings.Default.GearLights_RotateFlipType,
                        Properties.Settings.Default.GearLights_Monochrome);
                    if (_landingGearLightsCounter != null)
                    {
                        _landingGearLightsCounter.Increment();
                    }
                    _landingGearLightsRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void NWSIndexerRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _nwsIndexerRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _nwsIndexerRenderer,
                        _nwsIndexerForm,
                        Properties.Settings.Default.NWSIndexer_RotateFlipType,
                        Properties.Settings.Default.NWSIndexer_Monochrome);
                    if (_nwsIndexerCounter != null)
                    {
                        _nwsIndexerCounter.Increment();
                    }
                    _nwsIndexerRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void NOZPos1RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _nozPos1RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _nozPos1Renderer,
                        _nozPos1Form,
                        Properties.Settings.Default.NOZ1_RotateFlipType,
                        Properties.Settings.Default.NOZ1_Monochrome);
                    if (_nozPos1Counter != null)
                    {
                        _nozPos1Counter.Increment();
                    }
                    _nozPos1RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void NOZPos2RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _nozPos2RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _nozPos2Renderer,
                        _nozPos2Form,
                        Properties.Settings.Default.NOZ2_RotateFlipType,
                        Properties.Settings.Default.NOZ2_Monochrome);
                    if (_nozPos2Counter != null)
                    {
                        _nozPos2Counter.Increment();
                    }
                    _nozPos2RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void OilGauge1RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _oilGauge1RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _oilGauge1Renderer,
                        _oilGauge1Form,
                        Properties.Settings.Default.OIL1_RotateFlipType,
                       Properties.Settings.Default.OIL1_Monochrome);
                    if (_oilGauge1Counter != null)
                    {
                        _oilGauge1Counter.Increment();
                    }
                    _oilGauge1RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void OilGauge2RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _oilGauge2RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _oilGauge2Renderer,
                        _oilGauge2Form,
                        Properties.Settings.Default.OIL2_RotateFlipType,
                        Properties.Settings.Default.OIL2_Monochrome);
                    if (_oilGauge2Counter != null)
                    {
                        _oilGauge2Counter.Increment();
                    }
                    _oilGauge2RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void RWRRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _rwrRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _rwrRenderer,
                        _rwrForm,
                        Properties.Settings.Default.RWR_RotateFlipType,
                        Properties.Settings.Default.RWR_Monochrome);
                    if (_rwrCounter != null)
                    {
                        _rwrCounter.Increment();
                    }
                    _rwrRenderEnd.Set();

                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void SpeedbrakeRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _speedbrakeRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _speedbrakeRenderer,
                        _speedbrakeForm,
                        Properties.Settings.Default.Speedbrake_RotateFlipType,
                        Properties.Settings.Default.Speedbrake_Monochrome);
                    if (_speedbrakeCounter != null)
                    {
                        _speedbrakeCounter.Increment();
                    }
                    _speedbrakeRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void RPM1RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _rpm1RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _rpm1Renderer,
                        _rpm1Form,
                        Properties.Settings.Default.RPM1_RotateFlipType,
                        Properties.Settings.Default.RPM1_Monochrome);
                    if (_rpm1Counter != null)
                    {
                        _rpm1Counter.Increment();
                    }
                    _rpm1RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void RPM2RenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _rpm2RenderStart.WaitOne();
                    RenderInstrumentImage(
                        _rpm2Renderer,
                        _rpm2Form,
                        Properties.Settings.Default.RPM2_RotateFlipType,
                        Properties.Settings.Default.RPM2_Monochrome);
                    if (_rpm2Counter != null)
                    {
                        _rpm2Counter.Increment();
                    }
                    _rpm2RenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void VVIRenderThreadWork()
        {
            try
            {
                while (_keepRunning)
                {
                    _vviRenderStart.WaitOne();
                    RenderInstrumentImage(
                        _vviRenderer,
                        _vviForm,
                        Properties.Settings.Default.VVI_RotateFlipType,
                        Properties.Settings.Default.VVI_Monochrome);
                    if (_vviCounter != null)
                    {
                        _vviCounter.Increment();
                    }
                    _vviRenderEnd.Set();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }
        private void RenderInstrumentImage(IInstrumentRenderer renderer, InstrumentForm targetForm, RotateFlipType rotation, bool monochrome)
        {
            DateTime startTime = DateTime.Now;
            if (renderer == null || targetForm == null) return;
            if (DateTime.Now.Subtract(targetForm.LastRenderedOn).TotalMilliseconds < Properties.Settings.Default.PollingDelay)
            {
                return;
            }
            Bitmap image = null;
            try
            {
                if (targetForm.Rotation.ToString().Contains("90") || targetForm.Rotation.ToString().Contains("270"))
                {
                    image = new Bitmap(targetForm.ClientRectangle.Height, targetForm.ClientRectangle.Width, PixelFormat.Format32bppPArgb);
                }
                else
                {
                    image = new Bitmap(targetForm.ClientRectangle.Width, targetForm.ClientRectangle.Height, PixelFormat.Format32bppPArgb);
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
                            Pen scopeGreenPen = new Pen(scopeGreenColor);
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
                            _log.Error("An error occurred while rendering " + renderer.GetType().ToString(), e);
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
                    if (this.NightMode)
                    {
                        ImageAttributes nvisImageAttribs = new ImageAttributes();
                        ColorMatrix cm = Common.Imaging.Util.GetNVISColorMatrix(255, 255);
                        nvisImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                        graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, nvisImageAttribs);
                    }
                    else if (monochrome)
                    {
                        ImageAttributes monochromeImageAttribs = new ImageAttributes();
                        ColorMatrix cm = GetGreyscaleColorMatrix();
                        monochromeImageAttribs.SetColorMatrix(cm, ColorMatrixFlag.Default);
                        graphics.DrawImage(image, targetForm.ClientRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, monochromeImageAttribs);
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
                TimeSpan toWait = new TimeSpan(0, 0, 0, 0, (int)(MIN_RENDERER_PASS_TIME_MILLSECONDS - elapsed.TotalMilliseconds));
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
                Properties.Settings.Default.HighlightOutputWindows
            ;
        }

        private bool WindowSizingOrMovingBeingAttemptedOnAnyOutputWindow()
        {
            bool retVal = false;
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    InstrumentForm iForm = form as InstrumentForm;
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
            catch (InvalidOperationException e) //if the OpenForms collection is modified during our loop (by the application shutting down, etc), we need to swallow this here
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
        private void KeyboardWatcherThreadWork()
        {
            AutoResetEvent resetEvent = null;
            Microsoft.DirectX.DirectInput.Device device = null;
            try
            {
                resetEvent = new AutoResetEvent(false);
                device = new Microsoft.DirectX.DirectInput.Device(Microsoft.DirectX.DirectInput.SystemGuid.Keyboard);
                device.SetCooperativeLevel(null, Microsoft.DirectX.DirectInput.CooperativeLevelFlags.Background | Microsoft.DirectX.DirectInput.CooperativeLevelFlags.NonExclusive);
                device.SetEventNotification(resetEvent);
                device.Properties.BufferSize = 255;
                device.Acquire();
                bool[] lastKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
                bool[] currentKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
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
                        Array possibleKeys = Enum.GetValues(typeof(Key));

                        int i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            currentKeyboardState[i] = curState[thisKey];
                            i++;
                        }

                        i=0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            bool isPressedNow = currentKeyboardState[i];
                            bool wasPressedBefore = lastKeyboardState[i];
                            Keys winFormsKey = (Keys)Common.Win32.NativeMethods.MapVirtualKey((uint)thisKey, Common.Win32.NativeMethods.MAPVK_VSC_TO_VK_EX);
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
                _log.Error(e.Message.ToString(), e);
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

        /// <summary>
        /// Worker thread for coordinating the image capturing sequence
        /// </summary>
        private void CaptureOrchestrationThreadWork()
        {
            List<WaitHandle> toWait = new List<WaitHandle>();
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
                            _log.Error(e.Message.ToString(), e);
                        }
                    }
                    else
                    {
                        FlightData toSet = new FlightData();
                        toSet.hsiBits = Int32.MaxValue;
                        SetFlightData(toSet);
                        UpdateRendererStatesFromFlightData();
                        SetMfd4Image(Common.Imaging.Util.CloneBitmap(_mfd4BlankImage));
                        SetMfd3Image(Common.Imaging.Util.CloneBitmap(_mfd3BlankImage));
                        SetLeftMfdImage(Common.Imaging.Util.CloneBitmap(_leftMfdBlankImage));
                        SetRightMfdImage(Common.Imaging.Util.CloneBitmap(_rightMfdBlankImage));
                        SetHudImage(Common.Imaging.Util.CloneBitmap(_hudBlankImage));
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
                        _log.Error(e.Message.ToString(), e);
                    }

                    DateTime thisLoopFinishTime = DateTime.Now;
                    TimeSpan timeElapsed = thisLoopFinishTime.Subtract(thisLoopStartTime);
                    int millisToSleep = Properties.Settings.Default.PollingDelay - ((int)timeElapsed.TotalMilliseconds);
                    if (_testMode) millisToSleep = 500;
                    DateTime sleepUntil = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, millisToSleep));
                    while (DateTime.Now < sleepUntil)
                    {
                        int millisRemaining = (int)Math.Floor(DateTime.Now.Subtract(sleepUntil).TotalMilliseconds);
                        int millisWaited = millisRemaining >= 5 ? 5 : 1;
                        Thread.Sleep(millisWaited);
                        Application.DoEvents();
                    }
                    Application.DoEvents();
                    if ((!_simRunning && !(_networkMode == NetworkMode.Client)) && !_testMode)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5); //sleep an additional half-second or so here if we're not a client and there's no sim running and we're not in test mode
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

            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableRWROutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rwrRenderer)) || (_rwrForm !=null && _rwrForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.RWR_RenderEveryN == Properties.Settings.Default.RWR_RenderOnN - 1) || (_rwrForm !=null && _rwrForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableCMDSOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cmdsPanelRenderer)) || (_cmdsPanelForm !=null && _cmdsPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.CMDS_RenderEveryN == Properties.Settings.Default.CMDS_RenderOnN - 1) || (_cmdsPanelForm !=null && _cmdsPanelForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableCautionPanelOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cautionPanelRenderer)) || (_cautionPanelForm !=null && _cautionPanelForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.CautionPanel_RenderEveryN == Properties.Settings.Default.CautionPanel_RenderOnN - 1) || (_cautionPanelForm !=null && _cautionPanelForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableGearLightsOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_landingGearLightsRenderer)) || (_landingGearLightsForm !=null && _landingGearLightsForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.GearLights_RenderEveryN == Properties.Settings.Default.GearLights_RenderOnN - 1) || (_landingGearLightsForm !=null && _landingGearLightsForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableSpeedbrakeOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_speedbrakeRenderer)) || (_speedbrakeForm !=null && _speedbrakeForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.Speedbrake_RenderEveryN == Properties.Settings.Default.Speedbrake_RenderOnN - 1) || (_speedbrakeForm !=null && _speedbrakeForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableDEDOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_dedRenderer) || (_dedForm !=null && _dedForm.RenderImmediately)))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.DED_RenderEveryN == Properties.Settings.Default.DED_RenderOnN - 1) || (_dedForm !=null && _dedForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnablePFLOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_pflRenderer)) || (_pflForm !=null && _pflForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.PFL_RenderEveryN == Properties.Settings.Default.PFL_RenderOnN - 1) || (_pflForm !=null && _pflForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableFuelFlowOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_fuelFlowRenderer)) || (_fuelFlowForm !=null && _fuelFlowForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.FuelFlow_RenderEveryN == Properties.Settings.Default.FuelFlow_RenderOnN - 1) || (_fuelFlowForm !=null && _fuelFlowForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableFuelQuantityOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_fuelQuantityRenderer)) || (_fuelQuantityForm !=null && _fuelQuantityForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.FuelQuantity_RenderEveryN == Properties.Settings.Default.FuelQuantity_RenderOnN - 1) || (_fuelQuantityForm !=null && _fuelQuantityForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableEPUFuelOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_epuFuelRenderer)) || (_epuFuelForm !=null && _epuFuelForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.EPUFuel_RenderEveryN == Properties.Settings.Default.EPUFuel_RenderOnN - 1) || (_epuFuelForm !=null && _epuFuelForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableAOAIndexerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_aoaIndexerRenderer)) || (_aoaIndexerForm !=null && _aoaIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.AOAIndexer_RenderEveryN == Properties.Settings.Default.AOAIndexer_RenderOnN - 1) || (_aoaIndexerForm !=null && _aoaIndexerForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableNWSIndexerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nwsIndexerRenderer)) || (_nwsIndexerForm !=null && _nwsIndexerForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.NWSIndexer_RenderEveryN == Properties.Settings.Default.NWSIndexer_RenderOnN - 1) || (_nwsIndexerForm !=null && _nwsIndexerForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableFTIT1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ftit1Renderer)) || (_ftit1Form !=null && _ftit1Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.FTIT1_RenderEveryN == Properties.Settings.Default.FTIT1_RenderOnN - 1) || (_ftit1Form !=null && _ftit1Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableNOZ1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nozPos1Renderer)) || (_nozPos1Form !=null && _nozPos1Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.NOZ1_RenderEveryN == Properties.Settings.Default.NOZ1_RenderOnN - 1) || (_nozPos1Form !=null && _nozPos1Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableOIL1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_oilGauge1Renderer)) || (_oilGauge1Form !=null && _oilGauge1Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.OIL1_RenderEveryN == Properties.Settings.Default.OIL1_RenderOnN - 1) || (_oilGauge1Form !=null && _oilGauge1Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableRPM1Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rpm1Renderer)) || (_rpm1Form !=null && _rpm1Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.RPM1_RenderEveryN == Properties.Settings.Default.RPM1_RenderOnN - 1) || (_rpm1Form !=null && _rpm1Form.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableFTIT2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ftit2Renderer)) || (_ftit2Form !=null && _ftit2Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.FTIT2_RenderEveryN == Properties.Settings.Default.FTIT2_RenderOnN - 1) || (_ftit2Form !=null && _ftit2Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableNOZ2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_nozPos2Renderer)) || (_nozPos2Form !=null && _nozPos2Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.NOZ2_RenderEveryN == Properties.Settings.Default.NOZ2_RenderOnN - 1) || (_nozPos2Form !=null && _nozPos2Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableOIL2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_oilGauge2Renderer)) || (_oilGauge2Form !=null && _oilGauge2Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.OIL2_RenderEveryN == Properties.Settings.Default.OIL2_RenderOnN - 1) || (_oilGauge2Form !=null && _oilGauge2Form.RenderImmediately))
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
            if (Properties.Settings.Default.EnableRPM2Output)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rpm2Renderer)) || (_rpm2Form !=null && _rpm2Form.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.RPM2_RenderEveryN == Properties.Settings.Default.RPM2_RenderOnN - 1) || (_rpm2Form !=null && _rpm2Form.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableHYDAOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hydARenderer)) || (_hydAForm !=null && _hydAForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.HYDA_RenderEveryN == Properties.Settings.Default.HYDA_RenderOnN - 1) || (_hydAForm !=null && _hydAForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableHYDBOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hydBRenderer)) || (_hydBForm !=null && _hydBForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.HYDB_RenderEveryN == Properties.Settings.Default.HYDB_RenderOnN - 1) || (_hydBForm !=null && _hydBForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableCabinPressOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_cabinPressRenderer)) || (_cabinPressForm !=null && _cabinPressForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.CabinPress_RenderEveryN == Properties.Settings.Default.CabinPress_RenderOnN - 1) || (_cabinPressForm !=null && _cabinPressForm.RenderImmediately))
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
            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableRollTrimOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_rollTrimRenderer)) || (_rollTrimForm !=null && _rollTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.RollTrim_RenderEveryN == Properties.Settings.Default.RollTrim_RenderOnN - 1) || (_rollTrimForm !=null && _rollTrimForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnablePitchTrimOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_pitchTrimRenderer)) || (_pitchTrimForm !=null && _pitchTrimForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.PitchTrim_RenderEveryN == Properties.Settings.Default.PitchTrim_RenderOnN - 1) || (_pitchTrimForm !=null && _pitchTrimForm.RenderImmediately))
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

            bool renderOnlyOnStateChanges = Properties.Settings.Default.RenderInstrumentsOnlyOnStatechanges;
            if (Properties.Settings.Default.EnableADIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_adiRenderer) || (_adiForm !=null && _adiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.ADI_RenderEveryN == Properties.Settings.Default.ADI_RenderOnN - 1) || (_adiForm !=null && _adiForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableISISOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_isisRenderer)) || (_isisForm !=null && _isisForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.ISIS_RenderEveryN == Properties.Settings.Default.ISIS_RenderOnN - 1) || (_isisForm !=null && _isisForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableHSIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_hsiRenderer) || (_hsiForm !=null && _hsiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.HSI_RenderEveryN == Properties.Settings.Default.HSI_RenderOnN - 1) || (_hsiForm !=null && _hsiForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableEHSIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_ehsiRenderer)) || (_ehsiForm !=null && _ehsiForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.EHSI_RenderEveryN == Properties.Settings.Default.EHSI_RenderOnN - 1) || (_ehsiForm !=null && _ehsiForm.RenderImmediately)) 
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
            if (Properties.Settings.Default.EnableAltimeterOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_altimeterRenderer)) || (_altimeterForm !=null && _altimeterForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.Altimeter_RenderEveryN == Properties.Settings.Default.Altimeter_RenderOnN - 1) || (_altimeterForm !=null && _altimeterForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableASIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_asiRenderer) || (_asiForm !=null && _asiForm.RenderImmediately)))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.ASI_RenderEveryN == Properties.Settings.Default.ASI_RenderOnN - 1) || (_asiForm !=null && _asiForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableBackupADIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_backupAdiRenderer)) || (_backupAdiForm !=null && _backupAdiForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.Backup_ADI_RenderEveryN == Properties.Settings.Default.Backup_ADI_RenderOnN - 1) || (_backupAdiForm !=null && _backupAdiForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableVVIOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_vviRenderer) || (_vviForm !=null && _vviForm.RenderImmediately)))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.VVI_RenderEveryN == Properties.Settings.Default.VVI_RenderOnN - 1) || (_vviForm !=null && _vviForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableAOAIndicatorOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_aoaIndicatorRenderer)) || (_aoaIndicatorForm !=null && _aoaIndicatorForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.AOAIndicator_RenderEveryN == Properties.Settings.Default.AOAIndicator_RenderOnN - 1) || (_aoaIndicatorForm !=null && _aoaIndicatorForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableCompassOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_compassRenderer)) || (_compassForm !=null && _compassForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.Compass_RenderEveryN == Properties.Settings.Default.Compass_RenderOnN - 1) || (_compassForm !=null && _compassForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableAccelerometerOutput)
            {
                if (_testMode || !renderOnlyOnStateChanges || (renderOnlyOnStateChanges && IsInstrumentStateStaleOrChangedOrIsInstrumentWindowHighlighted(_accelerometerRenderer)) || (_accelerometerForm !=null && _accelerometerForm.RenderImmediately))
                {
                    if ((_renderCycleNum % Properties.Settings.Default.Accelerometer_RenderEveryN == Properties.Settings.Default.Accelerometer_RenderOnN - 1) || (_accelerometerForm !=null && _accelerometerForm.RenderImmediately))
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
            if (Properties.Settings.Default.EnableMfd4Output || _networkMode == NetworkMode.Server)
            {
                _mfd4CaptureStart.Set();
            }
            if (Properties.Settings.Default.EnableMfd3Output || _networkMode == NetworkMode.Server)
            {
                _mfd3CaptureStart.Set();
            }
            if (Properties.Settings.Default.EnableLeftMFDOutput || _networkMode == NetworkMode.Server)
            {
                _leftMfdCaptureStart.Set();
            }
            if (Properties.Settings.Default.EnableRightMFDOutput || _networkMode == NetworkMode.Server)
            {
                _rightMfdCaptureStart.Set();
            }
            if (Properties.Settings.Default.EnableHudOutput || _networkMode == NetworkMode.Server)
            {
                _hudCaptureStart.Set();
            }
        }
        /// <summary>
        /// Worker thread method for monitoring whether the sim is running
        /// </summary>
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
                        if (count % 1 == 0)
                        {
                            count = 0;
                            Common.Util.DisposeObject(_texSmStatusReader);
                            _texSmStatusReader = new F4TexSharedMem.Reader();

#if SIMRUNNING
                            _simRunning = true;
#else
                            try
                            {
                                _simRunning = NetworkMode == NetworkMode.Client || F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex.Message, ex);
                            }
#endif
                            _sim3DDataAvailable = _simRunning && (NetworkMode == NetworkMode.Client || _texSmStatusReader.IsDataAvailable);

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
                                        if ((Properties.Settings.Default.EnableLeftMFDOutput || 
                                            Properties.Settings.Default.EnableRightMFDOutput ||
                                            Properties.Settings.Default.EnableMfd3Output ||
                                            Properties.Settings.Default.EnableMfd4Output || 
                                            Properties.Settings.Default.EnableHudOutput || 
                                            NetworkMode == NetworkMode.Server))
                                        {
                                            if ((_hud3DInputRect == Rectangle.Empty) || (_leftMfd3DInputRect == Rectangle.Empty) || (_rightMfd3DInputRect == Rectangle.Empty) || (_mfd3_3DInputRect == Rectangle.Empty) || (_mfd3_3DInputRect == Rectangle.Empty))
                                            {
                                                Read3DCoordinatesFromCurrentBmsDatFile(ref _mfd4_3DInputRect, ref _mfd3_3DInputRect, ref _leftMfd3DInputRect, ref _rightMfd3DInputRect, ref _hud3DInputRect);
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
        /// <summary>
        /// Wait for a thread to end, then abort the thread
        /// </summary>
        /// <param name="t">thread to wait for end/abort</param>
        /// <param name="timeout">amount of time to wait before forcefully aborting the thread</param>
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
                        AbortThread(ref t);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        #endregion

        #endregion

        #region BMS 2.0/OpenFalcon support functions
        private static void Read3DCoordinatesFromCurrentBmsDatFile(ref Rectangle mfd4_3DImageSourceRectangle, ref Rectangle mfd3_3DImageSourceRectangle, ref Rectangle leftMfd3DImageSourceRectangle, ref Rectangle rightMfd3DImageSourceRectangle, ref Rectangle hud3DImageSourceRectangle)
        {
            FileInfo file = FindBms3DCockpitFile();
            if (file == null)
            {
                return;
            }
            bool isDoubleResolution = IsDoubleResolutionRtt();
            mfd4_3DImageSourceRectangle = new Rectangle();
            mfd3_3DImageSourceRectangle = new Rectangle();
            leftMfd3DImageSourceRectangle = new Rectangle();
            rightMfd3DImageSourceRectangle = new Rectangle();
            hud3DImageSourceRectangle = new Rectangle();

            using (FileStream stream = file.OpenRead())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    if (currentLine.ToLowerInvariant().StartsWith("hud"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                hud3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                hud3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                hud3DImageSourceRectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - hud3DImageSourceRectangle.X);

                                hud3DImageSourceRectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - hud3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd4"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                mfd4_3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                mfd4_3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                mfd4_3DImageSourceRectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - mfd4_3DImageSourceRectangle.X);
                                mfd4_3DImageSourceRectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - mfd4_3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }

                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfd3"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                mfd3_3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                mfd3_3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                mfd3_3DImageSourceRectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - mfd3_3DImageSourceRectangle.X);
                                mfd3_3DImageSourceRectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - mfd3_3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }

                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdleft"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                leftMfd3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                leftMfd3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                leftMfd3DImageSourceRectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - leftMfd3DImageSourceRectangle.X);
                                leftMfd3DImageSourceRectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - leftMfd3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }

                        }
                    }
                    else if (currentLine.ToLowerInvariant().StartsWith("mfdright"))
                    {
                        List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 12)
                        {
                            try
                            {
                                rightMfd3DImageSourceRectangle.X = Convert.ToInt32(tokens[10]);
                                rightMfd3DImageSourceRectangle.Y = Convert.ToInt32(tokens[11]);
                                rightMfd3DImageSourceRectangle.Width = Math.Abs(Convert.ToInt32(tokens[12]) - rightMfd3DImageSourceRectangle.X);
                                rightMfd3DImageSourceRectangle.Height = Math.Abs(Convert.ToInt32(tokens[13]) - rightMfd3DImageSourceRectangle.Y);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, e);
                            }
                        }
                    }
                }
            }
            if (isDoubleResolution)
            {
                leftMfd3DImageSourceRectangle = MultiplyRectangle(leftMfd3DImageSourceRectangle, 2);
                rightMfd3DImageSourceRectangle = MultiplyRectangle(rightMfd3DImageSourceRectangle, 2);
                mfd3_3DImageSourceRectangle = MultiplyRectangle(mfd3_3DImageSourceRectangle, 2);
                mfd4_3DImageSourceRectangle = MultiplyRectangle(mfd4_3DImageSourceRectangle, 2);
                hud3DImageSourceRectangle = MultiplyRectangle(hud3DImageSourceRectangle, 2);
            }
        }
        private static Rectangle MultiplyRectangle(Rectangle rect, int factor)
        {
            return new Rectangle(rect.X*factor, rect.Y*factor, rect.Width*factor,
                                 rect.Height*factor);
        }
        private static string RunningBmsInstanceBasePath()
        {
            string toReturn = null;
            string exePath = F4Utils.Process.Util.GetFalconExePath();
            if (!string.IsNullOrEmpty(exePath))
            {
                toReturn = new FileInfo(exePath).Directory.FullName;
            }
            return toReturn;
        }
        private static bool IsDoubleResolutionRtt()
        {
            string bmsPath = RunningBmsInstanceBasePath();
            if (string.IsNullOrEmpty(bmsPath)) return false;
            FileInfo file = new FileInfo(Path.Combine(bmsPath, "FalconBMS.cfg"));
            if (!file.Exists)
            {
                file = new FileInfo(Path.Combine(Path.Combine(bmsPath, "config"), "Falcon BMS.cfg"));
                if (!file.Exists)
                {
                    file = new FileInfo(Path.Combine(Path.Combine(bmsPath, @"..\..\User\config"), "Falcon BMS.cfg"));
                }

            }

            if (file.Exists)
            {
                List<string> allLines = new List<string>();
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        allLines.Add(reader.ReadLine());
                    }
                    reader.Close();
                }

                for (int i = 0; i < allLines.Count; i++)
                {
                    string currentLine = allLines[i];
                    List<String> tokens = Common.Strings.Util.Tokenize(currentLine);
                    if (tokens.Count > 2)
                    {
                        if (tokens[0].ToLowerInvariant() == "set" && tokens[1].ToLowerInvariant() == "g_bDoubleRTTResolution".ToLowerInvariant() && (tokens[2].ToLowerInvariant() == "1".ToLowerInvariant()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static FileInfo FindBms3DCockpitFile()
        {
            string basePath = RunningBmsInstanceBasePath();
            string path = null;
            if (basePath != null)
            {

                path = basePath + @"\art\ckptartn";
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (System.IO.IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        try
                        {
                            using (FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                            {
                                fs.Close();
                            }
                        }
                        catch (System.IO.IOException)
                        {
                            return file;
                        }
                    }

                }

                path = basePath + @"\art\ckptart";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (System.IO.IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }






                path = basePath + @"\..\..\Data\art\ckptartn";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (System.IO.IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }

                path = basePath + @"\..\..\Data\art\ckptart";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (DirectoryInfo thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (file.Exists)
                        {
                            try
                            {
                                using (FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (System.IO.IOException)
                            {
                                return file;
                            }
                        }
                    }

                    file = new FileInfo(dir.FullName + @"\3dckpit.dat");
                    if (file.Exists)
                    {
                        return file;
                    }
                }

            }
            return null;
        }
        #endregion

        #region WMI Performance Counters Creation & Management
        private void CreatePerformanceCounters()
        {
            try
            {
                // Create a category that contains multiple counters
                // define the CounterCreationData for the three counters
                CounterCreationData[] ccds = new CounterCreationData[]
                {
                  new CounterCreationData("MFD4 FPS", "MFD #4 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                  new CounterCreationData("MFD3 FPS", "MFD #3 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                  new CounterCreationData("LMFD FPS", "Left MFD Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                  new CounterCreationData("RMFD FPS", "Right MFD Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                  new CounterCreationData("HUD FPS", "HUD Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("ADI FPS", "ADI Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Backup ADI FPS", "Backup ADI Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Airspeed Indicator FPS", "Airspeed Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Altimeter FPS", "Altimeter Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("AOA Indexer FPS", "AOA Indexer Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("AOA Indicator FPS", "AOA Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Caution Panel FPS", "Caution Panel Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("CMDS Panel FPS", "CMDS Panel Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Compass FPS", "Compass Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("DED FPS", "DED Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("PFL FPS", "PFL Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("EPU FUEL FPS", "EPU FUEL Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Accelerometer FPS", "Accelerometer Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("FTIT1 FPS", "FTIT1 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("FTIT2 FPS", "FTIT2 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Fuel Flow Indicator FPS", "Fuel Flow Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("ISIS FPS", "ISIS Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Fuel Quantity Indicator FPS", "Fuel Quantity Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("HSI FPS", "HSI Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("EHSI FPS", "EHSI Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Landing Gear Lights FPS", "Landing Gear Lights Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("NWS Indexer FPS", "NWS Indexer Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("NOZ1 FPS", "NOZ1 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("NOZ2 FPS", "NOZ2 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Oil1 FPS", "Oil1 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Oil2 FPS", "Oil2 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("RWR FPS", "RWR Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Speedbrake FPS", "Speedbrake Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("RPM1 FPS", "RPM1 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("RPM2 FPS", "RPM2 Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("VVI FPS", "VVI Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("HYD A FPS", "HYD A Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("HYD B FPS", "HYD B Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Cabin Pressure Altitude Indicator FPS", "Cabin Pressure Altitude Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Pitch Trim Indicator FPS", "Pitch Trim Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32),
                   new CounterCreationData("Roll Trim Indicator FPS", "Roll Trim Indicator Frames per Second", PerformanceCounterType.RateOfCountsPerSecond32)
                };

                // Create a CounterCreationDataCollection from the array
                CounterCreationDataCollection counterCollection =
                  new CounterCreationDataCollection(ccds);

                //delete existing counters
                try
                {
                    PerformanceCounterCategory.Delete(Application.ProductName);
                }
                catch (InvalidOperationException)
                {
                }
                // Create the category with the counters
                PerformanceCounterCategory category =
                  PerformanceCounterCategory.Create(Application.ProductName, Application.ProductName + " performance counters", PerformanceCounterCategoryType.SingleInstance,
                    counterCollection);


                _mfd4PerfCounter = new PerformanceCounter(Application.ProductName, "MFD4 FPS");
                _mfd4PerfCounter.ReadOnly = false;
                _mfd3PerfCounter = new PerformanceCounter(Application.ProductName, "MFD3 FPS");
                _mfd3PerfCounter.ReadOnly = false;
                _leftMfdPerfCounter = new PerformanceCounter(Application.ProductName, "LMFD FPS");
                _leftMfdPerfCounter.ReadOnly = false;
                _rightMfdPerfCounter = new PerformanceCounter(Application.ProductName, "RMFD FPS");
                _rightMfdPerfCounter.ReadOnly = false;
                _hudPerfCounter = new PerformanceCounter(Application.ProductName, "HUD FPS");
                _hudPerfCounter.ReadOnly = false;

                _adiCounter = new PerformanceCounter(Application.ProductName, "ADI FPS");
                _adiCounter.ReadOnly = false;

                _backupAdiCounter = new PerformanceCounter(Application.ProductName, "Backup ADI FPS");
                _backupAdiCounter.ReadOnly = false;

                _asiCounter = new PerformanceCounter(Application.ProductName, "Airspeed Indicator FPS");
                _asiCounter.ReadOnly = false;

                _altimeterCounter = new PerformanceCounter(Application.ProductName, "Altimeter FPS");
                _altimeterCounter.ReadOnly = false;

                _aoaIndexerCounter = new PerformanceCounter(Application.ProductName, "AOA Indexer FPS");
                _aoaIndexerCounter.ReadOnly = false;

                _aoaIndicatorCounter = new PerformanceCounter(Application.ProductName, "AOA Indicator FPS");
                _aoaIndicatorCounter.ReadOnly = false;

                _cautionPanelCounter = new PerformanceCounter(Application.ProductName, "Caution Panel FPS");
                _cautionPanelCounter.ReadOnly = false;

                _cmdsPanelCounter = new PerformanceCounter(Application.ProductName, "CMDS Panel FPS");
                _cmdsPanelCounter.ReadOnly = false;

                _compassCounter = new PerformanceCounter(Application.ProductName, "Compass FPS");
                _compassCounter.ReadOnly = false;

                _dedCounter = new PerformanceCounter(Application.ProductName, "DED FPS");
                _dedCounter.ReadOnly = false;

                _pflCounter = new PerformanceCounter(Application.ProductName, "PFL FPS");
                _pflCounter.ReadOnly = false;

                _epuFuelCounter = new PerformanceCounter(Application.ProductName, "EPU FUEL FPS");
                _epuFuelCounter.ReadOnly = false;

                _accelerometerCounter = new PerformanceCounter(Application.ProductName, "Accelerometer FPS");
                _accelerometerCounter.ReadOnly = false;

                _ftit1Counter = new PerformanceCounter(Application.ProductName, "FTIT1 FPS");
                _ftit1Counter.ReadOnly = false;

                _ftit2Counter = new PerformanceCounter(Application.ProductName, "FTIT2 FPS");
                _ftit2Counter.ReadOnly = false;

                _fuelFlowCounter = new PerformanceCounter(Application.ProductName, "Fuel Flow Indicator FPS");
                _fuelFlowCounter.ReadOnly = false;

                _isisCounter = new PerformanceCounter(Application.ProductName, "ISIS FPS");
                _isisCounter.ReadOnly = false;

                _fuelQuantityCounter = new PerformanceCounter(Application.ProductName, "Fuel Quantity Indicator FPS");
                _fuelQuantityCounter.ReadOnly = false;

                _hsiCounter = new PerformanceCounter(Application.ProductName, "HSI FPS");
                _hsiCounter.ReadOnly = false;

                _ehsiCounter = new PerformanceCounter(Application.ProductName, "EHSI FPS");
                _ehsiCounter.ReadOnly = false;

                _landingGearLightsCounter = new PerformanceCounter(Application.ProductName, "Landing Gear Lights FPS");
                _landingGearLightsCounter.ReadOnly = false;

                _nwsIndexerCounter = new PerformanceCounter(Application.ProductName, "NWS Indexer FPS");
                _nwsIndexerCounter.ReadOnly = false;

                _nozPos1Counter = new PerformanceCounter(Application.ProductName, "NOZ1 FPS");
                _nozPos1Counter.ReadOnly = false;

                _nozPos2Counter = new PerformanceCounter(Application.ProductName, "NOZ2 FPS");
                _nozPos2Counter.ReadOnly = false;

                _oilGauge1Counter = new PerformanceCounter(Application.ProductName, "Oil1 FPS");
                _oilGauge1Counter.ReadOnly = false;

                _oilGauge2Counter = new PerformanceCounter(Application.ProductName, "Oil2 FPS");
                _oilGauge2Counter.ReadOnly = false;

                _rwrCounter = new PerformanceCounter(Application.ProductName, "RWR FPS");
                _rwrCounter.ReadOnly = false;

                _speedbrakeCounter = new PerformanceCounter(Application.ProductName, "Speedbrake FPS");
                _speedbrakeCounter.ReadOnly = false;

                _rpm1Counter = new PerformanceCounter(Application.ProductName, "RPM1 FPS");
                _rpm1Counter.ReadOnly = false;

                _rpm2Counter = new PerformanceCounter(Application.ProductName, "RPM2 FPS");
                _rpm2Counter.ReadOnly = false;

                _vviCounter = new PerformanceCounter(Application.ProductName, "VVI FPS");
                _vviCounter.ReadOnly = false;

                _hydACounter = new PerformanceCounter(Application.ProductName, "HYD A FPS");
                _hydACounter.ReadOnly = false;

                _hydBCounter = new PerformanceCounter(Application.ProductName, "HYD B FPS");
                _hydBCounter.ReadOnly = false;

                _cabinPressCounter = new PerformanceCounter(Application.ProductName, "Cabin Pressure Altitude Indicator FPS");
                _cabinPressCounter.ReadOnly = false;

                _pitchTrimCounter = new PerformanceCounter(Application.ProductName, "Pitch Trim Indicator FPS");
                _pitchTrimCounter.ReadOnly = false;

                _rollTrimCounter = new PerformanceCounter(Application.ProductName, "Roll Trim Indicator FPS");
                _rollTrimCounter.ReadOnly = false;

            }
            catch (Exception)
            {
            }

        }
        #endregion

        #region Object Disposal & Destructors
        /// <summary>
        /// Public implementation of the IDisposable pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Private implementation of the IDisposable pattern
        /// </summary>
        /// <param name="disposing"></param>
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
