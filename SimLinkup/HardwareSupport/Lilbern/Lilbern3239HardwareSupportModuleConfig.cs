using System;

namespace SimLinkup.HardwareSupport.Lilbern
{
    [Serializable]
    public class Lilbern3239HardwareSupportModuleConfig
    {
        public static Lilbern3239HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Lilbern3239HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}