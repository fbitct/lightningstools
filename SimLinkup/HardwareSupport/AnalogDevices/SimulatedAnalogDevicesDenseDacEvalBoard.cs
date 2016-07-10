using AnalogDevices;
using LibUsbDotNet;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.AnalogDevices
{
    internal class SimulatedAnalogDevicesDenseDacEvalBoard : IDenseDacEvalBoard
    {

        public DacPrecision DacPrecision { get; set; }
        public UsbDevice UsbDevice { get; internal set; }
        public string SymbolicName { get; internal set; }


        private ChannelMonitorOptions _channelMonitorOptions = new ChannelMonitorOptions(channelMonitorSource: ChannelMonitorSource.None, channelNumberOrInputPinNumber: 0);
        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get
            {
                Task.Run(()=>SimulateSendingUSBCommandAsync()).Wait();
                return _channelMonitorOptions;
            }
            set { Task.Run(()=>SetChannelMonitorOptionsAsync(value)).Wait(); }
        }
        public async Task SetChannelMonitorOptionsAsync(ChannelMonitorOptions value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _channelMonitorOptions = value;
        }



        private IODirection _generalPurposeIOPinDirection = IODirection.Output;
        public IODirection GeneralPurposeIOPinDirection
        {
            get { return Task.Run(()=>GetGeneralPurposeIOPinDirectionAsync()).Result; }
            set { Task.Run(()=>SetGeneralPurposeIOPinDirectionAsync(value)).Wait(); }
        }
        public async Task<IODirection> GetGeneralPurposeIOPinDirectionAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _generalPurposeIOPinDirection;
        }
        public async Task SetGeneralPurposeIOPinDirectionAsync(IODirection value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _generalPurposeIOPinDirection=value;
        }



        private bool _generalPurposeIOPinState = false;
        public bool GeneralPurposeIOPinState
        {
            get { return Task.Run(() => GetGeneralPurposeIOPinStateAsync()).Result; }
            set { Task.Run(()=>SetGeneralPurposeIOPinStateAsync(value)).Wait(); }
        }
        public async Task<bool> GetGeneralPurposeIOPinStateAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _generalPurposeIOPinState;
        }
        public async Task SetGeneralPurposeIOPinStateAsync(bool value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _generalPurposeIOPinState=value;
        }





        private bool _isOverTemperature = false;
        public bool IsOverTemperature
        {
            get { return Task.Run(() => GetIsOverTemperatureAsync()).Result; }
            internal set
            {
                _isOverTemperature = value;
            }
        }
        public async Task<bool> GetIsOverTemperatureAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _isOverTemperature;
        }



        private bool _isThermalShutdownEnabled = false;
        public bool IsThermalShutdownEnabled
        {
            get { return Task.Run(() => GetIsThermalShutdownEnabledAsync()).Result; }
            set { Task.Run(()=>SetIsThermalShutdownEnabledAsync(value)).Wait(); }
        }
        public async Task<bool> GetIsThermalShutdownEnabledAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _isThermalShutdownEnabled;
        }
        public async Task SetIsThermalShutdownEnabledAsync(bool value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _isThermalShutdownEnabled = value;
        }
        public void DisableThermalShutdown()
        {
            Task.Run(()=>SetIsThermalShutdownEnabledAsync(false).ConfigureAwait(false)).Wait();
        }
        public async Task DisableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(false).ConfigureAwait(false);
        }
        public void EnableThermalShutdown()
        {
            Task.Run(()=>SetIsThermalShutdownEnabledAsync(true).ConfigureAwait(false)).Wait();
        }
        public async Task EnableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(true).ConfigureAwait(false);
        }



        private ushort _offsetDAC0 = 1555;
        public ushort OffsetDAC0
        {
            get { return Task.Run(() => GetOffsetDAC0Async()).Result; }
            set { Task.Run(()=>SetOffsetDAC0Async(value)).Wait(); }
        }
        public async Task<ushort> GetOffsetDAC0Async()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _offsetDAC0;
        }
        public async Task SetOffsetDAC0Async(ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _offsetDAC0 = value;
        }



        private ushort _offsetDAC1 = 1555;
        public ushort OffsetDAC1
        {
            get { return Task.Run(() => GetOffsetDAC1Async()).Result; }
            set { Task.Run(()=>SetOffsetDAC1Async(value)).Wait(); }
        }
        public async Task<ushort> GetOffsetDAC1Async()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _offsetDAC1;
        }
        public async Task SetOffsetDAC1Async(ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _offsetDAC1 = value;
        }


        private ushort _offsetDAC2 = 1555;
        public ushort OffsetDAC2
        {
            get { return Task.Run(() => GetOffsetDAC2Async()).Result; }
            set { Task.Run(()=>SetOffsetDAC2Async(value)).Wait(); }
        }
        public async Task<ushort> GetOffsetDAC2Async()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _offsetDAC2;
        }
        public async Task SetOffsetDAC2Async(ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _offsetDAC2 = value;
        }


        private bool _packetErrorCheckErrorOccurred = false;
        public bool PacketErrorCheckErrorOccurred
        {
            get { return Task.Run(() => GetPacketErrorCheckErrorOccurredAsync()).Result; }
            internal set { _packetErrorCheckErrorOccurred = value; }
        }
        public async Task<bool> GetPacketErrorCheckErrorOccurredAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _packetErrorCheckErrorOccurred;
        }


        private DacChannelDataSource[] _dacChannelDataSource = new DacChannelDataSource[40];
        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel)
        {
            return Task.Run(() => GetDacChannelDataSourceAsync(channel)).Result;
        }
        public async Task<DacChannelDataSource> GetDacChannelDataSourceAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _dacChannelDataSource[(int)channel - 8];
        }
        public void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value)
        {
            Task.Run(()=>SetDacChannelDataSourceAsync(channel, value).ConfigureAwait(false)).Wait();
        }
        public async Task SetDacChannelDataSourceAsync(ChannelAddress channel, DacChannelDataSource value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _dacChannelDataSource[(int)channel - 8]=value;
        }


        public void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            Task.Run(()=>SetDacChannelDataSourceAsync(group, channel0, channel1, channel2, channel3, channel4, channel5, channel6, channel7).ConfigureAwait(false)).Wait();    
        }
        public async Task SetDacChannelDataSourceAsync(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            for (int i = 0; i < 8; i++)
            {
                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8)] = channel0;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 1] = channel1;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 2] = channel2;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 3] = channel3;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 4] = channel4;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 5] = channel5;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 6] = channel6;

                await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 7] = channel7;
            }
        }


        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            Task.Run(()=>SetDacChannelDataSourceAllChannelsAsync(source).ConfigureAwait(false)).Wait();
        }
        public async Task SetDacChannelDataSourceAllChannelsAsync(DacChannelDataSource source)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataSource[i] = source;
            }
        }



        private ushort[] _dacChannelDataValueA = new ushort[40];
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            return Task.Run(() => GetDacChannelDataValueAAsync(channel)).Result;
        }
        public void SetDacChannelDataValueA(ChannelAddress channel, ushort value)
        {
            Task.Run(()=>SetDacChannelDataValueAAsync(channel, value).ConfigureAwait(false)).Wait();
        }
        public async Task<ushort> GetDacChannelDataValueAAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _dacChannelDataValueA[(int)channel - 8];
        }
        public async Task SetDacChannelDataValueAAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _dacChannelDataValueA[(int)channel - 8]=value;
        }


        private ushort[] _dacChannelDataValueB = new ushort[40];
        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            return Task.Run(() => GetDacChannelDataValueBAsync(channel)).Result;
        }
        public void SetDacChannelDataValueB(ChannelAddress channel, ushort value)
        {
            Task.Run(()=>SetDacChannelDataValueBAsync(channel, value).ConfigureAwait(false)).Wait();
        }
        public async Task<ushort> GetDacChannelDataValueBAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _dacChannelDataValueB[(int)channel - 8];
        }
        public async Task SetDacChannelDataValueBAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _dacChannelDataValueB[(int)channel - 8] = value;
        }


        private ushort[] _dacChannelOffset = new ushort[40];
        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            return Task.Run(() => GetDacChannelOffsetAsync(channel)).Result;
        }
        public async Task<ushort> GetDacChannelOffsetAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _dacChannelOffset[(int)channel - 8];
        }
        public void SetDacChannelOffset(ChannelAddress channel, ushort value)
        {
            Task.Run(()=>SetDacChannelOffsetAsync(channel, value).ConfigureAwait(false)).Wait();
        }
        public async Task SetDacChannelOffsetAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _dacChannelOffset[(int)channel - 8]=value;
        }




        private ushort[] _dacChannelGain = new ushort[40];
        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            return Task.Run(() => GetDacChannelGainAsync(channel)).Result;
        }
        public async Task<ushort> GetDacChannelGainAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            return _dacChannelGain[(int)channel - 8];
        }
        public void SetDacChannelGain(ChannelAddress channel, ushort value)
        {
            Task.Run(()=>SetDacChannelGainAsync(channel, value).ConfigureAwait(false)).Wait();
        }
        public async Task SetDacChannelGainAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
            _dacChannelGain[(int)channel - 8]=value;
        }



        public void PerformSoftPowerDown()
        {
            Task.Run(()=>PerformSoftPowerDownAsync().ConfigureAwait(false)).Wait();
        }
        public async Task PerformSoftPowerDownAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }


        public void PerformSoftPowerUp()
        {
            Task.Run(()=>PerformSoftPowerUpAsync().ConfigureAwait(false)).Wait();
        }
        public async Task PerformSoftPowerUpAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }

        public void PulseLDACPin()
        {
            Task.Run(()=>PulseLDACPinAsync().ConfigureAwait(false)).Wait();
        }
        public async Task PulseLDACPinAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }


        public void Reset()
        {
            Task.Run(()=>ResetAsync().ConfigureAwait(false)).Wait();
        }
        public async Task ResetAsync()
        {
            await Task.Delay(1000).ConfigureAwait(false);
        }

        public void ResumeAllDacOutputs()
        {
            Task.Run(()=>ResumeAllDacOutputsAsync().ConfigureAwait(false)).Wait();
        }
        public async Task ResumeAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }

        public void SetLDACPinHigh()
        {
            Task.Run(()=>SetLDACPinHighAsync().ConfigureAwait(false)).Wait();
        }
        public async Task SetLDACPinHighAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }

        public void SetLDACPinLow()
        {
            Task.Run(()=>SetLDACPinLowAsync().ConfigureAwait(false)).Wait();
        }
        public async Task SetLDACPinLowAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }


        public void SuspendAllDacOutputs()
        {
            Task.Run(()=>SuspendAllDacOutputsAsync().ConfigureAwait(false)).Wait();
        }
        public async Task SuspendAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }


        public void UpdateAllDacOutputs()
        {
            Task.Run(()=>UpdateAllDacOutputsAsync().ConfigureAwait(false)).Wait();
        }
        public async Task UpdateAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsync().ConfigureAwait(false);
        }


        public long UploadFirmware(IhxFile ihxFile)
        {
            return Task.Run(() => UploadFirmwareAsync(ihxFile)).Result;
        }
        public async Task<long> UploadFirmwareAsync(IhxFile ihxFile)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            return 0;
        }


        private async Task SimulateSendingUSBCommandAsync()
        {
            await Task.Delay(1).ConfigureAwait(false);
        }
    }
}
