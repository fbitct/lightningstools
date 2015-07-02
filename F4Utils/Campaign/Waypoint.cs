using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

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
        public Waypoint(Stream stream, int version)
            : this()
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                haves = reader.ReadByte();
                GridX = reader.ReadInt16();
                GridY = reader.ReadInt16();
                GridZ = reader.ReadInt16();
                Arrive = reader.ReadUInt32();
                Action = reader.ReadByte();
                RouteAction = reader.ReadByte();
                var tmp = reader.ReadByte();
                Formation = (byte)(tmp & 0x0f);
                FormationSpacing = (short)(((tmp >> 4) & 0x0F) - 8);

                if (version < 72)
                {
                    Flags = reader.ReadUInt16();
                }
                else
                {
                    Flags = reader.ReadUInt32();
                }
                if ((haves & WP_HAVE_TARGET) != 0)
                {
                    TargetID = new VU_ID();
                    TargetID.num_ = reader.ReadUInt32();
                    TargetID.creator_ = reader.ReadUInt32();
                    TargetBuilding = reader.ReadByte();
                }
                else
                {
                    TargetID = new VU_ID();
                    TargetBuilding = 255;
                }
                if ((haves & WP_HAVE_DEPTIME) != 0)
                {
                    Depart = reader.ReadUInt32();
                }
                else
                {
                    Depart = Arrive;
                }
            }
        }
    }
}