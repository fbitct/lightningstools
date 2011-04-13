using System;
using System.Windows.Forms;
using log4net;
using MFDExtractor.UI;
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
using System.IO;
using LightningGauges.Renderers;
using MFDExtractor.Networking;
using Microsoft.DirectX.DirectInput;
using Common.Generic;
using Common.Networking;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.Runtime;
using MFDExtractor.Runtime.SimSupport.Falcon4;
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

        #endregion

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
            SetupInstrumentRenderers();
            _networkManager = new NetworkManager(_settingsManager);
            _messageManager = new MessageManager(_renderers, _networkManager);

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
                        showBrightnessLabel = ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel;
                    }
                }
            }
            ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
        }
        private bool EHSIRightKnobIsCurrentlyDepressed()
        {
            return _ehsiRightKnobDepressedTime.HasValue;
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

            if (_keySettingsLoaded == false) LoadKeySettings();
            
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
            
            _keySettingsLoaded = false;

            InstrumentFormController.DestroyAll();

            NetworkManager.TeardownServer();
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
            if (_settingsManager == null) _settingsManager = new SettingsManager();
            _settingsManager.LoadSettings();

        }
        #endregion

        #region Public Properties
       
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
            bool testMode, 
            NetworkMode networkMode, 
            bool threeDeeMode, 
            bool twoDeePrimaryView, 
            Image testAlignmentBitmap, 
            CaptureCoordinates coordinates,
            Func<Image> readBitmapFromNetworkFunc,
            Func<Rectangle, Image> readRTTBitmapFunc
            )
        {
            Image toReturn = null;
            if (testMode)
            {
                toReturn = Common.Imaging.Util.CloneBitmap(testAlignmentBitmap);
            }
            else
            {
                if (_simRunning || networkMode == NetworkMode.Client)
                {
                    if (threeDeeMode && (networkMode == NetworkMode.Server || networkMode== NetworkMode.Standalone))
                    {
                        toReturn = readRTTBitmapFunc(coordinates.RTTSourceCoords);
                    }
                    else
                    {
                        if (networkMode == NetworkMode.Server || networkMode == NetworkMode.Standalone)
                        {
                            toReturn = Common.Screen.Util.CaptureScreenRectangle(twoDeePrimaryView? coordinates.Primary2DModeCoords: coordinates.Secondary2DModeCoords);
                        }
                        else if (networkMode== NetworkMode.Client)
                        {
                            toReturn = readBitmapFromNetworkFunc();
                        }
                    }
                }
            }
            return toReturn;
        }
        private static Image GetCurrentInstrumentImage(bool testMode,NetworkMode networkMode,bool threeDeeMode,bool twoDeePrimaryView,Image testAlignmentBitmap,CaptureCoordinates coordinates,Func<Image> readBitmapFromNetworkFunc,F4TexSharedMem.Reader rttReader)
        {
            return GetCurrentBitmap(testMode, networkMode, threeDeeMode, twoDeePrimaryView, testAlignmentBitmap, coordinates, readBitmapFromNetworkFunc, (coords)=> ReadRTTImage(coords, rttReader));
        }





        /// <summary>
        /// Returns the current MFD #4 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #4 image</returns>
        private Image GetMfd4Bitmap()
        {
            return GetCurrentInstrumentImage(_settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, _mfd4TestAlignmentImage, _captureCoordinatesSet.MFD4, ()=>ReadInstrumentImageFromNetwork("MFD4"), _texSmReader);
        }
        /// <summary>
        /// Returns the current MFD #3 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #3 image</returns>
        private Image GetMfd3Bitmap()
        {
            return GetCurrentInstrumentImage(_settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, _mfd3TestAlignmentImage, _captureCoordinatesSet.MFD3, () => ReadInstrumentImageFromNetwork("MFD3"), _texSmReader);
        }
        /// <summary>
        /// Returns the current Left MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Left MFD image</returns>
        private Image GetLeftMfdBitmap()
        {
            return GetCurrentInstrumentImage(_settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, _leftMfdTestAlignmentImage, _captureCoordinatesSet.LMFD, () => ReadInstrumentImageFromNetwork("LMFD"), _texSmReader);
        }
        /// <summary>
        /// Returns the current Right MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Right MFD image</returns>
        private Image GetRightMfdBitmap()
        {
            return GetCurrentInstrumentImage(_settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, _rightMfdTestAlignmentImage, _captureCoordinatesSet.RMFD, () => ReadInstrumentImageFromNetwork("RMFD"), _texSmReader);
        }
        /// <summary>
        /// Returns the current HUD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current HUD image</returns>
        private Image GetHudBitmap()
        {
            return GetCurrentInstrumentImage(_settingsManager.TestMode, _settingsManager.NetworkMode, _threeDeeMode, _twoDeePrimaryView, _hudTestAlignmentImage, _captureCoordinatesSet.HUD, () => ReadInstrumentImageFromNetwork("HUD"), _texSmReader);
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
                if (_networkMode == NetworkMode.Server)
                {
                    SendFlightData(flightData);
                }
            }
        }
        private static void SetInstrumentImage(Image image, string instrumentName, NetworkMode networkMode)
        {
            if (image== null) return;
            (InstrumentFormController.Instances[instrumentName].Renderer as CanvasRenderer).Image = image;
            if (networkMode == NetworkMode.Server)
            {

                SendInstrumentImageToClients(instrumentName, image, networkMode);
            }
        }
        
        #endregion
        #endregion

        #region Forms Management
        #region Forms Setup
        private void SetupOutputForms(Form mainForm)
        {
            DateTime startTime = DateTime.Now;
            _log.DebugFormat("Started setting up output forms on the extractor at: {0}", startTime.ToString());
            InstrumentFormController.Create("MFD4", "MFD #4", mainForm, _mfd4BlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.MFD4Renderer);
            InstrumentFormController.Create("MFD3", "MFD #3", mainForm, _mfd3BlankImage, new EventHandler((s, e) => { MessageBox.Show("hi"); ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.MFD3Renderer);
            InstrumentFormController.Create("LMFD", "Left MFD", mainForm, _leftMfdBlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.LMFDRenderer);
            InstrumentFormController.Create("RMFD", "RMFD", mainForm, _rightMfdBlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RMFDRenderer);
            InstrumentFormController.Create("HUD", "HUD", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HUDRenderer);
            InstrumentFormController.Create("NWSIndexer", "NWS Indexer", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NWSIndexerRenderer);
            InstrumentFormController.Create("AOAIndexer", "AOA Indexer", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AOAIndexerRenderer);
            InstrumentFormController.Create("AOAIndicator", "AOA Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AOAIndicatorRenderer);
            InstrumentFormController.Create("VVI", "VVI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.VVIRenderer);
            InstrumentFormController.Create("ADI", "ADI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ADIRenderer);
            InstrumentFormController.Create("StandbyADI", "StandbyADI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.StandbyADIRenderer);
            InstrumentFormController.Create("ASI", "ASI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ASIRenderer);
            InstrumentFormController.Create("Altimeter", "Altimeter", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AltimeterRenderer);
            InstrumentFormController.Create("CautionPanel", "Caution Panel", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CautionPanelRenderer);
            InstrumentFormController.Create("CMDS", "CMDS", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CMDSPanelRenderer);
            InstrumentFormController.Create("Compass", "Compass", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CompassRenderer);
            InstrumentFormController.Create("DED", "DED", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.DEDRenderer);
            InstrumentFormController.Create("PFL", "PFL", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.PFLRenderer);
            InstrumentFormController.Create("EPUFuel", "EPU Fuel", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.EPUFuelRenderer);
            InstrumentFormController.Create("Accelerometer", "Accelerometer", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AccelerometerRenderer);
            InstrumentFormController.Create("FTIT1", "Engine 1 FTIT", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FTIT1Renderer);
            InstrumentFormController.Create("FTIT2", "Engine 2 FTIT", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FTIT2Renderer);
            InstrumentFormController.Create("FuelFlow", "Fuel Flow", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FuelFlowRenderer);
            InstrumentFormController.Create("ISIS", "ISIS", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ISISRenderer);
            InstrumentFormController.Create("FuelQuantity", "Fuel Quantity", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FuelQuantityRenderer);
            InstrumentFormController.Create("HSI", "HSI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HSIRenderer);
            InstrumentFormController.Create("EHSI", "EHSI", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.EHSIRenderer);
            InstrumentFormController.Create("GearLights", "Gear Lights", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.LandingGearLightsRenderer);
            InstrumentFormController.Create("NOZ1", "Engine 1 - Nozzle Position Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NOZ1Renderer);
            InstrumentFormController.Create("NOZ2", "Engine 2 - Nozzle Position Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NOZ2Renderer);
            InstrumentFormController.Create("OIL1", "Engine 1 - Oil Pressure Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.OIL1Renderer);
            InstrumentFormController.Create("OIL2", "Engine 2 - Oil Pressure Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.OIL2Renderer);
            InstrumentFormController.Create("RWR", "RWR", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RWRRenderer);
            InstrumentFormController.Create("Speedbrake", "Speedbrake", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.SpeedbrakeRenderer);
            InstrumentFormController.Create("RPM1", "Engine 1 - Tachometer", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RPM1Renderer);
            InstrumentFormController.Create("RPM2", "Engine 2 - Tachometer", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RPM2Renderer);
            InstrumentFormController.Create("HYDA", "HYD A", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HYDARenderer);
            InstrumentFormController.Create("HYDB", "HYD B", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HYDBRenderer);
            InstrumentFormController.Create("CabinPress", "Cabin Pressure Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CabinPressRenderer);
            InstrumentFormController.Create("RollTrim", "Roll Trim Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RollTrimRenderer);
            InstrumentFormController.Create("PitchTrim", "Pitch Trim Indicator", mainForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.PitchTrimRenderer);

            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Finished setting up output forms on the extractor at: {0}", endTime.ToString());
            _log.DebugFormat("Time taken to set up output forms on the extractor: {0}", elapsed.TotalMilliseconds);

        }
        private void ScheduleSettingsSaveAndReload()
        {
            if (!_testMode)
            {
                _settingsSaveScheduled = true;
            }
            _settingsLoadScheduled = true;
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
            _simStatusMonitorThread.Start();
            _keyboardWatcherThread.Start();
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
                GDIPlusOptions = _gdiPlusOptions,
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
            SetupSimStatusMonitorThread();
            SetupCaptureOrchestrationThread();
            SetupKeyboardWatcherThread();

            DateTime endTime = DateTime.Now;
            _log.DebugFormat("Finished setting up threads at: {0}", endTime.ToString());
            TimeSpan elapsed = endTime.Subtract(startTime);
            _log.DebugFormat("Time taken setting up threads: {0}", elapsed.TotalMilliseconds);

        }


        private void SetupCaptureOrchestrationThread()
        {
            AbortThread(ref _captureOrchestrationThread);
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
                    _windowSizingOrMoving = InstrumentFormController.IsWindowSizingOrMovingBeingAttemptedOnAnyOutputWindow();
                    Application.DoEvents();
                    if (_settingsSaveScheduled && !_windowSizingOrMoving)
                    {
                        SaveSettingsAsync();
                    }
                    if (_settingsLoadScheduled && !_windowSizingOrMoving)
                    {
                        LoadSettingsAsync();
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

                        FlightDataToRendererStateTranslator.UpdateRendererStatesFromFlightData(
                            _flightData,
                            _settingsManager.NetworkMode, 
                            _simRunning, 
                            _renderers, 
                            _useBMSAdvancedSharedmemValues, 
                            UpdateEHSIBrightnessLabelVisibility);
                    }
                    else
                    {
                        FlightData toSet = new FlightData();
                        toSet.hsiBits = Int32.MaxValue;
                        SetFlightData(toSet);
                        FlightDataToRendererStateTranslator.UpdateRendererStatesFromFlightData(
                            _flightData, 
                            _settingsManager.NetworkMode, 
                            _simRunning, 
                            _renderers, 
                            _useBMSAdvancedSharedmemValues, 
                            UpdateEHSIBrightnessLabelVisibility);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_mfd4BlankImage), "MFD4", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_mfd3BlankImage), "MFD3", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_leftMfdBlankImage), "LMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_rightMfdBlankImage), "RMFD", _settingsManager.NetworkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_hudBlankImage), "HUD", _settingsManager.NetworkMode);
                        setNullImages = false;
                    }

                    try
                    {
                        toWait = new List<WaitHandle>();
                        InstrumentFormController.RenderAll();
                        //performance group 0
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
                    if ((!_simRunning && !(_settingsManager.NetworkMode == NetworkMode.Client)) && !_settingsManager.TestMode)
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
            finally
            {
                _windowSizingOrMoving = false;
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
                    Common.Util.DisposeObject(_settingsManager);                    
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
