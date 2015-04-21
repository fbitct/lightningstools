using System;

namespace SimLinkup.HardwareSupport.Gould
{
    [Serializable]
    public class GouldHS070D51341HardwareSupportModuleConfig
    {
        public static GouldHS070D51341HardwareSupportModuleConfig Load(string filePath)
        {
            return
                Common.Serialization.Util.DeserializeFromXmlFile<GouldHS070D51341HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}