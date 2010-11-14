using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class DirectInputBinding:IBinding
    {
        private string _callback=null;
        private int _buttonIndex = 0;
        private int _cockpitItemId = -1;
        private DirectInputBindingType _bindingType = DirectInputBindingType.None;
        private PovDirections _povDirection = PovDirections.Up;
        private KeyWithModifiers _comboKey = null;
        private Guid _deviceGuid = Guid.Empty;
        public DirectInputBinding()
        {
        }
        public DirectInputBinding(string callback, int buttonIndex, int cockpitItemId, DirectInputBindingType bindingType, PovDirections povDirection, KeyWithModifiers comboKey)
            : this(callback, buttonIndex, cockpitItemId, bindingType, povDirection, comboKey, Guid.Empty)
        {
        }
        public DirectInputBinding(string callback, int buttonIndex, int cockpitItemId, DirectInputBindingType bindingType, PovDirections povDirection, KeyWithModifiers comboKey, Guid deviceGuid)
        {
            this.Callback = callback;
            this.ButtonIndex = buttonIndex;
            this.CockpitItemId = cockpitItemId;
            this.BindingType = bindingType;
            this.PovDirection = povDirection;
            this.ComboKey = comboKey;
            this.DeviceGuid = deviceGuid;
        }
        public int LineNum { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Callback);
            sb.Append(" ");
            if (this.BindingType == DirectInputBindingType.Button) {
                sb.Append(this.ButtonIndex);
            }
            else {
                sb.Append("0");
            }
            sb.Append(" ");
            sb.Append(this.CockpitItemId);
            sb.Append(" ");
            sb.Append((int)this.BindingType);
            sb.Append(" ");
            if (this.BindingType == DirectInputBindingType.Button)
            {
                sb.Append("0");
            }
            else
            {
                sb.Append((int)this.PovDirection);
            }
            sb.Append(" ");
            if (this.ComboKey != null && this.ComboKey.ScanCode !=0)
            {
                sb.Append("0X");
                sb.Append(this.ComboKey.ScanCode.ToString("X").TrimStart(new char[] { '0' }));
                sb.Append(" ");
                sb.Append((int)this.ComboKey.Modifiers);
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            if (this.DeviceGuid != Guid.Empty)
            {
                sb.Append(this.DeviceGuid);
            }
            return sb.ToString();
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
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
        public DirectInputBinding Parse(string input)
        {
            List<string> tokenList = Util.Tokenize(input);
            string callback = tokenList[0];
            int buttonIndex = Int32.Parse(tokenList[1]);
            int itemId = Int32.Parse(tokenList[2]);
            DirectInputBindingType bindingType = (DirectInputBindingType)Int32.Parse(tokenList[3]);
            PovDirections povDirection = (PovDirections)Int32.Parse(tokenList[4]);
            int keycode;
            if (tokenList[5].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase)) {
                keycode = Int32.Parse(tokenList[5].ToLowerInvariant().Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber);
            }
            else {
                keycode = Int32.Parse(tokenList[5]);
            }
            KeyModifiers modifiers = (KeyModifiers)Int32.Parse(tokenList[6]);
            KeyWithModifiers comboKey = new KeyWithModifiers(keycode, modifiers);
            DirectInputBinding binding = null;
            if (tokenList.Count == 8)
            {
                bool isGuid = false;
                Guid guid = Guid.Empty;
                try
                {
                    guid = new Guid(tokenList[7]);
                    isGuid = true;
                }
                catch (Exception e)
                {
                }
                if (isGuid)
                {
                    binding = new DirectInputBinding(callback, buttonIndex, itemId, bindingType, povDirection, comboKey, guid);
                }
                else
                {
                    binding = new DirectInputBinding(callback, buttonIndex, itemId, bindingType, povDirection, comboKey);
                }
            }
            else
            {
                binding = new DirectInputBinding(callback, buttonIndex, itemId, bindingType, povDirection, comboKey);
            }
            return binding;
        }
        public string Callback
        {
            get
            {
                return _callback;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _callback = value;
            }
        }
        public int ButtonIndex
        {
            get
            {
                return _buttonIndex;
            }
            set
            {
                if (value < 0 || value > 1024)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _buttonIndex = value;
            }
        }
        public int CockpitItemId
        {
            get
            {
                return _cockpitItemId;
            }
            set
            {
                if (value > 0 && value < 1000)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _cockpitItemId = value;
            }
        }
        public DirectInputBindingType BindingType
        {
            get
            {
                return _bindingType;
            }
            set
            {
                _bindingType = value;
            }
        }
        public PovDirections PovDirection
        {
            get
            {
                return _povDirection;
            }
            set
            {
                _povDirection = value;
            }
        }
        public KeyWithModifiers ComboKey
        {
            get
            {
                return _comboKey;
            }
            set
            {
                _comboKey = value;
            }
        }
        public Guid DeviceGuid
        {
            get
            {
                return _deviceGuid;
            }
            set
            {
                _deviceGuid = value;
            }
        }
    }
}
