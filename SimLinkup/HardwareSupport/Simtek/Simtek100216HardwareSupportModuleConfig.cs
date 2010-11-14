using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100216HardwareSupportModuleConfig
    {
        public Simtek100216HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek100216HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100216HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek100216HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
