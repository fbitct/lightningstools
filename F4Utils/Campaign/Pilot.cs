namespace F4Utils.Campaign
{
    public class Pilot
    {
        public byte aa_kills;
        public byte ag_kills;
        public byte an_kills;
        public byte as_kills;
        public short missions_flown;
        public short pilot_id; // Index into the PilotInfoClass table
        public byte pilot_skill_and_rating; // Low Nibble: Skill, Hi Nibble: Rating
        public byte pilot_status;
    }
}