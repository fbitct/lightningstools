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
    [StructLayout(LayoutKind.Sequential)]
    public struct BMS4FlightData2
    {
        float nozzlePos2;   // Ownship engine nozzle2 percent open (0-100)
        float rpm2;         // Ownship engine rpm2 (Percent 0-103)
        float ftit2;        // Ownship Forward Turbine Inlet Temp2 (Degrees C)
        float oilPressure2; // Ownship Oil Pressure2 (Percent 0-100)
        byte navMode;  // current mode selected for HSI/eHSI (added in BMS4)
        float AAUZ; // Ownship barometric altitude given by AAU (depends on calibration)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TacanSources.NUMBER_OF_SOURCES)]
        byte[] tacanInfo;      // Tacan band/mode settings for UFC and AUX COMM
        int AltCalReading;	// barometric altitude calibration (depends on CalType)
        int altBits;		// various altimeter bits, see AltBits enum for details
        int powerBits;		// Ownship power bus / generator states, see PowerBits enum for details
        int blinkBits;		// Cockpit indicator lights blink status, see BlinkBits enum for details
        // NOTE: these bits indicate only *if* a lamp is blinking, in addition to the
        // existing on/off bits. It's up to the external program to implement the
        // *actual* blinking.
        int cmdsMode;		// Ownship CMDS mode state, see CmdsModes enum for details
        int BupUhfPreset;	// BUP UHF channel preset

        int bupUhfFreq; // BUP UHF channel frequency
        float cabinAlt;// Ownship cabin altitude
        float hydPressureA; // Ownship Hydraulic Pressure A
        float hydPressureB;// Ownship Hydraulic Pressure B
        int currentTime;	// Current time in seconds (max 60 * 60 * 24)
        short vehicleACD;	// Ownship ACD index number, i.e. which aircraft type are we flying.
        int flightData2VersionNum;

        float fuelFlow2;
        // VERSION 5
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        char[] RwrInfo;  // New RWR Info
        float lefPos;       // Ownship LEF position
        float tefPos;       // Ownship TEF position

        // VERSION 6
        float vtolPos;      // Ownship VTOL exhaust angle
    }


}
