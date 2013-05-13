using System;
using System.Drawing;

namespace Common.SimSupport
{
    public interface IInstrumentRenderer:IDisposable
    {
        void Render(Graphics g, Rectangle bounds);
        InstrumentStateBase GetState();
    }
}