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
            var blipsToWrite = Blips.Where(x => (int)x.Symbol < 31);
            if (blipsToWrite.Count() == 0)
            {
                return null;
            }
            var toReturn = new byte[(3 * blipsToWrite.Count()) + 1];
            toReturn[0] = (byte)blipsToWrite.Count();
            for (var i = 0; i < blipsToWrite.Count(); i++)
            {
                var thisSymbol = blipsToWrite.ElementAt(i);
                toReturn[(i * 3)+1] = thisSymbol.X;
                toReturn[(i * 3) + 2] = thisSymbol.Y;
                toReturn[(i * 3) + 3] = (byte)thisSymbol.Symbol;
            }
            return toReturn;
        }
    }
}
