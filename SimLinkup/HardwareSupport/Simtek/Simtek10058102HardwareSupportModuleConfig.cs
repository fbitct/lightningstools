using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek10058102HardwareSupportModuleConfig
    {
        public static Simtek10058102HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek10058102HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}