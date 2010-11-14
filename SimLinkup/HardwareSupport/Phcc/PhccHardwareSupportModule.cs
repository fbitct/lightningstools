using System;
using System.Collections.Generic;
using p = Phcc;
using Common.MacroProgramming;
using Common.HardwareSupport;
using Phcc.DeviceManager.Config;
using System.IO;
using log4net;
using System.Runtime.Remoting;
namespace SimLinkup.HardwareSupport.Phcc
{
    public class PhccHardwareSupportModule:HardwareSupportModuleBase, IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(PhccHardwareSupportModule));
        #endregion

        #region Instance variables
        private p.Device _device = null;
        private Motherboard _motherboard = null;
        private bool _isDisposed=false;
        private p.AnalogInputChangedEventHandler _analogInputChangedEventHandler=null;
        private p.DigitalInputChangedEventHandler _digitalInputChangedEventHandler = null;
        private p.I2CDataReceivedEventHandler _i2cDataReceivedEventHandler = null;
        private DigitalSignal[] _digitalInputSignals = null;
        private AnalogSignal[] _analogInputSignals = null;
        private DigitalSignal[] _digitalOutputSignals= null;
        private AnalogSignal[] _analogOutputSignals = null;
        private Dictionary<string, byte[]> _peripheralByteStates = new Dictionary<string, byte[]>();
        private Dictionary<string, double[]> _peripheralFloatStates = new Dictionary<string, double[]>();
        #endregion

        #region Constructors
        private PhccHardwareSupportModule()
            : base()
        {
        }
        private PhccHardwareSupportModule(Motherboard motherboard):this()
        {
            if (motherboard == null) throw new ArgumentNullException("motherboard");
            _motherboard = motherboard;
#if PHCC_COMMUNICATION_DISABLED
            _device = CreateDevice(motherboard.ComPort, false);
#else
            _device = CreateDevice(motherboard.ComPort, true);
#endif

            _analogInputSignals = CreateAnalogInputSignals(_device, motherboard.ComPort);
            _digitalInputSignals = CreateDigitalInputSignals(_device, motherboard.ComPort);
            CreateOutputSignals(_device, motherboard, out _digitalOutputSignals, out _analogOutputSignals, out _peripheralByteStates, out _peripheralFloatStates);

            CreateInputEventHandlers();
            RegisterForInputEvents(_device, _analogInputChangedEventHandler, _digitalInputChangedEventHandler, _i2cDataReceivedEventHandler);
#if PHCC_COMMUNICATION_DISABLED
#else
            SendCalibrations(_device, motherboard);
            StartTalking(_device);
#endif
        }

        public override string FriendlyName
        {
            get
            {
                return "PHCC";
            }
        }
        public static IHardwareSupportModule[] GetInstances()
        {
            List<IHardwareSupportModule> toReturn = new List<IHardwareSupportModule>();
            try
            {
                string hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "PhccHardwareSupportModule.config");
                PhccHardwareSupportModuleConfig hsmConfig = PhccHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                string phccDeviceManagerConfigFilePath = hsmConfig.PhccDeviceManagerConfigFilePath;
                ConfigurationManager phccConfigManager = LoadConfiguration(phccDeviceManagerConfigFilePath);
                foreach (Motherboard m in phccConfigManager.Motherboards)
                {
                    IHardwareSupportModule thisHsm = new PhccHardwareSupportModule(m);
                    toReturn.Add(thisHsm);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return toReturn.ToArray();
        }
        #endregion
        
        #region Device Communications
        private static p.Device CreateDevice(string comPort, bool openComPortNow)
        {
            p.Device device = new p.Device(comPort, openComPortNow);
            return device;
        }
        private static void SendCalibrations(p.Device device, Motherboard motherboard)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (motherboard== null) throw new ArgumentNullException("motherboard");
            foreach (var peripheral in motherboard.Peripherals)
            {
                if (peripheral is p.DeviceManager.Config.Doa8Servo)
                {
                    var servoConfig = peripheral as p.DeviceManager.Config.Doa8Servo;
                    if (servoConfig != null)
                    {
                        SendServoCalibrations(device, servoConfig);
                    }
                }
                else if (peripheral is p.DeviceManager.Config.DoaAnOut1)
                {
                    var anOut1Config = peripheral as p.DeviceManager.Config.DoaAnOut1;
                    if (anOut1Config != null)
                    {
                        SendAnOut1Calibrations(device, anOut1Config);
                    }
                }
            }
        }
        private static void SendAnOut1Calibrations(p.Device device, p.DeviceManager.Config.DoaAnOut1 anOut1Config)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (anOut1Config == null) throw new ArgumentNullException("anOut1Config");
            device.DoaSendAnOut1GainAllChannels(anOut1Config.Address, anOut1Config.GainAllChannels);
        }
        private static void SendServoCalibrations(p.Device device, p.DeviceManager.Config.Doa8Servo servoConfig) 
        {
            if (device == null) throw new ArgumentNullException("device");
            if (servoConfig == null) throw new ArgumentNullException("servoConfig");
            if (servoConfig.ServoCalibrations == null) throw new InvalidOperationException("ServoCalibrations not set on servoConfig parameter object");
            foreach (var calibration in servoConfig.ServoCalibrations)
            {
                device.DoaSend8ServoCalibration(servoConfig.Address, (byte)calibration.ServoNum, calibration.CalibrationOffset);
                device.DoaSend8ServoGain(servoConfig.Address, (byte)calibration.ServoNum, calibration.Gain);

            }
        }
        private static void StopTalking(p.Device device)
        {
            device.StopTalking();
        }
        private static void StartTalking(p.Device device)
        {
            device.StartTalking();
        }
        #endregion

        #region Input Event Handlers

        #region Input Event Handler Setup and Teardown
        private static void RegisterForInputEvents(p.Device device, p.AnalogInputChangedEventHandler analogInputChangedEventHandler, p.DigitalInputChangedEventHandler digitalInputChangedEventHandler, p.I2CDataReceivedEventHandler i2cDataReceivedEventHandler)
        {
            if (device == null) return;

            if (analogInputChangedEventHandler != null)
            {
                device.AnalogInputChanged += analogInputChangedEventHandler;
            }
            if (digitalInputChangedEventHandler != null)
            {
                device.DigitalInputChanged += digitalInputChangedEventHandler;
            }
            if (i2cDataReceivedEventHandler != null)
            {
                device.I2CDataReceived += i2cDataReceivedEventHandler;
            }
        }
        private static void UnregisterForInputEvents(p.Device device, p.AnalogInputChangedEventHandler analogInputChangedEventHandler, p.DigitalInputChangedEventHandler digitalInputChangedEventHandler, p.I2CDataReceivedEventHandler i2cDataReceivedEventHandler)
        {
            if (device == null) return;
            if (analogInputChangedEventHandler != null)
            {
                try
                {
                    device.AnalogInputChanged -= analogInputChangedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }
            if (digitalInputChangedEventHandler != null)
            {
                try
                {
                    device.DigitalInputChanged -= digitalInputChangedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }
            if (i2cDataReceivedEventHandler != null)
            {
                try
                {
                    device.I2CDataReceived -= i2cDataReceivedEventHandler;
                }
                catch (RemotingException ex)
                {
                }
            }
        }
        private void CreateInputEventHandlers()
        {
            _analogInputChangedEventHandler = new p.AnalogInputChangedEventHandler(device_AnalogInputChanged);
            _digitalInputChangedEventHandler = new p.DigitalInputChangedEventHandler(device_DigitalInputChanged);
            _i2cDataReceivedEventHandler = new p.I2CDataReceivedEventHandler(device_I2CDataReceived);
        }
        private void AbandonInputEventHandlers()
        {
            _analogInputChangedEventHandler = null;
            _digitalInputChangedEventHandler = null;
            _i2cDataReceivedEventHandler = null;
        }
        #endregion
        private void device_I2CDataReceived(object sender, global::Phcc.I2CDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void device_DigitalInputChanged(object sender, p.DigitalInputChangedEventArgs e)
        {
            if (_digitalInputSignals != null && _digitalInputSignals.Length > e.Index)
            {
                DigitalSignal signal = _digitalInputSignals[e.Index];
                signal.State = e.NewValue;
            }
        }
        private void device_AnalogInputChanged(object sender, p.AnalogInputChangedEventArgs e)
        {
            if (_analogInputSignals != null && _analogInputSignals.Length > e.Index)
            {
                AnalogSignal signal = _analogInputSignals[e.Index];
                signal.State = e.NewValue;
            }
        }



        #endregion

        #region Configuration
        private static p.DeviceManager.Config.ConfigurationManager LoadConfiguration(string phccConfigFile)
        {
            p.DeviceManager.Config.ConfigurationManager toReturn = ConfigurationManager.Load(phccConfigFile);
            return toReturn;
        }
        #endregion

        #region Virtual Method Implementations
        public override AnalogSignal[] AnalogInputs
        {
            get 
            {
                return _analogInputSignals;
            }
        }
        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return _digitalInputSignals;
            }
        }
        public override AnalogSignal[] AnalogOutputs
        {
            get
            {
                return _analogOutputSignals;
            }
        }
        public override DigitalSignal[] DigitalOutputs
        {
            get
            {
                return _digitalOutputSignals;
            }
        }
        #endregion

        #region Signals Handling
        #region Signal Creation
        private AnalogSignal[] CreateAnalogInputSignals(p.Device device, string portName)
        {
            List<AnalogSignal> toReturn = new List<AnalogSignal>();
            for (int i = 0; i < 35; i++)
            {
                AnalogSignal thisSignal = new AnalogSignal();
                thisSignal.CollectionName= "Analog Inputs";
                thisSignal.FriendlyName = string.Format("Analog Input {0}", string.Format("{0:0}", i + 1));
                thisSignal.Id = string.Format("PhccAnalogInput[{0}][{1}]", portName, i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceAddress = portName;
                thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                thisSignal.State = 0;
                toReturn.Add(thisSignal);
            }
            return toReturn.ToArray();
        }
        private DigitalSignal[] CreateDigitalInputSignals(p.Device device, string portName)
        {
            List<DigitalSignal> toReturn = new List<DigitalSignal>();
            for (int i = 0; i < 1024; i++)
            {
                DigitalSignal thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("Digital Input {0}", string.Format("{0:0}", i + 1));
                thisSignal.Id = string.Format("PhccDigitalInput[{0}][{1}]", portName, i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceAddress = portName;
                thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                thisSignal.State = false;
                toReturn.Add(thisSignal);
            }
            return toReturn.ToArray();
        }
        private void CreateOutputSignals(p.Device device, Motherboard motherboard, out DigitalSignal[] digitalSignals, out AnalogSignal[] analogSignals, out Dictionary<string, byte[]> peripheralByteStates, out Dictionary<string, double[]> peripheralFloatStates)
        {
            if (motherboard == null) throw new ArgumentNullException("motherboard");
            string portName = motherboard.ComPort;
            List<DigitalSignal> digitalSignalsToReturn= new List<DigitalSignal>();
            List<AnalogSignal> analogSignalsToReturn= new List<AnalogSignal>();
            List<p.DeviceManager.Config.Peripheral> attachedPeripherals = motherboard.Peripherals;
            peripheralByteStates = new Dictionary<string, byte[]>();
            peripheralFloatStates = new Dictionary<string, double[]>();
            foreach (p.DeviceManager.Config.Peripheral peripheral in attachedPeripherals)
            {
                if (peripheral is p.DeviceManager.Config.Doa40Do)
                {
                    p.DeviceManager.Config.Doa40Do typedPeripheral = peripheral as p.DeviceManager.Config.Doa40Do;
                    string baseAddress="0x" + typedPeripheral.Address.ToString("X4");
                    for (int i=0;i<40;i++) 
                    {
                        DigitalSignal thisSignal = new DigitalSignal();
                        thisSignal.CollectionName = "Digital Outputs";
                        thisSignal.FriendlyName = string.Format("Digital Output {0}", string.Format("{0:0}", i + 1));
                        thisSignal.Id = string.Format("DOA_40DO[{0}][{1}][{2}]", portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this; 
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_40DO @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_40DO @ {0}",baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.State = false;
                        thisSignal.SignalChanged += new DigitalSignal.SignalChangedEventHandler(DOA40DOOutputSignalChanged);
                        digitalSignalsToReturn.Add(thisSignal);
                    }
                    peripheralByteStates[baseAddress] = new byte[5];
                }
                else if (peripheral is p.DeviceManager.Config.Doa7Seg)
                {
                    p.DeviceManager.Config.Doa7Seg typedPeripheral = peripheral as p.DeviceManager.Config.Doa7Seg;
                    string baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (int i = 0; i < 32; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            DigitalSignal thisSignal = new DigitalSignal();
                            thisSignal.CollectionName = "Digital Outputs";
                            thisSignal.FriendlyName = string.Format("Display {0}, Output Line {1}", string.Format("{0:0}", j + 1), string.Format("{0:0}", i + 1));
                            thisSignal.Id = string.Format("DOA_7SEG[{0}][{1}][{2}][{3}]", portName,baseAddress, j, i);
                            thisSignal.Index = (i*8)+j;
                            thisSignal.PublisherObject = this; 
                            thisSignal.Source = device;
                            thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                            thisSignal.SourceAddress = portName;
                            thisSignal.SubSource = string.Format("DOA_7SEG @ {0}", baseAddress);
                            thisSignal.SubSourceFriendlyName = string.Format("DOA_7SEG @ {0}", baseAddress);
                            thisSignal.SubSourceAddress = baseAddress;
                            thisSignal.State = false;
                            thisSignal.SignalChanged += new DigitalSignal.SignalChangedEventHandler(DOA7SegOutputSignalChanged);
                            digitalSignalsToReturn.Add(thisSignal);
                        }
                    }
                    peripheralByteStates[baseAddress] = new byte[32];
                }
                else if (peripheral is p.DeviceManager.Config.Doa8Servo)
                {
                    p.DeviceManager.Config.Doa8Servo typedPeripheral = peripheral as p.DeviceManager.Config.Doa8Servo;
                    string baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (int i = 0; i < 8; i++)
                    {
                        AnalogSignal thisSignal = new AnalogSignal();
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i+1);
                        thisSignal.Id = string.Format("DOA_8SERVO[{0}][{1}][{2}]", portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this; 
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_8SERVO @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_8SERVO @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.State = 0;
                        thisSignal.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(DOA8ServoOutputSignalChanged);
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[8];
                }
                else if (peripheral is p.DeviceManager.Config.DoaAirCore)
                {
                    p.DeviceManager.Config.DoaAirCore typedPeripheral = peripheral as p.DeviceManager.Config.DoaAirCore;
                    string baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (int i = 0; i < 4; i++)
                    {
                        AnalogSignal thisSignal = new AnalogSignal();
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i+1);
                        thisSignal.Id = string.Format("DOA_AIRCORE[{0}][{1}][{2}]", portName,baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.Source= device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_AIRCORE @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_AIRCORE @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.State = 0;
                        thisSignal.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(DOAAircoreOutputSignalChanged);
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[4];
                }
                else if (peripheral is p.DeviceManager.Config.DoaAnOut1)
                {
                    p.DeviceManager.Config.DoaAnOut1 typedPeripheral = peripheral as p.DeviceManager.Config.DoaAnOut1;
                    string baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (int i = 0; i < 16; i++)
                    {
                        AnalogSignal thisSignal = new AnalogSignal();
                        thisSignal.CollectionName = "Analog Outputs";
                        thisSignal.FriendlyName = string.Format("Analog Output {0}", string.Format("{0:0}", i+1));
                        thisSignal.Id = string.Format("DOA_ANOUT1[{0}][{1}][{2}]", baseAddress,portName, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this; 
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_ANOUT1 @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_ANOUT1 @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;

                        thisSignal.State = 0;
                        thisSignal.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(DOAAnOut1SignalChanged);
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[16];
                }
                else if (peripheral is p.DeviceManager.Config.DoaStepper)
                {
                    p.DeviceManager.Config.DoaStepper typedPeripheral = peripheral as p.DeviceManager.Config.DoaStepper;
                    string baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (int i = 0; i < 4; i++)
                    {
                        AnalogSignal thisSignal = new AnalogSignal();
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i+1);
                        thisSignal.Id = string.Format("DOA_STEPPER[{0}][{1}][{2}]",portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this; 
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_STEPPER @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_STEPPER @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;

                        thisSignal.State = 0;
                        thisSignal.SignalChanged += new AnalogSignal.AnalogSignalChangedEventHandler(DOAStepperSignalChanged);
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[4];
                }
                else
                {
                }
            }
            analogSignals = analogSignalsToReturn.ToArray();
            digitalSignals = digitalSignalsToReturn.ToArray();
        }
        #endregion

        #region Signal Forwarding
        private void DOAStepperSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            AnalogSignal source = sender as AnalogSignal;
            if (source != null)
            {
                int motorNumZeroBase = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    int motorNum = motorNumZeroBase + 1;
                    double newPosition = args.CurrentState;
                    double oldPosition = _peripheralFloatStates[baseAddress][motorNumZeroBase];
                    int numSteps = (int)Math.Abs(newPosition - oldPosition);
                    p.MotorDirections direction = p.MotorDirections.Clockwise;
                    if (oldPosition < newPosition)
                    {
                        direction = p.MotorDirections.Counterclockwise;
                    }
                    device.DoaSendStepperMotor(baseAddressByte, (byte)motorNum, direction, (byte)numSteps, p.MotorStepTypes.FullStep);
                    _peripheralFloatStates[baseAddress][motorNumZeroBase]=newPosition;
                }
            }
        }
        private void DOAAnOut1SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            AnalogSignal source = sender as AnalogSignal;
            if (source != null)
            {
                int channelNumZeroBase = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    int channelNum = channelNumZeroBase + 1;
                    byte newVal = (byte)(Math.Abs(args.CurrentState) *255);
                    device.DoaSendAnOut1(baseAddressByte, (byte)channelNum, newVal);
                    _peripheralFloatStates[baseAddress][channelNum] = newVal;
                }
            }
        }
        private void DOAAircoreOutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            AnalogSignal source = sender as AnalogSignal;
            if (source != null)
            {
                int motorNumZeroBase = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    int motorNum = motorNumZeroBase + 1;
                    int newPosition = (int)(Math.Abs(args.CurrentState) * 1023);
                    device.DoaSendAirCoreMotor(baseAddressByte, (byte)motorNum, newPosition);
                    _peripheralFloatStates[baseAddress][motorNum] = newPosition;
                }
            }
        }
        private void DOA8ServoOutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            AnalogSignal source = sender as AnalogSignal;
            if (source != null)
            {
                int servoNumZeroBase = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    int servoNum = servoNumZeroBase + 1;
                    byte newPosition = (byte)(Math.Abs(args.CurrentState) * 255);
                    device.DoaSend8ServoPosition(baseAddressByte, (byte)servoNum, newPosition);
                    _peripheralFloatStates[baseAddress][servoNum] = newPosition;
                }
            }
            
            
        }
        private void DOA7SegOutputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            DigitalSignal source = sender as DigitalSignal;
            if (source != null)
            {
                int outputLineNum = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    bool newBitVal = args.CurrentState;
                    byte[] peripheralState = _peripheralByteStates[baseAddress];
                    int displayNumZeroBase = (int)(outputLineNum / 8);
                    int thisBitIndex = outputLineNum % 8;
                    int displayNum = displayNumZeroBase + 1;
                    byte currentBitsThisDisplay = peripheralState[displayNumZeroBase];
                    byte newBits = Common.Util.SetBit(currentBitsThisDisplay, thisBitIndex, newBitVal);
                    device.DoaSend7Seg(baseAddressByte, (byte)displayNum, newBits);
                    peripheralState[displayNumZeroBase] = newBits;
                }
            }
            
        }
        private void DOA40DOOutputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            DigitalSignal source = sender as DigitalSignal;
            if (source != null)
            {
                int outputNum = source.Index.Value;
                string baseAddress = source.SubSourceAddress;
                byte baseAddressByte = Byte.Parse(baseAddress);
                p.Device device = source.Source as p.Device;
                if (device != null)
                {
                    bool newBitVal = args.CurrentState;
                    byte[] peripheralState= _peripheralByteStates[baseAddress];
                    int connectorNumZeroBase = (int)(outputNum / 8);
                    int thisBitIndex = outputNum %8;
                    int connectorNum=connectorNumZeroBase+3;
                    byte currentBitsThisConnector = peripheralState[connectorNumZeroBase];
                    byte newBits = Common.Util.SetBit(currentBitsThisConnector, thisBitIndex, newBitVal);
                    device.DoaSend40DO(baseAddressByte, (byte)connectorNum, newBits);
                    peripheralState[connectorNumZeroBase] = newBits;
                }
            }
        }
        #endregion
        #endregion

        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object 
        /// is not manually disposed.  Ordinarily called only 
        /// by the garbage collector.
        /// </summary>
        ~PhccHardwareSupportModule()
        {
            Dispose();
        }
        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually
        /// perform disposal.  Distinguishes the private method signature 
        /// from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
#if PHCC_COMMUNICATION_DISABLED
#else
                    try 
                    {
                        StopTalking(_device);
                    }
                    catch (Exception ex) 
                    {
                    }
#endif
                    UnregisterForInputEvents(_device, _analogInputChangedEventHandler, _digitalInputChangedEventHandler, _i2cDataReceivedEventHandler);
                    AbandonInputEventHandlers();
                    Common.Util.DisposeObject(_device); //disconnect from the PHCC
                    _device =null;
                }
            }
            _isDisposed = true;

        }
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up 
        /// managed and unmanaged resources used by this 
        /// object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
