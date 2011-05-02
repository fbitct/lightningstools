using System;

namespace F4Utils.Campaign
{
    public class Package : AirUnit
    {
        #region Public Fields

        public VU_ID awacs;
        public short bpx;
        public short bpy;
        public short caps;
        public short eax;
        public short eay;
        public VU_ID ecm;
        public Waypoint[] egress_waypoints;
        public VU_ID[] element;
        public byte elements;
        public byte flights;
        public short iax;
        public short iay;
        public Waypoint[] ingress_waypoints;
        public VU_ID interceptor;
        public VU_ID jstar;
        public MissionRequest mis_request;
        public byte num_egress_waypoints;
        public byte num_ingress_waypoints;
        public uint package_flags;
        public short requests;
        public short responses;
        public uint takeoff;
        public VU_ID tanker;
        public short threat_stats;
        public uint tp_time;
        public short tpx;
        public short tpy;
        public byte wait_cycles;
        public short wait_for;

        #endregion

        protected Package()
        {
        }

        public Package(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            elements = bytes[offset];
            offset++;
            element = new VU_ID[elements];
            if (elements < 5) element = new VU_ID[5];
            for (var i = 0; i < elements; i++)
            {
                var thisElement = new VU_ID();
                thisElement.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisElement.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                element[i] = thisElement;
            }
            interceptor = new VU_ID();
            interceptor.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            interceptor.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            if (version >= 7)
            {
                awacs = new VU_ID();
                awacs.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                awacs.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                jstar = new VU_ID();
                jstar.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                jstar.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                ecm = new VU_ID();
                ecm.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                ecm.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                tanker = new VU_ID();
                tanker.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                tanker.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            wait_cycles = bytes[offset];
            offset++;

            mis_request = new MissionRequest();

            if (Final && wait_cycles == 0)
            {
                requests = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                if (version < 35)
                {
                    threat_stats = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }

                responses = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.mission = (byte) BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.context = (byte) BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.requesterID = new VU_ID();
                mis_request.requesterID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.requesterID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.targetID = new VU_ID();
                mis_request.targetID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.targetID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                if (version >= 26)
                {
                    mis_request.tot = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                }
                else if (version >= 16)
                {
                    mis_request.tot = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                }
                if (version >= 35)
                {
                    mis_request.action_type = bytes[offset];
                    offset++;
                }
                else
                {
                    mis_request.action_type = 0;
                }

                if (version >= 41)
                {
                    mis_request.priority = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }
                else
                {
                    mis_request.priority = 1;
                }
                package_flags = 0;
            }
            else
            {
                flights = bytes[offset];
                offset++;
                wait_for = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                iax = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                iay = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                eax = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                eay = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                bpx = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                bpy = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                tpx = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                tpy = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                takeoff = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                tp_time = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                package_flags = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                caps = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                requests = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                if (version < 35)
                {
                    threat_stats = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }

                responses = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                num_ingress_waypoints = bytes[offset];
                offset++;

                ingress_waypoints = new Waypoint[num_ingress_waypoints];
                for (var j = 0; j < num_ingress_waypoints; j++)
                {
                    ingress_waypoints[j] = new Waypoint(bytes, ref offset, version);
                }

                num_egress_waypoints = bytes[offset];
                offset++;
                egress_waypoints = new Waypoint[num_egress_waypoints];
                for (var j = 0; j < num_egress_waypoints; j++)
                {
                    egress_waypoints[j] = new Waypoint(bytes, ref offset, version);
                }


                mis_request.requesterID = new VU_ID();
                mis_request.requesterID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.requesterID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.targetID = new VU_ID();
                mis_request.targetID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.targetID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.secondaryID = new VU_ID();
                mis_request.secondaryID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.secondaryID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.pakID = new VU_ID();
                mis_request.pakID.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                mis_request.pakID.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.who = bytes[offset];
                offset++;

                mis_request.vs = bytes[offset];
                offset++;

                offset += 2; //align on int32 boundary

                mis_request.tot = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.tx = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.ty = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.flags = BitConverter.ToUInt32(bytes, offset);
                offset += 4;

                mis_request.caps = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.target_num = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.speed = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.match_strength = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.priority = BitConverter.ToInt16(bytes, offset);
                offset += 2;

                mis_request.tot_type = bytes[offset];
                offset++;

                mis_request.action_type = bytes[offset];
                offset++;


                mis_request.mission = bytes[offset];
                offset++;

                mis_request.aircraft = bytes[offset];
                offset++;

                mis_request.context = bytes[offset];
                offset++;

                mis_request.roe_check = bytes[offset];
                offset++;

                if (!(version < 35))
                {
                    mis_request.delayed = bytes[offset];
                    offset++;

                    mis_request.start_block = bytes[offset];
                    offset++;

                    mis_request.final_block = bytes[offset];
                    offset++;

                    mis_request.slots = new byte[4];
                    for (var k = 0; k < 4; k++)
                    {
                        mis_request.slots[k] = bytes[offset];
                        offset++;
                    }

                    mis_request.min_to = bytes[offset];
                    offset++;

                    mis_request.max_to = bytes[offset];
                    offset++;

                    offset += 3; // align on int32 boundary
                }
            }
        }
    }
}