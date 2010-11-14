using System;
namespace F4Utils.Campaign
{
    public class Squadron : AirUnit
    {
        #region Public Fields
        public int fuel;
        public byte specialty;
        public byte[] stores;
        public Pilot[] pilots;
        public int[] schedule;
        public VU_ID airbase_id;
        public VU_ID hot_spot;
        public VU_ID junk;
        public byte[] rating;
        public short aa_kills;
        public short ag_kills;
        public short as_kills;
        public short an_kills;
        public short missions_flown;
        public short mission_score;
        public byte total_losses;
        public byte pilot_losses;
        public byte squadron_patch;
        #endregion

        protected Squadron()
            : base()
        {
        }
        public Squadron(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            fuel = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            specialty = bytes[offset];
            offset++;

            if (version < 69)
            {
                stores = new byte[200];
                for (int i = 0; i < 200; i++)
                {
                    stores[i] = bytes[offset];
                    offset++;
                }
            }
            else
            {
                if (version >= 72)
                {
                    stores = new byte[600];
                    for (int i = 0; i < 600; i++)
                    {
                        stores[i] = bytes[offset];
                        offset++;
                    }
                }
                else
                {
                    stores = new byte[220];
                    for (int i = 0; i < 220; i++)
                    {
                        stores[i] = bytes[offset];
                        offset++;
                    }
                }
            }

            if (version < 47)
            {
                if (version >= 29)
                {
                    pilots = new Pilot[48];
                    for (int j = 0; j < pilots.Length; j++)
                    {
                        Pilot thisPilot = new Pilot();
                        thisPilot.pilot_id = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                        thisPilot.pilot_skill_and_rating = bytes[offset];
                        offset++;
                        thisPilot.pilot_status = bytes[offset];
                        offset++;
                        thisPilot.aa_kills = bytes[offset];
                        offset++;
                        thisPilot.ag_kills = bytes[offset];
                        offset++;
                        thisPilot.as_kills = bytes[offset];
                        offset++;
                        thisPilot.an_kills = bytes[offset];
                        offset++;
                        /*
                        p.missions_flown = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                         */
                        pilots[j] = thisPilot;
                    }
                }
                else
                {
                    pilots = new Pilot[36];
                    for (int j = 0; j < pilots.Length; j++)
                    {
                        Pilot thisPilot = new Pilot();
                        thisPilot.pilot_id = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                        thisPilot.pilot_skill_and_rating = bytes[offset];
                        offset++;
                        thisPilot.pilot_status = bytes[offset];
                        offset++;
                        thisPilot.aa_kills = bytes[offset];
                        offset++;
                        thisPilot.ag_kills = bytes[offset];
                        offset++;
                        thisPilot.as_kills = bytes[offset];
                        offset++;
                        thisPilot.an_kills = bytes[offset];
                        offset++;
                        /*
                        p.missions_flown = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                         */
                        pilots[j] = thisPilot;
                    }
                }
            }
            else
            {
                pilots = new Pilot[48];
                for (int j = 0; j < pilots.Length; j++)
                {
                    Pilot thisPilot = new Pilot();
                    thisPilot.pilot_id = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    thisPilot.pilot_skill_and_rating = bytes[offset];
                    offset++;
                    thisPilot.pilot_status = bytes[offset];
                    offset++;
                    thisPilot.aa_kills = bytes[offset];
                    offset++;
                    thisPilot.ag_kills = bytes[offset];
                    offset++;
                    thisPilot.as_kills = bytes[offset];
                    offset++;
                    thisPilot.an_kills = bytes[offset];
                    offset++;
                    thisPilot.missions_flown = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    pilots[j] = thisPilot;
                }
            }
            schedule = new int[16];
            for (int j = 0; j < schedule.Length; j++)
            {
                schedule[j] = BitConverter.ToInt32(bytes, offset);
                offset += 4;
            }
            airbase_id = new VU_ID();
            airbase_id.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            airbase_id.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            hot_spot = new VU_ID();
            hot_spot.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            hot_spot.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            if (version >= 6 && version < 16)
            {
                junk = new VU_ID();
                junk.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                junk.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            rating = new byte[16];
            for (int j = 0; j < rating.Length; j++)
            {
                rating[j] = bytes[offset];
                offset++;
            }
            aa_kills = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            ag_kills = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            as_kills = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            an_kills = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            missions_flown = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            mission_score = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            total_losses = bytes[offset];
            offset++;

            if (version >= 9)
            {
                pilot_losses = bytes[offset];
                offset++;
            }
            else
            {
                pilot_losses = 0;
            }

            if (!(version < 45))
            {
                squadron_patch = bytes[offset];
                offset++;
            }

        }
    }
}