using System.Threading.Tasks;
using LibUsbDotNet;

namespace AnalogDevices
{
    public interface IDenseDacEvalBoard
    {
        UsbDevice UsbDevice { get; }
        DacPrecision DacPrecision { get; set; }
        string SymbolicName { get; }

        ChannelMonitorOptions ChannelMonitorOptions { get; set; }
        Task SetChannelMonitorOptionsAsync(ChannelMonitorOptions value);

        IODirection GeneralPurposeIOPinDirection { get; set; }
        Task<IODirection> GetGeneralPurposeIOPinDirectionAsync();
        Task SetGeneralPurposeIOPinDirectionAsync(IODirection value);

        bool GeneralPurposeIOPinState { get; set; }
        Task<bool> GetGeneralPurposeIOPinStateAsync();
        Task SetGeneralPurposeIOPinStateAsync(bool value);

        bool IsOverTemperature { get; }
        Task<bool> GetIsOverTemperatureAsync();

        bool IsThermalShutdownEnabled { get; set; }
        Task<bool> GetIsThermalShutdownEnabledAsync();
        Task SetIsThermalShutdownEnabledAsync(bool value);

        void DisableThermalShutdown();
        Task DisableThermalShutdownAsync();

        void EnableThermalShutdown();
        Task EnableThermalShutdownAsync();

        ushort OffsetDAC0 { get; set; }
        Task<ushort> GetOffsetDAC0Async();
        Task SetOffsetDAC0Async(ushort value);

        ushort OffsetDAC1 { get; set; }
        Task<ushort> GetOffsetDAC1Async();
        Task SetOffsetDAC1Async(ushort value);

        ushort OffsetDAC2 { get; set; }
        Task<ushort> GetOffsetDAC2Async();
        Task SetOffsetDAC2Async(ushort value);

        bool PacketErrorCheckErrorOccurred { get; }
        Task<bool> GetPacketErrorCheckErrorOccurredAsync();

        DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel);
        Task<DacChannelDataSource> GetDacChannelDataSourceAsync(ChannelAddress channel);

        ushort GetDacChannelDataValueA(ChannelAddress channel);
        Task<ushort> GetDacChannelDataValueAAsync(ChannelAddress channel);

        ushort GetDacChannelDataValueB(ChannelAddress channel);
        Task<ushort> GetDacChannelDataValueBAsync(ChannelAddress channel);

        ushort GetDacChannelGain(ChannelAddress channel);
        Task<ushort> GetDacChannelGainAsync(ChannelAddress channel);

        ushort GetDacChannelOffset(ChannelAddress channel);
        Task<ushort> GetDacChannelOffsetAsync(ChannelAddress channel);

        void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value);
        Task SetDacChannelDataSourceAsync(ChannelAddress channel, DacChannelDataSource value);

        void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7);
        Task SetDacChannelDataSourceAsync(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7);

        void SetDacChannelDataSourceAllChannels(DacChannelDataSource source);
        Task SetDacChannelDataSourceAllChannelsAsync(DacChannelDataSource source);

        void SetDacChannelDataValueA(ChannelAddress channel, ushort value);
        Task SetDacChannelDataValueAAsync(ChannelAddress channel, ushort value);

        void SetDacChannelDataValueB(ChannelAddress channel, ushort value);
        Task SetDacChannelDataValueBAsync(ChannelAddress channel, ushort value);

        void SetDacChannelOffset(ChannelAddress channel, ushort value);
        Task SetDacChannelOffsetAsync(ChannelAddress channel, ushort value);

        void SetDacChannelGain(ChannelAddress channel, ushort value);
        Task SetDacChannelGainAsync(ChannelAddress channel, ushort value);

        void PerformSoftPowerDown();
        Task PerformSoftPowerDownAsync();

        void PerformSoftPowerUp();
        Task PerformSoftPowerUpAsync();

        void SetLDACPinHigh();
        Task SetLDACPinHighAsync();

        void SetLDACPinLow();
        Task SetLDACPinLowAsync();

        void PulseLDACPin();
        Task PulseLDACPinAsync();

        void SuspendAllDacOutputs();
        Task SuspendAllDacOutputsAsync();

        void Reset();
        Task ResetAsync();

        void ResumeAllDacOutputs();
        Task ResumeAllDacOutputsAsync();

        void UpdateAllDacOutputs();
        Task UpdateAllDacOutputsAsync();

        long UploadFirmware(IhxFile ihxFile);
        Task<long> UploadFirmwareAsync(IhxFile ihxFile);
    }
}