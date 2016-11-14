using LibUsbDotNet;

namespace AnalogDevices
{
    public interface IDenseDacEvalBoard
    {
        UsbDevice UsbDevice { get; }
        DacPrecision DacPrecision { get; set; }
        string SymbolicName { get; }

        ChannelMonitorOptions ChannelMonitorOptions { get; set; }

        IODirection GeneralPurposeIOPinDirection { get; set; }
        bool GeneralPurposeIOPinState { get; set; }

        bool IsOverTemperature { get; }
        bool IsThermalShutdownEnabled { get; set; }

        ushort OffsetDAC0 { get; set; }
        ushort OffsetDAC1 { get; set; }
        ushort OffsetDAC2 { get; set; }

        bool PacketErrorCheckErrorOccurred { get; }

        DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel);
        void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value);
        void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7);
        void SetDacChannelDataSourceAllChannels(DacChannelDataSource source);

        ushort GetDacChannelDataValueA(ChannelAddress channel);
        ushort GetDacChannelDataValueB(ChannelAddress channel);

        ushort GetDacChannelGain(ChannelAddress channel);
        ushort GetDacChannelOffset(ChannelAddress channel);

        void SetDacChannelDataValueA(ChannelAddress channel, ushort value);
        void SetDacChannelDataValueB(ChannelAddress channel, ushort value);

        void SetDacChannelOffset(ChannelAddress channel, ushort value);
        void SetDacChannelGain(ChannelAddress channel, ushort value);

        void PerformSoftPowerDown();
        void PerformSoftPowerUp();

        void SetLDACPinHigh();
        void SetLDACPinLow();
        void PulseLDACPin();

        void SuspendAllDacOutputs();
        void ResumeAllDacOutputs();

        void Reset();

        void UpdateAllDacOutputs();

        long UploadFirmware(IhxFile ihxFile);
    }
}