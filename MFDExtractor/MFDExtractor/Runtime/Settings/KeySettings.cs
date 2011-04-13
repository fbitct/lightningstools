﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.InputSupport.UI;
using Common.InputSupport;
using System.Windows.Forms;

namespace MFDExtractor.Runtime.Settings
{
    internal class KeySettings
    {
        private bool _keySettingsLoaded = false;
        #region Public Properties
        public InputControlSelection NVISKey {get;set;}
        public InputControlSelection AirspeedIndexIncreaseKey { get; set; }
        public InputControlSelection AirspeedIndexDecreaseKey { get; set; }
        public InputControlSelection EHSIMenuButtonDepressedKey { get; set; }
        public InputControlSelection EHSIHeadingIncreaseKey { get; set; }
        public InputControlSelection EHSIHeadingDecreaseKey { get; set; }
        public InputControlSelection EHSICourseIncreaseKey { get; set; }
        public InputControlSelection EHSICourseDecreaseKey { get; set; }
        public InputControlSelection EHSICourseDepressedKey { get; set; }
        public InputControlSelection ISISBrightButtonKey { get; set; }
        public InputControlSelection ISISStandardButtonKey { get; set; }
        public InputControlSelection AzimuthIndicatorBrightnessIncreaseKey { get; set; }
        public InputControlSelection AzimuthIndicatorBrightnessDecreaseKey { get; set; }
        public InputControlSelection AccelerometerResetKey { get; set; }
        #endregion

        public KeySettings():base()
        {
            LoadKeySettings();
        }
        public void Reload()
        {
            LoadKeySettings();
        }
        private void LoadKeySettings()
        {
            NVISKey = LoadKeySetting(()=>Properties.Settings.Default.NVISKey );
            AzimuthIndicatorBrightnessDecreaseKey = LoadKeySetting(() => Properties.Settings.Default.AzimuthIndicatorBrightnessDecreaseKey);
            AzimuthIndicatorBrightnessIncreaseKey = LoadKeySetting(() => Properties.Settings.Default.AzimuthIndicatorBrightnessIncreaseKey);
            AirspeedIndexIncreaseKey = LoadKeySetting(() => Properties.Settings.Default.AirspeedIndexIncreaseKey);
            AirspeedIndexDecreaseKey = LoadKeySetting(() => Properties.Settings.Default.AirspeedIndexDecreaseKey);
            EHSIHeadingIncreaseKey = LoadKeySetting(() => Properties.Settings.Default.EHSIHeadingIncreaseKey);
            EHSIHeadingDecreaseKey = LoadKeySetting(() => Properties.Settings.Default.EHSIHeadingDecreaseKey);
            EHSICourseIncreaseKey= LoadKeySetting(() => Properties.Settings.Default.EHSICourseIncreaseKey);
            EHSICourseDecreaseKey = LoadKeySetting(() => Properties.Settings.Default.EHSICourseDecreaseKey);
            EHSICourseDepressedKey = LoadKeySetting(() => Properties.Settings.Default.EHSICourseKnobDepressedKey);
            EHSIMenuButtonDepressedKey = LoadKeySetting(() => Properties.Settings.Default.EHSIMenuButtonKey);
            ISISBrightButtonKey = LoadKeySetting(() => Properties.Settings.Default.ISISBrightButtonKey);
            ISISStandardButtonKey = LoadKeySetting(() => Properties.Settings.Default.ISISStandardButtonKey);
            AzimuthIndicatorBrightnessIncreaseKey = LoadKeySetting(() => Properties.Settings.Default.AzimuthIndicatorBrightnessIncreaseKey);
            AzimuthIndicatorBrightnessDecreaseKey = LoadKeySetting(() => Properties.Settings.Default.AzimuthIndicatorBrightnessDecreaseKey);
            AccelerometerResetKey = LoadKeySetting(() => Properties.Settings.Default.AccelerometerResetKey);
        }
        private InputControlSelection LoadKeySetting(Func<string> SerializedControlSelection )
        {
            string keyFromSettingsString = SerializedControlSelection();
            InputControlSelection toReturn = null;

            if (!string.IsNullOrEmpty(keyFromSettingsString))
            {
                try
                {
                    toReturn = (InputControlSelection)Common.Serialization.Util.DeserializeFromXml(keyFromSettingsString, typeof(InputControlSelection));
                }
                catch (Exception e)
                {
                }
            }

            if (toReturn == null)
            {
                toReturn = new InputControlSelection() { ControlType = ControlType.Unknown, Keys = Keys.None };
            }

            return toReturn;
        }

    }
}
