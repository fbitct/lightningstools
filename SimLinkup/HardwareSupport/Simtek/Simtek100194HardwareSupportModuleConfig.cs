using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100194HardwareSupportModuleConfig
    {
        public static Simtek100194HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100194HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}