using System;
using System.IO;
using System.Reflection;

namespace SimLinkup
{
    internal static class Util
    {
        public static string ApplicationDirectory
        {
            get { return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName; }
        }

        public static string ApplicationPath
        {
            get { return Assembly.GetExecutingAssembly().Location; }
        }
    }
}