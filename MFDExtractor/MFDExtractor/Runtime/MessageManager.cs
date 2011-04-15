using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightningGauges.Renderers;
using F4SharedMem;
using Common.Networking;
using MFDExtractor.Networking;
using MFDExtractor.UI;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.Runtime.SimSupport.Falcon4;

namespace MFDExtractor.Runtime
{
    internal class MessageManager
    {
        private NetworkManager _networkManager = null;
        private SettingsManager _settingsManager = null;
        private Falcon4SimSupport _simSupport = null;
        private InstrumentRenderers _renderers = null;
        private DateTime? _ehsiRightKnobDepressedTime = null;
        private DateTime? _ehsiRightKnobReleasedTime = null;
        private DateTime? _ehsiRightKnobLastActivityTime = null;

        private MessageManager()
            : base()
        {
        }
        public MessageManager(InstrumentRenderers renderers, NetworkManager networkManager, SettingsManager settingsManager, Falcon4SimSupport simSupport )
        {
            _renderers = renderers;
            _networkManager = networkManager;
            _settingsManager = settingsManager;
            _simSupport = simSupport;
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
            Networking.Message pendingMessage = _networkManager.GetNextPendingMessageToServerFromClient();
            while (pendingMessage != null)
            {
                Networking.MessageTypes messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
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
                Networking.MessageTypes messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), pendingMessage.MessageType);
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
        public bool EHSIRightKnobIsCurrentlyDepressed
        {
            get
            {
                return _ehsiRightKnobDepressedTime.HasValue;
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
                        showBrightnessLabel = ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel;
                    }
                }
            }
            ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel = showBrightnessLabel;
        }
        public void NotifyAccelerometerIsReset(bool relayToListeners)
        {
            ((F16Accelerometer)_renderers.AccelerometerRenderer).InstrumentState.ResetMinAndMaxGs();
            if (relayToListeners) SendMessage(MessageTypes.AccelerometerIsReset, null);
        }
        public void NotifyNightModeIsToggled(bool relayToListeners)
        {
            InstrumentFormController.NightMode = !InstrumentFormController.NightMode;
            if (relayToListeners && _settingsManager.NetworkMode  == NetworkMode.Server)
            {
                SendMessage(MessageTypes.ToggleNightMode, null);
            }
        }
        public void NotifyAzimuthIndicatorBrightnessIncreased(bool relayToListeners)
        {
            int newBrightness = (int)Math.Floor(
                (float)((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness +
                ((float)(((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners) SendMessage(MessageTypes.AzimuthIndicatorBrightnessDecrease, MessageTypes.AzimuthIndicatorBrightnessIncrease);
        }
        public void NotifyAzimuthIndicatorBrightnessDecreased(bool relayToListeners)
        {
            int newBrightness = (int)Math.Floor(
                (float)((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness -
                ((float)(((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
            ((F16AzimuthIndicator)_renderers.RWRRenderer).InstrumentState.Brightness = newBrightness;
            Properties.Settings.Default.AzimuthIndicatorBrightness = newBrightness;

            if (relayToListeners) SendMessage(MessageTypes.AzimuthIndicatorBrightnessDecrease, null);
        }
        public void NotifyISISBrightButtonDepressed(bool relayToListeners)
        {
            int newBrightness = ((F16ISIS)_renderers.ISISRenderer).InstrumentState.MaxBrightness;
            ((F16ISIS)_renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners) SendMessage(MessageTypes.ISISBrightButtonDepressed, null);
        }
        public void NotifyISISStandardButtonDepressed(bool relayToListeners)
        {
            int newBrightness = (int)Math.Floor(
                    ((float)((F16ISIS)_renderers.ISISRenderer).InstrumentState.MaxBrightness) * 0.5f
                );
            ((F16ISIS)_renderers.ISISRenderer).InstrumentState.Brightness = newBrightness;
            if (relayToListeners) SendMessage(MessageTypes.ISISStandardButtonDepressed, null);
        }
        public void NotifyEHSILeftKnobIncreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
            bool useIncrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgIncBy1");
                if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useIncrementByOne = true;
                }
            }
            if (useIncrementByOne)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHdgIncBy1");
            }
            else
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHeadingInc");
            }
            if (relayToListeners) SendMessage(MessageTypes.EHSILeftKnobIncrease, null);
        }
        public void NotifyEHSILeftKnobDecreasedByOne(bool relayToListeners)
        {
            FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
            bool useDecrementByOne = false;
            if (format.HasValue && format.Value == FalconDataFormats.BMS4)
            {
                F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiHdgDecBy1");
                if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                {
                    useDecrementByOne = true;
                }
            }
            if (useDecrementByOne)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHdgDecBy1");
            }
            else
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimHsiHeadingDec");
            }

            if (relayToListeners) SendMessage(MessageTypes.EHSILeftKnobDecrease, null);
        }
        public void NotifyEHSIRightKnobIncreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness +
                    ((float)(((F16EHSI)_renderers.EHSIRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useIncrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding incByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsIncBy1");
                    if (incByOneCallback != null && incByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useIncrementByOne = true;
                    }
                }
                if (useIncrementByOne)
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCrsIncBy1");
                }
                else
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCourseInc");
                }
            }
            
            if (relayToListeners) SendMessage(MessageTypes.EHSIRightKnobIncrease, null);

        }
        public void NotifyEHSIRightKnobDecreasedByOne(bool relayToListeners)
        {
            _ehsiRightKnobLastActivityTime = DateTime.Now;
            if (((F16EHSI)_renderers.EHSIRenderer).InstrumentState.ShowBrightnessLabel)
            {
                int newBrightness = (int)Math.Floor(
                    (float)((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness -
                    ((float)(((F16EHSI)_renderers.EHSIRenderer).InstrumentState.MaxBrightness) * (1.0f / 32.0f)));
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.Brightness = newBrightness;
                Properties.Settings.Default.EHSIBrightness = newBrightness;
            }
            else
            {

                FalconDataFormats? format = F4Utils.Process.Util.DetectFalconFormat();
                bool useDecrementByOne = false;
                if (format.HasValue && format.Value == FalconDataFormats.BMS4)
                {
                    F4KeyFile.KeyBinding decByOneCallback = F4Utils.Process.Util.FindKeyBinding("SimHsiCrsDecBy1");
                    if (decByOneCallback != null && decByOneCallback.Key.ScanCode != (int)F4KeyFile.ScanCodes.NotAssigned)
                    {
                        useDecrementByOne = true;
                    }
                }
                if (useDecrementByOne)
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCrsDecBy1");
                }
                else
                {
                    F4Utils.Process.Util.SendCallbackToFalcon("SimHsiCourseDec");
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
            F16EHSI.F16EHSIInstrumentState.InstrumentModes currentMode = ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.InstrumentMode;
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
                ((F16EHSI)_renderers.EHSIRenderer).InstrumentState.InstrumentMode = newMode.Value;
            }
            if (_settingsManager.NetworkMode == NetworkMode.Standalone || _settingsManager.NetworkMode == NetworkMode.Server)
            {
                F4Utils.Process.Util.SendCallbackToFalcon("SimStepHSIMode");
            }
            if (relayToListeners) SendMessage(MessageTypes.EHSIMenuButtonDepressed, null);
        }
        public void NotifyAirspeedIndexDecreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator)_renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots -= 2.5F;
            if (relayToListeners) SendMessage(MessageTypes.AirspeedIndexDecrease, null);
        }
        public void NotifyAirspeedIndexIncreasedByOne(bool relayToListeners)
        {
            ((F16AirspeedIndicator)_renderers.ASIRenderer).InstrumentState.AirspeedIndexKnots += 2.5F;
            if (relayToListeners) SendMessage(MessageTypes.AirspeedIndexIncrease, null);
        }

        private void SendMessage(MessageTypes messageType, object payload)
        {
            Networking.Message msg = new MFDExtractor.Networking.Message(messageType.ToString(), payload);
            if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                _networkManager.SubmitMessageToClientFromServer(msg);
            }
            else if (_settingsManager.NetworkMode == NetworkMode.Client )
            {
                _networkManager.SubmitMessageToServerFromClient(msg);
            }
        }

    }
}
