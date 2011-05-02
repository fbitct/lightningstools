﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Threading;
using Common.Networking;
using Common.UI;
using Util = Common.Screen.Util;

namespace MFDExtractor.Runtime.Settings
{
    internal class SettingsManager : IDisposable
    {
        #region Instance variables

        private readonly CaptureCoordinatesSet _captureCoordinatesSet = new CaptureCoordinatesSet();
        private readonly BackgroundWorker _settingsLoaderAsyncWorker = new BackgroundWorker();
        private readonly BackgroundWorker _settingsSaverAsyncWorker = new BackgroundWorker();
        private string _compressionType = "None";
        private bool _disposed;
        private GDIPlusOptions _gdiPlusOptions;
        private string _imageFormat = "PNG";
        private KeySettings _keySettings;
        private NetworkMode _networkMode = NetworkMode.Standalone;
        private IPEndPoint _serverEndpoint;
        private string _serviceName = "MFDExtractorService";
        private bool _testMode;

        /// <summary>
        /// Thread priority at which the Extractor worker threads should run
        /// </summary>
        private ThreadPriority _threadPriority = ThreadPriority.BelowNormal;

        #endregion

        #region Public Properties

        public CaptureCoordinatesSet CaptureCoordinatesSet
        {
            get { return _captureCoordinatesSet; }
        }

        public NetworkMode NetworkMode
        {
            get { return _networkMode; }
            set { _networkMode = value; }
        }

        public bool TestMode
        {
            get { return _testMode; }
            set { _testMode = value; }
        }

        public GDIPlusOptions GDIPlusOptions
        {
            get { return _gdiPlusOptions; }
            set { _gdiPlusOptions = value; }
        }

        public string CompressionType
        {
            get { return _compressionType; }
            set { _compressionType = value; }
        }

        public string ImageFormat
        {
            get { return _imageFormat; }
            set { _imageFormat = value; }
        }

        public IPEndPoint ServerEndpont
        {
            get { return _serverEndpoint; }
            set { _serverEndpoint = value; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        public ThreadPriority ThreadPriority
        {
            get { return _threadPriority; }
            set { _threadPriority = value; }
        }

        public KeySettings KeySettings
        {
            get { return _keySettings; }
            set { _keySettings = value; }
        }

        #endregion

        #region Constructors

        public SettingsManager()
        {
            RegisterBackgroundWorkerFunctions();
        }

        #endregion

        #region Background worker functions

        private void RegisterBackgroundWorkerFunctions()
        {
            _settingsSaverAsyncWorker.DoWork += SettingsSaverAsyncWorkerDoWork;
            _settingsLoaderAsyncWorker.DoWork += SettingsLoaderAsyncWorkerDoWork;
        }

        private void UnregisterBackgroundWorkerFunctions()
        {
            _settingsSaverAsyncWorker.DoWork -= SettingsSaverAsyncWorkerDoWork;
            _settingsLoaderAsyncWorker.DoWork -= SettingsLoaderAsyncWorkerDoWork;
        }

        private void SettingsLoaderAsyncWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            LoadSettings();
        }

        private static void SettingsSaverAsyncWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        #endregion

        #region Settings Loaders and Savers

        public void LoadSettingsAsync()
        {
            _settingsLoaderAsyncWorker.RunWorkerAsync();
        }

        public void SaveSettingsAsync()
        {
            if (!_settingsSaverAsyncWorker.IsBusy)
            {
                _settingsSaverAsyncWorker.RunWorkerAsync();
            }
        }

        private void LoadGDIPlusSettings()
        {
            _gdiPlusOptions = new GDIPlusOptions
                                  {
                                      CompositingQuality = Properties.Settings.Default.CompositingQuality,
                                      InterpolationMode = Properties.Settings.Default.InterpolationMode,
                                      PixelOffsetMode = Properties.Settings.Default.PixelOffsetMode,
                                      SmoothingMode = Properties.Settings.Default.SmoothingMode,
                                      TextRenderingHint = Properties.Settings.Default.TextRenderingHint
                                  };
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
            var settings = Properties.Settings.Default;
            LoadGDIPlusSettings();
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
                _captureCoordinatesSet.MFD4.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_MFD4_2D_ULX,
                                                                                     settings.Primary_MFD4_2D_ULY,
                                                                                     settings.Primary_MFD4_2D_LRX,
                                                                                     settings.Primary_MFD4_2D_LRY);
                _captureCoordinatesSet.MFD3.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_MFD3_2D_ULX,
                                                                                     settings.Primary_MFD3_2D_ULY,
                                                                                     settings.Primary_MFD3_2D_LRX,
                                                                                     settings.Primary_MFD3_2D_LRY);
                _captureCoordinatesSet.LMFD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_LMFD_2D_ULX,
                                                                                     settings.Primary_LMFD_2D_ULY,
                                                                                     settings.Primary_LMFD_2D_LRX,
                                                                                     settings.Primary_LMFD_2D_LRY);
                _captureCoordinatesSet.RMFD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_RMFD_2D_ULX,
                                                                                     settings.Primary_RMFD_2D_ULY,
                                                                                     settings.Primary_RMFD_2D_LRX,
                                                                                     settings.Primary_RMFD_2D_LRY);
                _captureCoordinatesSet.HUD.Primary2DModeCoords = Rectangle.FromLTRB(settings.Primary_HUD_2D_ULX,
                                                                                    settings.Primary_HUD_2D_ULY,
                                                                                    settings.Primary_HUD_2D_LRX,
                                                                                    settings.Primary_HUD_2D_LRY);

                _captureCoordinatesSet.MFD4.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_MFD4_2D_ULX,
                                                                                       settings.Secondary_MFD4_2D_ULY,
                                                                                       settings.Secondary_MFD4_2D_LRX,
                                                                                       settings.Secondary_MFD4_2D_LRY);
                _captureCoordinatesSet.MFD3.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_MFD3_2D_ULX,
                                                                                       settings.Secondary_MFD3_2D_ULY,
                                                                                       settings.Secondary_MFD3_2D_LRX,
                                                                                       settings.Secondary_MFD3_2D_LRY);
                _captureCoordinatesSet.LMFD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_LMFD_2D_ULX,
                                                                                       settings.Secondary_LMFD_2D_ULY,
                                                                                       settings.Secondary_LMFD_2D_LRX,
                                                                                       settings.Secondary_LMFD_2D_LRY);
                _captureCoordinatesSet.RMFD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_RMFD_2D_ULX,
                                                                                       settings.Secondary_RMFD_2D_ULY,
                                                                                       settings.Secondary_RMFD_2D_LRX,
                                                                                       settings.Secondary_RMFD_2D_LRY);
                _captureCoordinatesSet.HUD.Secondary2DModeCoords = Rectangle.FromLTRB(settings.Secondary_HUD_2D_ULX,
                                                                                      settings.Secondary_HUD_2D_ULY,
                                                                                      settings.Secondary_HUD_2D_LRX,
                                                                                      settings.Secondary_HUD_2D_LRY);
            }
            _captureCoordinatesSet.MFD4.OutputWindowCoords = Rectangle.FromLTRB(settings.MFD4_OutULX,
                                                                                settings.MFD4_OutULY,
                                                                                settings.MFD4_OutLRX,
                                                                                settings.MFD4_OutLRY);
            _captureCoordinatesSet.MFD3.OutputWindowCoords = Rectangle.FromLTRB(settings.MFD3_OutULX,
                                                                                settings.MFD3_OutULY,
                                                                                settings.MFD3_OutLRX,
                                                                                settings.MFD3_OutLRY);
            _captureCoordinatesSet.LMFD.OutputWindowCoords = Rectangle.FromLTRB(settings.LMFD_OutULX,
                                                                                settings.LMFD_OutULY,
                                                                                settings.LMFD_OutLRX,
                                                                                settings.LMFD_OutLRY);
            _captureCoordinatesSet.RMFD.OutputWindowCoords = Rectangle.FromLTRB(settings.RMFD_OutULX,
                                                                                settings.RMFD_OutULY,
                                                                                settings.RMFD_OutLRX,
                                                                                settings.RMFD_OutLRY);
            _captureCoordinatesSet.HUD.OutputWindowCoords = Rectangle.FromLTRB(settings.HUD_OutULX, settings.HUD_OutULY,
                                                                               settings.HUD_OutLRX, settings.HUD_OutLRY);

            _captureCoordinatesSet.MFD4.OutputScreen = Util.FindScreen(settings.MFD4_OutputDisplay);
            _captureCoordinatesSet.MFD3.OutputScreen = Util.FindScreen(settings.MFD3_OutputDisplay);
            _captureCoordinatesSet.LMFD.OutputScreen = Util.FindScreen(settings.LMFD_OutputDisplay);
            _captureCoordinatesSet.RMFD.OutputScreen = Util.FindScreen(settings.RMFD_OutputDisplay);
            _captureCoordinatesSet.HUD.OutputScreen = Util.FindScreen(settings.HUD_OutputDisplay);

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
                    UnregisterBackgroundWorkerFunctions();
                }
            }
            _disposed = true;
        }

        #endregion
    }
}