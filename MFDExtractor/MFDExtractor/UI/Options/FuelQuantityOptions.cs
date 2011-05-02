using System;
using System.Text;
using MFDExtractor.Properties;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void rdoFuelQuantityNeedleCModel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFuelQuantityNeedleCModel.Checked)
            {
                Settings.Default.FuelQuantityIndicator_NeedleCModel = true;
            }
        }

        private void rdoFuelQuantityDModel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFuelQuantityDModel.Checked)
            {
                Settings.Default.FuelQuantityIndicator_NeedleCModel = false;
            }
        }
    }
}
