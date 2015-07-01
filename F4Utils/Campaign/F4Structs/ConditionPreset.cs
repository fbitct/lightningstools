using System;
using System.IO;
using System.Runtime.InteropServices;
namespace F4Utils.Campaign.F4Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ConditionPreset
    {
        public ConditionPreset() { }
        public ConditionPreset(Stream stream): this()
        {
            using (var reader = new BinaryReader(stream))
            {
                nMinTemperature = reader.ReadInt32();
                nMidTemperature = reader.ReadInt32();
                nMaxTemperature = reader.ReadInt32();
                nMinPressure = reader.ReadInt32();
                nMidPressure = reader.ReadInt32();
                nMaxPressure = reader.ReadInt32();
                nMinWindSpeed = reader.ReadInt32();
                nMidWindSpeed = reader.ReadInt32();
                nMaxWindSpeed = reader.ReadInt32();
                nThreshWindSpeed = reader.ReadInt32();
                nStratusBase = reader.ReadInt32();
                nCumulusBase = reader.ReadInt32();
                nStratusThick = reader.ReadInt32();
                dwContrailBase = reader.ReadInt32();
                nlastTOD = reader.ReadInt32();

                fFogStartBelowLayer = reader.ReadSingle();
                fFogStartAboveLayer = reader.ReadSingle();

                fFogEndBelowLayer = reader.ReadSingle();
                fFogEndAboveLayer = reader.ReadSingle();
                fHorizonBlend = reader.ReadSingle();
                fSkyFogLevel = reader.ReadSingle();
                fSkyFogStartElev = reader.ReadSingle();
                fSkyFogEndElev = reader.ReadSingle();
                fEarthColor = new color(stream);


                fStratusSunnyFactor = reader.ReadSingle();
                fStratusFairFactor = reader.ReadSingle();
                fStratusPoorFactor = reader.ReadSingle();
                fStratusInclementFactor = reader.ReadSingle();
                fHazeMin = reader.ReadSingle();

                fCumulusZ = reader.ReadSingle();
                fStratusZ = reader.ReadSingle();
                fContrailLow = reader.ReadSingle();
                fContrailHigh = reader.ReadSingle();
                fTemperature = reader.ReadSingle();
            }
        }

        // Interpolated
        public int nMinTemperature = 0;
        public int nMidTemperature = 0;
        public int nMaxTemperature = 0;
        public int nMinPressure = 0;
        public int nMidPressure = 0;
        public int nMaxPressure = 0;
        public int nMinWindSpeed = 0;
        public int nMidWindSpeed = 0;
        public int nMaxWindSpeed = 0;
        public int nThreshWindSpeed = 0;
        public int nStratusBase = 0;
        public int nCumulusBase = 0;
        public int nStratusThick = 0;
        public int dwContrailBase = 0;
        public int nlastTOD = 0;

        public float fFogStartBelowLayer = 0f;
        public float fFogStartAboveLayer = 0f;
        public float fFogEndBelowLayer = 0f;
        public float fFogEndAboveLayer = 0f;
        public float fHorizonBlend = 0f;
        public float fSkyFogLevel = 0f;
        public float fSkyFogStartElev = 0f;
        public float fSkyFogEndElev = 0f;
        public color fEarthColor = color.Empty;
        public float fStratusSunnyFactor = 0f;
        public float fStratusFairFactor = 0f;
        public float fStratusPoorFactor = 0f;
        public float fStratusInclementFactor = 0f;
        public float fHazeMin = 0f;


        // Computed based on interpolated values in two-min updates
        public float fCumulusZ = 0f;
        public float fStratusZ = 0f;
        public float fContrailLow = 0f;
        public float fContrailHigh = 0f;
        public float fTemperature = 0f;
    };
}
