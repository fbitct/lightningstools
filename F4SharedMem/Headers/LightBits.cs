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
    public enum LightBits : int
    {
        MasterCaution = 0x1,  // Left eyebrow

        // Brow Lights
        TF = 0x2,   // Left eyebrow
        OBS = 0x4,   // Not used
        OXY_BROW = 0x4, // repurposed for eyebrow OXY LOW (was OBS, unused)
        ALT = 0x8,   // Caution light; not used
        EQUIP_HOT = 0x8, // Caution light; repurposed for cooling fault (was: not used)
        WOW = 0x10,  // True if weight is on wheels: this is not a lamp bit!
        ENG_FIRE = 0x20,  // Right eyebrow; upper half of split face lamp
        CONFIG = 0x40,  // Stores config, caution panel
        HYD = 0x80,  // Right eyebrow; see also OIL (this lamp is not split face)
        OIL = 0x100, // Right eyebrow; see also HYD (this lamp is not split face)
        Flcs_ABCD = 0x100, // TEST panel FLCS channel lamps; repurposed, was OIL (see HYD; that lamp is not split face)
        DUAL = 0x200, // Right eyebrow; block 25, 30/32 and older 40/42
        FLCS = 0x200,// Right eyebrow; was called DUAL which matches block 25, 30/32 and older 40/42
        CAN = 0x400, // Right eyebrow
        T_L_CFG = 0x800, // Right eyebrow

        // AOA Indexers
        AOAAbove = 0x1000,
        AOAOn = 0x2000,
        AOABelow = 0x4000,

        // Refuel/NWS
        RefuelRDY = 0x8000,
        RefuelAR = 0x10000,
        RefuelDSC = 0x20000,

        // Caution Lights
        FltControlSys = 0x40000,
        LEFlaps = 0x80000,
        EngineFault = 0x100000,
        Overheat = 0x200000,
        FuelLow = 0x400000,
        Avionics = 0x800000,
        RadarAlt = 0x1000000,
        IFF = 0x2000000,
        ECM = 0x4000000,
        Hook = 0x8000000,
        NWSFail = 0x10000000,
        CabinPress = 0x20000000,

        AutoPilotOn = 0x40000000,  // TRUE if is AP on.  NB: This is not a lamp bit!
        //TFR_STBY = 0x80000000,  // MISC panel; lower half of split face TFR lamp
        TFR_STBY = -2147483648,  // MISC panel; lower half of split face TFR lamp

        // Used with the MAL/IND light code to light up "everything"
        // please update this is you add/change bits!
        AllLampBitsOn = -1073741841 //0xBFFFFFEF
    };
}