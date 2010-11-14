using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using JoyMapper.Properties;
using System.IO;
using System.Security;
using Microsoft.VisualBasic.Devices;
using log4net;

namespace JoyMapper
{
    internal sealed partial class frmOptions : Form
    {
        #region Instance Variable Declarations
        private static ILog _log = LogManager.GetLogger(typeof(frmOptions));
        private bool _cancelClose = false;
        private OpenFileDialog fileBrowseDialog;
        #endregion
        #region Constructors
        public frmOptions()
        {
            InitializeComponent();
        }
        #endregion
        #region Event Handlers
        private void chkLoadDefaultMappingFile_CheckedChanged(object sender, EventArgs e)
        {
            cmdBrowse.Enabled = chkLoadDefaultMappingFile.Checked;
            txtDefaultMappingFile.Enabled = chkLoadDefaultMappingFile.Checked;
            chkStartMappingOnProgramLaunch.Enabled = chkLoadDefaultMappingFile.Checked;
        }
        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            fileBrowseDialog.ShowDialog();
        }
        private void cmdOk_Click(object sender, EventArgs e)
        {
            Settings set = Settings.Default;
            if (chkLoadDefaultMappingFile.Checked) {
                if (txtDefaultMappingFile.Text == null || txtDefaultMappingFile.Text == "")
                {
                    errorProvider1.SetError(txtDefaultMappingFile, "Please select a mapping to load on startup, or disable auto-loading.");
                    _cancelClose = true;
                    return;
                }
                string fileName = txtDefaultMappingFile.Text;
                FileInfo fi = new FileInfo(fileName);
                if (!fi.Exists)
                {
                    errorProvider1.SetError(txtDefaultMappingFile, "The mapping file selected does not exist. Please select a valid mapping file to load on startup, or disable auto-loading.");
                    _cancelClose = true;
                    return;
                }
            }
            set.DefaultMappingFile = txtDefaultMappingFile.Text;
            set.MinimizeOnMappingStart = chkMinimizeToTrayWhenMapping.Checked;
            set.StartMappingOnLaunch = chkStartMappingOnProgramLaunch.Checked;
            set.LoadDefaultMappingFile = chkLoadDefaultMappingFile.Checked;
            set.EnableAutoHighlighting = chkAutoHighlighting.Checked;
            set.PollEveryNMillis = (int)nudPollingPeriod.Value;
            set.Save();
            if (chkLaunchAtStartup.Checked)
            {
                Computer c = new Computer();
                try
                {
                    using (Microsoft.Win32.RegistryKey startupKey = c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true))
                    {
                        startupKey.SetValue(Application.ProductName, Application.ExecutablePath, Microsoft.Win32.RegistryValueKind.String);
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug(ex.Message, ex);
                }
            }
            else
            {
                Computer c = new Computer();
                try
                {
                    using (Microsoft.Win32.RegistryKey startupKey = c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true))
                    {
                        startupKey.DeleteValue(Application.ProductName, false);
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug(ex.Message, ex);

                }
            }
        }
        private void fileBrowseDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = fileBrowseDialog.FileName;
            txtDefaultMappingFile.Text = fileName;
        }
        private void frmOptions_Load(object sender, EventArgs e)
        {
            fileBrowseDialog = new OpenFileDialog();
            fileBrowseDialog.AddExtension = true;
            fileBrowseDialog.CheckFileExists = true;
            fileBrowseDialog.CheckPathExists = true;
            fileBrowseDialog.Multiselect = false;
            fileBrowseDialog.RestoreDirectory = false;
            fileBrowseDialog.ShowHelp = false;
            fileBrowseDialog.ShowReadOnly = false;
            fileBrowseDialog.SupportMultiDottedExtensions = true;
            fileBrowseDialog.Title = "Select default mappings file";
            fileBrowseDialog.ValidateNames = true;
            fileBrowseDialog.Filter = "JoyMapper mapping files (*.map)|*.map|All files (*.*)|*.*";
            fileBrowseDialog.FileOk += new CancelEventHandler(fileBrowseDialog_FileOk);


            Settings set = Settings.Default;
            chkStartMappingOnProgramLaunch.Checked = set.StartMappingOnLaunch;
            chkMinimizeToTrayWhenMapping.Checked = set.MinimizeOnMappingStart;
            txtDefaultMappingFile.Text = set.DefaultMappingFile;
            chkLoadDefaultMappingFile.Checked = set.LoadDefaultMappingFile;
            chkAutoHighlighting.Checked = set.EnableAutoHighlighting;
            cmdBrowse.Enabled = chkLoadDefaultMappingFile.Checked;
            txtDefaultMappingFile.Enabled = chkLoadDefaultMappingFile.Checked;
            chkStartMappingOnProgramLaunch.Enabled = chkLoadDefaultMappingFile.Checked;

            nudPollingPeriod.Value = Properties.Settings.Default.PollEveryNMillis;
            String startupKeyValue = String.Empty;
            Computer c = new Computer();
            try
            {
                using (Microsoft.Win32.RegistryKey startupKey = c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    startupKeyValue = (string)startupKey.GetValue(Application.ProductName, String.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex.Message, ex);

            }

            if (startupKeyValue == null || startupKeyValue == string.Empty)
            {
                chkLaunchAtStartup.Checked = false;
            }
            else
            {
                chkLaunchAtStartup.Checked = true;
            }
        }
        private void frmOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = _cancelClose;
            _cancelClose = false;
        }
        #endregion

    }
}