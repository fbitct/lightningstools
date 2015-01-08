using System;
using System.IO;
using System.Windows.Forms;
using Common.Serialization;
using F4KeyFile;

namespace F4KeyFileEditor
{
    public partial class frmMain : Form
    {
        private EditorState _editorState;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _editorState = new EditorState();
            Text = Application.ProductName + " v" + Application.ProductVersion;
        }

        private static string GetKeyDescription(KeyWithModifiers keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            var keyScanCodeName = ((ScanCodes) keys.ScanCode).ToString();
            switch ((ScanCodes) keys.ScanCode)
            {
                case ScanCodes.NotAssigned:
                    keyScanCodeName = "";
                    break;
                case ScanCodes.Escape:
                    keyScanCodeName = "ESC";
                    break;
                case ScanCodes.One:
                    keyScanCodeName = "1";
                    break;
                case ScanCodes.Two:
                    keyScanCodeName = "2";
                    break;
                case ScanCodes.Three:
                    keyScanCodeName = "3";
                    break;
                case ScanCodes.Four:
                    keyScanCodeName = "4";
                    break;
                case ScanCodes.Five:
                    keyScanCodeName = "5";
                    break;
                case ScanCodes.Six:
                    keyScanCodeName = "6";
                    break;
                case ScanCodes.Seven:
                    keyScanCodeName = "7";
                    break;
                case ScanCodes.Eight:
                    keyScanCodeName = "8";
                    break;
                case ScanCodes.Nine:
                    keyScanCodeName = "9";
                    break;
                case ScanCodes.Zero:
                    keyScanCodeName = "0";
                    break;
                case ScanCodes.Minus:
                    keyScanCodeName = "-";
                    break;
                case ScanCodes.Equals:
                    keyScanCodeName = "=";
                    break;
                case ScanCodes.Backspace:
                    keyScanCodeName = "BKSP";
                    break;
                case ScanCodes.LBracket:
                    keyScanCodeName = "[";
                    break;
                case ScanCodes.RBracket:
                    keyScanCodeName = "]";
                    break;
                case ScanCodes.Return:
                    keyScanCodeName = "ENTER";
                    break;
                case ScanCodes.LControl:
                    keyScanCodeName = "LCTRL";
                    break;
                case ScanCodes.Semicolon:
                    keyScanCodeName = ";";
                    break;
                case ScanCodes.Apostrophe:
                    keyScanCodeName = "'";
                    break;
                case ScanCodes.Grave:
                    keyScanCodeName = "`";
                    break;
                case ScanCodes.LShift:
                    keyScanCodeName = "LSHIFT";
                    break;
                case ScanCodes.Backslash:
                    keyScanCodeName = "\\";
                    break;
                case ScanCodes.Comma:
                    keyScanCodeName = ",";
                    break;
                case ScanCodes.Period:
                    keyScanCodeName = ".";
                    break;
                case ScanCodes.Slash:
                    keyScanCodeName = "/";
                    break;
                case ScanCodes.RShift:
                    keyScanCodeName = "RSHIFT";
                    break;
                case ScanCodes.Multiply:
                    keyScanCodeName = "KP*";
                    break;
                case ScanCodes.LMenu:
                    keyScanCodeName = "LALT";
                    break;
                case ScanCodes.Space:
                    keyScanCodeName = "SPACE";
                    break;
                case ScanCodes.CapsLock:
                    keyScanCodeName = "CAPSLOCK";
                    break;
                case ScanCodes.NumLock:
                    keyScanCodeName = "NUMLOCK";
                    break;
                case ScanCodes.ScrollLock:
                    keyScanCodeName = "SCROLLLOCK";
                    break;
                case ScanCodes.NumPad7:
                    keyScanCodeName = "KP7";
                    break;
                case ScanCodes.NumPad8:
                    keyScanCodeName = "KP8";
                    break;
                case ScanCodes.NumPad9:
                    keyScanCodeName = "KP9";
                    break;
                case ScanCodes.Subtract:
                    keyScanCodeName = "KP-";
                    break;
                case ScanCodes.NumPad4:
                    keyScanCodeName = "KP4";
                    break;
                case ScanCodes.NumPad5:
                    keyScanCodeName = "KP5";
                    break;
                case ScanCodes.NumPad6:
                    keyScanCodeName = "KP6";
                    break;
                case ScanCodes.Add:
                    keyScanCodeName = "KP+";
                    break;
                case ScanCodes.NumPad1:
                    keyScanCodeName = "KP1";
                    break;
                case ScanCodes.NumPad2:
                    keyScanCodeName = "KP2";
                    break;
                case ScanCodes.NumPad3:
                    keyScanCodeName = "KP3";
                    break;
                case ScanCodes.NumPad0:
                    keyScanCodeName = "KP0";
                    break;
                case ScanCodes.Decimal:
                    keyScanCodeName = "KP.";
                    break;
                case ScanCodes.Kana:
                    keyScanCodeName = "KANA";
                    break;
                case ScanCodes.Convert:
                    keyScanCodeName = "CONVERT";
                    break;
                case ScanCodes.NoConvert:
                    keyScanCodeName = "NOCONVERT";
                    break;
                case ScanCodes.Yen:
                    keyScanCodeName = "YEN";
                    break;
                case ScanCodes.NumPadEquals:
                    keyScanCodeName = "KP=";
                    break;
                case ScanCodes.Circumflex:
                    keyScanCodeName = "^";
                    break;
                case ScanCodes.At:
                    keyScanCodeName = "@";
                    break;
                case ScanCodes.Colon:
                    keyScanCodeName = ":";
                    break;
                case ScanCodes.Underline:
                    keyScanCodeName = "_";
                    break;
                case ScanCodes.Kanji:
                    keyScanCodeName = "KANJI";
                    break;
                case ScanCodes.Stop:
                    keyScanCodeName = "STOP";
                    break;
                case ScanCodes.Ax:
                    keyScanCodeName = "AX";
                    break;
                case ScanCodes.Unlabeled:
                    keyScanCodeName = "UNLABELED";
                    break;
                case ScanCodes.NumPadEnter:
                    keyScanCodeName = "KPENTER";
                    break;
                case ScanCodes.RControl:
                    keyScanCodeName = "RCTRL";
                    break;
                case ScanCodes.NumPadComma:
                    keyScanCodeName = "KP,";
                    break;
                case ScanCodes.Divide:
                    keyScanCodeName = "KP/";
                    break;
                case ScanCodes.SysRq:
                    keyScanCodeName = "PRTSCR";
                    break;
                case ScanCodes.RMenu:
                    keyScanCodeName = "RALT";
                    break;
                case ScanCodes.Home:
                    keyScanCodeName = "HOME";
                    break;
                case ScanCodes.Up:
                    keyScanCodeName = "UARROW";
                    break;
                case ScanCodes.Prior:
                    keyScanCodeName = "PGUP";
                    break;
                case ScanCodes.Left:
                    keyScanCodeName = "LARROW";
                    break;
                case ScanCodes.Right:
                    keyScanCodeName = "RARROW";
                    break;
                case ScanCodes.End:
                    keyScanCodeName = "END";
                    break;
                case ScanCodes.Down:
                    keyScanCodeName = "DARROW";
                    break;
                case ScanCodes.Next:
                    keyScanCodeName = "PGDOWN";
                    break;
                case ScanCodes.Insert:
                    keyScanCodeName = "INSERT";
                    break;
                case ScanCodes.Delete:
                    keyScanCodeName = "DEL";
                    break;
                case ScanCodes.LWin:
                    keyScanCodeName = "LWIN";
                    break;
                case ScanCodes.RWin:
                    keyScanCodeName = "RWIN";
                    break;
                case ScanCodes.Apps:
                    keyScanCodeName = "APPS";
                    break;
                default:
                    break;
            }
            var keyModifierNames = "";
            switch (keys.Modifiers)
            {
                case KeyModifiers.None:
                    keyModifierNames = "";
                    break;
                case KeyModifiers.Shift:
                    keyModifierNames = "SHIFT";
                    break;
                case KeyModifiers.Ctrl:
                    keyModifierNames = "CTRL";
                    break;
                case KeyModifiers.ShiftControl:
                    keyModifierNames = "SHIFT CTRL";
                    break;
                case KeyModifiers.Alt:
                    keyModifierNames = "ALT";
                    break;
                case KeyModifiers.ShiftAlt:
                    keyModifierNames = "SHIFT ALT";
                    break;
                case KeyModifiers.CtrlAlt:
                    keyModifierNames = "CTRL ALT";
                    break;
                case KeyModifiers.ShiftCtrlAlt:
                    keyModifierNames = "SHIFT CTRL ALT";
                    break;
                default:
                    keyModifierNames = "";
                    break;
            }
            if (keys.Modifiers != KeyModifiers.None && keys.ScanCode > 0)
            {
                return (keyModifierNames + " " + keyScanCodeName).ToUpper();
            }
            if (keys.Modifiers == KeyModifiers.None && keys.ScanCode > 0)
            {
                return keyScanCodeName.ToUpper();
            }
            if (keys.Modifiers != KeyModifiers.None && keys.ScanCode <= 0)
            {
                return keyModifierNames.ToUpper();
            }
            if (keys.Modifiers == KeyModifiers.None && keys.ScanCode <= 0)
            {
                return "";
            }
            return "";
        }

        private void ParseKeyFile()
        {
            if (_editorState.KeyFile == null) throw new InvalidOperationException();
            gridKeyBindings.SuspendLayout();
            gridKeyBindings.Rows.Clear();

            gridDirectInputBindings.SuspendLayout();
            gridDirectInputBindings.Rows.Clear();

            if (_editorState.KeyFile.Lines != null)
            {
                foreach (var binding in _editorState.KeyFile.Lines)
                {
                    if (binding is DirectInputBinding)
                    {
                        var diBinding = binding as DirectInputBinding;
                        var newRowIndex = gridDirectInputBindings.Rows.Add();
                        var row = gridDirectInputBindings.Rows[newRowIndex];
                        row.Cells["diCallback"].Value = diBinding.Callback;
                        var diBindingType = "DirectInput";
                        if (diBinding.BindingType == DirectInputBindingType.POVDirection)
                        {
                            diBindingType = "POV";
                        }
                        else if (diBinding.BindingType == DirectInputBindingType.Button)
                        {
                            diBindingType = "Button";
                        }
                        row.Cells["diBindingType"].Value = diBindingType;
                        row.Cells["diButtonIndex"].Value = diBinding.ButtonIndex;
                        row.Cells["diCockpitItemId"].Value = diBinding.CockpitItemId >= 0
                                                                 ? diBinding.CockpitItemId.ToString()
                                                                 : "";
                        row.Cells["diComboKey"].Value = GetKeyDescription(diBinding.ComboKey);
                        row.Cells["diDeviceGuid"].Value = diBinding.DeviceGuid != Guid.Empty
                                                              ? diBinding.DeviceGuid.ToString()
                                                              : string.Empty;
                        row.Cells["diPovDirection"].Value = diBinding.BindingType == DirectInputBindingType.POVDirection
                                                                ? diBinding.PovDirection.ToString()
                                                                : string.Empty;
                        row.Cells["diLineNum"].Value = diBinding.LineNum;
                    }
                    else if (binding is KeyBinding)
                    {
                        var keyBinding = binding as KeyBinding;
                        var newRowIndex = gridKeyBindings.Rows.Add();
                        var row = gridKeyBindings.Rows[newRowIndex];
                        row.Cells["kbUIAccessibility"].Value = GetUIAccessibilityDescription(keyBinding.Accessibility);
                        row.Cells["kbCallback"].Value = keyBinding.Callback;
                        row.Cells["kbCockpitItemId"].Value = keyBinding.CockpitItemId >= 0
                                                                 ? keyBinding.CockpitItemId.ToString()
                                                                 : "";
                        row.Cells["kbKey"].Value = GetKeyDescription(keyBinding.Key);
                        row.Cells["kbComboKey"].Value = GetKeyDescription(keyBinding.ComboKey);
                        row.Cells["kbDescription"].Value = !string.IsNullOrEmpty(keyBinding.Description)
                                                               ? RemoveLeadingAndTrailingQuotes(keyBinding.Description)
                                                               : string.Empty;
                        row.Cells["kbMouseClickableOnly"].Value = keyBinding.MouseClickableOnly;
                        row.Cells["kbLineNum"].Value = keyBinding.LineNum;
                    }
                    Application.DoEvents();
                }
                gridKeyBindings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                gridDirectInputBindings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                gridKeyBindings.ResumeLayout();
                gridDirectInputBindings.ResumeLayout();
            }
        }

        private static string GetUIAccessibilityDescription(UIAcccessibility accessibility)
        {
            switch (accessibility)
            {
                case UIAcccessibility.VisibleWithChangesAllowed:
                    return "Visible, Changes Allowed";
                case UIAcccessibility.VisibleNoChangesAllowed:
                    return "Visible, No Changes Allowed";
                case UIAcccessibility.Invisible:
                    return "Invisible";
            }
            return "Unknown";
        }

        private static string RemoveLeadingAndTrailingQuotes(string toRemove)
        {
            var toReturn = toRemove;
            if (toReturn.StartsWith("\""))
            {
                toReturn = toReturn.Substring(1, toReturn.Length - 1);
            }
            if (toRemove.EndsWith("\""))
            {
                toReturn = toReturn.Substring(0, toReturn.Length - 1);
            }
            return toReturn;
        }

        private void LoadKeyFile()
        {
            var dlgOpen = new OpenFileDialog
                              {
                                  AddExtension = true,
                                  AutoUpgradeEnabled = true,
                                  CheckFileExists = true,
                                  CheckPathExists = true,
                                  DefaultExt = ".key",
                                  Filter = "Falcon 4 Key Files (*.key)|*.key",
                                  FilterIndex = 0,
                                  DereferenceLinks = true,
                                  Multiselect = false,
                                  ReadOnlyChecked = false,
                                  RestoreDirectory = true,
                                  ShowHelp = false,
                                  ShowReadOnly = false,
                                  SupportMultiDottedExtensions = true,
                                  Title = "Open Key File",
                                  ValidateNames = true
                              };
            //dlgOpen.InitialDirectory = new FileInfo(Application.ExecutablePath).DirectoryName;
            var result = dlgOpen.ShowDialog(this);
            if (result == DialogResult.Cancel)
            {
                return;
            }
            if (result == DialogResult.OK)
            {
                LoadKeyFile(dlgOpen.FileName);
            }
        }

        private void LoadKeyFile(string keyfilePath)
        {
            if (string.IsNullOrEmpty(keyfilePath)) throw new ArgumentNullException("keyfilePath");
            var keyfileFI = new FileInfo(keyfilePath);
            if (!keyfileFI.Exists) throw new FileNotFoundException(keyfilePath);

            var oldEditorState = (EditorState) _editorState.Clone();
            try
            {
                _editorState.KeyFilePath = keyfilePath;
                _editorState.KeyFile = new KeyFile(keyfileFI);
                _editorState.KeyFile.Load();
                ParseKeyFile();
                _editorState.ChangesMade = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred while loading the file.\n\n {0}", e.Message),
                                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                _editorState = oldEditorState;
            }
        }

        private void SaveKeyFileAs(string keyfilePath)
        {
            var oldEditorState = (EditorState) _editorState.Clone();
            try
            {
                _editorState.KeyFile.Save(new FileInfo(keyfilePath));
                _editorState.KeyFilePath = keyfilePath;
                _editorState.ChangesMade = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred while saving the file.\n\n {0}", e.Message),
                                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                _editorState = oldEditorState;
            }
        }

        private void SaveKeyFile(bool overwritePrompt, string keyFilePath)
        {
            if (string.IsNullOrEmpty(keyFilePath))
            {
                var dlgSave = new SaveFileDialog
                                  {
                                      AddExtension = true,
                                      AutoUpgradeEnabled = true,
                                      CheckFileExists = false,
                                      CheckPathExists = true,
                                      CreatePrompt = false,
                                      DefaultExt = ".key",
                                      DereferenceLinks = true,
                                      FileName = null,
                                      Filter = "Falcon 4 Key Files (*.key)|*.key",
                                      FilterIndex = 0,
                                      OverwritePrompt = overwritePrompt,
                                      RestoreDirectory = true,
                                      ShowHelp = false,
                                      SupportMultiDottedExtensions = true,
                                      Title = "Save Key File",
                                      ValidateNames = true
                                  };
                //dlgSave.InitialDirectory = new FileInfo(Application.ExecutablePath).DirectoryName;
                var result = dlgSave.ShowDialog(this);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                keyFilePath = dlgSave.FileName;
            }
            SaveKeyFileAs(keyFilePath);
        }

        private void Exit()
        {
            if (!CheckForUnsavedChangesAndSaveIfUserWantsTo())
            {
                Application.Exit();
            }
        }

        private bool CheckForUnsavedChangesAndSaveIfUserWantsTo()
        {
            if (_editorState.ChangesMade)
            {
                var res =
                    MessageBox.Show("There are unsaved changes. Would you like to save them before proceeding?",
                                    Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button3);
                if (res == DialogResult.Cancel)
                {
                    return true;
                }
                if (res == DialogResult.Yes)
                {
                    SaveKeyFile(false, _editorState.KeyFilePath);
                }
            }
            return false;
        }

        private void NewKeyFile()
        {
            if (CheckForUnsavedChangesAndSaveIfUserWantsTo())
            {
                _editorState.KeyFile = new KeyFile();
                _editorState.KeyFilePath = null;
                _editorState.ChangesMade = false;
            }
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            NewKeyFile();
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            LoadKeyFile();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            new frmHelpAbout().ShowDialog(this);
        }

        private void btnNewFile_Click(object sender, EventArgs e)
        {
            NewKeyFile();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            LoadKeyFile();
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            SaveKeyFile(false, _editorState.KeyFilePath);
        }

        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            SaveKeyFile(false, _editorState.KeyFilePath);
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveKeyFile(true, null);
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        #region Nested type: EditorState

        [Serializable]
        private class EditorState : ICloneable
        {
            public bool ChangesMade;

            public KeyFile KeyFile;
            public string KeyFilePath;

            #region ICloneable Members

            public object Clone()
            {
                return Util.DeepClone(this);
            }

            #endregion
        }

        #endregion
    }
}