using System;

namespace SimLinkup.HardwareSupport.AMI
{
    [Serializable]
    public class AMI9001584HardwareSupportModuleConfig
    {
        public static AMI9001584HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<AMI9001584HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}