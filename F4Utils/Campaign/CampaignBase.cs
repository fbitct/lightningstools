using System;

namespace F4Utils.Campaign
{
    public class CampaignBase
    {
        #region Public Fields

        public short baseFlags;
        public short campId;
        public ushort entityType;
        public VU_ID id;
        public byte owner;
        public uint spotTime;
        public short spotted;
        public short x;
        public short y;
        public float z;

        #endregion

        protected CampaignBase()
        {
        }

        public CampaignBase(byte[] bytes, ref int offset, int version)
            : this()
        {
            id = new VU_ID();
            id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            entityType = BitConverter.ToUInt16(bytes, offset);
            offset += 2;

            x = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            y = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            if (version < 70)
            {
                z = 0;
            }
            else
            {
                z = BitConverter.ToSingle(bytes, offset);
                offset += 4;
            }
            spotTime = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            spotted = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            baseFlags = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            owner = bytes[offset];
            offset++;
            campId = BitConverter.ToInt16(bytes, offset);
            offset += 2;
        }
    }
}