using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Malwin246102HardwareSupportModuleConfig
    {
        public Malwin246102HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Malwin246102HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Malwin246102HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Malwin246102HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
