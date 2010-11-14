using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek101091HardwareSupportModuleConfig
    {
        public Simtek101091HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek101091HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek101091HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek101091HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
