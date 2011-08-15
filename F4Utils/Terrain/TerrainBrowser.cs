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
    public class TerrainBrowser : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TerrainBrowser));
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
            if (_terrainLoaded || _disposing || _isDisposed) return;
            var falconFormat = Process.Util.DetectFalconFormat();
            if (!falconFormat.HasValue) return;
            //TODO: check these against other theaters, for correct way to read theater installation locations
            var exePath = Process.Util.GetFalconExePath();
            if (exePath == null) return;
            var f4BasePathFI = new FileInfo(exePath);
            exePath = f4BasePathFI.DirectoryName + Path.DirectorySeparatorChar;
            var currentTheaterTdf = GetCurrentTheaterDotTdf(exePath, falconFormat.Value);
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

            _theaterDotMapFileInfo = Util.ReadTheaterDotMapFile(theaterDotMapFilePath);
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

            var mapInfo = _theaterDotMapFileInfo;
            var lodInfo = _theaterDotLxFiles[lod];

            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            if (postColumn < 0) postColumn = 0;
            if (postRow < 0) postRow = 0;
            if (postColumn > (mapInfo.LODMapWidths[lodInfo.LoDLevel]*postsAcross) - 1)
                postColumn = (int) (mapInfo.LODMapWidths[lodInfo.LoDLevel]*postsAcross) - 1;
            if (postRow > (mapInfo.LODMapHeights[lodInfo.LoDLevel]*postsAcross) - 1)
                postRow = (int) (mapInfo.LODMapHeights[lodInfo.LoDLevel]*postsAcross) - 1;
        }

        public Bitmap GetFarTileTexture(uint textureId)
        {
            if (_farTileTextures != null && _farTileTextures.ContainsKey(textureId))
            {
                return _farTileTextures[textureId];
            }
            if (String.IsNullOrEmpty(_farTilesDotDdsFilePath) && (String.IsNullOrEmpty(_farTilesDotRawFilePath)))
                return null;

            if (_farTilesDotDdsFilePath != null)
            {
                var fileInfo = new FileInfo(_farTilesDotDdsFilePath);
                var useDDS = true;
                if (!fileInfo.Exists) 
                {
                    useDDS = false;
                    fileInfo = new FileInfo(_farTilesDotRawFilePath);
                    if (!fileInfo.Exists) return null;
                }


                Bitmap bitmap;
                if (useDDS)
                {
                    using (var stream = File.OpenRead(_farTilesDotDdsFilePath))
                    {
                        var headerSize = Marshal.SizeOf(typeof (NativeMethods.DDSURFACEDESC2));
                        var header = new byte[headerSize];
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Read(header, 0, headerSize);

                        var pinnedHeader = GCHandle.Alloc(header, GCHandleType.Pinned);
                        var surfaceDesc =
                            (NativeMethods.DDSURFACEDESC2)
                            Marshal.PtrToStructure(pinnedHeader.AddrOfPinnedObject(),
                                                   typeof (NativeMethods.DDSURFACEDESC2));
                        pinnedHeader.Free();

                        var imageSize = ((surfaceDesc.dwFlags & NativeMethods.DDSD_PITCH) == NativeMethods.DDSD_PITCH)
                                            ? surfaceDesc.dwHeight*surfaceDesc.lPitch
                                            : surfaceDesc.dwLinearSize;
                        var ddsBytes = new byte[headerSize + 4 + imageSize];
                        ddsBytes[0] = 0x44;
                        ddsBytes[1] = 0x44;
                        ddsBytes[2] = 0x53;
                        ddsBytes[3] = 0x20;
                        Array.Copy(header, 0, ddsBytes, 4, header.Length);
                        stream.Seek((imageSize*textureId), SeekOrigin.Current);
                        stream.Read(ddsBytes, headerSize + 4, imageSize);
                        bitmap = DDS.GetBitmapFromDDSFileBytes(ddsBytes);

                        if (_farTileTextures != null && !_farTileTextures.ContainsKey(textureId))
                        {
                            _farTileTextures.Add(textureId, bitmap);
                        }
                        stream.Close();
                    }
                }
                else
                {
                    bitmap = new Bitmap(32, 32, PixelFormat.Format8bppIndexed);
                    var pal = bitmap.Palette;
                    for (var i = 0; i < 256; i++)
                    {
                        pal.Entries[i] = _farTilesDotPalFileInfo.pallete[i];
                    }
                    bitmap.Palette = pal;
                    using (var stream = File.OpenRead(_farTilesDotRawFilePath))
                    {
                        const int imageSizeBytes = 32*32;
                        stream.Seek(imageSizeBytes*textureId, SeekOrigin.Begin);
                        var bytesRead = new byte[imageSizeBytes];
                        stream.Read(bytesRead, 0, imageSizeBytes);
                        var lockData = bitmap.LockBits(new Rectangle(0, 0, 32, 32), ImageLockMode.WriteOnly,
                                                       bitmap.PixelFormat);
                        var scan0 = lockData.Scan0;
                        var height = lockData.Height;
                        var width = lockData.Width;
                        Marshal.Copy(bytesRead, 0, scan0, width*height);
                        bitmap.UnlockBits(lockData);
                        if (_farTileTextures != null && !_farTileTextures.ContainsKey(textureId))
                        {
                            _farTileTextures.Add(textureId, bitmap);
                        }
                        stream.Close();
                    }
                }
                return bitmap;
            }
            return null;
        }

        public Bitmap LoadNearTileTexture(string textureBaseFolderPath, string tileName)
        {
            Bitmap toReturn;
            var tileFullPath = Path.Combine(textureBaseFolderPath, tileName);

            var tileInfo = new FileInfo(tileFullPath);
            if (string.Equals(tileInfo.Extension, ".PCX", StringComparison.InvariantCultureIgnoreCase))
            {
                tileFullPath = Path.Combine(Path.Combine(textureBaseFolderPath, "texture"),
                                            Path.GetFileNameWithoutExtension(tileInfo.Name) + ".DDS");
                tileInfo = new FileInfo(tileFullPath);
            }
            if (tileInfo.Exists)
            {
                try
                {
                    toReturn = DDS.Load(tileFullPath);
                    return toReturn;
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message, e);
                }
            }


            if (_textureZipFile == null)
            {
                _textureZipFile = new ZipFile(textureBaseFolderPath + Path.DirectorySeparatorChar + "texture.zip");
            }
            if (_textureDotZipFileEntries == null || _textureDotZipFileEntries.Count == 0)
            {
                _textureDotZipFileEntries = Common.Compression.Zip.Util.GetZipFileEntries(_textureZipFile);
            }
            if (!_textureDotZipFileEntries.ContainsKey(tileName.ToLowerInvariant())) return null;
            var thisEntry = _textureDotZipFileEntries[tileName.ToLowerInvariant()];
            using (var zipStream = _textureZipFile.GetInputStream(thisEntry))
            {
                var rawBytes = new byte[zipStream.Length];
                zipStream.Read(rawBytes, 0, rawBytes.Length);
                toReturn = PCX.LoadFromBytes(rawBytes);
            }
            return toReturn;
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
            var col = postCol;
            var row = postRow;

            ClampElevationPostCoordinates(ref col, ref row, lod);
            if (postCol != col || postRow != row)
            {
                col = 0;
                row = 0;
            }


            var lRecord = GetElevationPostRecordByColumnAndRow(col, row, lod);

            var textureId = lRecord.TextureId;
            var bigTexture = GetTerrainTextureByTextureId(textureId, lod);
            Bitmap toReturn;
            if (lod <= _theaterDotMapFileInfo.LastNearTiledLOD)
            {
                var chunksWide = 4 >> (int) lod;
                var thisChunkXIndex = (uint) (col%chunksWide);
                var thisChunkYIndex = (uint) (row%chunksWide);

                var key = new LodTextureKey
                              {
                                  Lod = lod,
                                  textureId = textureId,
                                  chunkXIndex = thisChunkXIndex,
                                  chunkYIndex = thisChunkYIndex
                              };
                if (_elevationPostTextures.ContainsKey(key))
                {
                    toReturn = _elevationPostTextures[key];
                }
                else
                {
                    var leftX = (int) (thisChunkXIndex*(bigTexture.Width/chunksWide));
                    var rightX = (int) ((thisChunkXIndex + 1)*(bigTexture.Width/chunksWide)) - 1;
                    var topY = (int) (bigTexture.Height - (thisChunkYIndex + 1)*(bigTexture.Height/chunksWide));
                    var bottomY = (int) (bigTexture.Height - thisChunkYIndex*(bigTexture.Height/chunksWide)) - 1;

                    var sourceRect = new Rectangle(leftX, topY, (rightX - leftX) + 1, (bottomY - topY) + 1);

                    toReturn = (Bitmap) Common.Imaging.Util.CropBitmap(bigTexture, sourceRect);
                    _elevationPostTextures.Add(key, toReturn);
                }
            }
            else
            {
                toReturn = bigTexture;
            }
            return toReturn;
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

            var lodInfo = _theaterDotLxFiles[lod];
            Bitmap toReturn = null;

            if (lod <= _theaterDotMapFileInfo.LastNearTiledLOD)
            {
                var textureBinInfo = _textureDotBinFileInfo;
                var textureBaseFolderPath = _currentTheaterTextureBaseFolderPath;
                textureId -= lodInfo.minTexOffset;
                if (_nearTileTextures.ContainsKey(textureId)) return _nearTileTextures[textureId];

                var setNum = textureId/Constants.NUM_TEXTURES_PER_SET;
                var tileNum = textureId%Constants.NUM_TEXTURES_PER_SET;
                var thisSet = textureBinInfo.setRecords[setNum];
                var tileName = thisSet.tileRecords[tileNum].tileName;
                toReturn = LoadNearTileTexture(textureBaseFolderPath, tileName);
                if (toReturn != null)
                {
                    _nearTileTextures.Add(textureId, toReturn);
                }
            }
            else if (lod <= _theaterDotMapFileInfo.LastFarTiledLOD)
            {
                toReturn = GetFarTileTexture(textureId);
            }
            return toReturn;
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

            var lodInfo = _theaterDotLxFiles[lod];
            var mapInfo = _theaterDotMapFileInfo;
            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            var bmp = new Bitmap((int) mapInfo.LODMapWidths[lodInfo.LoDLevel]*postsAcross,
                                 (int) mapInfo.LODMapHeights[lodInfo.LoDLevel]*postsAcross,
                                 PixelFormat.Format8bppIndexed);
            TheaterDotOxFileRecord block;
            TheaterDotLxFileRecord lRecord;
            var palette = bmp.Palette;
            for (var i = 0; i < 256; i++)
            {
                palette.Entries[i] = mapInfo.Pallete[i];
            }
            bmp.Palette = palette;

            var bmpLock = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                                       bmp.PixelFormat);
            var scan0 = bmpLock.Scan0;
            var startPtr = scan0.ToPointer();
            var height = bmp.Height;
            var width = bmp.Width;
            for (var blockRow = 0; blockRow < ((int) mapInfo.LODMapHeights[lodInfo.LoDLevel]); blockRow++)
            {
                for (var blockCol = 0; blockCol < (mapInfo.LODMapWidths[lodInfo.LoDLevel]); blockCol++)
                {
                    var oIndex = (int) (blockRow*mapInfo.LODMapHeights[lodInfo.LoDLevel]) + blockCol;
                    block = lodInfo.O[oIndex];
                    for (var postRow = 0; postRow < postsAcross; postRow++)
                    {
                        for (var postCol = 0; postCol < postsAcross; postCol++)
                        {
                            var lIndex =
                                (int)
                                (((block.LRecordStartingOffset/(lodInfo.LRecordSizeBytes*postsAcross*postsAcross))*
                                  postsAcross*postsAcross) + ((postRow*postsAcross) + postCol));
                            lRecord = lodInfo.L[lIndex];
                            var xCoord = (blockCol*postsAcross) + postCol;
                            var yCoord = height - 1 - (blockRow*postsAcross) - postRow;
                            *((byte*) startPtr + (yCoord*width) + xCoord) = lRecord.Pallete;
                        }
                    }
                }
            }
            bmp.UnlockBits(bmpLock);
            _theaterMaps[lod] = bmp;
            return bmp;
        }

        public string DetectCurrentTheaterName()
        {
            string theaterName = null;
            var currentDataFormat = Process.Util.DetectFalconFormat();
            FileVersionInfo verInfo = null;
            var exePath = Process.Util.GetFalconExePath();
            if (exePath != null) verInfo = FileVersionInfo.GetVersionInfo(exePath);

            if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.AlliedForce)
            {
                try
                {
                    if (exePath != null)
                    {
                        exePath = Path.GetDirectoryName(exePath);
                        var configFolder = exePath + Path.DirectorySeparatorChar + "config";
                        using (
                            var reader =
                                File.OpenText(configFolder + Path.DirectorySeparatorChar + "options.cfg"))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                if (line == null) continue;
                                line = line.Trim();
                                if (line.StartsWith("gs_curTheater"))
                                {
                                    var equalsLoc = line.IndexOf('=');
                                    if (equalsLoc >= 13)
                                    {
                                        theaterName = line.Substring(equalsLoc + 1, line.Length - equalsLoc - 1);
                                        theaterName = theaterName.Trim();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    theaterName = null;
                }
            }
            else if (currentDataFormat.HasValue && currentDataFormat.Value == FalconDataFormats.BMS4 && verInfo != null &&
                     ((verInfo.ProductMajorPart == 4 && verInfo.ProductMinorPart >= 6826) ||
                      (verInfo.ProductMajorPart > 4)))
            {
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Benchmark Sims");
                    if (key != null)
                    {
                        var subkeys = key.GetSubKeyNames();
                        if (subkeys != null && subkeys.Length > 0)
                        {
                            foreach (var subkey in subkeys)
                            {
                                var toRead = key.OpenSubKey(subkey, false);
                                if (toRead != null)
                                {
                                    var baseDir = (string) toRead.GetValue("baseDir", null);
                                    var exePathFI = new FileInfo(exePath);
                                    if (baseDir != null && exePathFI.Directory.Parent.Parent.FullName.Equals(baseDir))
                                    {
                                        theaterName= (string) toRead.GetValue("curTheater", null);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(theaterName)) theaterName = theaterName.Trim();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    theaterName = null;
                }
            }
            else
            {
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MicroProse\Falcon\4.0");
                    if (key != null) theaterName = (string) key.GetValue("curTheater");
                    if (!string.IsNullOrEmpty(theaterName)) theaterName = theaterName.Trim();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    theaterName = null;
                }
            }
            return theaterName;
        }

        private TheaterDotTdfFileInfo GetCurrentTheaterDotTdf(string exePath, FalconDataFormats version)
        {
            if (exePath == null) return null;
            var currentTheaterName = DetectCurrentTheaterName();
            if (currentTheaterName == null) return null;
            var f4BaseDir = new FileInfo(exePath).DirectoryName;
            FileInfo theaterDotLstFI;
            
            theaterDotLstFI = new FileInfo(f4BaseDir + Path.DirectorySeparatorChar + "theater.lst");
            if (!theaterDotLstFI.Exists)
            {
                theaterDotLstFI =
                    new FileInfo(f4BaseDir + Path.DirectorySeparatorChar +
                                    "terrdata\\theaterdefinition\\theater.lst");
            }
            if (!theaterDotLstFI.Exists)
            {
                theaterDotLstFI =
                    new FileInfo(new DirectoryInfo(f4BaseDir).Parent.Parent.FullName + Path.DirectorySeparatorChar +
                                    "data\\terrdata\\theaterdefinition\\theater.lst");
            }

            if (theaterDotLstFI.Exists)
            {
                using (var fs = new FileStream(theaterDotLstFI.FullName, FileMode.Open))
                using (var sw = new StreamReader(fs))
                {
                    while (!sw.EndOfStream)
                    {
                        var thisLine = sw.ReadLine();
                        var tdfDetailsThisLine =
                            ReadTheaterDotTdf(f4BaseDir + Path.DirectorySeparatorChar + thisLine);

                        if (tdfDetailsThisLine == null)
                        {
                            tdfDetailsThisLine = ReadTheaterDotTdf(f4BaseDir + Path.DirectorySeparatorChar + "..\\..\\data" + Path.DirectorySeparatorChar + thisLine);
                        }
                        if (tdfDetailsThisLine != null)
                        {
                            if (tdfDetailsThisLine.theaterName != null &&
                                tdfDetailsThisLine.theaterName.ToLower().Trim() ==
                                currentTheaterName.ToLower().Trim())
                            {
                                return tdfDetailsThisLine;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static TheaterDotTdfFileInfo ReadTheaterDotTdf(string path)
        {
            if (String.IsNullOrEmpty(path)) return null;

            var basePathFI = new FileInfo(path);
            if (!basePathFI.Exists) return null;

            var toReturn = new TheaterDotTdfFileInfo();
            using (var fs = new FileStream(path, FileMode.Open))
            using (var sw = new StreamReader(fs))
            {
                while (!sw.EndOfStream)
                {
                    var thisLine = sw.ReadLine();
                    var thisLineTokens = Common.Strings.Util.Tokenize(thisLine);
                    if (thisLineTokens.Count > 0)
                    {
                        if (thisLineTokens[0].ToLower() == "name")
                        {
                            toReturn.theaterName = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "desc")
                        {
                            toReturn.theaterDesc = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "bitmap")
                        {
                            toReturn.bitmap = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "campaigndir")
                        {
                            toReturn.campaignDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "terraindir")
                        {
                            toReturn.terrainDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "artdir")
                        {
                            toReturn.artDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "moviedir")
                        {
                            toReturn.movieDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "uisounddir")
                        {
                            toReturn.uiSoundDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "objectdir")
                        {
                            toReturn.objectDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "misctexdir")
                        {
                            toReturn.miscTextDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "3ddatadir")
                        {
                            toReturn.ThreeDeeDataDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "mintacan")
                        {
                            toReturn.minTacan = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "sounddir")
                        {
                            toReturn.soundDir = JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "acmidir")
                        {
                            toReturn.acmiDir = JoinTokens(thisLineTokens, true);
                        }
                    }
                }
            }
            return toReturn;
        }

        private static string JoinTokens(List<string> tokens, bool omitFirstToken)
        {
            string toReturn = null;
            if (tokens != null && tokens.Count > 0)
            {
                var sb = new StringBuilder();
                var first = true;
                foreach (var st in tokens)
                {
                    if (omitFirstToken && first)
                    {
                        first = false;
                        continue;
                    }
                    sb.Append(st);
                    sb.Append(" ");
                    first = false;
                }
                toReturn = sb.ToString().Trim();
            }
            return toReturn;
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
            int col;
            int row;

            var feetAcross = GetNumFeetBetweenElevationPosts(0);

            //determine the column and row in the DTED matrix where the nearest elevation post can be found
            GetNearestElevationPostColumnAndRowForNorthEastCoordinates(feetNorth, feetEast, out col, out row);

            //retrieve the 4 elevation posts which form a box around our current position (origin point x=0,y=0 is in lower left)
            var Q11 = GetElevationPostRecordByColumnAndRow(col, row, 0);
            var Q21 = GetElevationPostRecordByColumnAndRow(col + 1, row, 0);
            var Q22 = GetElevationPostRecordByColumnAndRow(col + 1, row + 1, 0);
            var Q12 = GetElevationPostRecordByColumnAndRow(col, row + 1, 0);

            //determine the North/East coordinates of these 4 posts, respectively
            var Q11North = row*feetAcross;
            var Q11East = col*feetAcross;
            float FQ11 = Q11.Elevation;

            var Q21East = (col + 1)*feetAcross;
            float FQ21 = Q21.Elevation;

            float FQ22 = Q22.Elevation;

            var Q12North = (row + 1)*feetAcross;
            float FQ12 = Q12.Elevation;

            //perform bilinear interpolation on the 4 outer elevation posts relative to our actual center post
            //see: http://en.wikipedia.org/wiki/Bilinear_interpolation

            var x = feetEast;
            var y = feetNorth;

            var x1 = Q11East;
            var x2 = Q21East;
            var y1 = Q11North;
            var y2 = Q12North;

            var result =
                (
                    ((FQ11/((x2 - x1)*(y2 - y1)))*(x2 - x)*(y2 - y))
                    +
                    ((FQ21/((x2 - x1)*(y2 - y1)))*(x - x1)*(y2 - y))
                    +
                    ((FQ12/((x2 - x1)*(y2 - y1)))*(x2 - x)*(y - y1))
                    +
                    ((FQ22/((x2 - x1)*(y2 - y1)))*(x - x1)*(y - y1))
                );

            return result;
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
            if (_theaterDotLxFiles == null) return 0;
            var lodInfo = _theaterDotLxFiles[0];
            var mapInfo = _theaterDotMapFileInfo;
            var feetBetweenPosts = mapInfo.FeetBetweenL0Posts;
            for (var i = 1; i <= lodInfo.LoDLevel; i++)
            {
                feetBetweenPosts *= 2;
            }
            return feetBetweenPosts;
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
            var theatreOriginLatitudeInDegrees = _theaterDotMapFileInfo.baseLat;
            var theatreOriginLongitudeInDegrees = _theaterDotMapFileInfo.baseLong;
            const float earthEquatorialRadiusInFeet = 2.09257E7F;
            const float feetPerMinuteOfLatLongAtEquator = 6087.03141F;
            const float feetPerDegreeOfLatLongAtEquator = feetPerMinuteOfLatLongAtEquator*60.0F;
            const float radiansPerDegree = 0.01745329f;
            const float degreesPerRadian = 57.295780f;
            const float degreesPerMinute = 60.00f;

            var latitudeInRadians = (theatreOriginLatitudeInDegrees*feetPerDegreeOfLatLongAtEquator + feetNorth)/
                                    earthEquatorialRadiusInFeet;
            var cosineOfLatitude = (float) Math.Cos(latitudeInRadians);
            var longitudeInRadians = ((theatreOriginLongitudeInDegrees*radiansPerDegree*earthEquatorialRadiusInFeet*
                                       cosineOfLatitude) + feetEast)/(earthEquatorialRadiusInFeet*cosineOfLatitude);

            var latitudeInDegrees = latitudeInRadians*degreesPerRadian;
            var longitudeInDegrees = longitudeInRadians*degreesPerRadian;

            longitudeWholeDegrees = (int) Math.Floor(longitudeInDegrees);
            longitudeFactionalMinutes = Math.Abs(longitudeInDegrees - longitudeWholeDegrees)*degreesPerMinute;

            latitudeWholeDegrees = (int) Math.Floor(latitudeInDegrees);
            latitudeFractionalMinutes = Math.Abs(latitudeInDegrees - latitudeWholeDegrees)*degreesPerMinute;
        }

        public void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast,
                                                                               out int col, out int row)
        {
            const int lod = 0;
            var feetBetweenElevationPosts = GetNumFeetBetweenElevationPosts(lod);
            col = (int) Math.Floor(feetEast/feetBetweenElevationPosts);
            row = (int) Math.Floor(feetNorth/feetBetweenElevationPosts);
            ClampElevationPostCoordinates(ref row, ref col, lod);
        }

        public TheaterDotLxFileRecord GetElevationPostRecordByNorthEastCoordinate(float feetNorth, float feetEast)
        {
            int col;
            int row;
            GetNearestElevationPostColumnAndRowForNorthEastCoordinates(feetNorth, feetEast, out col, out row);
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
            var lodInfo = _theaterDotLxFiles[lod];
            var mapInfo = _theaterDotMapFileInfo;
            const int postsAcross = Constants.NUM_ELEVATION_POSTS_ACROSS_SINGLE_LOD_SEGMENT;
            ClampElevationPostCoordinates(ref postColumn, ref postRow, lodInfo.LoDLevel);
            var blockRow = (int) Math.Floor((postRow/(float) postsAcross));
            var blockCol = (int) Math.Floor((postColumn/(float) postsAcross));
            var oIndex = (int) (blockRow*mapInfo.LODMapHeights[lodInfo.LoDLevel]) + blockCol;
            var block = lodInfo.O[oIndex];
            var col = (postColumn%postsAcross);
            var row = (postRow%postsAcross);
            var lIndex =
                (int)
                (((block.LRecordStartingOffset/(lodInfo.LRecordSizeBytes*postsAcross*postsAcross))*postsAcross*
                  postsAcross) + ((row*postsAcross) + col));
            var lRecord = lodInfo.L[lIndex];
            return lRecord;
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

        #region Nested type: LodTextureKey

        [Serializable]
        private class LodTextureKey
        {
            public uint Lod;
            public uint chunkXIndex;
            public uint chunkYIndex;
            public uint textureId;

            #region Object Overrides (ToString, GetHashCode, Equals)

            /// <summary>
            ///   Gets a textual representation of this object.
            /// </summary>
            /// <returns>a String containing a textual representation of this object.</returns>
            public override string ToString()
            {
                return (Common.Serialization.Util.ToRawBytes(this));
            }

            /// <summary>
            ///   Gets an integer (hash) representation of this object, 
            ///   for use in hashtables.  If two objects are equal, 
            ///   then their hashcodes should be equal as well.
            /// </summary>
            /// <returns>an integer containing a hashed representation of this object</returns>
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            /// <summary>
            ///   Compares two objects to determine if they are equal to each other.
            /// </summary>
            /// <param name = "obj">An object to compare this instance to</param>
            /// <returns>a boolean, set to true if the specified object is 
            ///   equal to this instance, or false if the specified object
            ///   is not equal.</returns>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (GetType() != obj.GetType()) return false;
                if (ToString() != obj.ToString()) return false;
                return true;
            }

            #endregion

            public LodTextureKey()
            {
            }

            public LodTextureKey(uint lod, uint textureId, uint chunkXIndex, uint chunkYIndex) : this()
            {
                Lod = lod;
                this.textureId = textureId;
                this.chunkXIndex = chunkXIndex;
                this.chunkYIndex = chunkYIndex;
            }
        }

        #endregion
    }
}