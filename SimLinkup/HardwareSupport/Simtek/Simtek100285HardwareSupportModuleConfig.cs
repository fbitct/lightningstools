using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100285HardwareSupportModuleConfig
    {
        public Simtek100285HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek100285HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100285HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek100285HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
