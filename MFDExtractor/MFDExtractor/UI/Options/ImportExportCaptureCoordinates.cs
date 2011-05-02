using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using MFDExtractor.Properties;
using F4Utils.Terrain;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void cmdImportCoordinates_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog();
            file.AddExtension = true;
            file.AutoUpgradeEnabled = true;
            file.CheckFileExists = true;
            file.CheckPathExists = true;
            file.DefaultExt = "mfde";
            file.DereferenceLinks = true;
            file.Filter = "MFD Extractor Coordinates Files|*.mfde";
            file.FilterIndex = 0;
            file.InitialDirectory = Application.ExecutablePath;
            file.Multiselect = false;
            file.ReadOnlyChecked = true;
            file.RestoreDirectory = true;
            file.ShowHelp = false;
            file.ShowReadOnly = false;
            file.SupportMultiDottedExtensions = true;
            file.Title = "Load Coordinates";
            file.ValidateNames = true;
            DialogResult result = file.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var selectForm = new frmSelectImportExportCoordinates();
                selectForm.Text = "Load Coordinates";
                selectForm.DisableAll();
                //determine which coordinate sets can be loaded
                using (StreamReader sr = File.OpenText(file.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string thisLine = sr.ReadLine();
                        if (String.IsNullOrEmpty(thisLine))
                        {
                            continue;
                        }
                        thisLine = thisLine.Trim();
                        if (String.IsNullOrEmpty(thisLine))
                        {
                            continue;
                        }
                        if (thisLine.StartsWith("//") || thisLine.StartsWith("REM") || thisLine.StartsWith("#"))
                        {
                            continue;
                        }

                        List<string> tokens = Util.Tokenize(thisLine);
                        if (tokens.Count == 3)
                        {
                            try
                            {
                                if (tokens[0].StartsWith("Primary_LMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_LMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectLeftMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_RMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectRightMfdPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD3_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd3Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_MFD4_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd4Primary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_ULX"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_ULY"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_LRX"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Primary_HUD_2D_LRY"))
                                {
                                    selectForm.EnableSelectHudPrimary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectLeftMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULX"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULY"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRX"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRY"))
                                {
                                    selectForm.EnableSelectRightMfdSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd3Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULX"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULY"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRX"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRY"))
                                {
                                    selectForm.EnableSelectMfd4Secondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_ULX"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_ULY"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_LRX"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                                else if (tokens[0].StartsWith("Secondary_HUD_2D_LRY"))
                                {
                                    selectForm.EnableSelectHudSecondary = true;
                                }
                            }
                            catch
                            {
                            }
                        } //end if (tokens.Count == 2)
                    } //end while (!sr.EndOfStream)
                } //end using (StreamReader sr = File.OpenText(file.FileName))


                //select which coordinate sets to load
                selectForm.SelectAll();
                result = selectForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    //load the selected coordinate sets from the file
                    using (StreamReader sr = File.OpenText(file.FileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            string thisLine = sr.ReadLine();
                            if (String.IsNullOrEmpty(thisLine))
                            {
                                continue;
                            }
                            thisLine = thisLine.Trim();
                            if (String.IsNullOrEmpty(thisLine))
                            {
                                continue;
                            }
                            if (thisLine.StartsWith("//") || thisLine.StartsWith("REM") || thisLine.StartsWith("#"))
                            {
                                continue;
                            }

                            List<string> tokens = Util.Tokenize(thisLine);
                            if (tokens.Count == 3)
                            {
                                try
                                {
                                    if (selectForm.ExportLeftMfdPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_LMFD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_LMFD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_LMFD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportRightMfdPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_RMFD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_RMFD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_RMFD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd3Primary)
                                    {
                                        if (tokens[0].StartsWith("Primary_MFD3_2D_ULX"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_ULY"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_LRX"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD3_2D_LRY"))
                                        {
                                            Settings.Default.Primary_MFD3_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd4Primary)
                                    {
                                        if (tokens[0].StartsWith("Primary_MFD4_2D_ULX"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_ULY"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_LRX"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_MFD4_2D_LRY"))
                                        {
                                            Settings.Default.Primary_MFD4_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportHudPrimary)
                                    {
                                        if (tokens[0].StartsWith("Primary_HUD_2D_ULX"))
                                        {
                                            Settings.Default.Primary_HUD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_ULY"))
                                        {
                                            Settings.Default.Primary_HUD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_LRX"))
                                        {
                                            Settings.Default.Primary_HUD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Primary_HUD_2D_LRY"))
                                        {
                                            Settings.Default.Primary_HUD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportLeftMfdSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_LMFD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_LMFD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_LMFD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportRightMfdSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_RMFD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_RMFD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_RMFD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd3Secondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_MFD3_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD3_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_MFD3_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportMfd4Secondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_MFD4_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_MFD4_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_MFD4_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                    if (selectForm.ExportHudSecondary)
                                    {
                                        if (tokens[0].StartsWith("Secondary_HUD_2D_ULX"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_ULX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_ULY"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_ULY = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_LRX"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_LRX = (int)UInt32.Parse(tokens[2]);
                                        }
                                        else if (tokens[0].StartsWith("Secondary_HUD_2D_LRY"))
                                        {
                                            Settings.Default.Secondary_HUD_2D_LRY = (int)UInt32.Parse(tokens[2]);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            } //end if (tokens.Count == 2)
                        } //end while (!sr.EndOfStream)
                    } //end using (StreamReader sr = File.OpenText(file.FileName))
                    LoadSettings();
                } //end if (result == DialogResult.OK)
            } //end if (result == DialogResult.OK)
        }

        private void cmdExportCoordinates_Click(object sender, EventArgs e)
        {
            if (!ValidateSettings())
            {
                MessageBox.Show(
                    "One or more settings are currently marked as invalid.\n\nYou must correct any settings that are marked as invalid before you can save coordinates to a file.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            var selectForm = new frmSelectImportExportCoordinates();
            selectForm.Text = "Save Coordinates";
            selectForm.EnableAll();
            selectForm.SelectAll();
            DialogResult result = selectForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var file = new SaveFileDialog();
                file.CreatePrompt = false;
                file.OverwritePrompt = true;
                file.AddExtension = true;
                file.AutoUpgradeEnabled = true;
                file.CheckFileExists = false;
                file.CheckPathExists = false;
                file.DefaultExt = "mfde";
                file.DereferenceLinks = true;
                file.Filter = "MFD Extractor Coordinates Files|*.mfde";
                file.FilterIndex = 0;
                file.InitialDirectory = Application.ExecutablePath;
                file.RestoreDirectory = true;
                file.ShowHelp = false;
                file.SupportMultiDottedExtensions = true;
                file.Title = "Save Coordinates";
                file.ValidateNames = true;
                result = file.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new StreamWriter(file.FileName))
                        {
                            if (selectForm.ExportLeftMfdPrimary)
                            {
                                sw.WriteLine("Primary_LMFD_2D_ULX" + " = " + Settings.Default.Primary_LMFD_2D_ULX);
                                sw.WriteLine("Primary_LMFD_2D_ULY" + " = " + Settings.Default.Primary_LMFD_2D_ULY);
                                sw.WriteLine("Primary_LMFD_2D_LRX" + " = " + Settings.Default.Primary_LMFD_2D_LRX);
                                sw.WriteLine("Primary_LMFD_2D_LRY" + " = " + Settings.Default.Primary_LMFD_2D_LRY);
                            }
                            if (selectForm.ExportRightMfdPrimary)
                            {
                                sw.WriteLine("Primary_RMFD_2D_ULX" + " = " + Settings.Default.Primary_RMFD_2D_ULX);
                                sw.WriteLine("Primary_RMFD_2D_ULY" + " = " + Settings.Default.Primary_RMFD_2D_ULY);
                                sw.WriteLine("Primary_RMFD_2D_LRX" + " = " + Settings.Default.Primary_RMFD_2D_LRX);
                                sw.WriteLine("Primary_RMFD_2D_LRY" + " = " + Settings.Default.Primary_RMFD_2D_LRY);
                            }
                            if (selectForm.ExportMfd3Primary)
                            {
                                sw.WriteLine("Primary_MFD3_2D_ULX" + " = " + Settings.Default.Primary_MFD3_2D_ULX);
                                sw.WriteLine("Primary_MFD3_2D_ULY" + " = " + Settings.Default.Primary_MFD3_2D_ULY);
                                sw.WriteLine("Primary_MFD3_2D_LRX" + " = " + Settings.Default.Primary_MFD3_2D_LRX);
                                sw.WriteLine("Primary_MFD3_2D_LRY" + " = " + Settings.Default.Primary_MFD3_2D_LRY);
                            }
                            if (selectForm.ExportMfd4Primary)
                            {
                                sw.WriteLine("Primary_MFD4_2D_ULX" + " = " + Settings.Default.Primary_MFD4_2D_ULX);
                                sw.WriteLine("Primary_MFD4_2D_ULY" + " = " + Settings.Default.Primary_MFD4_2D_ULY);
                                sw.WriteLine("Primary_MFD4_2D_LRX" + " = " + Settings.Default.Primary_MFD4_2D_LRX);
                                sw.WriteLine("Primary_MFD4_2D_LRY" + " = " + Settings.Default.Primary_MFD4_2D_LRY);
                            }
                            if (selectForm.ExportHudPrimary)
                            {
                                sw.WriteLine("Primary_HUD_2D_ULX" + " = " + Settings.Default.Primary_HUD_2D_ULX);
                                sw.WriteLine("Primary_HUD_2D_ULY" + " = " + Settings.Default.Primary_HUD_2D_ULY);
                                sw.WriteLine("Primary_HUD_2D_LRX" + " = " + Settings.Default.Primary_HUD_2D_LRX);
                                sw.WriteLine("Primary_HUD_2D_LRY" + " = " + Settings.Default.Primary_HUD_2D_LRY);
                            }
                            if (selectForm.ExportLeftMfdSecondary)
                            {
                                sw.WriteLine("Secondary_LMFD_2D_ULX" + " = " + Settings.Default.Secondary_LMFD_2D_ULX);
                                sw.WriteLine("Secondary_LMFD_2D_ULY" + " = " + Settings.Default.Secondary_LMFD_2D_ULY);
                                sw.WriteLine("Secondary_LMFD_2D_LRX" + " = " + Settings.Default.Secondary_LMFD_2D_LRX);
                                sw.WriteLine("Secondary_LMFD_2D_LRY" + " = " + Settings.Default.Secondary_LMFD_2D_LRY);
                            }
                            if (selectForm.ExportRightMfdSecondary)
                            {
                                sw.WriteLine("Secondary_RMFD_2D_ULX" + " = " + Settings.Default.Secondary_RMFD_2D_ULX);
                                sw.WriteLine("Secondary_RMFD_2D_ULY" + " = " + Settings.Default.Secondary_RMFD_2D_ULY);
                                sw.WriteLine("Secondary_RMFD_2D_LRX" + " = " + Settings.Default.Secondary_RMFD_2D_LRX);
                                sw.WriteLine("Secondary_RMFD_2D_LRY" + " = " + Settings.Default.Secondary_RMFD_2D_LRY);
                            }
                            if (selectForm.ExportMfd3Secondary)
                            {
                                sw.WriteLine("Secondary_MFD3_2D_ULX" + " = " + Settings.Default.Secondary_MFD3_2D_ULX);
                                sw.WriteLine("Secondary_MFD3_2D_ULY" + " = " + Settings.Default.Secondary_MFD3_2D_ULY);
                                sw.WriteLine("Secondary_MFD3_2D_LRX" + " = " + Settings.Default.Secondary_MFD3_2D_LRX);
                                sw.WriteLine("Secondary_MFD3_2D_LRY" + " = " + Settings.Default.Secondary_MFD3_2D_LRY);
                            }
                            if (selectForm.ExportMfd4Secondary)
                            {
                                sw.WriteLine("Secondary_MFD4_2D_ULX" + " = " + Settings.Default.Secondary_MFD4_2D_ULX);
                                sw.WriteLine("Secondary_MFD4_2D_ULY" + " = " + Settings.Default.Secondary_MFD4_2D_ULY);
                                sw.WriteLine("Secondary_MFD4_2D_LRX" + " = " + Settings.Default.Secondary_MFD4_2D_LRX);
                                sw.WriteLine("Secondary_MFD4_2D_LRY" + " = " + Settings.Default.Secondary_MFD4_2D_LRY);
                            }
                            if (selectForm.ExportHudSecondary)
                            {
                                sw.WriteLine("Secondary_HUD_2D_ULX" + " = " + Settings.Default.Secondary_HUD_2D_ULX);
                                sw.WriteLine("Secondary_HUD_2D_ULY" + " = " + Settings.Default.Secondary_HUD_2D_ULY);
                                sw.WriteLine("Secondary_HUD_2D_LRX" + " = " + Settings.Default.Secondary_HUD_2D_LRX);
                                sw.WriteLine("Secondary_HUD_2D_LRY" + " = " + Settings.Default.Secondary_HUD_2D_LRY);
                            }
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    catch (Exception f)
                    {
                        MessageBox.Show(f.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                                        MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }


    }
}
