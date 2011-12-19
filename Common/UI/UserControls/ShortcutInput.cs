using System.ComponentModel;
using System.Windows.Forms;

namespace Common.UI.UserControls
{
    /// <summary>
    ///   User control that allows selecting a hotkey and (optional) modifier keys such as SHIFT-, ALT-, and CTRL-.
    /// </summary>
    public class ShortcutInput : UserControl
    {
        #region Public Properties

        public byte MinModifiers;

        public byte CharCode
        {
            get { return (byte) ((string) _ddnChars.SelectedItem)[1]; }
            set
            {
                foreach (var item in _ddnChars.Items)
                {
                    if (item.ToString() == " " + (char) value)
                    {
                        _ddnChars.SelectedItem = item;
                        return;
                    }
                }
            }
        }


        public byte Win32Modifiers
        {
            get
            {
                byte toReturn = 0;
                if (_cbxShift.Checked)
                    toReturn += MOD_SHIFT;
                if (_cbxControl.Checked)
                    toReturn += MOD_CONTROL;
                if (_cbxAlt.Checked)
                    toReturn += MOD_ALT;
                return toReturn;
            }
        }


        public Keys Keys
        {
            get
            {
                var k = (Keys) CharCode;
                if (_cbxShift.Checked)
                    k |= Keys.Shift;
                if (_cbxControl.Checked)
                    k |= Keys.Control;
                if (_cbxAlt.Checked)
                    k |= Keys.Alt;
                return k;
            }
            set
            {
                var k = value;
                if (((int) k & (int) Keys.Shift) != 0)
                    Shift = true;
                if (((int) k & (int) Keys.Control) != 0)
                    Control = true;
                if (((int) k & (int) Keys.Alt) != 0)
                    Alt = true;

                CharCode = CharCodeFromKeys(k);
            }
        }


        public bool Shift
        {
            get { return _cbxShift.Checked; }
            set { _cbxShift.Checked = value; }
        }


        public bool Control
        {
            get { return _cbxControl.Checked; }
            set { _cbxControl.Checked = value; }
        }


        public bool Alt
        {
            get { return _cbxAlt.Checked; }
            set { _cbxAlt.Checked = value; }
        }


        public bool IsValid
        {
            get
            {
                byte modCount = 0;
                modCount += (byte) ((Shift) ? 1 : 0);
                modCount += (byte) ((Control) ? 1 : 0);
                modCount += (byte) ((Alt) ? 1 : 0);
                if (modCount < MinModifiers)
                    return false;
                return true;
            }
        }

        #endregion

        private const byte MOD_ALT = 1, MOD_CONTROL = 2, MOD_SHIFT = 4;

        /// <summary>
        ///   Required designer variable.
        /// </summary>
        private readonly Container components;

        private CheckBox _cbxAlt;
        private CheckBox _cbxControl;
        private CheckBox _cbxShift;
        private ComboBox _ddnChars;


        public ShortcutInput()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            for (var i = 65; i < 91; i++)
                _ddnChars.Items.Add(" " + (char) i);

            for (var i = 48; i < 58; i++)
                _ddnChars.Items.Add(" " + (char) i);

            _ddnChars.SelectedIndex = 0;
        }


        /// <summary>
        ///   Calculates the Win32 Modifiers total for a Keys enum
        /// </summary>
        /// <param name = "k">An instance of the Keys enumaration</param>
        /// <returns>The Win32 Modifiers total as required by RegisterHotKey</returns>
        public static byte Win32ModifiersFromKeys(Keys k)
        {
            return (byte) (k & Keys.Modifiers);
        }


        /// <summary>
        ///   Calculates the character code of alphanumeric key of the Keys enum instance
        /// </summary>
        /// <param name = "k">An instance of the Keys enumaration</param>
        /// <returns>The character code of the alphanumeric key</returns>
        public static byte CharCodeFromKeys(Keys k)
        {
            return (byte) (k & Keys.KeyCode);
        }


        /// <summary>
        ///   Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Common.Util.DisposeObject(components);
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        ///   Required method for Designer support - do not modify 
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._cbxShift = new System.Windows.Forms.CheckBox();
            this._cbxControl = new System.Windows.Forms.CheckBox();
            this._cbxAlt = new System.Windows.Forms.CheckBox();
            this._ddnChars = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _cbxShift
            // 
            this._cbxShift.Location = new System.Drawing.Point(14, 3);
            this._cbxShift.Name = "_cbxShift";
            this._cbxShift.Size = new System.Drawing.Size(61, 24);
            this._cbxShift.TabIndex = 0;
            this._cbxShift.Text = "Shift";
            // 
            // _cbxControl
            // 
            this._cbxControl.Location = new System.Drawing.Point(93, 3);
            this._cbxControl.Name = "_cbxControl";
            this._cbxControl.Size = new System.Drawing.Size(88, 24);
            this._cbxControl.TabIndex = 1;
            this._cbxControl.Text = "Control";
            // 
            // _cbxAlt
            // 
            this._cbxAlt.Location = new System.Drawing.Point(187, 3);
            this._cbxAlt.Name = "_cbxAlt";
            this._cbxAlt.Size = new System.Drawing.Size(60, 24);
            this._cbxAlt.TabIndex = 2;
            this._cbxAlt.Text = "Alt";
            // 
            // _ddnChars
            // 
            this._ddnChars.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ddnChars.Location = new System.Drawing.Point(253, 3);
            this._ddnChars.Name = "_ddnChars";
            this._ddnChars.Size = new System.Drawing.Size(40, 24);
            this._ddnChars.TabIndex = 4;
            // 
            // ShortcutInput
            // 
            this.Controls.Add(this._ddnChars);
            this.Controls.Add(this._cbxAlt);
            this.Controls.Add(this._cbxControl);
            this.Controls.Add(this._cbxShift);
            this.Name = "ShortcutInput";
            this.Size = new System.Drawing.Size(360, 29);
            this.ResumeLayout(false);

        }

        #endregion
    }
}