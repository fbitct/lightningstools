namespace AnalogDevices
{
    internal enum GpioRegisterBits:byte
    {
        F0=(byte)BasicMasks.BitZero,
        F1=(byte)BasicMasks.BitOne,
        ReadableBits = F1 | F0,
        WritableBits = F1 | F0,

        Data=F0,
        Direction=F1,
    }
}
