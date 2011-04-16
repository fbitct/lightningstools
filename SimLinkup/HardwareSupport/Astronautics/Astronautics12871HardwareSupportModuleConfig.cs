using System;

namespace SimLinkup.HardwareSupport.Astronautics
{
    [Serializable]
    public class Astronautics12871HardwareSupportModuleConfig
    {
        public static Astronautics12871HardwareSupportModuleConfig Load(string filePath)
        {
            return
                Common.Serialization.Util.DeserializeFromXmlFile<Astronautics12871HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}