using System;
using Lzss;

namespace F4Utils.Campaign
{
    public class ObjFile
    {
        #region Public Fields

        public Objective[] objectives;

        #endregion

        protected int _version;

        protected ObjFile()
        {
        }

        public ObjFile(byte[] compressed, int version)
            : this()
        {
            _version = version;
            short numObjectives = 0;
            var expanded = Expand(compressed, out numObjectives);
            if (expanded != null) Decode(expanded, version, numObjectives);
        }

        protected void Decode(byte[] bytes, int version, short numObjectives)
        {
            objectives = new Objective[numObjectives];

            var curByte = 0;
            curByte = 0;
            for (var i = 0; i < numObjectives; i++)
            {
                var thisObjectiveType = BitConverter.ToInt16(bytes, curByte);
                curByte += 2;
                var thisObjective = new Objective(bytes, ref curByte, version);
                objectives[i] = thisObjective;
            }
        }

        protected static byte[] Expand(byte[] compressed, out short numObjectives)
        {
            var curByte = 0;
            numObjectives = BitConverter.ToInt16(compressed, curByte);
            curByte += 2;
            var uncompressedSize = BitConverter.ToInt32(compressed, curByte);
            curByte += 4;
            var newSize = BitConverter.ToInt32(compressed, curByte);
            curByte += 4;
            if (uncompressedSize == 0) return null;

            var actualCompressed = new byte[compressed.Length - 10];
            Array.Copy(compressed, 10, actualCompressed, 0, actualCompressed.Length);
            byte[] uncompressed = null;
            uncompressed = Codec.Decompress(actualCompressed, uncompressedSize);
            return uncompressed;
        }
    }
}