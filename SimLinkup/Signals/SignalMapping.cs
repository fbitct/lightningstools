using System;
using Common.MacroProgramming;
using SimLinkup.Scripting;

namespace SimLinkup.Signals
{
    [Serializable]
    public class SignalMapping
    {
        public SignalMappingType MappingType;
        public Signal Source { get; set; }
        public Signal Destination { get; set; }
        public Script Script { get; set; }
    }
}