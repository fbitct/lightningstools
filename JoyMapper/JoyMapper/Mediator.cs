#region Using statements

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport;
using Common.InputSupport.BetaInnovations;
using Common.InputSupport.DirectInput;
using Common.InputSupport.Phcc;
using JoyMapper.Properties;
using log4net;
using Microsoft.DirectX.DirectInput;
using PPJoy;
using Device = PPJoy.Device;

#endregion

namespace JoyMapper
{
    /// <summary>
    /// Event arguments class for the PhysicalControlStateChanged Event.  Extends EventArgs.
    /// </summary>
    public sealed class PhysicalControlStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// a PhysicalControlInfo object representing the physical control whose state change is being signalled
        /// </summary>
        private readonly PhysicalControlInfo _control;

        /// <summary>
        /// an integer value indicating the current state of the physical control whose state change is being signalled
        /// </summary>
        private readonly int _currentState;

        /// <summary>
        /// an integer value indicating the previous state of the physical control whose state changed is being signalled
        /// </summary>
        private readonly int _previousState;

        /// <summary>
        /// Creates a new PhysicalControlStateChangedEventArgs event argument object and sets its required properties in a single call.
        /// </summary>
        /// <param name="control">a PhysicalControlInfo object representing the physical control (axis, button, Pov, etc) whose state change is being signalled by raising the event</param>
        /// <param name="currentstate">an integer value indicating the current state of the physical control whose state change is being signalled</param>
        /// <param name="previousState">an integer value indicating the previous state of the physical control whose state change is being signalled</param>
        public PhysicalControlStateChangedEventArgs(PhysicalControlInfo control, int currentstate, int previousState)
        {
            _control = control;
            _currentState = currentstate;
            _previousState = previousState;
        }

        /// <summary>
        /// Gets the PhysicalControlInfo object representing the physical control (axis, button, Pov, etc) whose state change is being signalled 
        /// </summary>
        public PhysicalControlInfo Control
        {
            get { return _control; }
        }

        /// <summary>
        ///Gets an integer value indicating the current state of the physical control whose state change is being signalled.  For axes, this will
        /// be a value in the range specified by the DIDeviceMonitor's axisRangeMin and axisRangeMax values.  For Povs, it will
        /// be a value in the same range as the axis, but can also be set to -1, indicating centered.  For Povs, the translation from
        /// degrees to linear values in the corresponding axis range will already have been performed.  For buttons, this value is either zero or one, 
        /// where 0=unpressed and 1=pressed.  Any negative value <1 indicates an error during polling.
        /// </summary>
        public int CurrentState
        {
            get { return _currentState; }
        }

        /// <summary>
        ///Gets an integer value indicating the previous state of the physical control whose state change is being signalled.  For axes, this will
        /// be a value in the range specified by the DIDeviceMonitor's axisRangeMin and axisRangeMax values.  For Povs, it will
        /// be a value in the same range as the axis, but can also be set to -1, indicating centered.  For Povs, the translation from
        /// degrees to linear values in the corresponding axis range will already have been performed.  For buttons, this value is either zero or one, 
        /// where 0=unpressed and 1=pressed.  Any negative value <1 indicates an error during polling.
        /// </summary>
        public int PreviousState
        {
            get { return _previousState; }
        }
    }

    /// <summary>
    /// Mediator is an object that sits between physical input devices and (optionally) one or more PPJoy virtual joysticks.
    /// Meditor relies on an Output Map to define how to map input control events from specific input devices to
    /// their corresponding virtual (output) devices and virtual controls on those virtual devices.  If no virtual
    /// output is desired, an Output Map can be supplied where all the output controls are set to NULL, in which case,
    /// Mediator can still raise events whenever the state of input controls in the map have changed.
    /// 
    /// Internally, Mediator keeps references to a set of DIDeviceMonitor objects, one per unique physical device in the output map,
    /// so that it can poll those monitor objects for state changes and raise corresponding events.
    /// </summary>
    public sealed class Mediator : IDisposable
    {
        #region Delegates

        /// <summary>
        /// Event handler delegate for the PhysicalControlStateChanged event
        /// </summary>
        /// <param name="sender">the Mediator object raising the event</param>
        /// <param name="e">a PhysicalControlStateChangedEventArgs object representing the physical control whose state change is being signalled and its current/previous states.</param>
        public delegate void PhysicalControlStateChangedEventHandler(
            object sender, PhysicalControlStateChangedEventArgs e);

        #endregion

        #region Instance Variable Declarations

        private static readonly ILog _log = LogManager.GetLogger(typeof (Mediator));

        /// <summary>
        /// A Dictionary of BIDeviceMonitor objects, where the Dictionary's key 
        /// is the Product ID of the BetaInnovations device being monitored by the 
        /// BIDeviceMonitor object contained in the corresponding Value.
        /// Allows retrieving a running BIDeviceMonitor object from 
        /// the collection by knowing its Product ID.
        /// </summary>
        private readonly Dictionary<string, BIDeviceMonitor> _biDeviceMonitors =
            new Dictionary<string, BIDeviceMonitor>();

        /// <summary>
        /// A Dictionary of DIDeviceMonitor objects, where the Dictionary's key 
        /// is the DirectInput Device Instance GUID being monitored by the 
        /// DIDeviceMonitor object contained in the corresponding Value.
        /// Allows retrieving a running DIDeviceMonitor object from 
        /// the collection by knowing its Device Instance GUID.
        /// </summary>
        private readonly Dictionary<Guid, DIDeviceMonitor> _diDeviceMonitors = new Dictionary<Guid, DIDeviceMonitor>();

        /// <summary>
        /// A reference to the main Windows Form hosting the application consuming this Mediator.  
        /// </summary>
        private readonly Control _parentForm;

        /// <summary>
        /// A Dictionary of PHCCDeviceMonitor objects, where the Dictionary's key 
        /// is the COM port name that the device being monitored by the 
        /// BIDeviceMonitor object contained in the corresponding Value is attached to.
        /// Allows retrieving a running PHCCDeviceMonitor object from 
        /// the collection by knowing its COM port.
        /// </summary>
        private readonly Dictionary<string, PHCCDeviceMonitor> _phccDeviceMonitors =
            new Dictionary<string, PHCCDeviceMonitor>();

        /// <summary>
        /// A Dictionary of VirtualJoystick objects, where the Dictionary's key is the virtual joystick number of the VirtualJoystick contained in the corresponding Value.
        /// Allows retrieving a running VirtualJoystick object from the collection, by knowing its virtual joystick number
        /// </summary>
        private readonly Dictionary<int, VirtualJoystick> _virtualJoysticks = new Dictionary<int, VirtualJoystick>();

        private DIDeviceMonitor.DIStateChangedEventHandler _diMonitorStateChangedHandler;
        private PhysicalControlInfo[] _enabledPhysicalControls;

        /// <summary>
        /// Signal flag to indicate if this object is currently disposed.  
        /// Used by the IDisposable.Dispose() implementation to avoid double-disposing.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Signal flag used internally to determine if initialization tasks have been performed.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Signal flag to inform a running MediatorWork worker thread that it should quit before
        /// its next polling loop.  Can be exposed via a public property getter/setter.
        /// </summary>
        private bool _keepRunning;

        /// <summary>
        /// Stores a reference to an OutputMap object which contains a
        /// list of input controls to watch, and (optionally) a set of 
        /// corresponding virtual (output) controls (via PPJoy virtual joysticks) 
        /// to send data to based on changes to the state of the corresponding input control.
        /// </summary>
        private OutputMap _map;

        private PHCCDeviceMonitor.PHCCStateChangedEventHandler _phccMonitorStateChangedHandler;

        /// <summary>
        /// Signal flag that determines whether events will be raised when physical control values change.
        /// </summary>
        private bool _raiseEvents = true;

        /// <summary>
        /// Signal flag that indicates if this Mediator is currently 
        /// actively watching the input devices referred to in the OutputMap.
        /// Should not be manually lowered.  To lower, set the _keepRunning flag to low, and
        /// the running work thread will gracefully exit, and when it does, it will lower this flag.
        /// </summary>
        private bool _running;

        /// <summary>
        /// Signal flag to control the behavior of the Mediator -- if set to true, then the 
        /// Mediator will send output to the corresponding virtual controls in the output map
        /// whenever changes occur on the corresponding physical controls, 
        /// while the Mediator is running. If set to false, then all other Mediator behavior will
        /// continue normally (raising events, etc.) as configured, but no output will be sent to
        /// the virtual controls.
        /// </summary>
        private bool _sendOutput;

        #endregion

        #region Constructors

        private Mediator()
        {
            SetupInputDeviceStateChangedEventHandlers();
        }

        /// <summary>
        /// Creates a Mediator with an empty output map.
        /// </summary>
        /// <param name="parentForm">A reference to the main Windows Form hosting the application creating this Mediator.</param>
        public Mediator(Control parentForm) : this()
        {
            _parentForm = parentForm;
        }

        /// <summary>
        /// Creates a Mediator using a specific output map.
        /// </summary>
        /// <param name="parentForm">A reference to the main Windows Form hosting the application creating this Mediator.</param>
        /// <param name="map">An OutputMap object containing an output map definition to use during mediation.</param>
        public Mediator(OutputMap map, Control parentForm) : this(parentForm)
        {
            _map = map;
        }

        private void SetupInputDeviceStateChangedEventHandlers()
        {
            _diMonitorStateChangedHandler = new DIDeviceMonitor.DIStateChangedEventHandler(diMonitor_StateChanged);
            _phccMonitorStateChangedHandler =
                new PHCCDeviceMonitor.PHCCStateChangedEventHandler(phccMonitor_StateChanged);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes this Mediator instance by creating a DeviceMonitor input 
        /// monitoring object for each
        /// unique PhysicalDeviceInfo referenced in the output map,
        /// and creates a PPJoy VirtualJoystick output wrapper object for each
        /// VirtualDeviceInfo referenced in the output map.
        /// </summary>
        private void Initialize()
        {
            //TODO: document this method
            if (_isInitialized)
            {
                return;
            }
            if (_map != null)
            {
                bool oldSendEventsVal = _raiseEvents;
                _raiseEvents = false;
                _enabledPhysicalControls = _map.EnabledPhysicalControls;
                SetupPhysicalInputDevices();
                //SetupVirtualJoysticks();
                _raiseEvents = oldSendEventsVal;
            }
            else
            {
                _enabledPhysicalControls = null;
            }
            _isInitialized = true;
        }

        private void SetupPhysicalInputDevices()
        {
            foreach (PhysicalDeviceInfo physicalDevice in _map.EnabledPhysicalDevices)
            {
                if (physicalDevice is DIPhysicalDeviceInfo)
                {
                    var monitorGuid = new Guid(physicalDevice.Key.ToString());
                    if (!_diDeviceMonitors.ContainsKey(monitorGuid))
                    {
                        DIDeviceMonitor diMonitor = DIDeviceMonitor.GetInstance((DIPhysicalDeviceInfo) physicalDevice,
                                                                                _parentForm,
                                                                                VirtualJoystick.MinAnalogDataSourceVal,
                                                                                VirtualJoystick.MaxAnalogDataSourceVal);
                        diMonitor.StateChanged += _diMonitorStateChangedHandler;
                        _diDeviceMonitors.Add(monitorGuid, diMonitor);
                    }
                }
                else if (physicalDevice is BIPhysicalDeviceInfo)
                {
                    string devicePath = physicalDevice.Key != null ? physicalDevice.Key.ToString() : string.Empty;
                    if (!_biDeviceMonitors.ContainsKey(devicePath))
                    {
                        BIDeviceMonitor biMonitor = BIDeviceMonitor.GetInstance((BIPhysicalDeviceInfo) physicalDevice);
                        _biDeviceMonitors.Add(devicePath, biMonitor);
                    }
                }
                else if (physicalDevice is PHCCPhysicalDeviceInfo)
                {
                    string monitorId = physicalDevice.Key.ToString();
                    if (!_phccDeviceMonitors.ContainsKey(monitorId))
                    {
                        PHCCDeviceMonitor phccMonitor =
                            PHCCDeviceMonitor.GetInstance((PHCCPhysicalDeviceInfo) physicalDevice,
                                                          VirtualJoystick.MinAnalogDataSourceVal,
                                                          VirtualJoystick.MaxAnalogDataSourceVal);
                        phccMonitor.StateChanged += _phccMonitorStateChangedHandler;
                        _phccDeviceMonitors.Add(monitorId, phccMonitor);
                    }
                }
            }
        }

        private void SetupVirtualJoysticks()
        {
            TeardownVirtualJoysticks();
            Device[] ppJoyDevices = new DeviceManager().GetAllDevices();
            for (int i = 0; i < ppJoyDevices.Length; i++)
            {
                if (ppJoyDevices[i].DeviceType == JoystickTypes.Virtual_Joystick)
                {
                    var virtualStick = new VirtualJoystick(ppJoyDevices[i].UnitNum + 1);
                    _virtualJoysticks.Add(ppJoyDevices[i].UnitNum + 1, virtualStick);
                }
            }
        }

        private void phccMonitor_StateChanged(object sender, PHCCStateChangedEventArgs e)
        {
            var source = (PHCCDeviceMonitor) sender;
            PHCCPhysicalDeviceInfo device = source.DeviceInfo;
            var enabledControls = new List<PhysicalControlInfo>(_enabledPhysicalControls);
            PhysicalControlInfo inputControl = null;
            if (e.ControlType == ControlType.Button)
            {
                foreach (PhysicalControlInfo button in device.Buttons)
                {
                    if (button.ControlNum == e.ControlIndex)
                    {
                        inputControl = button;
                        break;
                    }
                }
            }
            else if (e.ControlType == ControlType.Axis)
            {
                foreach (PhysicalControlInfo axis in device.Axes)
                {
                    if (axis.ControlNum == e.ControlIndex)
                    {
                        inputControl = axis;
                        break;
                    }
                }
            }
            if (enabledControls.Contains(inputControl))
            {
                VirtualControlInfo outputControl = _map.GetMapping(inputControl);

                if (_phccDeviceMonitors.ContainsKey(device.Key.ToString()))
                {
                    ProcessStateChange(inputControl, outputControl);
                }
            }
        }

        private void diMonitor_StateChanged(object sender, DIStateChangedEventArgs e)
        {
            var source = (DIDeviceMonitor) sender;
            DIPhysicalDeviceInfo device = source.DeviceInfo;
            Guid deviceId = device.Guid;
            var enabledControls = new List<PhysicalControlInfo>(_enabledPhysicalControls);
            foreach (PhysicalControlInfo physicalControl in device.Controls)
            {
                if (enabledControls.Contains(physicalControl))
                {
                    //get the corresponding output control (virtual control) 
                    //from the output map
                    VirtualControlInfo outputControl = _map.GetMapping(physicalControl);

                    if (_diDeviceMonitors.ContainsKey(deviceId))
                    {
                        ProcessStateChange(physicalControl, outputControl);
                    }
                }
            }
        }

        /// <summary>
        /// Main worker thread for Mediator.  Contains the main polling 
        /// loop which reads from physical devices and sends output 
        /// to virtual devices, as well as raising events, if
        /// so configured.
        /// </summary>
        private void MediatorWork()
        {
            //TODO: document this method

            //if there's no output map, there's nothing to do, so return
            if (_map == null)
            {
                return;
            }

            //set the Running flag
            _running = true;

            //initialize the mediator if it's not in the initialized state.
            if (!_isInitialized)
            {
                Initialize();
            }

            bool firstPass = true;
            while (_keepRunning) //main polling loop
            {
                try
                {
                    //initialize the mediator if it's not in the initialized state.
                    //We check for this each time through the loop in case
                    //something goes wrong, we can re-initialize and continue
                    if (!_isInitialized)
                    {
                        Initialize();
                    }

                    foreach (BIDeviceMonitor biDevice in _biDeviceMonitors.Values)
                    {
                        try
                        {
                            biDevice.Poll(false);
                        }
                        catch (ApplicationException e)
                        {
                            _log.Debug(e.Message, e);
                        }
                    }
                    if (firstPass)
                    {
                        foreach (PHCCDeviceMonitor phccDevice in _phccDeviceMonitors.Values)
                        {
                            try
                            {
                                phccDevice.Poll();
                            }
                            catch (ApplicationException)
                            {
                            }
                        }
                    }
                    if (firstPass)
                    {
                        foreach (DIDeviceMonitor diDevice in _diDeviceMonitors.Values)
                        {
                            try
                            {
                                diDevice.Poll();
                            }
                            catch (ApplicationException e)
                            {
                                _log.Debug(e.Message, e);
                            }
                        }
                    }
                    //for each input control listed in the output map, determine if 
                    //the state has changed for that control, and if so,
                    //update PPJoy if necessary and raise the appropriate state-changed
                    //events if necessary.
                    foreach (PhysicalControlInfo physicalControl in _enabledPhysicalControls)
                    {
                        //get the corresponding output control (virtual control) 
                        //from the output map
                        VirtualControlInfo outputControl = _map.GetMapping(physicalControl);

                        //if the physical joystick containing this physical control is known in the
                        //joystick collection (as created during the Initialize() call),
                        //then we can do something about checking the state of this control
                        PhysicalDeviceInfo device = physicalControl.Parent;
                        bool isKnownDevice = false;

                        if (device is DIPhysicalDeviceInfo && firstPass)
                        {
                            if (_diDeviceMonitors.ContainsKey(new Guid(device.Key.ToString())))
                            {
                                isKnownDevice = true;
                            }
                        }
                        else if (device is BIPhysicalDeviceInfo)
                        {
                            string devicePath = device.Key != null ? device.Key.ToString() : string.Empty;
                            if (_biDeviceMonitors.ContainsKey(devicePath))
                            {
                                isKnownDevice = true;
                            }
                        }
                        else if (device is PHCCPhysicalDeviceInfo && firstPass)
                        {
                            if (_phccDeviceMonitors.ContainsKey(device.Key.ToString()))
                            {
                                isKnownDevice = true;
                            }
                        }
                        if (isKnownDevice)
                        {
                            //DI and PHCC device updates will not be handled by polling (i.e. won't be handled here), they'll happen via event handlers
                            ProcessStateChange(physicalControl, outputControl);
                        }
                    }

                    //now that all analog and digital data source values have been updated in-memory,
                    //it's time to send those updates in bulk to PPJoy.
                    if (_virtualJoysticks != null)
                    {
                        try
                        {
                            foreach (VirtualJoystick virtualStick in _virtualJoysticks.Values)
                            {
                                try
                                {
                                    virtualStick.SendUpdates();
                                }
                                catch (ApplicationException ex)
                                {
                                    _log.Debug(ex.Message, ex);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Debug(e.Message, e);
                        }
                    }
                    //since this is a polling loop, let's take some time to allow the
                    //application's event sink to do its work
                    if (_parentForm != null)
                    {
                        //Application.DoEvents();
                    }
                    int pollingPeriod = Settings.Default.PollEveryNMillis;
                    Thread.Sleep(pollingPeriod); //don't chew up 100% CPU 
                    firstPass = false;
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            //if we reach here, the _keepRunning flag has lowered
            _running = false;
        }

        private void ProcessStateChange(PhysicalControlInfo physicalControl, VirtualControlInfo outputControl)
        {
            VirtualJoystick virtualStick = null;

            //find the virtual joystick that contains the output control that is mapped to this input control, if any
            if (outputControl != null && _virtualJoysticks != null &&
                _virtualJoysticks.ContainsKey(outputControl.Parent.VirtualDeviceNum))
            {
                virtualStick = _virtualJoysticks[outputControl.Parent.VirtualDeviceNum];
            }

            //get the previous and current state of this input control
            int? prevValue = GetPhysicalControlValue(physicalControl, StateType.Previous);
            int? newValue = GetPhysicalControlValue(physicalControl, StateType.Current);
            ;

            if (newValue.HasValue) //if there's a new value (current value) for this input control
            {
                //if this control is an axis, then set the appropriate PPJoy analog data source value to match
                if (physicalControl.ControlType == ControlType.Axis || physicalControl.ControlType == ControlType.Pov)
                {
                    if (_sendOutput && virtualStick != null)
                    {
                        //only do this if we actually found the virtual joystick that the output control appears on
                        //and if we are supposed to be sending output to PPJoy (i.e. the _sendOutput flag is high)
                        virtualStick.SetAnalogDataSourceValue(outputControl.ControlNum, newValue.Value);
                    }
                }
                    //if this control is a button, then set the appropriate PPJoy digital data source value to match
                else if (physicalControl.ControlType == ControlType.Button)
                {
                    //determine if the input control's (button's) state is "pressed"
                    bool pressed = false;
                    if (newValue == 1)
                    {
                        pressed = true;
                    }

                    if (_sendOutput && virtualStick != null)
                    {
                        //only do this if we actually found the virtual joystick that the output control appears on
                        //and if we are supposed to be sending output to PPJoy (i.e. the _sendOutput flag is high)
                        virtualStick.SetDigitalDataSourceState(outputControl.ControlNum, pressed);
                    }
                }
            }
            //if the current state of this control is different than the previous state, and if raising of change-notification events is enabled,
            //then raise the appropriate event
            if (_raiseEvents && newValue.HasValue &&
                ((prevValue.HasValue && newValue.Value != prevValue.Value) || !prevValue.HasValue))
            {
                var args = new PhysicalControlStateChangedEventArgs(physicalControl,
                                                                    newValue.HasValue ? newValue.Value : 0,
                                                                    prevValue.HasValue ? prevValue.Value : 0);
                PhysicalControlStateChanged(this, args);
            }
        }


        /// <summary>
        /// Gets the value of a specific physical control 
        /// (either the current value or the previous value).  
        /// Performs Pov-to-axis-range translation and auto-detects 
        /// axis types, reading the corresponding value 
        /// from the state object(s) returned from the 
        /// corresponding DeviceMonitor object.
        /// </summary>
        /// <param name="control">a PhysicalControlInfo object representing the 
        /// physical control whose value should be returned</param>
        /// <param name="stateType">a value from the StateType enum indicating 
        /// whether to return the current state or the previous state of the 
        /// specified physical control</param>
        /// <returns>a <see cref="Nullable"/> integer, whose Value will be set to the requested 
        /// value if it can be obtained, or null if it cannot be obtained.
        /// </returns>
        private int? GetPhysicalControlValue(PhysicalControlInfo control, StateType stateType)
        {
            //verify method arguments are valid
            if (control == null)
            {
                throw new ArgumentException("Invalid or missing control value specified.", "control");
            }

            PhysicalDeviceInfo devInfo = control.Parent;
            if (devInfo == null)
            {
                throw new ArgumentException("The .Parent property was not set on the supplied control", "control");
            }

            if (devInfo is DIPhysicalDeviceInfo)
            {
                if (_diDeviceMonitors == null || _diDeviceMonitors.Count < 1)
                {
                    throw new InvalidOperationException("No DirectInput physical devices were found in the output map.");
                }

                JoystickState? state;

                DIDeviceMonitor currentJoystick = _diDeviceMonitors[new Guid(devInfo.Key.ToString())];
                if (currentJoystick == null)
                {
                    throw new ArgumentException(
                        "No physical joystick was found matching the Guid supplied in the 'control' parameter's .Parent.Key property.",
                        "control");
                }

                //retrieve the requested JoystickState structure (current or previous)
                //from the relevant physical joystick's monitor object
                if (stateType == StateType.Current)
                {
                    state = currentJoystick.CurrentState; //TODO: implement this for all device types
                }
                else
                {
                    state = currentJoystick.PreviousState; //TODO: implement this for all device types
                }

                //if the supplied input control object refers to a Button, then read
                //the state in terms of that button
                if (control.ControlType == ControlType.Button)
                {
                    byte buttonState = 0;

                    if (state.HasValue)
                    {
                        //get the state of just this button from the larger state-bag
                        buttonState = state.Value.GetButtons()[control.ControlNum];
                    }
                    else
                    {
                        return null; //no state was found, so we can't return a value
                    }
                    if ((buttonState & 0x80) != 0)
                    {
                        return 1; //return a 1 if the button is in the pressed state 
                    }
                    else
                    {
                        return 0; //return a 0 if the button is in the released state
                    }
                }
                    //else if the supplied input control object refers to an Axis, then read
                    //the state in terms of that axis
                else if (control.ControlType == ControlType.Axis)
                {
                    //TODO: finish documenting this method
                    int? toReturn = null;

                    if (state.HasValue)
                    {
                        if (control.AxisType == AxisType.X)
                        {
                            toReturn = state.Value.X;
                        }
                        else if (control.AxisType == AxisType.Y)
                        {
                            toReturn = state.Value.Y;
                        }
                        else if (control.AxisType == AxisType.Z)
                        {
                            toReturn = state.Value.Z;
                        }
                        else if (control.AxisType == AxisType.XR)
                        {
                            toReturn = state.Value.Rx;
                        }
                        else if (control.AxisType == AxisType.YR)
                        {
                            toReturn = state.Value.Ry;
                        }
                        else if (control.AxisType == AxisType.ZR)
                        {
                            toReturn = state.Value.Rz;
                        }
                        else if (control.AxisType == AxisType.Slider)
                        {
                            toReturn = state.Value.GetSlider()[control.ControlNum];
                        }
                        else
                        {
                            throw new ArgumentException("Unsupported control type", "control");
                        }
                    }
                    return toReturn;
                }
                else if (control.ControlType == ControlType.Pov)
                {
                    int degrees = 0;
                    if (state.HasValue)
                    {
                        degrees = state.Value.GetPointOfView()[control.ControlNum];
                    }
                    else
                    {
                        return null;
                    }
                    if (degrees == -1)
                    {
                        return -1;
                    }
                    else
                    {
                        int min = VirtualJoystick.MinAnalogDataSourceVal;
                        int max = VirtualJoystick.MaxAnalogDataSourceVal;
                        double linearPctage = (degrees/(double) 36000);
                        int scaleLength = (max - min) + 2;
                        double linearVal = (linearPctage*scaleLength);
                        var intVal = (int) (Math.Round(linearVal, MidpointRounding.ToEven));
                        if (intVal > 0 && degrees < 31500)
                        {
                            intVal++;
                        }
                        else if (degrees == 31500)
                        {
                            intVal = 28673;
                        }
                        return intVal;
                    }
                }
                else
                {
                    throw new ArgumentException("Unsupported control type", "control");
                }
            }
            else if (devInfo is BIPhysicalDeviceInfo)
            {
                if (_biDeviceMonitors == null || _biDeviceMonitors.Count < 1)
                {
                    throw new InvalidOperationException(
                        "No BetaInnovations physical devices were found in the output map.");
                }

                bool[] state = null;
                string devicePath = devInfo.Key != null ? devInfo.Key.ToString() : string.Empty;
                BIDeviceMonitor currentJoystick = _biDeviceMonitors[devicePath];
                if (currentJoystick == null)
                {
                    throw new ArgumentException(
                        "No BetaInnovations physical device was found matching the Registry device path supplied in the 'control' parameter's .Parent.Key property.",
                        "control");
                }

                //retrieve the requested input state array (current or previous)
                //from the relevant input device's monitor object
                if (stateType == StateType.Current)
                {
                    state = currentJoystick.CurrentState;
                }
                else
                {
                    state = currentJoystick.PreviousState;
                }

                //read the state 
                if (control.ControlType == ControlType.Button)
                {
                    bool buttonState = false;

                    if (state != null)
                    {
                        //get the state of just this button from the larger state-bag
                        buttonState = state[control.ControlNum];
                    }
                    else
                    {
                        return null; //no state was found
                    }

                    if (buttonState)
                    {
                        return 1; //return a 1 if the button is in the pressed state 
                    }
                    else
                    {
                        return 0; //return a 0 if the button is in the released state
                    }
                }
                else
                {
                    throw new ArgumentException("Unsupported control type", "control");
                }
            }
            else if (devInfo is PHCCPhysicalDeviceInfo)
            {
                if (_phccDeviceMonitors == null || _phccDeviceMonitors.Count < 1)
                {
                    throw new InvalidOperationException("No PHCC physical devices were found in the output map.");
                }

                PHCCInputState? state;

                PHCCDeviceMonitor currentPhcc = _phccDeviceMonitors[devInfo.Key.ToString()];
                if (currentPhcc == null)
                {
                    throw new ArgumentException(
                        "No PHCC physical device was found on the COM port supplied in the 'control' parameter's .Parent.Key property.",
                        "control");
                }

                //retrieve the requested input state array (current or previous)
                //from the relevant input device's monitor object
                if (stateType == StateType.Current)
                {
                    state = currentPhcc.CurrentState;
                }
                else
                {
                    state = currentPhcc.PreviousState;
                }

                //read the state 
                if (control.ControlType == ControlType.Button)
                {
                    bool buttonState = false;

                    if (state != null)
                    {
                        //get the state of just this button from the larger state-bag
                        buttonState = state.Value.digitalInputs[control.ControlNum];
                    }
                    else
                    {
                        return null; //no state was found
                    }

                    if (buttonState)
                    {
                        return 1; //return a 1 if the button is in the pressed state 
                    }
                    else
                    {
                        return 0; //return a 0 if the button is in the released state
                    }
                }
                    //else if the supplied input control object refers to an Axis, then read
                    //the state in terms of that axis
                else if (control.ControlType == ControlType.Axis)
                {
                    int? toReturn = null;
                    if (state.HasValue)
                    {
                        toReturn = state.Value.analogInputs[control.ControlNum];
                    }
                    return toReturn;
                }
                else
                {
                    throw new ArgumentException("Unsupported control type", "control");
                }
            }
            else
            {
                throw new ArgumentException("Unsupported control type", "control");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Causes mediation to begin, according to the mappings specified in the Output Map.  
        /// </summary>
        public void StartMediating()
        {
            if (_running) return;
            _keepRunning = true;
            var t = new Thread(MediatorWork);
            t.SetApartmentState(ApartmentState.STA);
            t.Name = "MediatorThread";
            t.IsBackground = true;
            t.Start();
        }

        public void ResetMonitors()
        {
            /*
            PHCCDeviceMonitor.DestroyAllInstances();
            _phccDeviceMonitors.Clear();
             */
        }

        /// <summary>
        /// Stops mediation after the next polling loop.  
        /// </summary>
        public void StopMediating()
        {
            _keepRunning = false;
            int elapsed = 0;
            int timeout = 1000;
            while (_running && (elapsed < timeout))
            {
                Thread.Sleep(20);
                Application.DoEvents();
                elapsed += 20;
            }
        }

        #endregion

        #region Public Properties

        public bool RaiseEvents
        {
            get { return _raiseEvents; }
            set { _raiseEvents = value; }
        }

        /// <summary>
        /// Gets a value that indicates if this Mediator is in the Running state.
        /// </summary>
        public bool IsRunning
        {
            get { return _running; }
        }

        /// <summary>
        /// Sets or gets a value indicating if this Mediator should send output data to the virtual controls defined in the Output Map (if any).  If false, 
        /// this Mediator will still read from the physical controls defined in the Output Map, but it will not update any of the virtual control values.  If true,
        /// virtual (output) control values will be updated with values from their corresponding physical (input) controls.
        /// </summary>
        public bool SendOutput
        {
            get { return _sendOutput; }
            set
            {
                if (value)
                {
                    SetupVirtualJoysticks();
                }
                else
                {
                    TeardownVirtualJoysticks();
                }
                _sendOutput = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the Output Map associated that this Mediator should use.
        /// </summary>
        public OutputMap OutputMap
        {
            get { return _map; }
            set
            {
                _map = value;
                _isInitialized = false;
            }
        }

        #region Destructors

        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed and unmanaged objects
        /// and suppresses finalization by the garbage collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Standard finalizer.  Normally only called by the garbage collector.
        /// </summary>
        ~Mediator()
        {
            Dispose();
        }

        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate whether disposal is occurring</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_running)
                    {
                        StopMediating();
                    }
                    TeardownVirtualJoysticks();
                    TeardownPhysicalInputDevices();
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        private void TeardownVirtualJoysticks()
        {
            foreach (VirtualJoystick vstick in _virtualJoysticks.Values)
            {
                try
                {
                    vstick.Dispose();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            _virtualJoysticks.Clear();
        }

        private void TeardownPhysicalInputDevices()
        {
            foreach (DIDeviceMonitor pstick in _diDeviceMonitors.Values)
            {
                try
                {
                    pstick.StateChanged -= _diMonitorStateChangedHandler;
                    pstick.Dispose();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            _diDeviceMonitors.Clear();
            foreach (BIDeviceMonitor biDevice in _biDeviceMonitors.Values)
            {
                try
                {
                    biDevice.Dispose();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            _biDeviceMonitors.Clear();
            foreach (PHCCDeviceMonitor phcc in _phccDeviceMonitors.Values)
            {
                try
                {
                    phcc.StateChanged -= _phccMonitorStateChangedHandler;
                    phcc.Dispose();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            _phccDeviceMonitors.Clear();
        }

        #endregion

        /// <summary>
        /// Raised whenever one of the physical controls declared in the output map have changed
        /// </summary>
        public event PhysicalControlStateChangedEventHandler PhysicalControlStateChanged;
    }
}