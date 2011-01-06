using System;
using System.Collections.Generic;
using System.Text;

namespace SimLinkup.HardwareSupport.Lilbern
{
    [Serializable]
    public class Lilbern3239HardwareSupportModuleConfig
    {
        public Lilbern3239HardwareSupportModuleConfig()
            : base()
        {
        }
        public static Lilbern3239HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Lilbern3239HardwareSupportModuleConfig>(filePath);
        }
        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile<Lilbern3239HardwareSupportModuleConfig>(this, filePath);
        }
    }
}
