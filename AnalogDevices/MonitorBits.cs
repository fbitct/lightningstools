namespace AnalogDevices
{
    internal enum MonitorBits:ushort
    {
        F0=BasicMasks.BitZero,
        F1=BasicMasks.BitOne,
        F2=BasicMasks.BitTwo,
        F3=BasicMasks.BitThree,
        F4=BasicMasks.BitFour,
        F5=BasicMasks.BitFive,

        MonitorEnable=F5,
        SourceSelect=F4,
        DacChannel=F3|F2|F1|F0,
        InputPin=F0,
        NotUsedWithInputPinMonitoring = F3 | F2 | F1,


    }
}
