using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MFDExtractor.Properties;

namespace MFDExtractor
{
    internal class SettingsHelper
    {
        private static readonly object SerializationLock = new object();
        private static readonly ILog Log = LogManager.GetLogger(typeof (SettingsHelper));

        private static void LogSaveSettings()
        {
            Log.InfoFormat("Saving settings in method {0} at {1}\nStack Trace:\n({2})",
                            MethodBase.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0));
        }

        private static void LogLoadSettings()
        {
            Log.InfoFormat("Loading settings in method {0} at {1}\nStack Trace:\n{2}",
                            MethodBase.GetCurrentMethod().Name, DateTime.Now, new StackTrace().GetFrame(0));
        }

        public static void SaveSetttings()
        {
            lock (SerializationLock)
            {
                LogSaveSettings();
                Settings.Default.Save();
            }
        }

        public static void LoadSettings()
        {
            lock (SerializationLock)
            {
                LogLoadSettings();
                Settings.Default.Reload();
            }
        }

        public static void SaveAndReloadSettings()
        {
            lock (SerializationLock)
            {
                SaveSetttings();
                LoadSettings();
            }
        }
    }
}