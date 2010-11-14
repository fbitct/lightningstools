using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Common.SimSupport
{
    public interface IInstrumentRenderer
    {
        void Render(Graphics g, Rectangle bounds);
    }
}
