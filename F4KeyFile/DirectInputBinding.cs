using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class DirectInputBinding : IBinding
    {
        private DirectInputBindingType _bindingType = DirectInputBindingType.None;
        private int _buttonIndex;
        private string _callback;
        private int _cockpitItemId = -1;
        private KeyWithModifiers _comboKey;
        private Guid _deviceGuid = Guid.Empty;
        private PovDirections _povDirection = PovDirections.Up;

        public DirectInputBinding()
        {
        }

        public DirectInputBinding(string callback, int buttonIndex, int cockpitItemId,
                                  DirectInputBindingType bindingType, PovDirections povDirection,
                                  KeyWithModifiers comboKey)
            : this(callback, buttonIndex, cockpitItemId, bindingType, povDirection, comboKey, Guid.Empty)
        {
        }

        public DirectInputBinding(string callback, int buttonIndex, int cockpitItemId,
                                  DirectInputBindingType bindingType, PovDirections povDirection,
                                  KeyWithModifiers comboKey, Guid deviceGuid)
        {
            Callback = callback;
            ButtonIndex = buttonIndex;
            CockpitItemId = cockpitItemId;
            BindingType = bindingType;
            PovDirection = povDirection;
            ComboKey = comboKey;
            DeviceGuid = deviceGuid;
        }

        public int ButtonIndex
        {
            get { return _buttonIndex; }
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
            get { return _cockpitItemId; }
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
            get { return _bindingType; }
            set { _bindingType = value; }
        }

        public PovDirections PovDirection
        {
            get { return _povDirection; }
            set { _povDirection = value; }
        }

        public KeyWithModifiers ComboKey
        {
            get { return _comboKey; }
            set { _comboKey = value; }
        }

        public Guid DeviceGuid
        {
            get { return _deviceGuid; }
            set { _deviceGuid = value; }
        }

        #region IBinding Members

        public int LineNum { get; set; }

        public string Callback
        {
            get { return _callback; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _callback = value;
            }
        }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Callback);
            sb.Append(" ");
            if (BindingType == DirectInputBindingType.Button)
            {
                sb.Append(ButtonIndex);
            }
            else
            {
                sb.Append("0");
            }
            sb.Append(" ");
            sb.Append(CockpitItemId);
            sb.Append(" ");
            sb.Append((int) BindingType);
            sb.Append(" ");
            if (BindingType == DirectInputBindingType.Button)
            {
                sb.Append("0");
            }
            else
            {
                sb.Append((int) PovDirection);
            }
            sb.Append(" ");
            if (ComboKey != null && ComboKey.ScanCode != 0)
            {
                sb.Append("0X");
                sb.Append(ComboKey.ScanCode.ToString("X").TrimStart(new[] {'0'}));
                sb.Append(" ");
                sb.Append((int) ComboKey.Modifiers);
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            if (DeviceGuid != Guid.Empty)
            {
                sb.Append(DeviceGuid);
            }
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
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

        public DirectInputBinding Parse(string input)
        {
            var tokenList = Util.Tokenize(input);
            var callback = tokenList[0];
            var buttonIndex = Int32.Parse(tokenList[1]);
            var itemId = Int32.Parse(tokenList[2]);
            var bindingType = (DirectInputBindingType) Int32.Parse(tokenList[3]);
            var povDirection = (PovDirections) Int32.Parse(tokenList[4]);
            int keycode;
            if (tokenList[5].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                keycode = Int32.Parse(tokenList[5].ToLowerInvariant().Replace("0x", string.Empty),
                                      NumberStyles.HexNumber);
            }
            else
            {
                keycode = Int32.Parse(tokenList[5]);
            }
            var modifiers = (KeyModifiers) Int32.Parse(tokenList[6]);
            var comboKey = new KeyWithModifiers(keycode, modifiers);
            DirectInputBinding binding;
            if (tokenList.Count == 8)
            {
                var isGuid = false;
                var guid = Guid.Empty;
                try
                {
                    guid = new Guid(tokenList[7]);
                    isGuid = true;
                }
                catch
                {
                }
                if (isGuid)
                {
                    binding = new DirectInputBinding(callback, buttonIndex, itemId, bindingType, povDirection, comboKey,
                                                     guid);
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
    }
}