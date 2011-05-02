using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFDExtractor.Properties;
using System.Globalization;
using LightningGauges.Renderers;
using System.Threading;
using Common.Networking;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System.Windows.Forms;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        /// <summary>
        /// Saves user settings to the in-memory user-config cache (and optionally, to disk as well)
        /// </summary>
        /// <param name="persist">if <see langword="true"/>, the current settings will be persisited to 
        /// the on-disk user-config file.  If <see langword="false"/>, the settings will not be 
        /// persisited to disk.  In either case, the current Extractor instance will be 
        /// updated with the new settings.</param>
        private void SaveSettings(bool persist)
        {
            Settings settings = Settings.Default;
            settings.UpgradeNeeded = false;
            settings.NetworkImageFormat = cboImageFormat.SelectedItem.ToString();
            settings.CompressionType = cboCompressionType.SelectedItem.ToString();
            if (rdoServer.Checked || rdoStandalone.Checked)
            {
                settings.Primary_MFD4_2D_ULX = Convert.ToInt32(txtPrimaryViewMFD4_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_ULY = Convert.ToInt32(txtPrimaryViewMFD4_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_LRX = Convert.ToInt32(txtPrimaryViewMFD4_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD4_2D_LRY = Convert.ToInt32(txtPrimaryViewMFD4_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_ULX = Convert.ToInt32(txtPrimaryViewMFD3_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_ULY = Convert.ToInt32(txtPrimaryViewMFD3_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_LRX = Convert.ToInt32(txtPrimaryViewMFD3_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_MFD3_2D_LRY = Convert.ToInt32(txtPrimaryViewMFD3_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_ULX = Convert.ToInt32(txtPrimaryViewLMFD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_ULY = Convert.ToInt32(txtPrimaryViewLMFD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_LRX = Convert.ToInt32(txtPrimaryViewLMFD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_LMFD_2D_LRY = Convert.ToInt32(txtPrimaryViewLMFD_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_ULX = Convert.ToInt32(txtPrimaryViewRMFD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_ULY = Convert.ToInt32(txtPrimaryViewRMFD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_LRX = Convert.ToInt32(txtPrimaryViewRMFD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_RMFD_2D_LRY = Convert.ToInt32(txtPrimaryViewRMFD_LRY.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_ULX = Convert.ToInt32(txtPrimaryViewHUD_ULX.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_ULY = Convert.ToInt32(txtPrimaryViewHUD_ULY.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_LRX = Convert.ToInt32(txtPrimaryViewHUD_LRX.Text, CultureInfo.InvariantCulture);
                settings.Primary_HUD_2D_LRY = Convert.ToInt32(txtPrimaryViewHUD_LRY.Text, CultureInfo.InvariantCulture);


                settings.Secondary_MFD4_2D_ULX = Convert.ToInt32(txtSecondaryViewMFD4_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_ULY = Convert.ToInt32(txtSecondaryViewMFD4_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_LRX = Convert.ToInt32(txtSecondaryViewMFD4_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD4_2D_LRY = Convert.ToInt32(txtSecondaryViewMFD4_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_ULX = Convert.ToInt32(txtSecondaryViewMFD3_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_ULY = Convert.ToInt32(txtSecondaryViewMFD3_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_LRX = Convert.ToInt32(txtSecondaryViewMFD3_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_MFD3_2D_LRY = Convert.ToInt32(txtSecondaryViewMFD3_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_ULX = Convert.ToInt32(txtSecondaryViewLMFD_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_ULY = Convert.ToInt32(txtSecondaryViewLMFD_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_LRX = Convert.ToInt32(txtSecondaryViewLMFD_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_LMFD_2D_LRY = Convert.ToInt32(txtSecondaryViewLMFD_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_ULX = Convert.ToInt32(txtSecondaryViewRMFD_ULX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_ULY = Convert.ToInt32(txtSecondaryViewRMFD_ULY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_LRX = Convert.ToInt32(txtSecondaryViewRMFD_LRX.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_RMFD_2D_LRY = Convert.ToInt32(txtSecondaryViewRMFD_LRY.Text,
                                                                 CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_ULX = Convert.ToInt32(txtSecondaryViewHUD_ULX.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_ULY = Convert.ToInt32(txtSecondaryViewHUD_ULY.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_LRX = Convert.ToInt32(txtSecondaryViewHUD_LRX.Text,
                                                                CultureInfo.InvariantCulture);
                settings.Secondary_HUD_2D_LRY = Convert.ToInt32(txtSecondaryViewHUD_LRY.Text,
                                                                CultureInfo.InvariantCulture);
            }
            settings.EnableMfd4Output = chkEnableMFD4.Checked;
            settings.EnableMfd3Output = chkEnableMFD3.Checked;
            settings.EnableLMFDOutput = chkEnableLeftMFD.Checked;
            settings.EnableRMFDOutput = chkEnableRightMFD.Checked;
            settings.EnableHudOutput = chkEnableHud.Checked;

            settings.EnableRWROutput = chkAzimuthIndicator.Checked;
            settings.EnableADIOutput = chkADI.Checked;
            settings.EnableBackupADIOutput = chkStandbyADI.Checked;
            settings.EnableASIOutput = chkAirspeedIndicator.Checked;
            settings.EnableAltimeterOutput = chkAltimeter.Checked;
            settings.EnableAOAIndexerOutput = chkAOAIndexer.Checked;
            settings.EnableAOAIndicatorOutput = chkAOAIndicator.Checked;
            settings.EnableCautionPanelOutput = chkCautionPanel.Checked;
            settings.EnableCMDSOutput = chkCMDSPanel.Checked;
            settings.EnableCompassOutput = chkCompass.Checked;
            settings.EnableDEDOutput = chkDED.Checked;
            settings.EnableAccelerometerOutput = chkAccelerometer.Checked;
            settings.EnableFTIT1Output = chkFTIT1.Checked;
            settings.EnableFTIT2Output = chkFTIT2.Checked;
            settings.EnableNOZ1Output = chkNOZ1.Checked;
            settings.EnableNOZ2Output = chkNOZ2.Checked;
            settings.EnableOIL1Output = chkOIL1.Checked;
            settings.EnableOIL2Output = chkOIL2.Checked;
            settings.EnableRPM1Output = chkRPM1.Checked;
            settings.EnableRPM2Output = chkRPM2.Checked;
            settings.EnableEPUFuelOutput = chkEPU.Checked;
            settings.EnableFuelFlowOutput = chkFuelFlow.Checked;
            settings.EnableISISOutput = chkISIS.Checked;
            settings.EnableFuelQuantityOutput = chkFuelQty.Checked;
            settings.EnableGearLightsOutput = chkGearLights.Checked;
            settings.EnableHSIOutput = chkHSI.Checked;
            settings.EnableEHSIOutput = chkEHSI.Checked;
            settings.EnableNWSIndexerOutput = chkNWSIndexer.Checked;
            settings.EnablePFLOutput = chkPFL.Checked;
            settings.EnableSpeedbrakeOutput = chkSpeedbrake.Checked;
            settings.EnableVVIOutput = chkVVI.Checked;
            settings.EnableHYDAOutput = chkHydA.Checked;
            settings.EnableHYDBOutput = chkHydB.Checked;
            settings.EnableCabinPressOutput = chkCabinPress.Checked;
            settings.EnableRollTrimOutput = chkRollTrim.Checked;
            settings.EnablePitchTrimOutput = chkPitchTrim.Checked;


            if (rdoMillibars.Checked)
            {
                settings.Altimeter_PressureUnits = F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars.ToString();
            }
            else if (rdoInchesOfMercury.Checked)
            {
                settings.Altimeter_PressureUnits =
                    F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury.ToString();
            }

            if (rdoAltimeterStyleDigital.Checked)
            {
                settings.Altimeter_Style = F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic.ToString();
            }
            else if (rdoAltimeterStyleElectromechanical.Checked)
            {
                settings.Altimeter_Style =
                    F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical.ToString();
            }

            if (rdoAzimuthIndicatorStyleScope.Checked)
            {
                if (rdoRWRIP1310BezelType.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                    settings.AzimuthIndicator_ShowBezel = true;
                }
                else if (rdoRWRHAFBezelType.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF.ToString();
                    settings.AzimuthIndicator_ShowBezel = true;
                }
                else if (rdoAzimuthIndicatorNoBezel.Checked)
                {
                    settings.AzimuthIndicatorType =
                        F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR.ToString();
                    settings.AzimuthIndicator_ShowBezel = false;
                }
            }
            else if (rdoAzimuthIndicatorStyleDigital.Checked)
            {
                settings.AzimuthIndicatorType =
                    F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay.ToString();
            }

            if (rdoVVIStyleNeedle.Checked)
            {
                settings.VVI_Style = VVIStyles.Needle.ToString();
            }
            else if (rdoVVIStyleTape.Checked)
            {
                settings.VVI_Style = VVIStyles.Tape.ToString();
            }

            if (rdoFuelQuantityNeedleCModel.Checked)
            {
                settings.FuelQuantityIndicator_NeedleCModel = true;
            }
            else if (rdoFuelQuantityDModel.Checked)
            {
                settings.FuelQuantityIndicator_NeedleCModel = false;
            }

            settings.StartOnLaunch = chkStartOnLaunch.Checked;
            if (rdoStandalone.Checked || rdoServer.Checked)
            {
                settings.TwoDPrimaryHotkey = sciPrimary2DModeHotkey.Keys;
                settings.TwoDSecondaryHotkey = sciSecondary2DModeHotkey.Keys;
                settings.ThreeDHotkey = sci3DModeHotkey.Keys;
            }
            settings.ThreadPriority =
                (ThreadPriority)Enum.Parse(typeof(ThreadPriority), cboThreadPriority.SelectedItem.ToString());
            settings.PollingDelay = Convert.ToInt32(txtPollDelay.Text, CultureInfo.InvariantCulture);
            settings.LaunchWithWindows = chkStartWithWindows.Checked;

            if (rdoClient.Checked)
            {
                settings.NetworkingMode = (int)NetworkMode.Client;
                settings.ClientUseServerPortNum = Convert.ToInt32(txtNetworkClientUseServerPortNum.Text,
                                                                  CultureInfo.InvariantCulture);
                settings.ClientUseServerIpAddress = ipaNetworkClientUseServerIpAddress.Text;
            }
            else if (rdoServer.Checked)
            {
                settings.NetworkingMode = (int)NetworkMode.Server;
                settings.ServerUsePortNumber = Convert.ToInt32(txtNetworkServerUsePortNum.Text,
                                                               CultureInfo.InvariantCulture);
            }
            else if (rdoStandalone.Checked)
            {
                settings.NetworkingMode = (int)NetworkMode.Standalone;
            }

            SaveGDIPlusSettings();

            settings.HighlightOutputWindows = chkHighlightOutputWindowsWhenContainMouseCursor.Checked;
            settings.RenderInstrumentsOnlyOnStatechanges = chkOnlyUpdateImagesWhenDataChanges.Checked;
            if (persist) //persist the user settings to disk
            {
                bool testMode = Extractor.GetInstance().TestMode; //store whether we're in test mode
                settings.TestMode = false; //clear test mode flag from the current settings cache (in-memory)
                _extractorRunningStateOnFormOpen = Extractor.GetInstance().Running;
                //store current Extractor instance's "isRunning" state
                SettingsHelper.SaveAndReloadSettings();
                settings.TestMode = testMode; //reset the test-mode flag in the current settings cache

                //we have to do the above steps because we don't want the test-mode flag
                //to be persisted to disk, but we do need it in memory to signal the Extractor
            }
            if (persist)
            {
                //update the Windows Registry's Run-at-startup applications list according
                //to the new user settings
                if (chkStartWithWindows.Checked)
                {
                    var c = new Computer();
                    try
                    {
                        using (
                            RegistryKey startupKey =
                                c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
                            )
                        {
                            startupKey.SetValue(Application.ProductName, Application.ExecutablePath,
                                                RegistryValueKind.String);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
                else
                {
                    var c = new Computer();
                    try
                    {
                        using (
                            RegistryKey startupKey =
                                c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
                            )
                        {
                            startupKey.DeleteValue(Application.ProductName, false);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
            }
        }
        /// <summary>
        /// Reloads user settings from the in-memory user settings class (Properties.Settings)
        /// </summary>
        private void LoadSettings()
        {
            //load all committed user settings from memory (these may not mirror the settings
            //which have been persisted to the user-config file on disk; that only happens 
            //when Properties.Settings.Default.Save() is called)
            var settings = Settings.Default;
            settings.UpgradeNeeded = false;

            //update the UI elements with the corresponding settings values that we just read in
            ipaNetworkClientUseServerIpAddress.Text = settings.ClientUseServerIpAddress;
            txtNetworkClientUseServerPortNum.Text =
                settings.ClientUseServerPortNum.ToString(CultureInfo.InvariantCulture);
            txtNetworkServerUsePortNum.Text = settings.ServerUsePortNumber.ToString(CultureInfo.InvariantCulture);

            //set the current Extractor instance's network mode as per the user-config
            var networkMode = (NetworkMode)settings.NetworkingMode;
            if (networkMode == NetworkMode.Client)
            {
                EnableClientModeOptions();
            }
            else
            {
                if (networkMode == NetworkMode.Server)
                {
                    EnableServerModeOptions();
                }
                else
                {
                    EnableStandaloneModeOptions();
                }
            }

            cboThreadPriority.SelectedItem = settings.ThreadPriority.ToString();


            txtPrimaryViewMFD4_ULX.Text = "" + settings.Primary_MFD4_2D_ULX;
            txtPrimaryViewMFD4_ULY.Text = "" + settings.Primary_MFD4_2D_ULY;
            txtPrimaryViewMFD4_LRX.Text = "" + settings.Primary_MFD4_2D_LRX;
            txtPrimaryViewMFD4_LRY.Text = "" + settings.Primary_MFD4_2D_LRY;
            txtPrimaryViewMFD3_ULX.Text = "" + settings.Primary_MFD3_2D_ULX;
            txtPrimaryViewMFD3_ULY.Text = "" + settings.Primary_MFD3_2D_ULY;
            txtPrimaryViewMFD3_LRX.Text = "" + settings.Primary_MFD3_2D_LRX;
            txtPrimaryViewMFD3_LRY.Text = "" + settings.Primary_MFD3_2D_LRY;
            txtPrimaryViewLMFD_ULX.Text = "" + settings.Primary_LMFD_2D_ULX;
            txtPrimaryViewLMFD_ULY.Text = "" + settings.Primary_LMFD_2D_ULY;
            txtPrimaryViewLMFD_LRX.Text = "" + settings.Primary_LMFD_2D_LRX;
            txtPrimaryViewLMFD_LRY.Text = "" + settings.Primary_LMFD_2D_LRY;
            txtPrimaryViewRMFD_ULX.Text = "" + settings.Primary_RMFD_2D_ULX;
            txtPrimaryViewRMFD_ULY.Text = "" + settings.Primary_RMFD_2D_ULY;
            txtPrimaryViewRMFD_LRX.Text = "" + settings.Primary_RMFD_2D_LRX;
            txtPrimaryViewRMFD_LRY.Text = "" + settings.Primary_RMFD_2D_LRY;
            txtPrimaryViewHUD_ULX.Text = "" + settings.Primary_HUD_2D_ULX;
            txtPrimaryViewHUD_ULY.Text = "" + settings.Primary_HUD_2D_ULY;
            txtPrimaryViewHUD_LRX.Text = "" + settings.Primary_HUD_2D_LRX;
            txtPrimaryViewHUD_LRY.Text = "" + settings.Primary_HUD_2D_LRY;


            txtSecondaryViewMFD4_ULX.Text = "" + settings.Secondary_MFD4_2D_ULX;
            txtSecondaryViewMFD4_ULY.Text = "" + settings.Secondary_MFD4_2D_ULY;
            txtSecondaryViewMFD4_LRX.Text = "" + settings.Secondary_MFD4_2D_LRX;
            txtSecondaryViewMFD4_LRY.Text = "" + settings.Secondary_MFD4_2D_LRY;
            txtSecondaryViewMFD3_ULX.Text = "" + settings.Secondary_MFD3_2D_ULX;
            txtSecondaryViewMFD3_ULY.Text = "" + settings.Secondary_MFD3_2D_ULY;
            txtSecondaryViewMFD3_LRX.Text = "" + settings.Secondary_MFD3_2D_LRX;
            txtSecondaryViewMFD3_LRY.Text = "" + settings.Secondary_MFD3_2D_LRY;
            txtSecondaryViewLMFD_ULX.Text = "" + settings.Secondary_LMFD_2D_ULX;
            txtSecondaryViewLMFD_ULY.Text = "" + settings.Secondary_LMFD_2D_ULY;
            txtSecondaryViewLMFD_LRX.Text = "" + settings.Secondary_LMFD_2D_LRX;
            txtSecondaryViewLMFD_LRY.Text = "" + settings.Secondary_LMFD_2D_LRY;
            txtSecondaryViewRMFD_ULX.Text = "" + settings.Secondary_RMFD_2D_ULX;
            txtSecondaryViewRMFD_ULY.Text = "" + settings.Secondary_RMFD_2D_ULY;
            txtSecondaryViewRMFD_LRX.Text = "" + settings.Secondary_RMFD_2D_LRX;
            txtSecondaryViewRMFD_LRY.Text = "" + settings.Secondary_RMFD_2D_LRY;
            txtSecondaryViewHUD_ULX.Text = "" + settings.Secondary_HUD_2D_ULX;
            txtSecondaryViewHUD_ULY.Text = "" + settings.Secondary_HUD_2D_ULY;
            txtSecondaryViewHUD_LRX.Text = "" + settings.Secondary_HUD_2D_LRX;
            txtSecondaryViewHUD_LRY.Text = "" + settings.Secondary_HUD_2D_LRY;


            chkEnableMFD4.Checked = settings.EnableMfd4Output;
            chkEnableMFD3.Checked = settings.EnableMfd3Output;
            chkEnableLeftMFD.Checked = settings.EnableLMFDOutput;
            chkEnableRightMFD.Checked = settings.EnableRMFDOutput;
            chkEnableHud.Checked = settings.EnableHudOutput;

            cmdRecoverHud.Enabled = chkEnableHud.Checked;
            cmdRecoverMfd3.Enabled = chkEnableMFD3.Checked;
            cmdRecoverMfd4.Enabled = chkEnableMFD4.Checked;
            cmdRecoverLeftMfd.Enabled = chkEnableLeftMFD.Checked;
            cmdRecoverRightMfd.Enabled = chkEnableRightMFD.Checked;

            sciPrimary2DModeHotkey.Keys = settings.TwoDPrimaryHotkey;
            sciPrimary2DModeHotkey.Refresh();

            sciSecondary2DModeHotkey.Keys = settings.TwoDSecondaryHotkey;
            sciSecondary2DModeHotkey.Refresh();

            sci3DModeHotkey.Keys = settings.ThreeDHotkey;
            sci3DModeHotkey.Refresh();
            chkStartOnLaunch.Checked = settings.StartOnLaunch;
            chkStartWithWindows.Checked = settings.LaunchWithWindows;

            txtPollDelay.Text = "" + settings.PollingDelay;
            cboThreadPriority.SelectedItem = settings.ThreadPriority;
            cboImageFormat.SelectedIndex = cboImageFormat.FindString(settings.NetworkImageFormat);
            UpdateCompressionTypeList();
            cboCompressionType.SelectedIndex = cboCompressionType.FindString(settings.CompressionType);

            chkAzimuthIndicator.Checked = settings.EnableRWROutput;
            chkADI.Checked = settings.EnableADIOutput;
            chkStandbyADI.Checked = settings.EnableBackupADIOutput;
            chkAirspeedIndicator.Checked = settings.EnableASIOutput;
            chkAltimeter.Checked = settings.EnableAltimeterOutput;
            chkAOAIndexer.Checked = settings.EnableAOAIndexerOutput;
            chkAOAIndicator.Checked = settings.EnableAOAIndicatorOutput;
            chkCautionPanel.Checked = settings.EnableCautionPanelOutput;
            chkCMDSPanel.Checked = settings.EnableCMDSOutput;
            chkCompass.Checked = settings.EnableCompassOutput;
            chkDED.Checked = settings.EnableDEDOutput;
            chkFTIT1.Checked = settings.EnableFTIT1Output;
            chkFTIT2.Checked = settings.EnableFTIT2Output;
            chkAccelerometer.Checked = settings.EnableAccelerometerOutput;
            chkNOZ1.Checked = settings.EnableNOZ1Output;
            chkNOZ2.Checked = settings.EnableNOZ2Output;
            chkOIL1.Checked = settings.EnableOIL1Output;
            chkOIL2.Checked = settings.EnableOIL2Output;
            chkRPM1.Checked = settings.EnableRPM1Output;
            chkRPM2.Checked = settings.EnableRPM2Output;
            chkEPU.Checked = settings.EnableEPUFuelOutput;
            chkFuelFlow.Checked = settings.EnableFuelFlowOutput;
            chkISIS.Checked = settings.EnableISISOutput;
            chkFuelQty.Checked = settings.EnableFuelQuantityOutput;
            chkGearLights.Checked = settings.EnableGearLightsOutput;
            chkHSI.Checked = settings.EnableHSIOutput;
            chkEHSI.Checked = settings.EnableEHSIOutput;
            chkNWSIndexer.Checked = settings.EnableNWSIndexerOutput;
            chkPFL.Checked = settings.EnablePFLOutput;
            chkSpeedbrake.Checked = settings.EnableSpeedbrakeOutput;
            chkVVI.Checked = settings.EnableVVIOutput;
            chkHydA.Checked = settings.EnableHYDAOutput;
            chkHydB.Checked = settings.EnableHYDBOutput;
            chkCabinPress.Checked = settings.EnableCabinPressOutput;
            chkRollTrim.Checked = settings.EnableRollTrimOutput;
            chkPitchTrim.Checked = settings.EnablePitchTrimOutput;

            //pbRecoverAzimuthIndicator.Enabled = chkAzimuthIndicator.Checked;
            //pbRecoverADI.Enabled = chkADI.Checked;
            //pbRecoverStandbyADI.Enabled = chkStandbyADI.Checked;
            //pbRecoverASI.Enabled = chkAirspeedIndicator.Checked;
            //pbRecoverAltimeter.Enabled = chkAltimeter.Checked;
            //pbRecoverAOAIndexer.Enabled = chkAOAIndexer.Checked;
            //pbRecoverAOAIndicator.Enabled = chkAOAIndicator.Checked;
            //pbRecoverCautionPanel.Enabled = chkCautionPanel.Checked;
            //pbRecoverCMDSPanel.Enabled = chkCMDSPanel.Checked;
            //pbRecoverCompass.Enabled = chkCompass.Checked;
            //pbRecoverDED.Enabled = chkDED.Checked;
            //pbRecoverPFL.Enabled = chkPFL.Checked;
            //pbRecoverEPU.Enabled = chkEPU.Checked;
            //pbRecoverAccelerometer.Enabled = chkAccelerometer.Checked;
            //pbRecoverFTIT1.Enabled = chkFTIT1.Checked;
            //pbRecoverFTIT2.Enabled = chkFTIT2.Checked;
            //pbRecoverFuelFlow.Enabled = chkFuelFlow.Checked;
            //pbRecoverISIS.Enabled = chkISIS.Checked;
            //pbRecoverFuelQuantity.Enabled = chkFuelQty.Checked;
            //pbRecoverHSI.Enabled = chkHSI.Checked;
            //pbRecoverEHSI.Enabled = chkEHSI.Checked;
            //pbRecoverGearLights.Enabled = chkGearLights.Checked;
            //pbRecoverNWS.Enabled = chkNWSIndexer.Checked;
            //pbRecoverNozPos1.Enabled = chkNOZ1.Checked;
            //pbRecoverNozPos2.Enabled = chkNOZ2.Checked;
            //pbRecoverOil1.Enabled = chkOIL1.Checked;
            //pbRecoverOil2.Enabled = chkOIL2.Checked;
            //pbRecoverSpeedbrake.Enabled = chkSpeedbrake.Checked;
            //pbRecoverRPM1.Enabled = chkRPM1.Checked;
            //pbRecoverRPM2.Enabled = chkRPM2.Checked;
            //pbRecoverVVI.Enabled = chkVVI.Checked;
            //pbRecoverHydA.Enabled = chkHydA.Checked;
            //pbRecoverHydB.Enabled = chkHydB.Checked;
            //pbRecoverCabinPress.Enabled = chkCabinPress.Checked;
            //pbRecoverRollTrim.Enabled = chkRollTrim.Checked;
            //pbRecoverPitchTrim.Enabled = chkPitchTrim.Checked;


            string azimuthIndicatorType = settings.AzimuthIndicatorType;
            var azimuthIndicatorStyle =
                (F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle)
                Enum.Parse(typeof(F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle), azimuthIndicatorType);
            switch (azimuthIndicatorStyle)
            {
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.IP1310ALR:
                    if (settings.AzimuthIndicator_ShowBezel)
                    {
                        rdoAzimuthIndicatorStyleDigital.Checked = false;
                        rdoRWRHAFBezelType.Checked = false;
                        rdoATDPlus.Checked = false;
                        rdoAzimuthIndicatorNoBezel.Checked = false;
                        rdoRWRIP1310BezelType.Checked = true;
                        rdoAzimuthIndicatorStyleScope.Checked = true;
                    }
                    else
                    {
                        rdoAzimuthIndicatorStyleDigital.Checked = false;
                        rdoRWRIP1310BezelType.Checked = false;
                        rdoRWRHAFBezelType.Checked = false;
                        rdoATDPlus.Checked = false;
                        rdoAzimuthIndicatorNoBezel.Checked = true;
                        rdoAzimuthIndicatorStyleScope.Checked = true;
                    }
                    break;
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.HAF:
                    rdoAzimuthIndicatorStyleDigital.Checked = false;
                    rdoRWRIP1310BezelType.Checked = false;
                    rdoATDPlus.Checked = false;
                    rdoAzimuthIndicatorNoBezel.Checked = false;
                    rdoRWRHAFBezelType.Checked = true;
                    rdoAzimuthIndicatorStyleScope.Checked = true;
                    break;
                case F16AzimuthIndicator.F16AzimuthIndicatorOptions.InstrumentStyle.AdvancedThreatDisplay:
                    rdoAzimuthIndicatorStyleScope.Checked = false;
                    rdoRWRIP1310BezelType.Checked = false;
                    rdoRWRHAFBezelType.Checked = false;
                    rdoAzimuthIndicatorNoBezel.Checked = false;
                    rdoAzimuthIndicatorStyleDigital.Checked = true;
                    rdoATDPlus.Checked = true;
                    break;
                default:
                    break;
            }

            string altimeterStyleString = Settings.Default.Altimeter_Style;
            var altimeterStyle =
                (F16Altimeter.F16AltimeterOptions.F16AltimeterStyle)
                Enum.Parse(typeof(F16Altimeter.F16AltimeterOptions.F16AltimeterStyle), altimeterStyleString);
            switch (altimeterStyle)
            {
                case F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electromechanical:
                    rdoAltimeterStyleElectromechanical.Checked = true;
                    rdoAltimeterStyleDigital.Checked = false;
                    break;
                case F16Altimeter.F16AltimeterOptions.F16AltimeterStyle.Electronic:
                    rdoAltimeterStyleElectromechanical.Checked = false;
                    rdoAltimeterStyleDigital.Checked = true;
                    break;
                default:
                    break;
            }

            string pressureUnitsString = Settings.Default.Altimeter_PressureUnits;
            var pressureUnits =
                (F16Altimeter.F16AltimeterOptions.PressureUnits)
                Enum.Parse(typeof(F16Altimeter.F16AltimeterOptions.PressureUnits), pressureUnitsString);
            switch (pressureUnits)
            {
                case F16Altimeter.F16AltimeterOptions.PressureUnits.InchesOfMercury:
                    rdoInchesOfMercury.Checked = true;
                    rdoMillibars.Checked = false;
                    break;
                case F16Altimeter.F16AltimeterOptions.PressureUnits.Millibars:
                    rdoInchesOfMercury.Checked = false;
                    rdoMillibars.Checked = true;
                    break;
                default:
                    break;
            }

            string vviStyleString = Settings.Default.VVI_Style;
            var vviStyle = (VVIStyles)Enum.Parse(typeof(VVIStyles), vviStyleString);
            switch (vviStyle)
            {
                case VVIStyles.Tape:
                    rdoVVIStyleNeedle.Checked = false;
                    rdoVVIStyleTape.Checked = true;
                    break;
                case VVIStyles.Needle:
                    rdoVVIStyleNeedle.Checked = true;
                    rdoVVIStyleTape.Checked = false;
                    break;
                default:
                    break;
            }
            grpVVIOptions.Enabled = chkVVI.Checked;
            grpAltimeterStyle.Enabled = chkAltimeter.Checked;
            //grpPressureAltitudeSettings.Enabled = chkAltimeter.Checked;
            grpAzimuthIndicatorStyle.Enabled = chkAzimuthIndicator.Checked;

            rdoFuelQuantityNeedleCModel.Checked = Settings.Default.FuelQuantityIndicator_NeedleCModel;
            rdoFuelQuantityDModel.Checked = !Settings.Default.FuelQuantityIndicator_NeedleCModel;

            gbFuelQuantityOptions.Enabled = chkFuelQty.Checked;
            LoadGDIPlusSettings();
            chkHighlightOutputWindowsWhenContainMouseCursor.Checked = Settings.Default.HighlightOutputWindows;
            chkOnlyUpdateImagesWhenDataChanges.Checked = Settings.Default.RenderInstrumentsOnlyOnStatechanges;
        }

    }
}
