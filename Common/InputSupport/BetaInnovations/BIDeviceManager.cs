using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using BIUSBWrapper;
using Common.Exeptions;

namespace Common.InputSupport.BetaInnovations
{
    public class BIDeviceManager : IDisposable
    {
        private static BIDeviceManager _instance;
        private readonly DeviceParam[] _deviceList = new DeviceParam[BIUSB.MAX_DEVICES];
        private DeviceStatus[] _deviceStatus = new DeviceStatus[BIUSB.MAX_DEVICES];
        private bool _devicesOpen;
        private bool _isDisposed;
        private uint _numDevices;

        private BIDeviceManager()
        {
        }

        public static BIDeviceManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BIDeviceManager();
            }
            return _instance;
        }

        public bool IsDeviceAttached(BIPhysicalDeviceInfo device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            return IsDeviceAttached(device.Key.ToString(), true);
        }

        public bool IsDeviceAttached(BIPhysicalDeviceInfo device, bool throwOnFail)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            return IsDeviceAttached(device.Key.ToString(), throwOnFail);
        }

        public bool IsDeviceAttached(string devicePath)
        {
            return IsDeviceAttached(devicePath, true);
        }

        public bool IsDeviceAttached(string devicePath, bool throwOnFail)
        {
            BIPhysicalDeviceInfo[] devices = GetDevices(throwOnFail);
            foreach (BIPhysicalDeviceInfo thisDevice in devices)
            {
                string key = thisDevice.Key != null ? thisDevice.Key.ToString() : string.Empty;
                if (key.Equals(devicePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public BIPhysicalControlInfo[] GetControlsOnDevice(BIPhysicalDeviceInfo device)
        {
            return GetControlsOnDevice(device, true);
        }

        public BIPhysicalControlInfo[] GetControlsOnDevice(BIPhysicalDeviceInfo device, bool throwOnFail)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            string devicePath = device.Key.ToString();
            if (!_devicesOpen)
            {
                GetDevices(throwOnFail);
            }
            var controls = new List<BIPhysicalControlInfo>();
            for (int i = 0; i < _numDevices; i++)
            {
                DeviceParam thisDevice = _deviceList[i];
                string thisDevicePath = (Encoding.Default.GetString(thisDevice.DevicePath)).Substring(0,
                                                                                                      (int)
                                                                                                      thisDevice.
                                                                                                          PathLength);
                if (thisDevicePath != null &&
                    thisDevicePath.Equals(devicePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    ushort numInputs = thisDevice.NumberInputIndices;
                    for (ushort j = 0; j < numInputs; j++)
                    {
                        var thisControl = new BIPhysicalControlInfo(device, j, "Button " + (j + 1));
                        controls.Add(thisControl);
                    }
                    break;
                }
            }
            var toReturn = new BIPhysicalControlInfo[controls.Count];
            toReturn = controls.ToArray();
            return toReturn;
        }

        public BIPhysicalDeviceInfo[] GetDevices()
        {
            return GetDevices(true);
        }

        public BIPhysicalDeviceInfo[] GetDevices(bool throwOnFail)
        {
            DetectHIDDevices(throwOnFail);
            var devices = new List<BIPhysicalDeviceInfo>();
            for (int i = 0; i < _numDevices; i++)
            {
                string devName = Encoding.Default.GetString(_deviceList[i].DeviceName);
                devName = devName.Substring(0, (int) _deviceList[i].DeviceNameLength);
                string serial = (Encoding.Default.GetString(_deviceList[i].SerialNum)).Substring(0,
                                                                                                 (int)
                                                                                                 _deviceList[i].
                                                                                                     SerialNumLength);
                if (!string.IsNullOrEmpty(serial))
                {
                    devName += " (Serial #:";
                    devName += serial;
                    devName += ")";
                }
                string devicePath = (Encoding.Default.GetString(_deviceList[i].DevicePath)).Substring(0,
                                                                                                      (int)
                                                                                                      _deviceList[i].
                                                                                                          PathLength);
                var deviceInfo = new BIPhysicalDeviceInfo(devicePath, devName);
                devices.Add(deviceInfo);
            }
            var toReturn = new BIPhysicalDeviceInfo[devices.Count];
            toReturn = devices.ToArray();
            return toReturn;
        }

        public bool[] Poll(BIPhysicalDeviceInfo device, bool throwOnFail)
        {
            bool deviceFound = false;
            for (int i = 0; i < _numDevices; i++)
            {
                DeviceParam thisDevice = _deviceList[i];
                string thisDevicePath = (Encoding.Default.GetString(thisDevice.DevicePath)).Substring(0,
                                                                                                      (int)
                                                                                                      thisDevice.
                                                                                                          PathLength);
                if (thisDevicePath != null && device.Key != null &&
                    thisDevicePath.Equals(device.Key.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    deviceFound = true;
                    byte[] outbuffer;
                    int result = ReadInputData(ref thisDevice, out outbuffer, throwOnFail);
                    //if (result == BIUSB.DEV_INPUT || result == BIUSB.DEV_WAIT)
                    {
                        var state = new bool[thisDevice.NumberInputIndices];
                        for (int j = 0; j < System.Math.Min(outbuffer.Length, thisDevice.NumberInputIndices); j++)
                        {
                            state[j] = (outbuffer[j] == 1);
                        }
                        return state;
                    }
                    break;
                }
            }
            if (throwOnFail && !deviceFound)
            {
                throw new OperationFailedException("Device not found.");
            }
            return null;
        }

        private int ReadInputData(ref DeviceParam device, out byte[] databuffer)
        {
            return ReadInputData(ref device, out databuffer, true);
        }

        private int ReadInputData(ref DeviceParam device, out byte[] databuffer, bool throwOnFail)
        {
            databuffer = new byte[BIUSB.MAX_INPUTS];
            int result = BIUSB.ReadInputData(ref device, databuffer, 0);
            if (throwOnFail)
            {
                int lastError = Marshal.GetLastWin32Error();
                //check to see if a Win32 error was returned as a result of 
                //the previous calls
                if (lastError != 0)
                {
                    //a Win32 API error occured, so throw it as a wrapped exception
                    Exception e = new Win32Exception(lastError);
                    throw new OperationFailedException(e.Message, e);
                }
                if (result == BIUSB.DEV_TIMEOUT)
                {
                    throw new TimeoutException("Device did not respond within 1 second.");
                }
                else if (result == BIUSB.DEV_FAILED)
                {
                    throw new OperationFailedException("Failure reading from device.");
                }
            }
            return result;
        }

        private bool CloseDevices()
        {
            return CloseDevices(true);
        }

        private bool CloseDevices(bool throwOnFail)
        {
            bool success = BIUSB.CloseDevices(_numDevices, _deviceList);
            if (throwOnFail)
            {
                int lastError = Marshal.GetLastWin32Error();
                //check to see if a Win32 error was returned as a result of 
                //the previous calls
                if (lastError != 0)
                {
                    //a Win32 API error occured, so throw it as a wrapped exception
                    Exception e = new Win32Exception(lastError);
                    throw new OperationFailedException(e.Message, e);
                }
            }
            if (!success && throwOnFail)
            {
                throw new OperationFailedException("Failure closing devices or releasing memory.");
            }
            if (success)
            {
                _devicesOpen = false;
            }
            return success;
        }

        private bool DetectHIDDevices()
        {
            return DetectHIDDevices(true);
        }

        private bool DetectHIDDevices(bool throwOnFail)
        {
            if (_devicesOpen)
            {
                CloseDevices(false);
            }
            bool success = false;
            success = BIUSB.DetectHID(out _numDevices, _deviceList, BIUSB.DT_HID);
            if (throwOnFail)
            {
                int lastError = Marshal.GetLastWin32Error();
                //check to see if a Win32 error was returned as a result of 
                //the previous calls
                if (lastError != 0)
                {
                    //a Win32 API error occured, so throw it as a wrapped exception
                    Exception e = new Win32Exception(lastError);
                    throw new OperationFailedException(e.Message, e);
                }
            }
            if (!success && throwOnFail)
            {
                throw new OperationFailedException("Failure detecting devices or no devices found.");
            }
            return success;
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
        ~BIDeviceManager()
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
            // Code to dispose the un-managed resources of the class
            if (_devicesOpen)
            {
                CloseDevices(false);
            }
            _isDisposed = true;
        }

        #endregion
    }
}