using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal class DrawBlipsCommand : RWRCommand
    {
        public DrawBlipsCommand()
        {
            Blips = new List<Blip>();
        }
        public IEnumerable<Blip> Blips { get; set; }
        public override byte[] ToBytes()
        {
            var toReturn = new byte[3 * Blips.Count()];
            for (var i = 0; i < Blips.Count(); i++)
            {
                var thisSymbol = Blips.ElementAt(i);
                toReturn[(i * 3)] = thisSymbol.X;
                toReturn[(i * 3) + 1] = thisSymbol.Y;
                toReturn[(i * 3) + 2] = (byte)thisSymbol.Symbol;
            }
            return toReturn;
        }
    }
}
