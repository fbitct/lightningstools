using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using log4net;
namespace MFDExtractor.UI
{
    public partial class frmBMSOptions : Form
    {
        private static ILog _log = LogManager.GetLogger(typeof(frmBMSOptions));

        private string _bmsPath = null;
        private bool _cancelled = false;

        public bool Cancelled
        {
            get { return _cancelled; }
            set { _cancelled = value; }
        }
        public string BmsPath
        {
            get { return _bmsPath; }
            set {
                bool valid = false;
                if (value != null && value != string.Empty)
                {
                    DirectoryInfo proposed = new DirectoryInfo(value);
                    if (proposed.Exists)
                    {
                        FileInfo bmsExeFile = new FileInfo(Path.Combine(proposed.FullName, "F4-BMS.exe"));
                        if (bmsExeFile.Exists)
                        {
                            DirectoryInfo artFolder = new DirectoryInfo(Path.Combine(proposed.FullName, @"art\ckptart"));
                            if (artFolder.Exists)
                            {
                                valid = true;
                            }
                            else
                            {
                                artFolder = new DirectoryInfo(Path.Combine(proposed.FullName, @"art\ckptartn"));
                                if (artFolder.Exists)
                                {
                                    valid = true;
                                }
                            }
                        }
                        bmsExeFile = new FileInfo(Path.Combine(proposed.FullName, "Falcon BMS.exe"));
                        if (bmsExeFile.Exists)
                        {
                            DirectoryInfo artFolder = new DirectoryInfo(Path.Combine(proposed.FullName, @"art\ckptart"));
                            if (artFolder.Exists)
                            {
                                valid = true;
                            }
                            else
                            {
                                artFolder = new DirectoryInfo(Path.Combine(proposed.FullName, @"art\ckptartn"));
                                if (artFolder.Exists)
                                {
                                    valid = true;
                                }
                            }
                        }
                    }
                }
                if (valid)
                {
                    _bmsPath = value;
                    txtBmsInstallationPath.Text = Common.Win32.Paths.Util.Compact(_bmsPath, 75);
                    grpSharedMemOptions.Enabled = true;
                    cmdOk.Enabled = true;
                }
                else
                {
                    grpSharedMemOptions.Enabled = false;
                    cmdOk.Enabled = false;
                    txtBmsInstallationPath.Text = null;
                    _bmsPath = null;
                }
            }
        }
        public frmBMSOptions()
        {
            InitializeComponent();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            bool valid = ValidateUserInput();
            if (valid)
            {
               try
                {
                    WriteBmsConfigSettings();
                }
                catch (IOException ex)
                {
                    _log.Error(ex.Message.ToString(), ex);
                }
                try
                {
                    WriteBms3DCockpitRttColorDepths();
                }
                catch (IOException ex)
                {
                    _log.Error(ex.Message.ToString(), ex);
                }
                this.Close();
            }
        }
        private void ReadBmsConfigSettings(ref bool? bms3dExportEnabled, ref int? batchSize, ref int? minBatchSize)
        {
            if (_bmsPath == null || _bmsPath == string.Empty)
            {
                return;
            }
            minBatchSize = 2;
            FileInfo file = new FileInfo(Path.Combine(_bmsPath, "FalconBMS.cfg"));
            if (!file.Exists)
            {
                file = new FileInfo(Path.Combine(Path.Combine(_bmsPath, "config"), "Falcon BMS.cfg"));
                if (file.Exists)
                {
                    try
                    {
                        FileVersionInfo verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(file.Directory.Parent.FullName, "Falcon BMS.exe"));
                        if (verInfo.ProductMajorPart >= 4)
                        {
                            minBatchSize = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
            }
            if (file.Exists)
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        List<String> tokens = Common.Strings.Util.Tokenize(currentLine);
                        if (tokens.Count > 2)
                        {
                            if (tokens[0].ToLowerInvariant() == "set")
                            {
                                if (tokens[1].ToLowerInvariant() == "g_bExportRTTTextures".ToLowerInvariant())
                                {
                                    if (tokens[2].ToLowerInvariant() == "1".ToLowerInvariant()) 
                                    {
                                        bms3dExportEnabled = true;
                                    }
                                    else 
                                    {
                                        bms3dExportEnabled = false;
                                    }
                                }
                                else if (tokens[1].ToLowerInvariant() == "g_nRTTExportBatchSize".ToLowerInvariant())
                                {
                                    try 
                                    {
                                        batchSize = Convert.ToInt32(tokens[2]);
                                    }
                                    catch (Exception e) 
                                    {
                                        _log.Error(e.Message.ToString(), e);
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }
        }
        private int? ReadBms3DCockpitRttColorDepth()
        {
            if (_bmsPath == null || _bmsPath == string.Empty)
            {
                return null;
            }
            FileInfo fileToRead = new FileInfo(Path.Combine(_bmsPath, @"art\ckptart\3dckpit.dat"));
            if (!fileToRead.Exists)
            {
                fileToRead = new FileInfo(Path.Combine(_bmsPath, @"art\ckptartn\3dckpit.dat"));
                if (!fileToRead.Exists)
                {
                    return null;
                }
            }
            int? toReturn = null;
            using (StreamReader reader = new StreamReader(fileToRead.FullName))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                    if (tokens.Count > 4)
                    {
                        if (tokens[0].ToLowerInvariant() == "rttTarget".ToLowerInvariant())
                        {
                            toReturn = Convert.ToInt32(tokens[3]);
                        }
                    }
                }
                reader.Close();
            }
            return toReturn;

        }
        private void WriteBms3DCockpitRttColorDepths()
        {
            string path = Path.Combine (_bmsPath, @"art\ckptart");
            if (new DirectoryInfo(path).Exists) 
            {
                string[] files = Directory.GetFiles(path, "3dckpit.dat", SearchOption.AllDirectories);
                foreach (string fileName in files)
                {
                    WriteBms3DCockpitRttColorDepth(new FileInfo(fileName));
                }
            }
            path = Path.Combine(_bmsPath, @"art\ckptartn");
            if (new DirectoryInfo(path).Exists)
            {
                string[] files = Directory.GetFiles(path, "3dckpit.dat", SearchOption.AllDirectories);
                foreach (string fileName in files)
                {
                    WriteBms3DCockpitRttColorDepth(new FileInfo(fileName));
                }
            }
        }
        private void WriteBms3DCockpitRttColorDepth(FileInfo fileToUpdate)
        {
            if (fileToUpdate == null)
            {
                return;
            }
            if (fileToUpdate.Exists)
            {
                List<string> allLines = new List<string>();
                using (StreamReader reader = new StreamReader(fileToUpdate.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        allLines.Add(currentLine);
                    }
                    reader.Close();
                }
                for (int i = 0; i < allLines.Count; i++)
                {
                    string currentLine = allLines[i];
                    List<string> tokens = Common.Strings.Util.Tokenize(currentLine);
                    if (tokens.Count > 2)
                    {
                        if (tokens[0].ToLowerInvariant() == "rttTarget".ToLowerInvariant()) 
                        {
                            int bitsPerPixel = 32;
                            if (rdoSixteenBit.Checked)
                            {
                                bitsPerPixel = 16;
                            }
                            if (rdoThirtyTwoBit.Checked)
                            {
                                bitsPerPixel = 32;
                            }
                            if (rdoTwentyFourBit.Checked)
                            {
                                bitsPerPixel = 24;
                            }
                            string newLine = tokens[0] + " " + tokens[1] + " " + tokens[2];
                            if (!rdoUsePrimaryColorDepth.Checked) 
                            {
                                newLine += " " + bitsPerPixel;
                            }
                            newLine += ";";
                            allLines[i] = newLine;
                        }
                    }
                }
                using (StreamWriter writer = new StreamWriter(fileToUpdate.FullName))
                {
                    foreach (string line in allLines)
                    {
                        writer.WriteLine(line);
                    }
                    writer.Flush();
                    writer.Close();
                }

            }
        }
        private void WriteBmsConfigSettings()
        {
            FileInfo file = new FileInfo(Path.Combine(_bmsPath, "FalconBMS.cfg"));
            if (!file.Exists)
            {
                file = new FileInfo(Path.Combine(Path.Combine(_bmsPath, "config"), "Falcon BMS.cfg"));
            }

            if (file.Exists)
            {
                List<string> allLines = new List<string>();
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        allLines.Add(reader.ReadLine());
                    }
                    reader.Close();
                }

                bool rttFound = false;
                bool batchSizeFound = false;
                for (int i=0;i<allLines.Count; i++)
                {
                    string currentLine = allLines[i];
                    List<String> tokens = Common.Strings.Util.Tokenize(currentLine);
                    if (tokens.Count > 2)
                    {
                        if (tokens[0].ToLowerInvariant() == "set")
                        {
                            if (tokens[1].ToLowerInvariant() == "g_bExportRTTTextures".ToLowerInvariant())
                            {
                                currentLine= "set g_bExportRTTTextures ";
                                if (chkEnable3DModeExtraction.Checked)
                                {
                                    currentLine += "1";
                                }
                                else
                                {
                                    currentLine += "0";
                                }
                                allLines[i] = currentLine;
                                rttFound = true;
                            }
                            else if (tokens[1].ToLowerInvariant() == "g_nRTTExportBatchSize".ToLowerInvariant())
                            {
                                currentLine = "set g_nRTTExportBatchSize " + udBatchSize.Value.ToString();
                                allLines[i] = currentLine;
                                batchSizeFound = true;
                            }
                        }
                    }
                }
                if (!rttFound)
                {
                    string newLine = "set g_bExportRTTTextures ";
                    if (chkEnable3DModeExtraction.Checked)
                    {
                        newLine += "1";
                    }
                    else
                    {
                        newLine+= "0";
                    }
                    allLines.Add(newLine);
                }
                if (!batchSizeFound)
                {
                    allLines.Add("set g_nRTTExportBatchSize " + udBatchSize.Value.ToString());
                }
                using (StreamWriter writer = new StreamWriter(file.FullName))
                {
                    foreach (string currentLine in allLines)
                    {
                        writer.WriteLine(currentLine);
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                MessageBox.Show(this, "A FalconBMS.cfg file could not be found in " + _bmsPath + ".  Changes could not be made to this file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private bool ValidateUserInput()
        {
            _errProvider.Clear();
            if (_bmsPath == null || txtBmsInstallationPath.Text == null)
            {
                _errProvider.SetError(lblBmsInstallationPath, "Please select your Falcon BMS installation path.");
                return false;
            }
            return true;

        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            string oldBmsPath = _bmsPath;
            dlgBrowse.ShowDialog(this);
            if (dlgBrowse.SelectedPath != null && dlgBrowse.SelectedPath != string.Empty)
            {
                DirectoryInfo proposedPath = new DirectoryInfo(dlgBrowse.SelectedPath);
                if (proposedPath.Exists)
                {
                    this.BmsPath = proposedPath.FullName;
                    if (this.BmsPath == null)
                    {
                        this.BmsPath = oldBmsPath;
                        MessageBox.Show(this, "Could not find a valid Falcon BMS installation in " + proposedPath.FullName + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            }

        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            _cancelled = true;
        }
        
        private void frmBMSOptions_Load(object sender, EventArgs e)
        {
            bool? bms3dExportEnabled= new bool?();
            int? batchSize= new int?();
            int? minBatchSize = new int?();
            ReadBmsConfigSettings(ref bms3dExportEnabled, ref batchSize, ref minBatchSize);
            if (bms3dExportEnabled.HasValue)
            {
                chkEnable3DModeExtraction.Checked = bms3dExportEnabled.Value;
            }
            if (minBatchSize.HasValue) udBatchSize.Minimum = minBatchSize.Value;
            if (batchSize.HasValue)
            {
                udBatchSize.Value = batchSize.Value >= udBatchSize.Minimum? batchSize.Value : udBatchSize.Minimum;
            }
            int? colorDepth = ReadBms3DCockpitRttColorDepth();
            if (colorDepth.HasValue)
            {
                switch (colorDepth)
                {
                    case 16:
                        rdoUsePrimaryColorDepth.Checked = false;
                        rdoThirtyTwoBit.Checked = false;
                        rdoTwentyFourBit.Checked = false;
                        rdoSixteenBit.Checked = true;
                        break;
                    case 24:
                        rdoUsePrimaryColorDepth.Checked = false;
                        rdoThirtyTwoBit.Checked = false;
                        rdoTwentyFourBit.Checked = true;
                        rdoSixteenBit.Checked = false;
                        break;
                    case 32:
                        rdoUsePrimaryColorDepth.Checked = false;
                        rdoThirtyTwoBit.Checked = true;
                        rdoTwentyFourBit.Checked = false;
                        rdoSixteenBit.Checked = false;
                        break;
                    default:
                        rdoUsePrimaryColorDepth.Checked = true;
                        rdoThirtyTwoBit.Checked = false;
                        rdoTwentyFourBit.Checked = false;
                        rdoSixteenBit.Checked = false;
                        break;
                }
            }
        }

    }
}
