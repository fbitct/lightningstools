using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Diagnostics;

namespace MFDExtractor
{
    internal class SettingsHelper
    {
        private static object _serializationLock = new object();
        private static ILog _log = LogManager.GetLogger(typeof(SettingsHelper));
        private static void LogSaveSettings() 
        {
            _log.InfoFormat("Saving settings in method {0} at {1}\nStack Trace:\n({2})", MethodInfo.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0).ToString());
        }
        private static void LogLoadSettings()
        {
            _log.InfoFormat("Loading settings in method {0} at {1}\nStack Trace:\n{2}", MethodInfo.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0).ToString());
        }
        public static void SaveSetttings()
        {
            lock (_serializationLock)
            {
                LogSaveSettings();
                Properties.Settings.Default.Save();

            }
        }
        public static void ReloadSettings()
        {
            lock (_serializationLock)
            {
                LogLoadSettings();
               // Properties.Settings.Default.Reload();
            }
        }
        public static void SaveAndReloadSettings()
        {
            lock (_serializationLock)
            {
                SaveSetttings();
                LogLoadSettings();
            }
        }
    }
}
