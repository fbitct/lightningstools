using System.Collections.Generic;
using System.Drawing;
using F4Utils.Terrain.Structs;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using F4Utils.Process;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    public class TerrainDB
    {
        private readonly ITextureDotBinFileReader _textureDotBinFileReader = new TextureDotBinFileReader();
        private readonly IFarTilesDotPalFileReader _farTilesDotPalFileReader = new FarTilesDotPalFileReader();
        private readonly ICurrentTheaterDotTdfLoader _currentTheaterDotTdfLoader = new CurrentTheaterDotTdfLoader();
        private readonly ITheaterDotMapFileReader _theaterDotMapFileReader = new TheaterDotMapFileReader();
        private readonly ITheaterDotLxFileReader _theaterDotLxFileReader = new TheaterDotLxFileReader();
        private readonly IDetailTextureForElevationPostRetriever _detailTextureForElevationPostRetriever;
        private readonly ITheaterMapBuilder _theaterMapBuilder;
        private readonly ITerrainHeightCalculator _terrainHeightCalculator;
        private readonly IElevationPostCoordinateClamper _elevationPostCoordinateClamper;
        private readonly IDistanceBetweenElevationPostsCalculator _distanceBetweenElevationPostsCalculator;
        private readonly INearestElevationPostColumnAndRowCalculator _nearestElevationPostColumnAndRowCalculator;
        private readonly ITerrainTextureByTextureIdRetriever _terrainTextureByTextureIdRetriever;
        private readonly INearTileTextureLoader _nearTileTextureLoader;
        private readonly IFarTileTextureRetriever _farTileTextureRetriever;
        private readonly ColumnAndRowElevationPostRecordRetriever _columnAndRowElevationPostRecordRetriever;
        private readonly ILatLongCalculator _latLongCalculator;
        private Dictionary<string, ZipEntry> _textureDotZipFileEntries = new Dictionary<string, ZipEntry>();
        private TheaterDotTdfFileInfo _theaterDotTdf;
        private TheaterDotLxFileInfo[] _theaterDotLxFiles;
        private Nullable<TheaterDotMapFileInfo> _theaterDotMap;
        private Nullable<TextureDotBinFileInfo> _textureDotBin;
        private Nullable<FarTilesDotPalFileInfo> _farTilesDotPal;
        private ZipFile _textureZipFile;
        private bool _loadAllLods;

        private TerrainDB() {}
        public TerrainDB(string falconExePath, bool loadAllLods = true):this()
        {
            if (falconExePath == null) throw new ArgumentNullException("falconExePath");
            if (!falconExePath.EndsWith(Path.DirectorySeparatorChar.ToString())) falconExePath += Path.DirectorySeparatorChar;
            FalconExePath = falconExePath;
            _loadAllLods = loadAllLods;
            _elevationPostCoordinateClamper = new ElevationPostCoordinateClamper(this);
            _columnAndRowElevationPostRecordRetriever = new ColumnAndRowElevationPostRecordRetriever(this, _elevationPostCoordinateClamper);
            _nearTileTextureLoader = new NearTileTextureLoader(this);
            _farTileTextureRetriever = new FarTileTextureRetriever(this);
            _theaterMapBuilder = new TheaterMapBuilder(this);
            _distanceBetweenElevationPostsCalculator = new DistanceBetweenElevationPostsCalculator(this);
            _terrainTextureByTextureIdRetriever = new TerrainTextureByTextureIdRetriever(this, _nearTileTextureLoader, _farTileTextureRetriever);
            _detailTextureForElevationPostRetriever = new DetailTextureForElevationPostRetriever(this, _elevationPostCoordinateClamper, _terrainTextureByTextureIdRetriever, _columnAndRowElevationPostRecordRetriever);
            _nearestElevationPostColumnAndRowCalculator = new NearestElevationPostColumnAndRowCalculator(this, _distanceBetweenElevationPostsCalculator, _elevationPostCoordinateClamper);
            _terrainHeightCalculator = new TerrainHeightCalculator(this, _columnAndRowElevationPostRecordRetriever, _distanceBetweenElevationPostsCalculator, _nearestElevationPostColumnAndRowCalculator);
            _latLongCalculator = new LatLongCalculator(this);
        }

        public TheaterDotTdfFileInfo TheaterDotTdf
        {
            get
            {
                if (_theaterDotTdf == null)
                {
                    _theaterDotTdf = _currentTheaterDotTdfLoader.GetCurrentTheaterDotTdf(FalconExePath);
                }
                return _theaterDotTdf;
            }
        }
        public string FalconExePath { get; private set; }
        public string DataPath { get { return FalconExePath + "..\\..\\data"; } }
        public string TerrainBasePath { get { return DataPath + Path.DirectorySeparatorChar + TheaterDotTdf.terrainDir; } }
        public string CurrentTheaterTextureBaseFolderPath { get { return TerrainBasePath + Path.DirectorySeparatorChar + "texture"; } }
        public string FarTilesDotDdsFilePath { get { return CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "FARTILES.DDS"; } }
        public string FarTilesDotRawFilePath { get { return CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "FARTILES.RAW"; } }
        public string FarTilesDotPalFilePath { get { return CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "FARTILES.PAL"; } }
        public string TheaterDotMapFilePath { get { return TerrainBasePath + Path.DirectorySeparatorChar + "terrain" + Path.DirectorySeparatorChar + "THEATER.MAP"; } }
        public string TextureDotBinFilePath { get { return CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "TEXTURE.BIN"; } }
        public FarTilesDotPalFileInfo FarTilesDotPal 
        {
            get
            {
                if (!_farTilesDotPal.HasValue)
                {
                    _farTilesDotPal = _farTilesDotPalFileReader.ReadFarTilesDotPalFile(FarTilesDotPalFilePath);
                }
                return _farTilesDotPal.Value;
            }
        }
        public TextureDotBinFileInfo TextureDotBin 
        { 
            get 
            {
                if (!_textureDotBin.HasValue)
                {
                    _textureDotBin = _textureDotBinFileReader.ReadTextureDotBinFile(TextureDotBinFilePath);
                }
                return _textureDotBin.Value;
            }
        }
        public Dictionary<string, ZipEntry> TextureDotZipFileEntries 
        { 
            get 
            {
                if (_textureDotZipFileEntries == null || _textureDotZipFileEntries.Count == 0)
                {
                    _textureDotZipFileEntries = Common.Compression.Zip.Util.GetZipFileEntries(TextureZipFile);
                }
                return _textureDotZipFileEntries;
            } 
        }
        public ZipFile TextureZipFile 
        {
            get
            {
                if (_textureZipFile == null)
                {
                    _textureZipFile = new ZipFile(CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "texture.zip");
                }
                return _textureZipFile;
            }
        }
        public TheaterDotLxFileInfo[] TheaterDotLxFiles 
        { 
            get 
            {
                if (_theaterDotLxFiles == null)
                {
                    _theaterDotLxFiles = new TheaterDotLxFileInfo[TheaterDotMap.NumLODs];
                    if (_loadAllLods)
                    {
                        Parallel.For(0, TheaterDotMap.NumLODs, i =>
                        {
                            _theaterDotLxFiles[i] = _theaterDotLxFileReader.LoadTheaterDotLxFile((uint)i, TheaterDotMapFilePath);
                        });
                    }
                    else
                    {
                        TheaterDotLxFiles[0] = _theaterDotLxFileReader.LoadTheaterDotLxFile(0, TheaterDotMapFilePath);
                    }
                }
                return _theaterDotLxFiles;
            } 
        }
        public TheaterDotMapFileInfo TheaterDotMap
        {
            get
            {
                if (!_theaterDotMap.HasValue)
                {
                    _theaterDotMap = _theaterDotMapFileReader.ReadTheaterDotMapFile(TheaterDotMapFilePath);
                }
                return _theaterDotMap.Value;
            }
        }
        public float GetDistanceInFeetBetweenElevationPosts(int lod)
        {
            return _distanceBetweenElevationPostsCalculator.GetNumFeetBetweenElevationPosts(lod);
        }
        public void ClampElevationPostCoordinates(ref int postColumn, ref int postRow, uint lod)
        {
            _elevationPostCoordinateClamper.ClampElevationPostCoordinates(ref postColumn, ref postRow, lod);
        }
        public void GetNearestElevationPostColumnAndRowForNorthEastCoordinates(float feetNorth, float feetEast, out int col, out int row)
        {
            _nearestElevationPostColumnAndRowCalculator.GetNearestElevationPostColumnAndRowForNorthEastCoordinates(feetNorth, feetEast, out col, out row);
        }
        public TheaterDotLxFileRecord GetElevationPostRecordByColumnAndRow(int postColumn, int postRow, uint lod)
        {
            return _columnAndRowElevationPostRecordRetriever.GetElevationPostRecordByColumnAndRow(postColumn, postRow, lod);
        }
        public Bitmap GetDetailTextureForElevationPost(int postCol, int postRow, uint lod) 
        {
            return _detailTextureForElevationPostRetriever.GetDetailTextureForElevationPost(postCol, postRow, lod);
        }
        public void CalculateLatLong(float feetNorth, float feetEast, out int latitudeWholeDegrees,
                                    out float latitudeFractionalMinutes, out int longitudeWholeDegrees,
                                    out float longitudeFactionalMinutes)
        {
            _latLongCalculator.CalculateLatLong(feetNorth, feetEast, out latitudeWholeDegrees, out latitudeFractionalMinutes, out longitudeWholeDegrees, out longitudeFactionalMinutes);
        }
        public float CalculateTerrainHeight(float feetNorth, float feetEast)
        {
            return _terrainHeightCalculator.CalculateTerrainHeight(feetNorth, feetEast);
        }
        public Bitmap GetNearTileTexture(string tileName)
        {
            return _nearTileTextureLoader.LoadNearTileTexture(tileName);
        }
        public Bitmap GetFarTileTexture(uint textureId)
        {
            return _farTileTextureRetriever.GetFarTileTexture(textureId);
        }
        public Bitmap GetTerrainTextureByTextureId(uint textureId, uint lod)
        {
            return _terrainTextureByTextureIdRetriever.GetTerrainTextureByTextureId(textureId, lod);
        }
        public Bitmap GetTheaterMapImage(uint lod)
        {
            return _theaterMapBuilder.GetTheaterMap(lod);
        }

    }
}
