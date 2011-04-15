using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Common.Generic;
using Common.InputSupport.DirectInput;
using Common.Networking;
using F4SharedMem;
using LightningGauges.Renderers;
using log4net;
using MFDExtractor.Runtime;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.Runtime.SimSupport.Falcon4;
using MFDExtractor.UI;
namespace MFDExtractor
{
    public sealed class Extractor : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(typeof(Extractor));
        #region Extractor state
        private bool _disposed = false;
        /// <summary>
        /// Reference to an instance of this class -- this reference is required so that we
        /// can implement the Singleton pattern, which allows only a single instance of this
        /// class to be created as an object, per app-domain
        /// </summary>
        private static Extractor _extractor;
        #region Public Property Backing Fields
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

        private object _flightData = null;
        #endregion

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




        #region Threads

        /// <summary>
        /// Referencce to the thread that is responsible for orchestrating the image capture sequence
        /// </summary>
        private Thread _captureOrchestrationThread = null;



        private InstrumentRenderers _renderers = null;
        private MessageManager _messageManager= null;
        private SettingsManager _settingsManager= null;
        private NetworkManager _networkManager = null;
        private Falcon4SimSupport _simSupport = null;
        private InputSupport _inputSupport = null;
        private FormManager _formManager = null;
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
            Initialize();
        }

        private void Initialize()
        {
            //load user settings when an instance of the Extractor engine is created by
            //one of the Factory methods
            _settingsManager = new SettingsManager();
            LoadSettings();
            SetupInstrumentRenderers();
            _networkManager = new NetworkManager(_settingsManager);
            _simSupport = new Falcon4SimSupport(_settingsManager, _networkManager, this);
            _messageManager = new MessageManager(_renderers, _networkManager, _settingsManager, _simSupport);
            _inputSupport = new InputSupport(_settingsManager, _messageManager, this);
            _formManager = new FormManager(_settingsManager, _renderers);
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
            F4Utils.Process.Util.ResetCurrentKeyFile();

            DateTime beginStartingEventTime = DateTime.Now;
            //Fire the Starting event to all listeners
            if (Starting != null)
            {
                Starting.Invoke(this, new EventArgs());
            }

            _settingsManager.KeySettings.Reload();
            
            SetInstrumentImage(null, "MFD4", _settingsManager.NetworkMode);
            SetInstrumentImage(null, "MFD3", _settingsManager.NetworkMode);
            SetInstrumentImage(null, "LMFD", _settingsManager.NetworkMode);
            SetInstrumentImage(null, "RMFD", _settingsManager.NetworkMode);
            SetInstrumentImage(null, "HUD", _settingsManager.NetworkMode);

            RunThreads();

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
            
            InstrumentFormController.DestroyAll();

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
            _settingsManager.LoadSettings();

        }
        #endregion

        #region Public Properties

        public Mediator Mediator
        {
            get { return _inputSupport.Mediator; }
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
                return _settingsManager.TestMode;
            }
            set
            {
                _settingsManager.TestMode = value;
                Properties.Settings.Default.TestMode = _settingsManager.TestMode;
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
        #endregion


        #region Capture Implementation 
        #region Capture Strategy Orchestration Methods

        /// <summary>
        /// Returns the current image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #4 image</returns>
        private static Image GetCurrentBitmap(
            bool simRunning,
            bool testMode, 
            NetworkMode networkMode, 
            bool threeDeeMode, 
            bool twoDeePrimaryView, 
            Image testAlignmentBitmap, 
            CaptureCoordinates coordinates,
            NetworkManager networkManager, 
            string instrumentName,
            Falcon4SimSupport simSupport
            )
        {
            Image toReturn = null;
            if (testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(testAlignmentBitmap);
            }
            else
            {
                if (simRunning || networkMode == NetworkMode.Client)
                {
                    if (threeDeeMode && (networkMode == NetworkMode.Server || networkMode== NetworkMode.Standalone))
                    {
                        toReturn = simSupport.ReadRTTImage(coordinates.RTTSourceCoords);
                    }
                    else
                    {
                        if (networkMode == NetworkMode.Server || networkMode == NetworkMode.Standalone)
                        {
                            toReturn = Common.Screen.Util.CaptureScreenRectangle(twoDeePrimaryView? coordinates.Primary2DModeCoords: coordinates.Secondary2DModeCoords);
                        }
                        else if (networkMode== NetworkMode.Client)
                        {
                            toReturn = networkManager.ReadInstrumentImageFromServer(instrumentName);
                        }
                    }
                }
            }
            return toReturn;
        }
        private static Image GetCurrentInstrumentImage(bool simRunning, bool testMode,NetworkMode networkMode,bool threeDeeMode,bool twoDeePrimaryView,Image testAlignmentBitmap,CaptureCoordinates coordinates,NetworkManager networkManager, Falcon4SimSupport simSupport, string instrumentName)
        {
            return GetCurrentBitmap(simRunning, testMode, networkMode, threeDeeMode, twoDeePrimaryView, testAlignmentBitmap, coordinates, networkManager, instrumentName, simSupport);
        }

        /// <summary>
        /// Returns the current MFD #4 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #4 image</returns>
        private Image GetMfd4Bitmap(CaptureCoordinatesSet coordinatesSet)
        {
            return GetCurrentInstrumentImage(_simSupport.IsSimRunning, _settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, BlankAndTestImages.Mfd4TestAlignmentImage, coordinatesSet.MFD4, _networkManager, _simSupport, "MFD4");
        }
        /// <summary>
        /// Returns the current MFD #3 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #3 image</returns>
        private Image GetMfd3Bitmap(CaptureCoordinatesSet coordinatesSet)
        {
            return GetCurrentInstrumentImage(_simSupport.IsSimRunning, _settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, BlankAndTestImages.Mfd3TestAlignmentImage, coordinatesSet.MFD3, _networkManager, _simSupport,"MFD3");
        }
        /// <summary>
        /// Returns the current Left MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Left MFD image</returns>
        private Image GetLeftMfdBitmap(CaptureCoordinatesSet coordinatesSet)
        {
            return GetCurrentInstrumentImage(_simSupport.IsSimRunning, _settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, BlankAndTestImages.LeftMfdTestAlignmentImage, coordinatesSet.LMFD, _networkManager, _simSupport,"LMFD");
        }
        /// <summary>
        /// Returns the current Right MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Right MFD image</returns>
        private Image GetRightMfdBitmap(CaptureCoordinatesSet coordinatesSet)
        {
            return GetCurrentInstrumentImage(_simSupport.IsSimRunning, _settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, BlankAndTestImages.RightMfdTestAlignmentImage, coordinatesSet.RMFD, _networkManager, _simSupport,"RMFD");
        }
        /// <summary>
        /// Returns the current HUD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current HUD image</returns>
        private Image GetHudBitmap(CaptureCoordinatesSet coordinatesSet)
        {
            return GetCurrentInstrumentImage(_simSupport.IsSimRunning, _settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, BlankAndTestImages.HudTestAlignmentImage, coordinatesSet.HUD, _networkManager, _simSupport,"HUD");
        }
        
        #endregion
        /// <summary>
        /// Captures and stores an image
        /// </summary>
        private void CaptureAndSetImage(bool enableOutput, string instrumentName, NetworkMode networkMode, 
            Func<Image> imageDelegate, Func<Image> blankImageDelegate, PerformanceCounter perfCounter )
        {
            if (enableOutput || networkMode == NetworkMode.Server)
            {
                Image image = null;
                try
                {
                    image = imageDelegate();
                    if (image == null)
                    {
                        image= Common.Imaging.Util.CloneBitmap(blankImageDelegate());
                    }
                    SetInstrumentImage(image, instrumentName, networkMode);
                    if (perfCounter != null)
                    {
                        perfCounter.Increment();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message.ToString(), e);
                }
                finally
                {
                    Common.Util.DisposeObject(image);
                }
            }
        }
        #region MFD & HUD Image Swapping

        private void SetFlightData(FlightData flightData)
        {
            if (flightData == null) return;
            lock (flightData)
            {
                _flightData = flightData;
                if (_settingsManager.NetworkMode == NetworkMode.Server)
                {
                    _networkManager.SendFlightDataToClients(flightData);
                }
            }
        }
        private void SetInstrumentImage(Image image, string instrumentName, NetworkMode networkMode)
        {
            if (image== null) return;
            if (InstrumentFormController.Instances.ContainsKey(instrumentName))
            {
                (InstrumentFormController.Instances[instrumentName].Renderer as CanvasRenderer).Image = image;
            }
            if (networkMode == NetworkMode.Server)
            {

                _networkManager.SendInstrumentImageToClients(instrumentName, image, networkMode);
            }
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
                _networkManager.SetupNetworking();
                _keepRunning = true;

                SetupInstrumentRenderers();
                _formManager.SetupOutputForms();
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
            _captureOrchestrationThread.Start();
        }

        private void SetupInstrumentRenderers()
        {
            InstrumentRenderers.InitializationParams instrumentRendererInitializationParms = new InstrumentRenderers.InitializationParams()
            {
                AltimeterPressureUnitsProperty = new PropertyInvoker<string>("Altimeter_PressureUnits", Properties.Settings.Default),
                AltimeterStyleProperty = new PropertyInvoker<string>("Altimeter_Style", Properties.Settings.Default),
                AzimuthIndicatorTypeProperty = new PropertyInvoker<string>("AzimuthIndicatorType", Properties.Settings.Default),
                FuelQuantityIndicatorNeedleIsCModelProperty = new PropertyInvoker<bool>("FuelQuantityIndicator_NeedleCModel", Properties.Settings.Default),
                GDIPlusOptions = _settingsManager.GDIPlusOptions,
                ISISPressureUnitsProperty = new PropertyInvoker<string>("ISIS_PressureUnits", Properties.Settings.Default),
                ShowAzimuthIndicatorBezelProperty = new PropertyInvoker<bool>("AzimuthIndicator_ShowBezel", Properties.Settings.Default),
                VVIStyleProperty = new PropertyInvoker<string>("VVI_Style", Properties.Settings.Default)
            };
            _renderers = new InstrumentRenderers(instrumentRendererInitializationParms);
        }
        
        #region Thread Setup
        private void SetupThreads()
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Starting setting up threads at: {0}", startTime.ToString());
            SetupCaptureOrchestrationThread();

            DateTime endTime = DateTime.Now;
            _log.DebugFormat("Finished setting up threads at: {0}", endTime.ToString());
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Time taken setting up threads: {0}", elapsed.TotalMilliseconds);

        }


        private void SetupCaptureOrchestrationThread()
        {
            Common.Threading.Util.AbortThread(ref _captureOrchestrationThread);
            _captureOrchestrationThread = new Thread(CaptureOrchestrationThreadWork);
            _captureOrchestrationThread.Priority = _settingsManager.ThreadPriority;
            _captureOrchestrationThread.IsBackground = true;
            _captureOrchestrationThread.Name = "CaptureOrchestrationThread";
        }

        
        #endregion
        #region Gauges rendering thread-work methods
        
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
       
        #endregion
       

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
                    bool windowSizingOrMoving = InstrumentFormController.IsWindowSizingOrMovingBeingAttemptedOnAnyOutputWindow();

                    Application.DoEvents();
                    if (!windowSizingOrMoving)
                    {
                        _settingsManager.SaveSettingsAsync();
                    }
                    DateTime thisLoopStartTime = DateTime.Now;

                    _messageManager.ProcessPendingMessages();

                    if (_simSupport.IsSimRunning || _settingsManager.TestMode || _settingsManager.NetworkMode == NetworkMode.Client)
                    {
                        FlightData current = _simSupport.GetFlightData();
                        SetFlightData(current);

                        FlightDataToRendererStateTranslator.UpdateRendererStatesFromFlightData(
                            (FlightData)_flightData,
                            _settingsManager.NetworkMode,
                            _simSupport.IsSimRunning,
                            _renderers,
                            _simSupport.UseBMSAdvancedSharedmemValues,
                            _messageManager.UpdateEHSIBrightnessLabelVisibility);

                        SetInstrumentImage(GetMfd4Bitmap(_settingsManager.CaptureCoordinatesSet) , "MFD4", _settingsManager.NetworkMode);
                        SetInstrumentImage(GetMfd3Bitmap(_settingsManager.CaptureCoordinatesSet), "MFD3", _settingsManager.NetworkMode);
                        SetInstrumentImage(GetLeftMfdBitmap(_settingsManager.CaptureCoordinatesSet), "LMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(GetRightMfdBitmap(_settingsManager.CaptureCoordinatesSet), "RMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(GetHudBitmap(_settingsManager.CaptureCoordinatesSet), "HUD", _settingsManager.NetworkMode);

                    }
                    else
                    {
                        FlightData toSet = new FlightData();
                        toSet.hsiBits = Int32.MaxValue;
                        SetFlightData(toSet);
                        FlightDataToRendererStateTranslator.UpdateRendererStatesFromFlightData(
                            (FlightData)_flightData,
                            _settingsManager.NetworkMode,
                            _simSupport.IsSimRunning,
                            _renderers,
                            _simSupport.UseBMSAdvancedSharedmemValues,
                            _messageManager.UpdateEHSIBrightnessLabelVisibility);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(BlankAndTestImages.Mfd4BlankImage), "MFD4", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(BlankAndTestImages.Mfd3BlankImage), "MFD3", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(BlankAndTestImages.LeftMfdBlankImage), "LMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(BlankAndTestImages.RightMfdBlankImage), "RMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(BlankAndTestImages.HudBlankImage), "HUD", _settingsManager.NetworkMode);
                    }

                    try
                    {
                        toWait = new List<WaitHandle>();
                        InstrumentFormController.RenderAll();
                        //performance group 0
                        Common.Threading.Util.WaitAllHandlesInListAndClearList(toWait, 1000);
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
                    if (_settingsManager.TestMode) millisToSleep = 500;
                    DateTime sleepUntil = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, millisToSleep));
                    while (DateTime.Now < sleepUntil)
                    {
                        int millisRemaining = (int)Math.Floor(DateTime.Now.Subtract(sleepUntil).TotalMilliseconds);
                        int millisWaited = millisRemaining >= 5 ? 5 : 1;
                        Thread.Sleep(millisWaited);
                        Application.DoEvents();
                    }
                    Application.DoEvents();
                    if ((!_simSupport.IsSimRunning && !(_settingsManager.NetworkMode == NetworkMode.Client)) && !_settingsManager.TestMode)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5); //sleep an additional half-second or so here if we're not a client and there's no sim running and we're not in test mode
                    }
                    else if (_settingsManager.TestMode)
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

        public void RecoverInstrumentForm(string instrumentName, Screen screen)
        {
            InstrumentFormController.RecoverInstrumentForm(instrumentName, screen);
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

                    Common.Util.DisposeObject(_simSupport);
                    _simSupport = null;

                    Common.Util.DisposeObject(_inputSupport);
                    _simSupport = null;

                    Common.Util.DisposeObject(_networkManager);
                    _simSupport = null;

                    Common.Threading.Util.AbortThread(ref _captureOrchestrationThread);
                    _captureOrchestrationThread = null;
                    Common.Util.DisposeObject(_settingsManager);                    
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
