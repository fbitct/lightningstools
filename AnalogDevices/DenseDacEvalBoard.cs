using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Linq;

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
                return  _usbDevice !=null && 
                        _usbDevice.UsbRegistryInfo !=null 
                            ? _usbDevice.UsbRegistryInfo.SymbolicName 
                            : null;
            }
        }

        public bool GeneralPurposeIOPinState
        {
            get
            {
                if (GeneralPurposeIOPinDirection != IODirection.Input)
                {
                    throw new InvalidOperationException(
                        "GeneralPurposeIOPinDirection must be set to Input in order to read the GeneralPurposeIOPinState property.");
                }
                return (ReadbackGPIORegister() & GpioRegisterBits.Data) == GpioRegisterBits.Data; //readback GPIO register and return bit F0
            }
            set
            {
                if (GeneralPurposeIOPinDirection != IODirection.Output)
                {
                    throw new InvalidOperationException(
                        "GeneralPurposeIOPinDirection must be set to Output in order to set the GeneralPurposeIOPinState property.");
                }
                SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite,
                    value
                        ? (ushort)(GpioRegisterBits.Direction | GpioRegisterBits.Data) //sets bit F1=1 to configure GPIO as an output; sets bit F0=1 to write a 1 to the GPIO output 
                        : (ushort)(GpioRegisterBits.Direction) //sets bit F1=1 to configure GPIO as an output, bit F0 will not be set so will be written as a 0 to the GPIO output
                ); 
            }
        }

        public IODirection GeneralPurposeIOPinDirection
        {
            get
            {
                var gpioRegisterVal = ReadbackGPIORegister();
                return ((gpioRegisterVal & GpioRegisterBits.Direction) == GpioRegisterBits.Direction)//if bit F1 =1, GPIO pin is configured for output (else, is configured for input)
                            ? IODirection.Output
                            : IODirection.Input;
            }
            set
            {
                SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, 
                    value==IODirection.Output 
                        ? (ushort)GpioRegisterBits.Direction
                        : (ushort)BasicMasks.AllBitsZero);
            }
        }

        public ushort OffsetDAC0
        {
            get { return ReadbackOFS0Register(); }
            set
            {
                SetCLRPinLow();
                WriteOFS0Register(value);
                SetCLRPinHigh();
            }
        }

        public ushort OffsetDAC1
        {
            get { return ReadbackOFS1Register(); }
            set
            {
                SetCLRPinLow();
                WriteOFS1Register(value);
                SetCLRPinHigh();
            }
        }

        public ushort OffsetDAC2
        {
            get { return ReadbackOFS2Register(); }
            set
            {
                SetCLRPinLow();
                WriteOFS2Register(value);
                SetCLRPinHigh();
            }
        }

        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get
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
                        channelNumberOrInputPinNumber = (byte) (_monitorFlags & MonitorBits.DacChannel); //bits F0-F3 specify the monitored DAC channel
                    }
                }
                
                var toReturn = new ChannelMonitorOptions(source, channelNumberOrInputPinNumber);
                toReturn.PropertyChanged += MonitorOptionsPropertyChangedHandler;
                return toReturn;
            }
            set { SendNewChannelMonitorOptionsToDevice(value); }
        }

        public bool IsThermalShutdownEnabled
        {
            get
            {
                var controlRegisterBits = ReadbackControlRegister();
                return ((controlRegisterBits & ControlRegisterBits.ThermalShutdownEnabled) == ControlRegisterBits.ThermalShutdownEnabled); //if bit 1=1, thermal shutdown is enabled
            }
            set
            {
                var controlRegisterBits = ReadbackControlRegister();
                if (value)
                {
                    controlRegisterBits |= ControlRegisterBits.ThermalShutdownEnabled;
                }
                else
                {
                    controlRegisterBits &= ~ControlRegisterBits.ThermalShutdownEnabled;
                }
                WriteControlRegister(controlRegisterBits);
            }
        }

        public DacPrecision DacPrecision
        {
            get
            {
                return _thisDevicePrecision;
            }
            set
            {
                _thisDevicePrecision = value;
            }
        }

        public bool PacketErrorCheckErrorOccurred
        {
            get
            {
                var controlRegisterBits = ReadbackControlRegister();
                return (controlRegisterBits & ControlRegisterBits.PacketErrorCheckErrorOccurred) == ControlRegisterBits.PacketErrorCheckErrorOccurred;
            }
        }

        public bool IsOverTemperature
        {
            get
            {
                var controlRegisterBits = ReadbackControlRegister();
                return (controlRegisterBits & ControlRegisterBits.OverTemperature) == ControlRegisterBits.OverTemperature;
            }
        }

        #endregion

        #region Dac Channel Data Source Selection

        public void SetDacChannelDataSource(ChannelAddress channelAddress, DacChannelDataSource value)
        {
            if ((int) channelAddress < 8 || (int) channelAddress > 47)
            {
                if (channelAddress == ChannelAddress.AllGroupsAllChannels)
                {
                    SetDacChannelDataSourceAllChannels(value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group0, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group1, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group2AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group2, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group3AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group3, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group4AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group4, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel0)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel0, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel1)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel1, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel2)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel2, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel3)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel3, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel4)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel4, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel5)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel5, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel6)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel6, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group0Through4Channel7)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel7, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel0)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel0, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel1)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel1, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel2)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel2, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel3)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel3, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel4)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel4, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel5)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel5, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel6)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel6, value);
                    return;
                }
                else if (channelAddress == ChannelAddress.Group1Through4Channel7)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel7, value);
                    return;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("channelAddress");
                }
            }
            var channelNum = (byte) ((byte) channelAddress - 8);
            var currentSourceSelections = ABSelectRegisterBits.AllChannelsA;

            var specialFunctionCode = SpecialFunctionCode.NOP;
            if (channelNum < 8)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister0;
                currentSourceSelections = ReadbackABSelect0Register();
            }
            else if (channelNum >= 8 && channelNum < 16)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister1;
                currentSourceSelections = ReadbackABSelect1Register();
            }
            else if (channelNum >= 16 && channelNum < 24)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister2;
                currentSourceSelections = ReadbackABSelect2Register();
            }
            else if (channelNum >= 24 && channelNum < 32)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister3;
                currentSourceSelections = ReadbackABSelect3Register();
            }
            else if (channelNum >= 32 && channelNum <= 39)
            {
                specialFunctionCode = SpecialFunctionCode.WriteToABSelectRegister4;
                currentSourceSelections = ReadbackABSelect4Register();
            }
            var newSourceSelections = currentSourceSelections;

            var channelOffset = (byte) (channelNum % 8);
            var channelMask = (ABSelectRegisterBits) (1 << channelOffset);

            if (value == DacChannelDataSource.DataValueA)
            {
                newSourceSelections &= ~channelMask;
            }
            else
            {
                newSourceSelections |= channelMask;
            }
            newSourceSelections &= ABSelectRegisterBits.WritableBits; //ensure we only send 8 bits of data (one for each channel in the group)
            SendSpecialFunction(specialFunctionCode, (ushort)newSourceSelections);
        }

        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channelAddress)
        {
            if ((int) channelAddress < 8 || (int) channelAddress > 47)
            {
                throw new ArgumentOutOfRangeException("channelAddress");
            }
            var channelNum = (byte) ((byte) channelAddress - 8);
            var currentSourceSelections = ABSelectRegisterBits.AllChannelsA;
            if (channelNum < 8)
            {
                currentSourceSelections = ReadbackABSelect0Register();
            }
            else if (channelNum >= 8 && channelNum < 16)
            {
                currentSourceSelections = ReadbackABSelect1Register();
            }
            else if (channelNum >= 16 && channelNum < 24)
            {
                currentSourceSelections = ReadbackABSelect2Register();
            }
            else if (channelNum >= 24 && channelNum < 32)
            {
                currentSourceSelections = ReadbackABSelect3Register();
            }
            else if (channelNum >= 32 && channelNum <= 39)
            {
                currentSourceSelections = ReadbackABSelect4Register();
            }
            var channelOffset = (byte) (channelNum % 8);
            var channelMask = (ABSelectRegisterBits) (1 << channelOffset);
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
            SendSpecialFunction(specialFunctionCode, (ushort)abSelectRegisterBits);
        }

        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            switch (source)
            {
                case DacChannelDataSource.DataValueA:
                    SendSpecialFunction(SpecialFunctionCode.BlockWriteABSelectRegisters, (ushort)ABSelectRegisterBits.AllChannelsA);
                    break;
                case DacChannelDataSource.DataValueB:
                    SendSpecialFunction(SpecialFunctionCode.BlockWriteABSelectRegisters, (ushort)ABSelectRegisterBits.AllChannelsB);
                    break;
                default:
                    throw new ArgumentException("source");
            }
        }

        #endregion

        #region Device Control Functions

        public void PerformSoftPowerDown()
        {
            var controlRegisterBits = ReadbackControlRegister();
            controlRegisterBits |= ControlRegisterBits.SoftPowerDown; //set bit F0=1 to perform soft power-down
            WriteControlRegister(controlRegisterBits);
        }

        public void PerformSoftPowerUp()
        {
            var controlRegisterBits = ReadbackControlRegister();
            controlRegisterBits &= ~ControlRegisterBits.SoftPowerDown; //set bit F0=0 to perform soft power-up;
            WriteControlRegister(controlRegisterBits);
        }

        public void Reset()
        {
            SetRESETPinHigh();
            Thread.Sleep(1000);
            SetRESETPinLow();
        }

        public void SuspendAllDacOutputs()
        {
            SetCLRPinLow();
        }

        public void ResumeAllDacOutputs()
        {
            SetCLRPinHigh();
        }

        public void UpdateAllDacOutputs()
        {
            PulseLDacPin();
        }

        #endregion

        #region Dac Functions

        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackX1ARegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackX1BRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelDataValueA(ChannelAddress channelAddress, ushort newVal)
        {
            var controlRegisterBits = ReadbackControlRegister();
            controlRegisterBits &= ~ControlRegisterBits.InputRegisterSelect; //set control register bit F2 =0 to select register X1A for input
            WriteControlRegister(controlRegisterBits);

            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | newVal);
            }
            else
            {
                SendSPI((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channelAddress & (byte)BasicMasks.SixBits) << 16) | (uint) ((newVal & (uint)BasicMasks.FourteenBits) << 2));
            }
        }

        public void SetDacChannelDataValueB(ChannelAddress channels, ushort newVal)
        {
            var controlRegisterBits = ReadbackControlRegister();
            controlRegisterBits |= ControlRegisterBits.InputRegisterSelect;//set control register bit F2 =1 to select register X1B for input
            WriteControlRegister(controlRegisterBits);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | newVal));
            }
            else
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | (uint) ((newVal & (uint)BasicMasks.FourteenBits) << 2)));
            }
        }

        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackCRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelOffset(ChannelAddress channels, ushort newVal)
        {
            SetCLRPinLow();
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | newVal));
            }
            else
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | (uint) ((newVal & (uint)BasicMasks.FourteenBits) << 2)));
            }
            SetCLRPinHigh();
        }

        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackMRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelGain(ChannelAddress channels, ushort newVal)
        {
            SetCLRPinLow();
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACGainRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | newVal));
            }
            else
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACGainRegister | (uint) (((byte) channels & (byte)BasicMasks.SixBits) << 16) | (uint) ((newVal & (uint)BasicMasks.FourteenBits) << 2)));
            }
            SetCLRPinHigh();
        }

        #endregion

        #region Device Enumeration

        public static DenseDacEvalBoard[] Enumerate()
        {
            var discoveredDevices = new List<string>();
            var toReturn = new List<DenseDacEvalBoard>();
            var devs = UsbDevice.AllDevices;
            for (var i = 0; i < devs.Count; i++)
            {
                var device = devs[i].Device;
                if (device != null)
                {
                    if (
                        device.UsbRegistryInfo.Vid == 0x0456
                        &&
                        (
                            (ushort) device.UsbRegistryInfo.Pid == 0xB20F
                            ||
                            (ushort) device.UsbRegistryInfo.Pid == 0xB20E
                        )
                        )
                    {
                        if (!discoveredDevices.Contains(device.UsbRegistryInfo.SymbolicName))
                        {
                            toReturn.Add(new DenseDacEvalBoard(device));
                        }
                        discoveredDevices.Add(device.UsbRegistryInfo.SymbolicName);
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

        private void SendNewChannelMonitorOptionsToDevice(ChannelMonitorOptions value)
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
            SendSpecialFunction(SpecialFunctionCode.ConfigureMonitoring, (ushort)_monitorFlags);
        }

        private void MonitorOptionsPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            SendNewChannelMonitorOptionsToDevice((ChannelMonitorOptions) sender);
        }

        #endregion

        #region Register Readback Functions

        private ushort ReadbackX1ARegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((((byte) channelNum) & (byte)BasicMasks.SixBits) << 7));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private ushort ReadbackX1BRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.X1BRegister) | (ushort) ((((byte) channelNum) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private ushort ReadbackCRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.CRegister)| (ushort) ((((byte) channelNum) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private ushort ReadbackMRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) ((ushort)(AddressCodesForDataReadback.MRegister) | (ushort) ((((byte) channelNum) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            return val;
        }

        private ControlRegisterBits ReadbackControlRegister()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ControlRegister);
            return (ControlRegisterBits) (ReadSPI() & (ushort)ControlRegisterBits.ReadableBits);
        }

        private ushort ReadbackOFS0Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF0Register);
            return (ushort) (ReadSPI() & (ushort)BasicMasks.FourteenBits);
        }

        private ushort ReadbackOFS1Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF1Register);
            return (ushort) (ReadSPI() & (ushort)BasicMasks.FourteenBits);
        }

        private ushort ReadbackOFS2Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OSF2Register);
            return (ushort) (ReadSPI() & (ushort)BasicMasks.FourteenBits);
        }

        private ABSelectRegisterBits ReadbackABSelect0Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect0Register);
            return (ABSelectRegisterBits) (ReadSPI() & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private ABSelectRegisterBits ReadbackABSelect1Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect1Register);
            return (ABSelectRegisterBits)(ReadSPI() & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private ABSelectRegisterBits ReadbackABSelect2Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect2Register);
            return (ABSelectRegisterBits)(ReadSPI() & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private ABSelectRegisterBits ReadbackABSelect3Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect3Register);
            return (ABSelectRegisterBits)(ReadSPI() & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private ABSelectRegisterBits ReadbackABSelect4Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect4Register);
            return (ABSelectRegisterBits)(ReadSPI() & (ushort)ABSelectRegisterBits.ReadableBits);
        }

        private GpioRegisterBits ReadbackGPIORegister()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.GPIORegister);
            return (GpioRegisterBits)((ReadSPI() & (ushort)GpioRegisterBits.ReadableBits));
        }

        #endregion

        #region Register Writing Functions

        private void WriteControlRegister(ControlRegisterBits controlRegisterBits)
        {
            controlRegisterBits &= ControlRegisterBits.WritableBits;
            SendSpecialFunction(SpecialFunctionCode.WriteControlRegister, (ushort) controlRegisterBits);
        }

        private void WriteOFS0Register(ushort newVal)
        {
            newVal &= (ushort)BasicMasks.FourteenBits;
            SendSpecialFunction(SpecialFunctionCode.WriteOSF0Register, newVal);
        }

        private void WriteOFS1Register(ushort newVal)
        {
            newVal &= (ushort)BasicMasks.FourteenBits;
            SendSpecialFunction(SpecialFunctionCode.WriteOSF1Register, newVal);
        }

        private void WriteOFS2Register(ushort newVal)
        {
            newVal &= (ushort)BasicMasks.FourteenBits;
            SendSpecialFunction(SpecialFunctionCode.WriteOSF2Register, newVal);
        }

        #endregion

        #region Pin Manipulation Functions

        private void SetRESETPinHigh()
        {
            SendDeviceCommand(DeviceCommand.SetRESETPinHigh, 0);
        }

        private void SetRESETPinLow()
        {
            SendDeviceCommand(DeviceCommand.SetRESETPinLow, 0);
        }

        private void SetCLRPinHigh()
        {
            SendDeviceCommand(DeviceCommand.SetCLRPinHigh, 0);
        }

        private void SetCLRPinLow()
        {
            SendDeviceCommand(DeviceCommand.SetCLRPinLow, 0);
        }

        public void PulseLDacPin()
        {
            SendDeviceCommand(DeviceCommand.PulseLDacPin, 0);
        }

        public void SetLDacPinLow()
        {
            SendDeviceCommand(DeviceCommand.SetLDacPinLow, 0);
        }

        public void SetLDacPinHigh()
        {
            SendDeviceCommand(DeviceCommand.SetLDacPinHigh, 0);
        }

        private void InitializeSPIPins()
        {
            SendDeviceCommand(DeviceCommand.InitializeSPIPins, 0);
            _spiInitialized = true;
        }

        #endregion

        #region Device Communications

        private void SendSpecialFunction(SpecialFunctionCode specialFunction, ushort data)
        {
            SendSPI((uint) ((((byte) specialFunction & (byte)BasicMasks.SixBits) << 16) | data));
        }

        private int SendSPI(UInt32 data)
        {
            return SendDeviceCommand(DeviceCommand.SendSPI, data);
        }

        private ushort ReadSPI()
        {
            if (!_spiInitialized)
            {
                InitializeSPIPins();
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
            
            int lengthTransferred;
            UsbControlTransfer(ref setupPacket, buf, buf.Length, out lengthTransferred);
            return (ushort)(((ushort)buf[0]) |  (((ushort)buf[1])<<8));
        }

        private void UsbControlTransfer(ref UsbSetupPacket setupPacket, object buffer, int bufferLength,
                                        out int lengthTransferred)
        {
            if (_usbDevice != null)
            {
                _usbDevice.ControlTransfer(ref setupPacket, buffer, bufferLength, out lengthTransferred);
            }
            else
            {
                lengthTransferred = bufferLength;
            }
        }

        private int SendDeviceCommand(DeviceCommand command, UInt32 setupData)
        {
            return SendDeviceCommand(command, setupData, _emptyBuf);
        }

        private int SendDeviceCommand(DeviceCommand command, UInt32 setupData, byte[] data)
        {
            if (!_spiInitialized && command != DeviceCommand.InitializeSPIPins) InitializeSPIPins();
            var bRequest = (byte) command;
            var setupPacket = new UsbSetupPacket
            {
                Request = bRequest,
                Value = (short) (setupData & (uint)BasicMasks.SixteenBits),
                Index = (short) ((setupData & 0xFF0000)/0x10000),
                RequestType = (byte)UsbRequestType.TypeVendor,
                Length = 0
            };
            int lengthTransferred;
            UsbControlTransfer(ref setupPacket, data, data.Length, out lengthTransferred);
            return lengthTransferred;
        }

        #endregion

        #endregion

        #region EZ-USB firmware update

        private void ResetDevice(bool r)
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
            int lengthTransferred;
            UsbControlTransfer(ref setupPacket, buffer, buffer.Length, out lengthTransferred);
            Thread.Sleep(r ? 50 : 400); // give the firmware some time for initialization
        }

        public long UploadFirmware(IhxFile ihxFile)
        {
            const int transactionBytes = 256;
            var buffer = new byte[transactionBytes];

            ResetDevice(true); // reset = 1

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
                        int k;
                        UsbControlTransfer(ref setupPacket, buffer, j, out k);
                        if (k < 0 || k != j)
                        {
                            throw new ApplicationException();
                        }
                        Thread.Sleep(1); // to avoid package loss
                    }
                    j = 0;
                }

                if (i >= ihxFile.IhxData.Length || ihxFile.IhxData[i] < 0 || ihxFile.IhxData[i] > 255) continue;
                buffer[j] = (byte) ihxFile.IhxData[i];
                j += 1;
            }
            var endTime = DateTime.UtcNow;

            ResetDevice(false); //error (may caused re-numeration) can be ignored
            return (long) endTime.Subtract(startTime).TotalMilliseconds;
        }

        #endregion
    }
}