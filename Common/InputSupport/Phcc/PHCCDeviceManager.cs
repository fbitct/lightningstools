using System;
using System.Collections.Generic;
using log4net;
using Microsoft.VisualBasic.Devices;
using PPJoy;

namespace Common.InputSupport.Phcc
{
    public class PHCCDeviceManager : IDisposable
    {
        private static PHCCDeviceManager _instance;
        private static readonly ILog _log = LogManager.GetLogger(typeof (PHCCDeviceManager));
        private bool _isDisposed;

        private PHCCDeviceManager()
        {
        }

        public static PHCCDeviceManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new PHCCDeviceManager();
            }
            return _instance;
        }

        public bool IsDeviceAttached(PHCCPhysicalDeviceInfo device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            return
                PHCCDeviceMonitor.GetInstance(device, VirtualJoystick.MinAnalogDataSourceVal,
                                              VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(true);
        }

        public bool IsDeviceAttached(PHCCPhysicalDeviceInfo device, bool throwOnFail)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            return
                PHCCDeviceMonitor.GetInstance(device, VirtualJoystick.MinAnalogDataSourceVal,
                                              VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(throwOnFail);
        }

        public PHCCPhysicalControlInfo[] GetControlsOnDevice(PHCCPhysicalDeviceInfo device)
        {
            return GetControlsOnDevice(device, true);
        }

        public PHCCPhysicalControlInfo[] GetControlsOnDevice(PHCCPhysicalDeviceInfo device, bool throwOnFail)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            var controls = new List<PHCCPhysicalControlInfo>();
            for (int i = 0; i < 1024; i++)
            {
                var thisControl = new PHCCPhysicalControlInfo(device, i, ControlType.Button, "Button " + (i + 1));
                controls.Add(thisControl);
            }
            for (int i = 0; i < 35; i++)
            {
                var thisControl = new PHCCPhysicalControlInfo(device, i, ControlType.Axis, "Axis " + (i + 1));
                controls.Add(thisControl);
            }

            var toReturn = new PHCCPhysicalControlInfo[controls.Count];
            toReturn = controls.ToArray();
            return toReturn;
        }

        public PHCCPhysicalDeviceInfo[] GetDevices()
        {
            return GetDevices(true);
        }

        public PHCCPhysicalDeviceInfo[] GetDevices(bool throwOnFail)
        {
            var devices = new List<PHCCPhysicalDeviceInfo>();
            var ports = new Ports();

            foreach (string portName in ports.SerialPortNames)
            {
                var deviceInfo = new PHCCPhysicalDeviceInfo(portName, "PHCC device on " + portName);
                try
                {
                    if (
                        PHCCDeviceMonitor.GetInstance(deviceInfo, VirtualJoystick.MinAnalogDataSourceVal,
                                                      VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(false))
                    {
                        devices.Add(deviceInfo);
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug(ex.Message, ex);
                }
            }
            var toReturn = new PHCCPhysicalDeviceInfo[devices.Count];
            toReturn = devices.ToArray();
            return toReturn;
        }

        #region Destructors

        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~PHCCDeviceManager()
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
                    _instance = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}