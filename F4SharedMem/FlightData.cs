using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using F4SharedMem.Headers;

namespace F4SharedMem
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class FlightData
    {
        public int AUXTChan;
        public float AdiIlsHorPos; // Position of horizontal ILS bar
        public float AdiIlsVerPos; // Position of vertical ILS bar
        public float ChaffCount; // Number of Chaff left
        public string[] DEDLines = new string[5]; //25 usable chars
        public FalconDataFormats DataFormat;
        public object ExtensionData;
        public float FlareCount; // Number of Flare left
        public string[] Invert = new string[5]; //25 usable chars
        public float LeftGearPos; // Position of the left landinggear; caution: full down values defined in dat files
        public int MainPower;
        public float NoseGearPos; // Position of the nose landinggear; caution: full down values defined in dat files
        public string[] PFLInvert = new string[5]; //25 usable chars
        public string[] PFLLines = new string[5]; //25 usable chars
        public int[] RWRsymbol = new int[40];
        public float RightGearPos; // Position of the right landinggear; caution: full down values defined in dat files
        public int RwrObjectCount;
        public float TrimPitch; // Value of trim in pitch axis, -0.5 to +0.5
        public float TrimRoll; // Value of trim in roll axis, -0.5 to +0.5
        public float TrimYaw; // Value of trim in yaw axis, -0.5 to +0.5
        public int UFCTChan;
        public int VersionNum; //Version of Mem area
        public float aauz; //AAU altimeter indicated altitude (new in BMS4)
        public float aft;
        public float airbaseX; // HSI_VAL_AIRBASE_X
        public float airbaseY; // HSI_VAL_AIRBASE_Y
        public float alpha; // Ownship AOA (Degrees)
        public float[] bearing = new float[40];
        public float bearingToBeacon; // HSI_VAL_BEARING_TO_BEACON
        public float beta; // Ownship Beta (Degrees)
        public float courseDeviation; // HSI_VAL_CRS_DEVIATION
        public int courseState; // HSI_STA_CRS_STATE
        public float currentHeading; // HSI_VAL_CURRENT_HEADING
        public float desiredCourse; // HSI_VAL_DESIRED_CRS
        public float desiredHeading; // HSI_VAL_DESIRED_HEADING
        public float deviationLimit; // HSI_VAL_DEV_LIMIT
        public float distanceToBeacon; // HSI_VAL_DISTANCE_TO_BEACON
        public float epuFuel; // Ownship EPU fuel (Percent 0-100)
        public float externalFuel; // Ownship external fuel (Lbs)
        public float ftit; // Ownship Forward Turbine Inlet Temp (Degrees C)
        public float ftit2; // Ownship Forward Turbine Inlet Temp2 (Degrees C)
        public float fuelFlow; // Ownship fuel flow (Lbs/Hour)
        public float fwd;
        public float gamma; // Ownship Gamma (Radians)
        public float gearPos; // Ownship Gear position 0 = up, 1 = down;
        public float gs; // Ownship Normal Gs
        public float halfDeviationLimit; // HSI_VAL_HALF_DEV_LIMIT

        // These are inputs. Use them carefully
        // NB: these do not work when TrackIR device is enabled
        public float headPitch; // Head pitch offset from design eye (radians)
        public float headRoll; // Head roll offset from design eye (radians)
        public float headX; // Head X offset from design eye (feet)
        public float headY; // Head Y offset from design eye (feet)
        public float headYaw; // Head yaw offset from design eye (radians)
        public float headZ; // Head Z offset from design eye (feet)
        public int headingState; // HSI_STA_HDG_STATE
        public int hsiBits; // HSI flags
        public float internalFuel; // Ownship internal fuel (Lbs)
        public float kias; // Ownship Indicated Airspeed (Knots)
        public OptionSelectButtonLabel[] leftMFD = new OptionSelectButtonLabel[20];
        public float[] lethality = new float[40];
        public int lightBits; // Cockpit Indicator Lights, one bit per bulb. See enum

        // new lights
        public int lightBits2; // Cockpit Indicator Lights, one bit per bulb. See enum
        public int lightBits3; // Cockpit Indicator Lights, one bit per bulb. See enum

        // chaff/flare
        public float localizerCourse; // HSI_VAL_LOCALIZER_CRS
        public float mach; // Ownship Mach number
        public int[] missileActivity = new int[40];
        public int[] missileLaunch = new int[40];
        public byte navMode; //HSI nav mode (new in BMS4)
        public int[] newDetection = new int[40];
        public float nozzlePos; // Ownship engine nozzle percent open (0-100)
        public float nozzlePos2; // Ownship engine nozzle2 percent open (0-100)
        public float oilPressure; // Ownship Oil Pressure (Percent 0-100)
        public float oilPressure2; // Ownship Oil Pressure2 (Percent 0-100)
        public float pitch; // Ownship Pitch (Radians)
        public OptionSelectButtonLabel[] rightMFD = new OptionSelectButtonLabel[20];
        public float roll; // Ownship Roll (Radians)
        public float rpm; // Ownship engine rpm (Percent 0-103)
        public float rpm2; // Ownship engine rpm2 (Percent 0-103)
        public int[] selected = new int[40];
        public float speedBrake; // Ownship speed brake position 0 = closed, 1 = 60 Degrees open
        public byte[] tacanInfo = new byte[2]; //TACAN info (new in BMS4)
        public float total;
        public int totalStates; // HSI_STA_TOTAL_STATES; never set
        public float totalValues; // HSI_VAL_TOTAL_VALUES; never set
        public float vt; // Ownship True Airspeed (Ft/Sec)
        public float windOffset; // Wind delta to FPM (Radians)
        public float x; // Ownship North (Ft)
        public float xDot; // Ownship North Rate (ft/sec)
        public float y; // Ownship East (Ft)
        public float yDot; // Ownship East Rate (ft/sec)
        public float yaw; // Ownship Yaw (Radians)
        public float z; // Ownship Down (Ft)
        public float zDot; // Ownship Down Rate (ft/sec)

        public FlightData()
        {
        }

        internal FlightData(AFFlightData data)
        {
            PopulateFromStruct(data);
            DataFormat = FalconDataFormats.AlliedForce;
        }

        internal FlightData(BMS4FlightData data)
        {
            PopulateFromStruct(data);
            DataFormat = FalconDataFormats.BMS4;
        }

        internal FlightData(BMS3FlightData data)
        {
            PopulateFromStruct(data);
            DataFormat = FalconDataFormats.BMS3;
        }

        internal FlightData(BMS2FlightData data)
        {
            PopulateFromStruct(data);
            DataFormat = FalconDataFormats.BMS2;
        }

        internal FlightData(FreeFalcon5FlightData data)
        {
            PopulateFromStruct(data);
            DataFormat = FalconDataFormats.FreeFalcon5;
        }

        public bool UfcTacanIsAA
        {
            get
            {
                return (tacanInfo != null && ((tacanInfo[(int) TacanSources.UFC] & (byte) TacanBits.mode) != 0)
                            ? true
                            : false);
            }
        }

        public bool AuxTacanIsAA
        {
            get
            {
                return (tacanInfo != null && ((tacanInfo[(int) TacanSources.AUX] & (byte) TacanBits.mode) != 0)
                            ? true
                            : false);
            }
        }

        public bool UfcTacanIsX
        {
            get
            {
                return (tacanInfo != null && ((tacanInfo[(int) TacanSources.UFC] & (byte) TacanBits.band) != 0)
                            ? true
                            : false);
            }
        }

        public bool AuxTacanIsX
        {
            get
            {
                return (tacanInfo != null && ((tacanInfo[(int) TacanSources.AUX] & (byte) TacanBits.band) != 0)
                            ? true
                            : false);
            }
        }

        internal void PopulateFromStruct(object data)
        {
            Type thisType = GetType();
            Type dataType = data.GetType();
            FieldInfo[] fields = dataType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo currentField = fields[i];
                FieldInfo thisField = thisType.GetField(currentField.Name);
                if (thisField == null) continue;
                Type currentFieldType = currentField.FieldType;
                if (currentFieldType.IsArray)
                {
                    if (currentFieldType == typeof (DED_PFL_LineOfText[]))
                    {
                        var currentValue = (DED_PFL_LineOfText[]) currentField.GetValue(data);
                        var valuesToAssign = new string[currentValue.Length];
                        for (int j = 0; j < currentValue.Length; j++)
                        {
                            DED_PFL_LineOfText currentItem = currentValue[j];
                            var sb = new StringBuilder(currentItem.chars.Length);
                            bool invert = false;
                            if (currentField.Name.ToLowerInvariant().Contains("invert"))
                            {
                                invert = true; //this is an inversion line
                            }
                            for (int k = 0; k < currentItem.chars.Length; k++)
                            {
                                sbyte chr = currentItem.chars[k];
                                if (invert)
                                {
                                    if (chr == 0x02)
                                    {
                                        sb.Append((char) chr);
                                    }
                                    else
                                    {
                                        sb.Append(' ');
                                    }
                                }
                                else
                                {
                                    if (chr != 0)
                                    {
                                        sb.Append((char) chr);
                                    }
                                }
                            }
                            valuesToAssign[j] = sb.ToString();
                        }
                        thisField.SetValue(this, valuesToAssign);
                    }
                    else if (currentFieldType == typeof (OSBLabel[]))
                    {
                        var currentValue = (OSBLabel[]) currentField.GetValue(data);
                        var valuesToAssign = new OptionSelectButtonLabel[currentValue.Length];
                        for (int j = 0; j < currentValue.Length; j++)
                        {
                            OSBLabel currentItem = currentValue[j];
                            var label = new OptionSelectButtonLabel();
                            var lineBuilder = new StringBuilder(currentItem.Line1.Length);

                            foreach (sbyte chr in currentItem.Line1)
                            {
                                if (chr == 0)
                                {
                                    lineBuilder.Append(" ");
                                }
                                else
                                {
                                    lineBuilder.Append((char) chr);
                                }
                            }
                            label.Line1 = lineBuilder.ToString();
                            lineBuilder = new StringBuilder(currentItem.Line2.Length);
                            foreach (sbyte chr in currentItem.Line2)
                            {
                                if (chr == 0)
                                {
                                    lineBuilder.Append(" ");
                                }
                                else
                                {
                                    lineBuilder.Append((char) chr);
                                }
                            }
                            label.Inverted = currentItem.Inverted;
                            valuesToAssign[j] = label;
                        }
                        thisField.SetValue(this, valuesToAssign);
                    }
                    else
                    {
                        var currentValue = (Array) currentField.GetValue(data);
                        thisField.SetValue(this, currentValue);
                    }
                }
                else if (currentFieldType.Name.ToLowerInvariant().Contains("uint"))
                {
                    thisField.SetValue(this,
                                       BitConverter.ToInt32(BitConverter.GetBytes((uint) currentField.GetValue(data)), 0));
                }
                else
                {
                    thisField.SetValue(this, currentField.GetValue(data));
                }
            }
        }

        #region Nested type: OptionSelectButtonLabel

        [ComVisible(true)]
        [Serializable]
        public struct OptionSelectButtonLabel
        {
            public bool Inverted;
            public string Line1;
            public string Line2;
        }

        #endregion
    }
}