using System;
using System.Collections.Generic;
using System.ComponentModel;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Linq;
using System.Threading.Tasks;

namespace AnalogDevices
{
    public class DenseDacEvalBoard : IDenseDacEvalBoard
    {
        #region Public stuff

        #region Class Constructors

        public DenseDacEvalBoard(UsbDevice device)
        {
            _usbDevice = device;
            UploadFirmware(new IhxFile("AD5371SPI.hex"));
        }

        #endregion

        #region Public Properties

        public UsbDevice UsbDevice
        {
            get { return _usbDevice; }
        }

        public string SymbolicName
        {
            get
            {
                return _usbDevice != null &&
                        _usbDevice.UsbRegistryInfo != null
                            ? _usbDevice.UsbRegistryInfo.SymbolicName
                            : null;
            }
        }

        public bool GeneralPurposeIOPinState
        {
            get { return GetGeneralPurposeIOPinStateAsync().Result; }
            set { SetGeneralPurposeIOPinStateAsync(value).RunSynchronously(); }
        }

        public async Task<bool> GetGeneralPurposeIOPinStateAsync()
        {
            var gpioPinDirection = await GetGeneralPurposeIOPinDirectionAsync().ConfigureAwait(false);
            if (gpioPinDirection != IODirection.Input)
            {
                throw new InvalidOperationException(
                    "GeneralPurposeIOPinDirection must be set to Input in order to read the GeneralPurposeIOPinState property.");
            }
            var gpioRegister = await ReadbackGPIORegisterAsync().ConfigureAwait(false);////readback GPIO register 
            return (gpioRegister & GpioRegisterBits.Data) == GpioRegisterBits.Data; //return bit F0
        }

        public async Task SetGeneralPurposeIOPinStateAsync(bool value)
        {
            var gpioPinDirection = await GetGeneralPurposeIOPinDirectionAsync().ConfigureAwait(false);
            if (gpioPinDirection != IODirection.Output)
            {
                throw new InvalidOperationException(
                    "GeneralPurposeIOPinDirection must be set to Output in order to set the GeneralPurposeIOPinState property.");
            }
            await SendSpecialFunctionAsync(SpecialFunctionCode.GPIOConfigureAndWrite,
                value
                    ? (ushort)(GpioRegisterBits.Direction | GpioRegisterBits.Data) //sets bit F1=1 to configure GPIO as an output; sets bit F0=1 to write a 1 to the GPIO output 
                    : (ushort)(GpioRegisterBits.Direction) //sets bit F1=1 to configure GPIO as an output, bit F0 will not be set so will be written as a 0 to the GPIO output
            ).ConfigureAwait(false);
        }

        public IODirection GeneralPurposeIOPinDirection
        {
            get { return GetGeneralPurposeIOPinDirectionAsync().Result; }
            set { SetGeneralPurposeIOPinDirectionAsync(value).RunSynchronously(); }
        }

        public async Task<IODirection> GetGeneralPurposeIOPinDirectionAsync()
        {
            var gpioRegisterVal = await ReadbackGPIORegisterAsync().ConfigureAwait(false);
            return ((gpioRegisterVal & GpioRegisterBits.Direction) == GpioRegisterBits.Direction)//if bit F1 =1, GPIO pin is configured for output (else, is configured for input)
                        ? IODirection.Output
                        : IODirection.Input;
        }

        public async Task SetGeneralPurposeIOPinDirectionAsync(IODirection value)
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.GPIOConfigureAndWrite,
                                value == IODirection.Output
                                    ? (ushort)GpioRegisterBits.Direction
                                    : (ushort)BasicMasks.AllBitsZero).ConfigureAwait(false);
        }

        public ushort OffsetDAC0
        {
            get { return GetOffsetDAC0Async().Result; }
            set { SetOffsetDAC0Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC0Async()
        {
            return await ReadbackOFS0RegisterAsync().ConfigureAwait(false);
        }
        public async Task SetOffsetDAC0Async(ushort value)
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
            await WriteOFS0RegisterAsync(value).ConfigureAwait(false);
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }

        public ushort OffsetDAC1
        {
            get { return GetOffsetDAC1Async().Result; }
            set { SetOffsetDAC1Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC1Async()
        {
            return await ReadbackOFS1RegisterAsync().ConfigureAwait(false);
        }
        public async Task SetOffsetDAC1Async(ushort value)
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
            await WriteOFS1RegisterAsync(value).ConfigureAwait(false);
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }

        public ushort OffsetDAC2
        {
            get { return GetOffsetDAC2Async().Result; }
            set { SetOffsetDAC2Async(value).RunSynchronously(); }
        }
        public async Task<ushort> GetOffsetDAC2Async()
        {
            return await ReadbackOFS2RegisterAsync().ConfigureAwait(false);
        }
        public async Task SetOffsetDAC2Async(ushort value)
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
            await WriteOFS2RegisterAsync(value).ConfigureAwait(false);
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }


        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get { return GetChannelMonitorOptions(); }
            set { SetChannelMonitorOptionsAsync(value).RunSynchronously(); }
        }

        private ChannelMonitorOptions GetChannelMonitorOptions()
        {
            var source = ChannelMonitorSource.None;
            byte channelNumberOrInputPinNumber = 0;
            if ((_monitorFlags & MonitorBits.MonitorEnable) == MonitorBits.MonitorEnable) //if bit F5 is set (monitor enable bit = 1), monitoring is enabled
            {
                if ((_monitorFlags & MonitorBits.SourceSelect) == MonitorBits.SourceSelect) //if bit F4 =1, input pin monitoring is selected
                {
                    source = ChannelMonitorSource.InputPin;
                    channelNumberOrInputPinNumber = (byte)(_monitorFlags & MonitorBits.InputPin); //if bit F0=0, MON_IN0 is selected for monitoring, else MON_IN1 is selected for monitoring
                }
                else //bit F4 =0; DAC channel monitoring is selected
                {
                    source = ChannelMonitorSource.DacChannel;
                    channelNumberOrInputPinNumber = (byte)(_monitorFlags & MonitorBits.DacChannel); //bits F0-F3 specify the monitored DAC channel
                }
            }

            var toReturn = new ChannelMonitorOptions(source, channelNumberOrInputPinNumber);
            toReturn.PropertyChanged += MonitorOptionsPropertyChangedHandler;
            return toReturn;
        }
        public async Task SetChannelMonitorOptionsAsync(ChannelMonitorOptions value)
        {
            await SendNewChannelMonitorOptionsToDeviceAsync(value).ConfigureAwait(false);
        }


        public bool IsThermalShutdownEnabled
        {
            get { return GetIsThermalShutdownEnabledAsync().Result; }
            set { SetIsThermalShutdownEnabledAsync(value).RunSynchronously();}
        }

        public void EnableThermalShutdown()
        {
            EnableThermalShutdownAsync().RunSynchronously();
        }
        public async Task EnableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(true).ConfigureAwait(false);
        }
        public void DisableThermalShutdown()
        {
            DisableThermalShutdownAsync().RunSynchronously();
        }
        public async Task DisableThermalShutdownAsync()
        {
            await SetIsThermalShutdownEnabledAsync(false).ConfigureAwait(false);
        }
        public async Task<bool> GetIsThermalShutdownEnabledAsync()
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            return ((controlRegisterBits & ControlRegisterBits.ThermalShutdownEnabled) == ControlRegisterBits.ThermalShutdownEnabled); //if bit 1=1, thermal shutdown is enabled
        }
        public async Task SetIsThermalShutdownEnabledAsync(bool value)
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            if (value)
            {
                controlRegisterBits |= ControlRegisterBits.ThermalShutdownEnabled;
            }
            else
            {
                controlRegisterBits &= ~ControlRegisterBits.ThermalShutdownEnabled;
            }
            await WriteControlRegisterAsync(controlRegisterBits).ConfigureAwait(false);
        }


        public DacPrecision DacPrecision
        {
            get { return _thisDevicePrecision; }
            set { _thisDevicePrecision = value; }
        }

        public bool PacketErrorCheckErrorOccurred
        {
            get { return GetPacketErrorCheckErrorOccurredAsync().Result; }
        }

        public async Task<bool> GetPacketErrorCheckErrorOccurredAsync()
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            return (controlRegisterBits & ControlRegisterBits.PacketErrorCheckErrorOccurred) == ControlRegisterBits.PacketErrorCheckErrorOccurred;
        }

        public bool IsOverTemperature
        {
            get
            {
                return GetIsOverTemperatureAsync().Result;
            }
        }

        public async Task<bool> GetIsOverTemperatureAsync()
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            return (controlRegisterBits & ControlRegisterBits.OverTemperature) == ControlRegisterBits.OverTemperature;
        }

        #endregion

        #region Dac Channel Data Source Selection
        public void SetDacChannelDataSource(ChannelAddress channelAddress, DacChannelDataSource value)
        {
            SetDacChannelDataSourceAsync(channelAddress, value).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAsync(ChannelAddress channelAddress, DacChannelDataSource value)
        {
            if ((int)channelAddress < 8 || (int)channelAddress > 47)
            {
                if (channelAddress == ChannelAddress.AllGroupsAllChannels)
                {
                    await SetDacChannelDataSourceAllChannelsAsync(value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0AllChannels)
                {
                    await SetDacChannelDataSourceAsync(ChannelGroup.Group0, value, value, value, value, value, value, value, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1AllChannels)
                {
                    await SetDacChannelDataSourceAsync(ChannelGroup.Group1, value, value, value, value, value, value, value, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group2AllChannels)
                {
                    await SetDacChannelDataSourceAsync(ChannelGroup.Group2, value, value, value, value, value, value, value, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group3AllChannels)
                {
                    await SetDacChannelDataSourceAsync(ChannelGroup.Group3, value, value, value, value, value, value, value, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group4AllChannels)
                {
                    await SetDacChannelDataSourceAsync(ChannelGroup.Group4, value, value, value, value, value, value, value, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel0)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel0, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel1)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel1, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel2)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel2, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel3)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel3, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel4)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel4, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel5)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel5, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel6)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel6, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel7)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group0Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel7, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel0)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel0, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel0, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel1)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel1, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel1, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel2)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel2, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel2, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel3)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel3, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel3, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel4)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel4, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel4, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel5)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel5, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel5, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel6)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel6, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel6, value).ConfigureAwait(false);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel7)
                {
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group1Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group2Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group3Channel7, value).ConfigureAwait(false);
                    await SetDacChannelDataSourceAsync(ChannelAddress.Group4Channel7, value).ConfigureAwait(false);
                    return;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("channelAddress");
                }
            }
            var channelNum = (byte)((byte)channelAddress - 8);
            var currentSourceSelections = ABSelectRegisterBits.AllChannelsA;

            var specialFunctionCode = SpecialFunctionCode.NOP;
            if (channelNum < 8)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister0;
                currentSourceSelections = await ReadbackABSelect0RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 8 && channelNum < 16)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister1;
                currentSourceSelections = await ReadbackABSelect1RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 16 && channelNum < 24)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister2;
                currentSourceSelections = await ReadbackABSelect2RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 24 && channelNum < 32)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister3;
                currentSourceSelections = await ReadbackABSelect3RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 32 && channelNum <= 39)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister4;
                currentSourceSelections = await ReadbackABSelect4RegisterAsync().ConfigureAwait(false);
            }
            var newSourceSelections = currentSourceSelections;

            var channelOffset = (byte)(channelNum % 8);
            var channelMask = (ABSelectRegisterBits)(1 << channelOffset);

            if (value == DacChannelDataSource.DataValueA)
            {
                newSourceSelections &= ~channelMask;
            }
            else
            {
                newSourceSelections |= channelMask;
            }
            newSourceSelections &= ABSelectRegisterBits.WritableBits; //ensure we only send 8 bits of data (one for each channel in the group)
            await SendSpecialFunctionAsync(specialFunctionCode, (ushort)newSourceSelections).ConfigureAwait(false);
        }
        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel)
        {
            return GetDacChannelDataSourceAsync(channel).Result;
        }
        public async Task<DacChannelDataSource> GetDacChannelDataSourceAsync(ChannelAddress channel)
        {
            if ((int)channel < 8 || (int)channel > 47)
            {
                throw new ArgumentOutOfRangeException("channelAddress");
            }
            var channelNum = (byte)((byte)channel - 8);
            var currentSourceSelections = ABSelectRegisterBits.AllChannelsA;
            if (channelNum < 8)
            {
                currentSourceSelections = await ReadbackABSelect0RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 8 && channelNum < 16)
            {
                currentSourceSelections = await ReadbackABSelect1RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 16 && channelNum < 24)
            {
                currentSourceSelections = await ReadbackABSelect2RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 24 && channelNum < 32)
            {
                currentSourceSelections = await ReadbackABSelect3RegisterAsync().ConfigureAwait(false);
            }
            else if (channelNum >= 32 && channelNum <= 39)
            {
                currentSourceSelections = await ReadbackABSelect4RegisterAsync().ConfigureAwait(false);
            }
            var channelOffset = (byte)(channelNum % 8);
            var channelMask = (ABSelectRegisterBits)(1 << channelOffset);
            var sourceIsDataValueB = ((currentSourceSelections & channelMask) == channelMask);
            if (sourceIsDataValueB)
            {
                return DacChannelDataSource.DataValueB;
            }
            else
            {
                return DacChannelDataSource.DataValueA;
            }
        }
        public void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0,
                                            DacChannelDataSource channel1, DacChannelDataSource channel2,
                                            DacChannelDataSource channel3, DacChannelDataSource channel4,
                                            DacChannelDataSource channel5, DacChannelDataSource channel6,
                                            DacChannelDataSource channel7)
        {
            SetDacChannelDataSourceAsync(group, channel0, channel1, channel2, channel3, channel4, channel5, channel6, channel7).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAsync(ChannelGroup group, DacChannelDataSource channel0,
                                            DacChannelDataSource channel1, DacChannelDataSource channel2,
                                            DacChannelDataSource channel3, DacChannelDataSource channel4,
                                            DacChannelDataSource channel5, DacChannelDataSource channel6,
                                            DacChannelDataSource channel7)
        {
            ABSelectRegisterBits abSelectRegisterBits = ABSelectRegisterBits.AllChannelsA;

            if (channel0 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel0;
            if (channel1 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel1;
            if (channel2 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel2;
            if (channel3 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel3;
            if (channel4 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel4;
            if (channel5 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel5;
            if (channel6 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel6;
            if (channel7 == DacChannelDataSource.DataValueB) abSelectRegisterBits |= ABSelectRegisterBits.Channel7;

            var specialFunctionCode = SpecialFunctionCode.NOP;
            switch (group)
            {
                case ChannelGroup.Group0:
                    specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister0;
                    break;
                case ChannelGroup.Group1:
                    specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister1;
                    break;
                case ChannelGroup.Group2:
                    specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister2;
                    break;
                case ChannelGroup.Group3:
                    specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister3;
                    break;
                case ChannelGroup.Group4:
                    specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister4;
                    break;
            }
            abSelectRegisterBits &= ABSelectRegisterBits.WritableBits; //ensure we only send 8 bits of data
            await SendSpecialFunctionAsync(specialFunctionCode, (ushort)abSelectRegisterBits).ConfigureAwait(false);
        }
        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            SetDacChannelDataSourceAllChannelsAsync(source).RunSynchronously();
        }
        public async Task SetDacChannelDataSourceAllChannelsAsync(DacChannelDataSource source)
        {
            switch (source)
            {
                case DacChannelDataSource.DataValueA:
                    await SendSpecialFunctionAsync(SpecialFunctionCode.BlockWriteABSelectRegisters, (ushort)ABSelectRegisterBits.AllChannelsA).ConfigureAwait(false);
                    break;
                case DacChannelDataSource.DataValueB:
                    await SendSpecialFunctionAsync(SpecialFunctionCode.BlockWriteABSelectRegisters, (ushort)ABSelectRegisterBits.AllChannelsB).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentException("source");
            }
        }

        #endregion

        #region Device Control Functions

        public void PerformSoftPowerDown()
        {
            PerformSoftPowerDownAsync().RunSynchronously();
        }
        public async Task PerformSoftPowerDownAsync()
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            controlRegisterBits |= ControlRegisterBits.SoftPowerDown; //set bit F0=1 to perform soft power-down
            await WriteControlRegisterAsync(controlRegisterBits).ConfigureAwait(false);
        }

        public void PerformSoftPowerUp()
        {
            PerformSoftPowerUpAsync().RunSynchronously();
        }
        public async Task PerformSoftPowerUpAsync()
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            controlRegisterBits &= ~ControlRegisterBits.SoftPowerDown; //set bit F0=0 to perform soft power-up;
            await WriteControlRegisterAsync(controlRegisterBits).ConfigureAwait(false);
        }

        public void Reset()
        {
            ResetAsync().RunSynchronously();
        }
        public async Task ResetAsync()
        {
            await SetRESETPinHighAsync().ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            await SetRESETPinLowAsync().ConfigureAwait(false);
        }

        public void SuspendAllDacOutputs()
        {
            SuspendAllDacOutputsAsync().RunSynchronously();
        }
        public async Task SuspendAllDacOutputsAsync()
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
        }

        public void ResumeAllDacOutputs()
        {
            ResumeAllDacOutputsAsync().RunSynchronously();
        }
        public async Task ResumeAllDacOutputsAsync()
        {
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }

        public void UpdateAllDacOutputs()
        {
            UpdateAllDacOutputsAsync().RunSynchronously();
        }
        public async Task UpdateAllDacOutputsAsync()
        {
            await PulseLDACPinAsync().ConfigureAwait(false);
        }

        #endregion

        #region Dac Functions
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            return GetDacChannelDataValueAAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelDataValueAAsync(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return await ReadbackX1ARegisterAsync(channel).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            return GetDacChannelDataValueBAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelDataValueBAsync(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return await ReadbackX1BRegisterAsync(channel).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelDataValueA(ChannelAddress channel, ushort value)
        {
            SetDacChannelDataValueAAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelDataValueAAsync(ChannelAddress channelAddress, ushort value)
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            controlRegisterBits &= ~ControlRegisterBits.InputRegisterSelect; //set control register bit F2 =0 to select register X1A for input
            await WriteControlRegisterAsync(controlRegisterBits).ConfigureAwait(false);

            if (DacPrecision == DacPrecision.SixteenBit)
            {
                await SendSPIAsync((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | value).ConfigureAwait(false);
            }
            else
            {
                await SendSPIAsync((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | (uint) ((value & (uint)BasicMasks.FourteenBits) << 2)).ConfigureAwait(false);
            }
        }


        public void SetDacChannelDataValueB(ChannelAddress channel, ushort value)
        {
            SetDacChannelDataValueBAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelDataValueBAsync(ChannelAddress channelAddress, ushort newVal)
        {
            var controlRegisterBits = await ReadbackControlRegisterAsync().ConfigureAwait(false);
            controlRegisterBits |= ControlRegisterBits.InputRegisterSelect;//set control register bit F2 =1 to select register X1B for input
            await WriteControlRegisterAsync(controlRegisterBits).ConfigureAwait(false);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | newVal)).ConfigureAwait(false);
            }
            else
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | (uint) ((newVal & (uint)BasicMasks.FourteenBits) << 2))).ConfigureAwait(false);
            }
        }

        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            return GetDacChannelOffsetAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelOffsetAsync(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return await ReadbackCRegisterAsync(channel).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelOffset(ChannelAddress channel, ushort value)
        {
            SetDacChannelOffsetAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelOffsetAsync(ChannelAddress channel, ushort value)
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegister | (uint) (((byte) channel & (byte)BasicMasks.SixBits) << 16) | value)).ConfigureAwait(false);
            }
            else
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegister | (uint) (((byte) channel & (byte)BasicMasks.SixBits) << 16) | (uint) ((value & (uint)BasicMasks.FourteenBits) << 2))).ConfigureAwait(false);
            }
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }

        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            return GetDacChannelGainAsync(channel).Result;
        }
        public async Task<ushort> GetDacChannelGainAsync(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return await ReadbackMRegisterAsync(channel).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelGain(ChannelAddress channel, ushort value)
        {
            SetDacChannelGainAsync(channel, value).RunSynchronously();
        }
        public async Task SetDacChannelGainAsync(ChannelAddress channel, ushort value)
        {
            await SetCLRPinLowAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACGainRegister | (uint) (((byte) channel & (byte)BasicMasks.SixBits) << 16) | value)).ConfigureAwait(false);
            }
            else
            {
                await SendSPIAsync(((uint)SerialInterfaceModeBits.WriteToDACGainRegister | (uint) (((byte) channel & (byte)BasicMasks.SixBits) << 16) | (uint) ((value & (uint)BasicMasks.FourteenBits) << 2))).ConfigureAwait(false);
            }
            await SetCLRPinHighAsync().ConfigureAwait(false);
        }

        #endregion

        #region Device Enumeration
        public static IDenseDacEvalBoard[] Enumerate()
        {
            return EnumerateAsync().Result;
        }
        public static async Task<IDenseDacEvalBoard[]> EnumerateAsync()
        {
            var discoveredDevices = new List<string>();
            var toReturn = new List<DenseDacEvalBoard>();
            var devs = await Task.FromResult(UsbDevice.AllDevices).ConfigureAwait(false);
            for (var i = 0; i < devs.Count; i++)
            {
                var device = await Task.FromResult(devs[i].Device).ConfigureAwait(false);
                if (device != null)
                {
                    var registryInfo = await Task.FromResult(device.UsbRegistryInfo).ConfigureAwait(false);
                    if (
                        registryInfo.Vid == 0x0456
                        &&
                        (
                            (ushort)registryInfo.Pid == 0xB20F
                            ||
                            (ushort)registryInfo.Pid == 0xB20E
                        )
                        )
                    {
                        if (!discoveredDevices.Contains(registryInfo.SymbolicName))
                        {
                            var newDevice = await Task.FromResult(new DenseDacEvalBoard(device));
                            toReturn.Add(newDevice);
                        }
                        discoveredDevices.Add(registryInfo.SymbolicName);
                    }
                }
            }
            return toReturn.OrderByDescending(x=>x.UsbDevice.UsbRegistryInfo.SymbolicName).ToArray();
        }

        #endregion

        #endregion

        #region Private stuff

        #region Private Enums

        #region Nested type: DeviceCommand

        #endregion

        #region Nested type: SpecialFunctionCode

        #endregion

        #endregion

        #region Instance Variables

        private readonly byte[] _emptyBuf = new byte[0];
        private readonly UsbDevice _usbDevice;
        private MonitorBits _monitorFlags;
        private bool _spiInitialized;
        private DacPrecision _thisDevicePrecision = DacPrecision.SixteenBit;

        #endregion

        #region Channel Monitoring Options Change Handling

        private async Task SendNewChannelMonitorOptionsToDeviceAsync(ChannelMonitorOptions value)
        {
            if (value == null) throw new ArgumentNullException("value");

            if (value.ChannelMonitorSource == ChannelMonitorSource.None)
            {
                _monitorFlags &= ~MonitorBits.MonitorEnable; //set bit F5 to 0 (set monitor enable bit to 0, monitor disable)
            }
            else
            {
                _monitorFlags |= MonitorBits.MonitorEnable; //set bit F5 to 1 (set monitor enable bit to 1, monitor enable)
                if (value.ChannelMonitorSource == ChannelMonitorSource.InputPin)
                {
                    if (value.ChannelNumberOrInputPinNumber == 0)
                    {
                        _monitorFlags |= MonitorBits.SourceSelect; //set bit F4 to 1 (monitor input pin selected by F0) 
                        _monitorFlags &= ~MonitorBits.InputPin; //set bit F0 to 0 (select MON_IN0 for monitoring)
                        _monitorFlags &= ~MonitorBits.NotUsedWithInputPinMonitoring; //set bits F1-F3 to 0 (not used when F4 =1)
                    }
                    else if (value.ChannelNumberOrInputPinNumber == 1)
                    {
                        _monitorFlags |= MonitorBits.SourceSelect;//set bit F4 to 1 (monitor input pin selected by F0) 
                        _monitorFlags |= MonitorBits.InputPin;//set bit F0 to 1 (select MON_IN1 for monitoring)
                        _monitorFlags &= ~MonitorBits.NotUsedWithInputPinMonitoring; //set bits F1-F3 to 0 (not used when F4 =1)
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value",
                                                              "value.ChannelNumberOrInputPinNumber is out of range with respect to ChannelMonitorSource.");
                    }
                }
                else if (value.ChannelMonitorSource == ChannelMonitorSource.DacChannel)
                {
                    _monitorFlags |= MonitorBits.MonitorEnable;//set bit F5 to 1 (set monitor enable bit to 1, monitor enable)
                    _monitorFlags &= ~MonitorBits.DacChannel; //clear bits F0-F3 (DAC channel selection)
                    _monitorFlags |= (MonitorBits) (value.ChannelNumberOrInputPinNumber & (byte)BasicMasks.FourBits); //set bits F0-F3 with the DAC channel number to monitor
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "value.ChannelMonitorSource is not valid.");
                }
            }
            await SendSpecialFunctionAsync(SpecialFunctionCode.ConfigureMonitoring, (ushort)_monitorFlags).ConfigureAwait(false);
        }

        private async void MonitorOptionsPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await SendNewChannelMonitorOptionsToDeviceAsync((ChannelMonitorOptions) sender).ConfigureAwait(false);
        }

        #endregion

        #region Register Readback Functions

        private async Task<ushort> ReadbackX1ARegisterAsync(ChannelAddress channel)
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((((byte) channel) & (byte)BasicMasks.SixBits) << 7)).ConfigureAwait(false);
            var val = await ReadSPIAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private async Task<ushort> ReadbackX1BRegisterAsync(ChannelAddress channel)
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.X1BRegister) | (ushort) ((((byte) channel) & (byte)BasicMasks.SixBits) << 7))).ConfigureAwait(false);
            var val = await ReadSPIAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private async Task<ushort> ReadbackCRegisterAsync(ChannelAddress channel)
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.CRegister)| (ushort) ((((byte) channel) & (byte)BasicMasks.SixBits) << 7))).ConfigureAwait(false);
            var val = await ReadSPIAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private async Task<ushort> ReadbackMRegisterAsync(ChannelAddress channel)
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.MRegister) | (ushort) ((((byte) channel) & (byte)BasicMasks.SixBits) << 7))).ConfigureAwait(false);
            var val = await ReadSPIAsync().ConfigureAwait(false);
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private async Task<ControlRegisterBits> ReadbackControlRegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ControlRegister).ConfigureAwait(false);
            return (ControlRegisterBits) (await ReadSPIAsync().ConfigureAwait(false) & (ushort)ControlRegisterBits.ReadableBits);
        }

        private async Task<ushort> ReadbackOFS0RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF0Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ushort) (spi & (ushort)BasicMasks.FourteenBits);
        }
        private async Task<ushort> ReadbackOFS1RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF1Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ushort) (spi & (ushort)BasicMasks.FourteenBits);
        }
        private async Task<ushort> ReadbackOFS2RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF2Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ushort) (spi & (ushort)BasicMasks.FourteenBits);
        }

        private async Task<ABSelectRegisterBits> ReadbackABSelect0RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect0Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ABSelectRegisterBits) (spi & (ushort)ABSelectRegisterBits.ReadableBits);
        }
        private async Task<ABSelectRegisterBits> ReadbackABSelect1RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect1Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
        }
        private async Task<ABSelectRegisterBits> ReadbackABSelect2RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect2Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
        }
        private async Task<ABSelectRegisterBits> ReadbackABSelect3RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect3Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
        }
        private async Task<ABSelectRegisterBits> ReadbackABSelect4RegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect4Register).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private async Task<GpioRegisterBits> ReadbackGPIORegisterAsync()
        {
            await SendSpecialFunctionAsync(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.GPIORegister).ConfigureAwait(false);
            var spi = await ReadSPIAsync().ConfigureAwait(false);
            return (GpioRegisterBits)(spi & (ushort)GpioRegisterBits.ReadableBits);
        }

        #endregion

        #region Register Writing Functions

        private async Task WriteControlRegisterAsync(ControlRegisterBits controlRegisterBits)
        {
            controlRegisterBits &= ControlRegisterBits.WritableBits;
            await SendSpecialFunctionAsync(SpecialFunctionCode.WriteControlRegister, (ushort)controlRegisterBits).ConfigureAwait(false);
        }

        private async Task WriteOFS0RegisterAsync(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            await SendSpecialFunctionAsync(SpecialFunctionCode.WriteOSF0Register, value).ConfigureAwait(false);
        }
        private async Task WriteOFS1RegisterAsync(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            await SendSpecialFunctionAsync(SpecialFunctionCode.WriteOSF1Register, value).ConfigureAwait(false);
        }
        private async Task WriteOFS2RegisterAsync(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            await SendSpecialFunctionAsync(SpecialFunctionCode.WriteOSF2Register, value).ConfigureAwait(false);
        }


        #endregion

        #region Pin Manipulation Functions

        private async Task SetRESETPinHighAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetRESETPinHigh, 0).ConfigureAwait(false);
        }
        private async Task SetRESETPinLowAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetRESETPinLow, 0).ConfigureAwait(false);
        }
        private async Task SetCLRPinHighAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetCLRPinHigh, 0);
        }
        private async Task SetCLRPinLowAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetCLRPinLow, 0).ConfigureAwait(false);
        }

        public void PulseLDACPin()
        {
            PulseLDACPinAsync().RunSynchronously();
        }
        public async Task PulseLDACPinAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.PulseLDACPin, 0).ConfigureAwait(false);
        }

        public void SetLDACPinLow()
        {
            SetLDACPinLowAsync().RunSynchronously();
        }
        public async Task SetLDACPinLowAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetLDACPinLow, 0).ConfigureAwait(false);
        }

        public void SetLDACPinHigh()
        {
            SetLDACPinHighAsync().RunSynchronously();
        }
        public async Task SetLDACPinHighAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.SetLDACPinHigh, 0).ConfigureAwait(false);
        }

        private async Task InitializeSPIPinsAsync()
        {
            await SendDeviceCommandAsync(DeviceCommand.InitializeSPIPins, 0).ConfigureAwait(false);
            _spiInitialized = true;
        }

        #endregion

        #region Device Communications
        private async Task SendSpecialFunctionAsync(SpecialFunctionCode specialFunctionCode, ushort data)
        {
            await SendSPIAsync((uint) ((((byte) specialFunctionCode & (byte)BasicMasks.SixBits) << 16) | data)).ConfigureAwait(false);
        }
        private async Task<int> SendSPIAsync(uint data)
        {
            return await SendDeviceCommandAsync(DeviceCommand.SendSPI, data).ConfigureAwait(false);
        }
        private async Task<ushort> ReadSPIAsync()
        {
            if (!_spiInitialized)
            {
                await InitializeSPIPinsAsync().ConfigureAwait(false);
            }

            const ushort len = 3;
            var buf = new byte[len];
            var setupPacket = new UsbSetupPacket
            {
                Request = (byte)DeviceCommand.SendSPI,
                Index = 0,
                RequestType = (byte)((byte)UsbRequestType.TypeVendor | (byte)UsbCtrlFlags.Direction_In),
                Length = (short) len,
                Value = 0

            };
            
            var lengthTransferred = await UsbControlTransferAsync(setupPacket, buf, buf.Length).ConfigureAwait(false);
            return (ushort)(((ushort)buf[0]) |  (((ushort)buf[1])<<8));
        }
        private async Task<int> UsbControlTransferAsync(UsbSetupPacket usbSetupPacket, object buffer, int bufferLength)
        {
            int lengthTransferred = 0;
            if (_usbDevice != null)
            {
                await Task.Run(()=>_usbDevice.ControlTransfer(ref usbSetupPacket, buffer, bufferLength, out lengthTransferred)).ConfigureAwait(false);
            }
            else
            {
                lengthTransferred = bufferLength;
            }
            return lengthTransferred;
        }

        private async Task<int> SendDeviceCommandAsync(DeviceCommand deviceCommand, uint setupData)
        {
            return await SendDeviceCommandAsync(deviceCommand, setupData, _emptyBuf).ConfigureAwait(false);
        }
        private async Task<int> SendDeviceCommandAsync(DeviceCommand deviceCommand, uint setupData, byte[] data)
        {
            if (!_spiInitialized && deviceCommand != DeviceCommand.InitializeSPIPins)
            {
                await InitializeSPIPinsAsync().ConfigureAwait(false);
            }
            var bRequest = (byte) deviceCommand;
            var setupPacket = new UsbSetupPacket
            {
                Request = bRequest,
                Value = (short) (setupData & (uint)BasicMasks.SixteenBits),
                Index = (short) ((setupData & 0xFF0000)/0x10000),
                RequestType = (byte)UsbRequestType.TypeVendor,
                Length = 0
            };
            return await UsbControlTransferAsync(setupPacket, data, data.Length).ConfigureAwait(false);
        }

        #endregion

        #endregion

        #region EZ-USB firmware update
        private async Task ResetDeviceAsync(bool r)
        {
            byte[] buffer = {(byte) (r ? 1 : 0)};
            var setupPacket = new UsbSetupPacket
            {
                RequestType = (byte)UsbRequestType.TypeVendor,
                Request = 0xA0
            };
            unchecked
            {
                setupPacket.Value = (short) 0xE600;
            }
            setupPacket.Index = 0;
            int lengthTransferred = await UsbControlTransferAsync(setupPacket, buffer, buffer.Length).ConfigureAwait(false);
            await Task.Delay(r ? 50 : 400).ConfigureAwait(false); // give the firmware some time for initialization
        }
        public long UploadFirmware(IhxFile ihxFile)
        {
            return UploadFirmwareAsync(ihxFile).Result;
        }
        public async Task<long> UploadFirmwareAsync(IhxFile ihxFile)
        {
            const int transactionBytes = 256;
            var buffer = new byte[transactionBytes];

            await ResetDeviceAsync(true).ConfigureAwait(false); // reset = 1

            var startTime = DateTime.UtcNow;
            var j = 0;
            for (var i = 0; i <= ihxFile.IhxData.Length; i++)
            {
                if (i >= ihxFile.IhxData.Length || ihxFile.IhxData[i] < 0 || j >= transactionBytes)
                {
                    if (j > 0)
                    {
                        var setupPacket = new UsbSetupPacket
                        {
                            RequestType = (byte)UsbRequestType.TypeVendor,
                            Request = 0xA0,
                            Value = (short) (i - j),
                            Index = 0
                        };
                        int k = await UsbControlTransferAsync(setupPacket, buffer, j).ConfigureAwait(false);
                        if (k < 0 || k != j)
                        {
                            throw new ApplicationException();
                        }
                        await Task.Delay(1).ConfigureAwait(false); // to avoid package loss
                    }
                    j = 0;
                }

                if (i >= ihxFile.IhxData.Length || ihxFile.IhxData[i] < 0 || ihxFile.IhxData[i] > 255) continue;
                buffer[j] = (byte) ihxFile.IhxData[i];
                j += 1;
            }
            var endTime = DateTime.UtcNow;

            await ResetDeviceAsync(false).ConfigureAwait(false); //error (may caused re-numeration) can be ignored
            return (long) endTime.Subtract(startTime).TotalMilliseconds;
        }

        #endregion
    }
}