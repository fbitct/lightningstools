using System;

namespace F4Utils.Campaign
{
    public class NavalTaskingManager : CampaignManager
    {
        #region Public Fields

        public short flags;

        #endregion

        protected NavalTaskingManager()
        {
        }

        public NavalTaskingManager(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            flags = BitConverter.ToInt16(bytes, offset);
            offset += 2;
        }
    }
}