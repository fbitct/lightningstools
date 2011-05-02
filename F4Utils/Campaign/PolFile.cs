using System;

namespace F4Utils.Campaign
{
    public class PolFile
    {
        #region Public Fields

        public short numPrimaryObjectives;
        public PrimaryObjective[] primaryObjectives;
        public byte teammask;

        #endregion

        protected PolFile()
        {
        }

        public PolFile(byte[] bytes, int version)
            : this()
        {
            Decode(bytes, version);
        }

        protected void Decode(byte[] bytes, int version)
        {
            var offset = 0;
            teammask = bytes[offset];
            offset++;
            numPrimaryObjectives = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            primaryObjectives = new PrimaryObjective[numPrimaryObjectives];
            for (var i = 0; i < numPrimaryObjectives; i++)
            {
                var thisObjective = new PrimaryObjective();
                thisObjective.id = new VU_ID();
                thisObjective.id.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisObjective.id.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisObjective.priority = new short[8];
                for (var j = 0; j < 8; j++)
                {
                    if ((teammask & (1 << j)) > 0)
                    {
                        thisObjective.priority[j] = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                    }
                }
            }
        }
    }
}