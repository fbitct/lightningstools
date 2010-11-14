using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek101084HardwareSupportModuleConfig
    {
        public Simtek101084HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek101084HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek101084HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek101084HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
