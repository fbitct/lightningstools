using System.IO;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class PhdFile
    {
        public PtHeaderDataType[] PtHeaderDataTable { get; private set; }
        public PhdFile(string fileName)
        {
            PtHeaderDataTable = LoadPtHeaderData(fileName);
        }

        private PtHeaderDataType[] LoadPtHeaderData(string fileName)
        {
            //reads PHD file
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var ptHeaderDataTable = new PtHeaderDataType[entries];
                for (var i = 0; i < ptHeaderDataTable.Length; i++)
                {
                    var entry = new PtHeaderDataType();
                    entry.objID = reader.ReadInt16();
                    entry.type = reader.ReadByte();
                    entry.count = reader.ReadByte();
                    entry.features = reader.ReadBytes((int)CampLibConstants.MAX_FEAT_DEPEND);
                    reader.ReadByte(); //padding
                    entry.data = reader.ReadInt16();
                    entry.sinHeading = reader.ReadSingle();
                    entry.cosHeading = reader.ReadSingle();
                    entry.first = reader.ReadInt16();
                    entry.texIdx = reader.ReadInt16();
                    entry.runwayNum = reader.ReadSByte();
                    entry.ltrt = reader.ReadSByte();
                    entry.nextHeader = reader.ReadInt16();
                    ptHeaderDataTable[i] = entry;
                }
                ptHeaderDataTable[0].cosHeading = 1.0F;
                return ptHeaderDataTable;
            }

        }
    }
}
