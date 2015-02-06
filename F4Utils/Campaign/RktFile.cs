﻿using System.IO;

namespace F4Utils.Campaign
{
    public class RktFile
    {
        public RocketClassDataType[] RocketDataTable { get; private set; }
        public RktFile(string fileName)
        {
            RocketDataTable = LoadRocketData(fileName);
        }

        private RocketClassDataType[] LoadRocketData(string fileName)
        {
            //reads RKT file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var rocketDataTable = new RocketClassDataType[entries];
                for (var i = 0; i < rocketDataTable.Length; i++)
                {
                    var entry = new RocketClassDataType();
                    entry.weaponId = reader.ReadInt16();
                    entry.nweaponId = reader.ReadInt16();
                    entry.weaponCount = reader.ReadInt16();
                    rocketDataTable[i] = entry;
                }
                return rocketDataTable;
            }
        }
    }
}