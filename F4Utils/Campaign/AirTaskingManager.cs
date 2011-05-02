using System;

namespace F4Utils.Campaign
{
    public class AirTaskingManager : CampaignManager
    {
        #region Public Fields

        public ATMAirbase[] airbases;
        public short averageCAMissions;
        public short averageCAStrength;
        public byte cycle;
        public short flags;
        public MissionRequest[] missionRequests;
        public byte numAirbases;
        public short numMissionRequests;
        public byte sampleCycles;

        #endregion

        protected AirTaskingManager()
        {
        }

        public AirTaskingManager(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            flags = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            if (version >= 28)
            {
                if (version >= 63)
                {
                    averageCAStrength = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }
                averageCAMissions = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                sampleCycles = bytes[offset];
                offset++;
            }
            if (version < 63)
            {
                averageCAMissions = 500;
                averageCAStrength = 500;
                sampleCycles = 10;
            }
            numAirbases = bytes[offset];
            offset++;

            airbases = new ATMAirbase[numAirbases];
            for (var j = 0; j < numAirbases; j++)
            {
                airbases[j] = new ATMAirbase(bytes, ref offset, version);
            }

            cycle = bytes[offset];
            offset++;

            numMissionRequests = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            missionRequests = new MissionRequest[numMissionRequests];
            if (version < 35)
            {
                for (var j = 0; j < numMissionRequests; j++)
                {
                    offset += 64;
                }
            }
            else
            {
                for (var j = 0; j < numMissionRequests; j++)
                {
                    var mis_request = new MissionRequest();

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
                    missionRequests[j] = mis_request;
                }
            }
        }
    }
}