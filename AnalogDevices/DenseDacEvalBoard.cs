using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                return _usbDevice != null &&
                        _usbDevice.UsbRegistryInfo != null
                            ? _usbDevice.UsbRegistryInfo.SymbolicName
                            : null;
            }
        }

        public bool GeneralPurposeIOPinState
        {
            get
            {
                var gpioPinDirection = GeneralPurposeIOPinDirection;
                if (gpioPinDirection != IODirection.Input)
                {
                    throw new InvalidOperationException(
                        "GeneralPurposeIOPinDirection must be set to Input in order to read the GeneralPurposeIOPinState property.");
                }
                var gpioRegister = ReadbackGPIORegister();////readback GPIO register 
                return (gpioRegister & GpioRegisterBits.Data) == GpioRegisterBits.Data; //return bit F0
            }
            set
            {
                var gpioPinDirection = GeneralPurposeIOPinDirection;
                if (gpioPinDirection != IODirection.Output)
                {
                    throw new InvalidOperationException(
                        "GeneralPurposeIOPinDirection must be set to Output in order to set the GeneralPurposeIOPinState property.");
                }
                var newValue = value
                        ? (GpioRegisterBits.Direction | GpioRegisterBits.Data) //sets bit F1=1 to configure GPIO as an output; sets bit F0=1 to write a 1 to the GPIO output 
                        : (GpioRegisterBits.Direction); //sets bit F1=1 to configure GPIO as an output, bit F0 will not be set so will be written as a 0 to the GPIO output

                SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, (ushort)newValue);
                _gpioRegister = newValue;
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
                                    value == IODirection.Output
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
                        channelNumberOrInputPinNumber = (byte)(_monitorFlags & MonitorBits.DacChannel); //bits F0-F3 specify the monitored DAC channel
                    }
                }
                var toReturn = new ChannelMonitorOptions(source, channelNumberOrInputPinNumber);
                toReturn.PropertyChanged += MonitorOptionsPropertyChangedHandler;
                return toReturn;
            }
            set
            {
                SendNewChannelMonitorOptionsToDevice(value);
            }
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
                lock (_controlRegisterLock)
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
        }

        public DacPrecision DacPrecision
        {
            get { return _thisDevicePrecision; }
            set { _thisDevicePrecision = value; }
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
        public void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value)
        {
            if ((int)channel < 8 || (int)channel > 47)
            {
                if (channel == ChannelAddress.AllGroupsAllChannels)
                {
                    SetDacChannelDataSourceAllChannels(value);
                    return;
                }
                else if (channel == ChannelAddress.Group0AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group0, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group1, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channel == ChannelAddress.Group2AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group2, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channel == ChannelAddress.Group3AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group3, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channel == ChannelAddress.Group4AllChannels)
                {
                    SetDacChannelDataSource(ChannelGroup.Group4, value, value, value, value, value, value, value, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel0)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel0, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel1)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel1, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel2)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel2, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel3)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel3, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel4)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel4, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel5)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel5, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel6)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel6, value);
                    return;
                }
                else if (channel == ChannelAddress.Group0Through4Channel7)
                {
                    SetDacChannelDataSource(ChannelAddress.Group0Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group1Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel7, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel0)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel0, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel0, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel1)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel1, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel1, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel2)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel2, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel2, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel3)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel3, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel3, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel4)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel4, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel4, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel5)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel5, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel5, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel6)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel6, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel6, value);
                    return;
                }
                else if (channel == ChannelAddress.Group1Through4Channel7)
                {
                    SetDacChannelDataSource(ChannelAddress.Group1Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group2Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group3Channel7, value);
                    SetDacChannelDataSource(ChannelAddress.Group4Channel7, value);
                    return;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("channel");
                }
            }
            SetDacChannelDataSourceInternal(channel, value);
        }

        private void SetDacChannelDataSourceInternal(ChannelAddress channel, DacChannelDataSource value)
        {
            var channelNum = (byte)((byte)channel - 8);
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
            SendSpecialFunction(specialFunctionCode, (ushort)newSourceSelections);
            switch (specialFunctionCode)
            {
                case SpecialFunctionCode.WriteToABSelectRegister0:
                    _abSelect0Register = newSourceSelections;
                    break;
                case SpecialFunctionCode.WriteToABSelectRegister1:
                    _abSelect1Register = newSourceSelections;
                    break;
                case SpecialFunctionCode.WriteToABSelectRegister2:
                    _abSelect2Register = newSourceSelections;
                    break;
                case SpecialFunctionCode.WriteToABSelectRegister3:
                    _abSelect3Register = newSourceSelections;
                    break;
                case SpecialFunctionCode.WriteToABSelectRegister4:
                    _abSelect4Register = newSourceSelections;
                    break;
            }
        }

        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel)
        {
            if ((int)channel < 8 || (int)channel > 47)
            {
                throw new ArgumentOutOfRangeException("channel");
            }
            var channelNum = (byte)((byte)channel - 8);
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
                    _abSelect0Register = ABSelectRegisterBits.AllChannelsA;
                    _abSelect1Register = ABSelectRegisterBits.AllChannelsA;
                    _abSelect2Register = ABSelectRegisterBits.AllChannelsA;
                    _abSelect3Register = ABSelectRegisterBits.AllChannelsA;
                    break;
                case DacChannelDataSource.DataValueB:
                    SendSpecialFunction(SpecialFunctionCode.BlockWriteABSelectRegisters, (ushort)ABSelectRegisterBits.AllChannelsB);
                    _abSelect0Register = ABSelectRegisterBits.AllChannelsB;
                    _abSelect1Register = ABSelectRegisterBits.AllChannelsB;
                    _abSelect2Register = ABSelectRegisterBits.AllChannelsB;
                    _abSelect3Register = ABSelectRegisterBits.AllChannelsB;
                    break;
                default:
                    throw new ArgumentException("source");
            }
        }

        #endregion

        #region Device Control Functions

        public void PerformSoftPowerDown()
        {
            lock (_controlRegisterLock)
            {
                ClearAllCachedCopiesOfRegisters();
                var controlRegisterBits = ReadbackControlRegister();
                controlRegisterBits |= ControlRegisterBits.SoftPowerDown; //set bit F0=1 to perform soft power-down
                WriteControlRegister(controlRegisterBits);
            }
        }

        public void PerformSoftPowerUp()
        {
            lock (_controlRegisterLock)
            {
                ClearAllCachedCopiesOfRegisters();
                var controlRegisterBits = ReadbackControlRegister();
                controlRegisterBits &= ~ControlRegisterBits.SoftPowerDown; //set bit F0=0 to perform soft power-up;
                WriteControlRegister(controlRegisterBits);
            }
        }

        public void Reset()
        {
            ClearAllCachedCopiesOfRegisters();
            SetRESETPinHigh();
            System.Threading.Thread.Sleep(1000);
            SetRESETPinLow();
        }
        private void ClearAllCachedCopiesOfRegisters()
        {
            _abSelect0Register = null;
            _abSelect1Register = null;
            _abSelect2Register = null;
            _abSelect3Register = null;
            _abSelect4Register = null;
            _controlRegister = null;
            _cRegisters = new ushort?[40];
            _mRegisters = new ushort?[40];
            _x1aRegisters = new ushort?[40];
            _x1bRegisters = new ushort?[40];
            _ofs0Register = null;
            _ofs1Register = null;
            _ofs2Register = null;
            _gpioRegister = null;
            
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
            PulseLDACPin();
        }

        #endregion

        #region Dac Functions
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            if ((int)channel >= 8 && (int)channel <= 47)
            {
                return ReadbackX1ARegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            if ((int)channel >= 8 && (int)channel <= 47)
            {
                return ReadbackX1BRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelDataValueA(ChannelAddress channel, ushort value)
        {
            lock (_controlRegisterLock)
            {
                var controlRegisterBits = ReadbackControlRegister();
                controlRegisterBits &= ~ControlRegisterBits.InputRegisterSelect; //set control register bit F2 =0 to select register X1A for input
                WriteControlRegister(controlRegisterBits);

                if (DacPrecision == DacPrecision.SixteenBit)
                {
                    SendSPI((uint)SerialInterfaceModeBits.WriteToDACInputDataRegisterX | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | value);
                    _x1aRegisters[(int)channel - 8] = value;
                }
                else
                {
                    SendSPI((uint)SerialInterfaceModeBits.WriteToDACInputDataRegisterX | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | (uint)((value & (uint)BasicMasks.FourteenBits) << 2));
                    _x1aRegisters[(int)channel - 8] = (ushort)((value & (uint)BasicMasks.FourteenBits) << 2);
                }
            }
        }


        public void SetDacChannelDataValueB(ChannelAddress channel, ushort value)
        {
            lock (_controlRegisterLock)
            {
                var controlRegisterBits = ReadbackControlRegister();
                controlRegisterBits |= ControlRegisterBits.InputRegisterSelect;//set control register bit F2 =1 to select register X1B for input
                WriteControlRegister(controlRegisterBits);
                if (DacPrecision == DacPrecision.SixteenBit)
                {
                    SendSPI(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegisterX | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | value));
                    _x1bRegisters[(int)channel - 8] = value;
                }
                else
                {
                    SendSPI(((uint)SerialInterfaceModeBits.WriteToDACInputDataRegisterX | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | (uint)((value & (uint)BasicMasks.FourteenBits) << 2)));
                    _x1bRegisters[(int)channel - 8] = (ushort)((value & (uint)BasicMasks.FourteenBits) << 2);
                }
            }
        }

        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            if ((int)channel >= 8 && (int)channel <= 47)
            {
                return ReadbackCRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelOffset(ChannelAddress channel, ushort value)
        {
            SetCLRPinLow();
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegisterC | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | value));
                _cRegisters[(int)channel - 8] = value;
            }
            else
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACOffsetRegisterC | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | (uint)((value & (uint)BasicMasks.FourteenBits) << 2)));
                _cRegisters[(int)channel - 8] = (ushort)((value & (uint)BasicMasks.FourteenBits) << 2);
            }
            SetCLRPinHigh();
        }

        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            if ((int)channel >= 8 && (int)channel <= 47)
            {
                return ReadbackMRegister(channel);
            }
            throw new ArgumentOutOfRangeException("channel");
        }

        public void SetDacChannelGain(ChannelAddress channel, ushort value)
        {
            SetCLRPinLow();
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACGainRegisterM | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | value));
                _mRegisters[(int)channel - 8] = value;
            }
            else
            {
                SendSPI(((uint)SerialInterfaceModeBits.WriteToDACGainRegisterM | (uint)(((byte)channel & (byte)BasicMasks.SixBits) << 16) | (uint)((value & (uint)BasicMasks.FourteenBits) << 2)));
                _mRegisters[(int)channel - 8] = (ushort)((value & (uint)BasicMasks.FourteenBits) << 2); 
            }
            SetCLRPinHigh();
        }

        #endregion

        #region Device Enumeration
        public static IDenseDacEvalBoard[] Enumerate()
        {
            var discoveredDevices = new List<string>();
            var toReturn = new List<DenseDacEvalBoard>();
            var devs = UsbDevice.AllDevices;
            for (var i = 0; i < devs.Count; i++)
            {
                var device = devs[i].Device;
                if (device != null)
                {
                    var registryInfo = device.UsbRegistryInfo;
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
                            var newDevice = new DenseDacEvalBoard(device);
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
        private object _controlRegisterLock = new object();
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
                    _monitorFlags |= (MonitorBits)(value.ChannelNumberOrInputPinNumber & (byte)BasicMasks.FourBits); //set bits F0-F3 with the DAC channel number to monitor
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
        private ushort?[] _x1aRegisters=new ushort?[40];
        private ushort ReadbackX1ARegister(ChannelAddress channel)
        {
            if (_x1aRegisters[(int)channel - 8].HasValue)
            {
                return _x1aRegisters[(int)channel - 8].Value;
            }

            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort)((((byte)channel) & (byte)BasicMasks.SixBits) << 7));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            _x1aRegisters[(int)channel - 8] = val;
            return val;
        }

        private ushort?[] _x1bRegisters = new ushort?[40];
        private ushort ReadbackX1BRegister(ChannelAddress channel)
        {
            if (_x1bRegisters[(int)channel-8].HasValue)
            {
                return _x1bRegisters[(int)channel - 8].Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                            (ushort)((ushort)(AddressCodesForDataReadback.X1BRegister) | (ushort)((((byte)channel) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            _x1bRegisters[(int)channel - 8] = val;
            return val;
        }

        private ushort?[] _cRegisters = new ushort?[40];
        private ushort ReadbackCRegister(ChannelAddress channel)
        {
            if (_cRegisters[(int)channel-8].HasValue)
            {
                return _cRegisters[(int)channel - 8].Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                            (ushort)((ushort)(AddressCodesForDataReadback.CRegister) | (ushort)((((byte)channel) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            _cRegisters[(int)channel - 8] = val;
            return val;
        }
        private ushort?[] _mRegisters = new ushort?[40];
        private ushort ReadbackMRegister(ChannelAddress channel)
        {
            if (_mRegisters[(int)channel - 8].HasValue)
            {
                return _mRegisters[(int)channel - 8].Value;
            }

            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                            (ushort)((ushort)(AddressCodesForDataReadback.MRegister) | (ushort)((((byte)channel) & (byte)BasicMasks.SixBits) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= (ushort)BasicMasks.FourteenBits;
            }
            _mRegisters[(int)channel - 8] = val;
            return val;
        }

        private ControlRegisterBits? _controlRegister;
        private ControlRegisterBits ReadbackControlRegister()
        {
            if (_controlRegister.HasValue)
            {
                return _controlRegister.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ControlRegister);
            var val = (ControlRegisterBits)(ReadSPI() & (ushort)ControlRegisterBits.ReadableBits);
            _controlRegister = val;
            return val;
        }

        private ushort? _ofs0Register;
        private ushort ReadbackOFS0Register()
        {
            if (_ofs0Register.HasValue)
            {
                return _ofs0Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OFS0Register);
            var spi = ReadSPI();
            var val = (ushort)(spi & (ushort)BasicMasks.FourteenBits);
            _ofs0Register = val;
            return val;
        }
        private ushort? _ofs1Register;
        private ushort ReadbackOFS1Register()
        {
            if (_ofs1Register.HasValue)
            {
                return _ofs1Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OFS1Register);
            var spi = ReadSPI();
            var val = (ushort)(spi & (ushort)BasicMasks.FourteenBits);
            _ofs1Register = val;
            return val;
        }
        private ushort? _ofs2Register;
        private ushort ReadbackOFS2Register()
        {
            if (_ofs2Register.HasValue)
            {
                return _ofs2Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.OFS2Register);
            var spi = ReadSPI();
            var val = (ushort)(spi & (ushort)BasicMasks.FourteenBits);
            _ofs2Register = val;
            return val;
        }

        private ABSelectRegisterBits? _abSelect0Register;
        private ABSelectRegisterBits ReadbackABSelect0Register()
        {
            if (_abSelect0Register.HasValue)
            {
                return _abSelect0Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect0Register);
            var spi = ReadSPI();
            var val = (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
            _abSelect0Register = val;
            return val;
        }
        private ABSelectRegisterBits? _abSelect1Register;
        private ABSelectRegisterBits ReadbackABSelect1Register()
        {
            if (_abSelect1Register.HasValue)
            {
                return _abSelect1Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect1Register);
            var spi = ReadSPI();
            var val = (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
            _abSelect1Register = val;
            return val;
        }
        private ABSelectRegisterBits? _abSelect2Register;
        private ABSelectRegisterBits ReadbackABSelect2Register()
        {
            if (_abSelect2Register.HasValue)
            {
                return _abSelect2Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect2Register);
            var spi = ReadSPI();
            var val = (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
            _abSelect2Register = val;
            return val;
        }
        private ABSelectRegisterBits? _abSelect3Register;
        private ABSelectRegisterBits ReadbackABSelect3Register()
        {
            if(_abSelect3Register.HasValue)
            {
                return _abSelect3Register.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect3Register);
            var spi = ReadSPI();
            var val = (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
            _abSelect3Register = val;
            return val;
        }
        private ABSelectRegisterBits? _abSelect4Register;
        private ABSelectRegisterBits ReadbackABSelect4Register()
        {
            if (_abSelect4Register.HasValue)
            {
                return _abSelect4Register.Value;
            }

            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.ABSelect4Register);
            var spi = ReadSPI();
            var val = (ABSelectRegisterBits)(spi & (ushort)ABSelectRegisterBits.ReadableBits);
            _abSelect4Register = val;
            return val;
        }
        private GpioRegisterBits? _gpioRegister;
        private GpioRegisterBits ReadbackGPIORegister()
        {
            if (_gpioRegister.HasValue)
            {
                return _gpioRegister.Value;
            }
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (ushort)AddressCodesForDataReadback.GPIORegister);
            var spi = ReadSPI();
            var val = (GpioRegisterBits)(spi & (ushort)GpioRegisterBits.ReadableBits);
            _gpioRegister = val;
            return val;
        }

        #endregion

        #region Register Writing Functions

        private void WriteControlRegister(ControlRegisterBits controlRegisterBits)
        {
            controlRegisterBits &= ControlRegisterBits.WritableBits;
            if (controlRegisterBits != _controlRegister)
            {
                _controlRegister = controlRegisterBits;
                SendSpecialFunction(SpecialFunctionCode.WriteControlRegister, (ushort)controlRegisterBits);
            }
        }

        private void WriteOFS0Register(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            if (value != _ofs0Register)
            {
                _ofs0Register = value;
                SendSpecialFunction(SpecialFunctionCode.WriteOFS0Register, value);
            }
        }
        private void WriteOFS1Register(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            if (value != _ofs1Register)
            {
                _ofs1Register = value;
                SendSpecialFunction(SpecialFunctionCode.WriteOFS1Register, value);
            }
        }
        private void WriteOFS2Register(ushort value)
        {
            value &= (ushort)BasicMasks.FourteenBits;
            if (value != _ofs2Register)
            {
                _ofs2Register = value;
                SendSpecialFunction(SpecialFunctionCode.WriteOFS2Register, value);
            }
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

        public void PulseLDACPin()
        {
            SendDeviceCommand(DeviceCommand.PulseLDACPin, 0);
        }

        public void SetLDACPinLow()
        {
            SendDeviceCommand(DeviceCommand.SetLDACPinLow, 0);
        }

        public void SetLDACPinHigh()
        {
            SendDeviceCommand(DeviceCommand.SetLDACPinHigh, 0);
        }

        private void InitializeSPIPins()
        {
            SendDeviceCommand(DeviceCommand.InitializeSPIPins, 0);
            _spiInitialized = true;
        }

        #endregion

        #region Device Communications
        private void SendSpecialFunction(SpecialFunctionCode specialFunctionCode, ushort data)
        {
            SendSPI((uint) ((((byte) specialFunctionCode & (byte)BasicMasks.SixBits) << 16) | data));
        }
        private int SendSPI(uint data)
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
            
            UsbControlTransfer(setupPacket, buf, buf.Length);
            return (ushort)(((ushort)buf[0]) |  (((ushort)buf[1])<<8));
        }
        private int UsbControlTransfer(UsbSetupPacket usbSetupPacket, object buffer, int bufferLength)
        {
            int lengthTransferred = 0;
            if (_usbDevice != null)
            {
                _usbDevice.ControlTransfer(ref usbSetupPacket, buffer, bufferLength, out lengthTransferred);
            }
            else
            {
                lengthTransferred = bufferLength;
            }
            return lengthTransferred;
        }

        private int SendDeviceCommand(DeviceCommand deviceCommand, uint setupData, byte[] data=null)
        {
            if (!_spiInitialized && deviceCommand != DeviceCommand.InitializeSPIPins)
            {
                InitializeSPIPins();
            }
            data = data ?? _emptyBuf;
            var bRequest = (byte) deviceCommand;
            var setupPacket = new UsbSetupPacket
            {
                Request = bRequest,
                Value = (short) (setupData & (uint)BasicMasks.SixteenBits),
                Index = (short) ((setupData & 0xFF0000)/0x10000),
                RequestType = (byte)UsbRequestType.TypeVendor,
                Length = 0
            };
            return UsbControlTransfer(setupPacket, data, data.Length);
        }

        #endregion

        #endregion

        #region EZ-USB firmware update
        private void ResetDevice(bool r)
        {
            ClearAllCachedCopiesOfRegisters();
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
            UsbControlTransfer(setupPacket, buffer, buffer.Length);
            System.Threading.Thread.Sleep(r ? 50 : 400); // give the firmware some time for initialization
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
                        int k = UsbControlTransfer(setupPacket, buffer, j);
                        if (k < 0 || k != j)
                        {
                            throw new ApplicationException();
                        }
                        System.Threading.Thread.Sleep(1); // to avoid package loss
                    }
                    j = 0;
                }

                if (i >= ihxFile.IhxData.Length || ihxFile.IhxData[i] < 0 || ihxFile.IhxData[i] > 255) continue;
                buffer[j] = (byte)ihxFile.IhxData[i];
                j += 1;
            }
            var endTime = DateTime.UtcNow;

            ResetDevice(false); //error (may caused re-numeration) can be ignored
            return (long)endTime.Subtract(startTime).TotalMilliseconds;
        }

        #endregion
    }
}