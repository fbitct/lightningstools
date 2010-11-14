using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using log4net;
namespace Common.InputSupport.Phcc
{
    public class PHCCDeviceManager : IDisposable
    {
        private bool _isDisposed = false;
        private static PHCCDeviceManager _instance = null;
        private static ILog _log = LogManager.GetLogger(typeof(PHCCDeviceManager));
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
            return PHCCDeviceMonitor.GetInstance(device, PPJoy.VirtualJoystick.MinAnalogDataSourceVal, PPJoy.VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(true);
        }
        public bool IsDeviceAttached(PHCCPhysicalDeviceInfo device, bool throwOnFail)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            return PHCCDeviceMonitor.GetInstance(device, PPJoy.VirtualJoystick.MinAnalogDataSourceVal, PPJoy.VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(throwOnFail);
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
            List<PHCCPhysicalControlInfo> controls = new List<PHCCPhysicalControlInfo>();
            for (int i = 0; i < 1024; i++)
            {
                PHCCPhysicalControlInfo thisControl = new PHCCPhysicalControlInfo(device, i, ControlType.Button, "Button " + (i + 1));
                controls.Add(thisControl);
            }
            for (int i = 0; i < 35; i++)
            {
                PHCCPhysicalControlInfo thisControl = new PHCCPhysicalControlInfo(device, i, ControlType.Axis, "Axis " + (i + 1));
                controls.Add(thisControl);
            }

            PHCCPhysicalControlInfo[] toReturn = new PHCCPhysicalControlInfo[controls.Count];
            toReturn = (PHCCPhysicalControlInfo[])controls.ToArray();
            return toReturn;
        }
        public PHCCPhysicalDeviceInfo[] GetDevices()
        {
            return GetDevices(true);
        }
        public PHCCPhysicalDeviceInfo[] GetDevices(bool throwOnFail)
        {
            List<PHCCPhysicalDeviceInfo> devices = new List<PHCCPhysicalDeviceInfo>();
            Ports ports = new Ports();
            
            foreach (string portName in ports.SerialPortNames)
            {
                PHCCPhysicalDeviceInfo deviceInfo = new PHCCPhysicalDeviceInfo(portName, "PHCC device on " + portName);
                try
                {
                    if (PHCCDeviceMonitor.GetInstance(deviceInfo, PPJoy.VirtualJoystick.MinAnalogDataSourceVal, PPJoy.VirtualJoystick.MaxAnalogDataSourceVal).IsDeviceAttached(false))
                    {
                        devices.Add(deviceInfo);
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug(ex.Message, ex);
                }
            }
            PHCCPhysicalDeviceInfo[] toReturn = new PHCCPhysicalDeviceInfo[devices.Count];
            toReturn = devices.ToArray();
            return toReturn;
        }
        #region Destructors
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
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
