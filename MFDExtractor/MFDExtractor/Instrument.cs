using Common.SimSupport;
using MFDExtractor.UI;
using System.Threading;

namespace MFDExtractor
{
    class Instrument
    {
        InstrumentForm Form { get; set; }
        IInstrumentRenderer Renderer { get; set; }
        AutoResetEvent StartSignal { get; set; }
        AutoResetEvent EndSignal { get; set; }
        Thread RenderThread { get; set; }
        IRenderThreadWorkHelper RenderThreadWorkHelper { get; set; }
    }
}
