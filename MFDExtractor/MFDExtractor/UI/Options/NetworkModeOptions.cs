using System;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {

        private void rdoStandalone_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoStandalone.Checked)
            {
                EnableStandaloneModeOptions();
            }
        }
        private void rdoClient_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoClient.Checked)
            {
                EnableClientModeOptions();
            }
        }
        private void rdoServer_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoServer.Checked)
            {
                EnableServerModeOptions();
            }
        }
        private void EnableServerModeOptions()
        {
            rdoServer.Checked = true;
            rdoClient.Checked = false;
            rdoStandalone.Checked = false;
            grpServerOptions.Enabled = true;
            grpServerOptions.Visible = true;
            grpServerOptions.BringToFront();
            grpClientOptions.Enabled = false;
            grpClientOptions.Visible = false;
            grpClientOptions.SendToBack();
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = true;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = true;
            //            tabHotkeysInner.Enabled = true;
            cmdBMSOptions.Enabled = true;
            errControlErrorProvider.Clear();
        }
        private void EnableClientModeOptions()
        {
            rdoClient.Checked = true;
            rdoStandalone.Checked = false;
            rdoServer.Checked = false;
            grpClientOptions.Enabled = true;
            grpClientOptions.Visible = true;
            grpClientOptions.BringToFront();
            grpServerOptions.Enabled = false;
            grpServerOptions.Visible = false;
            grpServerOptions.SendToBack();
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = false;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = false;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = false;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = false;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = false;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = false;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = false;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = false;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = false;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = false;
            //            tabHotkeysInner.Enabled = false;
            cmdBMSOptions.Enabled = false;
            errControlErrorProvider.Clear();
        }
        private void EnableStandaloneModeOptions()
        {
            rdoServer.Checked = false;
            rdoClient.Checked = false;
            rdoStandalone.Checked = true;
            grpServerOptions.Enabled = false;
            grpClientOptions.Enabled = false;
            grpServerOptions.Visible = false;
            grpClientOptions.Visible = false;
            grpPrimaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpPrimaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd4ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewMfd3ImageSourceCoordinates.Enabled = true;
            grpSecondaryViewLeftMfdImageSourceCoordinates.Enabled = true;
            grpSecondaryViewRightMfdImageSourceCoordinates.Enabled = true;
            grpPrimaryViewHudImageSourceCoordinates.Enabled = true;
            grpSecondaryViewHudImageSourceCoordinates.Enabled = true;
            //            tabHotkeysInner.Enabled = true;
            cmdBMSOptions.Enabled = true;
            errControlErrorProvider.Clear();
        }

    }
}
