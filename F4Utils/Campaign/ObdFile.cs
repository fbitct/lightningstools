using System;
using Lzss;

namespace F4Utils.Campaign
{
    public class ObdFile
    {
        #region Public Fields

        public ObjectiveDelta[] deltas;

        #endregion

        protected int _version;

        protected ObdFile()
        {
        }

        public ObdFile(byte[] compressed, int version)
            : this()
        {
            _version = version;
            short numObjectiveDeltas = 0;
            var expanded = Expand(compressed, out numObjectiveDeltas);
            if (expanded != null) Decode(expanded, version, numObjectiveDeltas);
        }

        protected void Decode(byte[] bytes, int version, short numObjectiveDeltas)
        {
            var curByte = 0;
            deltas = new ObjectiveDelta[numObjectiveDeltas];

            for (var i = 0; i < numObjectiveDeltas; i++)
            {
                var thisObjectiveDelta = new ObjectiveDelta();

                var id = new VU_ID();
                id.num_ = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                id.creator_ = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                thisObjectiveDelta.id = id;

                thisObjectiveDelta.last_repair = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                thisObjectiveDelta.owner = bytes[curByte];
                curByte++;
                thisObjectiveDelta.supply = bytes[curByte];
                curByte++;
                thisObjectiveDelta.fuel = bytes[curByte];
                curByte++;
                thisObjectiveDelta.losses = bytes[curByte];
                curByte++;
                var numFstatus = bytes[curByte];
                curByte++;
                thisObjectiveDelta.fStatus = new byte[numFstatus];
                if (version < 64)
                {
                    thisObjectiveDelta.fStatus[0] = bytes[curByte];
                    curByte++;
                }
                else
                {
                    for (var j = 0; j < numFstatus; j++)
                    {
                        thisObjectiveDelta.fStatus[j] = bytes[curByte];
                        curByte++;
                    }
                }
                deltas[i] = thisObjectiveDelta;
            }
        }

        protected static byte[] Expand(byte[] compressed, out short numObjectiveDeltas)
        {
            var curByte = 0;
            var cSize = BitConverter.ToInt32(compressed, curByte);
            curByte += 4;
            numObjectiveDeltas = BitConverter.ToInt16(compressed, curByte);
            curByte += 2;
            var uncompressedSize = BitConverter.ToInt32(compressed, curByte);
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