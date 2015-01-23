using System;
namespace F4Utils.Campaign
{
    public class CampaignManager
    {
        #region Public Fields
        public VU_ID id;
        public VU_ID ownerId;
        public ushort entityType;
        public short managerFlags;
        public byte owner;
        #endregion

        protected CampaignManager()
            : base()
        {
        }
        public CampaignManager(byte[] bytes, ref int offset, int version)
        {
            id = new VU_ID();
            id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            entityType = BitConverter.ToUInt16(bytes, offset);
            offset += 2;

            managerFlags = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            owner = bytes[offset];
            offset++;
        }
    }
}