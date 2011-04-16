using System;
using System.Text;
using Lzss;

namespace F4Utils.Campaign
{
    public class CmpFile
    {
        #region Public Fields

        public byte ActiveTeams;
        public short AirDefenseRatio;
        public short AirRatio;
        public short Brief;
        public byte BullseyeName;
        public short BullseyeX;
        public short BullseyeY;
        public byte[] CampMap;
        public short CampMapSize;
        public int CreationRand;
        public int CreationTime;
        public int CreatorIP;
        public byte CurrentDay;
        public uint CurrentTime;
        public byte DayZero;
        public byte EndgameResult;
        public byte EnemyADExp;
        public byte EnemyAirExp;
        public short GroundRatio;
        public short Group;
        public short LastIndexNum;
        public short NavalRatio;
        public short NumAvailableSquadrons;
        public int NumPriorityEventEntries;
        public int NumRecentEventEntries;
        public VU_ID PlayerSquadronID;
        public EventNode[] PriorityEventEntries;
        public EventNode[] RecentEventEntries;
        public string SaveFile;
        public string Scenario;
        public byte Situation;
        public SquadInfo[] SquadInfo;
        public uint TE_StartTime;
        public uint TE_TimeLimit;
        public int TE_Type;
        public int TE_VictoryPoints;
        public int TE_flags;
        public int[] TE_number_aircraft = new int[8];
        public int[] TE_number_f16s = new int[8];
        public int TE_number_teams;
        public int TE_team;
        public int[] TE_team_pts = new int[8];
        public TeamBasicInfo[] TeamBasicInfo = new TeamBasicInfo[8];
        public byte Tempo;
        public string TheaterName;
        public short TheaterSizeX;
        public short TheaterSizeY;
        public short TimeStamp;
        public string UIName;
        public uint lastMajorEvent;
        public uint lastReinforcement;
        public uint lastRepair;
        public uint lastResupply;

        #endregion

        protected int _version;

        protected CmpFile()
        {
        }

        public CmpFile(byte[] compressed, int version)
            : this()
        {
            _version = version;
            byte[] expanded = Expand(compressed);
            if (expanded != null) Decode(expanded);
        }

        protected void Decode(byte[] bytes)
        {
            int curByte = 0;
            int nullLoc = 0;
            CurrentTime = BitConverter.ToUInt32(bytes, curByte);
            if (CurrentTime == 0) CurrentTime = 1;
            curByte += 4;

            if (_version >= 48)
            {
                TE_StartTime = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;

                TE_TimeLimit = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                if (_version >= 49)
                {
                    TE_VictoryPoints = BitConverter.ToInt32(bytes, curByte);
                    curByte += 4;
                }
                else
                {
                    TE_VictoryPoints = 0;
                }
            }
            else
            {
                TE_StartTime = CurrentTime;
                TE_TimeLimit = CurrentTime + (60*60*5*1000);
                TE_VictoryPoints = 0;
            }
            if (_version >= 52)
            {
                TE_Type = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                TE_number_teams = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                for (int i = 0; i < 8; i++)
                {
                    TE_number_aircraft[i] = BitConverter.ToInt32(bytes, curByte);
                    curByte += 4;
                }

                for (int i = 0; i < 8; i++)
                {
                    TE_number_f16s[i] = BitConverter.ToInt32(bytes, curByte);
                    curByte += 4;
                }

                TE_team = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                for (int i = 0; i < 8; i++)
                {
                    TE_team_pts[i] = BitConverter.ToInt32(bytes, curByte);
                    curByte += 4;
                }

                TE_flags = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                nullLoc = 0;
                for (int i = 0; i < 8; i++)
                {
                    var info = new TeamBasicInfo();
                    info.teamFlag = bytes[curByte];
                    curByte++;
                    info.teamColor = bytes[curByte];
                    curByte++;
                    info.teamName = Encoding.ASCII.GetString(bytes, curByte, 20);
                    curByte += 20;
                    nullLoc = info.teamName.IndexOf('\0');
                    if (nullLoc > -1) info.teamName = info.teamName.Substring(0, nullLoc);
                    info.teamMotto = Encoding.ASCII.GetString(bytes, curByte, 200);
                    curByte += 200;
                    nullLoc = info.teamMotto.IndexOf('\0');
                    if (nullLoc > -1) info.teamMotto = info.teamMotto.Substring(0, nullLoc);
                    TeamBasicInfo[i] = info;
                }
            }
            else
            {
                TE_Type = 0;
                TE_number_teams = 0;
                TE_number_aircraft = new int[8];
                TE_number_f16s = new int[8];
                TE_team = 0;
                TE_team_pts = new int[8];
                TE_flags = 0;
            }
            if (_version >= 19)
            {
                lastMajorEvent = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
            }
            lastResupply = BitConverter.ToUInt32(bytes, curByte);
            curByte += 4;

            lastRepair = BitConverter.ToUInt32(bytes, curByte);
            curByte += 4;

            lastReinforcement = BitConverter.ToUInt32(bytes, curByte);
            curByte += 4;

            TimeStamp = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            Group = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            GroundRatio = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            AirRatio = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            AirDefenseRatio = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            NavalRatio = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            Brief = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            TheaterSizeX = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            TheaterSizeY = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            CurrentDay = bytes[curByte];
            curByte++;

            ActiveTeams = bytes[curByte];
            curByte++;

            DayZero = bytes[curByte];
            curByte++;

            EndgameResult = bytes[curByte];
            curByte++;

            Situation = bytes[curByte];
            curByte++;

            EnemyAirExp = bytes[curByte];
            curByte++;

            EnemyADExp = bytes[curByte];
            curByte++;

            BullseyeName = bytes[curByte];
            curByte++;

            BullseyeX = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            BullseyeY = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;

            TheaterName = Encoding.ASCII.GetString(bytes, curByte, 40);
            curByte += 40;
            nullLoc = TheaterName.IndexOf('\0');
            if (nullLoc > -1) TheaterName = TheaterName.Substring(0, nullLoc);

            Scenario = Encoding.ASCII.GetString(bytes, curByte, 40);
            curByte += 40;
            nullLoc = Scenario.IndexOf('\0');
            if (nullLoc > -1) Scenario = Scenario.Substring(0, nullLoc);

            SaveFile = Encoding.ASCII.GetString(bytes, curByte, 40);
            curByte += 40;
            nullLoc = SaveFile.IndexOf('\0');
            if (nullLoc > -1) SaveFile = SaveFile.Substring(0, nullLoc);

            UIName = Encoding.ASCII.GetString(bytes, curByte, 40);
            curByte += 40;
            nullLoc = UIName.IndexOf('\0');
            if (nullLoc > -1) UIName = UIName.Substring(0, nullLoc);

            var squadronId = new VU_ID();
            squadronId.num_ = BitConverter.ToUInt32(bytes, curByte);
            curByte += 4;
            squadronId.creator_ = BitConverter.ToUInt32(bytes, curByte);
            curByte += 4;
            PlayerSquadronID = squadronId;

            NumRecentEventEntries = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            if (NumRecentEventEntries > 0)
            {
                RecentEventEntries = new EventNode[NumRecentEventEntries];
                for (int i = 0; i < NumRecentEventEntries; i++)
                {
                    var thisNode = new EventNode();
                    thisNode.x = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;
                    thisNode.y = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisNode.time = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;

                    thisNode.flags = bytes[curByte];
                    curByte++;

                    thisNode.Team = bytes[curByte];
                    curByte++;

                    curByte += 2; //align on int32 boundary
                    //skip EventText pointer
                    curByte += 4;
                    //skip UiEventNode pointer
                    curByte += 4;
                    ushort eventTextSize = BitConverter.ToUInt16(bytes, curByte);
                    curByte += 2;
                    string eventText = Encoding.ASCII.GetString(bytes, curByte, eventTextSize);
                    curByte += eventTextSize;
                    nullLoc = eventText.IndexOf('\0');
                    if (nullLoc > -1) eventText = eventText.Substring(0, nullLoc);
                    thisNode.eventText = eventText;
                    RecentEventEntries[i] = thisNode;
                }
            }


            NumPriorityEventEntries = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            if (NumPriorityEventEntries > 0)
            {
                PriorityEventEntries = new EventNode[NumPriorityEventEntries];
                for (int i = 0; i < NumPriorityEventEntries; i++)
                {
                    var thisNode = new EventNode();
                    thisNode.x = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;
                    thisNode.y = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisNode.time = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;

                    thisNode.flags = bytes[curByte];
                    curByte++;

                    thisNode.Team = bytes[curByte];
                    curByte++;

                    curByte += 2; //align on int32 boundary
                    //skip EventText pointer
                    curByte += 4;
                    //skip UiEventNode pointer
                    curByte += 4;

                    ushort eventTextSize = BitConverter.ToUInt16(bytes, curByte);
                    curByte += 2;
                    string eventText = Encoding.ASCII.GetString(bytes, curByte, eventTextSize);
                    curByte += eventTextSize;
                    nullLoc = eventText.IndexOf('\0');
                    if (nullLoc > -1) eventText = eventText.Substring(0, nullLoc);
                    thisNode.eventText = eventText;
                    PriorityEventEntries[i] = thisNode;
                }
            }
            CampMapSize = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            if (CampMapSize > 0)
            {
                CampMap = new byte[CampMapSize];
                Array.Copy(bytes, CampMap, CampMapSize);
            }
            curByte += CampMapSize;

            LastIndexNum = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            NumAvailableSquadrons = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            if (NumAvailableSquadrons > 0)
            {
                SquadInfo = new SquadInfo[NumAvailableSquadrons];
                for (int i = 0; i < NumAvailableSquadrons; i++)
                {
                    var thisSquadInfo = new SquadInfo();
                    thisSquadInfo.x = BitConverter.ToSingle(bytes, curByte);
                    curByte += 4;
                    thisSquadInfo.y = BitConverter.ToSingle(bytes, curByte);
                    curByte += 4;

                    var thisSquadId = new VU_ID();
                    thisSquadId.num_ = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;
                    thisSquadId.creator_ = BitConverter.ToUInt32(bytes, curByte);
                    curByte += 4;
                    thisSquadInfo.id = thisSquadId;

                    thisSquadInfo.descriptionIndex = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisSquadInfo.nameId = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisSquadInfo.airbaseIcon = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisSquadInfo.squadronPath = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;

                    thisSquadInfo.specialty = bytes[curByte];
                    curByte++;

                    thisSquadInfo.currentStrength = bytes[curByte];
                    curByte++;

                    thisSquadInfo.country = bytes[curByte];
                    curByte++;

                    thisSquadInfo.airbaseName = Encoding.ASCII.GetString(bytes, curByte, 40);
                    nullLoc = thisSquadInfo.airbaseName.IndexOf('\0');
                    if (nullLoc > -1) thisSquadInfo.airbaseName = thisSquadInfo.airbaseName.Substring(0, nullLoc);
                    curByte += 40;

                    if (_version < 42)
                    {
                        curByte += 40;
                            //skip additional string length for squad name in older versions that had 80 bytes
                    }

                    curByte++; //align on int32 boundary
                    SquadInfo[i] = thisSquadInfo;
                }
            }
            if (_version >= 31)
            {
                Tempo = bytes[curByte];
                curByte++;
            }
            if (_version >= 43)
            {
                CreatorIP = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                CreationTime = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;

                CreationRand = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;
            }
        }

        protected static byte[] Expand(byte[] compressed)
        {
            int compressedSize = BitConverter.ToInt32(compressed, 0);
            int uncompressedSize = BitConverter.ToInt32(compressed, 4);
            if (uncompressedSize == 0) return null;
            var actualCompressed = new byte[compressed.Length - 8];
            Array.Copy(compressed, 8, actualCompressed, 0, actualCompressed.Length);
            byte[] uncompressed = null;
            uncompressed = Codec.Decompress(actualCompressed, uncompressedSize);
            return uncompressed;
        }
    }
}