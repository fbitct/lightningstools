﻿using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100207HardwareSupportModuleConfig
    {
        public static Simtek100207HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100207HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}