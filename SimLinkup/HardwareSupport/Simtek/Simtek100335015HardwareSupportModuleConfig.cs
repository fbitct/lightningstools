using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100335015HardwareSupportModuleConfig
    {
        public Simtek100335015HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek100335015HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100335015HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek100335015HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
