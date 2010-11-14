using System;
using System.Collections.Generic;

using System.Text;
using Common.InputSupport.DirectInput;
using System.Windows.Forms;
using F16CPD.UI.Forms;
using Common.InputSupport;

namespace F16CPD.Mfd.Controls
{
    [Serializable]
    public class ControlBinding
    {
        public CpdInputControls CpdInputControl
        {
            get;
            set;
        }
        public string ControlName
        {
            get;
            set;
        }
        public DIPhysicalDeviceInfo DirectInputDevice
        {
            get;
            set;
        }
        public DIPhysicalControlInfo DirectInputControl
        {
            get;
            set;
        }
        public Keys Keys
        {
            get;
            set;
        }
        public BindingType BindingType
        {
            get;
            set;
        }
        public PovDirections PovDirection
        {
            get;
            set;
        }

        #region Object Overrides (ToString, GetHashCode, Equals)
        /// <summary>
        /// Gets a textual representation of this object.
        /// </summary>
        /// <returns>a String containing a textual representation of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().FullName != null ? this.GetType().FullName : "null");
            sb.Append(":CpdInputControl:");
            sb.Append(Enum.GetName(typeof(CpdInputControls), this.CpdInputControl));
            sb.Append(":ControlName:");
            sb.Append(this.ControlName != null ? this.ControlName : "null");
            sb.Append(":DirectInputDevice:");
            sb.Append(this.DirectInputDevice != null ? this.DirectInputDevice.ToString() : "null");
            sb.Append(":DirectInputControl:");
            sb.Append(this.DirectInputControl != null ? this.DirectInputControl.ToString() : "null");
            sb.Append(":Keys:");
            sb.Append(Enum.GetName(typeof(Keys), this.Keys));
            sb.Append(":BindingType:");
            sb.Append(Enum.GetName(typeof(BindingType), this.BindingType));
            sb.Append(":PovDirection:");
            sb.Append(Enum.GetName(typeof(PovDirections), this.PovDirection));
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

            if (this.GetType() != obj.GetType()) return false;

            // safe because of the GetType check
            ControlBinding pc = (ControlBinding)obj;

            // use this pattern to compare value members
            if (this.ToString() == pc.ToString())
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
