using System;
using MFDExtractor.Properties;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void chkAOAIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAOAIndicatorOutput = chkAOAIndicator.Checked;
            BringToFront();
        }

        private void chkAzimuthIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRWROutput = chkAzimuthIndicator.Checked;
            grpAzimuthIndicatorStyle.Enabled = chkAzimuthIndicator.Checked;
            BringToFront();
        }

        private void chkADI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableADIOutput = chkADI.Checked;
            BringToFront();
        }

        private void chkAirspeedIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableASIOutput = chkAirspeedIndicator.Checked;
            BringToFront();
        }

        private void chkAltimeter_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAltimeterOutput = chkAltimeter.Checked;
            grpAltimeterStyle.Enabled = chkAltimeter.Checked;
            //grpPressureAltitudeSettings.Enabled = chkAltimeter.Checked;
            BringToFront();
        }

        private void chkAOAIndexer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAOAIndexerOutput = chkAOAIndexer.Checked;
            BringToFront();
        }

        private void chkCautionPanel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCautionPanelOutput = chkCautionPanel.Checked;
            BringToFront();
        }

        private void chkCMDSPanel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCMDSOutput = chkCMDSPanel.Checked;
            BringToFront();
        }

        private void chkCompass_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCompassOutput = chkCompass.Checked;
            BringToFront();
        }

        private void chkDED_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableDEDOutput = chkDED.Checked;
            BringToFront();
        }

        private void chkFTIT1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFTIT1Output = chkFTIT1.Checked;
            BringToFront();
        }

        private void chkAccelerometer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableAccelerometerOutput = chkAccelerometer.Checked;
            BringToFront();
        }

        private void chkNOZ1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNOZ1Output = chkNOZ1.Checked;
            BringToFront();
        }

        private void chkOIL1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableOIL1Output = chkOIL1.Checked;
            BringToFront();
        }

        private void chkRPM1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRPM1Output = chkRPM1.Checked;
            BringToFront();
        }

        private void chkFTIT2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFTIT2Output = chkFTIT2.Checked;
            BringToFront();
        }

        private void chkNOZ2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNOZ2Output = chkNOZ2.Checked;
            BringToFront();
        }

        private void chkOIL2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableOIL2Output = chkOIL2.Checked;
            BringToFront();
        }

        private void chkRPM2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRPM2Output = chkRPM2.Checked;
            BringToFront();
        }

        private void chkEPU_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableEPUFuelOutput = chkEPU.Checked;
            BringToFront();
        }

        private void chkFuelFlow_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFuelFlowOutput = chkFuelFlow.Checked;
            BringToFront();
        }

        private void chkISIS_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableISISOutput = chkISIS.Checked;
            BringToFront();
        }

        private void chkFuelQty_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableFuelQuantityOutput = chkFuelQty.Checked;
            gbFuelQuantityOptions.Enabled = chkFuelQty.Checked;
            BringToFront();
        }

        private void chkGearLights_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableGearLightsOutput = chkGearLights.Checked;
            BringToFront();
        }

        private void chkHSI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHSIOutput = chkHSI.Checked;
            BringToFront();
        }

        private void chkEHSI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableEHSIOutput = chkEHSI.Checked;
            BringToFront();
        }

        private void chkNWSIndexer_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableNWSIndexerOutput = chkNWSIndexer.Checked;
            BringToFront();
        }

        private void chkPFL_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnablePFLOutput = chkPFL.Checked;
            BringToFront();
        }

        private void chkSpeedbrake_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableSpeedbrakeOutput = chkSpeedbrake.Checked;
            BringToFront();
        }

        private void chkStandbyADI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableBackupADIOutput = chkStandbyADI.Checked;
            BringToFront();
        }

        private void chkVVI_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableVVIOutput = chkVVI.Checked;
            grpVVIOptions.Enabled = chkVVI.Checked;
            BringToFront();
        }

        private void chkHydA_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHYDAOutput = chkHydA.Checked;
            BringToFront();
        }

        private void chkHydB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHYDBOutput = chkHydB.Checked;
            BringToFront();
        }

        private void chkCabinPress_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableCabinPressOutput = chkCabinPress.Checked;
            BringToFront();
        }

        private void chkRollTrim_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRollTrimOutput = chkRollTrim.Checked;
            BringToFront();
        }

        private void chkPitchTrim_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnablePitchTrimOutput = chkPitchTrim.Checked;
            BringToFront();
        }

        private void chkEnableMFD4_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableMfd4Output = chkEnableMFD4.Checked;
            cmdRecoverMfd4.Enabled = chkEnableMFD4.Checked;
            BringToFront();
        }

        private void chkEnableMFD3_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableMfd3Output = chkEnableMFD3.Checked;
            cmdRecoverMfd3.Enabled = chkEnableMFD3.Checked;
            BringToFront();
        }

        private void chkEnableLeftMFD_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableLMFDOutput = chkEnableLeftMFD.Checked;
            cmdRecoverLeftMfd.Enabled = chkEnableLeftMFD.Checked;
            BringToFront();
        }

        private void chkEnableRightMFD_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableRMFDOutput = chkEnableRightMFD.Checked;
            cmdRecoverRightMfd.Enabled = chkEnableRightMFD.Checked;
            BringToFront();
        }

        private void chkEnableHud_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableHudOutput = chkEnableHud.Checked;
            cmdRecoverHud.Enabled = chkEnableHud.Checked;
            BringToFront();
        }
    }
}
