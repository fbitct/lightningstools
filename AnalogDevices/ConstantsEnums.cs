namespace AnalogDevices
{

    #region Public Enums

    public enum DeviceType
    {
        Unknown = 0,
        DacEvalBoard,
    }

    public enum ChannelMonitorSource
    {
        None = 0,
        InputPin,
        DacChannel
    }

    public enum DacPrecision
    {
        Unknown = 0,
        FourteenBit = 1,
        SixteenBit = 2
    }

    public enum DacChannelDataSource
    {
        DataValueA = 0,
        DataValueB = 1
    }

    public enum ChannelAddress
    {
        AllGroupsAllChannels = 0,
        Group0AllChannels = 1,
        Group1AllChannels = 2,
        Group2AllChannels = 3,
        Group3AllChannels = 4,
        Group4AllChannels = 5,
        //Reserved=6,
        //Reserved=7,

        Group0Channel0 = 8,
        Group0Channel1 = 9,
        Group0Channel2 = 10,
        Group0Channel3 = 11,
        Group0Channel4 = 12,
        Group0Channel5 = 13,
        Group0Channel6 = 14,
        Group0Channel7 = 15,

        Group1Channel0 = 16,
        Group1Channel1 = 17,
        Group1Channel2 = 18,
        Group1Channel3 = 19,
        Group1Channel4 = 20,
        Group1Channel5 = 21,
        Group1Channel6 = 22,
        Group1Channel7 = 23,

        Group2Channel0 = 24,
        Group2Channel1 = 25,
        Group2Channel2 = 26,
        Group2Channel3 = 27,
        Group2Channel4 = 28,
        Group2Channel5 = 29,
        Group2Channel6 = 30,
        Group2Channel7 = 31,

        Group3Channel0 = 32,
        Group3Channel1 = 33,
        Group3Channel2 = 34,
        Group3Channel3 = 35,
        Group3Channel4 = 36,
        Group3Channel5 = 37,
        Group3Channel6 = 38,
        Group3Channel7 = 39,

        Group4Channel0 = 40,
        Group4Channel1 = 41,
        Group4Channel2 = 42,
        Group4Channel3 = 43,
        Group4Channel4 = 44,
        Group4Channel5 = 45,
        Group4Channel6 = 46,
        Group4Channel7 = 47,

        Group0Through4Channel0 = 48,
        Group0Through4Channel1 = 49,
        Group0Through4Channel2 = 50,
        Group0Through4Channel3 = 51,
        Group0Through4Channel4 = 52,
        Group0Through4Channel5 = 53,
        Group0Through4Channel6 = 54,
        Group0Through4Channel7 = 55,

        Group1Through4Channel0 = 56,
        Group1Through4Channel1 = 57,
        Group1Through4Channel2 = 58,
        Group1Through4Channel3 = 59,
        Group1Through4Channel4 = 60,
        Group1Through4Channel5 = 61,
        Group1Through4Channel6 = 62,
        Group1Through4Channel7 = 63,
    }

    public enum ChannelGroup
    {
        Group0 = 0,
        Group1 = 1,
        Group2 = 2,
        Group3 = 3,
        Group4 = 4
    }

    public enum IODirection
    {
        Output,
        Input
    }

    #endregion
}