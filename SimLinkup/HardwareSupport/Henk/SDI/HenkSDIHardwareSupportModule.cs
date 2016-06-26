using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using SDIDriver = SDI;
using System.Linq;
using System.Globalization;

namespace SimLinkup.HardwareSupport.Henk.SDI
{
    //Henk Synchro Drive Interface Hardware Support Module
    public class HenkSDIHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(HenkSDIHardwareSupportModule));

        #endregion

        #region Instance variables

        private bool _isDisposed;
        private SDIDriver.Device _sdiDevice;
        private byte _deviceAddress;
        private DeviceConfig _deviceConfig;
        private AnalogSignal _positionInputSignal;
        private List<AnalogSignal> _inputSignalsForPwmOutputChannels = new List<AnalogSignal>();
        private List<DigitalSignal> _inputSignalsForDigitalOutputChannels = new List<DigitalSignal>();

        #endregion

        #region Constructors

        private HenkSDIHardwareSupportModule(DeviceConfig deviceConfig)
        {
            _deviceConfig = deviceConfig;
            if (_deviceConfig != null)
            {
                ConfigureDevice();
                CreateInputSignals();
                RegisterForInputEvents();
            }

        }

        public override string FriendlyName
        {
            get
            {
                return string.Format("Henk Synchro Drive Interface: 0x{0} {1} on {2} [ {3} ]", 
                    _deviceAddress.ToString("X").PadLeft(2, '0'),
                    string.IsNullOrWhiteSpace(DeviceFunction) ? string.Empty: string.Format("(\"{0}\")", DeviceFunction), 
                    _deviceConfig.ConnectionType.HasValue 
                        ? _deviceConfig.ConnectionType.Value.ToString() 
                        : "UNKNOWN", 
                    _deviceConfig.COMPort ?? "<UNKNOWN>");
            }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();

            try
            {
                var hsmConfigFilePath = Path.Combine(Util.ApplicationDirectory, "henksdi.config");
                var hsmConfig = HenkSDIHardwareSupportModuleConfig.Load(hsmConfigFilePath);
                if (hsmConfig != null)
                {
                    foreach (var deviceConfiguration in hsmConfig.Devices)
                    {
                        var hsmInstance = new HenkSDIHardwareSupportModule(deviceConfiguration);
                        toReturn.Add(hsmInstance);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }

            return toReturn.ToArray();
        }

        #endregion

        #region Virtual Method Implementations

        public override AnalogSignal[] AnalogInputs
        {
            get
            {
                return _inputSignalsForPwmOutputChannels
                            .Union(new[] { _positionInputSignal })
                            .OrderBy(x => x.FriendlyName)
                            .ToArray();
            }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get
            {
                return _inputSignalsForDigitalOutputChannels
                            .OrderBy(x => x.FriendlyName)
                            .ToArray();
            }
        }
        public override AnalogSignal[] AnalogOutputs { get { return null; } }
        public override DigitalSignal[] DigitalOutputs { get { return null; } }

        #endregion

        #region Signals Handling

        #region Signals Event Handling

        private void RegisterForInputEvents()
        {

            if (_positionInputSignal != null)
            {
                _positionInputSignal.SignalChanged += PositionInputSignal_SignalChanged;
            }
            foreach (var digitalSignal in _inputSignalsForDigitalOutputChannels)
            {
                digitalSignal.SignalChanged += InputSignalForDigitalOutputChannel_SignalChanged;
            }
            foreach (var analogSignal in _inputSignalsForPwmOutputChannels)
            {
                analogSignal.SignalChanged += InputSignalForPWMOutputChannel_SignalChanged;
            }

        }

        private void UnregisterForInputEvents()
        {

            if (_positionInputSignal != null)
            {
                try
                {
                    _positionInputSignal.SignalChanged -= PositionInputSignal_SignalChanged;
                }
                catch (RemotingException) { }
            }
            foreach (var digitalSignal in _inputSignalsForDigitalOutputChannels)
            {
                try
                {
                    digitalSignal.SignalChanged -= InputSignalForDigitalOutputChannel_SignalChanged;
                }
                catch (RemotingException) { }
            }
            foreach (var analogSignal in _inputSignalsForPwmOutputChannels)
            {
                try
                {
                    analogSignal.SignalChanged -= InputSignalForPWMOutputChannel_SignalChanged;
                }
                catch (RemotingException) { }
            }
        }

        #endregion

        #region Signal Creation

        private void CreateInputSignals()
        {
            _positionInputSignal = CreatePositionInputSignal();
            _inputSignalsForDigitalOutputChannels = CreateInputSignalsForDigitalOutputChannels();
            _inputSignalsForPwmOutputChannels = CreateInputSignalsForPWMOutputChannels();
        }

        private AnalogSignal CreatePositionInputSignal()
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Inputs";
            thisSignal.CollectionName = "Synchro Control";
            thisSignal.FriendlyName = "Synchro Position (Degrees)";
            thisSignal.Id = string.Format("HenkSDI[{0}]__Synchro_Position", _deviceAddress.ToString("X").PadLeft(3, '0'));
            thisSignal.Index = 0;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsAngle = true;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 360;
            return thisSignal;
        }
        private List<DigitalSignal> CreateInputSignalsForDigitalOutputChannels()
        {
            return AllOutputChannels.Where(x => OutputChannelMode(x) == SDIDriver.OutputChannelMode.Digital)
                .Select(x => CreateInputSignalForOutputChannelConfiguredAsDigital(ChannelNumber(x)))
                .ToList();
        }
        private List<AnalogSignal> CreateInputSignalsForPWMOutputChannels()
        {
            return AllOutputChannels.Where(x => OutputChannelMode(x) == SDIDriver.OutputChannelMode.PWM)
                .Select(x => CreateInputSignalForOutputChannelConfiguredAsPWM(ChannelNumber(x)))
                .ToList();
        }
        private AnalogSignal CreateInputSignalForOutputChannelConfiguredAsPWM(int channelNumber)
        {
            var thisSignal = new AnalogSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital/PWM Output Channels";
            if (channelNumber < 8)
            {
                thisSignal.FriendlyName = string.Format("DIG_PWM_{0} ({1})", channelNumber, "PWM");
                thisSignal.Id = string.Format("HenkSDI[{0}]__DIG_PWM_{1}", channelNumber, _deviceAddress.ToString("X").PadLeft(2, '0'));
            }
            else
            {
                thisSignal.FriendlyName = string.Format("PWM_OUT (PWM)");
                thisSignal.Id = string.Format("HenkSDI[{0}]__PWM_OUT", _deviceAddress.ToString("X").PadLeft(2, '0'));
            }

            thisSignal.Index = channelNumber;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = 0;
            thisSignal.IsPercentage = true;
            thisSignal.MinValue = 0;
            thisSignal.MaxValue = 1;
            return thisSignal;
        }
        private DigitalSignal CreateInputSignalForOutputChannelConfiguredAsDigital(int channelNumber)
        {
            var thisSignal = new DigitalSignal();
            thisSignal.Category = "Outputs";
            thisSignal.CollectionName = "Digital/PWM Output Channels";
            thisSignal.FriendlyName = string.Format("DIG_PWM_{0} ({1})", channelNumber, "Digital");
            thisSignal.Id = string.Format("HenkSDI[{0}]__DIG_PWM_{1}", channelNumber, _deviceAddress.ToString("X").PadLeft(2, '0'));
            thisSignal.Index = channelNumber;
            thisSignal.Source = this;
            thisSignal.SourceFriendlyName = FriendlyName;
            thisSignal.SourceAddress = null;
            thisSignal.State = false;
            return thisSignal;
        }
        private static SDIDriver.OutputChannels[] AllOutputChannels
        {
            get
            {
                return new[]
                {
                    SDIDriver.OutputChannels.DIG_PWM_1,
                    SDIDriver.OutputChannels.DIG_PWM_2,
                    SDIDriver.OutputChannels.DIG_PWM_3,
                    SDIDriver.OutputChannels.DIG_PWM_4,
                    SDIDriver.OutputChannels.DIG_PWM_5,
                    SDIDriver.OutputChannels.DIG_PWM_6,
                    SDIDriver.OutputChannels.DIG_PWM_7,
                    SDIDriver.OutputChannels.PWM_OUT,
                };
            }
        }
        private static int ChannelNumber(SDIDriver.OutputChannels outputChannel)
        {
            int channelNumber = 0;
            var lastCharOfChannelName = outputChannel.ToString().Substring(outputChannel.ToString().Length - 1, 1);
            bool isNumeric = int.TryParse(lastCharOfChannelName, out channelNumber);
            return channelNumber > 0 ? channelNumber : 8;
        }
        private static SDIDriver.OutputChannels OutputChannel(int? channelNumber)
        {
            if (!channelNumber.HasValue) return SDIDriver.OutputChannels.Unknown;
            if (channelNumber.Value == 1) return SDIDriver.OutputChannels.DIG_PWM_1;
            if (channelNumber.Value == 2) return SDIDriver.OutputChannels.DIG_PWM_2;
            if (channelNumber.Value == 3) return SDIDriver.OutputChannels.DIG_PWM_3;
            if (channelNumber.Value == 4) return SDIDriver.OutputChannels.DIG_PWM_4;
            if (channelNumber.Value == 5) return SDIDriver.OutputChannels.DIG_PWM_5;
            if (channelNumber.Value == 6) return SDIDriver.OutputChannels.DIG_PWM_6;
            if (channelNumber.Value == 7) return SDIDriver.OutputChannels.DIG_PWM_7;
            if (channelNumber.Value == 8) return SDIDriver.OutputChannels.PWM_OUT;
            return SDIDriver.OutputChannels.Unknown;
        }
        private SDIDriver.OutputChannelMode OutputChannelMode(SDIDriver.OutputChannels outputChannel)
        {
            switch (outputChannel)
            {
                case SDIDriver.OutputChannels.DIG_PWM_1:
                    return _deviceConfig !=null && _deviceConfig.OutputChannelsConfig !=null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_1 !=null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_1.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_1.Mode.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_2:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_2 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_2.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_2.Mode.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_3:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_3 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_3.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_3.Mode.Value
                                : DeviceFunction =="PITCH"  //HORIZONTAL ILS COMMAND BAR
                                    ? SDIDriver.OutputChannelMode.PWM
                                    : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_4:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_4 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_4.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_4.Mode.Value
                                : DeviceFunction == "PITCH" //VERTICAL ILS COMMAND BAR
                                    ? SDIDriver.OutputChannelMode.PWM 
                                    : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_5:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_5 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_5.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_5.Mode.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_6:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_6 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_6.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_6.Mode.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_7:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null && 
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_7 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_7.Mode.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_7.Mode.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.PWM_OUT:
                    return SDIDriver.OutputChannelMode.PWM;
            }
            return SDIDriver.OutputChannelMode.Digital;
        }
        private byte OutputChannelInitialValue(SDIDriver.OutputChannels outputChannel)
        {
            switch (outputChannel)
            {
                case SDIDriver.OutputChannels.DIG_PWM_1:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_1 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_1.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_1.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_2:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_2 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_2.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_2.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_3:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_3 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_3.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_3.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_4:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_4 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_4.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_4.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_5:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_5 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_5.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_5.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_6:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_6 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_6.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_6.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.DIG_PWM_7:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_7 != null &&
                            _deviceConfig.OutputChannelsConfig.DIG_PWM_7.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.DIG_PWM_7.InitialValue.Value
                                : (byte)0;
                case SDIDriver.OutputChannels.PWM_OUT:
                    return _deviceConfig != null && _deviceConfig.OutputChannelsConfig != null &&
                            _deviceConfig.OutputChannelsConfig.PWM_OUT != null &&
                            _deviceConfig.OutputChannelsConfig.PWM_OUT.InitialValue.HasValue
                                ? _deviceConfig.OutputChannelsConfig.PWM_OUT.InitialValue.Value
                                : DeviceFunction == "ROLL" 
                                    ? (byte)128 //center the Rate-of-Turn indicator by default if no user-supplied initial values are found in config file
                                    : (byte)0;
            }
            return (byte)0;
        }
        private void PositionInputSignal_SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            if (_positionInputSignal != null)
            {
                var requestedPosition = _positionInputSignal.State;
                if (requestedPosition >= 0 && requestedPosition <= 255)
                {
                    _sdiDevice.MoveIndicatorFine(SDIDriver.Quadrant.One, (byte)requestedPosition);
                }
                else if (requestedPosition >= 256 && requestedPosition <= 511)
                {
                    _sdiDevice.MoveIndicatorFine(SDIDriver.Quadrant.Two, (byte)(requestedPosition - 256));
                }
                else if (requestedPosition >= 512 && requestedPosition <= 767)
                {
                    _sdiDevice.MoveIndicatorFine(SDIDriver.Quadrant.Three, (byte)(requestedPosition - 512));
                }
                else if (requestedPosition >= 768 && requestedPosition <= 1023)
                {
                    _sdiDevice.MoveIndicatorFine(SDIDriver.Quadrant.Four, (byte)(requestedPosition - 768));
                }
            }
        }
        private void InputSignalForDigitalOutputChannel_SignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            var signal = (DigitalSignal)sender;
            var channelNumber = signal.Index;
            var outputChannel = OutputChannel(channelNumber);
            _sdiDevice.SetOutputChannelValue(outputChannel, args.CurrentState == true ? byte.MaxValue : byte.MinValue);
        }

        private void InputSignalForPWMOutputChannel_SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            var signal = (AnalogSignal)sender;
            var channelNumber = signal.Index;
            var outputChannel = OutputChannel(channelNumber);
            _sdiDevice.SetOutputChannelValue(outputChannel, (byte)(args.CurrentState * byte.MaxValue));
        }

        #endregion

        #endregion

        #region Device Configuration

        private void ConfigureDevice()
        {
            ConfigureDeviceConnection();
            ConfigurePowerDown();
            ConfigureMovementLimits();
            ConfigureStatorBaseAngles();
            ConfigureOutputChannels();
            ConfigureUpdateRateControl();
        }
        private void ConfigureDeviceConnection()
        {
            try
            {
                if (
                    _deviceConfig != null &&
                    _deviceConfig.ConnectionType.HasValue &&
                    _deviceConfig.ConnectionType.Value == SDIDriver.ConnectionType.USB &&
                    !string.IsNullOrWhiteSpace(_deviceConfig.COMPort)
                )
                {
                    ConfigureUSBConnection();
                }
                else if (
                    _deviceConfig != null &&
                    _deviceConfig.ConnectionType.HasValue &&
                    _deviceConfig.ConnectionType.Value == SDIDriver.ConnectionType.PHCC &&
                    !string.IsNullOrWhiteSpace(_deviceConfig.COMPort) &&
                    !string.IsNullOrWhiteSpace(_deviceConfig.Address)
                )
                {
                    ConfigurePhccConnection();
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }

        private void ConfigureUSBConnection()
        {
            if (
                _deviceConfig == null ||
                !_deviceConfig.ConnectionType.HasValue ||
                _deviceConfig.ConnectionType.Value != SDIDriver.ConnectionType.USB &&
                string.IsNullOrWhiteSpace(_deviceConfig.COMPort)
            )
            {
                return;
            }

            try
            {
                var comPort = _deviceConfig.COMPort;
                _sdiDevice = new SDIDriver.Device(COMPort: comPort);
                byte addressByte = 0x00;
                string addressString = (_deviceConfig.Address ?? "").ToLowerInvariant().Replace("0x",string.Empty).Trim();
                bool addressIsValid = byte.TryParse(addressString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addressByte);
                if (!addressIsValid) return;
                _deviceAddress = addressByte;
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }

        private void ConfigurePhccConnection()
        {
            if (
                _deviceConfig == null ||
                !_deviceConfig.ConnectionType.HasValue ||
                _deviceConfig.ConnectionType.Value != SDIDriver.ConnectionType.PHCC ||
                string.IsNullOrWhiteSpace(_deviceConfig.COMPort) ||
                string.IsNullOrWhiteSpace(_deviceConfig.Address
            )
            )
            {
                return;
            }
            byte addressByte = 0x00;
            string addressString = (_deviceConfig.Address ?? "").ToLowerInvariant().Replace("0x", string.Empty).Trim();
            bool addressIsValid = byte.TryParse(addressString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addressByte);
            if (!addressIsValid) return;

            try
            {
                var comPort = (_deviceConfig.COMPort ?? "").Trim();
                var phccDevice = new global::Phcc.Device(portName: comPort, openPort: false);
                _sdiDevice = new SDIDriver.Device(phccDevice: phccDevice, address: addressByte);
                _deviceAddress = addressByte;

            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            
        }
        private string DeviceFunction
        {
            get
            {
                if (_deviceAddress == 0x30 || _deviceAddress == 0x48)
                {
                    return "PITCH";
                }
                else if (_deviceAddress == 0x32 || _deviceAddress == 0x50)
                {
                    return "ROLL";
                }

                return string.Empty;
            }
        }
        private void ConfigurePowerDown()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.PowerDownConfig == null) return;

            _sdiDevice.ConfigurePowerDown(
                powerDownState: _deviceConfig.PowerDownConfig.Enabled.HasValue && _deviceConfig.PowerDownConfig.Enabled.Value == true ? SDIDriver.PowerDownState.Enabled : SDIDriver.PowerDownState.Disabled,
                powerDownLevel: _deviceConfig.PowerDownConfig.Level.HasValue ? _deviceConfig.PowerDownConfig.Level.Value : SDIDriver.PowerDownLevel.Half,
                delayTimeMilliseconds: _deviceConfig.PowerDownConfig.DelayTimeMilliseconds.HasValue ? _deviceConfig.PowerDownConfig.DelayTimeMilliseconds.Value : (short)0
            );
        }

        private void ConfigureMovementLimits()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.MovementLimitsConfig == null) return;

            if (_deviceConfig.MovementLimitsConfig.Min.HasValue)
            {
                _sdiDevice.SetIndicatorMovementLimitMinimum(_deviceConfig.MovementLimitsConfig.Min.Value);
            }

            if (_deviceConfig.MovementLimitsConfig.Max.HasValue)
            {
                _sdiDevice.SetIndicatorMovementLimitMaximum(_deviceConfig.MovementLimitsConfig.Max.Value);
            }
           
        }

        private void ConfigureStatorBaseAngles()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.StatorBaseAnglesConfig == null) return;
            
            if (_deviceConfig.StatorBaseAnglesConfig.S1BaseAngleDegrees.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S1, (short)((_deviceConfig.StatorBaseAnglesConfig.S1BaseAngleDegrees.Value/360.000)* SDIDriver.Device.STATOR_BASE_ANGLE_MAX_OFFSET));
            }

            if (_deviceConfig.StatorBaseAnglesConfig.S2BaseAngleDegrees.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S2, (short)((_deviceConfig.StatorBaseAnglesConfig.S2BaseAngleDegrees.Value / 360.000)* SDIDriver.Device.STATOR_BASE_ANGLE_MAX_OFFSET));
            }

            if (_deviceConfig.StatorBaseAnglesConfig.S3BaseAngleDegrees.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S3, (short)((_deviceConfig.StatorBaseAnglesConfig.S3BaseAngleDegrees.Value / 360.000)* SDIDriver.Device.STATOR_BASE_ANGLE_MAX_OFFSET));
            }
            
        }

        private void ConfigureOutputChannels()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.OutputChannelsConfig== null) return;

            _sdiDevice.ConfigureOutputChannels(
                digPwm1: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_1),
                digPwm2: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_2),
                digPwm3: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_3),
                digPwm4: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_4),
                digPwm5: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_5),
                digPwm6: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_6),
                digPwm7: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_7)
            );
            _sdiDevice.SetOutputChannelValue(SDIDriver.OutputChannels.DIG_PWM_1, OutputChannelInitialValue(SDIDriver.OutputChannels.DIG_PWM_1));

            
        }

        private void ConfigureUpdateRateControl()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControlConfig== null) return;
            
            if (_deviceConfig.UpdateRateControlConfig.Mode.HasValue)
            {
                switch (_deviceConfig.UpdateRateControlConfig.Mode.Value)
                {
                    case SDIDriver.UpdateRateControlModes.Limit:
                        ConfigureUpdateRateControlLimitMode();
                        break;
                    case SDIDriver.UpdateRateControlModes.Smooth:
                        ConfigureUpdateRateControlSmoothMode();
                        break;
                }
            }

            if (_deviceConfig.UpdateRateControlConfig.StepUpdateDelayMillis.HasValue)
            {
                _sdiDevice.SetUpdateRateControlSpeed((short)_deviceConfig.UpdateRateControlConfig.StepUpdateDelayMillis.Value);
            }

            if (_deviceConfig.UpdateRateControlConfig.UseShortestPath.HasValue)
            {
                _sdiDevice.SetUpdateRateControlMiscellaneous(_deviceConfig.UpdateRateControlConfig.UseShortestPath.Value);
            }

            if (_deviceConfig.DiagnosticLEDMode.HasValue)
            {
                _sdiDevice.ConfigureDiagnosticLEDBehavior(_deviceConfig.DiagnosticLEDMode.Value);
            }
            
        }

        private void ConfigureUpdateRateControlLimitMode()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControlConfig == null) return;
            _sdiDevice.SetUpdateRateControlModeLimit(_deviceConfig.UpdateRateControlConfig.LimitThreshold.HasValue 
                ? _deviceConfig.UpdateRateControlConfig.LimitThreshold.Value 
                : (byte)0);
        }

        private void ConfigureUpdateRateControlSmoothMode()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControlConfig == null) return;
            _sdiDevice.SetUpdateRateControlModeSmooth(
                smoothingMinimumThresholdValue:
                    _deviceConfig.UpdateRateControlConfig.SmoothingMinimumThreshold.HasValue
                        ? _deviceConfig.UpdateRateControlConfig.SmoothingMinimumThreshold.Value
                        : (byte)0,
                smoothingMode:
                    _deviceConfig.UpdateRateControlConfig.SmoothingMode.HasValue
                        ? _deviceConfig.UpdateRateControlConfig.SmoothingMode.Value
                        : SDIDriver.UpdateRateControlSmoothingMode.Adaptive
            );
        }

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
        ~HenkSDIHardwareSupportModule()
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
                    UnregisterForInputEvents();
                    Common.Util.DisposeObject(_sdiDevice);
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}