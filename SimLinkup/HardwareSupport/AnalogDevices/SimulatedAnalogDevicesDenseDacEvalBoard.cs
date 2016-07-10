using AnalogDevices;
using LibUsbDotNet;
using System.Threading;
using System;
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
                SimulateSendingUSBCommandAsynchronously().RunSynchronously();
                return _channelMonitorOptions;
            }
            set { SetChannelMonitorOptionsAsync(value).RunSynchronously(); }
        }
        public async Task SetChannelMonitorOptionsAsync(ChannelMonitorOptions value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _channelMonitorOptions = value;
        }



        private IODirection _generalPurposeIOPinDirection = IODirection.Output;
        public IODirection GeneralPurposeIOPinDirection
        {
            get { return GetGeneralPurposeIOPinDirectionAsync().Result; }
            set { SetGeneralPurposeIOPinDirectionAsync(value).RunSynchronously(); }
        }
        public async Task<IODirection> GetGeneralPurposeIOPinDirectionAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _generalPurposeIOPinDirection;
        }
        public async Task SetGeneralPurposeIOPinDirectionAsync(IODirection value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _generalPurposeIOPinDirection=value;
        }



        private bool _generalPurposeIOPinState = false;
        public bool GeneralPurposeIOPinState
        {
            get { return GetGeneralPurposeIOPinStateAsync().Result; }
            set { SetGeneralPurposeIOPinStateAsync(value).RunSynchronously(); }
        }
        public async Task<bool> GetGeneralPurposeIOPinStateAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _generalPurposeIOPinState;
        }
        public async Task SetGeneralPurposeIOPinStateAsync(bool value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _generalPurposeIOPinState=value;
        }





        private bool _isOverTemperature = false;
        public bool IsOverTemperature
        {
            get { return GetIsOverTemperatureAsync().Result; }
            internal set
            {
                _isOverTemperature = value;
            }
        }
        public async Task<bool> GetIsOverTemperatureAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _isOverTemperature;
        }



        private bool _isThermalShutdownEnabled = false;
        public bool IsThermalShutdownEnabled
        {
            get { return GetIsThermalShutdownEnabledAsync().Result; }
            set { SetIsThermalShutdownEnabledAsync(value).RunSynchronously(); }
        }
        public async Task<bool> GetIsThermalShutdownEnabledAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _isThermalShutdownEnabled;
        }
        public async Task SetIsThermalShutdownEnabledAsync(bool value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _isThermalShutdownEnabled = value;
        }
        public void DisableThermalShutdown()
        {
            SetIsThermalShutdownEnabledAsync(false).RunSynchronously();
        }
        public async Task DisableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(false).ConfigureAwait(false);
        }
        public void EnableThermalShutdown()
        {
            SetIsThermalShutdownEnabledAsync(true).RunSynchronously();
        }
        public async Task EnableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(true).ConfigureAwait(false);
        }



        private ushort _offsetDAC0 = 1555;
        public ushort OffsetDAC0
        {
            get { return GetOffsetDAC0Async().Result; }
            set { SetOffsetDAC0Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC0Async()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _offsetDAC0;
        }
        public async Task SetOffsetDAC0Async(ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _offsetDAC0 = value;
        }



        private ushort _offsetDAC1 = 1555;
        public ushort OffsetDAC1
        {
            get { return GetOffsetDAC1Async().Result; }
            set { SetOffsetDAC1Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC1Async()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _offsetDAC1;
        }
        public async Task SetOffsetDAC1Async(ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _offsetDAC1 = value;
        }


        private ushort _offsetDAC2 = 1555;
        public ushort OffsetDAC2
        {
            get { return GetOffsetDAC2Async().Result; }
            set { SetOffsetDAC2Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC2Async()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _offsetDAC2;
        }
        public async Task SetOffsetDAC2Async(ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _offsetDAC2 = value;
        }


        private bool _packetErrorCheckErrorOccurred = false;
        public bool PacketErrorCheckErrorOccurred
        {
            get { return GetPacketErrorCheckErrorOccurredAsync().Result; }
            internal set { _packetErrorCheckErrorOccurred = value; }
        }
        public async Task<bool> GetPacketErrorCheckErrorOccurredAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _packetErrorCheckErrorOccurred;
        }


        private DacChannelDataSource[] _dacChannelDataSource = new DacChannelDataSource[40];
        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel)
        {
            return GetDacChannelDataSourceAsync(channel).Result;
        }
        public async Task<DacChannelDataSource> GetDacChannelDataSourceAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _dacChannelDataSource[(int)channel - 8];
        }
        public void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value)
        {
            SetDacChannelDataSourceAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAsync(ChannelAddress channel, DacChannelDataSource value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _dacChannelDataSource[(int)channel - 8]=value;
        }


        public void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            SetDacChannelDataSourceAsync(group, channel0, channel1, channel2, channel3, channel4, channel5, channel6, channel7).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAsync(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            for (int i = 0; i < 8; i++)
            {
                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8)] = channel0;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 1] = channel1;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 2] = channel2;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 3] = channel3;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 4] = channel4;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 5] = channel5;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 6] = channel6;

                await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
                _dacChannelDataSource[((int)group * 8) + 7] = channel7;
            }
        }


        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            SetDacChannelDataSourceAllChannelsAsync(source).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAllChannelsAsync(DacChannelDataSource source)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataSource[i] = source;
            }
        }



        private ushort[] _dacChannelDataValueA = new ushort[40];
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            return GetDacChannelDataValueAAsync(channel).Result;
        }
        public void SetDacChannelDataValueA(ChannelAddress channel, ushort value)
        {
            SetDacChannelDataValueAAsync(channel, value).RunSynchronously();
        }
        public async Task<ushort> GetDacChannelDataValueAAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _dacChannelDataValueA[(int)channel - 8];
        }
        public async Task SetDacChannelDataValueAAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _dacChannelDataValueA[(int)channel - 8]=value;
        }


        private ushort[] _dacChannelDataValueB = new ushort[40];
        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            return GetDacChannelDataValueBAsync(channel).Result;
        }
        public void SetDacChannelDataValueB(ChannelAddress channel, ushort value)
        {
            SetDacChannelDataValueBAsync(channel, value).RunSynchronously();
        }
        public async Task<ushort> GetDacChannelDataValueBAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _dacChannelDataValueB[(int)channel - 8];
        }
        public async Task SetDacChannelDataValueBAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _dacChannelDataValueB[(int)channel - 8] = value;
        }


        private ushort[] _dacChannelOffset = new ushort[40];
        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            return GetDacChannelOffsetAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelOffsetAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _dacChannelOffset[(int)channel - 8];
        }
        public void SetDacChannelOffset(ChannelAddress channel, ushort value)
        {
            SetDacChannelOffsetAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelOffsetAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _dacChannelOffset[(int)channel - 8]=value;
        }




        private ushort[] _dacChannelGain = new ushort[40];
        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            return GetDacChannelGainAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelGainAsync(ChannelAddress channel)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            return _dacChannelGain[(int)channel - 8];
        }
        public void SetDacChannelGain(ChannelAddress channel, ushort value)
        {
            SetDacChannelGainAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelGainAsync(ChannelAddress channel, ushort value)
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
            _dacChannelGain[(int)channel - 8]=value;
        }



        public void PerformSoftPowerDown()
        {
            PerformSoftPowerDownAsync().RunSynchronously();
        }
        public async Task PerformSoftPowerDownAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }


        public void PerformSoftPowerUp()
        {
            PerformSoftPowerUpAsync().RunSynchronously();
        }
        public async Task PerformSoftPowerUpAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }

        public void PulseLDACPin()
        {
            PulseLDACPinAsync().RunSynchronously();
        }
        public async Task PulseLDACPinAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }


        public void Reset()
        {
            ResetAsync().RunSynchronously();
        }
        public async Task ResetAsync()
        {
            await Task.Delay(1000).ConfigureAwait(false);
        }

        public void ResumeAllDacOutputs()
        {
            ResumeAllDacOutputsAsync().RunSynchronously();
        }
        public async Task ResumeAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }

        public void SetLDACPinHigh()
        {
            SetLDACPinHighAsync().RunSynchronously();
        }
        public async Task SetLDACPinHighAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }

        public void SetLDACPinLow()
        {
            SetLDACPinLowAsync().RunSynchronously();
        }
        public async Task SetLDACPinLowAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }


        public void SuspendAllDacOutputs()
        {
            SuspendAllDacOutputsAsync().RunSynchronously();
        }
        public async Task SuspendAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }


        public void UpdateAllDacOutputs()
        {
            UpdateAllDacOutputsAsync().RunSynchronously();
        }
        public async Task UpdateAllDacOutputsAsync()
        {
            await SimulateSendingUSBCommandAsynchronously().ConfigureAwait(false);
        }


        public long UploadFirmware(IhxFile ihxFile)
        {
            return UploadFirmwareAsync(ihxFile).Result;
        }
        public async Task<long> UploadFirmwareAsync(IhxFile ihxFile)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            return 0;
        }


        private async Task SimulateSendingUSBCommandAsynchronously()
        {
            await Task.Delay(1).ConfigureAwait(false);
        }

    }
}
