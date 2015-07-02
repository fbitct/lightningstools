﻿using System;
using System.Text;
using F4Utils.Campaign.F4Structs;
using System.IO;

namespace F4Utils.Campaign
{
    public class Team
    {
        #region Public Fields
        public VU_ID id;
        public ushort entityType;
        public byte who;
        public byte cteam;
        public short flags;
        public byte[] member;
        public short[] stance;
        public short firstColonel;
        public short firstCommander;
        public short firstWingman;
        public short lastWingman;
        public byte airExperience;
        public byte airDefenseExperience;
        public byte groundExperience;
        public byte navalExperience;
        public short initiative;
        public ushort supplyAvail;
        public ushort fuelAvail;
        public ushort replacementsAvail;
        public float playerRating;
        public uint lastPlayerMission;
        public TeamStatus currentStats;
        public TeamStatus startStats;
        public short reinforcement;
        public VU_ID[] bonusObjs;
        public uint[] bonusTime;
        public byte[] objtype_priority;
        public byte[] unittype_priority;
        public byte[] mission_priority;
        public uint attackTime;
        public byte offensiveLoss;
        public byte[] max_vehicle;
        public byte teamFlag;
        public byte teamColor;
        public byte equipment;
        public string name;
        public string teamMotto;


        public TeamGndActionType groundAction;
        public TeamAirActionType defensiveAirAction;
        public TeamAirActionType offensiveAirAction;

        #endregion

        protected Team()
            : base()
        {
        }
        public Team(Stream stream, int version)
            : this()
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                id = new VU_ID();
                id.num_ = reader.ReadUInt32();
                id.creator_ = reader.ReadUInt32();

                entityType = reader.ReadUInt16();
                who = reader.ReadByte();
                cteam = reader.ReadByte();
                flags = reader.ReadInt16();

                if (version > 2)
                {
                    member = new byte[8];
                    for (int j = 0; j < member.Length; j++)
                    {
                        member[j] = reader.ReadByte();
                    }
                    stance = new short[8];
                    for (int j = 0; j < stance.Length; j++)
                    {
                        stance[j] = reader.ReadInt16();
                    }
                }
                else
                {
                    member = new byte[8];
                    for (int j = 0; j < 7; j++)
                    {
                        member[j] = reader.ReadByte();
                    }
                    stance = new short[8];
                    for (int j = 0; j < 7; j++)
                    {
                        stance[j] = reader.ReadInt16();
                    }
                }
                firstColonel = reader.ReadInt16();
                firstCommander = reader.ReadInt16();
                firstWingman = reader.ReadInt16();
                lastWingman = reader.ReadInt16();

                playerRating = 0.0F;
                lastPlayerMission = 0;

                if (version > 11)
                {
                    airExperience = reader.ReadByte();
                    airDefenseExperience = reader.ReadByte();
                    groundExperience = reader.ReadByte();
                    navalExperience = reader.ReadByte();
                }
                else
                {
                    reader.ReadBytes(4);
                    airExperience = 80;
                    airDefenseExperience = 80;
                    groundExperience = 80;
                    navalExperience = 80;
                }
                initiative = reader.ReadInt16();
                supplyAvail = reader.ReadUInt16();
                fuelAvail = reader.ReadUInt16();

                if (version > 53)
                {
                    replacementsAvail = reader.ReadUInt16();
                    playerRating = reader.ReadSingle();
                    lastPlayerMission = reader.ReadUInt32();
                }
                else
                {
                    replacementsAvail = 0;
                    playerRating = 0.0f;
                    lastPlayerMission = 0;
                }
                if (version < 40)
                {
                    reader.ReadBytes(4);
                }

                currentStats = new TeamStatus();
                currentStats.airDefenseVehs = reader.ReadUInt16();
                currentStats.aircraft = reader.ReadUInt16();
                currentStats.groundVehs = reader.ReadUInt16();
                currentStats.ships = reader.ReadUInt16();
                currentStats.supply = reader.ReadUInt16();
                currentStats.fuel = reader.ReadUInt16();
                currentStats.airbases = reader.ReadUInt16();
                currentStats.supplyLevel = reader.ReadByte();
                currentStats.fuelLevel = reader.ReadByte();

                startStats = new TeamStatus();
                startStats.airDefenseVehs = reader.ReadUInt16();
                startStats.aircraft = reader.ReadUInt16();
                startStats.groundVehs = reader.ReadUInt16();
                startStats.ships = reader.ReadUInt16();
                startStats.supply = reader.ReadUInt16();
                startStats.fuel = reader.ReadUInt16();
                startStats.airbases = reader.ReadUInt16();
                startStats.supplyLevel = reader.ReadByte();
                startStats.fuelLevel = reader.ReadByte();

                reinforcement = reader.ReadInt16();

                bonusObjs = new VU_ID[20];
                for (int j = 0; j < bonusObjs.Length; j++)
                {
                    VU_ID thisId = new VU_ID();
                    thisId.num_ = reader.ReadUInt32();
                    thisId.creator_ = reader.ReadUInt32();
                    bonusObjs[j] = thisId;
                }
                bonusTime = new uint[20];
                for (int j = 0; j < bonusTime.Length; j++)
                {
                    bonusTime[j] = reader.ReadUInt32();
                }
                objtype_priority = new byte[36];
                for (int j = 0; j < objtype_priority.Length; j++)
                {
                    objtype_priority[j] = reader.ReadByte();
                }
                unittype_priority = new byte[20];
                for (int j = 0; j < unittype_priority.Length; j++)
                {
                    unittype_priority[j] = reader.ReadByte();
                }
                if (version < 30)
                {
                    mission_priority = new byte[40];
                    for (int j = 0; j < mission_priority.Length; j++)
                    {
                        mission_priority[j] = reader.ReadByte();
                    }
                }
                else
                {
                    mission_priority = new byte[41];
                    for (int j = 0; j < mission_priority.Length; j++)
                    {
                        mission_priority[j] = reader.ReadByte();
                    }
                }
                if (version < 34)
                {
                    attackTime = reader.ReadUInt32();
                    offensiveLoss = reader.ReadByte();
                }

                max_vehicle = new byte[4];
                for (int j = 0; j < max_vehicle.Length; j++)
                {
                    max_vehicle[j] = reader.ReadByte();
                }
                int nullLoc = 0;
                if (version > 4)
                {
                    teamFlag = reader.ReadByte();
                    if (version > 32)
                    {
                        teamColor = reader.ReadByte();
                    }
                    else
                    {
                        teamColor = 0;
                    }
                    equipment = reader.ReadByte();
                    var nameBytes = reader.ReadBytes(20);
                    name = Encoding.ASCII.GetString(nameBytes, 0, 20);
                    nullLoc = name.IndexOf('\0');
                    if (nullLoc > 0)
                    {
                        name = name.Substring(0, nullLoc);
                    }
                    else
                    {
                        name = String.Empty;
                    }
                }

                if (version > 32)
                {
                    var mottoBytes = reader.ReadBytes(200);
                    teamMotto = Encoding.ASCII.GetString(mottoBytes, 0, 200);
                    nullLoc = teamMotto.IndexOf('\0');
                    if (nullLoc > 0)
                    {
                        teamMotto = teamMotto.Substring(0, nullLoc);
                    }
                    else
                    {
                        teamMotto = string.Empty;
                    }
                }
                else
                {
                    teamMotto = string.Empty;
                }

                if (version > 33)
                {
                    if (version > 50)
                    {
                        groundAction = new TeamGndActionType();
                        groundAction.actionTime = reader.ReadUInt32();
                        groundAction.actionTimeout = reader.ReadUInt32();
                        groundAction.actionObjective = new VU_ID();
                        groundAction.actionObjective.num_ = reader.ReadUInt32();
                        groundAction.actionObjective.creator_ = reader.ReadUInt32();
                        groundAction.actionType = reader.ReadByte();
                        groundAction.actionTempo = reader.ReadByte();
                        groundAction.actionPoints = reader.ReadByte();
                    }
                    else if (version > 41)
                    {
                        reader.ReadBytes(27);
                        groundAction = new TeamGndActionType();
                    }
                    else
                    {
                        reader.ReadBytes(23);
                        groundAction = new TeamGndActionType();
                    }
                    defensiveAirAction = new TeamAirActionType();
                    defensiveAirAction.actionStartTime = reader.ReadUInt32();
                    defensiveAirAction.actionStopTime = reader.ReadUInt32();
                    defensiveAirAction.actionObjective = new VU_ID();
                    defensiveAirAction.actionObjective.num_ = reader.ReadUInt32();
                    defensiveAirAction.actionObjective.creator_ = reader.ReadUInt32();
                    defensiveAirAction.lastActionObjective = new VU_ID();
                    defensiveAirAction.lastActionObjective.num_ = reader.ReadUInt32();
                    defensiveAirAction.lastActionObjective.creator_ = reader.ReadUInt32();
                    defensiveAirAction.actionType = reader.ReadByte();
                    reader.ReadBytes(3); //align on int32 boundary

                    offensiveAirAction = new TeamAirActionType();
                    offensiveAirAction.actionStartTime = reader.ReadUInt32();
                    offensiveAirAction.actionStopTime = reader.ReadUInt32();
                    offensiveAirAction.actionObjective = new VU_ID();
                    offensiveAirAction.actionObjective.num_ = reader.ReadUInt32();
                    offensiveAirAction.actionObjective.creator_ = reader.ReadUInt32();
                    offensiveAirAction.lastActionObjective = new VU_ID();
                    offensiveAirAction.lastActionObjective.num_ = reader.ReadUInt32();
                    offensiveAirAction.lastActionObjective.creator_ = reader.ReadUInt32();
                    offensiveAirAction.actionType = reader.ReadByte();
                    reader.ReadBytes(3); //align on int32 boundary
                }
                else
                {
                    groundAction = new TeamGndActionType();
                    defensiveAirAction = new TeamAirActionType();
                    offensiveAirAction = new TeamAirActionType();
                }

                if (version < 43)
                {
                    groundAction.actionType = 2;
                    supplyAvail = fuelAvail = 1000;
                }

                if (version < 51)
                {
                    if (who == (byte)CountryListEnum.COUN_RUSSIA)
                    {
                        firstColonel = 500;
                        firstCommander = 505;
                        firstWingman = 538;
                        lastWingman = 583;
                    }
                    else if (who == (byte)CountryListEnum.COUN_CHINA)
                    {
                        firstColonel = 600;
                        firstCommander = 605;
                        firstWingman = 639;
                        lastWingman = 686;
                    }
                    else if (who == (byte)CountryListEnum.COUN_US)
                    {
                        firstColonel = 0;
                        firstCommander = 20;
                        firstWingman = 149;
                        lastWingman = 373;
                    }
                    else
                    {
                        firstColonel = 400;
                        firstCommander = 408;
                        firstWingman = 460;
                        lastWingman = 499;
                    }
                }
            }
        }
    }
}