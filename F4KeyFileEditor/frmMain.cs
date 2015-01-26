using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Common.UI.UserControls.DrawingControl.SetDoubleBuffered(grid);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _editorState = new EditorState();
            Text = Application.ProductName + " v" + Application.ProductVersion;
        }

        private static string GetKeyDescription(KeyWithModifiers keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            var keyScancodeDescriptionAttibute=Common.Generic.EnumAttributeReader.GetAttribute<DescriptionAttribute>((ScanCodes)keys.ScanCode);
            var keyScanCodeName = keyScancodeDescriptionAttibute !=null ? keyScancodeDescriptionAttibute.Description: keys.ScanCode.ToString();
            var keyModifierDescriptionAttribute =
                Common.Generic.EnumAttributeReader.GetAttribute<DescriptionAttribute>(keys.Modifiers);

            var keyModifierNames = keyModifierDescriptionAttribute !=null ? keyModifierDescriptionAttribute.Description: keys.Modifiers.ToString();
           
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
            progressBar1.Visible = true;
            grid.Hide();
            grid.Enabled = false;
            grid.SuspendLayout();
            Common.UI.UserControls.DrawingControl.SuspendDrawing(grid);
            foreach (DataGridViewColumn c in grid.Columns)
            {
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.Rows.Clear();
            if (_editorState.KeyFile.Lines == null) return;
            progressBar1.Maximum = _editorState.KeyFile.Lines.Count();
            foreach (var line in _editorState.KeyFile.Lines)
            {
                if (line is DirectInputBinding)
                {
                    PopulateGridWithDirectInputBindingLineData(line);
                }
                else if (line is KeyBinding)
                {
                    PopulateGridWithKeyBindingLineData(line);
                }
                else if (line is CommentLine || line is BlankLine || line is UnparsableLine)
                {
                    PopulateGridWithCommentLineOrBlankLineOrUnparseableLineData(line);
                }
                progressBar1.Value++;
                Application.DoEvents();
            }
            foreach (DataGridViewColumn c in grid.Columns)
            {
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grid.ResumeLayout();
            grid.Enabled = true;
            grid.Show();
            Common.UI.UserControls.DrawingControl.ResumeDrawing(grid);
            progressBar1.Visible = false;
        }

        private void PopulateGridWithCommentLineOrBlankLineOrUnparseableLineData(ILineInFile line)
        {
            var newRowIndex = grid.Rows.Add();
            var row = grid.Rows[newRowIndex];
            row.Cells["LineNum"].Value = line.LineNum;
            if (line is CommentLine)
            {
                var commentLine = line as CommentLine;
                row.Cells["LineType"].Value = "Comment Line";
                row.Cells["Description"].Value = commentLine.Text ?? string.Empty;
            }
            else if (line is UnparsableLine)
            {
                var unparseableLine = line as UnparsableLine;
                row.Cells["LineType"].Value = "Unparseable Line";
                row.Cells["Description"].Value = unparseableLine.Text ?? string.Empty;
            }
            else if (line is BlankLine)
            {
                row.Cells["LineType"].Value = "Blank Line";
            }
        }

        private void PopulateGridWithKeyBindingLineData(ILineInFile line)
        {
            var keyBinding = line as KeyBinding;
            var newRowIndex = grid.Rows.Add();
            var row = grid.Rows[newRowIndex];
            row.Cells["LineNum"].Value = keyBinding.LineNum;
            row.Cells["LineType"].Value = "Key";
            row.Cells["Callback"].Value = keyBinding.Callback;
            row.Cells["SoundId"].Value = keyBinding.SoundId;
            row.Cells["kbKey"].Value = GetKeyDescription(keyBinding.Key);
            row.Cells["kbComboKey"].Value = GetKeyDescription(keyBinding.ComboKey);
            row.Cells["kbUIVisibility"].Value = GetUIAccessibilityDescription(keyBinding.UIVisibility);
            row.Cells["Description"].Value = !string.IsNullOrEmpty(keyBinding.Description)
                ? RemoveLeadingAndTrailingQuotes(keyBinding.Description)
                : string.Empty;
        }

        private void PopulateGridWithDirectInputBindingLineData(ILineInFile line)
        {
            var diBinding = line as DirectInputBinding;
            var newRowIndex = grid.Rows.Add();
            var row = grid.Rows[newRowIndex];
            row.Cells["LineNum"].Value = diBinding.LineNum;
            row.Cells["Callback"].Value = diBinding.Callback;
            row.Cells["diCallbackInvocationBehavior"].Value = string.Format("{0} / {1}", diBinding.CallbackInvocationBehavior,
                diBinding.TriggeringEvent);
            switch (diBinding.BindingType)
            {
                case DirectInputBindingType.POVDirection:
                    row.Cells["LineType"].Value = "DirectInput POV";
                    row.Cells["diButton"].Value = string.Format("POV # {0} ({1})", diBinding.POVHatNumber,
                        diBinding.PovDirection);
                    break;
                case DirectInputBindingType.Button:
                    row.Cells["LineType"].Value = "DirectInput Button";
                    row.Cells["diButton"].Value = string.Format("DI Button # {0}", diBinding.ButtonIndex);
                    break;
            }
            row.Cells["SoundId"].Value = diBinding.SoundId;
            row.Cells["Description"].Value = !string.IsNullOrEmpty(diBinding.Description)
                ? RemoveLeadingAndTrailingQuotes(diBinding.Description)
                : string.Empty;
        }

        private static string GetUIAccessibilityDescription(UIVisibility accessibility)
        {
            switch (accessibility)
            {
                case UIVisibility.Locked:
                    return "Locked (Visible, No Changes Allowed, Keys shown in green)";
                case UIVisibility.VisibleWithChangesAllowed:
                    return "Visible, Changes Allowed";
                case UIVisibility.Headline:
                    return "Headline (Visible, No Changes Allowed, Blue Background";
                case UIVisibility.Hidden:
                    return "Hidden";
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
                switch (res)
                {
                    case DialogResult.Cancel:
                        return true;
                    case DialogResult.Yes:
                        SaveKeyFile(false, _editorState.KeyFilePath);
                        break;
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