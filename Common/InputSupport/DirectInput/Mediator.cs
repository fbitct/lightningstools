﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    /// <summary>
    ///   Mediator is an object that sits between physical DirectInput input devices and application code.
    ///   Internally, Mediator keeps references to a set of DIDeviceMonitor objects, one per unique physical DirectInput device,
    ///   so that it can poll those monitor objects for state changes and raise corresponding events.
    /// </summary>
    public sealed class Mediator : IDisposable
    {
        #region Delegates

        /// <summary>
        ///   Event handler delegate for the PhysicalControlStateChanged event
        /// </summary>
        /// <param name = "sender">the Mediator object raising the event</param>
        /// <param name = "e">a PhysicalControlStateChangedEventArgs object representing the physical control whose state change is being signalled and its current/previous states.</param>
        public delegate void PhysicalControlStateChangedEventHandler(
            object sender, PhysicalControlStateChangedEventArgs e);

        #endregion

        #region Instance Variable Declarations

        private static readonly ILog Log = LogManager.GetLogger(typeof (Mediator));

        private readonly DIDeviceMonitor.DIStateChangedEventHandler _diDeviceMonitorStateChanged;

        /// <summary>
        ///   A Dictionary of DIDeviceMonitor objects, where the Dictionary's key 
        ///   is the DirectInput Device Instance GUID being monitored by the 
        ///   DIDeviceMonitor object contained in the corresponding Value.
        ///   Allows retrieving a running DIDeviceMonitor object from 
        ///   the collection by knowing its Device Instance GUID.
        /// </summary>
        private readonly Dictionary<Guid, DIDeviceMonitor> _diDeviceMonitors = new Dictionary<Guid, DIDeviceMonitor>();

        /// <summary>
        ///   A reference to the main Windows Form hosting the application consuming this Mediator.
        /// </summary>
        private readonly Control _parentForm;

        /// <summary>
        ///   Signal flag to indicate if this object is currently disposed.  
        ///   Used by the IDisposable.Dispose() implementation to avoid double-disposing.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        ///   Signal flag used internally to determine if initialization tasks have been performed.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        ///   Signal flag that determines whether events will be raised when physical control values change.
        /// </summary>
        private bool _raiseEvents = true;

        #endregion

        #region Constructors

        /// <summary>
        ///   Creates a Mediator instance.
        /// </summary>
        /// <param name = "parentForm">A reference to the main Windows Form hosting the application creating this Mediator.</param>
        public Mediator(Control parentForm)
        {
            _parentForm = parentForm;
            _diDeviceMonitorStateChanged = new DIDeviceMonitor.DIStateChangedEventHandler(DIMonitorStateChanged);
            Initialize();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Initializes this Mediator instance by creating a DeviceMonitor input 
        ///   monitoring object for each
        ///   detected unique PhysicalDeviceInfo
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            var oldSendEventsVal = _raiseEvents;
            _raiseEvents = false;
            DetectAttachedDIDevices();
            foreach (var monitor in _diDeviceMonitors.Values)
            {
                var physicalDevice = monitor.DeviceInfo;
                var monitorGuid = new Guid(physicalDevice.Key.ToString());
                if (!_diDeviceMonitors.ContainsKey(monitorGuid))
                {
                    var diMonitor = DIDeviceMonitor.GetInstance(physicalDevice, _parentForm, 0, 1024);
                    diMonitor.StateChanged += DIMonitorStateChanged;
                    _diDeviceMonitors.Add(monitorGuid, diMonitor);
                }
            }
            foreach (var diDevice in _diDeviceMonitors.Values)
            {
                try
                {
                    diDevice.Poll();
                }
                catch (ApplicationException e)
                {
                    Log.Debug(e.Message, e);
                }
            }
            _raiseEvents = oldSendEventsVal;
            _isInitialized = true;
        }

        private void DetectAttachedDIDevices()
        {
            _diDeviceMonitors.Clear();
            //get a list of joysticks that DirectInput can currently detect
            var detectedJoysticks = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AllDevices);
            var curDevice = 0;
            foreach (DeviceInstance instance in detectedJoysticks)
            {
                var deviceInfo = new DIPhysicalDeviceInfo(instance.InstanceGuid, instance.InstanceName)
                                     {DeviceNum = curDevice};
                //Get the DIDeviceMonitor object from the monitor pool that represents the input
                //device being evaluated.   If no monitor exists in the pool yet for that device,
                //one will be created and added to the pool.  This avoids having multiple objects
                //taking control of a device at the same time -- all communication with
                //the device itself will occur via the monitor object, not via DirectInput directly.
                var dev = DIDeviceMonitor.GetInstance(deviceInfo, _parentForm, 0, 1024);
                _diDeviceMonitors.Add(dev.DeviceInfo.Guid, dev);
                dev.StateChanged += _diDeviceMonitorStateChanged;
                curDevice++;
            }
        }

        private void DIMonitorStateChanged(object sender, DIStateChangedEventArgs e)
        {
            var source = (DIDeviceMonitor) sender;
            var device = source.DeviceInfo;
            foreach (var physicalControl in device.Controls)
            {
                ProcessStateChange(physicalControl);
            }
        }

        private void ProcessStateChange(PhysicalControlInfo physicalControl)
        {
            //get the previous and current state of this input control
            var prevValue = GetPhysicalControlValue(physicalControl, StateType.Previous);
            var newValue = GetPhysicalControlValue(physicalControl, StateType.Current);

            //if the current state of this control is different than the previous state, and if raising of change-notification events is enabled,
            //then raise the appropriate event
            if (_raiseEvents && newValue != prevValue && PhysicalControlStateChanged != null)
            {
                var args = new PhysicalControlStateChangedEventArgs(physicalControl,
                                                                    newValue.HasValue ? newValue.Value : 0,
                                                                    prevValue.HasValue ? prevValue.Value : 0);
                PhysicalControlStateChanged(this, args);
            }
        }


        /// <summary>
        ///   Gets the value of a specific physical control 
        ///   (either the current value or the previous value).  
        ///   Performs Pov-to-axis-range translation and auto-detects 
        ///   axis types, reading the corresponding value 
        ///   from the state object(s) returned from the 
        ///   corresponding DeviceMonitor object.
        /// </summary>
        /// <param name = "control">a PhysicalControlInfo object representing the 
        ///   physical control whose value should be returned</param>
        /// <param name = "stateType">a value from the StateType enum indicating 
        ///   whether to return the current state or the previous state of the 
        ///   specified physical control</param>
        /// <returns>a <see cref = "Nullable" /> integer, whose Value will be set to the requested 
        ///   value if it can be obtained, or null if it cannot be obtained.
        /// </returns>
        public int? GetPhysicalControlValue(PhysicalControlInfo control, StateType stateType)
        {
            //verify method arguments are valid
            if (control == null)
            {
                throw new ArgumentException("Invalid or missing control value specified.", "control");
            }

            var devInfo = control.Parent;
            if (devInfo == null)
            {
                throw new ArgumentException("The .Parent property was not set on the supplied control", "control");
            }

            if (devInfo is DIPhysicalDeviceInfo)
            {
                if (_diDeviceMonitors == null || _diDeviceMonitors.Count < 1)
                {
                    throw new InvalidOperationException("No DirectInput physical devices were found.");
                }

                var currentJoystick = _diDeviceMonitors[new Guid(devInfo.Key.ToString())];
                if (currentJoystick == null)
                {
                    throw new ArgumentException(
                        "No physical joystick was found matching the Guid supplied in the 'control' parameter's .Parent.Key property.",
                        "control");
                }

                //retrieve the requested JoystickState structure (current or previous)
                //from the relevant physical joystick's monitor object
                var state = stateType == StateType.Current
                                ? currentJoystick.CurrentState
                                : currentJoystick.PreviousState;

                //if the supplied input control object refers to a Button, then read
                //the state in terms of that button
                if (control.ControlType == ControlType.Button)
                {
                    byte buttonState;

                    if (state.HasValue)
                    {
                        //get the state of just this button from the larger state-bag
                        buttonState = state.Value.GetButtons()[control.ControlNum];
                    }
                    else
                    {
                        return null; //no state was found, so we can't return a value
                    }
                    return (buttonState & 0x80) != 0 ? 1 : 0;
                }
                //else if the supplied input control object refers to an Axis, then read
                //the state in terms of that axis
                if (control.ControlType == ControlType.Axis)
                {
                    int? toReturn = null;

                    if (state.HasValue)
                    {
                        switch (control.AxisType)
                        {
                            case AxisType.X:
                                toReturn = state.Value.X;
                                break;
                            case AxisType.Y:
                                toReturn = state.Value.Y;
                                break;
                            case AxisType.Z:
                                toReturn = state.Value.Z;
                                break;
                            case AxisType.XR:
                                toReturn = state.Value.Rx;
                                break;
                            case AxisType.YR:
                                toReturn = state.Value.Ry;
                                break;
                            case AxisType.ZR:
                                toReturn = state.Value.Rz;
                                break;
                            case AxisType.Slider:
                                toReturn = state.Value.GetSlider()[control.ControlNum];
                                break;
                            default:
                                throw new ArgumentException("Unsupported control type", "control");
                        }
                    }
                    return toReturn;
                }
                if (control.ControlType == ControlType.Pov)
                {
                    if (state.HasValue)
                    {
                        return state.Value.GetPointOfView()[control.ControlNum];
                    }
                    return null;
                }
                throw new ArgumentException("Unsupported control type", "control");
            }
            throw new ArgumentException("Unsupported control type", "control");
        }

        #endregion

        #region Public Properties

        public Dictionary<Guid, DIDeviceMonitor> DeviceMonitors
        {
            get { return _diDeviceMonitors; }
        }

        public bool RaiseEvents
        {
            get { return _raiseEvents; }
            set { _raiseEvents = value; }
        }

        #endregion

        #region Destructors

        /// <summary>
        ///   Public implementation of IDisposable.Dispose().  Cleans up managed and unmanaged objects
        ///   and suppresses finalization by the garbage collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Standard finalizer.  Normally only called by the garbage collector.
        /// </summary>
        ~Mediator()
        {
            Dispose();
        }

        /// <summary>
        ///   Private implementation of Dispose()
        /// </summary>
        /// <param name = "disposing">flag to indicate whether disposal is occurring</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    foreach (var pstick in _diDeviceMonitors.Values)
                    {
                        try
                        {
                            pstick.StateChanged -= _diDeviceMonitorStateChanged;
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                        try
                        {
                            pstick.Dispose();
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message, e);
                        }
                    }
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        #endregion

        /// <summary>
        ///   Raised whenever one of the physical controls on a DirectInput device has changed
        /// </summary>
        public event PhysicalControlStateChangedEventHandler PhysicalControlStateChanged;
    }
}