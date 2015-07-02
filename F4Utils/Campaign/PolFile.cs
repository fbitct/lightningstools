using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class PolFile
    {
        #region Public Fields
        public short numPrimaryObjectives;
        public byte teammask;
        public PrimaryObjective[] primaryObjectives;
        #endregion

        protected PolFile()
            : base()
        {
        }
        public PolFile(Stream stream, int version)
            : this()
        {
            Decode(stream, version);
        }
        protected void Decode(Stream stream, int version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                teammask = reader.ReadByte(); ;
                numPrimaryObjectives = reader.ReadInt16();
                primaryObjectives = new PrimaryObjective[numPrimaryObjectives];
                for (int i = 0; i < numPrimaryObjectives; i++)
                {
                    PrimaryObjective thisObjective = new PrimaryObjective();
                    thisObjective.id = new VU_ID();
                    thisObjective.id.num_ = reader.ReadUInt32();
                    thisObjective.id.creator_ = reader.ReadUInt32();
                    thisObjective.priority = new short[8];
                    for (int j = 0; j < 8; j++)
                    {
                        if ((teammask & (1 << j)) > 0)
                        {
                            thisObjective.priority[j] = reader.ReadInt16();
                            thisObjective.flags = reader.ReadByte();
                        }
                    }
                }
            }
        }
    }
}