using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal class DrawSymbolsCommand : RWRCommand
    {
        public DrawSymbolsCommand()
        {
            Symbols = new List<RWRSymbol>();
        }
        public IEnumerable<RWRSymbol> Symbols { get; set; }
        public override byte[] ToBytes()
        {
            var toReturn = new byte[3 * Symbols.Count()];
            for (var i = 0; i < Symbols.Count(); i++)
            {
                var thisSymbol = Symbols.ElementAt(i);
                toReturn[(i * 3)] = thisSymbol.XPosition;
                toReturn[(i * 3) + 1] = thisSymbol.YPosition;
                toReturn[(i * 3) + 2] = thisSymbol.SymbolNumber;
            }
            return toReturn;
        }
    }
}
