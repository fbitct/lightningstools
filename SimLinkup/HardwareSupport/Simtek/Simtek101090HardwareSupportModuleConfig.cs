using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek101090HardwareSupportModuleConfig
    {
        public Simtek101090HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek101090HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek101090HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek101090HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
