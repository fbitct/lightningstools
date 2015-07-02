using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class AirTaskingManager : CampaignManager
    {
        #region Public Fields
        public short flags;
        public short averageCAStrength;
        public short averageCAMissions;
        public byte sampleCycles;
        public byte numAirbases;
        public ATMAirbase[] airbases;
        public byte cycle;
        public short numMissionRequests;
        public MissionRequest[] missionRequests;
        #endregion

        protected AirTaskingManager()
            : base()
        {
        }
        public AirTaskingManager(Stream stream, int version)
            : base(stream, version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                flags = reader.ReadInt16();

                if (version >= 28)
                {
                    if (version >= 63)
                    {
                        averageCAStrength = reader.ReadInt16();
                    }
                    averageCAMissions = reader.ReadInt16();
                    sampleCycles = reader.ReadByte();
                }
                if (version < 63)
                {
                    averageCAMissions = 500;
                    averageCAStrength = 500;
                    sampleCycles = 10;

                }
                numAirbases = reader.ReadByte();

                airbases = new ATMAirbase[numAirbases];
                for (int j = 0; j < numAirbases; j++)
                {
                    airbases[j] = new ATMAirbase(stream, version);
                }

                cycle = reader.ReadByte();

                numMissionRequests = reader.ReadInt16();

                missionRequests = new MissionRequest[numMissionRequests];
                if (version < 35)
                {
                    for (int j = 0; j < numMissionRequests; j++)
                    {
                        reader.ReadBytes(64);
                    }
                }
                else
                {
                    for (int j = 0; j < numMissionRequests; j++)
                    {
                        MissionRequest mis_request = new MissionRequest();

                        mis_request.requesterID = new VU_ID();
                        mis_request.requesterID.num_ = reader.ReadUInt32();
                        mis_request.requesterID.creator_ = reader.ReadUInt32();

                        mis_request.targetID = new VU_ID();
                        mis_request.targetID.num_ = reader.ReadUInt32();
                        mis_request.targetID.creator_ = reader.ReadUInt32();

                        mis_request.secondaryID = new VU_ID();
                        mis_request.secondaryID.num_ = reader.ReadUInt32();
                        mis_request.secondaryID.creator_ = reader.ReadUInt32();

                        mis_request.pakID = new VU_ID();
                        mis_request.pakID.num_ = reader.ReadUInt32();
                        mis_request.pakID.creator_ = reader.ReadUInt32();

                        mis_request.who = reader.ReadByte();
                        mis_request.vs = reader.ReadByte();
                        reader.ReadBytes(2);//align on int32 boundary

                        mis_request.tot = reader.ReadUInt32();
                        mis_request.tx = reader.ReadInt16();
                        mis_request.ty = reader.ReadInt16();
                        mis_request.flags = reader.ReadUInt32();
                        mis_request.caps = reader.ReadInt16();
                        mis_request.target_num = reader.ReadInt16();
                        mis_request.speed = reader.ReadInt16();
                        mis_request.match_strength = reader.ReadInt16();
                        mis_request.priority = reader.ReadInt16();
                        mis_request.tot_type = reader.ReadByte();
                        mis_request.action_type = reader.ReadByte();
                        mis_request.mission = reader.ReadByte();
                        mis_request.aircraft = reader.ReadByte();
                        mis_request.context = reader.ReadByte();
                        mis_request.roe_check = reader.ReadByte();
                        mis_request.delayed = reader.ReadByte();
                        mis_request.start_block = reader.ReadByte();
                        mis_request.final_block = reader.ReadByte();

                        mis_request.slots = new byte[4];
                        for (int k = 0; k < 4; k++)
                        {
                            mis_request.slots[k] = reader.ReadByte();
                        }

                        mis_request.min_to = reader.ReadSByte();
                        mis_request.max_to = reader.ReadSByte();
                        reader.ReadBytes(3);// align on int32 boundary
                        missionRequests[j] = mis_request;

                    }
                }
            }
        }
    }
}