using System;
using System.Collections.Generic;
using log4net;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    /// <summary>
    /// Represents a specific physical DirectInput input device (gaming device)
    /// such as a joystick, gaming wheel, etc.
    /// </summary>
    [Serializable]
    public sealed class DIPhysicalDeviceInfo : PhysicalDeviceInfo
    {
        #region Instance variable declarations

        private static readonly ILog _log = LogManager.GetLogger(typeof (DIPhysicalDeviceInfo));
        private Guid _guid = Guid.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        private DIPhysicalDeviceInfo()
        {
        }

        /// <summary>
        /// Constructs a DIPhysicalDeviceInfo, given a DirectInput 
        /// Device Instance GUID and an (optional) alias ("Friendly name") 
        /// to use for the device
        /// </summary>
        /// <param name="guid">a GUID containing the DirectInput
        /// Device Instance GUID of the physical input device to be
        /// represented by the newly-created object</param>
        /// <param name="alias">a string containing a "friendly name" (alias)
        /// to associate with the device being represented</param>
        public DIPhysicalDeviceInfo(Guid guid, string alias) : base(guid, alias)
        {
            _guid = guid;
        }

        public int DeviceNum { get; set; }

        #endregion

        #region Private methods

        /// <summary>
        /// Discovers the physical controls that appear on this device,
        /// as reported by DirectInput, and stores them as an array 
        /// of PhysicalControlInfo objects at the instance level.
        /// NOT guaranteed to be successful -- if the calls to 
        /// DirectInput fail or if the device
        /// is not currently registered, then the controls list will remain
        /// unpopulated.
        /// </summary>
        internal override void LoadControls()
        {
            if (ControlsLoaded)
                return;
            try
            {
                if (!Manager.GetDeviceAttached(new Guid(Key.ToString())))
                {
                    return;
                }
            }
            catch (DirectXException e)
            {
                _log.Debug(e.Message, e);
                return;
            }
            catch (AccessViolationException e2)
            {
                _log.Debug(e2.Message, e2);
                return;
            }
            var controls = new List<PhysicalControlInfo>();
            Device joystick = Util.GetDIDevice(new Guid(Key.ToString()));
            if (joystick == null)
            {
                return;
            }

            DeviceObjectList dol = joystick.GetObjects(DeviceObjectTypeFlags.Axis);
            int lastSlider = -1;
            int lastAxis = -1;
            foreach (DeviceObjectInstance doi in dol)
            {
                if (doi.ObjectType == ObjectTypeGuid.Slider)
                {
                    lastSlider++;
                    PhysicalControlInfo control = new DIPhysicalControlInfo(this, doi, lastSlider,
                                                                            "Slider " + (lastSlider + 1));
                    controls.Add(control);
                }
                else if ((doi.ObjectId & (int) DeviceObjectTypeFlags.Axis) != 0)
                {
                    if (!(doi.ObjectType == ObjectTypeGuid.XAxis ||
                          doi.ObjectType == ObjectTypeGuid.YAxis ||
                          doi.ObjectType == ObjectTypeGuid.ZAxis ||
                          doi.ObjectType == ObjectTypeGuid.RxAxis ||
                          doi.ObjectType == ObjectTypeGuid.RyAxis ||
                          doi.ObjectType == ObjectTypeGuid.RzAxis))
                    {
                        continue;
                    }
                    lastAxis++;
                    string axisName = "Unknown";
                    if (doi.ObjectType == ObjectTypeGuid.XAxis)
                    {
                        axisName = "X Axis";
                    }
                    else if (doi.ObjectType == ObjectTypeGuid.YAxis)
                    {
                        axisName = "Y Axis";
                    }
                    else if (doi.ObjectType == ObjectTypeGuid.ZAxis)
                    {
                        axisName = "Y Axis";
                    }
                    else if (doi.ObjectType == ObjectTypeGuid.RxAxis)
                    {
                        axisName = "X Rotation Axis";
                    }
                    else if (doi.ObjectType == ObjectTypeGuid.RyAxis)
                    {
                        axisName = "Y Rotation Axis";
                    }
                    else if (doi.ObjectType == ObjectTypeGuid.RzAxis)
                    {
                        axisName = "Z Rotation Axis";
                    }

                    PhysicalControlInfo control = new DIPhysicalControlInfo(this, doi, lastAxis, axisName);
                    controls.Add(control);
                }
            }
            int lastButton = -1;
            dol = joystick.GetObjects(DeviceObjectTypeFlags.Button);
            int numButtons = joystick.Caps.NumberButtons;
            foreach (DeviceObjectInstance doi in dol)
            {
                lastButton++;
                PhysicalControlInfo control = new DIPhysicalControlInfo(this, doi, lastButton,
                                                                        "Button " + (lastButton + 1));
                controls.Add(control);
            }

            int lastPov = -1;
            dol = joystick.GetObjects(DeviceObjectTypeFlags.Pov);
            foreach (DeviceObjectInstance doi in dol)
            {
                lastPov++;
                PhysicalControlInfo control = new DIPhysicalControlInfo(this, doi, lastPov, "Hat " + (lastPov + 1));
                controls.Add(control);
            }
            _controls = new PhysicalControlInfo[controls.Count];
            int thisControlIndex = 0;
            foreach (PhysicalControlInfo control in controls)
            {
                _controls[thisControlIndex] = control;
                thisControlIndex++;
            }
            ControlsLoaded = true;
        }

        #endregion

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
    }
}