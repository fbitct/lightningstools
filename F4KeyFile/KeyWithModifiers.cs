using System;
using System.Runtime.InteropServices;
using System.Text;

namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyWithModifiers
    {
        private KeyModifiers _modifiers;
        private int _scanCode;

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
            get { return _scanCode; }
            set { _scanCode = value; }
        }

        public KeyModifiers Modifiers
        {
            get { return _modifiers; }
            set { _modifiers = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (ScanCode != 0)
            {
                sb.Append("0X");
                sb.Append(ScanCode.ToString("X").TrimStart(new[] {'0'}));
                sb.Append(" ");
                sb.Append((int) Modifiers);
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
            if (!obj.GetType().Equals(GetType()))
            {
                return false;
            }
            if (obj.ToString() != ToString())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}