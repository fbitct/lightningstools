using System;
using AnalogDevices;
using LibUsbDotNet;
using System.Threading;

namespace SimLinkup.HardwareSupport.AnalogDevices
{
    internal class SimulatedAnalogDevicesDenseDacEvalBoard : IDenseDacEvalBoard
    {
        private ChannelMonitorOptions _channelMonitorOptions=new ChannelMonitorOptions(channelMonitorSource=ChannelMonitorSource.None, channelNumberOrInputPinNumber=0);
        public ChannelMonitorOptions ChannelMonitorOptions
        {
            get
            {
                Thread.Sleep(100);
                return _channelMonitorOptions;
            }

            set
            {
                _channelMonitorOptions = value;
                Thread.Sleep(100);
            }
        }

        public DacPrecision DacPrecision
        {
            get;set;
        }

        private IODirection _generalPurposeIOPinDirection = IODirection.Output;
        public IODirection GeneralPurposeIOPinDirection
        {
            get
            {
                Thread.Sleep(100);
                return _generalPurposeIOPinDirection;
            }

            set
            {
                _generalPurposeIOPinDirection = value;
                Thread.Sleep(100);
            }
        }

        private bool _generalPurposeIOPinState = false;
        public bool GeneralPurposeIOPinState
        {
            get
            {
                Thread.Sleep(100);
                return _generalPurposeIOPinState;
            }

            set
            {
                Thread.Sleep(100);
                _generalPurposeIOPinState = value;
            }
        }

        private bool _isOverTemperature=false;
        public bool IsOverTemperature
        {
            get
            {
                Thread.Sleep(100);
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
                Thread.Sleep(100);
                return _isThermalShutdownEnabled;
            }

            set
            {
                Thread.Sleep(100);
                _isThermalShutdownEnabled = value;
            }
        }

        private ushort _offsetDAC0 = 1555;
        public ushort OffsetDAC0
        {
            get
            {
                Thread.Sleep(100);
                return _offsetDAC0;
            }

            set
            {
                Thread.Sleep(100);
                _offsetDAC0 = value;
            }
        }

        private ushort _offsetDAC1 = 1555;
        public ushort OffsetDAC1
        {
            get
            {
                Thread.Sleep(100);
                return _offsetDAC1;
            }

            set
            {
                Thread.Sleep(100);
                _offsetDAC1 = value;
            }
        }


        private ushort _offsetDAC2 = 1555;
        public ushort OffsetDAC2
        {
            get
            {
                Thread.Sleep(100);
                return _offsetDAC2;
            }

            set
            {
                Thread.Sleep(100);
                _offsetDAC2 = value;
            }
        }

        private bool _packetErrorCheckErrorOccurred = false;
        public bool PacketErrorCheckErrorOccurred
        {
            get
            {
                Thread.Sleep(100);
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
            Thread.Sleep(100);
            return _dacChannelDataSource[(int)channelAddress - 8];
        }

        private ushort[] _dacChannelDataValueA = new ushort[40];
        public ushort GetDacChannelDataValueA(ChannelAddress channel)
        {
            Thread.Sleep(100);
            return _dacChannelDataValueA[(int)channel - 8];
        }

        private ushort[] _dacChannelDataValueB = new ushort[40];
        public ushort GetDacChannelDataValueB(ChannelAddress channel)
        {
            Thread.Sleep(100);
            return _dacChannelDataValueB[(int)channel - 8];
        }

        private ushort[] _dacChannelGain = new ushort[40];
        public ushort GetDacChannelGain(ChannelAddress channel)
        {
            Thread.Sleep(100);
            return _dacChannelGain[(int)channel - 8];
        }

        private ushort[] _dacChannelOffset= new ushort[40];
        public ushort GetDacChannelOffset(ChannelAddress channel)
        {
            Thread.Sleep(100);
            return _dacChannelOffset[(int)channel - 8];
        }

        public void PerformSoftPowerDown()
        {
            Thread.Sleep(100);
        }

        public void PerformSoftPowerUp()
        {
            Thread.Sleep(100);
        }

        public void PulseLDacPin()
        {
            Thread.Sleep(100);
        }

        public void Reset()
        {
            Thread.Sleep(1000);
        }

        public void ResumeAllDacOutputs()
        {
            Thread.Sleep(100);
        }

        public void SetDacChannelDataSource(ChannelAddress channelAddress, DacChannelDataSource value)
        {
            Thread.Sleep(100);
            _dacChannelDataSource[(int)channelAddress - 8] = value;
        }

        public void SetDacChannelDataSource(ChannelGroup group, DacChannelDataSource channel0, DacChannelDataSource channel1, DacChannelDataSource channel2, DacChannelDataSource channel3, DacChannelDataSource channel4, DacChannelDataSource channel5, DacChannelDataSource channel6, DacChannelDataSource channel7)
        {
            Thread.Sleep(100);
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
            Thread.Sleep(100);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataSource[i] = source;
            }
        }

        public void SetDacChannelDataValueA(ChannelAddress channelAddress, ushort newVal)
        {
            Thread.Sleep(100);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataValueA[(int)channelAddress-8] = newVal;
            }
        }

        public void SetDacChannelDataValueB(ChannelAddress channels, ushort newVal)
        {
            Thread.Sleep(100);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelDataValueB[(int)channels - 8] = newVal;
            }
        }

        public void SetDacChannelGain(ChannelAddress channels, ushort newVal)
        {
            Thread.Sleep(100);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelGain[(int)channels - 8] = newVal;
            }
        }

        public void SetDacChannelOffset(ChannelAddress channels, ushort newVal)
        {
            Thread.Sleep(100);
            for (int i = 0; i < 40; i++)
            {
                _dacChannelOffset[(int)channels - 8] = newVal;
            }
        }

        public void SetLDacPinHigh()
        {
            Thread.Sleep(100);
        }

        public void SetLDacPinLow()
        {
            Thread.Sleep(100);
        }

        public void SuspendAllDacOutputs()
        {
            Thread.Sleep(100);
        }

        public void UpdateAllDacOutputs()
        {
            Thread.Sleep(100);
        }

        public long UploadFirmware(IhxFile ihxFile)
        {
            Thread.Sleep(1000);
            return 0;
        }

    }
}
