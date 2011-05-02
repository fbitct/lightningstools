using System;

namespace F4Utils.Campaign
{
    public class Flight : AirUnit
    {
        #region Public Fields

        public byte callsign_id;
        public byte callsign_num;
        public byte dummy;
        public byte eval_flags;
        public int fuel_burnt;
        public uint last_combat;
        public byte last_direction;
        public uint last_move;
        public byte last_player_slot;
        public LoadoutStruct[] loadout;
        public byte loadouts;
        public byte mission;
        public byte mission_context;
        public byte mission_id;
        public uint mission_over_time;
        public short mission_target;
        public byte old_mission;
        public VU_ID package;
        public byte[] pilots;
        public byte[] plane_stats;
        public byte[] player_slots;
        public byte priority;
        public VU_ID requester;
        public byte[] slots;
        public VU_ID squadron;
        public uint time_on_target;
        public byte use_loadout;
        public short[] weapon;
        public byte[] weapons;

        #endregion

        private const int WEAPON_IDS_WIDENED_VERSION = 73;
        private const int NEW_ENDING_FIELD_ADDED_VERSION = 73;

        protected Flight()
        {
        }

        public Flight(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            z = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            fuel_burnt = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            if (version < 65)
            {
                fuel_burnt = 0;
            }

            last_move = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            last_combat = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            time_on_target = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            mission_over_time = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            mission_target = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            loadouts = 0;
            if (version < 24)
            {
                use_loadout = 0;
                weapons = new byte[16];
                loadouts = 1;
                loadout = new LoadoutStruct[loadouts];
                if (version >= 8)
                {
                    use_loadout = bytes[offset];
                    offset++;

                    if (use_loadout != 0)
                    {
                        var junk = new LoadoutArray();
                        junk.Stores = new LoadoutStruct[5];
                        for (var j = 0; j < 5; j++)
                        {
                            var thisStore = junk.Stores[j];
                            thisStore.WeaponID = new ushort[16];
                            for (var k = 0; k < 16; k++)
                            {
                                thisStore.WeaponID[k] = bytes[offset];
                                offset++;
                            }

                            thisStore.WeaponCount = new byte[16];
                            for (var k = 0; k < 16; k++)
                            {
                                thisStore.WeaponCount[k] = bytes[offset];
                                offset++;
                            }
                        }
                        loadout[0] = junk.Stores[0];
                    }
                }
                weapon = new short[16];
                if (version < 18)
                {
                    for (var j = 0; j < 16; j++)
                    {
                        weapon[j] = BitConverter.ToInt16(bytes, offset);
                        offset += 2;
                    }
                    if (use_loadout == 0)
                    {
                        for (var j = 0; j < 16; j++)
                        {
                            loadout[0].WeaponID[j] = (byte) weapon[j];
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < 16; j++)
                    {
                        weapon[j] = bytes[offset];
                        offset++;
                    }
                    if (use_loadout == 0)
                    {
                        for (var j = 0; j < 16; j++)
                        {
                            loadout[0].WeaponID[j] = (byte) weapon[j];
                        }
                    }
                }
                for (var j = 0; j < 16; j++)
                {
                    weapons[j] = bytes[offset];
                    offset++;
                }
                if (use_loadout == 0)
                {
                    for (var j = 0; j < 16; j++)
                    {
                        loadout[0].WeaponCount[j] = weapons[j];
                    }
                }
            }
            else
            {
                loadouts = bytes[offset];
                offset++;
                loadout = new LoadoutStruct[loadouts];
                for (var j = 0; j < loadouts; j++)
                {
                    var thisLoadout = new LoadoutStruct();
                    thisLoadout.WeaponID = new ushort[16];
                    for (var k = 0; k < 16; k++)
                    {
                        if (version >= WEAPON_IDS_WIDENED_VERSION)
                        {
                            thisLoadout.WeaponID[k] = BitConverter.ToUInt16(bytes, offset);
                            offset += 2;
                        }
                        else
                        {
                            thisLoadout.WeaponID[k] = bytes[offset];
                            offset++;
                        }
                    }
                    thisLoadout.WeaponCount = new byte[16];
                    for (var k = 0; k < 16; k++)
                    {
                        thisLoadout.WeaponCount[k] = bytes[offset];
                        offset++;
                    }
                    loadout[j] = thisLoadout;
                }
            }
            mission = bytes[offset];
            offset++;

            if (version > 65)
            {
                old_mission = bytes[offset];
                offset++;
            }
            else
            {
                old_mission = mission;
            }
            last_direction = bytes[offset];
            offset++;

            priority = bytes[offset];
            offset++;

            mission_id = bytes[offset];
            offset++;

            if (version < 14)
            {
                dummy = bytes[offset];
                offset++;
            }
            eval_flags = bytes[offset];
            offset++;

            if (version > 65)
            {
                mission_context = bytes[offset];
                offset++;
            }
            else
            {
                mission_context = 0;
            }

            package = new VU_ID();
            package.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            package.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            squadron = new VU_ID();
            squadron.num_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            squadron.creator_ = BitConverter.ToUInt32(bytes, offset);
            offset += 4;

            if (version > 65)
            {
                requester = new VU_ID();
                requester.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                requester.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
            }
            else
            {
                requester = new VU_ID();
            }

            slots = new byte[4];
            for (var j = 0; j < 4; j++)
            {
                slots[j] = bytes[offset];
                offset++;
            }

            pilots = new byte[4];
            for (var j = 0; j < 4; j++)
            {
                pilots[j] = bytes[offset];
                offset++;
            }

            plane_stats = new byte[4];
            for (var j = 0; j < 4; j++)
            {
                plane_stats[j] = bytes[offset];
                offset++;
            }

            player_slots = new byte[4];
            for (var j = 0; j < 4; j++)
            {
                player_slots[j] = bytes[offset];
                offset++;
            }

            last_player_slot = bytes[offset];
            offset++;

            callsign_id = bytes[offset];
            offset++;

            callsign_num = bytes[offset];
            offset++;

            if (version >= NEW_ENDING_FIELD_ADDED_VERSION)
            {
                offset += 4;
            }
        }
    }
}