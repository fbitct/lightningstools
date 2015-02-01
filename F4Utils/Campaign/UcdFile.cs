using System.IO;

namespace F4Utils.Campaign
{
    public class UcdFile
    {
        public UnitClassDataType[] UnitData { get; set; }

        public UcdFile(string fileName)
        {
            UnitData = LoadUnitData(fileName);
        }
        private UnitClassDataType[] LoadUnitData(string fileName)
        {
            //reads UCD file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var unitDataTable = new UnitClassDataType[entries];
                for (var i = 0; i < unitDataTable.Length; i++)
                {
                    var entry = new UnitClassDataType();
                    entry.Index = reader.ReadInt16();
                    reader.ReadInt16();
                    for (var j = 0; j < entry.NumElements.Length; j++)
                    {
                        entry.NumElements[j] = reader.ReadInt32();
                    }
                    for (var j = 0; j < entry.VehicleType.Length; j++)
                    {
                        entry.VehicleType[j] = reader.ReadInt16();
                    }
                    for (var j = 0; j < entry.VehicleClass.Length; j++)
                    {
                        entry.VehicleClass[j] = reader.ReadBytes(8);
                    }
                    entry.Flags = reader.ReadUInt16();
                    entry.Name = reader.ReadBytes(20);
                    reader.ReadInt16(); //padding
                    entry.MovementType = (MoveType)reader.ReadInt32();
                    entry.MovementSpeed = reader.ReadInt16();
                    entry.MaxRange = reader.ReadInt16();
                    entry.Fuel = reader.ReadInt32();
                    entry.Rate = reader.ReadInt16();
                    entry.PtDataIndex = reader.ReadInt16();
                    entry.Scores = reader.ReadBytes((int)CampLibConstants.MAXIMUM_ROLES);
                    entry.Role = reader.ReadByte();
                    entry.HitChance = reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.Strength =  reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.Range = reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.Detection = reader.ReadBytes((int) MoveType.MOVEMENT_TYPES);
                    entry.DamageMod = reader.ReadBytes((int) DamageDataType.OtherDam+1);
                    entry.RadarVehicle = reader.ReadByte();
                    reader.ReadByte();//padding
                    entry.SpecialIndex = reader.ReadInt16();
                    entry.IconIndex = reader.ReadInt16();
                    reader.ReadInt16();//padding
                    unitDataTable[i] = entry;
                }
                return unitDataTable;
            }
        }

    }
}
