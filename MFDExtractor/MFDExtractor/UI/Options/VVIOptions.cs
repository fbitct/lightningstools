using System.Text;
using MFDExtractor.Properties;
using System;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void rdoVVIStyleTape_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVVIStyleTape.Checked)
            {
                Settings.Default.VVI_Style = VVIStyles.Tape.ToString();
            }
        }

        private void rdoVVIStyleNeedle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVVIStyleNeedle.Checked)
            {
                Settings.Default.VVI_Style = VVIStyles.Needle.ToString();
            }
        }
    }
}
