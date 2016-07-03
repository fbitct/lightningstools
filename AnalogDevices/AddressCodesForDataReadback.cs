namespace AnalogDevices
{
    internal enum AddressCodesForDataReadback:ushort
    {
        X1ARegister =0x0000,
        X1BRegister = 0x2000,
        CRegister = 0x4000,
        MRegister = 0x6000,
        ControlRegister = 0x8080,
        OSF0Register= 0x8100,
        OSF1Register = 0x8180,
        OSF2Register=0x8200,
        ABSelect0Register=0x8300,
        ABSelect1Register=0x8380,
        ABSelect2Register = 0x8400,
        ABSelect3Register = 0x8480,
        ABSelect4Register = 0x8500,
        GPIORegister= 0x8580,

    }
}
