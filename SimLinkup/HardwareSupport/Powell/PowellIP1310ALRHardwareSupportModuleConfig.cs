using System;

namespace SimLinkup.HardwareSupport.Malwin
{
    [Serializable]
    public class PowellIP1310ALRHardwareSupportModuleConfig
    {
        public static PowellIP1310ALRHardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<PowellIP1310ALRHardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}