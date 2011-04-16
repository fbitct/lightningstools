using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace Common.InputSupport.BetaInnovations
{
    public sealed class BIDeviceMonitor : DeviceMonitor
    {
        /// <summary>
        /// Class variable to hold all references to all instantiated device monitors of this type
        /// </summary>
        private static readonly ILog _log = LogManager.GetLogger(typeof (BIDeviceMonitor));

        private static readonly Dictionary<BIPhysicalDeviceInfo, BIDeviceMonitor> _monitors =
            new Dictionary<BIPhysicalDeviceInfo, BIDeviceMonitor>();

        private readonly BIPhysicalDeviceInfo _device;
        private BIDeviceManager _manager;
        private bool[] _prevState;
        private bool[] _state;

        #region Constructors

        /// <summary>
        /// Hidden constructor -- forces callers to use one of the static factory methods 
        /// on this class.
        /// Creates a new BIDeviceMonitor object
        /// <param name="device">a BIPhysicalDeviceInfo object representing the device to monitor.</param>
        /// </summary>
        private BIDeviceMonitor(BIPhysicalDeviceInfo device)
        {
            _device = device;
            Prepare();
        }

        #endregion

        #region Public Methods

        public BIPhysicalDeviceInfo Device
        {
            get { return _device; }
        }

        /// <summary>
        /// Returns an array representing the previous input state of the device being monitored by this object
        /// </summary>
        public bool[] PreviousState
        {
            get { return _prevState; }
        }

        /// <summary>
        /// Returns an array representing the most-recently-polled input state of the device being monitored by this object
        /// </summary>
        public bool[] CurrentState
        {
            get { return _state; }
        }

        public bool[] Poll()
        {
            return Poll(true);
        }

        public bool[] Poll(bool throwOnFail)
        {
            try
            {
                if (!_prepared)
                {
                    Prepare();
                }
                //if (!_manager.IsDeviceAttached(_device, throwOnFail))
                //{
                //    _prepared = false;
                //}
                //else
                //{
                bool[] newState;
                newState = _manager.Poll(_device, throwOnFail);
                if (newState != null)
                {
                    _prevState = _state;
                    _state = newState;
                }
                return newState;
                //}
            }
            catch (BIException e)
            {
                _log.Error(e.Message, e);
                _prepared = false;
                if (throwOnFail)
                {
                    throw;
                }
            }
            return null;
        }

        /// <summary>
        /// Factory method to create instances of this class.  Stands in place of a constructor,
        /// in order to re-use instances 
        /// when relevant constructor parameters are the same
        /// </summary>
        /// <param name="device">a BIPhysicalDeviceInfo object representing the Beta Innovations device to monitor</param>
        /// <returns>a BIDeviceMonitor object representing the BetaInnovations device 
        /// being monitored, either created newly from-scratch, or returned from 
        /// this class's internal object pool if a monitor instance already exists</returns>
        public static BIDeviceMonitor GetInstance(BIPhysicalDeviceInfo device)
        {
            BIDeviceMonitor monitor = null;
            if (_monitors.ContainsKey(device))
            {
                return _monitors[device];
            }

            monitor = new BIDeviceMonitor(device);
            _monitors.Add(device, monitor);
            return monitor;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes this object's state and sets up BetaInnovations SDK objects
        /// to monitor the BetaInnoviations device instance that this object 
        /// is responsible for.
        /// During preparation, the _preparing flag is raised.  Subsequent concurrent calls to
        /// Prepare() will simply wait until the _preparing flag is lowered.
        /// </summary>
        protected override void Prepare()
        {
            int elapsed = 0;
            int timeout = 1000;
            while (_preparing && elapsed <= timeout)
            {
                Thread.Sleep(20);
                System.Windows.Forms.Application.DoEvents();
                elapsed += 20;
            }
            if (!_preparing)
            {
                try
                {
                    _manager = BIDeviceManager.GetInstance();
                    try
                    {
                        _preparing = true;
                        //check if device is attached
                        bool connected = _manager.IsDeviceAttached(_device.Key.ToString(), false);
                        if (!connected)
                        {
                            _preparing = false;
                            _prepared = false;
                            return;
                        }
                    }
                    catch (BIException e)
                    {
                        _log.Error(e.Message, e);
                        _preparing = false;
                        _prepared = false;
                        return;
                    }

                    _prepared = true;
                }
                catch (BIException ex)
                {
                    _log.Error(ex.Message, ex);
                    _prepared = false;
                    throw;
                }
                finally
                {
                    _preparing = false;
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
            return GetType().Name + ":Device=" + _device;
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
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            // safe because of the GetType check
            var js = (BIDeviceMonitor) obj;

            // use this pattern to compare value members
            if (!_device.Equals(js.Device))
                return false;

            return true;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~BIDeviceMonitor()
        {
            Dispose();
        }

        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_manager != null)
                    {
                        try
                        {
                            _manager.Dispose();
                        }
                        catch (ApplicationException e)
                        {
                            _log.Debug(e.Message, e);
                        }
                    }
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;
        }

        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}