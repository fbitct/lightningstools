using F4SharedMem;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F4Utils.Terrain
{
    public interface ICurrentTheaterNameDetector
    {
        string DetectCurrentTheaterName();
    }
    public class CurrentTheaterNameDetector : Terrain.ICurrentTheaterNameDetector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurrentTheaterNameDetector));
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
                     ((verInfo.ProductMajorPart == 4 && verInfo.FileMinorPart >= 6826) ||
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
                                    var baseDir = (string)toRead.GetValue("baseDir", null);
                                    var exePathFI = new FileInfo(exePath);
                                    if (baseDir != null && exePathFI.Directory.Parent.Parent.FullName.Equals(baseDir))
                                    {
                                        theaterName = (string)toRead.GetValue("curTheater", null);
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
                    if (key != null) theaterName = (string)key.GetValue("curTheater");
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
    }
}
