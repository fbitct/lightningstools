using System;
using PPJoy;

namespace JoyMapper
{
    /// <summary>
    /// Represents a virtual device (PPJoy virtual joystick)
    /// </summary>
    [Serializable]
    public sealed class VirtualDeviceInfo
    {
        #region Instance variables

        private readonly int _virtualDeviceNum;

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor -- is effectively hidden
        /// </summary>
        private VirtualDeviceInfo()
        {
        }

        /// <summary>
        /// Creates a VirtualDeviceInfo object.
        /// </summary>
        /// <param name="virtualDeviceNum">a one-based integer specifying which PPJoy virtual joystick device this new object will represent</param>
        public VirtualDeviceInfo(int virtualDeviceNum)
        {
            if (virtualDeviceNum <= 0 || virtualDeviceNum > VirtualJoystick.MaxVirtualDevices)
            {
                throw new ArgumentOutOfRangeException("virtualDeviceNum");
            }
            _virtualDeviceNum = virtualDeviceNum;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Returns a one-based integer indicating the PPJoy virtual joystick device number tbat this object represents.
        /// </summary>
        public int VirtualDeviceNum
        {
            get { return _virtualDeviceNum; }
        }

        #endregion

        #region Object overrides

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (GetType() != obj.GetType()) return false;

            // safe because of the GetType check
            var vd = (VirtualDeviceInfo) obj;

            // use this pattern to compare value members
            if (!_virtualDeviceNum.Equals(vd.VirtualDeviceNum)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return ("VirtualDeviceNum:" + _virtualDeviceNum);
        }

        #endregion
    }
}