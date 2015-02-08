using System;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class Objective : CampaignBase
    {
        #region Public Fields
        public uint lastRepair;
        public uint obj_flags;
        public byte supply;
        public byte fuel;
        public byte losses;
        public byte[] fstatus;
        public byte priority;
        public short nameId;
        public VU_ID parent;
        public byte first_owner;
        public byte links;
        public CampObjectiveLinkDataType[] link_data;
        public float[] detect_ratio; //radar_data, init size=8
        #endregion

        protected Objective()
            : base()
        {
        }
        public Objective(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            lastRepair = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            if (version > 1)
            {
                obj_flags = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            else
            {
                obj_flags = BitConverter.ToUInt16(bytes, offset);
                offset += 2;
            }

            supply = bytes[offset];
            offset++;

            fuel = bytes[offset];
            offset++;

            losses = bytes[offset];
            offset++;

            byte numStatuses = bytes[offset];
            offset++;

            fstatus = new byte[numStatuses];
            for (int i = 0; i < numStatuses; i++)
            {
                fstatus[i] = bytes[offset];
                offset++;
            }
            priority = bytes[offset];
            offset++;

            nameId = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            parent = new VU_ID();
            parent.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            parent.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            first_owner = bytes[offset];
            offset++;

            links = bytes[offset];
            offset++;

            if (links > 0)
            {
                link_data = new CampObjectiveLinkDataType[links];
            }
            else
            {
                link_data = null;
            }
            for (int i = 0; i < links; i++)
            {
                CampObjectiveLinkDataType thisLink = new CampObjectiveLinkDataType();
                thisLink.costs = new byte[(int)MoveType.MOVEMENT_TYPES];
                for (int j = 0; j < (int)MoveType.MOVEMENT_TYPES; j++)
                {
                    thisLink.costs[j] = bytes[offset];
                    offset++;
                }
                VU_ID newId = new VU_ID();
                newId.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                newId.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisLink.id = newId;
                link_data[i] = thisLink;
            }

            if (version >= 20)
            {
                byte hasRadarData = bytes[offset];
                offset++;

                if (hasRadarData > 0)
                {
                    detect_ratio = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        detect_ratio[i] = BitConverter.ToSingle(bytes, offset);
                        offset += 4;
                    }
                }
                else
                {
                    detect_ratio = null;
                }
            }
            else
            {
                detect_ratio = null;
            }

        }
    }
}