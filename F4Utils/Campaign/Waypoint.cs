using System;

namespace F4Utils.Campaign
{
    public class Waypoint
    {
        #region Public Fields

        public byte Action;
        public uint Arrive;
        public uint Depart;
        public uint Flags;
        public byte Formation;
        public short GridX;
        public short GridY;
        public short GridZ;
        public byte RouteAction;
        public byte TargetBuilding;
        public VU_ID TargetID;
        public byte haves;

        #endregion

        public const byte WP_HAVE_DEPTIME = 0x01;
        public const byte WP_HAVE_TARGET = 0x02;

        private const int FLAGS_WIDENED_AT_VERSION = 73;

        protected Waypoint()
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
            Formation = bytes[offset];
            offset++;

            if (version < FLAGS_WIDENED_AT_VERSION)
            {
                Flags = BitConverter.ToUInt16(bytes, offset);
                offset += 2;
            }
            else
            {
                Flags = BitConverter.ToUInt32(bytes, offset);
                //TODO: SOME NEW FIELD, 2 BYTES LONG, COMES HERE, OR ELSE FLAGS IS EXPANDED IN AT LATEST V73 (PROBABLY EARLIER?) TO BE 4 BYTES LONG INSTEAD OF 2 BYTES LONG
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
            if ((haves & WP_HAVE_DEPTIME) != 0)
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