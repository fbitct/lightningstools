using System;
using System.Collections.Concurrent;
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
using Common.UI;
using F4SharedMem;
using F4Utils.Process;
using MFDExtractor.BMSSupport;
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
	    private KeySettings _keySettings;
	    private readonly IKeySettingsReader _keySettingsReader = new KeySettingsReader();
		private readonly IInstrumentRendererSet _renderers = new InstrumentRendererSet();
        private readonly IRendererSetInitializer _rendererSetInitializer;
        private GdiPlusOptions _gdiPlusOptions = new GdiPlusOptions();

	    private CaptureCoordinates _hudCaptureCoordinates;
        private CaptureCoordinates _leftMfdCaptureCoordinates;
        private CaptureCoordinates _rightMfdCaptureCoordinates;
        private CaptureCoordinates _mfd3CaptureCoordinates;
        private CaptureCoordinates _mfd4CaptureCoordinates;

        #region Output Window Coordinates

        private readonly object _texSmReaderLock = new object();
        private readonly IFlightDataUpdater _flightDataUpdater = new FlightDataUpdater();
        private bool _sim3DDataAvailable;

        #endregion

        #region Falcon 4 Sharedmem Readers & status flags

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

        private readonly AutoResetEvent _hudCaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _leftMfdCaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _mfd3CaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _mfd4CaptureStart = new AutoResetEvent(false);
        private readonly AutoResetEvent _rightMfdCaptureStart = new AutoResetEvent(false);
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
       private Thread _captureOrchestrationThread;
        private Thread _hudCaptureThread;
        private Thread _keyboardWatcherThread;
        private Thread _leftMfdCaptureThread;
        private Thread _mfd3CaptureThread;
        private Thread _mfd4CaptureThread;
        private Thread _rightMfdCaptureThread;
        private Thread _simStatusMonitorThread;

        private readonly ThreadAbortion _threadAbortion;

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

	    private readonly IDictionary<InstrumentType, IInstrument> _instruments = new ConcurrentDictionary<InstrumentType, IInstrument>();
	    private readonly IInstrumentFactory _instrumentFactory;
	    private InstrumentForm _hudForm;
        private InstrumentForm _leftMfdForm;
        private InstrumentForm _rightMfdForm;
        private InstrumentForm _mfd3Form;
        private InstrumentForm _mfd4Form;
	    private readonly IInstrumentFormFactory _instrumentFormFactory;
		private readonly IThreeDeeCaptureCoordinateReader _threeDeeCaptureCoordinateReader;
	    private readonly IFlightDataRetriever _flightDataRetriever;
        #endregion

        #endregion


        private Extractor(
            IKeyDownEventHandler keyDownEventHandler = null, 
			IKeyUpEventHandler keyUpEventHandler=null,
			IKeyboardWatcher keyboardWatcher = null,
			IDirectInputEventHotkeyFilter directInputEventHotkeyFilter= null, 
			IEHSIStateTracker ehsiStateTracker =null, 
			IClientSideIncomingMessageDispatcher clientSideIncomingMessageDispatcher = null,
			IServerSideIncomingMessageDispatcher serverSideIncomingMessageDispatcher = null, 
			IGdiPlusOptionsReader gdiPlusOptionsReader=null,
			IInputEvents inputEvents = null,
            IInstrumentFactory instrumentFactory = null,
            IInstrumentFormFactory instrumentFormFactory = null,
			IThreeDeeCaptureCoordinateReader threeDeeCaptureCoordinateReader=null,
            IFlightDataRetriever flightDataRetriever= null)
        {
            State = new ExtractorState();
            _instrumentFormFactory = instrumentFormFactory ?? new InstrumentFormFactory();
	        _gdiPlusOptionsReader = gdiPlusOptionsReader ?? new GdiPlusOptionsReader();
            LoadSettings();
			_rendererSetInitializer = new RendererSetInitializer(_renderers);
			_rendererSetInitializer.Initialize(_gdiPlusOptions);
            _instrumentFactory = instrumentFactory ?? new InstrumentFactory(_renderers);
			_ehsiStateTracker = ehsiStateTracker ?? new EHSIStateTracker(_renderers.EHSI);
			_directInputEventHotkeyFilter = directInputEventHotkeyFilter ?? new DirectInputEventHotkeyFilter();
			_diHotkeyDetection = new DIHotkeyDetection(Mediator);
            _inputEvents = inputEvents ?? new InputEvents(_renderers, _ehsiStateTracker, State);
	        _mediatorEventHandler =  new MediatorStateChangeHandler(_keySettings, _directInputEventHotkeyFilter,_diHotkeyDetection, _ehsiStateTracker,_inputEvents );
            if (!Settings.Default.DisableDirectInputMediator)
            {
                Mediator = new Mediator(null);
            }
            _threadAbortion = new ThreadAbortion();
	        _keyDownEventHandler = keyDownEventHandler ?? new KeyDownEventHandler(_ehsiStateTracker, _inputEvents, _keySettings);
			_keyUpEventHandler = keyUpEventHandler ??  new KeyUpEventHandler(_keySettings, _ehsiStateTracker, _inputEvents);
			_keyboardWatcher = keyboardWatcher ?? new KeyboardWatcher(_keyDownEventHandler, _keyUpEventHandler, Log);
			_clientSideIncomingMessageDispatcher = clientSideIncomingMessageDispatcher ?? new ClientSideIncomingMessageDispatcher(_inputEvents, _client);
			_serverSideIncomingMessageDispatcher = serverSideIncomingMessageDispatcher ?? new ServerSideIncomingMessageDispatcher(_inputEvents);
            _flightDataRetriever = flightDataRetriever ?? new FlightDataRetriever(_client);
			_threeDeeCaptureCoordinateReader = threeDeeCaptureCoordinateReader ?? new ThreeDeeCaptureCoordinateReader();
        }
        private void SetupInstruments()
        {
            foreach (InstrumentType instrumentType in Enum.GetValues(typeof (InstrumentType)))
            {
                var instrument = _instrumentFactory.Create(instrumentType);
                _instruments[instrumentType] = instrument;
                instrument.Start(State);
            }
        }
        
        
        public void Start()
        {
            if (State.Running)
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
            State.KeepRunning = false;
            if (Mediator != null)
            {
                Mediator.PhysicalControlStateChanged -= _mediatorEventHandler.HandleStateChange;
            }

            var threadsToKill = new[]
                {
                    _captureOrchestrationThread,
                    _simStatusMonitorThread,
                    _keyboardWatcherThread,
                    _mfd4CaptureThread,
                    _mfd3CaptureThread,
                    _leftMfdCaptureThread,
                    _rightMfdCaptureThread,
                    _hudCaptureThread
                };
            WaitForThreadEndThenAbort(ref threadsToKill, new TimeSpan(0, 0, 1));

            CloseOutputWindowForms();
            if (State.NetworkMode == NetworkMode.Server)
            {
                TearDownImageServer();
            }
            CloseAndDisposeSharedmemReaders();
            State.Running = false;
            OnStopped();
        }
        private void CloseOutputWindowForms()
        {       
            CloseAndDisposeForm(_hudForm);
            CloseAndDisposeForm(_leftMfdForm);
            CloseAndDisposeForm(_rightMfdForm);
            CloseAndDisposeForm(_mfd3Form);
            CloseAndDisposeForm(_mfd4Form);
            foreach (var instrument in _instruments)
            {
                CloseAndDisposeForm(instrument.Value.Form);
            }
        }
        private void CloseAndDisposeForm(Form form)
        {
            if (form == null) return;
            try
            {
                form.Close();
                form.Dispose();
            }
            catch {}
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
            State.NetworkMode = (NetworkMode) settings.NetworkingMode;
            switch (State.NetworkMode)
            {
                case NetworkMode.Server:
                    _serverEndpoint = new IPEndPoint(IPAddress.Any, settings.ServerUsePortNumber);
                    break;
                case NetworkMode.Client:
                    _serverEndpoint = new IPEndPoint(IPAddress.Parse(settings.ClientUseServerIpAddress),
                        settings.ClientUseServerPortNum);
                    break;
            }
            if (State.NetworkMode == NetworkMode.Server || State.NetworkMode == NetworkMode.Standalone)
            {
                _mfd4CaptureCoordinates = new CaptureCoordinates
                {
                    Primary2D =
                        Rectangle.FromLTRB(settings.Primary_MFD4_2D_ULX, settings.Primary_MFD4_2D_ULY,
                            settings.Primary_MFD4_2D_LRX, settings.Primary_MFD4_2D_LRY),
                    Secondary2D =
                        Rectangle.FromLTRB(settings.Secondary_MFD4_2D_ULX, settings.Secondary_MFD4_2D_ULY,
                            settings.Secondary_MFD4_2D_LRX, settings.Secondary_MFD4_2D_LRY),
                    Output =
                        Rectangle.FromLTRB(settings.MFD4_OutULX, settings.MFD4_OutULY, settings.MFD4_OutLRX,
                            settings.MFD4_OutLRY)
                };
                _mfd3CaptureCoordinates = new CaptureCoordinates
                {
                    Primary2D = Rectangle.FromLTRB(settings.Primary_MFD3_2D_ULX, settings.Primary_MFD3_2D_ULY,
                        settings.Primary_MFD3_2D_LRX, settings.Primary_MFD3_2D_LRY),
                    Secondary2D = Rectangle.FromLTRB(settings.Secondary_MFD3_2D_ULX,
                        settings.Secondary_MFD3_2D_ULY,
                        settings.Secondary_MFD3_2D_LRX,
                        settings.Secondary_MFD3_2D_LRY),
                    Output =
                        Rectangle.FromLTRB(settings.MFD3_OutULX, settings.MFD3_OutULY, settings.MFD3_OutLRX,
                            settings.MFD3_OutLRY)
                };
                _leftMfdCaptureCoordinates = new CaptureCoordinates
                {
                    Primary2D = Rectangle.FromLTRB(settings.Primary_LMFD_2D_ULX,
                        settings.Primary_LMFD_2D_ULY,
                        settings.Primary_LMFD_2D_LRX,
                        settings.Primary_LMFD_2D_LRY),
                    Secondary2D = Rectangle.FromLTRB(settings.Secondary_LMFD_2D_ULX,
                        settings.Secondary_LMFD_2D_ULY,
                        settings.Secondary_LMFD_2D_LRX,
                        settings.Secondary_LMFD_2D_LRY),
                    Output =
                        Rectangle.FromLTRB(settings.LMFD_OutULX, settings.LMFD_OutULY, settings.LMFD_OutLRX,
                            settings.LMFD_OutLRY)
                };
                _rightMfdCaptureCoordinates = new CaptureCoordinates
                {
                    Primary2D = Rectangle.FromLTRB(settings.Primary_RMFD_2D_ULX,
                        settings.Primary_RMFD_2D_ULY,
                        settings.Primary_RMFD_2D_LRX,
                        settings.Primary_RMFD_2D_LRY),
                    Secondary2D = Rectangle.FromLTRB(settings.Secondary_RMFD_2D_ULX,
                        settings.Secondary_RMFD_2D_ULY,
                        settings.Secondary_RMFD_2D_LRX,
                        settings.Secondary_RMFD_2D_LRY),
                    Output =
                        Rectangle.FromLTRB(settings.RMFD_OutULX, settings.RMFD_OutULY, settings.RMFD_OutLRX,
                            settings.RMFD_OutLRY)
                };
                _hudCaptureCoordinates = new CaptureCoordinates
                {
                    Primary2D = Rectangle.FromLTRB(settings.Primary_HUD_2D_ULX, settings.Primary_HUD_2D_ULY,
                        settings.Primary_HUD_2D_LRX, settings.Primary_HUD_2D_LRY),
                    Secondary2D = Rectangle.FromLTRB(settings.Secondary_HUD_2D_ULX,
                        settings.Secondary_HUD_2D_ULY,
                        settings.Secondary_HUD_2D_LRX,
                        settings.Secondary_HUD_2D_LRY),
                    Output =
                        Rectangle.FromLTRB(settings.HUD_OutULX, settings.HUD_OutULY, settings.HUD_OutLRX,
                            settings.HUD_OutLRY)
                };
            }

            
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
		public static ExtractorState State { get; set; }
        public Mediator Mediator { get; set; }

        #endregion

        #region Networking Support

        #region Basic Network Client/Server Setup Code

        private void SetupNetworking()
        {
            if (State.NetworkMode == NetworkMode.Client)
            {
                SetupNetworkingClient();
            }
            if (State.NetworkMode == NetworkMode.Server)
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


        private Image GetImage(Image testAlignmentImage,
            Func<Image> threeDeeModeLocalCaptureFunc,
            Func<Image> remoteImageRequestFunc, CaptureCoordinates captureCoordinates)
        {
            Image toReturn = null;
            if (State.TestMode)
            {
                toReturn = Util.CloneBitmap(testAlignmentImage);
            }
            else
            {
                if (!State.SimRunning && State.NetworkMode != NetworkMode.Client) return null;
                if (State.ThreeDeeMode && (State.NetworkMode == NetworkMode.Server || State.NetworkMode == NetworkMode.Standalone))
                {
                    toReturn = threeDeeModeLocalCaptureFunc();
                }
                else
                {
                    switch (State.NetworkMode)
                    {
                        case NetworkMode.Standalone: //fallthrough
                        case NetworkMode.Server:
                            toReturn = Common.Screen.Util.CaptureScreenRectangle(State.TwoDeePrimaryView
                                ? captureCoordinates.Primary2D 
                                : captureCoordinates.Secondary2D);
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
            return GetImage(_mfd4TestAlignmentImage, Get3DMFD4, () => _client != null ? _client.GetMfd4Bitmap() : null, _mfd4CaptureCoordinates);
        }

        private Image GetMfd3Bitmap()
        {
            return GetImage(_mfd3TestAlignmentImage, Get3DMFD3, () => _client != null ? _client.GetMfd3Bitmap():null, _mfd3CaptureCoordinates);
        }

        private Image GetLeftMfdBitmap()
        {
            return GetImage(_leftMfdTestAlignmentImage, Get3DLeftMFD, () => _client != null ? _client.GetLeftMfdBitmap():null, _leftMfdCaptureCoordinates);
        }

	    private Image GetRightMfdBitmap()
	    {
            return GetImage(_rightMfdTestAlignmentImage, Get3DRightMFD, () => _client != null ? _client.GetRightMfdBitmap():null, _rightMfdCaptureCoordinates);
	    }

	    private Image GetHudBitmap()
        {
            return GetImage(_hudTestAlignmentImage, Get3DHud, () => _client != null ? _client.GetHudBitmap():null, _hudCaptureCoordinates);
        }

        private Image Get3D(Rectangle rttInputRectangle)
        {
            if (!State.KeepRunning || (!State.SimRunning || !_sim3DDataAvailable) || rttInputRectangle == Rectangle.Empty)
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
            return Get3D(_hudCaptureCoordinates.ThreeDee);
        }

        private Image Get3DMFD4()
        {
            return Get3D(_mfd4CaptureCoordinates.ThreeDee);
        }

        private Image Get3DMFD3()
        {
            return Get3D(_mfd3CaptureCoordinates.ThreeDee);
        }

        private Image Get3DLeftMFD()
        {
            return Get3D(_leftMfdCaptureCoordinates.ThreeDee);
        }

        private Image Get3DRightMFD()
        {
            return Get3D(_rightMfdCaptureCoordinates.ThreeDee);
        }

        #endregion

        #region MFD Capturing implementation methods

        private void CaptureAndUpdateOutput(bool instrumentEnabled, Func<Image> getterFunc, Action<Image> setterFunc, Image blankVersion )
        {
            if (!instrumentEnabled && State.NetworkMode != NetworkMode.Server) return;

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
            if (State.NetworkMode == NetworkMode.Server)
            {
                ExtractorServer.SetFlightData(flightData);
            }
        }
        private void SetAndDisposeImage(Image image, Action<Image> serveImageFunc, RotateFlipType rotateFlipType, InstrumentForm instrumentForm, bool monochrome)
        {
            if (image == null) return;
            if (State.NetworkMode == NetworkMode.Server)
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
            SetAndDisposeImage(hudImage, ExtractorServer.SetHudBitmap, Settings.Default.HUD_RotateFlipType, _hudForm, Settings.Default.HUD_Monochrome);
        }
        private void SetMfd4Image(Image mfd4Image)
        {
            SetAndDisposeImage(mfd4Image, ExtractorServer.SetMfd4Bitmap, Settings.Default.MFD4_RotateFlipType, _mfd4Form, Settings.Default.MFD4_Monochrome);
        }

        private void SetMfd3Image(Image mfd3Image)
        {
            SetAndDisposeImage(mfd3Image, ExtractorServer.SetMfd3Bitmap, Settings.Default.MFD3_RotateFlipType, _mfd3Form, Settings.Default.MFD3_Monochrome);
        }

        private void SetLeftMfdImage(Image leftMfdImage)
        {
            SetAndDisposeImage(leftMfdImage, ExtractorServer.SetLeftMfdBitmap, Settings.Default.LMFD_RotateFlipType, _leftMfdForm, Settings.Default.LMFD_Monochrome);
        }

        private void SetRightMfdImage(Image rightMfdImage)
        {
            SetAndDisposeImage(rightMfdImage, ExtractorServer.SetRightMfdBitmap, Settings.Default.RMFD_RotateFlipType, _rightMfdForm, Settings.Default.RMFD_Monochrome);
        }

        #endregion

        #endregion

        #region Forms Management

        #region Forms Setup

        private void SetupOutputForms()
        {
            if (Settings.Default.EnableMfd4Output || State.NetworkMode == NetworkMode.Server)
            {
                SetupMfd4Form();
            }
            if (Settings.Default.EnableMfd3Output || State.NetworkMode == NetworkMode.Server)
            {
                SetupMfd3Form();
            }
            if (Settings.Default.EnableLeftMFDOutput || State.NetworkMode == NetworkMode.Server)
            {
                SetupLeftMfdForm();
            }

            if (Settings.Default.EnableRightMFDOutput || State.NetworkMode == NetworkMode.Server)
            {
                SetupRightMfdForm();
            }

            if (Settings.Default.EnableHudOutput || State.NetworkMode == NetworkMode.Server)
            {
                SetupHudForm();
            }
        }

        #region MFD Forms Setup

        private void SetupMfd4Form()
        {
            _mfd4Form = _instrumentFormFactory.Create("MFD4","MFD 4",null,_mfd4BlankImage);
        }

        private void SetupMfd3Form()
        {
            _mfd3Form = _instrumentFormFactory.Create("MFD3", "MFD 3", null, _mfd3BlankImage);
        }

        private void SetupLeftMfdForm()
        {
            _leftMfdForm = _instrumentFormFactory.Create("LMFD", "Left MFD", null,_leftMfdBlankImage);
        }

        private void SetupRightMfdForm()
        {
            _rightMfdForm = _instrumentFormFactory.Create("RMFD", "Right MFD", null, _rightMfdBlankImage);
        }

        private void SetupHudForm()
        {
            _hudForm = _instrumentFormFactory.Create("HUD", "HUD", null,_hudBlankImage);
        }

        #endregion


        #endregion


        #endregion

        #region Thread Management

        /// <summary>
        ///     Sets up all worker threads and output forms and starts the worker threads running
        /// </summary>
        private void RunThreads()
        {
            if (State.Running) return;
            State.Running = true;
            SetupNetworking();
            State.KeepRunning = true;
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

            StartAllInstruments();

            StartThread(_simStatusMonitorThread);
            StartThread(_captureOrchestrationThread);
            StartThread(_keyboardWatcherThread);
        }

	    private void StartAllInstruments()
	    {
	        _instruments.Select(i => i.Value).ToList().ForEach(i => i.Start(State));
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
                while (State.KeepRunning)
                {
                    Application.DoEvents();
                    if (State.RenderCycleNum < 10000)
                    {
                        State.RenderCycleNum++;
                    }
                    else
                    {
                        State.RenderCycleNum = 0;
                    }
                    var thisLoopStartTime = DateTime.Now;

                    switch (State.NetworkMode)
                    {
                        case NetworkMode.Client:
                            _clientSideIncomingMessageDispatcher.ProcessPendingMessages();
                            break;
                        case NetworkMode.Server:
                            _serverSideIncomingMessageDispatcher.ProcessPendingMessages();
                            break;
                    }


                    if (State.SimRunning || State.TestMode || State.NetworkMode == NetworkMode.Client)
                    {
                        var currentFlightData = _flightDataRetriever.GetFlightData(State);
                        SetFlightData(currentFlightData);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, currentFlightData, State.SimRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, State.NetworkMode);
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
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, flightDataToSet, State.SimRunning, _useBMSAdvancedSharedmemValues, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, State.NetworkMode);
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
                        _instruments[InstrumentType.RWR].Signal(toWait, State);
                        _instruments[InstrumentType.ADI].Signal(toWait, State);
                        _instruments[InstrumentType.ISIS].Signal(toWait, State);
                        _instruments[InstrumentType.HSI].Signal(toWait, State);
                        _instruments[InstrumentType.EHSI].Signal(toWait, State);
                        _instruments[InstrumentType.Altimeter].Signal(toWait, State);
                        _instruments[InstrumentType.ASI].Signal(toWait, State);
                        _instruments[InstrumentType.BackupADI].Signal(toWait, State);
                        _instruments[InstrumentType.VVI].Signal(toWait, State);
                        _instruments[InstrumentType.AOAIndicator].Signal(toWait, State);
                        _instruments[InstrumentType.Compass].Signal(toWait, State);
                        _instruments[InstrumentType.Accelerometer].Signal(toWait, State);

                        _instruments[InstrumentType.FuelFlow].Signal(toWait, State);
                        _instruments[InstrumentType.FuelQuantity].Signal(toWait, State);
                        _instruments[InstrumentType.EPUFuel].Signal(toWait, State);
                        _instruments[InstrumentType.AOAIndexer].Signal(toWait, State);
                        _instruments[InstrumentType.NWSIndexer].Signal(toWait, State);

                        _instruments[InstrumentType.FTIT1].Signal(toWait, State);
                        _instruments[InstrumentType.NOZ1].Signal(toWait, State);
                        _instruments[InstrumentType.OIL1].Signal(toWait, State);
                        _instruments[InstrumentType.RPM1].Signal(toWait, State);

                        _instruments[InstrumentType.FTIT2].Signal(toWait, State);
                        _instruments[InstrumentType.NOZ2].Signal(toWait, State);
                        _instruments[InstrumentType.OIL2].Signal(toWait, State);
                        _instruments[InstrumentType.RPM2].Signal(toWait, State);

                        _instruments[InstrumentType.HYDA].Signal(toWait, State);
                        _instruments[InstrumentType.HYDB].Signal(toWait, State);
                        _instruments[InstrumentType.CabinPress].Signal(toWait, State);
                        _instruments[InstrumentType.RollTrim].Signal(toWait, State);
                        _instruments[InstrumentType.PitchTrim].Signal(toWait, State);

                        _instruments[InstrumentType.DED].Signal(toWait, State);
                        _instruments[InstrumentType.PFL].Signal(toWait, State);

                        _instruments[InstrumentType.CautionPanel].Signal(toWait, State);
                        _instruments[InstrumentType.CMDSPanel].Signal(toWait, State);

                        _instruments[InstrumentType.LandingGearLights].Signal(toWait, State);
                        _instruments[InstrumentType.Speedbrake].Signal(toWait, State);

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
                    if (State.TestMode) millisToSleep = 500;
                    var sleepUntil = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, millisToSleep));
                    while (DateTime.Now < sleepUntil)
                    {
                        var millisRemaining = (int) Math.Floor(DateTime.Now.Subtract(sleepUntil).TotalMilliseconds);
                        var millisWaited = millisRemaining >= 5 ? 5 : 1;
                        Thread.Sleep(millisWaited);
                        Application.DoEvents();
                    }
                    Application.DoEvents();
                    if ((!State.SimRunning && State.NetworkMode != NetworkMode.Client) && !State.TestMode)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5);
                            //sleep an additional half-second or so here if we're not a client and there's no sim running and we're not in test mode
                    }
                    else if (State.TestMode)
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




        private void SignalMFDAndHudThreadsToStart()
        {
            if (!(State.Running && State.KeepRunning))
            {
                return;
            }
            if (Settings.Default.EnableMfd4Output || State.NetworkMode == NetworkMode.Server)
            {
                _mfd4CaptureStart.Set();
            }
            if (Settings.Default.EnableMfd3Output || State.NetworkMode == NetworkMode.Server)
            {
                _mfd3CaptureStart.Set();
            }
            if (Settings.Default.EnableLeftMFDOutput || State.NetworkMode == NetworkMode.Server)
            {
                _leftMfdCaptureStart.Set();
            }
            if (Settings.Default.EnableRightMFDOutput || State.NetworkMode == NetworkMode.Server)
            {
                _rightMfdCaptureStart.Set();
            }
            if (Settings.Default.EnableHudOutput || State.NetworkMode == NetworkMode.Server)
            {
                _hudCaptureStart.Set();
            }
        }

        private void SimStatusMonitorThreadWork()
        {
            try
            {
                var count = 0;

                while (State.KeepRunning)
                {
                    count++;
                    if (State.NetworkMode == NetworkMode.Server || State.NetworkMode == NetworkMode.Standalone)
                    {
                        var simWasRunning = State.SimRunning;

                        //TODO:make this check optional via the user-config file
                        if (count%1 == 0)
                        {
                            count = 0;
                            Common.Util.DisposeObject(_texSmStatusReader);
                            _texSmStatusReader = new F4TexSharedMem.Reader();

                            try
                            {
                                State.SimRunning = State.NetworkMode == NetworkMode.Client ||
                                              F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                            }
                            _sim3DDataAvailable = State.SimRunning &&
                                                  (State.NetworkMode == NetworkMode.Client ||
                                                   _texSmStatusReader.IsDataAvailable);

                            if (_sim3DDataAvailable)
                            {
                                try
                                {
                                    if (State.ThreeDeeMode)
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
                                             State.NetworkMode == NetworkMode.Server))
                                        {
                                            if ((_hudCaptureCoordinates.ThreeDee == Rectangle.Empty) ||
                                                (_leftMfdCaptureCoordinates.ThreeDee == Rectangle.Empty) ||
                                                (_rightMfdCaptureCoordinates.ThreeDee == Rectangle.Empty) ||
                                                (_mfd3CaptureCoordinates.ThreeDee == Rectangle.Empty) ||
                                                (_mfd4CaptureCoordinates.ThreeDee == Rectangle.Empty))
                                            {
												_threeDeeCaptureCoordinateReader.Read3DCoordinatesFromCurrentBmsDatFile(
                                                                                        _mfd4CaptureCoordinates,
                                                                                       _mfd3CaptureCoordinates,
                                                                                       _leftMfdCaptureCoordinates,
                                                                                       _rightMfdCaptureCoordinates,
                                                                                       _hudCaptureCoordinates);
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
                                _mfd4CaptureCoordinates.ThreeDee = Rectangle.Empty;
                                _mfd3CaptureCoordinates.ThreeDee = Rectangle.Empty;
                                _leftMfdCaptureCoordinates.ThreeDee = Rectangle.Empty;
                                _rightMfdCaptureCoordinates.ThreeDee = Rectangle.Empty;
                                _hudCaptureCoordinates.ThreeDee = Rectangle.Empty;
                            }
                            if (simWasRunning && !State.SimRunning)
                            {
                                CloseAndDisposeSharedmemReaders();

                                if (State.NetworkMode == NetworkMode.Server)
                                {
                                    TearDownImageServer();
                                }
                            }
                            if (State.NetworkMode == NetworkMode.Server && (!simWasRunning && State.SimRunning))
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
            Common.Util.DisposeObject(_texSmStatusReader);
            _texSmStatusReader = null;

            Common.Util.DisposeObject(_texSmReader);
            _texSmReader = null;

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

            SetupInstruments();

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
            SetupCaptureThread(ref _hudCaptureThread, () => Settings.Default.EnableHudOutput || State.NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_hudCaptureStart, CaptureHud), "HudCaptureThread");
        }

        private void SetupRightMFDCaptureThread()
        {
            SetupCaptureThread(ref _rightMfdCaptureThread, () => Settings.Default.EnableRightMFDOutput || State.NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_rightMfdCaptureStart, CaptureRightMfd), "RightMfdCaptureThread");
        }

        private void SetupLeftMFDCaptureThread()
        {
            SetupCaptureThread(ref _leftMfdCaptureThread, () => Settings.Default.EnableLeftMFDOutput || State.NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_leftMfdCaptureStart, CaptureLeftMfd), "LeftMfdCaptureThread");
        }

        private void SetupMFD3CaptureThread()
        {
            SetupCaptureThread(ref _mfd3CaptureThread, () => Settings.Default.EnableMfd3Output || State.NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_mfd3CaptureStart, CaptureMfd3), "Mfd3CaptureThread");
        }

        private void SetupMFD4CaptureThread()
        {
            SetupCaptureThread(ref _mfd4CaptureThread, () => Settings.Default.EnableMfd4Output || State.NetworkMode == NetworkMode.Server, () => CaptureThreadWork(_mfd4CaptureStart, CaptureMfd4), "Mfd4CaptureThread");
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


        private void CaptureThreadWork(WaitHandle waitHandle, Action capture)
        {
            try
            {
                while (State.KeepRunning)
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
                    Common.Util.DisposeObject(_texSmReader);
                    Common.Util.DisposeObject(_texSmStatusReader);
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