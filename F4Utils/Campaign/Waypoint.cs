using System;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class Waypoint
    {
        #region Public Fields
        public byte haves;
        public short GridX;
        public short GridY;
        public short GridZ;
        public uint Arrive;
        public byte Action;
        public byte RouteAction;
        public byte Formation;
        public short FormationSpacing;

        public uint Flags;
        public VU_ID TargetID;
        public byte TargetBuilding;
        public uint Depart;

        #endregion
        public const byte WP_HAVE_DEPTIME = 0x01;
        public const byte WP_HAVE_TARGET = 0x02;

        protected Waypoint()
            : base()
        {
        }
        public Waypoint(byte[] bytes, ref int offset, int version)
            : this()
        {
            haves = bytes[offset];
            offset++;
            GridX = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            GridY = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            GridZ = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            Arrive = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            Action = bytes[offset];
            offset++;
            RouteAction = bytes[offset];
            offset++;
            var tmp = bytes[offset];
            offset++;
            Formation = (byte)(tmp & 0x0f);
            FormationSpacing = (short)( ((tmp >> 4) & 0x0F) - 8);

            if (version < 72)
            {
                Flags = BitConverter.ToUInt16(bytes, offset);
                offset += 2;
            }
            else
            {
                Flags = BitConverter.ToUInt32(bytes, offset);
                offset += 4; 

            }
            if ((haves & WP_HAVE_TARGET) != 0)
            {
                TargetID = new VU_ID();
                TargetID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                TargetID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                TargetBuilding = bytes[offset];
                offset++;
            }
            else
            {
                TargetID = new VU_ID();
                TargetBuilding = 255;
            }
            if ((haves & WP_HAVE_DEPTIME) !=0)
            {
                Depart = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            else
            {
                Depart = Arrive;
            }
        }
    }
}