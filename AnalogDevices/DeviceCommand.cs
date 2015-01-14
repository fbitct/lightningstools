namespace AnalogDevices
{
    internal enum DeviceCommand : byte
    {
        /// <summary>
        ///   Loads Firmware to the device
        /// </summary>
        LoadFirmware = 0xA0,
        /// <summary>
        ///   Sets RESET pin high
        /// </summary>
        SetRESETPinHigh = 0xDA,
        /// <summary>
        ///   Sets RESET pin low
        /// </summary>
        SetRESETPinLow = 0xDB,
        /// <summary>
        ///   Sets CLR pin high
        /// </summary>
        SetCLRPinHigh = 0xDC,
        /// <summary>
        ///   Sends 24-bit word over SPI
        /// </summary>
        SendSPI = 0xDD,
        /// <summary>
        ///   Pulse LDac pin
        /// </summary>
        PulseLDacPin = 0xDE,
        /// <summary>
        ///   Sets CLR pin low
        /// </summary>
        SetCLRPinLow = 0xDF,
        /// <summary>
        ///   Initializes the SPI pins
        /// </summary>
        InitializeSPIPins = 0xE0,
        /// <summary>
        ///   Sets the LDac pin high
        /// </summary>
        SetLDacPinHigh = 0xE2,
        /// <summary>
        ///   Sets the LDac pin low
        /// </summary>
        SetLDacPinLow = 0xE3,
    }
}