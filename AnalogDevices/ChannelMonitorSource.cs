using System;

namespace AnalogDevices
{
    [Serializable]
    public enum ChannelMonitorSource
    {
        None = 0,
        InputPin,
        DacChannel
    }
}