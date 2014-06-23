using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Common.Imaging;
using Common.Win32;
using F4SharedMem;
using F4Utils.Terrain.Structs;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Microsoft.Win32;

namespace F4Utils.Terrain
{
	public interface ITerrainHeightProvider
	{
		float GetTerrainHeight(float feetNorth, float feetEast);
	}

	public class TerrainBrowser : IDisposable, ITerrainHeightProvider
	{
        private static readonly ILog Log = LogManager.GetLogger(typeof (TerrainBrowser));
        private ICurrentTheaterNameDetector _currentTheaterNameDetector = new CurrentTheaterNameDetector();
        private ITheaterDotTdfFileReader _theaterDotTdfFileReader = new TheaterDotTdfFileReader();
        private INearTileTextureLoader _nearTileTextureLoader = new NearTileTextureLoader();
        private ILatLongCalculator _latLongCalculator = new LatLongCalculator();
        private ITerrainHeightCalculator _terrainHeightCalculator = new TerrainHeightCalculator();
        private IColumnAndRowElevationPostRecordRetriever _columnAndRowElevationPostRecordRetriever = new ColumnAndRowElevationPostRecordRetriever();
        private IElevationPostCoordinateClamper _elevationPostCoordinateClamper = new ElevationPostCoordinateClamper();
        private IDistanceBetweenElevationPostsCalculator _distanceBetweenElevationPostsCalculator = new DistanceBetweenElevationPostsCalculator();
        private INearestElevationPostColumnAndRowCalculator _nearestElevationPostColumnAndRowCalculator = new NearestElevationPostColumnAndRowCalculator();
        private ITheaterMapBuilder _theaterMapBuilder = new TheaterMapBuilder();
        private ITerrainTextureByTextureIdRetriever _terrainTextureByTextureIdRetriever = new TerrainTextureByTextureIdRetriever();
        private IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever = new DetailTextureForElevationPostRetriever();
        private ICurrentTheaterDotTdfLoader _currentTheaterDotTdfLoader = new CurrentTheaterDotTdfLoader();
        private ITheaterDotMapFileReader _theaterDotMapFileReader = new TheaterDotMapFileReader();
        private readonly bool _loadAllLods;

        #region Instance Variables

        private string _currentTheaterTextureBaseFolderPath;
        private bool _disposing;
        private Dictionary<LodTextureKey, Bitmap> _elevationPostTextures = new Dictionary<LodTextureKey, Bitmap>();
        private BackgroundWorker _farTileReadingBackgroundWorker;
        private Dictionary<uint, Bitmap> _farTileTextures = new Dictionary<uint, Bitmap>();
        private string _farTilesDotDdsFilePath;
        private FarTilesDotPalFileInfo _farTilesDotPalFileInfo;
        private string _farTilesDotRawFilePath;
        private bool _isDisposed;
        private Dictionary<uint, Bitmap> _nearTileTextures = new Dictionary<uint, Bitmap>();
        private bool _terrainLoaded;
        private TextureDotBinFileInfo _textureDotBinFileInfo;
        private Dictionary<string, ZipEntry> _textureDotZipFileEntries = new Dictionary<string, ZipEntry>();
        private ZipFile _textureZipFile;
        private TheaterDotLxFileInfo[] _theaterDotLxFiles;
        private TheaterDotMapFileInfo _theaterDotMapFileInfo;
        private Bitmap[] _theaterMaps;
        private IFarTileTextureRetriever _farTileTextureRetriever = new FarTileTextureRetriever();
        #endregion

        public TerrainBrowser(bool loadAllLods)
        {
            _loadAllLods = loadAllLods;
        }

        public TheaterDotMapFileInfo CurrentTheaterDotMapFileInfo
        {
            get { return _theaterDotMapFileInfo; }
        }

        public void LoadCurrentTheaterTerrainDatabase()
        {
            lock (this)
            {
                if (_terrainLoaded || _disposing || _isDisposed) return;
                var falconFormat = Process.Util.DetectFalconFormat();
                if (!falconFormat.HasValue) return;
                //TODO: check these against other theaters, for correct way to read theater installation locations
                var exePath = Process.Util.GetFalconExePath();
                if (exePath == null) return;
                var f4BasePathFI = new FileInfo(exePath);
                exePath = f4BasePathFI.DirectoryName + Path.DirectorySeparatorChar;
                var currentTheaterTdf = _currentTheaterDotTdfLoader.GetCurrentTheaterDotTdf(exePath, falconFormat.Value);
                if (currentTheaterTdf == null) return;
                //string theaterName = currentTheaterTdf.theaterName//DetectCurrentTheaterName();
                //if (theaterName == null) return;
                var dataPath = exePath;
                if (falconFormat.Value == FalconDataFormats.BMS4)
                {
                    dataPath = exePath + "..\\..\\data";
                }
                var terrainBasePath = dataPath + Path.DirectorySeparatorChar + currentTheaterTdf.terrainDir;
                _currentTheaterTextureBaseFolderPath = terrainBasePath + Path.DirectorySeparatorChar + "texture";
                var theaterDotMapFilePath = terrainBasePath + Path.DirectorySeparatorChar + "terrain" +
                                            Path.DirectorySeparatorChar + "THEATER.MAP";
                var textureDotBinFilePath = _currentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +
                                            "TEXTURE.BIN";
                _farTilesDotRawFilePath = _currentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +
                                          "FARTILES.RAW";
                _farTilesDotDdsFilePath = _currentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +
                                          "FARTILES.DDS";
                var farTilesDotPalFilePath = _currentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +
                                             "FARTILES.PAL";

                _theaterDotMapFileInfo = _theaterDotMapFileReader.ReadTheaterDotMapFile(theaterDotMapFilePath);
                if (_elevationPostTextures != null)
                {
                    DisposeElevationPostTextures();
                    _elevationPostTextures = new Dictionary<LodTextureKey, Bitmap>();
                }
                if (_farTileTextures != null)
                {
                    DisposeFarTilesTextures();
                    _farTileTextures = new Dictionary<uint, Bitmap>();
                }
                if (_nearTileTextures != null)
                {
                    DisposeNearTileTextures();
                    _nearTileTextures = new Dictionary<uint, Bitmap>();
                }
                _theaterDotLxFiles = new TheaterDotLxFileInfo[_theaterDotMapFileInfo.NumLODs];
                if (_loadAllLods)
                {
                    for (uint i = 0; i < _theaterDotMapFileInfo.NumLODs; i++)
                    {
                        _theaterDotLxFiles[i] = Util.LoadTheaterDotLxFile(i, theaterDotMapFilePath);
                    }
                }
                else
                {
                    _theaterDotLxFiles[0] = Util.LoadTheaterDotLxFile(0, theaterDotMapFilePath);
                }
                _textureDotBinFileInfo = Util.ReadTextureDotBinFile(textureDotBinFilePath);
                _farTilesDotPalFileInfo = Util.ReadFarTilesDotPalFile(farTilesDotPalFilePath);
                _terrainLoaded = true;
            }
        }

        public void LoadFarTilesAsync()
        {
            if (_farTileReadingBackgroundWorker == null)
            {
                _farTileReadingBackgroundWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
                _farTileReadingBackgroundWorker.DoWork += FarTileReadingBackgroundWorkerDoWork;
            }
            if (_farTileReadingBackgroundWorker.IsBusy) return;

            _farTileReadingBackgroundWorker.RunWorkerAsync();
        }

        private void FarTileReadingBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (var lod = (_theaterDotMapFileInfo.LastNearTiledLOD + 1);
                     lod <= _theaterDotMapFileInfo.LastFarTiledLOD;
                     lod++)
                {
                    for (var x = 0;
                         x <
                         (_theaterDotMapFileInfo.LODMapWidths[lod]*
                          Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT);
                         x++)
                    {
                        for (var y = 0;
                             y <
                             (_theaterDotMapFileInfo.LODMapHeights[lod]*
                              Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT);
                             y++)
                        {
                            if (_farTileReadingBackgroundWorker == null ||
                                _farTileReadingBackgroundWorker.CancellationPending) return;
                            GetDetailTextureForElevationPost(x, y, lod);
                            if (y%1024 == 0) Thread.Sleep(5);
                        }
                        Thread.Sleep(5);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                if (ex is SystemException) throw;
            }
        }

        public void ClampElevationPostCoordinates(ref int postColumn, ref int postRow, uint lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _theaterDotLxFiles == null || _disposing || _isDisposed)
            {
                postColumn = 0;
                postRow = 0;
                return;
            }

            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(_theaterDotMapFileInfo, _theaterDotLxFiles, ref postColumn, ref postRow, lod);
        }

        public Bitmap GetFarTileTexture(uint textureId)
        {
            return _farTileTextureRetriever.GetFarTileTexture(textureId, _farTileTextures, _farTilesDotDdsFilePath, _farTilesDotRawFilePath, _farTilesDotPalFileInfo);
        }

 
        public Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                return null;
            }

            if (_theaterDotLxFiles == null) return null;
            return _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost(postCol, postRow, lod,
                _theaterDotLxFiles, _theaterDotMapFileInfo, _textureDotBinFileInfo, _nearTileTextures,
                ref _textureZipFile, ref _textureDotZipFileEntries,
                _farTileTextures,_farTilesDotDdsFilePath, _farTilesDotRawFilePath, _farTilesDotPalFileInfo, _elevationPostTextures,
                _currentTheaterTextureBaseFolderPath);
        }

        public Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _theaterDotLxFiles == null || _disposing)
            {
                return null;
            }

            return _terrainTextureByTextureIdRetriever.GetTerrainTextureByTextureId(textureId, lod, _theaterDotLxFiles, _theaterDotMapFileInfo, _textureDotBinFileInfo, _nearTileTextures, 
                ref _textureZipFile, ref _textureDotZipFileEntries, _farTileTextures, _farTilesDotDdsFilePath, _farTilesDotRawFilePath, _farTilesDotPalFileInfo, _currentTheaterTextureBaseFolderPath);
        }

        public unsafe Bitmap GetTheaterMap(uint lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                return null;
            }

            if (_theaterMaps == null)
            {
                _theaterMaps = new Bitmap[_theaterDotMapFileInfo.NumLODs];
            }
            if (_theaterMaps[lod] != null) return _theaterMaps[lod];

            var map = _theaterMapBuilder.GetTheaterMap(lod, _theaterDotLxFiles, _theaterDotMapFileInfo);
            _theaterMaps[lod] = map;
            return map;
        }
        public string DetectCurrentTheaterName() {
            return _currentTheaterNameDetector.DetectCurrentTheaterName();
        }

        public float GetTerrainHeight(float feetNorth, float feetEast)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                return 0;
            }

            if (_theaterDotLxFiles == null) return 0;
            return _terrainHeightCalculator.CalculateTerrainHeight(feetNorth, feetEast, _theaterDotLxFiles, _theaterDotMapFileInfo);
        }

        
        public void CalculateLatLong(float feetNorth, float feetEast, out int latitudeWholeDegrees,
                                     out float latitudeFractionalMinutes, out int longitudeWholeDegrees,
                                     out float longitudeFactionalMinutes)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                latitudeWholeDegrees = 0;
                latitudeFractionalMinutes = 0;
                longitudeWholeDegrees = 0;
                longitudeFactionalMinutes = 0;
                return;
            }
            _latLongCalculator.CalculateLatLong(_theaterDotMapFileInfo, feetNorth, feetEast, out latitudeWholeDegrees, out latitudeFractionalMinutes, out longitudeWholeDegrees, out longitudeFactionalMinutes);
        }
        public float GetNumFeetBetweenElevationPosts(int lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                return 0;
            }
            return _distanceBetweenElevationPostsCalculator.GetNumFeetBetweenElevationPosts(lod, _theaterDotLxFiles, _theaterDotMapFileInfo);
        }
        

        public TheaterDotLxFileRecord GetElevationPostRecordByNorthEastCoordinate(float feetNorth, float feetEast)
        {
            int col;
            int row;
            _nearestElevationPostColumnAndRowCalculator.GetNearestElevationPostColumnAndRowForNorthEastCoordinates(feetNorth, feetEast, out col, out row, _theaterDotLxFiles, _theaterDotMapFileInfo);
            return GetElevationPostRecordByColumnAndRow(col, row, 0);
        }

        public TheaterDotLxFileRecord GetElevationPostRecordByColumnAndRow(int postColumn, int postRow, uint lod)
        {
            if (!_terrainLoaded)
            {
                LoadCurrentTheaterTerrainDatabase();
            }

            if (!_terrainLoaded || _disposing || _isDisposed)
            {
                return null;
            }
            return _columnAndRowElevationPostRecordRetriever.GetElevationPostRecordByColumnAndRow(postColumn, postRow, lod, _theaterDotLxFiles, _theaterDotMapFileInfo);
        }

        #region Destructors

        /// <summary>
        ///   Public implementation of IDisposable.Dispose().  Cleans up managed
        ///   and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Standard finalizer, which will call Dispose() if this object is not
        ///   manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~TerrainBrowser()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _disposing = true;
                    if (_farTileReadingBackgroundWorker != null)
                    {
                        _farTileReadingBackgroundWorker.CancelAsync();
                    }
                    var waitCount = 0;
                    while (_farTileReadingBackgroundWorker != null && _farTileReadingBackgroundWorker.IsBusy &&
                           waitCount < 1000)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5);
                        waitCount++;
                    }
                    _farTileReadingBackgroundWorker = null;

                    //dispose of managed resources here
                    if (_textureZipFile != null)
                    {
                        try
                        {
                            _textureZipFile.Close();
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                    }
                    _textureZipFile = null;
                    DisposeFarTilesTextures();
                    DisposeNearTileTextures();
                    DisposeElevationPostTextures();
                    _theaterMaps = null;
                    _textureDotZipFileEntries = null;
                    _textureZipFile = null;
                    _theaterDotLxFiles = null;
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        private void DisposeFarTilesTextures()
        {
            if (_farTileTextures != null)
            {
                var toDispose = new List<Bitmap>();
                try
                {
                    foreach (var texture in _farTileTextures)
                    {
                        try
                        {
                            if (texture.Value != null)
                            {
                                toDispose.Add(texture.Value);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message, e);
                }
                foreach (var obj in toDispose)
                {
                    Common.Util.DisposeObject(obj);
                }
            }
            _farTileTextures = null;
        }

        private void DisposeNearTileTextures()
        {
            if (_nearTileTextures != null)
            {
                var toDispose = new List<Bitmap>();
                try
                {
                    foreach (var texture in _nearTileTextures)
                    {
                        try
                        {
                            if (texture.Value != null)
                            {
                                toDispose.Add(texture.Value);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message, e);
                }
                foreach (var obj in toDispose)
                {
                    Common.Util.DisposeObject(obj);
                }
            }
            _nearTileTextures = null;
        }

        private void DisposeElevationPostTextures()
        {
            if (_elevationPostTextures != null)
            {
                var toDispose = new List<Bitmap>();
                try
                {
                    foreach (var texture in _elevationPostTextures)
                    {
                        try
                        {
                            if (texture.Value != null)
                            {
                                toDispose.Add(texture.Value);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message, e);
                }
                foreach (var obj in toDispose)
                {
                    Common.Util.DisposeObject(obj);
                }
            }

            _elevationPostTextures = null;
        }

        #endregion

    }
}