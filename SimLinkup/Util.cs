using System.IO;
using System.Reflection;

namespace SimLinkup
{
    static class Util
    {
        public static string ApplicationDirectory
        {
            get
            {
                return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            }
        }
        public static string ApplicationPath
        {
            get
            {
                return Assembly.GetExecutingAssembly().Location;
            }
        }
    }
}
