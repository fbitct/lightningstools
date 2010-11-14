using System;
using System.Text;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyWithModifiers
    {
        private int _scanCode;
        private KeyModifiers _modifiers;
        public KeyWithModifiers()
        {
        }
        public KeyWithModifiers(int scanCode, KeyModifiers modifers)
        {
            _scanCode = scanCode;
            _modifiers = modifers;
        }
        public int ScanCode
        {
            get
            {
                return _scanCode;
            }
            set
            {
                _scanCode = value;
            }
        }
        public KeyModifiers Modifiers
        {
            get
            {
                return _modifiers;
            }
            set
            {
                _modifiers = value;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.ScanCode != 0)
            {
                sb.Append("0X");
                sb.Append(this.ScanCode.ToString("X").TrimStart(new char[] { '0' }));
                sb.Append(" ");
                sb.Append((int)this.Modifiers);
            }
            else
            {
                sb.Append("0X0 0");
            }
            return sb.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            if (obj.ToString() != this.ToString())
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

    }
}
