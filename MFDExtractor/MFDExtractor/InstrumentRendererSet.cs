using Common.SimSupport;
using LightningGauges.Renderers;
using LightningGauges.Renderers.F16;
using LightningGauges.Renderers.F16.ISIS;
using MFDExtractor.Renderer;

namespace MFDExtractor
{
    public interface IInstrumentRendererSet
    {
         IAccelerometer Accelerometer { get; set; }
         IADI ADI { get; set; }
         IAltimeter Altimeter { get; set; }
         IAngleOfAttackIndexer AOAIndexer { get; set; }
         IAngleOfAttackIndicator AOAIndicator { get; set; }
         IAirspeedIndicator ASI { get; set; }
         IStandbyADI BackupADI { get; set; }
         ICabinPressureAltitudeIndicator CabinPress { get; set; }
         ICautionPanel CautionPanel { get; set; }
         ICMDSPanel CMDS { get; set; }
         ICompass Compass{ get; set; }
         IDataEntryDisplayPilotFaultList DED{ get; set; }
         IEHSI EHSI{ get; set; }
         IEPUFuelGauge EPUFuel{ get; set; }
         IFanTurbineInletTemperature FTIT1{ get; set; }
         IFanTurbineInletTemperature FTIT2{ get; set; }
         IFuelFlow FuelFlow{ get; set; }
         IFuelQuantityIndicator FuelQuantity{ get; set; }
         IHorizontalSituationIndicator HSI{ get; set; }
         IHydraulicPressureGauge HYDA{ get; set; }
         IHydraulicPressureGauge HYDB{ get; set; }
         IISIS ISIS{ get; set; }
         ILandingGearWheelsLights GearLights{ get; set; }
         INozzlePositionIndicator NOZ1{ get; set; }
         INozzlePositionIndicator NOZ2{ get; set; }
         INosewheelSteeringIndexer NWSIndexer{ get; set; }
         IOilPressureGauge OIL1{ get; set; }
         IOilPressureGauge OIL2{ get; set; }
         IDataEntryDisplayPilotFaultList PFL{ get; set; }
         IPitchTrimIndicator PitchTrim{ get; set; }
         IRollTrimIndicator RollTrim{ get; set; }
         ITachometer RPM1 { get; set; }
         ITachometer RPM2 { get; set; }
         IAzimuthIndicator RWR{ get; set; }
         ISpeedbrakeIndicator Speedbrake{ get; set; }
         IVerticalVelocityIndicator VVI{ get; set; }
         IMfdRenderer LMFD { get; set; }
         IMfdRenderer RMFD { get; set; }
		 IMfdRenderer MFD3 { get; set; }
		 IMfdRenderer MFD4 { get; set; }
		 IMfdRenderer HUD { get; set; }

         IInstrumentRenderer this [InstrumentType instrumentType] { get; }
    }

    public class InstrumentRendererSet : IInstrumentRendererSet
    {
        public IAccelerometer Accelerometer { get; set; }
        public IADI ADI { get; set; }
        public IAltimeter Altimeter { get; set; }
        public IAngleOfAttackIndexer AOAIndexer { get; set; }
        public IAngleOfAttackIndicator AOAIndicator { get; set; }
        public IAirspeedIndicator ASI { get; set; }
        public IStandbyADI BackupADI { get; set; }
        public ICabinPressureAltitudeIndicator CabinPress { get; set; }
        public ICautionPanel CautionPanel { get; set; }
        public ICMDSPanel CMDS { get; set; }
        public ICompass Compass { get; set; }
        public IDataEntryDisplayPilotFaultList DED { get; set; }
        public IEHSI EHSI { get; set; }
        public IEPUFuelGauge EPUFuel { get; set; }
        public IFanTurbineInletTemperature FTIT1 { get; set; }
        public IFanTurbineInletTemperature FTIT2 { get; set; }
        public IFuelFlow FuelFlow { get; set; }
        public IFuelQuantityIndicator FuelQuantity { get; set; }
        public IHorizontalSituationIndicator HSI { get; set; }
        public IHydraulicPressureGauge HYDA { get; set; }
        public IHydraulicPressureGauge HYDB { get; set; }
        public IISIS ISIS { get; set; }
        public ILandingGearWheelsLights GearLights { get; set; }
        public INozzlePositionIndicator NOZ1 { get; set; }
        public INozzlePositionIndicator NOZ2 { get; set; }
        public INosewheelSteeringIndexer NWSIndexer { get; set; }
        public IOilPressureGauge OIL1 { get; set; }
        public IOilPressureGauge OIL2 { get; set; }
        public IDataEntryDisplayPilotFaultList PFL { get; set; }
        public IPitchTrimIndicator PitchTrim { get; set; }
        public IRollTrimIndicator RollTrim { get; set; }
        public ITachometer RPM1 { get; set; }
        public ITachometer RPM2 { get; set; }
        public IAzimuthIndicator RWR { get; set; }
        public ISpeedbrakeIndicator Speedbrake { get; set; }
        public IVerticalVelocityIndicator VVI { get; set; }
		public IMfdRenderer LMFD { get; set; }
		public IMfdRenderer RMFD { get; set; }
		public IMfdRenderer MFD3 { get; set; }
		public IMfdRenderer MFD4 { get; set; }
		public IMfdRenderer HUD { get; set; }

        public IInstrumentRenderer this[InstrumentType instrumentType]
        {
            get
            {
                var propertyInfo= GetType().GetProperty(instrumentType.ToString());
                if (propertyInfo ==null) return null;
                var getterMethod= propertyInfo.GetGetMethod();
                if (getterMethod ==null) return null;
                return getterMethod.Invoke(this, null) as IInstrumentRenderer;
            }
        }
    }
}
