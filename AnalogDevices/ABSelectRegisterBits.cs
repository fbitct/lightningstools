namespace AnalogDevices
{
    internal enum ABSelectRegisterBits
    {
        F0 = BasicMasks.BitZero,
        F1 = BasicMasks.BitOne,
        F2 = BasicMasks.BitTwo,
        F3 = BasicMasks.BitThree,
        F4 = BasicMasks.BitFour,
        F5 = BasicMasks.BitFive,
        F6 = BasicMasks.BitSix,
        F7 = BasicMasks.BitSeven,

        Channel0 = F0,
        Channel1 = F1,
        Channel2 = F2,
        Channel3 = F3,
        Channel4 = F4,
        Channel5 = F5,
        Channel6 = F6,
        Channel7 = F7,

        ReadableBits = F7 | F6 | F5 | F4 | F3 | F2 | F1 | F0,
        WritableBits = F7 | F6 | F5 | F4 | F3 | F2 | F1 | F0,

        AllChannelsA = BasicMasks.AllBitsZero,
        AllChannelsB = Channel0 | Channel1 | Channel2 | Channel3 | Channel4 | Channel5 | Channel6 | Channel7
    }
}
