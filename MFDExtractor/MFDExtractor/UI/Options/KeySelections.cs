using System;
using Common.InputSupport.UI;
using Common.InputSupport.DirectInput;
using Common.Generic;
using System.Windows.Forms;
using MFDExtractor.Properties;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private static InputControlSelection DeserializeInputControlSelection(PropertyInvoker<string> settingsProperty)
        {
            InputControlSelection keyFromSettings = null;
            try
            {
                keyFromSettings =
                    (InputControlSelection)
                    Common.Serialization.Util.DeserializeFromXml(settingsProperty.GetProperty(),
                                                                 typeof(InputControlSelection));
            }
            catch (Exception)
            {
            }
            return keyFromSettings;
        }
        private static void ShowKeySelectionDialog(Mediator mediator, PropertyInvoker<string> settingsProperty,
                                                  Form parentForm)
        {
            var toShow = new InputSourceSelector();
            toShow.Mediator = mediator;
            var keyFromSettings = DeserializeInputControlSelection(settingsProperty);
            if (keyFromSettings != null)
            {
                toShow.SelectedControl = keyFromSettings;
            }
            toShow.ShowDialog(parentForm);
            var selection = toShow.SelectedControl;
            if (selection != null)
            {
                string serialized = Common.Serialization.Util.SerializeToXml(selection, typeof(InputControlSelection));
                settingsProperty.SetProperty(serialized);
            }
        }
        private void cmdAirspeedIndexIncreaseHotkey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("AirspeedIndexIncreaseKey", Settings.Default), this);
        }

        private void cmdAirspeedIndexDecreaseHotkey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("AirspeedIndexDecreaseKey", Settings.Default), this);
        }

        private void cmdEHSIMenuButtonHotkey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSIMenuButtonKey", Settings.Default), this);
        }

        private void cmdEHSIHeadingIncreaseKey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSIHeadingIncreaseKey", Settings.Default), this);
        }

        private void cmdEHSIHeadingDecreaseKey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSIHeadingDecreaseKey", Settings.Default), this);
        }

        private void cmdEHSICourseIncreaseKey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSICourseIncreaseKey", Settings.Default), this);
        }

        private void cmdEHSICourseDecreaseKey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSICourseDecreaseKey", Settings.Default), this);
        }

        private void cmdEHSICourseKnobDepressedKey_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("EHSICourseKnobDepressedKey", Settings.Default), this);
        }

        private void cmdAzimuthIndicatorBrightnessIncrease_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("AzimuthIndicatorBrightnessIncreaseKey", Settings.Default),
                                   this);
        }

        private void cmdAzimuthIndicatorBrightnessDecrease_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("AzimuthIndicatorBrightnessDecreaseKey", Settings.Default),
                                   this);
        }

        private void cmdISISBrightButtonPressed_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("ISISBrightButtonKey", Settings.Default), this);
        }

        private void cmdISISStandardBrightnessButtonPressed_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("ISISStandardButtonKey", Settings.Default), this);
        }

        private void cmdAccelerometerResetButtonPressed_Click(object sender, EventArgs e)
        {
            ShowKeySelectionDialog(Extractor.GetInstance().Mediator,
                                   new PropertyInvoker<string>("AccelerometerResetKey", Settings.Default), this);
        }
    }
}
