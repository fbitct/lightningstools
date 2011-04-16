using System;

namespace F4Utils.Campaign
{
    public class GroundTaskingManager : CampaignManager
    {
        #region Public Fields

        public short flags;

        #endregion

        protected GroundTaskingManager()
        {
        }

        public GroundTaskingManager(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            flags = BitConverter.ToInt16(bytes, offset);
            offset += 2;
        }
    }
}