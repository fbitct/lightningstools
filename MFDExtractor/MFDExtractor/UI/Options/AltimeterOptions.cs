using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFDExtractor.Properties;
using LightningGauges.Renderers;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void rdoAltimeterStyleElectromechanical_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAltimeterStyleElectromechanical.Checked)
            {
                Settings.Default.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical.ToString();
            }
        }

        private void rdoAltimeterStyleDigital_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAltimeterStyleDigital.Checked)
            {
                Settings.Default.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic.ToString();
            }
        }

        private void rdoInchesOfMercury_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoInchesOfMercury.Checked)
            {
                Settings.Default.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury.ToString();
            }
        }

        private void rdoMillibars_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoMillibars.Checked)
            {
                Settings.Default.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars.ToString();
            }
        }

    }
}
