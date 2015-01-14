namespace AnalogDevices
{
    internal enum SpecialFunctionCode
    {
        /// <summary>
        ///   No-op
        /// </summary>
        NOP = 0,
        WriteControlRegister = 1,
        WriteOSF0Register = 2,
        WriteOSF1Register = 3,
        WriteOSF2Register = 4,
        SelectRegisterForReadback = 5,
        WriteToABSelectRegister0 = 6,
        WriteToABSelectRegister1 = 7,
        WriteToABSelectRegister2 = 8,
        WriteToABSelectRegister3 = 9,
        WriteToABSelectRegister4 = 10,
        BlockWriteABSelectRegisters = 11,
        ConfigureMonitoring = 12,
        GPIOConfigureAndWrite = 13
    }
}