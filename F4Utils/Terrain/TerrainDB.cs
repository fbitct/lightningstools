using F4SharedMem;
using F4Utils.Terrain.Structs;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    public class TerrainDB
    {
        public TerrainDB()
        {
            ElevationPostTextures = new Dictionary<LodTextureKey, Bitmap>();
            FarTileTextures = new Dictionary<uint, Bitmap>();
            NearTileTextures = new Dictionary<uint, Bitmap>();
           TextureDotZipFileEntries = new Dictionary<string, ZipEntry>();
        }
   
        public string FalconExePath { get; set; }
        public string DataPath { get; set; }
        public string TerrainBasePath { get; set; }
        public string CurrentTheaterTextureBaseFolderPath { get; set; }
        public string FarTilesDotDdsFilePath { get; set; }
        public string FarTilesDotRawFilePath { get; set; }
        public string FarTilesDotPalFilePath { get; set; }
        public string TheaterDotBinFilePath { get; set; }
        public string TheaterDotMapFilePath { get; set; }
        public string TextureDotBinFilePath { get; set; }
        public Dictionary<LodTextureKey, Bitmap> ElevationPostTextures { get; set; }
        public Dictionary<uint, Bitmap> FarTileTextures { get; set; }
        public FarTilesDotPalFileInfo FarTilesDotPal { get; set; }
        public Dictionary<uint, Bitmap> NearTileTextures { get; set; }
        public TextureDotBinFileInfo TextureDotBin { get; set; }
        public Dictionary<string, ZipEntry> TextureDotZipFileEntries{get;set;}
        public ZipFile TextureZipFile {get;set;}
        public TheaterDotLxFileInfo[] TheaterDotLxFiles { get; set; }
        public TheaterDotMapFileInfo TheaterDotMap { get; set; }
        public Bitmap[] TheaterMaps { get; set; }

    }
}
