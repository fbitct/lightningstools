using System.IO;
using F4Utils.Terrain.Structs;

namespace F4Utils.Terrain
{
    internal interface ICurrentTheaterDotTdfLoader
    {
        TheaterDotTdfFileInfo GetCurrentTheaterDotTdf(string exePath);
    }
    internal class CurrentTheaterDotTdfLoader:ICurrentTheaterDotTdfLoader
    {
        private readonly ICurrentTheaterNameDetector _currentTheaterNameDetector;
        private readonly ITheaterDotTdfFileReader _theaterDotTdfFileReader;
        public CurrentTheaterDotTdfLoader(ICurrentTheaterNameDetector currentTheaterNameDetector = null,
            ITheaterDotTdfFileReader theaterDotTdfFileReader = null)
        {
            _currentTheaterNameDetector = currentTheaterNameDetector ?? new CurrentTheaterNameDetector();
            _theaterDotTdfFileReader = theaterDotTdfFileReader ?? new TheaterDotTdfFileReader();
        }
        public TheaterDotTdfFileInfo GetCurrentTheaterDotTdf(string exePath)
        {
            if (exePath == null) return null;
            var currentTheaterName = _currentTheaterNameDetector.DetectCurrentTheaterName(exePath);
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
                            _theaterDotTdfFileReader.ReadTheaterDotTdfFile(f4BaseDir + Path.DirectorySeparatorChar + thisLine);

                        if (tdfDetailsThisLine == null)
                        {
                            tdfDetailsThisLine = _theaterDotTdfFileReader.ReadTheaterDotTdfFile(f4BaseDir + Path.DirectorySeparatorChar + "..\\..\\data" + Path.DirectorySeparatorChar + thisLine);
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

    }
}
