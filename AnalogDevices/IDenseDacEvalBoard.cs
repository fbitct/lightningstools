using LibUsbDotNet;

namespace AnalogDevices
{
    public interface IDenseDacEvalBoard
    {
        ChannelMonitorOptions ChannelMonitorOptions { get; set; }
        DacPrecision DacPrecision { get; set; }
        IODirection GeneralPurposeIOPinDirection { get; set; }
        bool GeneralPurposeIOPinState { get; set; }
        bool IsOverTemperature { get; }
        bool IsThermalShutdownEnabled { get; set; }
        ushort OffsetDAC0 { get; set; }
        ushort OffsetDAC1 { get; set; }
        ushort OffsetDAC2 { get; set; }
        bool PacketErrorCheckErrorOccurred { get; }
        string SymbolicName { get; }
        UsbDevice UsbDevice { get; }

        DacChannelDataSource GetDacChannelDataSource(ChannelAddress channelAddress);
        ushort GetDacChannelDataValueA(ChannelAddress channel);
        ushort GetDacChannelDataValueB(ChannelAddress channel);
        ushort GetDacChannelGain(ChannelAddress channel);
        ushort GetDacChannelOffset(ChannelAddress channel);
        void PerformSoftPowerDown();
        void PerformSoftPowerUp();
        void PulseLDACPin();
        void Reset();
        void ResumeAllDacOutputs();
        void SetDacChannelDataSource(ChannelAddress channelAddress, DacChannelDataSource value);
        void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7);
        void SetDacChannelDataSourceAllChannels(DacChannelDataSource source);
        void SetDacChannelDataValueA(ChannelAddress channelAddress, ushort newVal);
        void SetDacChannelDataValueB(ChannelAddress channels, ushort newVal);
        void SetDacChannelGain(ChannelAddress channels, ushort newVal);
        void SetDacChannelOffset(ChannelAddress channels, ushort newVal);
        void SetLDACPinHigh();
        void SetLDACPinLow();
        void SuspendAllDacOutputs();
        void UpdateAllDacOutputs();
        long UploadFirmware(IhxFile ihxFile);
    }
}