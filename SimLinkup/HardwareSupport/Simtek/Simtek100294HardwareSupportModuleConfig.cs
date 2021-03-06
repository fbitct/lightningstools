﻿using System;

namespace SimLinkup.HardwareSupport.Simtek
{
    [Serializable]
    public class Simtek100294HardwareSupportModuleConfig
    {
        public static Simtek100294HardwareSupportModuleConfig Load(string filePath)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<Simtek100294HardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }
}