using System.IO;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class PdxFile
    {
        public PtDataType[] PtDataTable { get; private set; }
        public PdxFile(string fileName)
        {
            PtDataTable = LoadPtData(fileName);
        }

        private PtDataType[] LoadPtData(string fileName)
        {
            //reads PDX file
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var ptDataTable = new PtDataType[entries];
                for (var i = 0; i < ptDataTable.Length; i++)
                {
                    var entry = new PtDataType();
                    entry.xOffset = reader.ReadSingle();
                    entry.yOffset = reader.ReadSingle();
                    entry.zOffset = reader.ReadSingle();
                    entry.height = reader.ReadSingle();
                    entry.width = reader.ReadSingle();
                    entry.length = reader.ReadSingle();
                    entry.type = reader.ReadByte();
                    entry.flags = reader.ReadByte();
                    entry.rootIdx = reader.ReadByte();
                    entry.branchIdx = reader.ReadByte();
                    ptDataTable[i] = entry;
                }
                return ptDataTable;
            }
        }
    }
}
