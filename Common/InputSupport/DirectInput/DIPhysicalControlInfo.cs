using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    [Serializable]
    public class DIPhysicalControlInfo : PhysicalControlInfo
    {
        /// <summary>
        /// The DirectInput Object ID of this physical control
        /// </summary>
        private int _doiObjectId;
        /// <summary>
        /// The DirectInput Object Type GUID of this physical control -- indicates its type
        /// from a DirectInput standpoint
        /// </summary>
        private Guid _doiObjectTypeGuid;
        public DIPhysicalControlInfo()
            : base()
        {

        }
        /// <summary>
        /// Creates a new DIPhysicalControlInfo object.
        /// </summary>
        /// <param name="parent">a DIPhysicalDeviceInfo object representing
        /// the physical device on which this control appears</param>
        /// <param name="deviceObjectInstance">this control's DeviceObjectInstance 
        /// structure, as obtained from DirectInput </param>
        /// <param name="controlNum">an integer indicating the relative zero-based 
        /// "offset" of this control in the collection of similar controls on the
        /// same device.  Button 1 would be controlNum=0; the first slider would 
        /// similarily be controlNum=0; as would the first Pov control, etc.</param>
        public DIPhysicalControlInfo(DIPhysicalDeviceInfo parent, DeviceObjectInstance deviceObjectInstance, int controlNum)
            : base((PhysicalDeviceInfo)parent, controlNum, ControlType.Unknown, AxisType.Unknown)
        {
            _doiObjectId = deviceObjectInstance.ObjectId;
            _doiObjectTypeGuid = deviceObjectInstance.ObjectType;
        }
        /// <summary>
        /// Creates a new DIPhysicalControlInfo object.
        /// </summary>
        /// <param name="parent">a DIPhysicalDeviceInfo object representing
        /// the physical device on which this control appears</param>
        /// <param name="deviceObjectInstance">this control's DeviceObjectInstance 
        /// structure, as obtained from DirectInput </param>
        /// <param name="controlNum">an integer indicating the relative zero-based 
        /// "offset" of this control in the collection of similar controls on the
        /// same device.  Button 1 would be controlNum=0; the first slider would 
        /// similarily be controlNum=0; as would the first Pov control, etc.</param>
        /// <param name="alias">A string containing a "friendly name" (alias) to 
        /// associate with this control.</param>
        public DIPhysicalControlInfo(DIPhysicalDeviceInfo parent, DeviceObjectInstance deviceObjectInstance, int controlNum, string alias)
            : base((PhysicalDeviceInfo)parent, controlNum, ControlType.Unknown, AxisType.Unknown, alias)
        {
            _doiObjectId = deviceObjectInstance.ObjectId;
            _doiObjectTypeGuid = deviceObjectInstance.ObjectType;
        }

        /// <summary>
        /// Gets the type of axis represented by this control (if the control is
        /// an axis -- otherwise, returns AxisType.Unknown
        /// </summary>
        public override AxisType AxisType
        {
            get
            {
                if (_axisType == AxisType.Unknown)
                {
                    if (this.ControlType == ControlType.Axis)
                    {
                        if (_doiObjectTypeGuid == ObjectTypeGuid.XAxis)
                        {
                            _axisType = AxisType.X;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.YAxis)
                        {
                            _axisType = AxisType.Y;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.ZAxis)
                        {
                            _axisType = AxisType.Z;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.RxAxis)
                        {
                            _axisType = AxisType.XR;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.RyAxis)
                        {
                            _axisType = AxisType.YR;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.RzAxis)
                        {
                            _axisType = AxisType.ZR;
                        }
                        else if (_doiObjectTypeGuid == ObjectTypeGuid.Slider)
                        {
                            _axisType = AxisType.Slider;
                        }
                        else
                        {
                            _axisType = AxisType.Unknown;
                        }
                    }
                    else if (this.ControlType == ControlType.Pov)
                    {
                        _axisType = AxisType.Pov;
                    }
                }
                return _axisType;
            }
            set
            {
                _axisType = value;
            }
        }
        /// <summary>
        /// Gets the type of this control (button, axis, Pov)
        /// </summary>
        public override ControlType ControlType
        {
            get
            {
                if (_controlType == ControlType.Unknown)
                {
                    if (_doiObjectTypeGuid == ObjectTypeGuid.Button)
                    {
                        _controlType = ControlType.Button;
                    }
                    else if (_doiObjectTypeGuid == ObjectTypeGuid.PointOfView)
                    {
                        _controlType = ControlType.Pov;
                    }
                    else if ((_doiObjectId & (int)DeviceObjectTypeFlags.Axis) != 0)
                    {
                        _controlType = ControlType.Axis;
                    }
                    else
                    {
                        _controlType = ControlType.Unknown;
                    }
                    return _controlType;
                }
                else
                {
                    return _controlType;
                }
            }
            set
            {
                _controlType = value;
            }
        }

    }
}
