using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using MFDExtractor.Properties;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void cmdBMSOptions_Click(object sender, EventArgs e)
        {
            var options = new frmBMSOptions();
            options.BmsPath = GetBmsPath();
            options.ShowDialog(this);
            if (!options.Cancelled)
            {
                string newBmsPath = options.BmsPath;
                if (newBmsPath != null && newBmsPath != string.Empty)
                {
                    Settings.Default.BmsPath = options.BmsPath;
                }
            }
        }

        private string GetBmsPath()
        {
            string bmsPath = Settings.Default.BmsPath;
            if (bmsPath != null) bmsPath = bmsPath.Trim();
            bool exists = false;
            if (!String.IsNullOrEmpty(bmsPath))
            {
                var bmsDirInfo = new DirectoryInfo(bmsPath);
                if (bmsDirInfo.Exists &&
                    ((bmsDirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                RegistryKey mainKey = null;
                try
                {
                    mainKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MicroProse\Falcon\4.0\", false);
                }
                catch (Exception)
                {
                }
                if (mainKey != null)
                {
                    string baseDir = null;
                    try
                    {
                        baseDir = (string)mainKey.GetValue("baseDir");
                    }
                    catch (Exception)
                    {
                    }
                    if (!String.IsNullOrEmpty(baseDir))
                    {
                        var dirInfo = new DirectoryInfo(baseDir);
                        if (dirInfo.Exists)
                        {
                            if ((dirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                bmsPath = dirInfo.FullName;
                            }
                        }
                    }
                }
            }
            return bmsPath;
        }


    }
}
