using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    public sealed class DIDeviceMonitor : DeviceMonitor, IDisposable
    {
        #region Delegates

        public delegate void DIStateChangedEventHandler(object sender, DIStateChangedEventArgs e);

        #endregion

        public event DIStateChangedEventHandler StateChanged;

        #region Instance Variable Declarations

        /// <summary>
        /// Class variable to hold all references to all instantiated device monitors of this type
        /// </summary>
        private static readonly Dictionary<Guid, DIDeviceMonitor> _monitors = new Dictionary<Guid, DIDeviceMonitor>();

        private static readonly ILog _log = LogManager.GetLogger(typeof (DIDeviceMonitor));

        /// <summary>
        /// Value passed in this object's constructor, indicating the integer value 
        /// that should represent an axis' maximum reported value for any axis being 
        /// monitored by this object instance.  This enables DirectInput to perform 
        /// translation between the internal values used by the device, and the values
        /// that are expected by calling code.
        /// </summary>
        private readonly int _axisRangeMax = 1024;

        /// <summary>
        /// Value passed in this object's constructor, indicating the integer value
        /// that should represent an axis' minimum reported value for any axis being
        /// monitored by this object instance.  This enables DirectInput to perform 
        /// translation between the internal values used by the device, and the values 
        /// that are expected by calling code.
        /// </summary>
        private readonly int _axisRangeMin;

        private readonly DIPhysicalDeviceInfo _deviceInfo;

        private readonly AutoResetEvent _diEvent = new AutoResetEvent(false);
        private readonly object _stateLock = new object();
        private Thread _eventMonitorThread;

        /// <summary>
        /// Reference to one of the application's Windows Forms, which will act as
        /// the notification form for events raised by DirectInput -- currently unused
        /// </summary>
        private Control _parentForm;

        /// <summary>
        /// The previous JoystickState structure from the DirectInput Device 
        /// being monitored by this object instance
        /// </summary>
        private JoystickState? _prevState;

        /// <summary>
        /// The last-polled JoystickState structure from the DirectInput Device 
        /// being monitored by this object instance
        /// </summary>
        private JoystickState? _state;

        /// <summary>
        /// The DirectInput Device object being monitored by this object instance
        /// </summary>
        private Device _underlyingDxDevice;

        #endregion

        #region Constructors

        /// <summary>
        /// Hidden default constructor -- forces callers to use one of the static 
        /// factory methods on this class
        /// </summary>
        private DIDeviceMonitor()
        {
        }

        /// <summary>
        /// Hidden constructor -- forces callers to use one of the static factory methods
        /// on this class.
        /// Creates a new DIDeviceMonitor object
        /// </summary>
        /// <param name="device">a DIPhysicalDeviceInfo object representing the 
        /// DirectInput Device Instance to monitor</param>
        /// <param name="parentForm">and an (optional) reference to a parent Windows Form 
        /// which will receive events directly from DirectInput if eventing is enabled 
        /// (currently, not implemented)</param>
        /// <param name="axisRangeMin">Value to report when an axis is reporting its 
        /// MINIMUM value</param>
        /// <param name="axisRangeMax">Value to report when an axis is reporting its 
        /// MAXIMUM value</param>
        private DIDeviceMonitor(DIPhysicalDeviceInfo device, Control parentForm, int axisRangeMin, int axisRangeMax)
        {
            _deviceInfo = device;
            _parentForm = parentForm;
            _axisRangeMin = axisRangeMin;
            _axisRangeMax = axisRangeMax;
            Prepare();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the DirectInput Device object being monitored by this object 
        /// </summary>
        public Device UnderlyingDirectXDevice
        {
            get { return _underlyingDxDevice; }
        }

        /// <summary>
        /// Returns a DirectInput JoystickState structure representing the previous joystick state of the device being monitored by this object
        /// </summary>
        public JoystickState? PreviousState
        {
            get
            {
                lock (_stateLock)
                {
                    return _prevState;
                }
            }
        }

        /// <summary>
        /// Returns a DirectInput JoystickState structure representing the most-recently-polled joystick state of the device being monitored by this object
        /// </summary>
        public JoystickState? CurrentState
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        public DIPhysicalDeviceInfo DeviceInfo
        {
            get { return _deviceInfo; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns an int? containing the Vendor Identity and the Product Id for the 
        /// DirectInput device being monitored by this object.  This token can 
        /// be examined to determine the type of device being monitored, and can be 
        /// used to distinguish physical devices from virtual devices, or to distinguish
        /// among various manufacturers and even specific products.  The high 16 bits
        /// contain the Vendor Identity and the low 16 bits contain the Product ID.
        /// If this token cannot be obtained, then this method will return an int? 
        /// with no value (.hasValue == false).
        /// </summary>
        public int? VendorIdentityProductId
        {
            get
            {
                if (!Prepared)
                {
                    Prepare();
                }
                if (_underlyingDxDevice != null)
                {
                    return _underlyingDxDevice.Properties.VendorIdentityProductId;
                }
                return null;
            }
        }

        /// <summary>
        /// Factory method to create instances of this class.  Stands in place of a constructor,
        /// in order to re-use instances 
        /// when relevant constructor parameters are the same
        /// </summary>
        /// <param name="device">a <see cfef="DIPhysicalDeviceInfo"/> object representing the 
        /// DirectInput Device Instance to monitor</param>
        /// <param name="parentForm">and an (optional) reference to a parent Windows Form 
        /// which will receive events directly from DirectInput if eventing is enabled
        /// (currently, not implemented)</param>
        /// <param name="axisRangeMin">Value to report when an axis is reporting its 
        /// MINIMUM value</param>
        /// <param name="axisRangeMax">Value to report when an axis is reporting its 
        /// MAXIMUM value</param>
        /// <returns>a DIDeviceMonitor object representing the DirectInput device being 
        /// monitored, either created newly from-scratch, or returned from this class's 
        /// internal object pool if a monitor instance already exists</returns>
        public static DIDeviceMonitor GetInstance(DIPhysicalDeviceInfo device, Control parentForm, int axisRangeMin,
                                                  int axisRangeMax)
        {
            var deviceId = new Guid(device.Key.ToString());
            if (_monitors.ContainsKey(deviceId))
            {
                return _monitors[deviceId];
            }
            var monitor = new DIDeviceMonitor(device, parentForm, axisRangeMin, axisRangeMax);
            _monitors.Add(deviceId, monitor);
            return monitor;
        }

        /// <summary>
        /// Polls the monitored DirectInput device.  This method also updates the 
        /// previous state variable to the results of the previous polling,
        /// so that the current and previous states can be compared to determine what has changed 
        /// in the latest poll.
        /// </summary>
        /// <returns>and returns a nullable JoystickState structure representing 
        /// the current joystick state discovered during polling</returns>
        public JoystickState? Poll()
        {
            try
            {
                if (!Prepared)
                {
                    Prepare();
                }
                if (_underlyingDxDevice != null)
                {
                    _underlyingDxDevice.Poll();
                    lock (_stateLock)
                    {
                        _prevState = _state;
                        _state = _underlyingDxDevice.CurrentJoystickState;
                    }
                }
                else
                {
                    Prepared = false;
                }
            }
            catch (DirectXException e)
            {
                _log.Debug(e.Message, e);
                Prepared = false;
                throw;
            }
            catch (NullReferenceException e2)
            {
                _log.Debug(e2.Message, e2);
                Prepared = false;
            }
            catch (AccessViolationException e3)
            {
                _log.Debug(e3.Message, e3);
            }
            return _state;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes this object's state and sets up a DirectInput Device object
        /// to monitor the device instance specified in this object's _guid variable.
        /// During preparation, the _preparing flag is raised.  Subsequent concurrent calls to
        /// Prepare() will simply wait until the _preparing flag is lowered.
        /// </summary>
        protected override void Prepare()
        {
            int elapsed = 0;
            const int timeout = 1000;
            while (Preparing && elapsed <= timeout)
            {
                Thread.Sleep(20);
                System.Windows.Forms.Application.DoEvents();
                elapsed += 20;
            }
            if (!Preparing)
            {
                try
                {
                    Common.Util.DisposeObject(_underlyingDxDevice);
                    Preparing = true;
                    try
                    {
                        if (!Manager.GetDeviceAttached(_deviceInfo.Guid))
                        {
                            Preparing = false;
                            Prepared = false;
                            return;
                        }
                    }
                    catch (DirectXException e)
                    {
                        _log.Debug(e.Message, e);
                        Preparing = false;
                        Prepared = false;
                        return;
                    }
                    catch (AccessViolationException e2)
                    {
                        _log.Debug(e2.Message, e2);
                        Preparing = false;
                        Prepared = false;
                        return;
                    }
                    _underlyingDxDevice = Util.GetDIDevice(_deviceInfo.Guid);
                    if (_underlyingDxDevice == null)
                    {
                        Preparing = false;
                        return;
                    }

                    _underlyingDxDevice.SetCooperativeLevel(null,
                                                            CooperativeLevelFlags.NonExclusive |
                                                            CooperativeLevelFlags.Background);

                    _underlyingDxDevice.SetDataFormat(DeviceDataFormat.Joystick);

                    //Set joystick axis ranges.
                    foreach (DeviceObjectInstance doi in _underlyingDxDevice.Objects)
                    {
                        if ((doi.ObjectId & (int) DeviceObjectTypeFlags.Axis) != 0)
                        {
                            _underlyingDxDevice.Properties.SetRange(
                                ParameterHow.ById,
                                doi.ObjectId,
                                new InputRange(_axisRangeMin, _axisRangeMax));
                        }
                    }
                    _underlyingDxDevice.Properties.AxisModeAbsolute = true;
                    _underlyingDxDevice.SetEventNotification(_diEvent);
                    _underlyingDxDevice.Acquire();
                    _eventMonitorThread = new Thread(DIEventMonitorThreadWork);
                    _eventMonitorThread.SetApartmentState(ApartmentState.STA);
                    _eventMonitorThread.Name = "DIMonitorThread:" + _underlyingDxDevice.DeviceInformation.InstanceGuid;
                    _eventMonitorThread.Priority = ThreadPriority.Normal;
                    _eventMonitorThread.IsBackground = true;
                    _eventMonitorThread.Start();

                    Prepared = true;
                }
                catch (DirectXException e)
                {
                    _log.Debug(e.Message, e);
                    Prepared = false;
                    throw;
                }
                catch (AccessViolationException e2)
                {
                    _log.Debug(e2.Message, e2);
                    Prepared = false;
                    throw;
                }
                finally
                {
                    Preparing = false;
                }
            }
        }

        public void DIEventMonitorThreadWork()
        {
            if (_underlyingDxDevice != null)
            {
                try
                {
                    GetNewJoyState();
                    while (!_isDisposed)
                    {
                        _diEvent.WaitOne();
                        GetNewJoyState();
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (ThreadInterruptedException)
                {
                    //_log.Debug(e2.Message, e2);
                }
            }
        }

        private void GetNewJoyState()
        {
            lock (_stateLock)
            {
                _prevState = _state;
                try
                {
                    if (_underlyingDxDevice != null)
                    {
                        _state = _underlyingDxDevice.CurrentJoystickState;
                        if (StateChanged != null)
                        {
                            StateChanged(this, new DIStateChangedEventArgs(_prevState, _state));
                        }
                    }
                }
                catch (DirectXException e)
                {
                    Prepare(); //TODO: check here whether to prepare here or just mark object as not-prepared
                    _log.Debug(e.Message, e);
                }
                catch (NullReferenceException e2)
                {
                    _log.Debug(e2.Message, e2);
                }
                catch (AccessViolationException e3)
                {
                    _log.Debug(e3.Message, e3);
                }
            }
        }

        #endregion

        #region Object Overrides (ToString, GetHashCode, Equals)

        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>a String containing a textual representation of this object.</returns>
        public override string ToString()
        {
            return GetType().Name + ":" + _deviceInfo.Guid;
        }

        /// <summary>
        /// Gets an integer "hash" representation of this object, for use in hashtables.
        /// </summary>
        /// <returns>an integer containing a numeric hash of this object's variables.  When two objects are Equal, their hashes should be equal as well.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Compares this object to another one to determine if they are equal.  Equality for this type of object simply means that the other object must be of the same type and must be monitoring the same DirectInput device.
        /// </summary>
        /// <param name="obj">An object to compare this object to</param>
        /// <returns>a boolean, set to true, if the this object is equal to the specified object, and set to false, if they are not equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (GetType() != obj.GetType()) return false;

            // safe because of the GetType check
            var js = (DIDeviceMonitor) obj;

            // use this pattern to compare value members
            if (!DeviceInfo.Guid.Equals(js.DeviceInfo.Guid)) return false;

            return true;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~DIDeviceMonitor()
        {
            Dispose();
        }

        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (_eventMonitorThread != null)
            {
                _eventMonitorThread.Interrupt();
                _eventMonitorThread.Abort();
            }
            if (!IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (_monitors != null && DeviceInfo != null && _monitors.ContainsKey(DeviceInfo.Guid))
                        {
                            _monitors.Remove(DeviceInfo.Guid);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                    if (_underlyingDxDevice != null)
                    {
                        try
                        {
                            _underlyingDxDevice.Unacquire();
                        }
                        catch (DirectXException e)
                        {
                            _log.Debug(e.Message, e);
                        }
                        catch (NullReferenceException e2)
                        {
                            _log.Debug(e2.Message, e2);
                        }
                        catch (AccessViolationException e3)
                        {
                            _log.Debug(e3.Message, e3);
                        }

                        try
                        {
                            _underlyingDxDevice.Dispose();
                        }
                        catch (DirectXException e)
                        {
                            _log.Debug(e.Message, e);
                        }
                        catch (NullReferenceException e2)
                        {
                            _log.Debug(e2.Message, e2);
                        }
                        catch (AccessViolationException e3)
                        {
                            _log.Debug(e3.Message, e3);
                        }
                    }
                }
            }
            // Code to dispose the un-managed resources of the class
            IsDisposed = true;
        }

        #endregion
    }
}