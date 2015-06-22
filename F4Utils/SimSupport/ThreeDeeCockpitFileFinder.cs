﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using F4Utils.Campaign;
using F4Utils.Campaign.F4Structs;
using F4Utils.Terrain;
using F4Utils.Process;
using Common.Strings;
namespace F4Utils.SimSupport
{
    public interface IThreeDeeCockpitFileFinder2
    {
        FileInfo FindThreeDeeCockpitFile(int vehicleACD);
    }
    public class ThreeDeeCockpitFileFinder : IThreeDeeCockpitFileFinder2
    {
        private readonly ICurrentTheaterDotTdfLoader _currentTheaterDotTdfLoader;
        private readonly IBMSRunningExecutableLocator _bmsRunningExecutableLocator;
        public ThreeDeeCockpitFileFinder(IBMSRunningExecutableLocator bmsRunningExecutableLocator=null, ICurrentTheaterDotTdfLoader currentTheaterDotTdfLoader = null)
        {
            _bmsRunningExecutableLocator = bmsRunningExecutableLocator ?? new BMSRunningExecutableLocator();
            _currentTheaterDotTdfLoader = currentTheaterDotTdfLoader ?? new CurrentTheaterDotTdfLoader();
        }
        public FileInfo FindThreeDeeCockpitFile(int vehicleACD )
        {
            if (vehicleACD ==-1) return FindThreeDeeCockpitFileUsingOldMethod();

            var exePath = _bmsRunningExecutableLocator.BMSExePath;
            if (exePath == null) return null;
            exePath += Path.DirectorySeparatorChar;
            var bmsBaseDirectory = new DirectoryInfo(exePath).Parent.Parent.FullName + Path.DirectorySeparatorChar;
            var currentTheaterTdf = _currentTheaterDotTdfLoader.GetCurrentTheaterDotTdf(bmsBaseDirectory);
            var dataDir = Path.Combine(bmsBaseDirectory, "data");
            var artDir = Path.Combine(dataDir, currentTheaterTdf != null ? currentTheaterTdf.artDir ?? "art" : "art");
            var objectDir = Path.Combine(dataDir, currentTheaterTdf != null ? currentTheaterTdf.objectDir ?? @"terrdata\objects" : @"terrdata\objects");
            var classTable = ClassTable.ReadClassTable(Path.Combine(objectDir, "FALCON4.CT"));
            var vehicleDataTable = new VcdFile(Path.Combine(objectDir, "FALCON4.VCD")).VehicleDataTable;
            var vehicleClass = classTable.Where(x => x.dataType == (byte)Data_Types.DTYPE_VEHICLE 
                && x.vuClassData.classInfo_[ (int)VuClassHierarchy.VU_DOMAIN ] == (byte)Classtable_Domains.DOMAIN_AIR  
                && x.vuClassData.classInfo_[ (int)VuClassHierarchy.VU_TYPE ] == (byte)Classtable_Types.TYPE_AIRPLANE  
                && x.vehicleDataIndex == vehicleACD).FirstOrDefault();
               
            var vehicleData = vehicleDataTable[vehicleClass.dataPtr];
            var vehicleName = Encoding.ASCII.GetString(vehicleData.Name).TrimAtNull().Replace("*", "");
            var vehicleNCTR = Encoding.ASCII.GetString(vehicleData.NCTR).TrimAtNull().Replace("*", "");
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
            return FileExists(file) ? fi : FindThreeDeeCockpitFileUsingOldMethod();
        }

        private static bool FileExists(string fileName)
        {
            try
            {
                var invalidPathChars = Path.GetInvalidPathChars();
                var invalidFilenameChars = Path.GetInvalidFileNameChars();
                string pathPart = Path.GetDirectoryName(fileName);
                string fileNamePart = Path.GetFileName(fileName);
                if (
                    invalidPathChars.Intersect(pathPart.ToCharArray()).Count() > 0 
                        ||
                    invalidFilenameChars.Intersect(fileNamePart.ToCharArray()).Count() > 0)
                {
                    return false;
                }
                return File.Exists(fileName);
            }
            catch {}
            return false;
        }


        private FileInfo FindThreeDeeCockpitFileUsingOldMethod()
        {
            var basePath = _bmsRunningExecutableLocator.BMSExePath;
            return basePath == null ? null : Paths(basePath).FirstOrDefault(x => x != null);
        }
        private static IEnumerable<FileInfo> Paths(string basePath)
        {
            yield return SearchIn(Path.Combine(basePath, @"..\..\Data\art\ckptartn"), "3dckpit.dat");
            yield return SearchIn(Path.Combine(basePath, @"..\..\Data\art\ckptart"), "3dckpit.dat");
        }
        private static FileInfo SearchIn(string searchPath, string fileName)
        {
            var dir = new DirectoryInfo(searchPath);
            if (!dir.Exists) return null;
            var subdirectories = dir.GetDirectories();

            return subdirectories.Concat(new[] { dir })
                .Select(x => new FileInfo(Path.Combine(x.FullName, fileName)))
                .Where(FileExistsAndIsInUse)
                .FirstOrDefault();
        }
        private static bool FileExistsAndIsInUse(FileInfo file)
        {
            if (!FileExists(file.FullName)) return false;
            try
            {
                using (var filestream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    filestream.Close();
                }
            }
            catch (IOException) { return true; }
            return false;
        }
    }
}
