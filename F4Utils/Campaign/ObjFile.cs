using System;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class ObjFile
    {
        #region Public Fields

        public Objective[] objectives;

        #endregion

        protected int _version = 0;
        protected ObjFile()
            : base()
        {
        }
        public ObjFile(byte[] compressed, int version)
            : this()
        {
            _version = version;
            short numObjectives = 0;
            byte[] expanded = Expand(compressed, out numObjectives);
            if (expanded != null) Decode(expanded, version, numObjectives);
        }
        protected void Decode(byte[] bytes, int version, short numObjectives)
        {
            this.objectives = new Objective[numObjectives];

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen:true))
            {
                for (int i = 0; i < numObjectives; i++)
                {
                    short thisObjectiveType = reader.ReadInt16();
                    Objective thisObjective = new Objective(stream, version);
                    this.objectives[i] = thisObjective;
                }
            }
        }
        protected static byte[] Expand(byte[] compressed, out short numObjectives)
        {
            using (var stream = new MemoryStream(compressed))
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen:true))
            {
                numObjectives = reader.ReadInt16();
                int uncompressedSize = reader.ReadInt32();
                int newSize = reader.ReadInt32();
                if (uncompressedSize == 0) return null;

                var actualCompressed = reader.ReadBytes(compressed.Length - 10);
                byte[] uncompressed = null;
                uncompressed = Lzss.Codec.Decompress(actualCompressed, uncompressedSize);
                return uncompressed;
            }
        }
    }
}