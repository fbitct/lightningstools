using System;
using System.IO;
using F4Utils.Process;
using log4net;
using Microsoft.Win32;

namespace F4Utils.Terrain
{
    internal interface ICurrentTheaterNameDetector
    {
        string DetectCurrentTheaterName(string exePath);
    }
    internal class CurrentTheaterNameDetector : ICurrentTheaterNameDetector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurrentTheaterNameDetector));
        public string DetectCurrentTheaterName(string exePath)
        {
            string theaterName = null;
           
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
