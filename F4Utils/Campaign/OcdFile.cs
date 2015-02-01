
using System.IO;

namespace F4Utils.Campaign
{
    public class OcdFile
    {
        public ObjClassDataType[] ObjDataTable { get; private set; }
        public OcdFile(string fileName)
        {
            ObjDataTable = LoadObjectiveData(fileName);
        }

        private ObjClassDataType[] LoadObjectiveData(string fileName)
        {
            //reads OCD file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var objDataTable = new ObjClassDataType[entries];
                for (var i = 0; i < objDataTable.Length; i++)
                {
                    var entry = new ObjClassDataType();
                    entry.Index = reader.ReadInt16();
                    entry.Name = reader.ReadBytes(20);
                    entry.DataRate = reader.ReadInt16();
                    entry.DeagDistance = reader.ReadInt16();
                    entry.PtDataIndex = reader.ReadInt16();
                    entry.Detection = reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.DamageMod = reader.ReadBytes((int)DamageDataType.OtherDam + 1);
                    reader.ReadByte(); //padding
                    entry.IconIndex = reader.ReadInt16();
                    entry.Features = reader.ReadByte();
                    entry.RadarFeature = reader.ReadByte();
                    entry.FirstFeature = reader.ReadByte();
                    reader.ReadByte(); //padding
                    objDataTable[i] = entry;
                }
                return objDataTable;
            }
        }
    }
}
