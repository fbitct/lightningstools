using System;
using Common.InputSupport.DirectInput;
using log4net;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using PPJoy;
using Device = PPJoy.Device;

namespace JoyMapper
{
    /// <summary>
    /// Utility class for holding static utility methods that don't belong
    /// to any given class
    /// </summary>
    internal sealed class Util
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Util));


        public static byte[] ConvertSbyteArrayToByteArray(sbyte[] toConvert)
        {
            var toReturn = new byte[toConvert.Length];
            for (int i = 0; i < toConvert.Length; i++)
            {
                toReturn[i] = Convert.ToByte(toConvert[i]);
            }
            return toReturn;
        }

        /// <summary>
        /// Counts the number of defined PPJoy virtual joystick devices on the system
        /// </summary>
        /// <returns>an int specifying the number of defined PPJoy virtual joystick devices on the system</returns>
        public static int CountPPJoyVirtualDevices()
        {
            var devMgr = new DeviceManager();
            Device[] devs = devMgr.GetAllDevices();
            int numSticksDefined = 0;
            foreach (Device dev in devs)
            {
                if (dev.DeviceType == JoystickTypes.Virtual_Joystick)
                {
                    numSticksDefined++;
                }
            }
            return numSticksDefined;
        }

        /// <summary>
        /// Gets the maximum number of PPJoy virtual devices that can exist on the system, taking into account the 
        /// number of non-virtual joysticks already registered.  This accounts for the Windows limitation of 16 
        /// joystick devices on the system at any given time.
        /// </summary>
        /// <returns>an int specifying the maximum number of virtual devices that can exist on the system.</returns>
        public static int GetMaxPPJoyVirtualDevicesAllowed()
        {
            int maxDevices = VirtualJoystick.MaxVirtualDevices;
                //start with the assumption we can actually create the maximum
            //number of virtual joysticks that PPJoy itself can support,
            //regardless of Windows limitations

            //get a list of devices currently registered with DirectInput
            DeviceList detectedJoysticks = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AllDevices);

            //for each detected device, determine if the device is a Physical (non-Virtual) device.
            //NOTE: even PPJoy non-virtual devices are considered physical devices.
            foreach (DeviceInstance instance in detectedJoysticks)
            {
                DIDeviceMonitor dev = null;
                try
                {
                    var deviceInfo = new DIPhysicalDeviceInfo(instance.InstanceGuid, instance.InstanceName);
                    dev = DIDeviceMonitor.GetInstance(deviceInfo, null, VirtualJoystick.MinAnalogDataSourceVal,
                                                      VirtualJoystick.MaxAnalogDataSourceVal);
                    int? productId = dev.VendorIdentityProductId;
                    if (productId.HasValue)
                    {
                        bool isVirtual = false;
                        try
                        {
                            isVirtual = new DeviceManager().IsVirtualDevice(productId.Value);
                        }
                        catch (DeviceNotFoundException e)
                        {
                            _log.Debug(e.Message, e);
                        }
                        if (!isVirtual)
                        {
                            maxDevices--; //a non-virtual device occupies a potential slot 
                        }
                        else
                        {
                            dev.Dispose();
                        }
                    }
                }
                catch (DirectXException e)
                {
                    _log.Debug(e.Message, e);
                }
                catch (AccessViolationException e)
                {
                    _log.Debug(e.Message, e);
                }
                catch (OutOfMemoryException e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            return maxDevices;
        }
    }
}