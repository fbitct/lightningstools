using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Common.InputSupport.UI;
using log4net;
using MFDExtractor.Properties;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm : Form
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (OptionsForm));
        private bool _extractorRunningStateOnFormOpen;
        private bool _formLoading = true;

        public OptionsForm()
        {
            InitializeComponent();
        }


        private void frmOptions_Load(object sender, EventArgs e)
        {
            _extractorRunningStateOnFormOpen = Extractor.GetInstance().Running; //store current running
            if (Extractor.GetInstance().Running)
            {
                Extractor.GetInstance().Stop();
            }
            Extractor.GetInstance().DataChanged += extractor_DataChanged;
            Extractor.GetInstance().TestMode = true;
            Text = Application.ProductName + " v" + Application.ProductVersion + " Options";
            sciPrimary2DModeHotkey.MinModifiers = 0;
            sciSecondary2DModeHotkey.MinModifiers = 0;
            sci3DModeHotkey.MinModifiers = 0;

            var names = Enum.GetNames(typeof (ThreadPriority));
            Array.Reverse(names);
            cboThreadPriority.Items.AddRange(names);

            cboImageFormat.Items.Add("BMP");
            cboImageFormat.Items.Add("GIF");
            cboImageFormat.Items.Add("JPEG");
            cboImageFormat.Items.Add("PNG");
            cboImageFormat.Items.Add("TIFF");

            PopulateGDIPlusOptionsCombos();

            LoadSettings();
            Extractor.GetInstance().Start();
            _formLoading = false;
        }


        private void UpdateCompressionTypeList()
        {
            cboCompressionType.Items.Clear();
            switch (cboImageFormat.SelectedItem.ToString())
            {
                case "BMP":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("RLE");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("RLE");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                case "GIF":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("RLE");
                    cboCompressionType.Items.Add("LZW");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("LZW");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                case "JPEG":
                    cboCompressionType.Items.Add("Implied");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("Implied");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
                case "PNG":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("None");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
                case "TIFF":
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.Items.Add("LZW");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("LZW");
                    cboCompressionType.Enabled = true;
                    lblCompressionType.Enabled = true;
                    break;
                default:
                    cboCompressionType.Items.Add("None");
                    cboCompressionType.SelectedIndex = cboCompressionType.FindString("");
                    cboCompressionType.Enabled = false;
                    lblCompressionType.Enabled = false;
                    break;
            }
        }
        private void extractor_DataChanged(object sender, EventArgs e)
        {
            //store the currently-selected user control on the Options form (required because
            //when we reload the user settings in the next step, the currenty-selected
            //control will go out of focus
            var currentControl = ActiveControl;

            //reload user settings from the in-memory user config
            LoadSettings();

            //refocus the control that was in focus before we reloaded the user settings
            ActiveControl = currentControl;
        }
        private void SetError(Control control, string errorString)
        {
            errControlErrorProvider.SetError(control, errorString);
            var parent = control.Parent;
            while (parent != null && parent.GetType() != typeof (TabPage))
            {
                parent = parent.Parent;
            }
            tabAllTabs.SelectedTab = ((TabPage) parent);
            control.Focus();
        }


        private bool ApplySettings(bool persist)
        {
            //Validate user settings
            if (!ValidateSettings())
            {
                return false;
            }
            //Commit the current settings to in-memory user-config settings storage (and persist them to 
            //the on-disk user-config file, if we're supposed to do that)
            SaveSettings(persist);
            return true; //if we've got this far, then validation was successful, so return true to indicate that
        }

        private void cmdApply_Click(object sender, EventArgs e)
        {
            ValidateAndApplySettings();
        }

        private void cmdOk_Click(object sender, EventArgs args)
        {
            bool valid = ValidateAndApplySettings();
            if (valid)
            {
                CloseThisDialog();
            }
        }
        private void CloseThisDialog()
        {
            try
            {
                Close(); //TODO: handle this kind of stuff centrally
            }
            catch
            {
            }
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                SettingsHelper.ReloadSettings();
                if (Extractor.GetInstance().Running)
                {
                    Extractor.GetInstance().Stop(); //stop the Extractor engine if it's running
                }
                Extractor.GetInstance().LoadSettings(); //tell the Extractor engine to reload its settings
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            Close(); //user has cancelled out of the Options form, so close the form now
        }

        private void frmOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            Extractor.GetInstance().TestMode = false;
            if (_extractorRunningStateOnFormOpen)
            {
                if (!Extractor.GetInstance().Running)
                {
                    Extractor.GetInstance().Start();
                }
            }
            else
            {
                if (Extractor.GetInstance().Running)
                {
                    Extractor.GetInstance().Stop();
                }
            }
        }

        private void StopAndRestartExtractor()
        {
            if (_formLoading) return;
            if (Extractor.GetInstance().Running)
            {
                Extractor.GetInstance().LoadSettings();
                Extractor.GetInstance().Stop();
                Extractor.GetInstance().LoadSettings();
                Extractor.GetInstance().Start();
            }
        }

        private void cboImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCompressionTypeList();
        }
        private void cmdResetToDefaults_Click(object sender, EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "Warning: This will reset all MFD Extractor options to their defaults.  You will lose any customizations you have made.  Do you want to continue?",
                    "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {
                Settings.Default.Reset();
                Settings.Default.UpgradeNeeded = false;
                bool extractorRunning = Extractor.GetInstance().Running;
                if (extractorRunning)
                {
                    Extractor.GetInstance().Stop();
                    Extractor.GetInstance().LoadSettings();
                    Extractor.GetInstance().TestMode = true;
                }
                if (extractorRunning)
                {
                    Extractor.GetInstance().Start();
                }
                LoadSettings();
            }
        }
        private void cmdNV_Click(object sender, EventArgs e)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = Extractor.GetInstance().Mediator;
            string keyFromSettingsString = Settings.Default.NVISKey;

            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof (InputControlSelection));
            }
            catch
            {
            }
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(this);
            var selection = toShow.SelectedControl;
            if (selection != null)
            {
                var serialized = Common.Serialization.Util.SerializeToXml(selection, typeof (InputControlSelection));
                Settings.Default.NVISKey = serialized;
            }
        }
        private void chkHighlightOutputWindowsWhenContainMouseCursor_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.HighlightOutputWindows = chkHighlightOutputWindowsWhenContainMouseCursor.Checked;
        }
        private void chkOnlyUpdateImagesWhenDataChanges_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.RenderInstrumentsOnlyOnStatechanges = chkOnlyUpdateImagesWhenDataChanges.Checked;
        }
    }
}