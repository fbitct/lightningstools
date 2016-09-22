using System;
using System.IO;
using System.Reflection;

namespace SimLinkup
{
    internal static class Util
    {
        private static string _defaultProfile;
        public static string DefaultProfile
        {
            get
            {
                if (_defaultProfile == null)
                {
                    using (var reader = File.OpenText(Path.Combine(MappingBaseDirectory, "default.profile")))
                    {
                        _defaultProfile = reader.ReadToEnd();
                    }
                }
                return _defaultProfile;
            }
        }
        public static string CurrentMappingProfileDirectory
        {
            get { return Path.Combine(MappingBaseDirectory, DefaultProfile);  }
        }
        public static string MappingBaseDirectory
        {
            get { return Path.Combine(ContentDirectory, "Mapping"); }
        }
        public static string ContentDirectory
        {
            get { return Path.Combine(ApplicationDirectory, "Content"); }
        }
        public static string ApplicationDirectory
        {
            get { return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName; }
        }

        public static string ExePath
        {
            get { return Assembly.GetExecutingAssembly().Location; }
        }
    }
}