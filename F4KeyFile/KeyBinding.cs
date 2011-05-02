using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyBinding : IBinding
    {
        private UIAcccessibility _accessibility = UIAcccessibility.Invisible;
        private string _callback;
        private int _cockpitItemId = -1;
        private KeyWithModifiers _comboKey;
        private string _description;
        private KeyWithModifiers _key;
        private bool _mouseClickableOnly;

        public KeyBinding()
        {
        }

        public KeyBinding(
            string callback,
            int cockpitItemId,
            bool mouseClickableOnly,
            KeyWithModifiers key,
            KeyWithModifiers comboKey,
            UIAcccessibility accessibility,
            string description
            )
        {
            Callback = callback;
            CockpitItemId = cockpitItemId;
            MouseClickableOnly = mouseClickableOnly;
            Key = key;
            ComboKey = comboKey;
            Accessibility = accessibility;
            Description = description;
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

        public bool MouseClickableOnly
        {
            get { return _mouseClickableOnly; }
            set { _mouseClickableOnly = value; }
        }

        public KeyWithModifiers Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public KeyWithModifiers ComboKey
        {
            get { return _comboKey; }
            set { _comboKey = value; }
        }

        public UIAcccessibility Accessibility
        {
            get { return _accessibility; }
            set { _accessibility = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
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

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.ToString() != ToString())
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Callback);
            sb.Append(" ");
            sb.Append(CockpitItemId);
            sb.Append(" ");
            if (MouseClickableOnly)
            {
                sb.Append("1");
            }
            else
            {
                sb.Append("0");
            }
            sb.Append(" ");
            if (Key != null && Key.ScanCode != 0)
            {
                sb.Append(Key.ToString());
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            if (ComboKey != null && ComboKey.ScanCode != 0)
            {
                sb.Append(ComboKey.ToString());
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            sb.Append((int) Accessibility);
            sb.Append(" ");
            if (Description != null)
            {
                if (Description.StartsWith("\""))
                {
                    sb.Append(Description);
                }
                else
                {
                    sb.Append("\"" + Description);
                }
                if (Description.EndsWith("\""))
                {
                }
                else
                {
                    sb.Append("\"");
                }
            }
            else
            {
                sb.Append("\"\"");
            }
            return sb.ToString();
        }

        public KeyBinding Parse(string input)
        {
            var tokenList = Util.Tokenize(input);
            var callback = tokenList[0];
            var itemId = Int32.Parse(tokenList[1]);
            var mouseClickableOnly = false;
            if (tokenList[2] == "1")
            {
                mouseClickableOnly = true;
            }

            int keycode;
            if (tokenList[3].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                keycode = Int32.Parse(tokenList[3].ToLowerInvariant().Replace("0x", string.Empty),
                                      NumberStyles.HexNumber);
            }
            else
            {
                keycode = Int32.Parse(tokenList[3]);
            }
            var modifiers = (KeyModifiers) Int32.Parse(tokenList[4]);
            var key = new KeyWithModifiers(keycode, modifiers);

            int keycode2;
            if (tokenList[5].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                keycode2 = Int32.Parse(tokenList[5].ToLowerInvariant().Replace("0x", string.Empty),
                                       NumberStyles.HexNumber);
            }
            else
            {
                keycode2 = Int32.Parse(tokenList[5]);
            }
            var modifiers2 = (KeyModifiers) Int32.Parse(tokenList[6]);
            var comboKey = new KeyWithModifiers(keycode2, modifiers2);
            if (tokenList[7].Contains("\""))
            {
                var quoteLocation = tokenList[7].IndexOf('"');
                tokenList.Insert(8, tokenList[7].Substring(quoteLocation, tokenList[7].Length - quoteLocation));
                tokenList[7] = tokenList[7].Substring(0, quoteLocation);
            }
            var accessibility = (UIAcccessibility) Int32.Parse(tokenList[7]);
            var description = new StringBuilder();
            if (tokenList.Count >= 9)
            {
                for (var i = 8; i < tokenList.Count; i++)
                {
                    description.Append(tokenList[i]);
                    if (i + 1 != tokenList.Count)
                    {
                        description.Append(" ");
                    }
                }
            }
            else
            {
                description.Append("\"\"");
            }
            return new KeyBinding(callback, itemId, mouseClickableOnly, key, comboKey, accessibility,
                                  description.ToString());
        }
    }
}