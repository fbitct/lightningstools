using SDI;
using System;
using System.Xml.Serialization;

namespace SimLinkup.HardwareSupport.Henk.SDI
{
    [Serializable]
    [XmlRoot("HenkSDI")]
    public class HenkSDIHardwareSupportModuleConfig
    {
        [XmlArray("Devices")]
        [XmlArrayItem("Device")]
        public DeviceConfig[] Devices { get; set; }

        public static HenkSDIHardwareSupportModuleConfig Load(string filePath)
        {
            return
                Common.Serialization.Util.DeserializeFromXmlFile<HenkSDIHardwareSupportModuleConfig>(filePath);
        }

        public void Save(string filePath)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, filePath);
        }
    }

    [Serializable]
    public class DeviceConfig
    {
        public ConnectionType? ConnectionType { get; set; }
        public string COMPort { get; set; }
        public string DOAAddress { get; set; }
        public PowerDownConfig PowerDown { get; set; }
        public StatorBaseAnglesConfig StatorBaseAngles { get; set; }
        public MovementLimitsConfig MovementLimits { get; set; }
        public OutputChannelConfig OutputChannels{ get; set; }
        public UpdateRateControlConfig UpdateRateControl{ get; set; }
        public DiagnosticLEDMode? DiagnosticLEDMode { get; set; }

    }

    [Serializable]
    public class PowerDownConfig
    {
        public bool? Enabled { get; set; }
        public PowerDownLevel? Level { get; set; }
        public short? DelayTimeMilliseconds { get; set; }
    }

    [Serializable]
    public class StatorBaseAnglesConfig
    {
        public short? S1 { get; set; }
        public short? S2 { get; set; }
        public short? S3 { get; set; }
    }

    [Serializable]
    public class MovementLimitsConfig
    {
        public byte? Min { get; set; }
        public byte? Max { get; set; }
    }

    [Serializable]
    public class OutputChannelConfig
    {
        public OutputChannelMode? Channel1 { get; set; }
        public OutputChannelMode? Channel2 { get; set; }
        public OutputChannelMode? Channel3 { get; set; }
        public OutputChannelMode? Channel4 { get; set; }
        public OutputChannelMode? Channel5 { get; set; }
        public OutputChannelMode? Channel6 { get; set; }
        public OutputChannelMode? Channel7 { get; set; }
    }

    [Serializable]
    public class UpdateRateControlConfig
    {
        public UpdateRateControlModes? Mode { get; set; }
        public byte? LimitThreshold { get; set; }
        public byte? SmoothingMinimumThreshold { get; set; }
        public UpdateRateControlSmoothingMode? SmoothingMode { get; set; }
        public short? StepUpdateDelayMillis { get; set; }
        public bool? UseShortestPath { get; set; }
    }
}