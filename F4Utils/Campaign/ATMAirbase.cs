using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class ATMAirbase
    {
        #region Public Fields
        public VU_ID id;
        public byte[] schedule;
        #endregion
        protected ATMAirbase()
            : base()
        {
        }
        public ATMAirbase(Stream stream, int version)
            : this()
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                id = new VU_ID();
                id.num_ = reader.ReadUInt32();
                id.creator_ = reader.ReadUInt32();

                schedule = new byte[32];
                for (int j = 0; j < schedule.Length; j++)
                {
                    schedule[j] = reader.ReadByte();
                }
            }
        }
    }
}