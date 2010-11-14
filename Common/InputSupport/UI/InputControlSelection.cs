using System;
using System.Collections.Generic;

using System.Text;
using Common.InputSupport.DirectInput;
using System.Windows.Forms;
using Common.InputSupport;

namespace Common.InputSupport.UI
{
    [Serializable]
    public class InputControlSelection
    {
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
        public ControlType ControlType
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
            sb.Append(":DirectInputDevice:");
            sb.Append(this.DirectInputDevice != null ? this.DirectInputDevice.ToString() : "null");
            sb.Append(":DirectInputControl:");
            sb.Append(this.DirectInputControl != null ? this.DirectInputControl.ToString() : "null");
            sb.Append(":Keys:");
            sb.Append(Enum.GetName(typeof(Keys), this.Keys));
            sb.Append(":ControlType:");
            sb.Append(Enum.GetName(typeof(ControlType), this.ControlType));
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
            InputControlSelection pc = (InputControlSelection)obj;

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
