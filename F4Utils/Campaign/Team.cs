using System;
using System.Text;

namespace F4Utils.Campaign
{
    public class Team
    {
        #region Public Fields

        public byte airDefenseExperience;
        public byte airExperience;
        public uint attackTime;
        public VU_ID[] bonusObjs;
        public uint[] bonusTime;
        public byte cteam;
        public TeamStatus currentStats;
        public TeamAirActionType defensiveAirAction;
        public ushort entityType;
        public byte equipment;
        public short firstColonel;
        public short firstCommander;
        public short firstWingman;
        public short flags;
        public ushort fuelAvail;
        public TeamGndActionType groundAction;
        public byte groundExperience;
        public VU_ID id;
        public short initiative;
        public uint lastPlayerMission;
        public short lastWingman;
        public byte[] max_vehicle;
        public byte[] member;
        public byte[] mission_priority;
        public string name;
        public byte navalExperience;
        public byte[] objtype_priority;
        public TeamAirActionType offensiveAirAction;
        public byte offensiveLoss;
        public float playerRating;
        public short reinforcement;
        public ushort replacementsAvail;
        public short[] stance;
        public TeamStatus startStats;
        public ushort supplyAvail;
        public byte teamColor;
        public byte teamFlag;
        public string teamMotto;
        public byte[] unittype_priority;
        public byte who;

        #endregion

        protected Team()
        {
        }

        public Team(byte[] bytes, ref int offset, int version)
            : this()
        {
            id = new VU_ID();
            id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            entityType = BitConverter.ToUInt16(bytes, offset);
            offset += 2;

            who = bytes[offset];
            offset++;

            cteam = bytes[offset];
            offset++;

            flags = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            if (version > 2)
            {
                member = new byte[8];
                for (var j = 0; j < member.Length; j++)
                {
                    member[j] = bytes[offset];
                    offset++;
                }
                stance = new short[8];
                for (var j = 0; j < stance.Length; j++)
                {
                    stance[j] = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }
            }
            else
            {
                member = new byte[8];
                for (var j = 0; j < 7; j++)
                {
                    member[j] = bytes[offset];
                    offset++;
                }
                stance = new short[8];
                for (var j = 0; j < 7; j++)
                {
                    stance[j] = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                }
            }
            firstColonel = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            firstCommander = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            firstWingman = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            lastWingman = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            playerRating = 0.0F;
            lastPlayerMission = 0;

            if (version > 11)
            {
                airExperience = bytes[offset];
                offset++;

                airDefenseExperience = bytes[offset];
                offset++;

                groundExperience = bytes[offset];
                offset++;

                navalExperience = bytes[offset];
                offset++;
            }
            else
            {
                offset += 4;
                airExperience = 80;
                airDefenseExperience = 80;
                groundExperience = 80;
                navalExperience = 80;
            }
            initiative = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            supplyAvail = BitConverter.ToUInt16(bytes, offset);
            offset += 2;

            fuelAvail = BitConverter.ToUInt16(bytes, offset);
            offset += 2;

            if (version > 53)
            {
                replacementsAvail = BitConverter.ToUInt16(bytes, offset);
                offset += 2;

                playerRating = BitConverter.ToSingle(bytes, offset);
                offset += 4;

                lastPlayerMission = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            else
            {
                replacementsAvail = 0;
                playerRating = 0.0f;
                lastPlayerMission = 0;
            }
            if (version < 40)
            {
                offset += 4;
            }

            currentStats = new TeamStatus();
            currentStats.airDefenseVehs = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.aircraft = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.groundVehs = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.ships = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.supply = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.fuel = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.airbases = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            currentStats.supplyLevel = bytes[offset];
            offset++;
            currentStats.fuelLevel = bytes[offset];
            offset++;

            startStats = new TeamStatus();
            startStats.airDefenseVehs = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.aircraft = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.groundVehs = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.ships = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.supply = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.fuel = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.airbases = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            startStats.supplyLevel = bytes[offset];
            offset++;
            startStats.fuelLevel = bytes[offset];
            offset++;

            reinforcement = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            bonusObjs = new VU_ID[20];
            for (var j = 0; j < bonusObjs.Length; j++)
            {
                var thisId = new VU_ID();
                thisId.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisId.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                bonusObjs[j] = thisId;
            }
            bonusTime = new uint[20];
            for (var j = 0; j < bonusTime.Length; j++)
            {
                bonusTime[j] = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            objtype_priority = new byte[36];
            for (var j = 0; j < objtype_priority.Length; j++)
            {
                objtype_priority[j] = bytes[offset];
                offset++;
            }
            unittype_priority = new byte[20];
            for (var j = 0; j < unittype_priority.Length; j++)
            {
                unittype_priority[j] = bytes[offset];
                offset++;
            }
            if (version < 30)
            {
                mission_priority = new byte[40];
                for (var j = 0; j < mission_priority.Length; j++)
                {
                    mission_priority[j] = bytes[offset];
                    offset++;
                }
            }
            else
            {
                mission_priority = new byte[41];
                for (var j = 0; j < mission_priority.Length; j++)
                {
                    mission_priority[j] = bytes[offset];
                    offset++;
                }
            }
            if (version < 34)
            {
                attackTime = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveLoss = bytes[offset];
                offset++;
            }

            max_vehicle = new byte[4];
            for (var j = 0; j < max_vehicle.Length; j++)
            {
                max_vehicle[j] = bytes[offset];
                offset++;
            }
            var nullLoc = 0;
            if (version > 4)
            {
                teamFlag = bytes[offset];
                offset++;
                if (version > 32)
                {
                    teamColor = bytes[offset];
                    offset++;
                }
                else
                {
                    teamColor = 0;
                }
                equipment = bytes[offset];
                offset++;

                name = Encoding.ASCII.GetString(bytes, offset, 20);
                offset += 20;
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
                teamMotto = Encoding.ASCII.GetString(bytes, offset, 200);
                offset += 200;
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
                    groundAction.actionTime = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                    groundAction.actionTimeout = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                    groundAction.actionObjective = new VU_ID();
                    groundAction.actionObjective.num_ = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                    groundAction.actionObjective.creator_ = BitConverter.ToUInt32(bytes, offset);
                    offset += 4;
                    groundAction.actionType = bytes[offset];
                    offset++;
                    groundAction.actionTempo = bytes[offset];
                    offset++;
                    groundAction.actionPoints = bytes[offset];
                    offset++;
                }
                else if (version > 41)
                {
                    offset += 27;
                    groundAction = new TeamGndActionType();
                }
                else
                {
                    offset += 23;
                    groundAction = new TeamGndActionType();
                }
                defensiveAirAction = new TeamAirActionType();
                defensiveAirAction.actionStartTime = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.actionStopTime = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.actionObjective = new VU_ID();
                defensiveAirAction.actionObjective.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.actionObjective.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.lastActionObjective = new VU_ID();
                defensiveAirAction.lastActionObjective.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.lastActionObjective.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                defensiveAirAction.actionType = bytes[offset];
                offset++;
                offset += 3; //align on int32 boundary

                offensiveAirAction = new TeamAirActionType();
                offensiveAirAction.actionStartTime = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.actionStopTime = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.actionObjective = new VU_ID();
                offensiveAirAction.actionObjective.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.actionObjective.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.lastActionObjective = new VU_ID();
                offensiveAirAction.lastActionObjective.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.lastActionObjective.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                offensiveAirAction.actionType = bytes[offset];
                offset++;
                offset += 3; //align on int32 boundary
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
                if (who == (byte) CountryListEnum.COUN_RUSSIA)
                {
                    firstColonel = 500;
                    firstCommander = 505;
                    firstWingman = 538;
                    lastWingman = 583;
                }
                else if (who == (byte) CountryListEnum.COUN_CHINA)
                {
                    firstColonel = 600;
                    firstCommander = 605;
                    firstWingman = 639;
                    lastWingman = 686;
                }
                else if (who == (byte) CountryListEnum.COUN_US)
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