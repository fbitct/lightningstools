using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyBinding:IBinding
    {
        private string _callback;
        private int _cockpitItemId = -1;
        private bool _mouseClickableOnly = false;
        private KeyWithModifiers _key = null;
        private KeyWithModifiers _comboKey = null;
        private UIAcccessibility _accessibility = UIAcccessibility.Invisible;
        private string _description = null;

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
            this.Callback = callback;
            this.CockpitItemId= cockpitItemId;
            this.MouseClickableOnly= mouseClickableOnly;
            this.Key= key;
            this.ComboKey= comboKey;
            this.Accessibility= accessibility;
            this.Description= description;
        }
        public int LineNum
        {
            get;
            set;
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
        public int CockpitItemId
        {
            get
            {
                return _cockpitItemId;
            }
            set
            {
                if (value >0 && value < 1000)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _cockpitItemId = value;
            }
        }
        public bool MouseClickableOnly
        {
            get
            {
                return _mouseClickableOnly;
            }
            set
            {
                _mouseClickableOnly = value;
            }
        }
        public KeyWithModifiers Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
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
        public UIAcccessibility Accessibility
        {
            get
            {
                return _accessibility;
            }
            set
            {
                _accessibility = value;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.ToString() != this.ToString())
            {
                return false;
            }
            return true;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Callback);
            sb.Append(" ");
            sb.Append(this.CockpitItemId);
            sb.Append(" ");
            if (this.MouseClickableOnly)
            {
                sb.Append("1");
            }
            else
            {
                sb.Append("0");
            }
            sb.Append(" ");
            if (this.Key != null && this.Key.ScanCode != 0)
            {
                sb.Append(this.Key.ToString());
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            if (this.ComboKey != null && this.ComboKey.ScanCode !=0)
            {
                sb.Append(this.ComboKey.ToString());
            }
            else
            {
                sb.Append("0X0 0");
            }
            sb.Append(" ");
            sb.Append((int)this.Accessibility);
            sb.Append(" ");
            if (this.Description != null)
            {
                if (this.Description.StartsWith("\"")) {
                    sb.Append(this.Description);
                }
                else {
                    sb.Append("\"" + this.Description);
                }
                if (this.Description.EndsWith("\"")) {
                }
                else {
                    sb.Append ("\"");
                }

            }
            else
            {
                sb.Append("\"\"");
            }
            return sb.ToString();
        }

        public KeyBinding Parse(string input) {
            List<string> tokenList = Util.Tokenize(input);
            string callback = tokenList[0];
            int itemId = Int32.Parse(tokenList[1]);
            bool mouseClickableOnly = false;
            if (tokenList[2] == "1")
            {
                mouseClickableOnly = true;
            }

            int keycode;
            if (tokenList[3].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                keycode = Int32.Parse(tokenList[3].ToLowerInvariant().Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                keycode = Int32.Parse(tokenList[3]);
            }
            KeyModifiers modifiers = (KeyModifiers)Int32.Parse(tokenList[4]);
            KeyWithModifiers key = new KeyWithModifiers(keycode, modifiers);

            int keycode2;
            if (tokenList[5].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                keycode2 = Int32.Parse(tokenList[5].ToLowerInvariant().Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                keycode2 = Int32.Parse(tokenList[5]);
            }
            KeyModifiers modifiers2 = (KeyModifiers)Int32.Parse(tokenList[6]);
            KeyWithModifiers comboKey = new KeyWithModifiers(keycode2, modifiers2);
            if (tokenList[7].Contains("\""))
            {
                int quoteLocation = tokenList[7].IndexOf('"');
                tokenList.Insert(8, tokenList[7].Substring(quoteLocation, tokenList[7].Length -quoteLocation));
                tokenList[7] = tokenList[7].Substring(0, quoteLocation);
            }
            UIAcccessibility accessibility = (UIAcccessibility)Int32.Parse(tokenList[7]);
            StringBuilder description = new StringBuilder();
            if (tokenList.Count >= 9)
            {
                for (int i = 8; i < tokenList.Count; i++)
                {
                    description.Append(tokenList[i]);
                    if (i + 1 != tokenList.Count)
                    {
                        description.Append(" ");
                    }
                }
            }
            else {
                description.Append ("\"\"");

            }
            return new KeyBinding(callback, itemId, mouseClickableOnly, key, comboKey, accessibility, description.ToString());
        }
    }

}
