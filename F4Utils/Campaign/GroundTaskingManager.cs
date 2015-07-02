using System;
using System.IO;
using System.Text;
namespace F4Utils.Campaign
{
    public class GroundTaskingManager : CampaignManager
    {
        #region Public Fields
        public short flags;
        #endregion

        protected GroundTaskingManager()
            : base()
        {
        }
        public GroundTaskingManager(Stream stream, int version)
            : base(stream, version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                flags = reader.ReadInt16();
            }
        }

    }
}