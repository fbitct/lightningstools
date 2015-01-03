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
            var exePath = Process.Util.GetFalconExePath();

           
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
            return theaterName;
        }
    }
}
