using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100207HardwareSupportModuleConfig
    {
        public Simtek100207HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek100207HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100207HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek100207HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
