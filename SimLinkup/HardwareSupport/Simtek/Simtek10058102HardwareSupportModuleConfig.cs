using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek10058102HardwareSupportModuleConfig
    {
        public Simtek10058102HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek10058102HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek10058102HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek10058102HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
