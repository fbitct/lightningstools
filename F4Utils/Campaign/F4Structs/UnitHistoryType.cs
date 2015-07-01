﻿
using System.IO;
using System.Text;
namespace F4Utils.Campaign.F4Structs
{
    public class UnitHistoryType 
    {
	    public byte team=0;
	    public short x=0;
        public short y=0;
        public UnitHistoryType() { }
        public UnitHistoryType(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                team = reader.ReadByte();
                x = reader.ReadInt16();
                y = reader.ReadInt16();
            }
        }
	};

}
