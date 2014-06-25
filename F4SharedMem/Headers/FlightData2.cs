using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace F4SharedMem.Headers
{
    [ComVisible(false)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FlightData2
    {
        int flightData2VersionNum;		// Version of FlightData2 mem area
        float nozzlePos2;   // Ownship engine nozzle2 percent open (0-100)
        float rpm2;         // Ownship engine rpm2 (Percent 0-103)
        float ftit2;        // Ownship Forward Turbine Inlet Temp2 (Degrees C)
        float oilPressure2; // Ownship Oil Pressure2 (Percent 0-100)
        float hydPressureA;  // Ownship Hydraulic Pressure A
        float hydPressureB;  // Ownship Hydraulic Pressure B

        byte navMode;  // current mode selected for HSI/eHSI, see NavModes enum for details

        float AAUZ;         // Ownship barometric altitude given by AAU (depends on calibration)
        int AltCalReading;	// barometric altitude calibration (depends on CalType)
        int altBits;		// various altimeter bits, see AltBits enum for details
        float cabinAlt;		// Ownship cabin altitude

        int BupUhfPreset;	// BUP UHF channel preset
        int BupUhfFreq;		// BUP UHF channel frequency

        int powerBits;		// Ownship power bus / generator states, see PowerBits enum for details
        int blinkBits;		// Cockpit indicator lights blink status, see BlinkBits enum for details
        // NOTE: these bits indicate only *if* a lamp is blinking, in addition to the
        // existing on/off bits. It's up to the external program to implement the
        // *actual* blinking.
        int cmdsMode;		// Ownship CMDS mode state, see CmdsModes enum for details
        int currentTime;	// Current time in seconds (max 60 * 60 * 24)

        short vehicleACD;	// Ownship ACD index number, i.e. which aircraft type are we flying.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        byte[] tacanInfo; //TACAN info (new in BMS4)
    }

}
