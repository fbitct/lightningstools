using F4Utils.Campaign.F4Structs;
using System;
using System.IO;

namespace F4Utils.Campaign.Save
{
    public class HsiFileRecord
    {
        public uint Time { get; internal set; }
        public UnitHistoryType[] UnitHistory { get; internal set; }
    }
    class HisFile
    {
        public HsiFileRecord[] HistoryRecords { get; internal set; }
        public HisFile(string fileName)
        {
            LoadHisFile(fileName);
        }

         private void LoadHisFile(string fileName)
         {
             //reads HIS file
             using (var stream = new FileStream(fileName, FileMode.Open))
             using (var reader = new BinaryReader(stream))
             {
                 while (stream.Position != stream.Length)
                 {
                     var rec = new HsiFileRecord();
                     rec.Time = reader.ReadUInt32();
                     var count = reader.ReadInt16();
                     if (count > 0)
                     {
                         rec.UnitHistory = new UnitHistoryType[count];
                         for (var i = 0; i < count; i++)
                         {
                            rec.UnitHistory[i] = new UnitHistoryType(stream);
                         }
                     }
                 }
             }
         }
    }
}
