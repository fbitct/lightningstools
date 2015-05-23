using System;
using System.Xml.Serialization;

namespace SimLinkup.HardwareSupport.Powell
{
    [Serializable]
    public class PowellIP1310ALRHardwareSupportModuleConfig
    {
        public static PowellIP1310ALRHardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<PowellIP1310ALRHardwareSupportModuleConfig>(filePath);
        }
        public string DeviceID { get; set; }
        public string COMPort { get; set; }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}