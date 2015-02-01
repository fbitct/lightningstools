﻿using System.IO;

namespace F4Utils.Campaign
{
    public class DdpFile
    {
        public DirtyDataClassType[] DirtyDataTable { get; private set; }
        public DdpFile(string fileName)
        {
            DirtyDataTable = LoadDirtyData(fileName);
        }

        private DirtyDataClassType[] LoadDirtyData(string fileName)
        {
            //reads DDP file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var dirtyDataTable = new DirtyDataClassType[entries];
                for (var i = 0; i < dirtyDataTable.Length; i++)
                {
                    var entry = new DirtyDataClassType();
                    entry.priority = (Dirtyness) reader.ReadInt32();
                    dirtyDataTable[i] = entry;
                }
                return dirtyDataTable;
            }
        }
    }
}
