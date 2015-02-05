using System;
using System.IO;
using System.Linq;
using System.Text;
using F4Utils.Campaign;
using F4Utils.Terrain;
using F4Utils.Terrain.Structs;

namespace MFDExtractor.BMSSupport
{
    internal interface IThreeDeeCockpitFileFinder2
    {
        FileInfo FindThreeDeeCockpitFile(int vehicleACD);
    }
    class ThreeDeeCockpitFileFinder2 : IThreeDeeCockpitFileFinder2
    {
        private readonly ICurrentTheaterDotTdfLoader _currentTheaterDotTdfLoader;
        private readonly IBMSRunningExecutableLocator _bmsRunningExecutableLocator;
        private VehicleClassDataType[] _vehicleDataTable;
        private Falcon4EntityClassType[] _classTable;
        private TheaterDotTdfFileInfo _theaterDotTdfFileInfo;
        public ThreeDeeCockpitFileFinder2(IBMSRunningExecutableLocator bmsRunningExecutableLocator=null, ICurrentTheaterDotTdfLoader currentTheaterDotTdfLoader = null)
        {
            _bmsRunningExecutableLocator = bmsRunningExecutableLocator ?? new BMSRunningExecutableLocator();
            _currentTheaterDotTdfLoader = currentTheaterDotTdfLoader ?? new CurrentTheaterDotTdfLoader();
        }
        public FileInfo FindThreeDeeCockpitFile(int vehicleACD )
        {
            if (vehicleACD ==-1) return null;

            var exePath = _bmsRunningExecutableLocator.BMSExePath;
            if (exePath == null) return null;
            exePath += Path.DirectorySeparatorChar;
            var basePath = new DirectoryInfo(exePath).Parent.Parent.FullName + Path.DirectorySeparatorChar;
            var currentTheaterTdf = _theaterDotTdfFileInfo ?? (_theaterDotTdfFileInfo = _currentTheaterDotTdfLoader.GetCurrentTheaterDotTdf(exePath));
            var dataDir = Path.Combine(basePath, "data");
            var artDir = Path.Combine(dataDir, currentTheaterTdf.artDir);
            var objectDir = Path.Combine(dataDir, currentTheaterTdf.objectDir);
            var classTable = _classTable ??
                             (_classTable =
                                 ClassTable.ReadClassTable(Path.Combine(objectDir, "FALCON4.CT")));
            var vehicleDataTable = _vehicleDataTable ?? (_vehicleDataTable = new VcdFile(Path.Combine(objectDir, "FALCON4.VCD")).VehicleDataTable);
            var vehicleClass = classTable.Where(x => x.dataType == (byte)Data_Types.DTYPE_VEHICLE 
                && x.vuClassData.classInfo_[ (int)VuClassHierarchy.VU_DOMAIN ] == (byte)Classtable_Domains.DOMAIN_AIR  
                && x.vuClassData.classInfo_[ (int)VuClassHierarchy.VU_TYPE ] == (byte)Classtable_Types.TYPE_AIRPLANE  
                && x.vehicleDataIndex == vehicleACD).FirstOrDefault();
               
            var vehicleData = vehicleDataTable[vehicleClass.dataPtr];
            var vehicleName = Encoding.ASCII.GetString(vehicleData.Name).Substring(0, Array.IndexOf<byte>(vehicleData.Name, 0x00));
            var vehicleNCTR = Encoding.ASCII.GetString(vehicleData.NCTR).Substring(0, Array.IndexOf<byte>(vehicleData.NCTR, 0x00));
            var visType = vehicleClass.visType[0];

            var mainCkptArtFolder = Path.Combine(artDir, "ckptart");
            const string threeDeeCockpitDatFile = "3dckpit.dat";
            string file;
            if (visType == (short) Vis_Types.VIS_F16C)
            {
                file = Path.Combine(mainCkptArtFolder, threeDeeCockpitDatFile);
            }
            else
            {
                file = Path.Combine(mainCkptArtFolder, visType.ToString(), threeDeeCockpitDatFile);
                if (!FileExists(file))
                {
                    file = Path.Combine(mainCkptArtFolder, vehicleName, threeDeeCockpitDatFile);
                    if (!FileExists(file))
                    {
                        file = Path.Combine(mainCkptArtFolder, vehicleNCTR, threeDeeCockpitDatFile);
                        if (!FileExists(file))
                        {
                            file = Path.Combine(mainCkptArtFolder, threeDeeCockpitDatFile);
                        }
                    }
                }
            }
            var fi = new FileInfo(file);
            return FileExists(file) ? fi : null;
        }

        private bool FileExists(string fileName)
        {
            try
            {
                return File.Exists(fileName);
            }
            catch {}
            return false;
        }
    }
}
