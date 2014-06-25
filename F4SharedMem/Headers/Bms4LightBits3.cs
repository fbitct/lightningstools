﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum Bms4LightBits3 : int
    {
        // Elec panel
        FlcsPmg = 0x1,
        MainGen = 0x2,
        StbyGen = 0x4,
        EpuGen = 0x8,
        EpuPmg = 0x10,
        ToFlcs = 0x20,
        FlcsRly = 0x40,
        BatFail = 0x80,

        // EPU panel
        Hydrazine = 0x100,
        Air = 0x200,

        // Caution panel
        Elec_Fault = 0x400,
        Lef_Fault = 0x800,

        OnGround = 0x1000,   // weight-on-wheels
        FlcsBitRun = 0x2000,   // FLT CONTROL panel RUN light (used to be Multi-engine fire light)
        FlcsBitFail = 0x4000,   // FLT CONTROL panel FAIL light (used to be Lock light Cue; non-F-16)
        DbuWarn = 0x8000,   // Right eyebrow DBU ON cell; was Shoot light cue; non-F16
        NoseGearDown = 0x10000,  // Landing gear panel; on means down and locked
        LeftGearDown = 0x20000,  // Landing gear panel; on means down and locked
        RightGearDown = 0x40000,  // Landing gear panel; on means down and locked
        ParkBrakeOn = 0x100000, // Parking brake engaged; NOTE: not a lamp bit
        Power_Off = 0x200000, // Set if there is no electrical power.  NB: not a lamp bit

        // Caution panel
        cadc = 0x400000,

        // Left Aux console
        SpeedBrake = 0x800000,  // True if speed brake is in anything other than stowed position

        // Threat Warning Prime - additional bits
        SysTest = 0x1000000,

        // Master Caution WILL come up (actual lightBit has 3sec delay like in RL),
        // usable for cockpit builders with RL equipment which has a delay on its own.
        // Will be set to false again as soon as the MasterCaution bit is set.
        MCAnnounced = 0x2000000,

        // Free bits in LightBits3
        //0x4000000,
        //0x8000000,
        //0x10000000,
        //0x20000000,
        //0x40000000,
        //0x80000000,

        // Used with the MAL/IND light code to light up "everything"
        // please update this if you add/change bits!
        AllLampBits3On = 0x0147EFFF

        // Used with the MAL/IND light code to light up "everything"
        // please update this if you add/change bits!
        //AllLampBits3On = 0x0047EFFF
    };
}
