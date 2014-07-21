using System;
using System.Drawing;

namespace Common.SimSupport
{
    public interface IInstrumentRenderer:IDisposable
    {
        void Render(Graphics destinationGraphics, Rectangle destinationRectangle);
        InstrumentStateBase GetState();
    }
}