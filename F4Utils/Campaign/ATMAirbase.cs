using System;
using F4Utils.Campaign.F4Structs;

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
        public ATMAirbase(byte[] bytes, ref int offset, int version)
            : this()
        {
            id = new VU_ID();
            id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            schedule = new byte[32];
            for (int j = 0; j < schedule.Length; j++)
            {
                schedule[j] = bytes[offset];
                offset++;
            }
        }
    }
}