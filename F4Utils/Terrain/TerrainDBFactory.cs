using F4Utils.Terrain.Structs;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    public interface ITerrainDBFactory
    {
        TerrainDB Create(bool loadAllLods);
    }
    public class TerrainDBFactory : ITerrainDBFactory
    {
        private readonly ICurrentTheaterDotTdfLoader _currentTheaterDotTdfLoader = new CurrentTheaterDotTdfLoader();
        private readonly ITheaterDotMapFileReader _theaterDotMapFileReader = new TheaterDotMapFileReader();
        private readonly ITextureDotBinFileReader _textureDotBinFileReader = new TextureDotBinFileReader();
        private readonly IFarTilesDotPalFileReader _farTilesDotPalFileReader = new FarTilesDotPalFileReader();
        private readonly ITheaterDotLxFileReader _theaterDotLxFileReader = new TheaterDotLxFileReader();

        public TerrainDB Create(bool loadAllLods)
        {
            var terrainDB = new TerrainDB();
            terrainDB.FalconExePath = GetFalconExePath();
            if (terrainDB.FalconExePath == null) return null;

            var currentTheaterTdf = _currentTheaterDotTdfLoader.GetCurrentTheaterDotTdf(terrainDB.FalconExePath);
            if (currentTheaterTdf == null) return null;
            
            terrainDB.DataPath = terrainDB.FalconExePath + "..\\..\\data";
            terrainDB.TerrainBasePath = terrainDB.DataPath + Path.DirectorySeparatorChar + currentTheaterTdf.terrainDir;
            terrainDB.CurrentTheaterTextureBaseFolderPath = terrainDB.TerrainBasePath + Path.DirectorySeparatorChar + "texture";
            terrainDB.TheaterDotMapFilePath = terrainDB.TerrainBasePath + Path.DirectorySeparatorChar + "terrain" + Path.DirectorySeparatorChar + "THEATER.MAP";
            terrainDB.TextureDotBinFilePath = terrainDB.CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +"TEXTURE.BIN";
            terrainDB.FarTilesDotRawFilePath = terrainDB.CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +"FARTILES.RAW";
            terrainDB.FarTilesDotDdsFilePath = terrainDB.CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar +"FARTILES.DDS";
            terrainDB.FarTilesDotPalFilePath= terrainDB.CurrentTheaterTextureBaseFolderPath + Path.DirectorySeparatorChar + "FARTILES.PAL";
            terrainDB.TheaterDotMap = _theaterDotMapFileReader.ReadTheaterDotMapFile(terrainDB.TheaterDotMapFilePath);
            terrainDB.TheaterDotLxFiles = new TheaterDotLxFileInfo[terrainDB.TheaterDotMap.NumLODs];
            terrainDB.TheaterMaps = new Bitmap[terrainDB.TheaterDotMap.NumLODs];
            if (loadAllLods)
            {
                Parallel.For(0, terrainDB.TheaterDotMap.NumLODs, i =>
                {
                    terrainDB.TheaterDotLxFiles[i] = _theaterDotLxFileReader.LoadTheaterDotLxFile((uint)i,
                        terrainDB.TheaterDotMapFilePath);
                });
            }
            else
            {
                terrainDB.TheaterDotLxFiles[0] = _theaterDotLxFileReader.LoadTheaterDotLxFile(0, terrainDB.TheaterDotMapFilePath);
            }
            terrainDB.TextureDotBin = _textureDotBinFileReader.ReadTextureDotBinFile(terrainDB.TextureDotBinFilePath);
            terrainDB.FarTilesDotPal = _farTilesDotPalFileReader.ReadFarTilesDotPalFile(terrainDB.FarTilesDotPalFilePath);
            return terrainDB;
        }

        private string GetFalconExePath()
        {
            //TODO: check these against other theaters, for correct way to read theater installation locations
            var exePath = Process.Util.GetFalconExePath();
            if (exePath == null) return null;
            var f4BasePathFI = new FileInfo(exePath);
            return f4BasePathFI.DirectoryName + Path.DirectorySeparatorChar;
        }
    }
}
