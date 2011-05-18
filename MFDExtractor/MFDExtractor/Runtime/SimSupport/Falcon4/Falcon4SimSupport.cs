using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using Common.Networking;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.Terrain;
using log4net;
using MFDExtractor.Networking;
using MFDExtractor.Runtime.Settings;
using Reader = F4TexSharedMem.Reader;
using Util = Common.Threading.Util;

namespace MFDExtractor.Runtime.SimSupport.Falcon4
{
    public class Falcon4SimSupport : IDisposable
    {
        #region Class variables

        private static readonly ILog Log = LogManager.GetLogger(typeof (Falcon4SimSupport));

        #endregion

        #region Instance variables

        private bool _disposed;

        /// <summary>
        /// Reference to the sim-is-running status monitor thread 
        /// </summary>
        private Thread _simStatusMonitorThread;

        #region Falcon 4 Sharedmem Readers & status flags

        private readonly Extractor _extractor;
        private readonly SettingsManager _settingsManager;

        /// <summary>
        /// Reference to a Reader object that can read values from Falcon's basic (non-textures) shared
        /// memory area.  This is used to detect whether Falcon is running and to provide flight data to rendered instruments
        /// </summary>
        private F4SharedMem.Reader _falconSmReader;

        private NetworkManager _networkManager;

        /// <summary>
        /// Flag to indicate whether BMS's 3D textures shared memory area is available and has data
        /// </summary>
        private bool _sim3DDataAvailable;

        /// <summary>
        /// Flag to indicate whether the sim is running
        /// </summary>
        private bool _simRunning;

        private TerrainBrowser _terrainBrowser = new TerrainBrowser(false);

        /// <summary>
        /// Reference to a Reader object that can read images from BMS's "textures shared memory" 
        /// area -- this reference is used to perform the actual 3D-mode image extraction
        /// </summary>
        private Reader _texSmReader = new Reader();

        /// <summary>
        /// Reference to a Reader object that can read images from BMS's "textures shared memory" area 
        /// -- this reference is used to detect whether the 3D-mode shared 
        /// memory images actually exist or not (can be recreated at certain 
        /// intervals without affecting code using the other reference)
        /// </summary>
        private Reader _texSmStatusReader = new Reader();

        private bool _useBMSAdvancedSharedmemValues;

        #endregion

        #endregion

        #region Constructors

        private Falcon4SimSupport()
        {
        }

        internal Falcon4SimSupport(SettingsManager settingsManager, NetworkManager networkManager, Extractor extractor)
            : this()
        {
            _settingsManager = settingsManager;
            _networkManager = networkManager;
            _extractor = extractor;
            SetupSimStatusMonitorThread();
        }

        #endregion

        #region Public Properties

        public bool IsSimRunning
        {
            get { return _simRunning; }
        }

        public bool UseBMSAdvancedSharedmemValues
        {
            get { return _useBMSAdvancedSharedmemValues; }
            internal set { _useBMSAdvancedSharedmemValues = value; }
        }

        #endregion

        #region Public Methods

        public Image ReadRTTImage(Rectangle areaToCapture)
        {
            return ReadRTTImage(areaToCapture, _texSmReader);
        }

        #endregion

        private void SetupSimStatusMonitorThread()
        {
            Util.AbortThread(ref _simStatusMonitorThread);
            _simStatusMonitorThread = new Thread(SimStatusMonitorThreadWork);
            _simStatusMonitorThread.Priority = _settingsManager.ThreadPriority;
            _simStatusMonitorThread.IsBackground = true;
            _simStatusMonitorThread.Name = "SimStatusMonitorThread";
            _simStatusMonitorThread.Start();
        }

        public FlightData GetFlightData()
        {
            FlightData toReturn = null;
            if (!_settingsManager.TestMode)
            {
                if (_simRunning || _settingsManager.NetworkMode == NetworkMode.Client)
                {
                    if (_settingsManager.NetworkMode != NetworkMode.Server &&
                        _settingsManager.NetworkMode != NetworkMode.Standalone)
                    {
                        if (_settingsManager.NetworkMode == NetworkMode.Client)
                        {
                            toReturn = _networkManager.ReadFlightDataFromServer();
                        }
                    }
                    else
                    {
                        var format = F4Utils.Process.Util.DetectFalconFormat();
#if (ALLIEDFORCE)
                        format = FalconDataFormats.AlliedForce;
#endif
                        //set automatic 3D mode for BMS
                        if (format.HasValue && format.Value == FalconDataFormats.BMS4) _extractor.ThreeDeeMode = true;

                        var doMore = true;
                        var newReader = false;
                        if (_falconSmReader == null)
                        {
                            if (format.HasValue)
                            {
                                _falconSmReader = new F4SharedMem.Reader(format.Value);
                                newReader = true;
                            }
                            else
                            {
                                _falconSmReader = new F4SharedMem.Reader();
                                newReader = true;
                            }
                        }
                        else
                        {
                            if (format.HasValue)
                            {
                                if (format.Value != _falconSmReader.DataFormat)
                                {
                                    _falconSmReader = new F4SharedMem.Reader(format.Value);
                                    newReader = true;
                                }
                            }
                            else
                            {
                                doMore = false;
                                Common.Util.DisposeObject(_falconSmReader);
                                _falconSmReader = null;
                                DisableBMSAdvancedSharedmemValues();
                                newReader = false;
                            }
                        }
                        if (newReader)
                        {
                            var exePath = F4Utils.Process.Util.GetFalconExePath();
                            FileVersionInfo verInfo = null;
                            if (exePath != null) verInfo = FileVersionInfo.GetVersionInfo(exePath);
                            if (format.HasValue && format.Value == FalconDataFormats.BMS4 && verInfo != null &&
                                ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >= 6826) ||
                                 (verInfo.ProductMajorPart > 4)))
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
                            var computeRalt = false;
                            if (Properties.Settings.Default.EnableISISOutput)
                            {
                                computeRalt = true;
                            }
                            if (computeRalt)
                            {
                                if (_terrainBrowser == null)
                                {
                                    _terrainBrowser = new TerrainBrowser(false);
                                    _terrainBrowser.LoadCurrentTheaterTerrainDatabase();
                                }
                                if (toReturn != null)
                                {
                                    var extensionData = new FlightDataExtension();
                                    var terrainHeight = _terrainBrowser.GetTerrainHeight(toReturn.x, toReturn.y);
                                    var ralt = -toReturn.z - terrainHeight;

                                    //reset AGL altitude to zero if we're on the ground
                                    if (
                                        ((toReturn.lightBits & (int) LightBits.WOW) == (int) LightBits.WOW)
                                        ||
                                        (
                                            ((toReturn.lightBits3 & (int) Bms4LightBits3.OnGround) ==
                                             (int) Bms4LightBits3.OnGround)
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
                }
            }
            return toReturn ?? (new FlightData {hsiBits = Int32.MaxValue});
        }

        private void DisableBMSAdvancedSharedmemValues()
        {
            _useBMSAdvancedSharedmemValues = false;
            if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                var msg = new Message(MessageTypes.DisableBMSAdvancedSharedmemValues.ToString(), null);
                _networkManager.SubmitMessageToClientFromServer(msg);
            }
        }

        private void EnableBMSAdvancedSharedmemValues()
        {
            _useBMSAdvancedSharedmemValues = true;
            if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                var msg = new Message(MessageTypes.EnableBMSAdvancedSharedmemValues.ToString(), null);
                _networkManager.SubmitMessageToClientFromServer(msg);
            }
        }

        #region RTT support functions

        private static void ReadRTTCoords(Dictionary<string, Rectangle> items)
        {
            var file = FindBms3DCockpitFile();
            if (file == null)
            {
                return;
            }

            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var currentLine = reader.ReadLine();
                    foreach (var itemName in items.Keys)
                    {
                        if (currentLine != null)
                            if (!currentLine.ToLowerInvariant().StartsWith(itemName.ToLowerInvariant())) continue;
                        var thisItemRect = new Rectangle();
                        var tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count <= 12)
                        {
                        }
                        else
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
                                Log.Error(e.Message, e);
                            }
                        }
                    }
                }
            }
        }

        private static void ReadRTTCoords(CaptureCoordinatesSet captureCoordinatesSet)
        {
            var items = new Dictionary<string, CaptureCoordinates>
                            {
                                {"LMFD", captureCoordinatesSet.LMFD},
                                {"RMFD", captureCoordinatesSet.RMFD},
                                {"MFD3", captureCoordinatesSet.MFD3},
                                {"MFD4", captureCoordinatesSet.MFD4},
                                {"HUD", captureCoordinatesSet.HUD}
                            };
        }

        private static string RunningBmsInstanceBasePath()
        {
            string toReturn = null;
            var exePath = F4Utils.Process.Util.GetFalconExePath();
            if (!string.IsNullOrEmpty(exePath))
            {
                toReturn = new FileInfo(exePath).Directory.FullName;
            }
            return toReturn;
        }

        private static FileInfo FindBms3DCockpitFile()
        {
            var basePath = RunningBmsInstanceBasePath();
            if (basePath != null)
            {
                var path = basePath + @"\art\ckptartn";
                var dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    var subDirs = dir.GetDirectories();
                    FileInfo file;
                    foreach (var thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (!file.Exists)
                        {
                        }
                        else
                        {
                            try
                            {
                                using (
                                    var fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
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
                            using (
                                var fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                          FileShare.None))
                            {
                                fs.Close();
                            }
                        }
                        catch (IOException)
                        {
                            return file;
                        }
                    }
                }

                path = basePath + @"\art\ckptart";
                dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    var subDirs = dir.GetDirectories();
                    FileInfo file = null;
                    foreach (var thisDir in subDirs)
                    {
                        file = new FileInfo(thisDir.FullName + @"\3dckpit.dat");
                        if (!file.Exists)
                        {
                        }
                        else
                        {
                            try
                            {
                                using (
                                    var fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite,
                                                              FileShare.None))
                                {
                                    fs.Close();
                                }
                            }
                            catch (IOException)
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

        internal static Image ReadRTTImage(Rectangle areaToCapture, Reader texSharedmemReader)
        {
            Image toReturn = null;
            try
            {
                if (texSharedmemReader != null)
                {
                    toReturn = texSharedmemReader.GetImage(areaToCapture); //Common.Imaging.Util.CloneBitmap();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            return toReturn;
        }

        /// <summary>
        /// Worker thread method for monitoring whether the sim is running
        /// </summary>
        private void SimStatusMonitorThreadWork()
        {
            try
            {
                var count = 0;

                while (!_disposed)
                {
                    count++;
                    if (_settingsManager.NetworkMode == NetworkMode.Server ||
                        _settingsManager.NetworkMode == NetworkMode.Standalone)
                    {
                        var simWasRunning = _simRunning;

                        //TODO:make this check optional via the user-config file
                        if (count%1 == 0)
                        {
                            count = 0;
                            Common.Util.DisposeObject(_texSmStatusReader);
                            _texSmStatusReader = new Reader();

#if SIMRUNNING
                            _simRunning = true;
#else
                            try
                            {
                                _simRunning = _settingsManager.NetworkMode == NetworkMode.Client ||
                                              F4Utils.Process.Util.IsFalconRunning();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                            }
#endif
                            _sim3DDataAvailable = _simRunning &&
                                                  (_settingsManager.NetworkMode == NetworkMode.Client ||
                                                   _texSmStatusReader.IsDataAvailable);

                            if (_sim3DDataAvailable)
                            {
                                try
                                {
                                    if (_extractor.ThreeDeeMode)
                                    {
                                        if (_texSmReader == null) _texSmReader = new Reader();
                                        if ((Properties.Settings.Default.EnableHudOutput ||
                                             _settingsManager.NetworkMode == NetworkMode.Server))
                                        {
                                            if (
                                                (_settingsManager.CaptureCoordinatesSet.HUD.RTTSourceCoords ==
                                                 Rectangle.Empty)
                                                ||
                                                (_settingsManager.CaptureCoordinatesSet.LMFD.RTTSourceCoords ==
                                                 Rectangle.Empty)
                                                ||
                                                (_settingsManager.CaptureCoordinatesSet.RMFD.RTTSourceCoords ==
                                                 Rectangle.Empty)
                                                ||
                                                (_settingsManager.CaptureCoordinatesSet.MFD3.RTTSourceCoords ==
                                                 Rectangle.Empty)
                                                ||
                                                (_settingsManager.CaptureCoordinatesSet.MFD4.RTTSourceCoords ==
                                                 Rectangle.Empty)
                                                )
                                            {
                                                ReadRTTCoords(_settingsManager.CaptureCoordinatesSet);
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
                                _settingsManager.CaptureCoordinatesSet.HUD.RTTSourceCoords = Rectangle.Empty;
                                _settingsManager.CaptureCoordinatesSet.LMFD.RTTSourceCoords = Rectangle.Empty;
                                _settingsManager.CaptureCoordinatesSet.RMFD.RTTSourceCoords = Rectangle.Empty;
                                _settingsManager.CaptureCoordinatesSet.MFD3.RTTSourceCoords = Rectangle.Empty;
                                _settingsManager.CaptureCoordinatesSet.MFD4.RTTSourceCoords = Rectangle.Empty;
                            }
                            if (simWasRunning && !_simRunning)
                            {
                                CloseAndDisposeSharedmemReaders();

                                if (_settingsManager.NetworkMode == NetworkMode.Server)
                                {
                                    Common.Util.DisposeObject(_networkManager);
                                    _networkManager = null;
                                }
                            }
                            if (_settingsManager.NetworkMode == NetworkMode.Server && (!simWasRunning && _simRunning))
                            {
                                Common.Util.DisposeObject(_networkManager);
                                _networkManager = null;
                                _networkManager = new NetworkManager(_settingsManager);
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
                Thread.ResetAbort();
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
                    Util.AbortThread(ref _simStatusMonitorThread);
                    _simStatusMonitorThread = null;
                    Common.Util.DisposeObject(_texSmReader);
                    Common.Util.DisposeObject(_texSmStatusReader);
                    Common.Util.DisposeObject(_falconSmReader);
                }
            }
            _disposed = true;
        }

        #endregion
    }
}