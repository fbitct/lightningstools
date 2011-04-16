﻿using System;
using System.Text;
using System.Windows.Forms;
using Common.InputSupport;
using Common.InputSupport.DirectInput;
using F16CPD.UI.Forms;

namespace F16CPD.Mfd.Controls
{
    [Serializable]
    public class ControlBinding
    {
        public CpdInputControls CpdInputControl { get; set; }
        public string ControlName { get; set; }
        public DIPhysicalDeviceInfo DirectInputDevice { get; set; }
        public DIPhysicalControlInfo DirectInputControl { get; set; }
        public Keys Keys { get; set; }
        public BindingType BindingType { get; set; }
        public PovDirections PovDirection { get; set; }

        #region Object Overrides (ToString, GetHashCode, Equals)

        /// <summary>
        /// Gets a textual representation of this object.
        /// </summary>
        /// <returns>a String containing a textual representation of this object.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(GetType().FullName != null ? GetType().FullName : "null");
            sb.Append(":CpdInputControl:");
            sb.Append(Enum.GetName(typeof (CpdInputControls), CpdInputControl));
            sb.Append(":ControlName:");
            sb.Append(ControlName != null ? ControlName : "null");
            sb.Append(":DirectInputDevice:");
            sb.Append(DirectInputDevice != null ? DirectInputDevice.ToString() : "null");
            sb.Append(":DirectInputControl:");
            sb.Append(DirectInputControl != null ? DirectInputControl.ToString() : "null");
            sb.Append(":Keys:");
            sb.Append(Enum.GetName(typeof (Keys), Keys));
            sb.Append(":BindingType:");
            sb.Append(Enum.GetName(typeof (BindingType), BindingType));
            sb.Append(":PovDirection:");
            sb.Append(Enum.GetName(typeof (PovDirections), PovDirection));
            return sb.ToString();
        }

        /// <summary>
        /// Gets an integer (hash) representation of this object, 
        /// for use in hashtables.  If two objects are equal, 
        /// then their hashcodes should be equal as well.
        /// </summary>
        /// <returns>an integer containing a hashed representation of this object</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Compares two objects to determine if they are equal to each other.
        /// </summary>
        /// <param name="obj">An object to compare this instance to</param>
        /// <returns>a boolean, set to true if the specified object is 
        /// equal to this instance, or false if the specified object
        /// is not equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (GetType() != obj.GetType()) return false;

            // safe because of the GetType check
            var pc = (ControlBinding) obj;

            // use this pattern to compare value members
            if (ToString() == pc.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}