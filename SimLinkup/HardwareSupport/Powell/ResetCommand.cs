using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal class ResetCommand : RWRCommand
    {
        public override byte[] ToBytes()
        {
            return new byte[] { 0x00 };
        }
    }
}
