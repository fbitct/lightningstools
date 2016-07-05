namespace AnalogDevices
{
    internal enum SerialInterfaceModeBits:uint
    {
        M1 = 1 << 23, //bit I23 in serial word bit assignment
        M0 = 1 << 22, //bit I22 in serial word bit assignment

        SpecialFunction            = ~M1 | ~M0, //bits M1=0 and bit M2=0
        WriteToDACGainRegister     = ~M1 |  M0, //bit M1=0 and bit M0=1
        WriteToDACOffsetRegister   =  M1 | ~M0, //bit M1=1 and bit M0=0
        WriteToDACInputDataRegister=  M1 |  M0  //bit M1=1 and bit M1=1
    }
}
