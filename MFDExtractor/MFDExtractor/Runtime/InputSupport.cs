﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common.Win32;
using System.Threading;
using Microsoft.DirectX.DirectInput;
using log4net;
using Common.InputSupport.DirectInput;
using Common.InputSupport;
using Common.InputSupport.UI;
using MFDExtractor.Runtime.Settings;

namespace MFDExtractor.Runtime
{
    internal class InputSupport
    {
        private static ILog _log = LogManager.GetLogger(typeof(InputSupport));

        private Thread _keyboardWatcherThread = null;
        private Mediator.PhysicalControlStateChangedEventHandler _mediatorEventHandler = null;
        private Mediator _mediator = null;
        private SettingsManager _settingsManager= null;
        private MessageManager _messageManager = null;
        private Extractor _extractor = null;
        public Mediator Mediator { get { return _mediator; } }
        private InputSupport() : base() 
        {
            Initialize();
        }
        public InputSupport(SettingsManager settingsManager, MessageManager messageManager, Extractor extractor):this()
        {
            _extractor = extractor;
            _settingsManager = settingsManager;
            _messageManager = messageManager;
        }
        private void Initialize()
        {
            CreateMediator();
            SetupKeyboardWatcherThread();
        }

        private void CreateMediator()
        {
            _mediatorEventHandler = new Mediator.PhysicalControlStateChangedEventHandler(Mediator_PhysicalControlStateChanged);
            if (!Properties.Settings.Default.DisableDirectInputMediator)
            {
                _mediator = new Mediator(null);
            }
        }
        private void RegisterMediatorEventHandler()
        {
            if (_mediator != null)
            {
                _mediator.PhysicalControlStateChanged += _mediatorEventHandler;
            }

        }
        private void UnregisterMediatorEventHandler()
        {
            if (_mediator != null)
            {
                _mediator.PhysicalControlStateChanged -= _mediatorEventHandler;
            }
        }
        private void Mediator_PhysicalControlStateChanged(object sender, Common.InputSupport.PhysicalControlStateChangedEventArgs e)
        {
            _settingsManager.KeySettings.Reload();

            if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.NVISKey))
            {
                _messageManager.NotifyNightModeIsToggled(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.AirspeedIndexIncreaseKey))
            {
                _messageManager.NotifyAirspeedIndexIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.AirspeedIndexDecreaseKey))
            {
                _messageManager.NotifyAirspeedIndexDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSIHeadingDecreaseKey))
            {
                _messageManager.NotifyEHSILeftKnobDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSIHeadingIncreaseKey))
            {
                _messageManager.NotifyEHSILeftKnobIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSICourseDecreaseKey))
            {
                _messageManager.NotifyEHSIRightKnobDecreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSICourseIncreaseKey))
            {
                _messageManager.NotifyEHSIRightKnobIncreasedByOne(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSICourseDepressedKey))
            {
                _messageManager.NotifyEHSIRightKnobDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.EHSIMenuButtonDepressedKey))
            {
                _messageManager.NotifyEHSIMenuButtonDepressed(true);
            }
            else if (
                    !DirectInputHotkeyIsTriggering(_settingsManager.KeySettings.EHSICourseDepressedKey)
                        &&
                    EHSIRightKnobIsCurrentlyDepressed()
                )
            {
                _messageManager.NotifyEHSIRightKnobReleased(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.ISISBrightButtonKey))
            {
                _messageManager.NotifyISISBrightButtonDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.ISISStandardButtonKey))
            {
                _messageManager.NotifyISISStandardButtonDepressed(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.AzimuthIndicatorBrightnessIncreaseKey))
            {
                _messageManager.NotifyAzimuthIndicatorBrightnessIncreased(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.AzimuthIndicatorBrightnessDecreaseKey))
            {
                _messageManager.NotifyAzimuthIndicatorBrightnessDecreased(true);
            }
            else if (DirectInputEventIsHotkey(e, _settingsManager.KeySettings.AccelerometerResetKey))
            {
                _messageManager.NotifyAccelerometerIsReset(true);
            }

        }
        private bool DirectInputHotkeyIsTriggering(InputControlSelection hotkey)
        {
            if (hotkey == null || hotkey.DirectInputControl == null) return false;
            int? currentVal = _mediator.GetPhysicalControlValue(hotkey.DirectInputControl, StateType.Current);
            int? prevVal = _mediator.GetPhysicalControlValue(hotkey.DirectInputControl, StateType.Previous);

            switch (hotkey.ControlType)
            {
                case ControlType.Unknown:
                    break;
                case ControlType.Axis:
                    if (currentVal.HasValue && !prevVal.HasValue)
                    {
                        return true;
                    }
                    else if (!currentVal.HasValue && prevVal.HasValue)
                    {
                        return true;
                    }
                    else
                    {
                        return (currentVal.Value != prevVal.Value);
                    }
                case ControlType.Button:
                    return (currentVal.HasValue && currentVal.Value == 1);
                case ControlType.Pov:
                    if (currentVal.HasValue)
                    {
                        return Common.InputSupport.Util.GetPovDirection(currentVal.Value) == hotkey.PovDirection;
                    }
                    else
                    {
                        return false;
                    }
                case ControlType.Key:
                    return (currentVal.HasValue && currentVal.Value == 1);
                default:
                    break;
            }
            return false;

        }
        private bool DirectInputEventIsHotkey(PhysicalControlStateChangedEventArgs diEvent, InputControlSelection hotkey)
        {
            if (diEvent == null) return false;
            if (diEvent.Control == null) return false;
            if (hotkey == null) return false;
            if (
                hotkey.ControlType != ControlType.Axis
                    &&
                hotkey.ControlType != ControlType.Button
                    &&
                hotkey.ControlType != ControlType.Pov
                )
            {
                return false;
            }
            if (hotkey.DirectInputControl == null) return false;
            if (hotkey.DirectInputDevice == null) return false;

            if (
                    diEvent.Control.ControlType == hotkey.DirectInputControl.ControlType
                        &&
                    diEvent.Control.ControlNum == hotkey.DirectInputControl.ControlNum
                        &&
                    (
                        (diEvent.Control.ControlType == ControlType.Axis && diEvent.Control.AxisType == hotkey.DirectInputControl.AxisType)
                            ||
                        (diEvent.Control.ControlType != ControlType.Axis)
                    )
                        &&
                    object.Equals(diEvent.Control.Parent.Key, hotkey.DirectInputDevice.Key)
                        &&
                    (
                        diEvent.Control.ControlType != ControlType.Pov
                           ||
                       (
                            hotkey.ControlType == ControlType.Pov
                                &&
                            hotkey.PovDirection == Common.InputSupport.Util.GetPovDirection(diEvent.CurrentState)
                       )
                    )
                        &&
                    (
                        diEvent.Control.ControlType != ControlType.Button
                            ||
                        (
                            diEvent.Control.ControlType == ControlType.Button
                                    &&
                            diEvent.CurrentState == 1
                        )
                    )
                )
            {
                return true;
            }
            return false;
        }
        private bool KeyIsHotkey(InputControlSelection hotkey, Keys keyPressed)
        {
            if (hotkey == null) return false;
            if (hotkey.ControlType == ControlType.Key)
            {
                if (
                        (hotkey.Keys & Keys.KeyCode) == (keyPressed & Keys.KeyCode)
                            &&
                        (hotkey.Keys & Keys.Modifiers) == (keyPressed & Keys.Modifiers)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        private void SetupKeyboardWatcherThread()
        {
            Common.Threading.Util.AbortThread(ref _keyboardWatcherThread);
            _keyboardWatcherThread = new Thread(KeyboardWatcherThreadWork);
            _keyboardWatcherThread.SetApartmentState(ApartmentState.STA);
            _keyboardWatcherThread.Priority = ThreadPriority.Highest;
            _keyboardWatcherThread.IsBackground = true;
            _keyboardWatcherThread.Name = "KeyboardWatcherThread";
        }
        public void ProcessKeyUpEvent(KeyEventArgs e)
        {
            _settingsManager.KeySettings.Reload();
            if (_settingsManager.KeySettings.EHSICourseDepressedKey == null) return;
            Keys modifiersPressedRightNow = UpdateKeyEventArgsWithExtendedKeyInfo(Keys.None);
            Keys modifiersInHotkey = (_settingsManager.KeySettings.EHSICourseDepressedKey.Keys & Keys.Modifiers);

            if (
                EHSIRightKnobIsCurrentlyDepressed()
                    &&
                (_settingsManager.KeySettings.EHSICourseDepressedKey.ControlType == ControlType.Key)
                    &&
                    (
                        (e.KeyData & Keys.KeyCode) == (_settingsManager.KeySettings.EHSICourseDepressedKey.Keys & Keys.KeyCode)
                            ||
                        ((e.KeyData & Keys.KeyCode) & ~Keys.LControlKey & ~Keys.RControlKey & ~Keys.LShiftKey & ~Keys.LMenu & ~Keys.RMenu) == Keys.None
                    )
                        &&
                    (
                        (
                            modifiersInHotkey == Keys.None
                        )
                            ||
                        (
                            ((modifiersInHotkey & Keys.Alt) == Keys.Alt && (modifiersPressedRightNow & Keys.Alt) != Keys.Alt)
                                ||
                            ((modifiersInHotkey & Keys.Control) == Keys.Control && (modifiersPressedRightNow & Keys.Control) != Keys.Control)
                                ||
                            ((modifiersInHotkey & Keys.Shift) == Keys.Shift && (modifiersPressedRightNow & Keys.Shift) != Keys.Shift)
                        )
                    )
                )
            {
                _messageManager.NotifyEHSIRightKnobReleased(true);
            }
        }
        public void ProcessKeyDownEvent(KeyEventArgs e)
        {
            _settingsManager.KeySettings.Reload();
            Keys keys = UpdateKeyEventArgsWithExtendedKeyInfo(e.KeyData);

            if (KeyIsHotkey(_settingsManager.KeySettings.NVISKey, keys))
            {
                _messageManager.NotifyNightModeIsToggled(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.AirspeedIndexIncreaseKey, keys))
            {
                _messageManager.NotifyAirspeedIndexIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.AirspeedIndexDecreaseKey, keys))
            {
                _messageManager.NotifyAirspeedIndexDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSIHeadingDecreaseKey, keys))
            {
                _messageManager.NotifyEHSILeftKnobDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSIHeadingIncreaseKey, keys))
            {
                _messageManager.NotifyEHSILeftKnobIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSICourseDecreaseKey, keys))
            {
                _messageManager.NotifyEHSIRightKnobDecreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSICourseIncreaseKey, keys))
            {
                _messageManager.NotifyEHSIRightKnobIncreasedByOne(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSICourseDepressedKey, keys) && !EHSIRightKnobIsCurrentlyDepressed())
            {
                _messageManager.NotifyEHSIRightKnobDepressed(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.EHSIMenuButtonDepressedKey, keys))
            {
                _messageManager.NotifyEHSIMenuButtonDepressed(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.ISISBrightButtonKey, keys))
            {
                _messageManager.NotifyISISBrightButtonDepressed(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.ISISStandardButtonKey, keys))
            {
                _messageManager.NotifyISISStandardButtonDepressed(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.AzimuthIndicatorBrightnessIncreaseKey, keys))
            {
                _messageManager.NotifyAzimuthIndicatorBrightnessIncreased(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.AzimuthIndicatorBrightnessDecreaseKey, keys))
            {
                _messageManager.NotifyAzimuthIndicatorBrightnessDecreased(true);
            }
            else if (KeyIsHotkey(_settingsManager.KeySettings.AccelerometerResetKey, keys))
            {
                _messageManager.NotifyAccelerometerIsReset(true);
            }

        }
        public static Keys UpdateKeyEventArgsWithExtendedKeyInfo(Keys keys)
        {
            if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
            {
                keys |= Keys.Shift;
                //SHIFT is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
            {
                keys |= Keys.Control;
                //CONTROL is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
            {
                keys |= Keys.Alt;
                //ALT is pressed
            }
            return keys;
        }

        private void KeyboardWatcherThreadWork()
        {
            AutoResetEvent resetEvent = null;
            Microsoft.DirectX.DirectInput.Device device = null;
            try
            {
                resetEvent = new AutoResetEvent(false);
                device = new Microsoft.DirectX.DirectInput.Device(Microsoft.DirectX.DirectInput.SystemGuid.Keyboard);
                device.SetCooperativeLevel(null, Microsoft.DirectX.DirectInput.CooperativeLevelFlags.Background | Microsoft.DirectX.DirectInput.CooperativeLevelFlags.NonExclusive);
                device.SetEventNotification(resetEvent);
                device.Properties.BufferSize = 255;
                device.Acquire();
                bool[] lastKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
                bool[] currentKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
                while (_keepRunning)
                {
                    resetEvent.WaitOne();
                    try
                    {
                        KeyboardState curState = device.GetCurrentKeyboardState();
                        Array possibleKeys = Enum.GetValues(typeof(Key));

                        int i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            currentKeyboardState[i] = curState[thisKey];
                            i++;
                        }

                        i = 0;
                        foreach (Key thisKey in possibleKeys)
                        {
                            bool isPressedNow = currentKeyboardState[i];
                            bool wasPressedBefore = lastKeyboardState[i];
                            Keys winFormsKey = (Keys)Common.Win32.NativeMethods.MapVirtualKey((uint)thisKey, Common.Win32.NativeMethods.MAPVK_VSC_TO_VK_EX);
                            if (isPressedNow && !wasPressedBefore)
                            {
                                ProcessKeyDownEvent(new KeyEventArgs(winFormsKey));
                            }
                            else if (wasPressedBefore && !isPressedNow)
                            {
                                ProcessKeyUpEvent(new KeyEventArgs(winFormsKey));
                            }
                            i++;
                        }
                        Array.Copy(currentKeyboardState, lastKeyboardState, currentKeyboardState.Length);
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
            }
            finally
            {
                if (device != null)
                {
                    device.Unacquire();
                }
                Common.Util.DisposeObject(device);
                device = null;

            }
        }

    }
}
