using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek10058101HardwareSupportModuleConfig
    {
        public static Simtek10058101HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek10058101HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}