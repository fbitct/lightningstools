using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport.DirectInput;
using F4SharedMem;
using F4Utils.Process;
using F4Utils.SimSupport;
using MFDExtractor.EventSystem;
using MFDExtractor.Networking;
using MFDExtractor.Properties;
using log4net;
using Common.Networking;
using MFDExtractor.EventSystem.Handlers;
using F4Utils.Terrain;
using System.Threading.Tasks;

namespace MFDExtractor
{
	public sealed class Extractor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Extractor));
        private static Extractor _extractor;
        private bool _disposed;
	    private IFlightDataUpdater _flightDataUpdater;
        private F4TexSharedMem.IReader _texSmReader = new F4TexSharedMem.Reader();
        private readonly ITerrainDBFactory _terrainDBFactory = new TerrainDBFactory();
        private TerrainDB _terrainDB;
	    private static FlightData _flightData;
        #region Network Configuration

        private string _compressionType = "None";
        private string _imageFormat = "PNG";

        #endregion

        #region Public Events

        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;
        public event EventHandler Starting;

        #endregion


        #region Threads

        private readonly IMediatorStateChangeHandler _mediatorEventHandler;
        
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;
       private Thread _captureOrchestrationThread;
        private Thread _keyboardWatcherThread;
        private Thread _simStatusMonitorThread;

	    private readonly IEHSIStateTracker _ehsiStateTracker;

	    private readonly IKeyboardWatcher _keyboardWatcher;
	    private  IClientSideIncomingMessageDispatcher _clientSideIncomingMessageDispatcher;
		private readonly IServerSideIncomingMessageDispatcher _serverSideIncomingMessageDispatcher;
	    private readonly IInputEvents _inputEvents;

	    private readonly IDictionary<InstrumentType, IInstrument> _instruments = new ConcurrentDictionary<InstrumentType, IInstrument>();
	    private readonly IInstrumentFactory _instrumentFactory;
		private readonly IThreeDeeCaptureCoordinateUpdater _threeDeeCaptureCoordinateUpdater;
	    private  IFlightDataRetriever _flightDataRetriever;
		private readonly TexturesSharedMemoryImageCoordinates _texturesSharedMemoryImageCoordinates = new TexturesSharedMemoryImageCoordinates();
	    private readonly PerformanceCounterInstaller _performanceCounterInstaller;

	    #endregion


        private Extractor(
			IKeyboardWatcher keyboardWatcher = null,
			IClientSideIncomingMessageDispatcher clientSideIncomingMessageDispatcher = null,
			IServerSideIncomingMessageDispatcher serverSideIncomingMessageDispatcher = null, 
            IInstrumentFactory instrumentFactory = null,
			IThreeDeeCaptureCoordinateUpdater threeDeeCaptureCoordinateUpdater=null,
            IFlightDataRetriever flightDataRetriever= null,
			IFlightDataUpdater flightDataUpdater =null)
        {
            State = new ExtractorState();
            LoadSettings();
            _instrumentFactory = instrumentFactory ?? new InstrumentFactory();
            _ehsiStateTracker = new EHSIStateTracker(_instruments);
            _inputEvents = new InputEvents(_instruments, _ehsiStateTracker);
            _clientSideIncomingMessageDispatcher = clientSideIncomingMessageDispatcher ?? new ClientSideIncomingMessageDispatcher(_inputEvents);
            if (!Settings.Default.DisableDirectInputMediator)
            {
                Mediator = new Mediator(null);
                _mediatorEventHandler = new MediatorStateChangeHandler(new DIHotkeyDetection(Mediator), _inputEvents);
            }
			_keyboardWatcher = keyboardWatcher ?? new KeyboardWatcher(_inputEvents, Log);
			_serverSideIncomingMessageDispatcher = serverSideIncomingMessageDispatcher ?? new ServerSideIncomingMessageDispatcher(_inputEvents);
            _flightDataRetriever = flightDataRetriever ?? new FlightDataRetriever();
			_threeDeeCaptureCoordinateUpdater = threeDeeCaptureCoordinateUpdater ?? new ThreeDeeCaptureCoordinateUpdater(_texturesSharedMemoryImageCoordinates);
	        _flightDataUpdater = flightDataUpdater ?? new FlightDataUpdater( _texturesSharedMemoryImageCoordinates);
            _performanceCounterInstaller = new PerformanceCounterInstaller();
        }
        private void SetupInstruments()
        {
            _performanceCounterInstaller.CreatePerformanceCounters();
            foreach (InstrumentType instrumentType in Enum.GetValues(typeof(InstrumentType)))
            {
                _instruments[instrumentType] = _instrumentFactory.Create(instrumentType);
            }
        }


	    public void Start()
        {
            LoadSettings();
            if (State.Running)
            {
                return;
            }
            OnStarting();
            SetupInstruments();
            KeyFileUtils.ResetCurrentKeyFile();
            if (Mediator != null && _mediatorEventHandler !=null)
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
            if (Mediator != null && _mediatorEventHandler !=null)
            {
                Mediator.PhysicalControlStateChanged -= _mediatorEventHandler.HandleStateChange;
            }
	        StopAllInstruments();
            TearDownImageServer();
            State.Running = false;
            Settings.Default.Save();
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
            State.NetworkMode = (NetworkMode) settings.NetworkingMode;
            switch (State.NetworkMode)
            {
                case NetworkMode.Server:
                    ServerEndpoint = new IPEndPoint(IPAddress.Any, settings.ServerUsePortNumber);
                    break;
                case NetworkMode.Client:
                    ServerEndpoint = new IPEndPoint(IPAddress.Parse(settings.ClientUseServerIpAddress),
                        settings.ClientUseServerPortNum);
                    break;
            }
            
            _threadPriority = settings.ThreadPriority;
            _compressionType = settings.CompressionType;
            _imageFormat = settings.NetworkImageFormat;
        }

        #region Public Properties


        public IPEndPoint ServerEndpoint { get; set; }
       
		internal static ExtractorState State { get; set; }
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
                ExtractorClient.ServerEndpoint = ServerEndpoint;
                ExtractorClient.ServiceName = "MFDExtractorService";
                _clientSideIncomingMessageDispatcher = new ClientSideIncomingMessageDispatcher(_inputEvents);
                _flightDataRetriever = new FlightDataRetriever();
                _flightDataUpdater = new FlightDataUpdater(_texturesSharedMemoryImageCoordinates, null);

            }
            catch {}
        }

        private void SetupNetworkingServer()
        {
            try
            {
                ExtractorServer.CreateService("MFDExtractorService", ServerEndpoint.Port, _compressionType, _imageFormat);
            }
            catch { }
        }

        private void TearDownImageServer()
        {
            try
            {
                if (ServerEndpoint != null)
                {
                    ExtractorServer.TearDownService(ServerEndpoint.Port);
                }
            }
            catch { }
        }

        #endregion


        #endregion


        
        #region MFD & HUD Image Swapping

        private static void SetFlightData(FlightData flightData)
        {
            _flightData = flightData;
            if (flightData == null) return;
            if (State.NetworkMode == NetworkMode.Server)
            {
                ExtractorServer.SetFlightData(flightData);
            }
        }
        

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
            SetupThreads();
            StartThreads();

            if (Started != null)
            {
                Started.Invoke(this, new EventArgs());
            }
        }

        private void StartThreads()
        {
            StartAllInstruments();
            StartThread(_simStatusMonitorThread);
            StartThread(_captureOrchestrationThread);
            StartThread(_keyboardWatcherThread);
        }

	    private void StartAllInstruments()
	    {
	        _instruments.Select(i => i.Value).ToList().ForEach(i => i.Start());
	    }
        private void StopAllInstruments()
        {
            Parallel.ForEach(_instruments.Select(i => i.Value).ToList(), x => x.Stop());
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

                Settings.Default.Save();
                var pollingDelay = Settings.Default.PollingDelay;
                while (State.KeepRunning)
                {
                    var thisLoopStartTime = DateTime.Now;
                    Application.DoEvents();

                    ProcessNetworkMessages();
                    if (_terrainDB == null && State.NetworkMode != NetworkMode.Client)
                    {
                        _terrainDB = _terrainDBFactory.Create(false);
                    }

                    if (State.SimRunning || State.OptionsFormIsShowing || State.NetworkMode == NetworkMode.Client)
                    {
                        var currentFlightData = _flightDataRetriever.GetFlightData();
                        SetFlightData(currentFlightData);

                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_instruments, currentFlightData, _terrainDB, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _texSmReader);
                    }
                    else
                    {
                        var flightDataToSet = new FlightData { hsiBits = Int32.MaxValue };
                        SetFlightData(flightDataToSet);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_instruments, flightDataToSet, _terrainDB, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility, _texSmReader);
                    }
                    
                    Application.DoEvents();

                    var thisLoopFinishTime = DateTime.Now;
                    var timeElapsed = thisLoopFinishTime.Subtract(thisLoopStartTime);
                    var millisToSleep = pollingDelay - ((int)timeElapsed.TotalMilliseconds);
                    if (millisToSleep < 5) millisToSleep = 5;
                    if ((!State.SimRunning && State.NetworkMode != NetworkMode.Client) && !State.OptionsFormIsShowing)
                    {
                        millisToSleep += 50;
                    }
                    Thread.Sleep((millisToSleep));

                }
                try
                {
                    Settings.Default.Save();
                }
                catch { }
            }
            catch { }
        }

	    private void ProcessNetworkMessages()
	    {
	        switch (State.NetworkMode)
	        {
	            case NetworkMode.Client:
	                _clientSideIncomingMessageDispatcher.ProcessPendingMessages();
	                break;
	            case NetworkMode.Server:
	                _serverSideIncomingMessageDispatcher.ProcessPendingMessages();
	                break;
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
                        if (count%1 == 0)
                        {
                            count = 0;
							if (State.SimRunning || State.NetworkMode == NetworkMode.Client)
                            {
                                try
                                {
                                    if (_texSmReader == null) _texSmReader = new F4TexSharedMem.Reader();
                                    if (NeedToCaptureMFDsAndOrHud)
                                    {
                                        EnsureThreeDeeCaptureCoordinatesAreLoaded(_flightData !=null ? _flightData.vehicleACD:-1);
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                }
                            }
                            else
                            {
                                ResetThreeDeeCaptureCoordinates();
                            }
                            if (simWasRunning && !State.SimRunning)
                            {
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
                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

	    private void EnsureThreeDeeCaptureCoordinatesAreLoaded(int vehicleACD)
	    {
			if ((_texturesSharedMemoryImageCoordinates.HUD == Rectangle.Empty) &&
				(_texturesSharedMemoryImageCoordinates.LMFD == Rectangle.Empty) &&
				(_texturesSharedMemoryImageCoordinates.RMFD == Rectangle.Empty) &&
				(_texturesSharedMemoryImageCoordinates.MFD3 == Rectangle.Empty) &&
				(_texturesSharedMemoryImageCoordinates.MFD4 == Rectangle.Empty))
	        {
	            _threeDeeCaptureCoordinateUpdater.Update3DCoordinatesFromCurrentBmsDatFile(vehicleACD);
	        }
	    }

	    private void ResetThreeDeeCaptureCoordinates()
	    {
	        _texturesSharedMemoryImageCoordinates.HUD = Rectangle.Empty;
            _texturesSharedMemoryImageCoordinates.LMFD = Rectangle.Empty;
            _texturesSharedMemoryImageCoordinates.RMFD = Rectangle.Empty;
            _texturesSharedMemoryImageCoordinates.MFD3 = Rectangle.Empty;
            _texturesSharedMemoryImageCoordinates.MFD4 = Rectangle.Empty;
        }

	    private static bool NeedToCaptureMFDsAndOrHud
	    {
	        get
	        {
	            return (Settings.Default.EnableLMFDOutput ||
	                Settings.Default.EnableRMFDOutput ||
	                Settings.Default.EnableMfd3Output ||
	                Settings.Default.EnableMfd4Output ||
	                Settings.Default.EnableHudOutput ||
	                State.NetworkMode == NetworkMode.Server);
	        }
	    }

	    private void CloseAndDisposeSharedmemReaders()
        {
            Common.Util.DisposeObject(_texSmReader);
            _texSmReader = null;

        }
        #region Thread Setup

        private void SetupThreads()
        {
            SetupSimStatusMonitorThread();
            SetupCaptureOrchestrationThread();
            SetupKeyboardWatcherThread();
        }

        private void SetupKeyboardWatcherThread()
        {
            Common.Threading.Util.AbortThread(ref _keyboardWatcherThread);
			_keyboardWatcherThread = new Thread(_keyboardWatcher.Start);
            _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
            _keyboardWatcherThread.Priority = ThreadPriority.Highest;
            _keyboardWatcherThread.IsBackground = true;
            _keyboardWatcherThread.Name = "KeyboardWatcherThread";
        }

        private void SetupCaptureOrchestrationThread()
        {
            Common.Threading.Util.AbortThread(ref _captureOrchestrationThread);
            _captureOrchestrationThread = new Thread(CaptureOrchestrationThreadWork)
            {
                Priority = _threadPriority,
                IsBackground = true,
                Name = "CaptureOrchestrationThread"
            };
        }
       

       
        private void SetupSimStatusMonitorThread()
        {
            Common.Threading.Util.AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork)
            {
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true,
                Name = "SimStatusMonitorThread"
            };
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
                    CloseAndDisposeSharedmemReaders();
                    Common.Util.DisposeObject(_texSmReader);
                }
            }
            _disposed = true;
        }

	    ~Extractor()
	    {
	        Dispose(false);
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