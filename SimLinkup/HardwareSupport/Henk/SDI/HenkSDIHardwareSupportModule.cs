using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using log4net;
using SDIDriver = SDI;
using System.Linq;
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
        private string _deviceIdentification;
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
            get { return string.Format("Henk Synchro Drive Interface: {0}", _deviceIdentification); }
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
            thisSignal.Id = "HenkSDI__Synchro_Position";
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
                thisSignal.Id = string.Format("HenkSDI__DIG_PWM_{0}", channelNumber);
            }
            else
            {
                thisSignal.FriendlyName = string.Format("OB_BUF_PWM ({1})", channelNumber, "PWM");
                thisSignal.Id = string.Format("HenkSDI__OB_BUF_PWM", channelNumber);
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
            thisSignal.Id = string.Format("HenkSDI__DIG_PWM_{0}", channelNumber);
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
                    SDIDriver.OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM,
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
            if (channelNumber.Value == 8) return SDIDriver.OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM;
            return SDIDriver.OutputChannels.Unknown;
        }
        private SDIDriver.OutputChannelMode OutputChannelMode(SDIDriver.OutputChannels outputChannel)
        {
            switch (outputChannel)
            {
                case SDIDriver.OutputChannels.ONBOARD_OPAMP_BUFFERED_PWM:
                    return SDIDriver.OutputChannelMode.PWM;
                case SDIDriver.OutputChannels.DIG_PWM_1:
                    return _deviceConfig !=null && _deviceConfig.OutputChannels !=null &&
                            _deviceConfig.OutputChannels.Channel1.HasValue
                                ? _deviceConfig.OutputChannels.Channel1.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_2:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel2.HasValue
                                ? _deviceConfig.OutputChannels.Channel2.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_3:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel3.HasValue
                                ? _deviceConfig.OutputChannels.Channel3.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_4:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel4.HasValue
                                ? _deviceConfig.OutputChannels.Channel4.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_5:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel5.HasValue
                                ? _deviceConfig.OutputChannels.Channel5.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_6:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel6.HasValue
                                ? _deviceConfig.OutputChannels.Channel6.Value
                                : SDIDriver.OutputChannelMode.Digital;
                case SDIDriver.OutputChannels.DIG_PWM_7:
                    return _deviceConfig != null && _deviceConfig.OutputChannels != null && 
                            _deviceConfig.OutputChannels.Channel7.HasValue
                                ? _deviceConfig.OutputChannels.Channel7.Value
                                : SDIDriver.OutputChannelMode.Digital;
            }
            return SDIDriver.OutputChannelMode.Digital;
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
                    !string.IsNullOrWhiteSpace(_deviceConfig.DOAAddress)
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
                //string identity = _sdiDevice.Identify();
                string identity = string.Format("${0}", (_deviceConfig.DOAAddress ?? string.Empty).Trim());
                string function = GetFunction(identity);
                _deviceIdentification = string.Format("{0} ({1}) on USB [{2}]", identity, function, comPort);
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
                string.IsNullOrWhiteSpace(_deviceConfig.DOAAddress
            )
            )
            {
                return;
            }
            byte doaAddressByte = 0x00;
            string doaAddressString = (_deviceConfig.DOAAddress ?? "").Trim();
            bool doaAddressIsValid = byte.TryParse(doaAddressString, out doaAddressByte);
            if (!doaAddressIsValid) return;

            try
            {
                var comPort = (_deviceConfig.COMPort ?? "").Trim();
                var phccDevice = new global::Phcc.Device(portName: comPort, openPort: false);
                _sdiDevice = new SDIDriver.Device(phccDevice: phccDevice, doaAddress: doaAddressByte);
                string identity = string.Format("${0}", (_deviceConfig.DOAAddress ?? string.Empty).Trim());
                string function = GetFunction(identity);
                _deviceIdentification = string.Format("{0} ({1}) on PHCC [{2}]", identity, function, comPort);

            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            
        }
        private static string GetFunction(string identification)
        {
            if (identification != null)
            {
                if (identification.Contains("30") || identification.Contains("48"))
                {
                    return "PITCH";
                }
                else if (identification.Contains("32") || identification.Contains("50"))
                {
                    return "ROLL";
                }
            }
            return string.Empty;
        }
        private void ConfigurePowerDown()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.PowerDown == null) return;

            _sdiDevice.ConfigurePowerDown(
                powerDownState: _deviceConfig.PowerDown.Enabled.HasValue && _deviceConfig.PowerDown.Enabled.Value == true ? SDIDriver.PowerDownState.Enabled : SDIDriver.PowerDownState.Disabled,
                powerDownLevel: _deviceConfig.PowerDown.Level.HasValue ? _deviceConfig.PowerDown.Level.Value : SDIDriver.PowerDownLevel.Half,
                delayTimeMilliseconds: _deviceConfig.PowerDown.DelayTimeMilliseconds.HasValue ? _deviceConfig.PowerDown.DelayTimeMilliseconds.Value : (short)0
            );
        }

        private void ConfigureMovementLimits()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.MovementLimits == null) return;

            if (_deviceConfig.MovementLimits.Min.HasValue)
            {
                _sdiDevice.SetIndicatorMovementLimitMinimum(_deviceConfig.MovementLimits.Min.Value);
            }

            if (_deviceConfig.MovementLimits.Max.HasValue)
            {
                _sdiDevice.SetIndicatorMovementLimitMaximum(_deviceConfig.MovementLimits.Max.Value);
            }
           
        }

        private void ConfigureStatorBaseAngles()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.StatorBaseAngles != null) return;
            
            if (_deviceConfig.StatorBaseAngles.S1.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S1, _deviceConfig.StatorBaseAngles.S1.Value);
            }

            if (_deviceConfig.StatorBaseAngles.S2.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S2, _deviceConfig.StatorBaseAngles.S2.Value);
            }

            if (_deviceConfig.StatorBaseAngles.S3.HasValue)
            {
                _sdiDevice.SetStatorBaseAngle(SDIDriver.StatorSignals.S3, _deviceConfig.StatorBaseAngles.S3.Value);
            }
            
        }

        private void ConfigureOutputChannels()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.OutputChannels== null) return;

            _sdiDevice.ConfigureOutputChannels(
                digPwm1: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_1),
                digPwm2: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_2),
                digPwm3: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_3),
                digPwm4: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_4),
                digPwm5: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_5),
                digPwm6: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_6),
                digPwm7: OutputChannelMode(SDIDriver.OutputChannels.DIG_PWM_7)
            );
            
        }

        private void ConfigureUpdateRateControl()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControl== null) return;
            
            if (_deviceConfig.UpdateRateControl.Mode.HasValue)
            {
                switch (_deviceConfig.UpdateRateControl.Mode.Value)
                {
                    case SDIDriver.UpdateRateControlModes.Limit:
                        ConfigureUpdateRateControlLimitMode();
                        break;
                    case SDIDriver.UpdateRateControlModes.Smooth:
                        ConfigureUpdateRateControlSmoothMode();
                        break;
                }
            }

            if (_deviceConfig.UpdateRateControl.StepUpdateDelayMillis.HasValue)
            {
                _sdiDevice.SetUpdateRateControlSpeed(_deviceConfig.UpdateRateControl.StepUpdateDelayMillis.Value);
            }

            if (_deviceConfig.UpdateRateControl.UseShortestPath.HasValue)
            {
                _sdiDevice.SetUpdateRateControlMiscellaneous(_deviceConfig.UpdateRateControl.UseShortestPath.Value);
            }

            if (_deviceConfig.DiagnosticLEDMode.HasValue)
            {
                _sdiDevice.ConfigureDiagnosticLEDBehavior(_deviceConfig.DiagnosticLEDMode.Value);
            }
            
        }

        private void ConfigureUpdateRateControlLimitMode()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControl == null) return;
            _sdiDevice.SetUpdateRateControlModeLimit(_deviceConfig.UpdateRateControl.LimitThreshold.HasValue 
                ? _deviceConfig.UpdateRateControl.LimitThreshold.Value 
                : (byte)0);
        }

        private void ConfigureUpdateRateControlSmoothMode()
        {
            if (_sdiDevice == null || _deviceConfig == null || _deviceConfig.UpdateRateControl == null) return;
            _sdiDevice.SetUpdateRateControlModeSmooth(
                smoothingMinimumThresholdValue:
                    _deviceConfig.UpdateRateControl.SmoothingMinimumThreshold.HasValue
                        ? _deviceConfig.UpdateRateControl.SmoothingMinimumThreshold.Value
                        : (byte)0,
                smoothingMode:
                    _deviceConfig.UpdateRateControl.SmoothingMode.HasValue
                        ? _deviceConfig.UpdateRateControl.SmoothingMode.Value
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