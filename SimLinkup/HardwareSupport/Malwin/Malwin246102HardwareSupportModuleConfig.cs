using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Malwin246102HardwareSupportModuleConfig
    {
        public static Malwin246102HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Malwin246102HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}