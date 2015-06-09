using System.IO;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class FcdFile
    {
        public FeatureClassDataType[] FeatureDataTable { get; private set; }
        public FcdFile(string fileName)
        {
            FeatureDataTable = LoadFeatureData(fileName);
        }

        private FeatureClassDataType[] LoadFeatureData(string fileName)
        {
            //reads FCD file
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var featureDataTable = new FeatureClassDataType[entries];
                for (var i = 0; i < featureDataTable.Length; i++)
                {
                    var entry = new FeatureClassDataType();
                    entry.Index = reader.ReadInt16();
                    entry.RepairTime = reader.ReadInt16();
                    entry.Priority = reader.ReadByte();
                    reader.ReadBytes(1);//padding
                    entry.Flags = reader.ReadUInt16();
                    entry.Name = reader.ReadBytes(20);
                    entry.HitPoints = reader.ReadInt16();
                    entry.Height = reader.ReadInt16();
                    entry.Angle = reader.ReadSingle();
                    entry.RadarType = reader.ReadInt16();
                    entry.Detection = reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.DamageMod = reader.ReadBytes((int) DamageDataType.OtherDam + 1);
                    reader.ReadBytes(3);//padding
                    featureDataTable[i] = entry;
                }
                return featureDataTable;
            }

        }
    }
}
