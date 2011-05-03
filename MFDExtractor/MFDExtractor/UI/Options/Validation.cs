using System;
using System.Windows.Forms;
using System.Globalization;
using System.Net;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private bool ValidateAndApplySettings()
        {
            bool valid = false; //assume all user input is *invalid*
            try
            {
                valid = ApplySettings(true); //try to commit user settings to disk (will perform validation as well)
                if (valid)
                //if validation succeeds, we can close the form (if not, then the form's ErrorProvider will display errors to the user)
                {
                    if (_extractorRunningStateOnFormOpen)
                    {
                        StopAndRestartExtractor();
                    }
                    else
                    {
                        StopExtractor();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "One or more settings are currently marked as invalid.\n\nYou must correct any settings that are marked as invalid before you can apply changes.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception e) //exceptions will cause the Options form to close
            {
                Log.Error(e.Message, e);
            }
            return valid;
        }

        private void StopExtractor()
        {
            if (Extractor.GetInstance().Running)
            {
                Extractor.GetInstance().Stop(); //stop the Extractor if it's currently running
            }
            SettingsHelper.LoadSettings();
        }

        /// <summary>
        /// Validate all user input on all tabs
        /// </summary>
        /// <returns><see langword="true"/> if validation succeeds, or <see langword="false"/>, if validation fails due to 
        /// a user-input error</returns>
        private bool ValidateSettings()
        {
            bool isValid = true; //start with the assumption that all user input is already valid

            errControlErrorProvider.Clear();
            //clear any errors from in the Form's ErrorProvider (leftovers from previous validation attempts)

            if (!sciPrimary2DModeHotkey.IsValid)
            {
                SetError(sciPrimary2DModeHotkey, "Please select a valid hotkey for 2D Mode (primary view).");
                isValid = false;
            }
            else if (!sciSecondary2DModeHotkey.IsValid)
            {
                SetError(sciSecondary2DModeHotkey, "Please select a valid hotkey for 2D Mode (seoncdary view).");
                isValid = false;
            }
            else if (!sci3DModeHotkey.IsValid)
            {
                SetError(sci3DModeHotkey, "Please select a valid hotkey for 3D Mode.");
                isValid = false;
            }
            else if (sciPrimary2DModeHotkey.Keys == sci3DModeHotkey.Keys)
            {
                SetError(sciPrimary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 3D Mode.");
                SetError(sci3DModeHotkey, "Please select a different hotkey for 2D Mode (primary view) and 3D Mode.");
                isValid = false;
            }
            else if (sciSecondary2DModeHotkey.Keys == sci3DModeHotkey.Keys)
            {
                SetError(sciSecondary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (secondary view) and 3D Mode.");
                SetError(sci3DModeHotkey, "Please select a different hotkey for 2D Mode (secondary view) and 3D Mode.");
                isValid = false;
            }
            else if (sciPrimary2DModeHotkey.Keys == sciSecondary2DModeHotkey.Keys)
            {
                SetError(sciPrimary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 2D Mode (secondary view).");
                SetError(sciSecondary2DModeHotkey,
                         "Please select a different hotkey for 2D Mode (primary view) and 2D Mode (secondary view).");
                isValid = false;
            }
            if (isValid && (rdoServer.Checked || rdoStandalone.Checked))
            {
                if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_ULX.Text))
                {
                    SetError(txtPrimaryViewMFD4_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_ULY.Text))
                {
                    SetError(txtPrimaryViewMFD4_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_LRX.Text))
                {
                    SetError(txtPrimaryViewMFD4_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD4_LRY.Text))
                {
                    SetError(txtPrimaryViewMFD4_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_ULX.Text))
                {
                    SetError(txtPrimaryViewMFD3_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_ULY.Text))
                {
                    SetError(txtPrimaryViewMFD3_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_LRX.Text))
                {
                    SetError(txtPrimaryViewMFD3_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewMFD3_LRY.Text))
                {
                    SetError(txtPrimaryViewMFD3_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_ULX.Text))
                {
                    SetError(txtPrimaryViewLMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_ULY.Text))
                {
                    SetError(txtPrimaryViewLMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_LRX.Text))
                {
                    SetError(txtPrimaryViewLMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewLMFD_LRY.Text))
                {
                    SetError(txtPrimaryViewLMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_ULX.Text))
                {
                    SetError(txtPrimaryViewRMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_ULY.Text))
                {
                    SetError(txtPrimaryViewRMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_LRX.Text))
                {
                    SetError(txtPrimaryViewRMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewRMFD_LRY.Text))
                {
                    SetError(txtPrimaryViewRMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_ULX.Text))
                {
                    SetError(txtPrimaryViewHUD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_ULY.Text))
                {
                    SetError(txtPrimaryViewHUD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_LRX.Text))
                {
                    SetError(txtPrimaryViewHUD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtPrimaryViewHUD_LRY.Text))
                {
                    SetError(txtPrimaryViewHUD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_ULX.Text))
                {
                    SetError(txtSecondaryViewMFD4_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_ULY.Text))
                {
                    SetError(txtSecondaryViewMFD4_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_LRX.Text))
                {
                    SetError(txtSecondaryViewMFD4_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD4_LRY.Text))
                {
                    SetError(txtSecondaryViewMFD4_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_ULX.Text))
                {
                    SetError(txtSecondaryViewMFD3_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_ULY.Text))
                {
                    SetError(txtSecondaryViewMFD3_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_LRX.Text))
                {
                    SetError(txtSecondaryViewMFD3_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewMFD3_LRY.Text))
                {
                    SetError(txtSecondaryViewMFD3_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_ULX.Text))
                {
                    SetError(txtSecondaryViewLMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_ULY.Text))
                {
                    SetError(txtSecondaryViewLMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_LRX.Text))
                {
                    SetError(txtSecondaryViewLMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewLMFD_LRY.Text))
                {
                    SetError(txtSecondaryViewLMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_ULX.Text))
                {
                    SetError(txtSecondaryViewRMFD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_ULY.Text))
                {
                    SetError(txtSecondaryViewRMFD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_LRX.Text))
                {
                    SetError(txtSecondaryViewRMFD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewRMFD_LRY.Text))
                {
                    SetError(txtSecondaryViewRMFD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_ULX.Text))
                {
                    SetError(txtSecondaryViewHUD_ULX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_ULY.Text))
                {
                    SetError(txtSecondaryViewHUD_ULY, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_LRX.Text))
                {
                    SetError(txtSecondaryViewHUD_LRX, "Please enter an integer value >= 0.");
                    isValid = false;
                }
                else if (!PositiveXyCoordinateIsValid(txtSecondaryViewHUD_LRY.Text))
                {
                    SetError(txtSecondaryViewHUD_LRY, "Please enter an integer value >= 0.");
                    isValid = false;
                }

                if (isValid)
                {
                    if (Convert.ToInt32(txtPrimaryViewMFD4_ULX.Text, CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewMFD4_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD4_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewMFD4_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD4_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD4_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD4_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewMFD4_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD3_ULX.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD3_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD3_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewMFD3_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewMFD3_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewMFD3_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewMFD3_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewMFD3_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewLMFD_ULX.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewLMFD_LRX.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewLMFD_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewLMFD_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (Convert.ToInt32(txtPrimaryViewLMFD_ULY.Text, CultureInfo.InvariantCulture) >
                             Convert.ToInt32(txtPrimaryViewLMFD_LRY.Text, CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewLMFD_ULY, "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewLMFD_LRY, "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewRMFD_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewRMFD_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewRMFD_ULX, "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewRMFD_LRX, "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewRMFD_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewRMFD_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewRMFD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewRMFD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewHUD_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewHUD_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewHUD_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtPrimaryViewHUD_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtPrimaryViewHUD_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtPrimaryViewHUD_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtPrimaryViewHUD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtPrimaryViewHUD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD4_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD4_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD4_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewMFD4_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD4_ULY.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD4_LRY.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD4_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewMFD4_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(txtSecondaryViewMFD3_ULX.Text,
                                        CultureInfo.InvariantCulture) >
                        Convert.ToInt32(txtSecondaryViewMFD3_LRX.Text,
                                        CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD3_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewMFD3_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewMFD3_ULY.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewMFD3_LRY.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewMFD3_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewMFD3_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_ULX.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_LRX.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewLMFD_ULX,
                                 "Must be <= the lower right X value.");
                        SetError(txtSecondaryViewLMFD_LRX,
                                 "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_ULY.Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewLMFD_LRY.Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(txtSecondaryViewLMFD_ULY,
                                 "Must be <= the lower right Y value.");
                        SetError(txtSecondaryViewLMFD_LRY,
                                 "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_ULX.
                                Text,
                            CultureInfo.InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_LRX.
                                Text,
                            CultureInfo.InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewRMFD_ULX,
                            "Must be <= the lower right X value.");
                        SetError(
                            txtSecondaryViewRMFD_LRX,
                            "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_ULY
                                .Text,
                            CultureInfo.
                                InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewRMFD_LRY
                                .Text,
                            CultureInfo.
                                InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewRMFD_ULY,
                            "Must be <= the lower right Y value.");
                        SetError(
                            txtSecondaryViewRMFD_LRY,
                            "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewHUD_ULX
                                .Text,
                            CultureInfo.
                                InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewHUD_LRX
                                .Text,
                            CultureInfo.
                                InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewHUD_ULX,
                            "Must be <= the lower right X value.");
                        SetError(
                            txtSecondaryViewHUD_LRX,
                            "Must be >= the upper left X value.");
                        isValid = false;
                    }
                    else if (
                        Convert.ToInt32(
                            txtSecondaryViewHUD_ULY
                                .Text,
                            CultureInfo.
                                InvariantCulture) >
                        Convert.ToInt32(
                            txtSecondaryViewHUD_LRY
                                .Text,
                            CultureInfo.
                                InvariantCulture))
                    {
                        SetError(
                            txtSecondaryViewHUD_ULY,
                            "Must be <= the lower right Y value.");
                        SetError(
                            txtSecondaryViewHUD_LRY,
                            "Must be >= the upper left Y value.");
                        isValid = false;
                    }
                }
            }
            if (isValid && rdoServer.Checked)
            {
                int serverPortNum = -1;
                if (Int32.TryParse(txtNetworkServerUsePortNum.Text, out serverPortNum))
                {
                    if (serverPortNum < 0 || serverPortNum > 65535)
                    {
                        SetError(txtNetworkServerUsePortNum, "Must be in the range 0 to 65535");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtNetworkServerUsePortNum, "Must be in the range 0 to 65535");
                    isValid = false;
                }
            }
            if (isValid && rdoClient.Checked)
            {
                int clientUseServerPortNum = -1;
                if (Int32.TryParse(txtNetworkClientUseServerPortNum.Text, out clientUseServerPortNum))
                {
                    if (clientUseServerPortNum < 0 || clientUseServerPortNum > 65535)
                    {
                        SetError(txtNetworkClientUseServerPortNum, "Must be in the range 0 to 65535");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtNetworkClientUseServerPortNum, "Must be in the range 0 to 65535");
                    isValid = false;
                }
            }
            if (isValid && rdoClient.Checked)
            {
                IPAddress ipAddress;
                string serverIpAddress = ipaNetworkClientUseServerIpAddress.Text;
                if (!IPAddress.TryParse(serverIpAddress, out ipAddress))
                {
                    SetError(ipaNetworkClientUseServerIpAddress, "Please enter a valid IP address.");
                    isValid = false;
                }
            }

            if (isValid)
            {
                int pollDelay = -1;
                if (Int32.TryParse(txtPollDelay.Text, out pollDelay))
                {
                    if (pollDelay <= 0)
                    {
                        SetError(txtPollDelay, "Must be an integer > 0.");
                        isValid = false;
                    }
                }
                else
                {
                    SetError(txtPollDelay, "Must be an integer > 0.");
                    isValid = false;
                }
            }
            return isValid;
        }


        private static bool xyCoordinateIsValid(string coordinateString)
        {
            int coordinateValue = -1;
            if (Int32.TryParse(coordinateString, out coordinateValue))
            {
                return true;
            }
            return false;
        }
        private static bool PositiveXyCoordinateIsValid(string coordinateString)
        {
            int coordinateValue = -1;
            if (Int32.TryParse(coordinateString, out coordinateValue))
            {
                if (coordinateValue >= 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
