using System;
using Common.Networking;
using F4KeyFile;
using F4SharedMem;
using F4Utils.Process;
using LightningGauges.Renderers;
using MFDExtractor.Networking;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.Runtime.SimSupport.Falcon4;
using MFDExtractor.UI;

namespace MFDExtractor.Runtime
{
    internal class MessageManager
    {
        private readonly NetworkManager _networkManager;
        private readonly InstrumentRenderers _renderers;
        private readonly SettingsManager _settingsManager;
        private readonly Falcon4SimSupport _simSupport;
        private DateTime? _ehsiRightKnobDepressedTime;
        private DateTime? _ehsiRightKnobLastActivityTime;
        private DateTime? _ehsiRightKnobReleasedTime;

        private MessageManager()
        {
        }

        public MessageManager(InstrumentRenderers renderers, NetworkManager networkManager,
                              SettingsManager settingsManager, Falcon4SimSupport simSupport)
        {
            _renderers = renderers;
            _networkManager = networkManager;
            _settingsManager = settingsManager;
            _simSupport = simSupport;
        }

        public bool EHSIRightKnobIsCurrentlyDepressed
        {
            get { return _ehsiRightKnobDepressedTime.HasValue; }
        }

        public void ProcessPendingMessages()
        {
            if (_settingsManager.NetworkMode == NetworkMode.Client)
            {
                ProcessPendingMessagesToClientFromServer();
            }
            else if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                ProcessPendingMessagesToServerFromClient();
            }
        }

        private void ProcessPendingMessagesToServerFromClient()
        {
            if (_settingsManager.NetworkMode != NetworkMode.Server) return;
            Message pendingMessage = _networkManager.GetNextPendingMessageToServerFromClient();
            while (pendingMessage != null)
            {
                var messageType = (MessageTypes) Enum.Parse(typeof (MessageTypes), pendingMessage.MessageType);
                switch (messageType)
                {
                    case MessageTypes.ToggleNightMode:
                        NotifyNightModeIsToggled(false);
                        break;
                    case MessageTypes.AirspeedIndexIncrease:
                        NotifyAirspeedIndexIncreasedByOne(false);
                        break;
                    case MessageTypes.AirspeedIndexDecrease:
                        NotifyAirspeedIndexDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobIncrease:
                        NotifyEHSILeftKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobDecrease:
                        NotifyEHSILeftKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobIncrease:
                        NotifyEHSIRightKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDecrease:
                        NotifyEHSIRightKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDepressed:
                        NotifyEHSIRightKnobDepressed(false);
                        break;
                    case MessageTypes.EHSIRightKnobReleased:
                        NotifyEHSIRightKnobReleased(false);
                        break;
                    case MessageTypes.EHSIMenuButtonDepressed:
                        NotifyEHSIMenuButtonDepressed(false);
                        break;
                    case MessageTypes.AccelerometerIsReset:
                        NotifyAccelerometerIsReset(false);
                        break;
                    default:
                        break;
                }
                pendingMessage = _networkManager.GetNextPendingMessageToServerFromClient();
            }
        }

        private void ProcessPendingMessagesToClientFromServer()
        {
            if (_settingsManager.NetworkMode != NetworkMode.Client) return;
            Message pendingMessage = _networkManager.GetNextPendingMessageToClientFromServer();
            while (pendingMessage != null)
            {
                var messageType = (MessageTypes) Enum.Parse(typeof (MessageTypes), pendingMessage.MessageType);
                switch (messageType)
                {
                    case MessageTypes.ToggleNightMode:
                        NotifyNightModeIsToggled(false);
                        break;
                    case MessageTypes.AirspeedIndexIncrease:
                        NotifyAirspeedIndexIncreasedByOne(false);
                        break;
                    case MessageTypes.AirspeedIndexDecrease:
                        NotifyAirspeedIndexDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobIncrease:
                        NotifyEHSILeftKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSILeftKnobDecrease:
                        NotifyEHSILeftKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobIncrease:
                        NotifyEHSIRightKnobIncreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDecrease:
                        NotifyEHSIRightKnobDecreasedByOne(false);
                        break;
                    case MessageTypes.EHSIRightKnobDepressed:
                        NotifyEHSIRightKnobDepressed(false);
                        break;
                    case MessageTypes.EHSIRightKnobReleased:
                        NotifyEHSIRightKnobReleased(false);
                        break;
                    case MessageTypes.EHSIMenuButtonDepressed:
                        NotifyEHSIMenuButtonDepressed(false);
                        break;
                    case MessageTypes.AccelerometerIsReset:
                        NotifyAccelerometerIsReset(false);
                        break;
                    case MessageTypes.EnableBMSAdvancedSharedmemValues:
                        _simSupport.UseBMSAdvancedSharedmemValues = true;
                        break;
                    case MessageTypes.DisableBMSAdvancedSharedmemValues:
                        _simSupport.UseBMSAdvancedSharedmemValues = false;
                        break;
                    default:
                        break;
                }
                pendingMessage = _networkManager.GetNextPendingMessageToClientFromServer();
            }
        }

        public void UpdateEHSIBrightnessLabelVisibility()
        {
            bool showBrightnessLabel = false;
            if (EHSIRightKnobIsCurrentlyDepressed)
            {
                DateTime? whenPressed = _ehsiRightKnobDepressedTime;
                if (whenPressed.HasValue)
                {
                    TimeSpan howLongPressed = DateTime.Now.Subtract(whenPressed.Value);
                    if (howLongPressed.TotalMilliseconds > 2000)
                    {
                        showBrightnessLabel = true;
                    }
                }
            }
            else
            {
                DateTime? whenReleased = _ehsiRightKnobReleasedTime;
                DateTime? lastActivity = _ehsiRightKnobLastActivityTime;
                if (whenReleased.HasValue && lastActivity.HasValue)
                {
                    TimeSpan howLongAgoReleased = DateTime.Now.Subtract(whenReleased.Value);
                    TimeSpan howLongAgoLastActivity = DateTime.Now.Subtract(lastActivity.Value);
                    if (howLongAgoReleased.TotalMilliseconds < 2000 || howLongAgoLastActivity.TotalMilliseconds < 2000)
                    {
                        showBrightnessLabel = ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel;
                    }
                }
            }
            ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
        }

        public void NotifyAccelerometerIsReset(bool relayToListeners)
        {
            ((F16Accelerometer) _renderers.AccelerometerRenderer).InstrumentState.ResetMinAndMaxGs();
            if (relayToListeners) SendMessage(MessageTypes.AccelerometerIsReset, null);
        }

        public void NotifyNightModeIsToggled(bool relayToListeners)
        {
            InstrumentFormController.NightMode = !InstrumentFormController.NightMode;
            if (relayToListeners && _settingsManager.NetworkMode == NetworkMode.Server)
            {
                SendMessage(MessageTypes.ToggleNightMode, null);
            }
        }

        public void NotifyAzimuthIndicatorBrightnessIncreased(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                ((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.Brightness +
                ((((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
            ((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners)
                SendMessage(MessageTypes.AzimuthIndicatorBrightnessDecrease,
                            MessageTypes.AzimuthIndicatorBrightnessIncrease);
        }

        public void NotifyAzimuthIndicatorBrightnessDecreased(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                ((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.Brightness -
                ((((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
            ((F16AzimuthIndicator) _renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners) SendMessage(MessageTypes.AzimuthIndicatorBrightnessDecrease, null);
        }

        public void NotifyISISBrightButtonDepressed(bool relayToListeners)
        {
            int newBrightness = ((F16ISIS) _renderers.ISISRenderer).InstrumentState.MaxBrightness;
            ((F16ISIS) _renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners) SendMessage(MessageTypes.ISISBrightButtonDepressed, null);
        }

        public void NotifyISISStandardButtonDepressed(bool relayToListeners)
        {
            var newBrightness = (int) Math.Floor(
                (((F16ISIS) _renderers.ISISRenderer).InstrumentState.MaxBrightness)*0.5f
                                          );
            ((F16ISIS) _renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners) SendMessage(MessageTypes.ISISStandardButtonDepressed, null);
        }

        public void NotifyEHSILeftKnobIncreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = Util.DetectFalconFormat();
            bool useIncrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                KeyBinding incByOneCallback = Util.FindKeyBinding("SimHsiHdgIncBy1");
                if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                {
                    useIncrementByOne = true;
                }
            }
            if (useIncrementByOne)
            {
                Util.SendCallbackToFalcon("SimHsiHdgIncBy1");
            }
            else
            {
                Util.SendCallbackToFalcon("SimHsiHeadingInc");
            }
            if (relayToListeners) SendMessage(MessageTypes.EHSILeftKnobIncrease, null);
        }

        public void NotifyEHSILeftKnobDecreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = Util.DetectFalconFormat();
            bool useDecrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                KeyBinding decByOneCallback = Util.FindKeyBinding("SimHsiHdgDecBy1");
                if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                {
                    useDecrementByOne = true;
                }
            }
            if (useDecrementByOne)
            {
                Util.SendCallbackToFalcon("SimHsiHdgDecBy1");
            }
            else
            {
                Util.SendCallbackToFalcon("SimHsiHeadingDec");
            }

            if (relayToListeners) SendMessage(MessageTypes.EHSILeftKnobDecrease, null);
        }

        public void NotifyEHSIRightKnobIncreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI) _renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                var newBrightness = (int) Math.Floor(
                    ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.Brightness +
                    ((((F16EHSI) _renderers.EHSIRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
                ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {
                FalconDataFormats? format = Util.DetectFalconFormat();
                bool useIncrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    KeyBinding incByOneCallback = Util.FindKeyBinding("SimHsiCrsIncBy1");
                    if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                    {
                        useIncrementByOne = true;
                    }
                }
                if (useIncrementByOne)
                {
                    Util.SendCallbackToFalcon("SimHsiCrsIncBy1");
                }
                else
                {
                    Util.SendCallbackToFalcon("SimHsiCourseInc");
                }
            }

            if (relayToListeners) SendMessage(MessageTypes.EHSIRightKnobIncrease, null);
        }

        public void NotifyEHSIRightKnobDecreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI) _renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                var newBrightness = (int) Math.Floor(
                    ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.Brightness -
                    ((((F16EHSI) _renderers.EHSIRenderer).InstrumentState.MaxBrightness)*(1.0f/32.0f)));
                ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {
                FalconDataFormats? format = Util.DetectFalconFormat();
                bool useDecrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    KeyBinding decByOneCallback = Util.FindKeyBinding("SimHsiCrsDecBy1");
                    if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int) ScanCodes.NotAssigned)
                    {
                        useDecrementByOne = true;
                    }
                }
                if (useDecrementByOne)
                {
                    Util.SendCallbackToFalcon("SimHsiCrsDecBy1");
                }
                else
                {
                    Util.SendCallbackToFalcon("SimHsiCourseDec");
                }
            }

            if (relayToListeners) SendMessage(MessageTypes.EHSIRightKnobDecrease, null);
        }

        public void NotifyEHSIRightKnobDepressed(bool relayToListeners)
        {
            _ehsiRightKnobDepressedTime = DateTime.Now;
            _ehsiRightKnobReleasedTime = null;
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (relayToListeners) SendMessage(MessageTypes.EHSIRightKnobDepressed, null);
        }

        public void NotifyEHSIRightKnobReleased(bool relayToListeners)
        {
            _ehsiRightKnobDepressedTime = null;
            _ehsiRightKnobReleasedTime = DateTime.Now;
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (relayToListeners) SendMessage(MessageTypes.EHSIRightKnobReleased, null);
        }

        public void NotifyEHSIMenuButtonDepressed(bool relayToListeners)
        {
            F16EHSI.F16EHSIInstrumentState.InstrumentModes currentMode =
                ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.InstrumentMode;
            F16EHSI.F16EHSIInstrumentState.InstrumentModes? newMode = null;
            switch (currentMode)
            {
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Unknown:
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsTacan;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.Nav:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav;
                    break;
                case F16EHSI.F16EHSIInstrumentState.InstrumentModes.PlsNav:
                    newMode = F16EHSI.F16EHSIInstrumentState.InstrumentModes.Tacan;
                    break;
                default:
                    break;
            }
            if (newMode.HasValue)
            {
                ((F16EHSI) _renderers.EHSIRenderer).InstrumentState.InstrumentMode = newMode.Value;
            }
            if (_settingsManager.NetworkMode == NetworkMode.Standalone ||
                _settingsManager.NetworkMode == NetworkMode.Server)
            {
                Util.SendCallbackToFalcon("SimStepHSIMode");
            }
            if (relayToListeners) SendMessage(MessageTypes.EHSIMenuButtonDepressed, null);
        }

        public void NotifyAirspeedIndexDecreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator) _renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots -= 2.5F;
            if (relayToListeners) SendMessage(MessageTypes.AirspeedIndexDecrease, null);
        }

        public void NotifyAirspeedIndexIncreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator) _renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots += 2.5F;
            if (relayToListeners) SendMessage(MessageTypes.AirspeedIndexIncrease, null);
        }

        private void SendMessage(MessageTypes messageType, object payload)
        {
            var msg = new Message(messageType.ToString(), payload);
            if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                _networkManager.SubmitMessageToClientFromServer(msg);
            }
            else if (_settingsManager.NetworkMode == NetworkMode.Client)
            {
                _networkManager.SubmitMessageToServerFromClient(msg);
            }
        }
    }
}