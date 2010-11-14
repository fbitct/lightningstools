using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek101082HardwareSupportModuleConfig
    {
        public Simtek101082HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek101082HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek101082HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek101082HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
