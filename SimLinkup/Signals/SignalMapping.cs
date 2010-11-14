using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;
using SimLinkup.Scripting;

namespace SimLinkup.Signals
{
    [Serializable]
    public class SignalMapping
    {
        public Signal Source { get; set; }
        public Signal Destination { get; set; }
        public SignalMappingType MappingType;
        public Script Script { get; set; }

    }
}
