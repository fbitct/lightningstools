using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal struct FalconRWRSymbol
    {
        public int SymbolID { get; set; }
        public double Bearing { get; set; }
        public double Lethality { get; set; }
        public bool MissileActivity { get; set; }
        public bool MissileLaunch { get; set; }
        public bool Selected { get; set; }
        public bool NewDetection { get; set; }
        
    }
}
