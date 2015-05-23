using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal abstract class RWRCommand
    {
        public abstract byte[] ToBytes();
    }
}
