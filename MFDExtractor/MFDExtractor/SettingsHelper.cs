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

        public static void SaveSetttings()
        {
            lock (SerializationLock)
            {
                Settings.Default.Save();
            }
        }

        public static void LoadSettings()
        {
            lock (SerializationLock)
            {
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