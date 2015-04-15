using System;

namespace SimLinkup.HardwareSupport.Malwin
{
    [Serializable]
    public class Malwin19581HardwareSupportModuleConfig
    {
        public static Malwin19581HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Malwin19581HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}