using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using Phcc.DeviceManager.Config;
using p = Phcc;

namespace SimLinkup.HardwareSupport.Phcc
{
    public class PhccHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (PhccHardwareSupportModule));

        #endregion

        #region Configuration

        private static ConfigurationManager LoadConfiguration(string phccConfigFile)
        {
            var toReturn = ConfigurationManager.Load(phccConfigFile);
            return toReturn;
        }

        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly AnalogSignal[] _analogOutputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private readonly DigitalSignal[] _digitalOutputSignals;
        private readonly Dictionary<string, byte[]> _peripheralByteStates = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, double[]> _peripheralFloatStates = new Dictionary<string, double[]>();
        private p.AnalogInputChangedEventHandler _analogInputChangedEventHandler;
        private p.Device _device;
        private p.DigitalInputChangedEventHandler _digitalInputChangedEventHandler;
        private p.I2CDataReceivedEventHandler _i2cDataReceivedEventHandler;
        private bool _isDisposed;
        private Motherboard _motherboard;

        #endregion

        #region Constructors

        private PhccHardwareSupportModule()
        {
        }

        private PhccHardwareSupportModule(Motherboard motherboard) : this()
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
            CreateOutputSignals(_device, motherboard, out _digitalOutputSignals, out _analogOutputSignals,
                out _peripheralByteStates, out _peripheralFloatStates);

            CreateInputEventHandlers();
            RegisterForInputEvents(_device, _analogInputChangedEventHandler, _digitalInputChangedEventHandler,
                _i2cDataReceivedEventHandler);
#if PHCC_COMMUNICATION_DISABLED
#else
            SendCalibrations(_device, motherboard);
            StartTalking(_device);
#endif
        }

        public override string FriendlyName
        {
            get { return "PHCC"; }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "PhccHardwareSupportModule.config");
                var hsmConfig = PhccHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                var phccDeviceManagerConfigFilePath = hsmConfig.PhccDeviceManagerConfigFilePath;
                var phccConfigManager = LoadConfiguration(phccDeviceManagerConfigFilePath);
                foreach (var m in phccConfigManager.Motherboards)
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
            var device = new p.Device(comPort, openComPortNow);
            return device;
        }

        private static void SendCalibrations(p.Device device, Motherboard motherboard)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (motherboard == null) throw new ArgumentNullException("motherboard");
            foreach (var peripheral in motherboard.Peripherals)
            {
                if (peripheral is Doa8Servo)
                {
                    var servoConfig = peripheral as Doa8Servo;
                    if (servoConfig != null)
                    {
                        SendServoCalibrations(device, servoConfig);
                    }
                }
                else if (peripheral is DoaAnOut1)
                {
                    var anOut1Config = peripheral as DoaAnOut1;
                    if (anOut1Config != null)
                    {
                        SendAnOut1Calibrations(device, anOut1Config);
                    }
                }
            }
        }

        private static void SendAnOut1Calibrations(p.Device device, DoaAnOut1 anOut1Config)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (anOut1Config == null) throw new ArgumentNullException("anOut1Config");
            device.DoaSendAnOut1GainAllChannels(anOut1Config.Address, anOut1Config.GainAllChannels);
        }

        private static void SendServoCalibrations(p.Device device, Doa8Servo servoConfig)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (servoConfig == null) throw new ArgumentNullException("servoConfig");
            if (servoConfig.ServoCalibrations == null)
                throw new InvalidOperationException("ServoCalibrations not set on servoConfig parameter object");
            foreach (var calibration in servoConfig.ServoCalibrations)
            {
                device.DoaSend8ServoCalibration(servoConfig.Address, (byte) calibration.ServoNum,
                    calibration.CalibrationOffset);
                device.DoaSend8ServoGain(servoConfig.Address, (byte) calibration.ServoNum, calibration.Gain);
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

        private static void RegisterForInputEvents(p.Device device,
            p.AnalogInputChangedEventHandler analogInputChangedEventHandler,
            p.DigitalInputChangedEventHandler digitalInputChangedEventHandler,
            p.I2CDataReceivedEventHandler i2cDataReceivedEventHandler)
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

        private static void UnregisterForInputEvents(p.Device device,
            p.AnalogInputChangedEventHandler analogInputChangedEventHandler,
            p.DigitalInputChangedEventHandler digitalInputChangedEventHandler,
            p.I2CDataReceivedEventHandler i2cDataReceivedEventHandler)
        {
            if (device == null) return;
            if (analogInputChangedEventHandler != null)
            {
                try
                {
                    device.AnalogInputChanged -= analogInputChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (digitalInputChangedEventHandler != null)
            {
                try
                {
                    device.DigitalInputChanged -= digitalInputChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (i2cDataReceivedEventHandler != null)
            {
                try
                {
                    device.I2CDataReceived -= i2cDataReceivedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
        }

        private void CreateInputEventHandlers()
        {
            _analogInputChangedEventHandler = device_AnalogInputChanged;
            _digitalInputChangedEventHandler = device_DigitalInputChanged;
            _i2cDataReceivedEventHandler = device_I2CDataReceived;
        }

        private void AbandonInputEventHandlers()
        {
            _analogInputChangedEventHandler = null;
            _digitalInputChangedEventHandler = null;
            _i2cDataReceivedEventHandler = null;
        }

        #endregion

        private void device_I2CDataReceived(object sender, p.I2CDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void device_DigitalInputChanged(object sender, p.DigitalInputChangedEventArgs e)
        {
            if (_digitalInputSignals != null && _digitalInputSignals.Length > e.Index)
            {
                var signal = _digitalInputSignals[e.Index];
                signal.State = e.NewValue;
            }
        }

        private void device_AnalogInputChanged(object sender, p.AnalogInputChangedEventArgs e)
        {
            if (_analogInputSignals != null && _analogInputSignals.Length > e.Index)
            {
                var signal = _analogInputSignals[e.Index];
                signal.State = e.NewValue;
            }
        }

        #endregion

        #region Virtual Method Implementations

        public override AnalogSignal[] AnalogInputs
        {
            get { return _analogInputSignals; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return _digitalInputSignals; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return _analogOutputSignals; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return _digitalOutputSignals; }
        }

        #endregion

        #region Signals Handling

        #region Signal Creation

        private AnalogSignal[] CreateAnalogInputSignals(p.Device device, string portName)
        {
            var toReturn = new List<AnalogSignal>();
            for (var i = 0; i < 35; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.Category = "Inputs";
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("Analog Input {0}", string.Format("{0:0}", i + 1));
                thisSignal.Id = string.Format("PhccAnalogInput[{0}][{1}]", portName, i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = device;
                thisSignal.SourceAddress = portName;
                thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                thisSignal.State = 0;
                thisSignal.MinValue = 0;
                thisSignal.MaxValue = 1023;
                toReturn.Add(thisSignal);
            }
            return toReturn.ToArray();
        }

        private DigitalSignal[] CreateDigitalInputSignals(p.Device device, string portName)
        {
            var toReturn = new List<DigitalSignal>();
            for (var i = 0; i < 1024; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.Category = "Inputs";
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

        private void CreateOutputSignals(p.Device device, Motherboard motherboard, out DigitalSignal[] digitalSignals,
            out AnalogSignal[] analogSignals,
            out Dictionary<string, byte[]> peripheralByteStates,
            out Dictionary<string, double[]> peripheralFloatStates)
        {
            if (motherboard == null) throw new ArgumentNullException("motherboard");
            var portName = motherboard.ComPort;
            var digitalSignalsToReturn = new List<DigitalSignal>();
            var analogSignalsToReturn = new List<AnalogSignal>();
            var attachedPeripherals = motherboard.Peripherals;
            peripheralByteStates = new Dictionary<string, byte[]>();
            peripheralFloatStates = new Dictionary<string, double[]>();
            foreach (var peripheral in attachedPeripherals)
            {
                if (peripheral is Doa40Do)
                {
                    var typedPeripheral = peripheral as Doa40Do;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 40; i++)
                    {
                        var thisSignal = new DigitalSignal();
                        thisSignal.Category = "Outputs";
                        thisSignal.CollectionName = "Digital Outputs";
                        thisSignal.FriendlyName = string.Format("Digital Output {0}", string.Format("{0:0}", i + 1));
                        thisSignal.Id = string.Format("DOA_40DO[{0}][{1}][{2}]", portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this;
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_40DO @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_40DO @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.State = false;
                        thisSignal.SignalChanged += DOA40DOOutputSignalChanged;
                        digitalSignalsToReturn.Add(thisSignal);
                    }
                    peripheralByteStates[baseAddress] = new byte[5];
                }
                else if (peripheral is Doa7Seg)
                {
                    var typedPeripheral = peripheral as Doa7Seg;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 32; i++)
                    {
                        for (var j = 0; j < 8; j++)
                        {
                            var thisSignal = new DigitalSignal();
                            thisSignal.Category = "Outputs";
                            thisSignal.CollectionName = "Digital Outputs";
                            thisSignal.FriendlyName = string.Format("Display {0}, Output Line {1}",
                                string.Format("{0:0}", j + 1),
                                string.Format("{0:0}", i + 1));
                            thisSignal.Id = string.Format("DOA_7SEG[{0}][{1}][{2}][{3}]", portName, baseAddress, j, i);
                            thisSignal.Index = (i*8) + j;
                            thisSignal.PublisherObject = this;
                            thisSignal.Source = device;
                            thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                            thisSignal.SourceAddress = portName;
                            thisSignal.SubSource = string.Format("DOA_7SEG @ {0}", baseAddress);
                            thisSignal.SubSourceFriendlyName = string.Format("DOA_7SEG @ {0}", baseAddress);
                            thisSignal.SubSourceAddress = baseAddress;
                            thisSignal.State = false;
                            thisSignal.SignalChanged += DOA7SegOutputSignalChanged;
                            digitalSignalsToReturn.Add(thisSignal);
                        }
                    }
                    peripheralByteStates[baseAddress] = new byte[32];
                }
                else if (peripheral is Doa8Servo)
                {
                    var typedPeripheral = peripheral as Doa8Servo;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 8; i++)
                    {
                        var thisSignal = new AnalogSignal();
                        thisSignal.Category = "Outputs";
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i + 1);
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
                        thisSignal.IsAngle = true;
                        thisSignal.MinValue = -90;
                        thisSignal.MaxValue = 90;
                        thisSignal.SignalChanged += DOA8ServoOutputSignalChanged;
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[8];
                }
                else if (peripheral is DoaAirCore)
                {
                    var typedPeripheral = peripheral as DoaAirCore;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 4; i++)
                    {
                        var thisSignal = new AnalogSignal();
                        thisSignal.Category = "Outputs";
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i + 1);
                        thisSignal.Id = string.Format("DOA_AIRCORE[{0}][{1}][{2}]", portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_AIRCORE @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_AIRCORE @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.State = 0;
                        thisSignal.SignalChanged += DOAAircoreOutputSignalChanged;
                        thisSignal.MinValue = 0;
                        thisSignal.MaxValue = 360;
                        thisSignal.IsAngle = true;
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[4];
                }
                else if (peripheral is DoaAnOut1)
                {
                    var typedPeripheral = peripheral as DoaAnOut1;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 16; i++)
                    {
                        var thisSignal = new AnalogSignal();
                        thisSignal.Category = "Outputs";
                        thisSignal.CollectionName = "Analog Outputs";
                        thisSignal.FriendlyName = string.Format("Analog Output {0}", string.Format("{0:0}", i + 1));
                        thisSignal.Id = string.Format("DOA_ANOUT1[{0}][{1}][{2}]", baseAddress, portName, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this;
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_ANOUT1 @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_ANOUT1 @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;
                        thisSignal.MinValue = 0;
                        thisSignal.MaxValue = 5;
                        thisSignal.IsVoltage = true;
                        thisSignal.State = 0;
                        thisSignal.SignalChanged += DOAAnOut1SignalChanged;
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[16];
                }
                else if (peripheral is DoaStepper)
                {
                    var typedPeripheral = peripheral as DoaStepper;
                    var baseAddress = "0x" + typedPeripheral.Address.ToString("X4");
                    for (var i = 0; i < 4; i++)
                    {
                        var thisSignal = new AnalogSignal();
                        thisSignal.Category = "Outputs";
                        thisSignal.CollectionName = "Motor Outputs";
                        thisSignal.FriendlyName = string.Format("Motor {0}", i + 1);
                        thisSignal.Id = string.Format("DOA_STEPPER[{0}][{1}][{2}]", portName, baseAddress, i);
                        thisSignal.Index = i;
                        thisSignal.PublisherObject = this;
                        thisSignal.Source = device;
                        thisSignal.SourceFriendlyName = string.Format("PHCC Device on {0}", portName);
                        thisSignal.SourceAddress = portName;
                        thisSignal.SubSource = string.Format("DOA_STEPPER @ {0}", baseAddress);
                        thisSignal.SubSourceFriendlyName = string.Format("DOA_STEPPER @ {0}", baseAddress);
                        thisSignal.SubSourceAddress = baseAddress;

                        thisSignal.State = 0;
                        thisSignal.SignalChanged += DOAStepperSignalChanged;
                        analogSignalsToReturn.Add(thisSignal);
                    }
                    peripheralFloatStates[baseAddress] = new double[4];
                }
            }
            analogSignals = analogSignalsToReturn.ToArray();
            digitalSignals = digitalSignalsToReturn.ToArray();
        }

        #endregion

        #region Signal Forwarding

        private void DOAStepperSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            var source = sender as AnalogSignal;
            if (source != null)
            {
                var motorNumZeroBase = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var motorNum = motorNumZeroBase + 1;
                    var newPosition = args.CurrentState;
                    var oldPosition = _peripheralFloatStates[baseAddress][motorNumZeroBase];
                    var numSteps = (int) Math.Abs(newPosition - oldPosition);
                    var direction = p.MotorDirections.Clockwise;
                    if (oldPosition < newPosition)
                    {
                        direction = p.MotorDirections.Counterclockwise;
                    }
                    device.DoaSendStepperMotor(baseAddressByte, (byte) motorNum, direction, (byte) numSteps,
                        p.MotorStepTypes.FullStep);
                    _peripheralFloatStates[baseAddress][motorNumZeroBase] = newPosition;
                }
            }
        }

        private void DOAAnOut1SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            var source = sender as AnalogSignal;
            if (source != null)
            {
                var channelNumZeroBase = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var channelNum = channelNumZeroBase + 1;
                    var newVal = (byte)(((Math.Abs(args.CurrentState)) / 5.00 )* 255);
                    device.DoaSendAnOut1(baseAddressByte, (byte) channelNum, newVal);
                    _peripheralFloatStates[baseAddress][channelNum] = newVal;
                }
            }
        }

        private void DOAAircoreOutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            var source = sender as AnalogSignal;
            if (source != null)
            {
                var motorNumZeroBase = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var motorNum = motorNumZeroBase + 1;
                    var newPosition = (int) (((Math.Abs(args.CurrentState))/360.00)*1023);
                    device.DoaSendAirCoreMotor(baseAddressByte, (byte) motorNum, newPosition);
                    _peripheralFloatStates[baseAddress][motorNum] = newPosition;
                }
            }
        }

        private void DOA8ServoOutputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            var source = sender as AnalogSignal;
            if (source != null)
            {
                var servoNumZeroBase = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var servoNum = servoNumZeroBase + 1;
                    var newPosition = (byte) ((Math.Abs(args.CurrentState +90.00)/180.00)*255);
                    device.DoaSend8ServoPosition(baseAddressByte, (byte) servoNum, newPosition);
                    _peripheralFloatStates[baseAddress][servoNum] = newPosition;
                }
            }
        }

        private void DOA7SegOutputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            var source = sender as DigitalSignal;
            if (source != null)
            {
                var outputLineNum = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var newBitVal = args.CurrentState;
                    var peripheralState = _peripheralByteStates[baseAddress];
                    var displayNumZeroBase = (outputLineNum/8);
                    var thisBitIndex = outputLineNum%8;
                    var displayNum = displayNumZeroBase + 1;
                    var currentBitsThisDisplay = peripheralState[displayNumZeroBase];
                    var newBits = Common.Util.SetBit(currentBitsThisDisplay, thisBitIndex, newBitVal);
                    device.DoaSend7Seg(baseAddressByte, (byte) displayNum, newBits);
                    peripheralState[displayNumZeroBase] = newBits;
                }
            }
        }

        private void DOA40DOOutputSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            var source = sender as DigitalSignal;
            if (source != null)
            {
                var outputNum = source.Index.Value;
                var baseAddress = source.SubSourceAddress;
                var baseAddressByte = Byte.Parse(baseAddress);
                var device = source.Source as p.Device;
                if (device != null)
                {
                    var newBitVal = args.CurrentState;
                    var peripheralState = _peripheralByteStates[baseAddress];
                    var connectorNumZeroBase = (outputNum/8);
                    var thisBitIndex = outputNum%8;
                    var connectorNum = connectorNumZeroBase + 3;
                    var currentBitsThisConnector = peripheralState[connectorNumZeroBase];
                    var newBits = Common.Util.SetBit(currentBitsThisConnector, thisBitIndex, newBitVal);
                    device.DoaSend40DO(baseAddressByte, (byte) connectorNum, newBits);
                    peripheralState[connectorNumZeroBase] = newBits;
                }
            }
        }

        #endregion

        #endregion

        #region Destructors

        /// <summary>
        ///     Public implementation of IDisposable.Dispose().  Cleans up
        ///     managed and unmanaged resources used by this
        ///     object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Standard finalizer, which will call Dispose() if this object
        ///     is not manually disposed.  Ordinarily called only
        ///     by the garbage collector.
        /// </summary>
        ~PhccHardwareSupportModule()
        {
            Dispose();
        }

        /// <summary>
        ///     Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">
        ///     flag to indicate if we should actually
        ///     perform disposal.  Distinguishes the private method signature
        ///     from the public signature.
        /// </param>
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
                    catch (Exception)
                    {
                    }
#endif
                    UnregisterForInputEvents(_device, _analogInputChangedEventHandler, _digitalInputChangedEventHandler,
                        _i2cDataReceivedEventHandler);
                    AbandonInputEventHandlers();
                    Common.Util.DisposeObject(_device); //disconnect from the PHCC
                    _device = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}