using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using log4net;

namespace AnalogDevices
{
    public class DenseDacEvalBoard
    {
        #region Public stuff

        #region Class Constructors

        private static ILog _log = LogManager.GetLogger(typeof (DenseDacEvalBoard));

        public DenseDacEvalBoard(UsbDevice device)
        {
            //lock (_instanceStateLock)
            //{
            _usbDevice = device;
            //}
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
            get { return _usbDevice.UsbRegistryInfo.SymbolicName; }
        }

        public bool GeneralPurposeIOPinState
        {
            get { return (ReadbackGPIORegister() & 0x01) == 0x01; }
            set
            {
                if (GeneralPurposeIOPinDirection != IODirection.Output)
                {
                    throw new InvalidOperationException(
                        "GeneralPurposeIOPinDirection must be set to Output in order to set the GeneralPurposeIOPinState property.");
                }
                if (value)
                {
                    SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, 0x03);
                }
                else
                {
                    SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, 0x02);
                }
            }
        }

        public IODirection GeneralPurposeIOPinDirection
        {
            get
            {
                var gpioRegisterVal = ReadbackGPIORegister();
                var gpioIsOutput = ((gpioRegisterVal & 0x02) == 0x02);
                if (gpioIsOutput)
                {
                    return IODirection.Output;
                }
                else
                {
                    return IODirection.Input;
                }
            }
            set
            {
                switch (value)
                {
                    case IODirection.Output:
                        SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, 0x03);
                        break;
                    case IODirection.Input:
                        SendSpecialFunction(SpecialFunctionCode.GPIOConfigureAndWrite, 0x00);
                        break;
                }
            }
        }

        public ushort Group0Offset
        {
            get { return ReadbackOSF0Register(); }
            set { WriteOSF0Register(value); }
        }

        public ushort Group1Offset
        {
            get { return ReadbackOSF1Register(); }
            set { WriteOSF1Register(value); }
        }

        public ushort Groups2Thru4Offset
        {
            get { return ReadbackOSF2Register(); }
            set { WriteOSF2Register(value); }
        }

        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get
            {
                var source = ChannelMonitorSource.None;
                byte channelNumberOrInputPinNumber = 0;
                //lock (_instanceStateLock)
                //{
                if ((_monitorFlags & 0x20) == 0x20)
                {
                    if ((_monitorFlags & 0x10) == 0x10) //if input pin monitoring is on
                    {
                        source = ChannelMonitorSource.InputPin;
                        if ((_monitorFlags & 0x01) == 0x01) //if input pin 1 is being monitored
                        {
                            channelNumberOrInputPinNumber = 1;
                        }
                        else
                        {
                            channelNumberOrInputPinNumber = 0; //else input pin 0 is being monitored
                        }
                    }
                    else //Dac monitoring is on
                    {
                        source = ChannelMonitorSource.DacChannel;
                        channelNumberOrInputPinNumber = (byte) (_monitorFlags & 0x0F);
                    }
                }
                else
                {
                    //source already == ChannelMonitorSources.None
                }
                //}
                var toReturn = new ChannelMonitorOptions(source, channelNumberOrInputPinNumber);
                toReturn.PropertyChanged += MonitorOptionsPropertyChangedHandler;
                return toReturn;
            }
            set { SendNewChannelMonitorOptionsToDevice(value); }
        }

        public bool IsTemperatureShutdownEnabled
        {
            get
            {
                ushort controlRegisterVal = ReadbackControlRegister();
                return ((controlRegisterVal & 0x04) == 0x04);
            }
            set
            {
                ushort controlRegisterVal = ReadbackControlRegister();
                if (value)
                {
                    controlRegisterVal |= 0x04;
                }
                else
                {
                    controlRegisterVal &= 0xFB;
                }
                WriteControlRegister(controlRegisterVal);
            }
        }

        public DacPrecision DacPrecision
        {
            get
            {
                //lock (_instanceStateLock)
                //{
                return _thisDevicePrecision;
                //}
            }
            set
            {
                //lock (_instanceStateLock)
                //{
                _thisDevicePrecision = value;
                //}
            }
        }

        public bool PECErrorOccurred
        {
            get
            {
                ushort controlRegisterVal = ReadbackControlRegister();
                return (controlRegisterVal & 0x08) == 0x08;
            }
        }

        public bool IsOverTemperature
        {
            get
            {
                ushort controlRegisterVal = ReadbackControlRegister();
                return (controlRegisterVal & 0x10) == 0x10;
            }
        }

        #endregion

        #region Dac Channel Data Source Selection

        public void SetDacChannelDataSource(ChannelAddress channel, DacChannelDataSource value)
        {
            if ((int) channel < 8 || (int) channel > 47)
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
            var channelNum = (byte) ((byte) channel - 8);
            byte currentSourceSelections = 0x00;
            var code = SpecialFunctionCode.NOP;
            if (channelNum >= 0 && channelNum < 8)
            {
                code = SpecialFunctionCode.WriteToABSelectRegister0;
                currentSourceSelections = ReadbackABSelect0Register();
            }
            else if (channelNum >= 8 && channelNum < 16)
            {
                code = SpecialFunctionCode.WriteToABSelectRegister1;
                currentSourceSelections = ReadbackABSelect1Register();
            }
            else if (channelNum >= 16 && channelNum < 24)
            {
                code = SpecialFunctionCode.WriteToABSelectRegister2;
                currentSourceSelections = ReadbackABSelect2Register();
            }
            else if (channelNum >= 24 && channelNum < 32)
            {
                code = SpecialFunctionCode.WriteToABSelectRegister3;
                currentSourceSelections = ReadbackABSelect3Register();
            }
            else if (channelNum >= 32 && channelNum <= 39)
            {
                code = SpecialFunctionCode.WriteToABSelectRegister4;
                currentSourceSelections = ReadbackABSelect4Register();
            }
            var toSend = currentSourceSelections;

            var channelOffset = (byte) (channelNum%8);
            var channelMask = (byte) (1 << channelOffset);
            if (value == DacChannelDataSource.DataValueA)
            {
                toSend &= (byte) (~channelMask);
            }
            else
            {
                toSend |= channelMask;
            }
            toSend &= 0xFF;
            SendSpecialFunction(code, toSend);
        }

        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channel)
        {
            if ((int) channel < 8 || (int) channel > 47)
            {
                throw new ArgumentOutOfRangeException("channel");
            }
            var channelNum = (byte) ((byte) channel - 8);
            byte currentSourceSelections = 0x00;
            if (channelNum >= 0 && channelNum < 8)
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
            var channelOffset = (byte) (channelNum%8);
            var channelMask = (byte) (1 << channelOffset);
            var sourceIsB = ((currentSourceSelections & channelMask) == channelMask);
            if (sourceIsB)
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
            byte toSend = 0x00;
            if (channel0 == DacChannelDataSource.DataValueB) toSend |= 0x01;
            if (channel1 == DacChannelDataSource.DataValueB) toSend |= 0x02;
            if (channel2 == DacChannelDataSource.DataValueB) toSend |= 0x03;
            if (channel3 == DacChannelDataSource.DataValueB) toSend |= 0x04;
            if (channel4 == DacChannelDataSource.DataValueB) toSend |= 0x05;
            if (channel5 == DacChannelDataSource.DataValueB) toSend |= 0x06;
            if (channel6 == DacChannelDataSource.DataValueB) toSend |= 0x07;
            if (channel7 == DacChannelDataSource.DataValueB) toSend |= 0x08;

            var code = SpecialFunctionCode.NOP;
            switch (group)
            {
                case ChannelGroup.Group0:
                    code = SpecialFunctionCode.WriteToABSelectRegister0;
                    break;
                case ChannelGroup.Group1:
                    code = SpecialFunctionCode.WriteToABSelectRegister1;
                    break;
                case ChannelGroup.Group2:
                    code = SpecialFunctionCode.WriteToABSelectRegister2;
                    break;
                case ChannelGroup.Group3:
                    code = SpecialFunctionCode.WriteToABSelectRegister3;
                    break;
                case ChannelGroup.Group4:
                    code = SpecialFunctionCode.WriteToABSelectRegister4;
                    break;
                default:
                    break;
            }
            toSend &= 0xFF;
            SendSpecialFunction(code, toSend);
        }

        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            switch (source)
            {
                case DacChannelDataSource.DataValueA:
                    SendSpecialFunction(SpecialFunctionCode.BlockWriteABSelectRegisters, 0x00);
                    break;
                case DacChannelDataSource.DataValueB:
                    SendSpecialFunction(SpecialFunctionCode.BlockWriteABSelectRegisters, 0xFF);
                    break;
                default:
                    throw new ArgumentException("source");
            }
        }

        #endregion

        #region Device Control Functions

        public void PerformSoftPowerDown()
        {
            ushort controlRegisterVal = ReadbackControlRegister();
            controlRegisterVal |= 0x01;
            WriteControlRegister(controlRegisterVal);
        }

        public void PerformSoftPowerUp()
        {
            ushort controlRegisterVal = ReadbackControlRegister();
            controlRegisterVal &= 0xFE;
            WriteControlRegister(controlRegisterVal);
        }

        public void Reset()
        {
            SetRESETPinHigh();
            Thread.Sleep(50);
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
            else
            {
                throw new ArgumentOutOfRangeException("channel");
            }
        }

        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackX1BRegister(channel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("channel");
            }
        }

        public void SetDacChannelDataValueA(ChannelAddress channels, ushort newVal)
        {
            ushort controlRegisterVal = ReadbackControlRegister();
            controlRegisterVal &= 0xFFFB;
            WriteControlRegister(controlRegisterVal);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI(0xC00000 | (uint) (((byte) channels & 0x3F) << 16) | newVal);
            }
            else
            {
                SendSPI((0xC00000 | (uint) (((byte) channels & 0x3F) << 16) | (uint) ((newVal & 0x3FFF) << 2)));
            }
        }

        public void SetDacChannelDataValueB(ChannelAddress channels, ushort newVal)
        {
            ushort controlRegisterVal = ReadbackControlRegister();
            controlRegisterVal |= 4;
            WriteControlRegister(controlRegisterVal);
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI((0xC00000 | (uint) (((byte) channels & 0x3F) << 16) | newVal));
            }
            else
            {
                SendSPI((0xC00000 | (uint) (((byte) channels & 0x3F) << 16) | (uint) ((newVal & 0x3FFF) << 2)));
            }
        }

        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackCRegister(channel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("channel");
            }
        }

        public void SetDacChannelOffset(ChannelAddress channels, ushort newVal)
        {
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI((0x800000 | (uint) (((byte) channels & 0x3F) << 16) | newVal));
            }
            else
            {
                SendSPI((0x800000 | (uint) (((byte) channels & 0x3F) << 16) | (uint) ((newVal & 0x3FFF) << 2)));
            }
        }

        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            if ((int) channel >= 8 && (int) channel <= 47)
            {
                return ReadbackMRegister(channel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("channel");
            }
        }

        public void SetDacChannelGain(ChannelAddress channels, ushort newVal)
        {
            if (DacPrecision == DacPrecision.SixteenBit)
            {
                SendSPI((0x400000 | (uint) (((byte) channels & 0x3F) << 16) | newVal));
            }
            else
            {
                SendSPI((0x400000 | (uint) (((byte) channels & 0x3F) << 16) | (uint) ((newVal & 0x3FFF) << 2)));
            }
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
            return toReturn.ToArray();
        }

        #endregion

        #endregion

        #region Private stuff

        #region Private Enums

        #region Nested type: DeviceCommand

        private enum DeviceCommand : byte
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

        #endregion

        #region Nested type: SpecialFunctionCode

        private enum SpecialFunctionCode
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

        #endregion

        #endregion

        #region Instance Variables

        private readonly byte[] _emptyBuf = new byte[0];
        private readonly UsbDevice _usbDevice;
        private object _instanceStateLock = new object();
        private byte _monitorFlags;
        private bool _spiInitialized;
        private DacPrecision _thisDevicePrecision = DacPrecision.SixteenBit;

        #endregion

        #region Channel Monitoring Options Change Handling

        private void SendNewChannelMonitorOptionsToDevice(ChannelMonitorOptions value)
        {
            if (value == null) throw new ArgumentNullException("value");

            //lock (_instanceStateLock)
            //{
            if (value.ChannelMonitorSource == ChannelMonitorSource.None)
            {
                _monitorFlags &= 0xDF;
            }
            else
            {
                _monitorFlags |= 0x20;
                if (value.ChannelMonitorSource == ChannelMonitorSource.InputPin)
                {
                    if (value.ChannelNumberOrInputPinNumber == 0)
                    {
                        _monitorFlags |= 0x18;
                        _monitorFlags &= 0xF0;
                    }
                    else if (value.ChannelNumberOrInputPinNumber == 1)
                    {
                        _monitorFlags |= 0x18;
                        _monitorFlags &= 0xF0;
                        _monitorFlags |= (1);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value",
                                                              "value.ChannelNumberOrInputPinNumber is out of range with respect to ChannelMonitorSource.");
                    }
                }
                else if (value.ChannelMonitorSource == ChannelMonitorSource.DacChannel)
                {
                    _monitorFlags |= 0x20;
                    _monitorFlags &= 0xE0;
                    _monitorFlags |= (byte) (value.ChannelNumberOrInputPinNumber & 0x0F);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "value.ChannelMonitorSource is not valid.");
                }
            }
            SendSpecialFunction(SpecialFunctionCode.ConfigureMonitoring, _monitorFlags);
            //}
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
                                (ushort) ((((byte) channelNum) & 0x3F) << 7));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= 0x3FFF;
            }
            return val;
        }

        private ushort ReadbackX1BRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) (0x2000 | (ushort) ((((byte) channelNum) & 0x3F) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= 0x3FFF;
            }
            return val;
        }

        private ushort ReadbackCRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) (0x4000 | (ushort) ((((byte) channelNum) & 0x3F) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= 0x3FFF;
            }
            return val;
        }

        private ushort ReadbackMRegister(ChannelAddress channelNum)
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback,
                                (ushort) (0x6000 | (ushort) ((((byte) channelNum) & 0x3F) << 7)));
            var val = ReadSPI();
            if (DacPrecision == DacPrecision.FourteenBit)
            {
                val &= 0x3FFF;
            }
            return val;
        }

        private byte ReadbackControlRegister()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, (0x8080));
            return (byte) (ReadSPI() & 0x1F);
        }

        private ushort ReadbackOSF0Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8100);
            return (ushort) (ReadSPI() & 0x3FFF);
        }

        private ushort ReadbackOSF1Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8180);
            return (ushort) (ReadSPI() & 0x3FFF);
        }

        private ushort ReadbackOSF2Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8200);
            return (ushort) (ReadSPI() & 0x3FFF);
        }

        private byte ReadbackABSelect0Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8300);
            return (byte) (ReadSPI() & 0xFF);
        }

        private byte ReadbackABSelect1Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8380);
            return (byte) (ReadSPI() & 0xFF);
        }

        private byte ReadbackABSelect2Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8400);
            return (byte) (ReadSPI() & 0xFF);
        }

        private byte ReadbackABSelect3Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8480);
            return (byte) (ReadSPI() & 0xFF);
        }

        private byte ReadbackABSelect4Register()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8500);
            return (byte) (ReadSPI() & 0xFF);
        }

        private byte ReadbackGPIORegister()
        {
            SendSpecialFunction(SpecialFunctionCode.SelectRegisterForReadback, 0x8580);
            return (byte) ((ReadSPI() & 0x03));
        }

        #endregion

        #region Register Writing Functions

        private void WriteControlRegister(ushort newVal)
        {
            newVal &= 0x07;
            SendSpecialFunction(SpecialFunctionCode.WriteControlRegister, newVal);
        }

        private void WriteOSF0Register(ushort newVal)
        {
            newVal &= 0x3FFF;
            SendSpecialFunction(SpecialFunctionCode.WriteOSF0Register, newVal);
        }

        private void WriteOSF1Register(ushort newVal)
        {
            newVal &= 0x3FFF;
            SendSpecialFunction(SpecialFunctionCode.WriteOSF1Register, newVal);
        }

        private void WriteOSF2Register(ushort newVal)
        {
            newVal &= 0x3FFF;
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

        private void PulseLDacPin()
        {
            SendDeviceCommand(DeviceCommand.PulseLDacPin, 0);
        }

        private void SetLDacPinLow()
        {
            SendDeviceCommand(DeviceCommand.SetLDacPinLow, 0);
        }

        private void SetLDacPinHigh()
        {
            SendDeviceCommand(DeviceCommand.SetLDacPinHigh, 0);
        }

        private void InitializeSPIPins()
        {
            //lock (_instanceStateLock)
            //{
            SendDeviceCommand(DeviceCommand.InitializeSPIPins, 0);
            _spiInitialized = true;
            //}
        }

        #endregion

        #region Device Communications

        private void SendSpecialFunction(SpecialFunctionCode specialFunction, ushort data)
        {
            SendSPI((uint) ((((byte) specialFunction & 0x3F) << 16) | data));
        }

        private int SendSPI(UInt32 data)
        {
            return SendDeviceCommand(DeviceCommand.SendSPI, data);
        }

        private ushort ReadSPI()
        {
            //lock (_instanceStateLock)
            //{
            if (!_spiInitialized)
            {
                InitializeSPIPins();
            }
            //}
            var bRequest = (byte) DeviceCommand.SendSPI;

            ushort len = 3;
            var buf = new byte[len];
            var setupPacket = new UsbSetupPacket();
            setupPacket.Request = (DeviceRequestType) bRequest;
            setupPacket.Index = 0;
            setupPacket.RequestType = UsbRequestType.TypeVendor | UsbRequestType.EndpointIn;
            setupPacket.Length = (short) len;
            setupPacket.Value = 0;
            var lengthTransferred = 0;
            UsbControlTransfer(ref setupPacket, buf, buf.Length, out lengthTransferred);
            return (ushort) (buf[0] + (ushort) (buf[1]*256));
        }

        private void UsbControlTransfer(ref UsbSetupPacket setupPacket, object buffer, int bufferLength,
                                        out int lengthTransferred)
        {
            //lock (_usbDevice)
            //{
            _usbDevice.ControlTransfer(ref setupPacket, buffer, bufferLength, out lengthTransferred);
            //}
        }

        private int SendDeviceCommand(DeviceCommand command, UInt32 setupData)
        {
            return SendDeviceCommand(command, setupData, _emptyBuf);
        }

        private int SendDeviceCommand(DeviceCommand command, UInt32 setupData, byte[] data)
        {
            //lock (_instanceStateLock)
            //{
            if (!_spiInitialized && command != DeviceCommand.InitializeSPIPins) InitializeSPIPins();
            //}
            var bRequest = (byte) command;
            var setupPacket = new UsbSetupPacket();
            setupPacket.Request = (DeviceRequestType) bRequest;
            setupPacket.Value = (short) (setupData & 0xFFFF);
            setupPacket.Index = (short) ((setupData & 0xFF0000)/0x10000);
            setupPacket.RequestType = UsbRequestType.TypeVendor;
            setupPacket.Length = 0;
            var lengthTransferred = 0;
            UsbControlTransfer(ref setupPacket, data, data.Length, out lengthTransferred);
            return lengthTransferred;
        }

        #endregion

        #endregion

        #region EZ-USB firmware update

        private void ResetDevice(bool r)
        {
            byte[] buffer = {(byte) (r ? 1 : 0)};
            var setupPacket = new UsbSetupPacket();
            setupPacket.RequestType = UsbRequestType.TypeVendor;
            setupPacket.Request = (DeviceRequestType) 0xA0;
            unchecked
            {
                setupPacket.Value = (short) 0xE600;
            }
            setupPacket.Index = 0;
            var lengthTransferred = 0;
            UsbControlTransfer(ref setupPacket, buffer, buffer.Length, out lengthTransferred);
            Thread.Sleep(r ? 50 : 400); // give the firmware some time for initialization
        }

        public long UploadFirmware(IhxFile ihxFile)
        {
            const int transactionBytes = 256;
            var buffer = new byte[transactionBytes];

            ResetDevice(true); // reset = 1

            var t0 = DateTime.Now;
            var j = 0;
            for (var i = 0; i <= ihxFile.IhxData.Length; i++)
            {
                if (i >= ihxFile.IhxData.Length || ihxFile.IhxData[i] < 0 || j >= transactionBytes)
                {
                    if (j > 0)
                    {
                        var setupPacket = new UsbSetupPacket();
                        setupPacket.RequestType = UsbRequestType.TypeVendor;
                        setupPacket.Request = (DeviceRequestType) 0xA0;
                        setupPacket.Value = (short) (i - j);
                        setupPacket.Index = 0;
                        var k = 0;
                        UsbControlTransfer(ref setupPacket, buffer, j, out k);
                        if (k < 0 || k != j)
                        {
                            throw new ApplicationException();
                        }
                        Thread.Sleep(1); // to avoid package loss
                    }
                    j = 0;
                }

                if (i < ihxFile.IhxData.Length && ihxFile.IhxData[i] >= 0 && ihxFile.IhxData[i] <= 255)
                {
                    buffer[j] = (byte) ihxFile.IhxData[i];
                    j += 1;
                }
            }
            var t1 = DateTime.Now;

            ResetDevice(false); //error (may caused re-numeration) can be ignored
            return (long) t1.Subtract(t0).TotalMilliseconds;
        }

        #endregion
    }
}