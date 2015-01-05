using System;

namespace SimLinkup.HardwareSupport.AMI
{
    [Serializable]
    public class AMI900158001HardwareSupportModuleConfig
    {
        public static AMI900158001HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<AMI900158001HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}