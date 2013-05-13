using Common.SimSupport;

namespace MFDExtractor
{
    public interface IInstrumentRendererSet
    {
         IInstrumentRenderer Accelerometer { get; set; }
         IInstrumentRenderer ADI { get; set; }
         IInstrumentRenderer Altimeter { get; set; }
         IInstrumentRenderer AOAIndexer { get; set; }
         IInstrumentRenderer AOAIndicator { get; set; }
         IInstrumentRenderer ASI { get; set; }
         IInstrumentRenderer BackupADI { get; set; }
         IInstrumentRenderer CabinPress { get; set; }
         IInstrumentRenderer CautionPanel { get; set; }
         IInstrumentRenderer CMDSPanel { get; set; }
         IInstrumentRenderer Compass{ get; set; }
         IInstrumentRenderer DED{ get; set; }
         IInstrumentRenderer EHSI{ get; set; }
         IInstrumentRenderer EPUFuel{ get; set; }
         IInstrumentRenderer FTIT1{ get; set; }
         IInstrumentRenderer FTIT2{ get; set; }
         IInstrumentRenderer FuelFlow{ get; set; }
         IInstrumentRenderer FuelQuantity{ get; set; }
         IInstrumentRenderer HSI{ get; set; }
         IInstrumentRenderer HYDA{ get; set; }
         IInstrumentRenderer HYDB{ get; set; }
         IInstrumentRenderer ISIS{ get; set; }
         IInstrumentRenderer LandingGearLights{ get; set; }
         IInstrumentRenderer NOZ1{ get; set; }
         IInstrumentRenderer NOZ2{ get; set; }
         IInstrumentRenderer NWSIndexer{ get; set; }
         IInstrumentRenderer OIL1{ get; set; }
         IInstrumentRenderer OIL2{ get; set; }
         IInstrumentRenderer PFL{ get; set; }
         IInstrumentRenderer PitchTrim{ get; set; }
         IInstrumentRenderer RollTrim{ get; set; }
         IInstrumentRenderer RPM1{ get; set; }
         IInstrumentRenderer RPM2{ get; set; }
         IInstrumentRenderer RWR{ get; set; }
         IInstrumentRenderer Speedbrake{ get; set; }
         IInstrumentRenderer VVI{ get; set; }

    }

    public class InstrumentRendererSet : IInstrumentRendererSet
    {
        public IInstrumentRenderer Accelerometer { get; set; }
        public IInstrumentRenderer ADI { get; set; }
        public IInstrumentRenderer Altimeter { get; set; }
        public IInstrumentRenderer AOAIndexer { get; set; }
        public IInstrumentRenderer AOAIndicator { get; set; }
        public IInstrumentRenderer ASI { get; set; }
        public IInstrumentRenderer BackupADI { get; set; }
        public IInstrumentRenderer CabinPress { get; set; }
        public IInstrumentRenderer CautionPanel { get; set; }
        public IInstrumentRenderer CMDSPanel { get; set; }
        public IInstrumentRenderer Compass { get; set; }
        public IInstrumentRenderer DED { get; set; }
        public IInstrumentRenderer EHSI { get; set; }
        public IInstrumentRenderer EPUFuel { get; set; }
        public IInstrumentRenderer FTIT1 { get; set; }
        public IInstrumentRenderer FTIT2 { get; set; }
        public IInstrumentRenderer FuelFlow { get; set; }
        public IInstrumentRenderer FuelQuantity { get; set; }
        public IInstrumentRenderer HSI { get; set; }
        public IInstrumentRenderer HYDA { get; set; }
        public IInstrumentRenderer HYDB { get; set; }
        public IInstrumentRenderer ISIS { get; set; }
        public IInstrumentRenderer LandingGearLights { get; set; }
        public IInstrumentRenderer NOZ1 { get; set; }
        public IInstrumentRenderer NOZ2 { get; set; }
        public IInstrumentRenderer NWSIndexer { get; set; }
        public IInstrumentRenderer OIL1 { get; set; }
        public IInstrumentRenderer OIL2 { get; set; }
        public IInstrumentRenderer PFL { get; set; }
        public IInstrumentRenderer PitchTrim { get; set; }
        public IInstrumentRenderer RollTrim { get; set; }
        public IInstrumentRenderer RPM1 { get; set; }
        public IInstrumentRenderer RPM2 { get; set; }
        public IInstrumentRenderer RWR { get; set; }
        public IInstrumentRenderer Speedbrake { get; set; }
        public IInstrumentRenderer VVI { get; set; }


    }
}
