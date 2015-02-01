﻿using System.IO;

namespace F4Utils.Campaign
{
    public class AcdFile
    {
        public SimACDefType[] SimACDefTable { get; private set; }
        public AcdFile(string fileName)
        {
            SimACDefTable = LoadSimACDefData(fileName);
        }

        private SimACDefType[] LoadSimACDefData(string fileName)
        {
            //reads ACD file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var simACDefTable = new SimACDefType[entries];
                for (var i = 0; i < simACDefTable.Length; i++)
                {
                    var entry = new SimACDefType();
                    entry.combatClass = reader.ReadInt32();
                    entry.airframeIdx = reader.ReadInt32();
                    entry.signatureIdx = reader.ReadInt32();
                    for (var j = 0; j < entry.sensorType.Length; j++)
                    {
                        entry.sensorType[j] = reader.ReadInt32();
                    }
                    for (var j = 0; j < entry.sensorIdx.Length; j++)
                    {
                        entry.sensorIdx[j] = reader.ReadInt32();
                    }
                    simACDefTable[i] = entry;
                }
                return simACDefTable;
            }
        }
    }
}
