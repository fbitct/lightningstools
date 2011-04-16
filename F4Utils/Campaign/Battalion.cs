using System;

namespace F4Utils.Campaign
{
    public class Battalion : GroundUnit
    {
        #region Public Fields

        public byte dummy;
        public byte fatigue;
        public byte final_heading;
        public byte heading;
        public uint last_combat;
        public uint last_move;
        public VU_ID last_obj;
        public short lfx;
        public short lfy;
        public byte morale;
        public VU_ID parent_id;
        public byte position;
        public short rfx;
        public short rfy;
        public byte supply;

        #endregion

        protected Battalion()
        {
        }

        public Battalion(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            last_move = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            last_combat = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            parent_id = new VU_ID();
            parent_id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            parent_id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            last_obj = new VU_ID();
            last_obj.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            last_obj.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            supply = bytes[offset];
            offset++;

            fatigue = bytes[offset];
            offset++;

            morale = bytes[offset];
            offset++;

            heading = bytes[offset];
            offset++;

            final_heading = bytes[offset];
            offset++;

            if (version < 15)
            {
                dummy = bytes[offset];
                offset++;
            }
            position = bytes[offset];
            offset++;
        }
    }
}