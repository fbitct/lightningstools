using System;
using MFDExtractor.Properties;
using LightningGauges.Renderers;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void rdoATDPlus_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoATDPlus.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay.ToString();
            }
        }
        private void rdoAzimuthIndicatorStyleScope_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorStyleScope.Checked)
            {
                grpAzimuthIndicatorDigitalTypes.Visible = false;
                grpAzimuthIndicatorBezelTypes.Visible = true;
                if (!rdoRWRIP1310BezelType.Checked && !rdoRWRHAFBezelType.Checked)
                {
                    rdoRWRIP1310BezelType.Checked = true;
                    Settings.Default.AzimuthIndicator_ShowBezel = false;
                }
                else
                {
                    Settings.Default.AzimuthIndicator_ShowBezel = true;
                }
            }
        }

        private void rdoRWRIP1310BezelType_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRWRIP1310BezelType.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = true;
            }
        }

        private void rdoRWRHAFBezelType_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRWRHAFBezelType.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = true;
            }
        }

        private void rdoAzimuthIndicatorNoBezel_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorNoBezel.Checked)
            {
                Settings.Default.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                Settings.Default.AzimuthIndicator_ShowBezel = false;
            }
        }

        private void rdoAzimuthIndicatorStyleDigital_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAzimuthIndicatorStyleDigital.Checked)
            {
                grpAzimuthIndicatorDigitalTypes.Visible = true;
                grpAzimuthIndicatorBezelTypes.Visible = false;
                if (!rdoATDPlus.Checked)
                {
                    rdoATDPlus.Checked = true;
                }
            }
        }

    }
}
