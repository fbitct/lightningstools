using System;
using Common.MacroProgramming;

namespace SimLinkup.Signals
{
    [Serializable]
    public class SignalMapping
    {
        public SignalMappingType MappingType;
        public Signal Source { get; set; }
        public Signal Destination { get; set; }
    }
}