using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Astronautics
{
    [Serializable]
    public class Astronautics12871HardwareSupportModuleConfig
    {
        public Astronautics12871HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Astronautics12871HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Astronautics12871HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Astronautics12871HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
