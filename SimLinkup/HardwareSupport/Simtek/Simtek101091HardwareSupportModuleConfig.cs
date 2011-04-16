using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek101091HardwareSupportModuleConfig
    {
        public static Simtek101091HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek101091HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}