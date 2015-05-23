using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal struct Blip
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public Symbols Symbol { get; set; }
        public bool Blink { get; set; }
    }
}
