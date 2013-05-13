using Common.SimSupport;

namespace LightningGauges.Renderers
{
    public interface IF16VerticalVelocityIndicator : IInstrumentRenderer
    {
        F16VerticalVelocityIndicatorInstrumentState InstrumentState { get; set; }
    }
}
