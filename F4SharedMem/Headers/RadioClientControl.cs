using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RadioClientControl
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int  PortNumber;                        // socket number to use in contacting the server

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType=UnmanagedType.U1, SizeConst=Constants.RCC_STRING_LENGTH)]
	    public byte[] Address;                       // string representation of server IPv4 dotted number address
        
        [FieldOffset(68)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.RCC_STRING_LENGTH)]
	    public byte[] Password;                    // plain text of password for voice server access
        
        [FieldOffset(132)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.RCC_STRING_LENGTH)]
	    public byte[] Nickname;                      // player nickname 
        
        [FieldOffset(196)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=Constants.NUMBER_OF_RADIOS)]
	    public RadioChannel[] Radios;  
        
        [FieldOffset(232)]
        [MarshalAs(UnmanagedType.U1)]
	    public bool SignalConnect;                     // tell the client we are ready to try a connection with the current settings

        [FieldOffset(233)]
        [MarshalAs(UnmanagedType.U1)]
	    public bool TerminateClient;                   // indicate to external client that it should shut down now

        [FieldOffset(234)]
        [MarshalAs(UnmanagedType.U1)]
	    public bool FlightMode;						 // true when in 3D world, false for UI state

        [FieldOffset(235)]
        [MarshalAs(UnmanagedType.U1)]
	    public bool UseAGC;							 // true when external voice client should use AGC features

        [FieldOffset(236)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NUMBER_OF_DEVICES)]
	    public RadioDevice[] Devices;

        [FieldOffset(240)]
        [MarshalAs(UnmanagedType.I4)]
        public int PlayerCount;                        // number of players for whom we have data in the telemetry map

        [FieldOffset(244)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.PLAYER_MAP_SIZE)]
        public Telemetry[] PlayerMap;    // array of player telemetry data relative to ownship (held in entry zero)
    }

    [ComVisible(true)]
    [Serializable]
    public enum Radios
    {
        UHF = 0,
        VHF,
        GUARD,
        NUMBER_OF_RADIOS,
    };

    [ComVisible(true)]
    [Serializable]
    public enum Devices
    {
        MAIN = 0,
        NUMBER_OF_DEVICES,
    };

    [ComVisible(true)]
    [Serializable]
    public struct Constants
    {
        public const int MAX_VOLUME = 0;
        public const int MIN_VOLUME = 10000;
        public const int Zero_dB_Raw_Volume_Default = 1304; // on a scale from +6dB ro -40dB
        public const int NAME_LEN = 20;
        public const int PLAYER_MAP_SIZE = 96;
        public const int NUMBER_OF_RADIOS = 3;
        public const int NUMBER_OF_DEVICES = 1;
        public const int RCC_STRING_LENGTH = 64;
    }

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Telemetry
    {
        // Data
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.R4)]
        public float Agl;  // height above terrain in feet

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.R4)]
        public float Range;  // range of remote player to ownship in nautical miles

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.U4)]
        public uint Flags;  // status information

        [FieldOffset(12)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = Constants.NAME_LEN + 1)]
        public byte[] LogbookName;  // copy of player logbook name

        [FieldOffset(33)]
        [MarshalAs(UnmanagedType.U1)]
        public byte padding1;

        [FieldOffset(34)]
        [MarshalAs(UnmanagedType.U1)]
        public byte padding2;

        [FieldOffset(35)]
        [MarshalAs(UnmanagedType.U1)]
        public byte padding3;
    }

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RadioDevice
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int IcVolume; // INTERCOM volume
    }

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RadioChannel
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int Frequency;     // 6 digit MHz frequency x1000 (i.e. no decimal places)

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I4)]
        public int RxVolume;      // 0-15000 range, high to low respectively

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.U1)]
        public bool PttDepressed;  // true for transmit switch activated

        [FieldOffset(9)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOn;          // true if this channel is associated with a radio that is on

        [FieldOffset(10)]
        [MarshalAs(UnmanagedType.U1)]
        public byte padding1;

        [FieldOffset(11)]
        [MarshalAs(UnmanagedType.U1)]
        public byte padding2;
    }
    [ComVisible(true)]
    [Flags]
    [Serializable]
    public enum TelemetryFlags:byte
    {
        NoFlags = 0x00,
        HasPlayerLoS = 0x01,
        IsAircraft = 0x02,
    };
}
