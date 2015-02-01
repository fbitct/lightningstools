using System.IO;

namespace F4Utils.Campaign
{
    public class FedFile
    {
        public FeatureEntry[] FeatureEntryDataTable { get; set; }
        public FedFile(string fileName)
        {
            FeatureEntryDataTable = LoadFeatureEntryData(fileName);
        }

        private FeatureEntry[] LoadFeatureEntryData(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var entries = reader.ReadInt16();
                var featureEntryDataTable = new FeatureEntry[entries];
                for (var i = 0; i < featureEntryDataTable.Length; i++)
                {
                    var entry = new FeatureEntry();
                    entry.Index = reader.ReadInt16();
                    entry.Flags = reader.ReadUInt16();
                    entry.eClass = reader.ReadBytes(8);
                    entry.Value = reader.ReadByte();
                    reader.ReadBytes(3); //padding
                    entry.Offset = new vector
                    {
                        x = reader.ReadSingle(),
                        y = reader.ReadSingle(),
                        z = reader.ReadSingle()
                    };

                    entry.Facing = reader.ReadInt16();
                    reader.ReadBytes(2); //padding
                    featureEntryDataTable[i] = entry;
                }
                return featureEntryDataTable;
            }


        }
    }
}
