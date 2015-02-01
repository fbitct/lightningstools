﻿using System.IO;

namespace F4Utils.Campaign
{
    public class RwdFile
    {
        public RwrDataType[] RWRDataTable { get; private set; }
        public RwdFile(string fileName)
        {
            RWRDataTable = LoadRWRData(fileName);
        }

        private RwrDataType[] LoadRWRData(string fileName)
        {
            //reads RWD file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var rwrDataTable = new RwrDataType[entries];
                for (var i = 0; i < rwrDataTable.Length; i++)
                {
                    var entry = new RwrDataType();
                    entry.nominalRange = reader.ReadSingle();
                    entry.top = reader.ReadSingle();
                    entry.bottom = reader.ReadSingle();
                    entry.left = reader.ReadSingle();
                    entry.right = reader.ReadSingle();
                    entry.flag = reader.ReadInt16();
                    rwrDataTable[i] = entry;
                }
                return rwrDataTable;
            }

        }
    }
}
