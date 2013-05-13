using Common.SimSupport;

namespace MFDExtractor
{
    public interface IInstrumentRendererSet
    {
        IInstrumentRenderer _accelerometerRenderer { get; set; }
        IInstrumentRenderer _adiRenderer { get; set; }
         IInstrumentRenderer _altimeterRenderer { get; set; }
         IInstrumentRenderer _aoaIndexerRenderer { get; set; }
         IInstrumentRenderer _aoaIndicatorRenderer { get; set; }
         IInstrumentRenderer _asiRenderer { get; set; }
         IInstrumentRenderer _backupAdiRenderer { get; set; }
         IInstrumentRenderer _cabinPressRenderer { get; set; }
         IInstrumentRenderer _cautionPanelRenderer { get; set; }
         IInstrumentRenderer _cmdsPanelRenderer { get; set; }
         IInstrumentRenderer _compassRenderer{ get; set; }
         IInstrumentRenderer _dedRenderer{ get; set; }
         IInstrumentRenderer _ehsiRenderer{ get; set; }
         IInstrumentRenderer _epuFuelRenderer{ get; set; }
         IInstrumentRenderer _ftit1Renderer{ get; set; }
         IInstrumentRenderer _ftit2Renderer{ get; set; }
         IInstrumentRenderer _fuelFlowRenderer{ get; set; }
         IInstrumentRenderer _fuelQuantityRenderer{ get; set; }
         IInstrumentRenderer _hsiRenderer{ get; set; }
         IInstrumentRenderer _hydARenderer{ get; set; }
         IInstrumentRenderer _hydBRenderer{ get; set; }
         IInstrumentRenderer _isisRenderer{ get; set; }
         IInstrumentRenderer _landingGearLightsRenderer{ get; set; }
         IInstrumentRenderer _nozPos1Renderer{ get; set; }
         IInstrumentRenderer _nozPos2Renderer{ get; set; }
         IInstrumentRenderer _nwsIndexerRenderer{ get; set; }
         IInstrumentRenderer _oilGauge1Renderer{ get; set; }
         IInstrumentRenderer _oilGauge2Renderer{ get; set; }
         IInstrumentRenderer _pflRenderer{ get; set; }
         IInstrumentRenderer _pitchTrimRenderer{ get; set; }
         IInstrumentRenderer _rollTrimRenderer{ get; set; }
         IInstrumentRenderer _rpm1Renderer{ get; set; }
         IInstrumentRenderer _rpm2Renderer{ get; set; }
         IInstrumentRenderer _rwrRenderer{ get; set; }
         IInstrumentRenderer _speedbrakeRenderer{ get; set; }
         IInstrumentRenderer _vviRenderer{ get; set; }

    }

    public class InstrumentRendererSet : IInstrumentRendererSet
    {
        public IInstrumentRenderer _accelerometerRenderer { get; set; }
        public IInstrumentRenderer _adiRenderer { get; set; }
        public IInstrumentRenderer _altimeterRenderer { get; set; }
        public IInstrumentRenderer _aoaIndexerRenderer { get; set; }
        public IInstrumentRenderer _aoaIndicatorRenderer { get; set; }
        public IInstrumentRenderer _asiRenderer { get; set; }
        public IInstrumentRenderer _backupAdiRenderer { get; set; }
        public IInstrumentRenderer _cabinPressRenderer { get; set; }
        public IInstrumentRenderer _cautionPanelRenderer { get; set; }
        public IInstrumentRenderer _cmdsPanelRenderer { get; set; }
        public IInstrumentRenderer _compassRenderer { get; set; }
        public IInstrumentRenderer _dedRenderer { get; set; }
        public IInstrumentRenderer _ehsiRenderer { get; set; }
        public IInstrumentRenderer _epuFuelRenderer { get; set; }
        public IInstrumentRenderer _ftit1Renderer { get; set; }
        public IInstrumentRenderer _ftit2Renderer { get; set; }
        public IInstrumentRenderer _fuelFlowRenderer { get; set; }
        public IInstrumentRenderer _fuelQuantityRenderer { get; set; }
        public IInstrumentRenderer _hsiRenderer { get; set; }
        public IInstrumentRenderer _hydARenderer { get; set; }
        public IInstrumentRenderer _hydBRenderer { get; set; }
        public IInstrumentRenderer _isisRenderer { get; set; }
        public IInstrumentRenderer _landingGearLightsRenderer { get; set; }
        public IInstrumentRenderer _nozPos1Renderer { get; set; }
        public IInstrumentRenderer _nozPos2Renderer { get; set; }
        public IInstrumentRenderer _nwsIndexerRenderer { get; set; }
        public IInstrumentRenderer _oilGauge1Renderer { get; set; }
        public IInstrumentRenderer _oilGauge2Renderer { get; set; }
        public IInstrumentRenderer _pflRenderer { get; set; }
        public IInstrumentRenderer _pitchTrimRenderer { get; set; }
        public IInstrumentRenderer _rollTrimRenderer { get; set; }
        public IInstrumentRenderer _rpm1Renderer { get; set; }
        public IInstrumentRenderer _rpm2Renderer { get; set; }
        public IInstrumentRenderer _rwrRenderer { get; set; }
        public IInstrumentRenderer _speedbrakeRenderer { get; set; }
        public IInstrumentRenderer _vviRenderer { get; set; }


    }
}
