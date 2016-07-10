using AnalogDevices;
using LibUsbDotNet;
using System.Threading;

namespace SimLinkup.HardwareSupport.AnalogDevices
{
    internal class SimulatedAnalogDevicesDenseDacEvalBoard : IDenseDacEvalBoard
    {
        private ChannelMonitorOptions _channelMonitorOptions = new ChannelMonitorOptions(channelMonitorSource: ChannelMonitorSource.None, channelNumberOrInputPinNumber: 0);
        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get
            {
                SimulateSendingUSBCommand();
                return _channelMonitorOptions;
            }

            set
            {
                _channelMonitorOptions = value;
                SimulateSendingUSBCommand();
            }
        }

        public DacPrecision DacPrecision
        {
            get; set;
        }

        private IODirection _generalPurposeIOPinDirection = IODirection.Output;
        public IODirection GeneralPurposeIOPinDirection
        {
            get
            {
                SimulateSendingUSBCommand();
                return _generalPurposeIOPinDirection;
            }

            set
            {
                _generalPurposeIOPinDirection = value;
                SimulateSendingUSBCommand();
            }
        }

        private bool _generalPurposeIOPinState = false;
        public bool GeneralPurposeIOPinState
        {
            get
            {
                SimulateSendingUSBCommand();
                return _generalPurposeIOPinState;
            }

            set
            {
                SimulateSendingUSBCommand();
                _generalPurposeIOPinState = value;
            }
        }

        private bool _isOverTemperature = false;
        public bool IsOverTemperature
        {
            get
            {
                SimulateSendingUSBCommand();
                return _isOverTemperature;
            }
            internal set
            {
                _isOverTemperature = value;
            }
        }

        private bool _isThermalShutdownEnabled = false;
        public bool IsThermalShutdownEnabled
        {
            get
            {
                SimulateSendingUSBCommand();
                return _isThermalShutdownEnabled;
            }

            set
            {
                SimulateSendingUSBCommand();
                _isThermalShutdownEnabled = value;
            }
        }

        private ushort _offsetDAC0 = 1555;
        public ushort OffsetDAC0
        {
            get
            {
                SimulateSendingUSBCommand();
                return _offsetDAC0;
            }

            set
            {
                SimulateSendingUSBCommand();
                _offsetDAC0 = value;
            }
        }

        private ushort _offsetDAC1 = 1555;
        public ushort OffsetDAC1
        {
            get
            {
                SimulateSendingUSBCommand();
                return _offsetDAC1;
            }

            set
            {
                SimulateSendingUSBCommand();
                _offsetDAC1 = value;
            }
        }


        private ushort _offsetDAC2 = 1555;
        public ushort OffsetDAC2
        {
            get
            {
                SimulateSendingUSBCommand();
                return _offsetDAC2;
            }

            set
            {
                SimulateSendingUSBCommand();
                _offsetDAC2 = value;
            }
        }

        private bool _packetErrorCheckErrorOccurred = false;
        public bool PacketErrorCheckErrorOccurred
        {
            get
            {
                SimulateSendingUSBCommand();
                return _packetErrorCheckErrorOccurred;
            }
            internal set
            {
                _packetErrorCheckErrorOccurred = value;
            }
        }

        public string SymbolicName
        {
            get;
            internal set;
        }

        public UsbDevice UsbDevice
        {
            get;
            internal set;
        }

        private DacChannelDataSource[] _dacChannelDataSource = new DacChannelDataSource[40];
        public DacChannelDataSource GetDacChannelDataSource(ChannelAddress channelAddress)
        {
            SimulateSendingUSBCommand();
            return _dacChannelDataSource[(int)channelAddress - 8];
        }

        private ushort[] _dacChannelDataValueA = new ushort[40];
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            SimulateSendingUSBCommand();
            return _dacChannelDataValueA[(int)channel - 8];
        }

        private ushort[] _dacChannelDataValueB = new ushort[40];
        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            SimulateSendingUSBCommand();
            return _dacChannelDataValueB[(int)channel - 8];
        }

        private ushort[] _dacChannelGain = new ushort[40];
        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            SimulateSendingUSBCommand();
            return _dacChannelGain[(int)channel - 8];
        }

        private ushort[] _dacChannelOffset = new ushort[40];
        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            SimulateSendingUSBCommand();
            return _dacChannelOffset[(int)channel - 8];
        }

        public void PerformSoftPowerDown()
        {
            SimulateSendingUSBCommand();
        }

        public void PerformSoftPowerUp()
        {
            SimulateSendingUSBCommand();
        }

        public void PulseLDacPin()
        {
            SimulateSendingUSBCommand();
        }

        public void Reset()
        {
            Thread.Sleep(1000);
        }

        public void ResumeAllDacOutputs()
        {
            SimulateSendingUSBCommand();
        }

        public void SetDacChannelDataSource(ChannelAddress channelAddress, DacChannelDataSource value)
        {
            SimulateSendingUSBCommand();
            _dacChannelDataSource[(int)channelAddress - 8] = value;
        }

        public void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 8; i++)
            {
                _dacChannelDataSource[((int)group * 8)] = channel0;
                _dacChannelDataSource[((int)group * 8) + 1] = channel1;
                _dacChannelDataSource[((int)group * 8) + 2] = channel2;
                _dacChannelDataSource[((int)group * 8) + 3] = channel3;
                _dacChannelDataSource[((int)group * 8) + 4] = channel4;
                _dacChannelDataSource[((int)group * 8) + 5] = channel5;
                _dacChannelDataSource[((int)group * 8) + 6] = channel6;
                _dacChannelDataSource[((int)group * 8) + 7] = channel7;
            }
        }

        public void SetDacChannelDataSourceAllChannels(DacChannelDataSource source)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataSource[i] = source;
            }
        }

        public void SetDacChannelDataValueA(ChannelAddress channelAddress, ushort newVal)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataValueA[(int)channelAddress - 8] = newVal;
            }
        }

        public void SetDacChannelDataValueB(ChannelAddress channels, ushort newVal)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataValueB[(int)channels - 8] = newVal;
            }
        }

        public void SetDacChannelGain(ChannelAddress channels, ushort newVal)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 40; i++)
            {
                _dacChannelGain[(int)channels - 8] = newVal;
            }
        }

        public void SetDacChannelOffset(ChannelAddress channels, ushort newVal)
        {
            SimulateSendingUSBCommand();
            for (int i = 0; i < 40; i++)
            {
                _dacChannelOffset[(int)channels - 8] = newVal;
            }
        }

        public void SetLDacPinHigh()
        {
            SimulateSendingUSBCommand();
        }

        public void SetLDacPinLow()
        {
            SimulateSendingUSBCommand();
        }

        public void SuspendAllDacOutputs()
        {
            SimulateSendingUSBCommand();
        }

        public void UpdateAllDacOutputs()
        {
            SimulateSendingUSBCommand();
        }

        public long UploadFirmware(IhxFile ihxFile)
        {
            Thread.Sleep(1000);
            return 0;
        }

        private void SimulateSendingUSBCommand()
        {
            Thread.Sleep(5);
        }
    }
}
