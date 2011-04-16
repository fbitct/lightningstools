using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100216HardwareSupportModuleConfig
    {
        public static Simtek100216HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100216HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}