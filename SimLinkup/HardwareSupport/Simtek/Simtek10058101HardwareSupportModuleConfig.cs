using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek10058101HardwareSupportModuleConfig
    {
        public Simtek10058101HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Simtek10058101HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek10058101HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Simtek10058101HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
