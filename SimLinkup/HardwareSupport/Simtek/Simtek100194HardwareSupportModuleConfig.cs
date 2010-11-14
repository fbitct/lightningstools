using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100194HardwareSupportModuleConfig
    {
        public Simtek100194HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek100194HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100194HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek100194HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
