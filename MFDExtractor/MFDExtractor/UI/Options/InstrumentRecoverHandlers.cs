using System;
using System.Windows.Forms;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void cmdRecoverMfd4_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("MFD4", Screen.FromPoint(Location));
        }

        private void cmdRecoverMfd3_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("MFD3", Screen.FromPoint(Location));
        }

        private void cmdRecoverLeftMfd_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("LMFD", Screen.FromPoint(Location));
        }

        private void cmdRecoverRightMfd_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("RMFD", Screen.FromPoint(Location));
        }

        private void cmdRecoverHud_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("HUD", Screen.FromPoint(Location));
        }
        private void pbRecoverASI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("Airspeed", Screen.FromPoint(Location));
        }

        private void pbRecoverAzimuthIndicator_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("RWR", Screen.FromPoint(Location));
        }
        private void pbRecoverADI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("ADI", Screen.FromPoint(Location));
        }

        private void pbRecoverAltimeter_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("Altimeter", Screen.FromPoint(Location));
        }

        private void pbRecoverAOAIndexer_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("AOAIndexer", Screen.FromPoint(Location));
        }

        private void pbRecoverAOAIndicator_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("AOAIndicator", Screen.FromPoint(Location));
        }

        private void pbRecoverCautionPanel_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("CautionPanel", Screen.FromPoint(Location));
        }

        private void pbRecoverCMDSPanel_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("CMDSPanel", Screen.FromPoint(Location));
        }

        private void pbRecoverCompass_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("Compass", Screen.FromPoint(Location));
        }

        private void pbRecoverDED_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("DED", Screen.FromPoint(Location));
        }

        private void pbRecoverFTIT1_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("FTIT", Screen.FromPoint(Location));
        }

        private void pbRecoverAccelerometer_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("Accelerometer", Screen.FromPoint(Location));
        }

        private void pbRecoverNozPos1_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("NOZ1", Screen.FromPoint(Location));
        }

        private void pbRecoverOil1_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("OIL1", Screen.FromPoint(Location));
        }

        private void pbRecoverRPM1_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("RPM1", Screen.FromPoint(Location));
        }

        private void pbRecoverFTIT2_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("FTIT2", Screen.FromPoint(Location));
        }

        private void pbRecoverNozPos2_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("NOZ2", Screen.FromPoint(Location));
        }

        private void pbRecoverOil2_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("OIL2", Screen.FromPoint(Location));
        }

        private void pbRecoverRPM2_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("RPM2", Screen.FromPoint(Location));
        }

        private void pbRecoverEPU_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("EPU", Screen.FromPoint(Location));
        }

        private void pbRecoverFuelFlow_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("FuelFlow", Screen.FromPoint(Location));
        }

        private void pbRecoverISIS_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("ISIS", Screen.FromPoint(Location));
        }

        private void pbRecoverNWS_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("NWS", Screen.FromPoint(Location));
        }

        private void pbRecoverPFL_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("PFL", Screen.FromPoint(Location));
        }

        private void pbRecoverSpeedbrake_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("Speedbrake", Screen.FromPoint(Location));
        }

        private void pbRecoverVVI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("VVI", Screen.FromPoint(Location));
        }

        private void pbRecoverHSI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("HSI", Screen.FromPoint(Location));
        }

        private void pbRecoverEHSI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("EHSI", Screen.FromPoint(Location));
        }

        private void pbRecoverGearLights_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("LandingGearLights", Screen.FromPoint(Location));
        }

        private void pbRecoverFuelQuantity_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("FuelQuantityIndicator", Screen.FromPoint(Location));
        }

        private void pbRecoverStandbyADI_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("StandbyADI", Screen.FromPoint(Location));
        }

        private void pbRecoverHydA_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("HYDA", Screen.FromPoint(Location));
        }

        private void pbRecoverHydB_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("HYDB", Screen.FromPoint(Location));
        }

        private void pbRecoverCabinPress_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("CabinPress", Screen.FromPoint(Location));
        }

        private void pbRecoverRollTrim_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("RollTrim", Screen.FromPoint(Location));
        }

        private void pbRecoverPitchTrim_Click(object sender, EventArgs e)
        {
            Extractor.GetInstance().RecoverInstrumentForm("PitchTrim", Screen.FromPoint(Location));
        }
    }
}
