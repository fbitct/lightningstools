using System;

namespace SimLinkup.HardwareSupport.Westin
{
    [Serializable]
    public class Westin521993HardwareSupportModuleConfig
    {
        public static Westin521993HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Westin521993HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}