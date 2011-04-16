using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MFDExtractor.Properties;

namespace MFDExtractor
{
    internal class SettingsHelper
    {
        private static readonly object _serializationLock = new object();
        private static readonly ILog _log = LogManager.GetLogger(typeof (SettingsHelper));

        private static void LogSaveSettings()
        {
            _log.InfoFormat("Saving settings in method {0} at {1}\nStack Trace:\n({2})",
                            MethodBase.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0));
        }

        private static void LogLoadSettings()
        {
            _log.InfoFormat("Loading settings in method {0} at {1}\nStack Trace:\n{2}",
                            MethodBase.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0));
        }

        public static void SaveSetttings()
        {
            lock (_serializationLock)
            {
                LogSaveSettings();
                Settings.Default.Save();
            }
        }

        public static void ReloadSettings()
        {
            lock (_serializationLock)
            {
                LogLoadSettings();
                Settings.Default.Reload();
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