using System;
namespace F4Utils.Campaign
{
    public class Unit : CampaignBase
    {
        #region Public Fields
        public uint last_check;
        public int roster;
        public int unit_flags;
        public short dest_x;
        public short dest_y;
        public VU_ID target_id;
        public VU_ID cargo_id;
        public byte moved;
        public byte losses;
        public byte tactic;
        public ushort current_wp;
        public short name_id;
        public short reinforcement;
        public ushort numWaypoints;
        public Waypoint[] waypoints;
        #endregion
        public const int U_FINAL = 0x100000;
        protected Unit()
            : base()
        {
        }
        public Unit(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            last_check = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            roster = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            unit_flags = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            dest_x = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            dest_y = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            target_id = new VU_ID();
            target_id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            target_id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            if (version > 1)
            {
                cargo_id = new VU_ID();
                cargo_id.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                cargo_id.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            else
            {
                cargo_id = new VU_ID();
            }
            moved = bytes[offset];
            offset++;
            losses = bytes[offset];
            offset++;
            tactic = bytes[offset];
            offset++;

            if (version >= 71)
            {
                current_wp = BitConverter.ToUInt16(bytes, offset);
                offset += 2;
            }
            else
            {
                current_wp = bytes[offset];
                offset++;
            }
            name_id = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            reinforcement = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            DecodeWaypoints(bytes, ref offset, version);

        }
        protected void DecodeWaypoints(byte[] bytes, ref int offset, int version)
        {
            if (version >= 71)
            {
                numWaypoints = BitConverter.ToUInt16(bytes, offset);
                offset += 2;
            }
            else
            {
                numWaypoints = (ushort)bytes[offset];
                offset++;
            }
            if (numWaypoints > 500) return;
            waypoints = new Waypoint[numWaypoints];
            for (int i = 0; i < numWaypoints; i++)
            {
                waypoints[i] = new Waypoint(bytes, ref offset, version);
            }
        }
        protected bool Final
        {
            get
            {
                return ((unit_flags & U_FINAL) > 0);
            }
        }
    }
}