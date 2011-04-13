using System;
using System.Windows.Forms;
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
using Common.Win32;
using LightningGauges.Renderers;
using MFDExtractor.Networking;
using Microsoft.DirectX.DirectInput;
using Common.Generic;
using log4net;
namespace MFDExtractor
{
    

    public sealed class Extractor : IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Extractor));
        /// <summary>
        /// Reference to an instance of this class -- this reference is required so that we
        /// can implement the Singleton pattern, which allows only a single instance of this
        /// class to be created as an object, per app-domain
        /// </summary>
        private static Extractor _extractor;
        #endregion
        #region Instance state
        private bool _disposed = false;

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

        private GDIPlusOptions _gdiPlusOptions = new GDIPlusOptions();

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
        private volatile bool _testMode = false;
        private volatile bool _windowSizingOrMoving = false;

        /// <summary>
        /// Reference to the application's main form (for supplying to DirectInput)
        /// </summary>
        private Form _applicationForm = null;
        #endregion

        #region Capture Coordinates
        private CaptureCoordinatesSet _captureCoordinatesSet = new CaptureCoordinatesSet();
        #endregion


        #region Falcon 4 Sharedmem Readers & status flags
        private F4Utils.Terrain.TerrainBrowser _terrainBrowser = new F4Utils.Terrain.TerrainBrowser(false);
        /// <summary>
        /// Reference to a Reader object that can read images from BMS's "textures shared memory" 
        /// area -- this reference is used to perform the actual 3D-mode image extraction
        /// </summary>
        private F4TexSharedMem.Reader _texSmReader = new F4TexSharedMem.Reader();
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

        private InstrumentRenderers _renderers = null;

        private Mediator.PhysicalControlStateChangedEventHandler _mediatorEventHandler = null;
        #endregion

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
            _mediatorEventHandler = new Mediator.PhysicalControlStateChangedEventHandler(Mediator_PhysicalControlStateChanged);
            if (!Properties.Settings.Default.DisableDirectInputMediator)
            {
                this.Mediator = new Mediator(null);
            }
            SetupInstrumentRenderers();
            _settingsSaverAsyncWorker.DoWork += new DoWorkEventHandler(_settingsSaverAsyncWorker_DoWork);
            _settingsLoaderAsyncWorker.DoWork += new DoWorkEventHandler(_settingsLoaderAsyncWorker_DoWork);

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
                (float)((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness +
                ((float)(((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
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
                (float)((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness -
                ((float)(((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
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
            int newBrightness = ((F16ISIS)_renderers.ISISRenderer).InstrumentState.MaxBrightness;
            ((F16ISIS)_renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
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
                    ((float)((F16ISIS)_renderers.ISISRenderer).InstrumentState.MaxBrightness) * 0.5f
                );
            ((F16ISIS)_renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
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
                F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgIncBy1");
                if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useIncrementByOne = true;
                }
            }
            if (useIncrementByOne)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHdgIncBy1");
            }
            else
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHeadingInc");
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
                F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgDecBy1");
                if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useDecrementByOne = true;
                }
            }
            if (useDecrementByOne)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHdgDecBy1");
            }
            else
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHeadingDec");
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
            if (((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness +
                    ((float)(((F16EHSI)_renderers.EHSIRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useIncrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsIncBy1");
                    if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useIncrementByOne = true;
                    }
                }
                if (useIncrementByOne)
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCrsIncBy1");
                }
                else
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCourseInc");
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
            if (((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness -
                    ((float)(((F16EHSI)_renderers.EHSIRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useDecrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsDecBy1");
                    if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useDecrementByOne = true;
                    }
                }
                if (useDecrementByOne)
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCrsDecBy1");
                }
                else
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCourseDec");
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
            F16EHSI.F16EHSIInstrumentState.InstrumentModes currentMode = ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.InstrumentMode;
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
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.InstrumentMode = newMode.Value;
            }
            if (this.NetworkMode == NetworkMode.Standalone || this.NetworkMode == NetworkMode.Server)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimStepHSIMode");
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
            ((F16AirspeedIndicator)_renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots -= 2.5F;
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
            ((F16AirspeedIndicator)_renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots += 2.5F;
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
            ((F16Accelerometer)_renderers.AccelerometerRenderer).InstrumentState.ResetMinAndMaxGs();
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
            InstrumentFormController.NightMode = !InstrumentFormController.NightMode;
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
                case ControlType.Button:
                    return (currentVal.HasValue && currentVal.Value == 1);
                case ControlType.Pov:
                    if (currentVal.HasValue)
                    {
                        return Common.InputSupport.Util.GetPovDirection(currentVal.Value) == hotkey.PovDirection;
                    }
                    else
                    {
                        return false;
                    }
                case ControlType.Key:
                    return (currentVal.HasValue && currentVal.Value == 1);
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
            F4Utils.Process.Util.ResetCurrentKeyFile();

            DateTime beginStartingEventTime = DateTime.Now;
            //Fire the Starting event to all listeners
            if (Starting != null)
            {
                Starting.Invoke(this, new EventArgs());
            }

            if (_keySettingsLoaded == false) LoadKeySettings();
            if (this.Mediator != null)
            {
                this.Mediator.PhysicalControlStateChanged += _mediatorEventHandler;
            }

            SetInstrumentImage(null, "MFD4", _networkMode);
            SetInstrumentImage(null, "MFD3", _networkMode);
            SetInstrumentImage(null, "LMFD", _networkMode);
            SetInstrumentImage(null, "RMFD", _networkMode);
            SetInstrumentImage(null, "HUD", _networkMode);

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
            if (this.Mediator != null)
            {
                this.Mediator.PhysicalControlStateChanged -= _mediatorEventHandler;
            }
            _keySettingsLoaded = false;

            InstrumentFormController.DestroyAll();

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
                _captureCoordinatesSet.MFD4.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_MFD4_2D_ULX, settings.Primary_MFD4_2D_ULY, settings.Primary_MFD4_2D_LRX, settings.Primary_MFD4_2D_LRY);
                _captureCoordinatesSet.MFD3.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_MFD3_2D_ULX, settings.Primary_MFD3_2D_ULY, settings.Primary_MFD3_2D_LRX, settings.Primary_MFD3_2D_LRY);
                _captureCoordinatesSet.LMFD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_LMFD_2D_ULX, settings.Primary_LMFD_2D_ULY, settings.Primary_LMFD_2D_LRX, settings.Primary_LMFD_2D_LRY);
                _captureCoordinatesSet.RMFD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_RMFD_2D_ULX, settings.Primary_RMFD_2D_ULY, settings.Primary_RMFD_2D_LRX, settings.Primary_RMFD_2D_LRY);
                _captureCoordinatesSet.HUD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_HUD_2D_ULX, settings.Primary_HUD_2D_ULY, settings.Primary_HUD_2D_LRX, settings.Primary_HUD_2D_LRY);

                _captureCoordinatesSet.MFD4.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_MFD4_2D_ULX, settings.Secondary_MFD4_2D_ULY, settings.Secondary_MFD4_2D_LRX, settings.Secondary_MFD4_2D_LRY);
                _captureCoordinatesSet.MFD3.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_MFD3_2D_ULX, settings.Secondary_MFD3_2D_ULY, settings.Secondary_MFD3_2D_LRX, settings.Secondary_MFD3_2D_LRY);
                _captureCoordinatesSet.LMFD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_LMFD_2D_ULX, settings.Secondary_LMFD_2D_ULY, settings.Secondary_LMFD_2D_LRX, settings.Secondary_LMFD_2D_LRY);
                _captureCoordinatesSet.RMFD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_RMFD_2D_ULX, settings.Secondary_RMFD_2D_ULY, settings.Secondary_RMFD_2D_LRX, settings.Secondary_RMFD_2D_LRY);
                _captureCoordinatesSet.HUD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_HUD_2D_ULX, settings.Secondary_HUD_2D_ULY, settings.Secondary_HUD_2D_LRX, settings.Secondary_HUD_2D_LRY);
            }
            _captureCoordinatesSet.MFD4.OutputWindowCoords = Rectangle.FromLTRB(settings.MFD4_OutULX, settings.MFD4_OutULY, settings.MFD4_OutLRX, settings.MFD4_OutLRY);
            _captureCoordinatesSet.MFD3.OutputWindowCoords = Rectangle.FromLTRB(settings.MFD3_OutULX, settings.MFD3_OutULY, settings.MFD3_OutLRX, settings.MFD3_OutLRY);
            _captureCoordinatesSet.LMFD.OutputWindowCoords = Rectangle.FromLTRB(settings.LMFD_OutULX, settings.LMFD_OutULY, settings.LMFD_OutLRX, settings.LMFD_OutLRY);
            _captureCoordinatesSet.RMFD.OutputWindowCoords = Rectangle.FromLTRB(settings.RMFD_OutULX, settings.RMFD_OutULY, settings.RMFD_OutLRX, settings.RMFD_OutLRY);
            _captureCoordinatesSet.HUD.OutputWindowCoords = Rectangle.FromLTRB(settings.HUD_OutULX, settings.HUD_OutULY, settings.HUD_OutLRX, settings.HUD_OutLRY);

            _captureCoordinatesSet.MFD4.OutputScreen = Common.Screen.Util.FindScreen(settings.MFD4_OutputDisplay);
            _captureCoordinatesSet.MFD3.OutputScreen= Common.Screen.Util.FindScreen(settings.MFD3_OutputDisplay);
            _captureCoordinatesSet.LMFD.OutputScreen= Common.Screen.Util.FindScreen(settings.LMFD_OutputDisplay);
            _captureCoordinatesSet.RMFD.OutputScreen= Common.Screen.Util.FindScreen(settings.RMFD_OutputDisplay);
            _captureCoordinatesSet.HUD.OutputScreen= Common.Screen.Util.FindScreen(settings.HUD_OutputDisplay);

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
                Networking.ExtractorServer.StoreFlightData(flightData);
            }
        }

        /// <summary>
        /// Makes an image of a specified instrument available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the specified instrument</param>
        private static void SendInstrumentImageToClients(string instrumentName, Image image, NetworkMode networkMode)
        {
            if (networkMode == NetworkMode.Server)
            {
                Networking.ExtractorServer.StoreInstrumentImage(instrumentName, image);
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

        private Image ReadInstrumentImageFromNetwork(string instrumentName)
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetInstrumentImage(instrumentName);
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
            SettingsHelper.SaveAndReloadSettings();
            _settingsSaveScheduled = false;
        }
        #endregion

        #region Capture Implementation 
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
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4 && verInfo != null && ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >= 6826) || (verInfo.ProductMajorPart > 4)))
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
                            if (Properties.Settings.Default.EnableISISOutput)
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

        private static Image ReadRTTImage(Rectangle areaToCapture, F4TexSharedMem.Reader texSharedmemReader)
        {
            Image toReturn = null;
            try
            {
                if (texSharedmemReader != null)
                {
                    toReturn = texSharedmemReader.GetImage(areaToCapture);//Common.Imaging.Util.CloneBitmap();
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            return toReturn;
        }



        /// <summary>
        /// Returns the current MFD #4 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #4 image</returns>
        private Image GetMfd4Bitmap()
        {
            return GetCurrentInstrumentImage(_testMode, _networkMode, _threeDeeMode, _twoDeePrimaryView, _mfd4TestAlignmentImage, _captureCoordinatesSet.MFD4, ()=>ReadInstrumentImageFromNetwork("MFD4"), _texSmReader);
        }
        /// <summary>
        /// Returns the current MFD #3 image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current MFD #3 image</returns>
        private Image GetMfd3Bitmap()
        {
            return GetCurrentInstrumentImage(_testMode, _networkMode, _threeDeeMode, _twoDeePrimaryView, _mfd3TestAlignmentImage, _captureCoordinatesSet.MFD3, () => ReadInstrumentImageFromNetwork("MFD3"), _texSmReader);
        }
        /// <summary>
        /// Returns the current Left MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Left MFD image</returns>
        private Image GetLeftMfdBitmap()
        {
            return GetCurrentInstrumentImage(_testMode, _networkMode, _threeDeeMode, _twoDeePrimaryView, _leftMfdTestAlignmentImage, _captureCoordinatesSet.LMFD, () => ReadInstrumentImageFromNetwork("LMFD"), _texSmReader);
        }
        /// <summary>
        /// Returns the current Right MFD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current Right MFD image</returns>
        private Image GetRightMfdBitmap()
        {
            return GetCurrentInstrumentImage(_testMode, _networkMode, _threeDeeMode, _twoDeePrimaryView, _rightMfdTestAlignmentImage, _captureCoordinatesSet.RMFD, () => ReadInstrumentImageFromNetwork("RMFD"), _texSmReader);
        }
        /// <summary>
        /// Returns the current HUD image from the appropriate source (local screen capture, BMS's 3D shared memory, or from the remote (networked) image server
        /// </summary>
        /// <returns>a Bitmap containing the current HUD image</returns>
        private Image GetHudBitmap()
        {
            return GetCurrentInstrumentImage(_testMode, _networkMode, _threeDeeMode, _twoDeePrimaryView, _hudTestAlignmentImage, _captureCoordinatesSet.HUD, () => ReadInstrumentImageFromNetwork("HUD"), _texSmReader);
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
            InstrumentFormController controller=InstrumentFormController.Instances[instrumentName];
            if (controller == null) return;
            CanvasRenderer renderer = (controller.Renderer as CanvasRenderer);
            if (renderer == null) return;
            renderer.Image = image;
            if (networkMode == NetworkMode.Server)
            {

                SendInstrumentImageToClients(instrumentName, image, networkMode);
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
            InstrumentFormController.Create("MFD4", "MFD #4", _applicationForm, _mfd4BlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.MFD4Renderer);
            InstrumentFormController.Create("MFD3", "MFD #3", _applicationForm, _mfd3BlankImage, new EventHandler((s, e) => { MessageBox.Show("hi"); ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.MFD3Renderer);
            InstrumentFormController.Create("LMFD", "Left MFD", _applicationForm, _leftMfdBlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.LMFDRenderer);
            InstrumentFormController.Create("RMFD", "RMFD", _applicationForm, _rightMfdBlankImage, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RMFDRenderer);
            InstrumentFormController.Create("HUD", "HUD", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HUDRenderer);
            InstrumentFormController.Create("NWSIndexer", "NWS Indexer", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NWSIndexerRenderer);
            InstrumentFormController.Create("AOAIndexer", "AOA Indexer", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AOAIndexerRenderer);
            InstrumentFormController.Create("AOAIndicator", "AOA Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AOAIndicatorRenderer);
            InstrumentFormController.Create("VVI", "VVI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.VVIRenderer);
            InstrumentFormController.Create("ADI", "ADI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ADIRenderer);
            InstrumentFormController.Create("StandbyADI", "StandbyADI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.StandbyADIRenderer);
            InstrumentFormController.Create("ASI", "ASI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ASIRenderer);
            InstrumentFormController.Create("Altimeter", "Altimeter", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AltimeterRenderer);
            InstrumentFormController.Create("CautionPanel", "Caution Panel", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CautionPanelRenderer);
            InstrumentFormController.Create("CMDS", "CMDS", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CMDSPanelRenderer);
            InstrumentFormController.Create("Compass", "Compass", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CompassRenderer);
            InstrumentFormController.Create("DED", "DED", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.DEDRenderer);
            InstrumentFormController.Create("PFL", "PFL", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.PFLRenderer);
            InstrumentFormController.Create("EPUFuel", "EPU Fuel", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.EPUFuelRenderer);
            InstrumentFormController.Create("Accelerometer", "Accelerometer", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.AccelerometerRenderer);
            InstrumentFormController.Create("FTIT1", "Engine 1 FTIT", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FTIT1Renderer);
            InstrumentFormController.Create("FTIT2", "Engine 2 FTIT", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FTIT2Renderer);
            InstrumentFormController.Create("FuelFlow", "Fuel Flow", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FuelFlowRenderer);
            InstrumentFormController.Create("ISIS", "ISIS", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.ISISRenderer);
            InstrumentFormController.Create("FuelQuantity", "Fuel Quantity", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.FuelQuantityRenderer);
            InstrumentFormController.Create("HSI", "HSI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HSIRenderer);
            InstrumentFormController.Create("EHSI", "EHSI", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.EHSIRenderer);
            InstrumentFormController.Create("GearLights", "Gear Lights", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.LandingGearLightsRenderer);
            InstrumentFormController.Create("NOZ1", "Engine 1 - Nozzle Position Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NOZ1Renderer);
            InstrumentFormController.Create("NOZ2", "Engine 2 - Nozzle Position Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.NOZ2Renderer);
            InstrumentFormController.Create("OIL1", "Engine 1 - Oil Pressure Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.OIL1Renderer);
            InstrumentFormController.Create("OIL2", "Engine 2 - Oil Pressure Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.OIL2Renderer);
            InstrumentFormController.Create("RWR", "RWR", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RWRRenderer);
            InstrumentFormController.Create("Speedbrake", "Speedbrake", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.SpeedbrakeRenderer);
            InstrumentFormController.Create("RPM1", "Engine 1 - Tachometer", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RPM1Renderer);
            InstrumentFormController.Create("RPM2", "Engine 2 - Tachometer", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RPM2Renderer);
            InstrumentFormController.Create("HYDA", "HYD A", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HYDARenderer);
            InstrumentFormController.Create("HYDB", "HYD B", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.HYDBRenderer);
            InstrumentFormController.Create("CabinPress", "Cabin Pressure Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.CabinPressRenderer);
            InstrumentFormController.Create("RollTrim", "Roll Trim Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.RollTrimRenderer);
            InstrumentFormController.Create("PitchTrim", "Pitch Trim Indicator", _applicationForm, null, new EventHandler((s, e) => { ScheduleSettingsSaveAndReload(); }), Properties.Settings.Default, _renderers.PitchTrimRenderer);

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
        private void SetupSimStatusMonitorThread()
        {
            AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork);
            _simStatusMonitorThread.Priority = _threadPriority;
            _simStatusMonitorThread.IsBackground = true;
            _simStatusMonitorThread.Name = "SimStatusMonitorThread";
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
                    resetEvent.WaitOne();
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
                        if (!(e is ThreadAbortException))_log.Debug(e.Message, e);
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
                            _networkMode, 
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
                            _networkMode, 
                            _simRunning, 
                            _renderers, 
                            _useBMSAdvancedSharedmemValues, 
                            UpdateEHSIBrightnessLabelVisibility);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_mfd4BlankImage), "MFD4", _networkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_mfd3BlankImage), "MFD3", _networkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_leftMfdBlankImage), "LMFD", _networkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_rightMfdBlankImage), "RMFD", _networkMode);
                        SetInstrumentImage(Common.Imaging.Util.CloneBitmap(_hudBlankImage), "HUD", _networkMode);
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
                        if (!(e is ThreadAbortException))
                        {
                            _log.Error(e.Message.ToString(), e);
                        }
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




        public void RecoverInstrumentForm(string instrumentName, Screen screen)
        {
            InstrumentFormController.RecoverInstrumentForm(instrumentName, screen);
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
                                        if (_texSmReader == null) _texSmReader = new F4TexSharedMem.Reader();
                                        if ((Properties.Settings.Default.EnableHudOutput || NetworkMode == NetworkMode.Server))
                                        {
                                            if (
                                                    (_captureCoordinatesSet.HUD.RTTSourceCoords == Rectangle.Empty) 
                                                        || 
                                                    (_captureCoordinatesSet.LMFD.RTTSourceCoords == Rectangle.Empty) 
                                                        || 
                                                    (_captureCoordinatesSet.RMFD.RTTSourceCoords == Rectangle.Empty) 
                                                        ||
                                                    (_captureCoordinatesSet.MFD3.RTTSourceCoords  == Rectangle.Empty) 
                                                        ||
                                                    (_captureCoordinatesSet.MFD4.RTTSourceCoords == Rectangle.Empty) 
                                             )
                                            {
                                                ReadRTTCoords(_captureCoordinatesSet);
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
                                _captureCoordinatesSet.HUD.RTTSourceCoords = Rectangle.Empty;
                                _captureCoordinatesSet.LMFD.RTTSourceCoords = Rectangle.Empty;
                                _captureCoordinatesSet.RMFD.RTTSourceCoords = Rectangle.Empty;
                                _captureCoordinatesSet.MFD3.RTTSourceCoords = Rectangle.Empty;
                                _captureCoordinatesSet.MFD4.RTTSourceCoords = Rectangle.Empty;
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
        #endregion

        #region RTT support functions
        private static void ReadRTTCoords(Dictionary<string, Rectangle> items)
        {
            FileInfo file = FindBms3DCockpitFile();
            if (file == null)
            {
                return;
            }

            using (FileStream stream = file.OpenRead())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    foreach (string itemName in items.Keys)
                    {
                        if (currentLine.ToLowerInvariant().StartsWith(itemName.ToLowerInvariant()))
                        {
                            Rectangle thisItemRect = new Rectangle();
                            List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                            if (tokens.Count > 12)
                            {
                                try
                                {
                                    thisItemRect.X = Convert.ToInt32(tokens[10]);
                                    thisItemRect.Y = Convert.ToInt32(tokens[11]);
                                    thisItemRect.Width = Math.Abs(Convert.ToInt32(tokens[12]) - thisItemRect.X);
                                    thisItemRect.Height = Math.Abs(Convert.ToInt32(tokens[13]) - thisItemRect.Y);
                                    items[itemName] = thisItemRect;
                                }
                                catch (Exception e)
                                {
                                    _log.Error(e.Message, e);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void ReadRTTCoords(CaptureCoordinatesSet captureCoordinatesSet)
        {
            Dictionary<string,CaptureCoordinates> items= new Dictionary<string,CaptureCoordinates>();
            items.Add("LMFD", captureCoordinatesSet.LMFD);
            items.Add("RMFD", captureCoordinatesSet.RMFD);
            items.Add("MFD3", captureCoordinatesSet.MFD3);
            items.Add("MFD4", captureCoordinatesSet.MFD4);
            items.Add("HUD", captureCoordinatesSet.HUD);
        }
        private static string RunningBmsInstanceBasePath()
        {
            string toReturn = null;
            string exePath = F4Utils.Process.Util.GetFalconExePath();
            if (!string.IsNullOrEmpty(exePath))
            {
                toReturn = new FileInfo(exePath).Directory.FullName;
            }
            else
            {
            }
            return toReturn;
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
            }
            return null;
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
