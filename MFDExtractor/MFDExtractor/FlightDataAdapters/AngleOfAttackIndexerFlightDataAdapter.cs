using F4SharedMem;
using F4SharedMem.Headers;
using LightningGauges.Renderers;

namespace MFDExtractor.FlightDataAdapters
{
    internal interface IAngleOfAttackIndexerFlightDataAdapter
    {
        void Adapt(IF16AngleOfAttackIndexer angleOfAttackIndexer, FlightData flightData);
    }

    class AngleOfAttackIndexerFlightDataAdapter : IAngleOfAttackIndexerFlightDataAdapter
    {
        public void Adapt(IF16AngleOfAttackIndexer angleOfAttackIndexer, FlightData flightData)
        {
            angleOfAttackIndexer.InstrumentState.AoaBelow = ((flightData.lightBits & (int)LightBits.AOABelow) == (int)LightBits.AOABelow);
            angleOfAttackIndexer.InstrumentState.AoaOn = ((flightData.lightBits & (int)LightBits.AOAOn) == (int)LightBits.AOAOn);
            angleOfAttackIndexer.InstrumentState.AoaAbove = ((flightData.lightBits & (int)LightBits.AOAAbove) == (int)LightBits.AOAAbove);
        }
    }
}
