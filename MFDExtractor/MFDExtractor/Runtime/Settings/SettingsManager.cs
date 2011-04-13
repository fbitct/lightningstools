using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Common.Networking;
using System.Threading;
using System.Net;
using System.Drawing;
using Common.UI;

namespace MFDExtractor.Runtime.Settings
{
    internal class SettingsManager:IDisposable
    {
        #region Instance variables
        private bool _disposed = false;

        private volatile bool _settingsSaveScheduled = false;
        private volatile bool _settingsLoadScheduled = false;
        private BackgroundWorker _settingsSaverAsyncWorker = new BackgroundWorker();
        private BackgroundWorker _settingsLoaderAsyncWorker = new BackgroundWorker();

        private CaptureCoordinatesSet _captureCoordinatesSet = new CaptureCoordinatesSet();
        private NetworkMode _networkMode = NetworkMode.Standalone;
        private bool _testMode = false;
        private GDIPlusOptions _gdiPlusOptions = null;
        private string _imageFormat = "PNG";
        private string _compressionType = "None";
        private IPEndPoint _serverEndpoint = null;
        private string _serviceName = "MFDExtractorService";
        private KeySettings _keySettings = null;
        /// <summary>
        /// Thread priority at which the Extractor worker threads should run
        /// </summary>
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;

        #endregion

        #region Public Properties
        public CaptureCoordinatesSet CaptureCoordinatesSet { get { return _captureCoordinatesSet; } }
        public NetworkMode NetworkMode { get { return _networkMode; } set { _networkMode = value; } }
        public bool TestMode { get { return _testMode; } set { _testMode = value; } }
        private GDIPlusOptions GDIPlusOptions { get { return _gdiPlusOptions; } set { _gdiPlusOptions = value; } }
        public string CompressionType { get { return _compressionType; } set { _compressionType = value; } }
        public string ImageFormat { get { return _imageFormat; } set { _imageFormat = value; } }
        public IPEndPoint ServerEndpont { get { return _serverEndpoint; } set { _serverEndpoint = value; } }
        public string ServiceName { get { return _serviceName; } set { _serviceName = value; } }
        public ThreadPriority ThreadPriority { get { return _threadPriority; } set { _threadPriority = value; } }
        public KeySettings KeySettings { get { return _keySettings; } set { _keySettings = value; } }
        #endregion

        #region Constructors
        public SettingsManager()
            : base()
        {
            RegisterBackgroundWorkerFunctions();
        }
        #endregion

        #region Background worker functions
        private void RegisterBackgroundWorkerFunctions()
        {
            _settingsSaverAsyncWorker.DoWork += new DoWorkEventHandler(_settingsSaverAsyncWorker_DoWork);
            _settingsLoaderAsyncWorker.DoWork += new DoWorkEventHandler(_settingsLoaderAsyncWorker_DoWork);
        }
        private void UnregisterBackgroundWorkerFunctions()
        {
            _settingsSaverAsyncWorker.DoWork -= _settingsSaverAsyncWorker_DoWork;
            _settingsLoaderAsyncWorker.DoWork -= _settingsLoaderAsyncWorker_DoWork;
        }
        private void _settingsLoaderAsyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadSettings();
            _settingsLoadScheduled = false;
        }

        private void _settingsSaverAsyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            _settingsSaveScheduled = false;
        }
        #endregion

        #region Settings Loaders and Savers
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
        private void LoadGDIPlusSettings()
        {
            _gdiPlusOptions = new GDIPlusOptions();
            _gdiPlusOptions.CompositingQuality = Properties.Settings.Default.CompositingQuality;
            _gdiPlusOptions.InterpolationMode = Properties.Settings.Default.InterpolationMode;
            _gdiPlusOptions.PixelOffsetMode = Properties.Settings.Default.PixelOffsetMode;
            _gdiPlusOptions.SmoothingMode = Properties.Settings.Default.SmoothingMode;
            _gdiPlusOptions.TextRenderingHint = Properties.Settings.Default.TextRenderingHint;
        }
        private void LoadKeySettings()
        {
            if (_keySettings == null)
            {
                _keySettings = new KeySettings();
            }
            else
            {
                _keySettings.Reload();
            }

        }
        /// <summary>
        /// Reads the user settings file from disk or the current in-memory user settings cache
        /// </summary>
        public void LoadSettings()
        {
            LoadKeySettings();
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
            _captureCoordinatesSet.MFD3.OutputScreen = Common.Screen.Util.FindScreen(settings.MFD3_OutputDisplay);
            _captureCoordinatesSet.LMFD.OutputScreen = Common.Screen.Util.FindScreen(settings.LMFD_OutputDisplay);
            _captureCoordinatesSet.RMFD.OutputScreen = Common.Screen.Util.FindScreen(settings.RMFD_OutputDisplay);
            _captureCoordinatesSet.HUD.OutputScreen = Common.Screen.Util.FindScreen(settings.HUD_OutputDisplay);

            _testMode = settings.TestMode;
            _threadPriority = settings.ThreadPriority;
            _compressionType = settings.CompressionType;
            _imageFormat = settings.NetworkImageFormat;
            
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
                }
            }
            _disposed = true;
        }
        #endregion

    }
}
