using System;

namespace SimLinkup.HardwareSupport.AMI
{
    [Serializable]
    public class AMI900278002HardwareSupportModuleConfig
    {
        public static AMI900278002HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<AMI900278002HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}