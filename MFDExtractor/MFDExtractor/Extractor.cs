using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using log4net;
using Common.Networking;
using MFDExtractor.Configuration;
using MFDExtractor.EventSystem.Handlers;
using F4Utils.Terrain;

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
        #region Output Window Coordinates

        private readonly IFlightDataUpdater _flightDataUpdater;

        #endregion

        #region Falcon 4 Sharedmem Readers & status flags

        private F4TexSharedMem.IReader _texSmReader = new F4TexSharedMem.Reader();
        private ITerrainDBFactory _terrainDBFactory = new TerrainDBFactory();
        private TerrainDB _terrainDB;
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
        private Thread _keyboardWatcherThread;
        private Thread _simStatusMonitorThread;

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
		private readonly IThreeDeeCaptureCoordinateUpdater _threeDeeCaptureCoordinateUpdater;
	    private readonly IFlightDataRetriever _flightDataRetriever;
		private readonly SharedMemorySpriteCoordinates _sharedMemorySpriteCoordinates = new SharedMemorySpriteCoordinates();
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
			IThreeDeeCaptureCoordinateUpdater threeDeeCaptureCoordinateUpdater=null,
            IFlightDataRetriever flightDataRetriever= null,
			IFlightDataUpdater flightDataUpdater =null)
        {
            State = new ExtractorState();
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
	        _keyDownEventHandler = keyDownEventHandler ?? new KeyDownEventHandler(_ehsiStateTracker, _inputEvents, _keySettings);
			_keyUpEventHandler = keyUpEventHandler ??  new KeyUpEventHandler(_keySettings, _ehsiStateTracker, _inputEvents);
			_keyboardWatcher = keyboardWatcher ?? new KeyboardWatcher(_keyDownEventHandler, _keyUpEventHandler, Log);
			_clientSideIncomingMessageDispatcher = clientSideIncomingMessageDispatcher ?? new ClientSideIncomingMessageDispatcher(_inputEvents, _client);
			_serverSideIncomingMessageDispatcher = serverSideIncomingMessageDispatcher ?? new ServerSideIncomingMessageDispatcher(_inputEvents);
            _flightDataRetriever = flightDataRetriever ?? new FlightDataRetriever(_client);
			_threeDeeCaptureCoordinateUpdater = threeDeeCaptureCoordinateUpdater ?? new ThreeDeeCaptureCoordinateUpdater(_sharedMemorySpriteCoordinates);
	        _flightDataUpdater = flightDataUpdater ?? new FlightDataUpdater(_texSmReader, _sharedMemorySpriteCoordinates, State);
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
                    _keyboardWatcherThread
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


        
        #region MFD & HUD Image Swapping

        private static void SetFlightData(FlightData flightData)
        {
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
            _rendererSetInitializer.Initialize(_gdiPlusOptions);
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

                    ProcessNetworkMessages();
                    if (_terrainDB == null)
                    {
                        _terrainDB = _terrainDBFactory.Create(false);
                    }

                    if (State.SimRunning || State.TestMode || State.NetworkMode == NetworkMode.Client)
                    {
                        var currentFlightData = _flightDataRetriever.GetFlightData(State);
                        SetFlightData(currentFlightData);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, currentFlightData, _terrainDB, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility);
                    }
                    else
                    {
                        var flightDataToSet = new FlightData {hsiBits = Int32.MaxValue};
                        SetFlightData(flightDataToSet);
                        _flightDataUpdater.UpdateRendererStatesFromFlightData(_renderers, flightDataToSet, _terrainDB, _ehsiStateTracker.UpdateEHSIBrightnessLabelVisibility);
                    }

                    SignalInstrumentRenderThreadsToStart();

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

		private void SignalInstrumentRenderThreadsToStart()
		{
			try
			{
				var toWait = new List<WaitHandle>();
                _instruments[InstrumentType.HUD].Signal(toWait, State);
                _instruments[InstrumentType.LMFD].Signal(toWait, State);
                _instruments[InstrumentType.RMFD].Signal(toWait, State);
                _instruments[InstrumentType.MFD3].Signal(toWait, State);
                _instruments[InstrumentType.MFD4].Signal(toWait, State);



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
				_instruments[InstrumentType.CMDS].Signal(toWait, State);

				_instruments[InstrumentType.GearLights].Signal(toWait, State);
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
                                        EnsureThreeDeeCaptureCoordinatesAreLoaded();
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
            }
            catch (ThreadAbortException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

	    private void EnsureThreeDeeCaptureCoordinatesAreLoaded()
	    {
			if ((_sharedMemorySpriteCoordinates.HUD == Rectangle.Empty) &&
				(_sharedMemorySpriteCoordinates.LMFD == Rectangle.Empty) &&
				(_sharedMemorySpriteCoordinates.RMFD == Rectangle.Empty) &&
				(_sharedMemorySpriteCoordinates.MFD3 == Rectangle.Empty) &&
				(_sharedMemorySpriteCoordinates.MFD4 == Rectangle.Empty))
	        {
	            _threeDeeCaptureCoordinateUpdater.Update3DCoordinatesFromCurrentBmsDatFile();
	        }
	    }

	    private void ResetThreeDeeCaptureCoordinates()
	    {
	        _sharedMemorySpriteCoordinates.HUD = Rectangle.Empty;
            _sharedMemorySpriteCoordinates.LMFD = Rectangle.Empty;
            _sharedMemorySpriteCoordinates.RMFD = Rectangle.Empty;
            _sharedMemorySpriteCoordinates.MFD3 = Rectangle.Empty;
            _sharedMemorySpriteCoordinates.MFD4 = Rectangle.Empty;
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
                    Common.Threading.Util.AbortThread(ref thread);
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

            SetupInstruments();

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
                    Common.Util.DisposeObject(_texSmReader);
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