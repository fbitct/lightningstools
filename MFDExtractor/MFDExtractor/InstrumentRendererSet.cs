using Common.SimSupport;
using LightningGauges.Renderers;
using MFDExtractor.Renderer;

namespace MFDExtractor
{
    public interface IInstrumentRendererSet
    {
         IF16Accelerometer Accelerometer { get; set; }
         IF16ADI ADI { get; set; }
         IF16Altimeter Altimeter { get; set; }
         IF16AngleOfAttackIndexer AOAIndexer { get; set; }
         IF16AngleOfAttackIndicator AOAIndicator { get; set; }
         IF16AirspeedIndicator ASI { get; set; }
         IF16StandbyADI BackupADI { get; set; }
         IF16CabinPressureAltitudeIndicator CabinPress { get; set; }
         IF16CautionPanel CautionPanel { get; set; }
         IF16CMDSPanel CMDSPanel { get; set; }
         IF16Compass Compass{ get; set; }
         IF16DataEntryDisplayPilotFaultList DED{ get; set; }
         IF16EHSI EHSI{ get; set; }
         IF16EPUFuelGauge EPUFuel{ get; set; }
         IF16FanTurbineInletTemperature FTIT1{ get; set; }
         IF16FanTurbineInletTemperature FTIT2{ get; set; }
         IF16FuelFlow FuelFlow{ get; set; }
         IF16FuelQuantityIndicator FuelQuantity{ get; set; }
         IF16HorizontalSituationIndicator HSI{ get; set; }
         IF16HydraulicPressureGauge HYDA{ get; set; }
         IF16HydraulicPressureGauge HYDB{ get; set; }
         IF16ISIS ISIS{ get; set; }
         IF16LandingGearWheelsLights GearLights{ get; set; }
         IF16NozzlePositionIndicator NOZ1{ get; set; }
         IF16NozzlePositionIndicator NOZ2{ get; set; }
         IF16NosewheelSteeringIndexer NWSIndexer{ get; set; }
         IF16OilPressureGauge OIL1{ get; set; }
         IF16OilPressureGauge OIL2{ get; set; }
         IF16DataEntryDisplayPilotFaultList PFL{ get; set; }
         IF16PitchTrimIndicator PitchTrim{ get; set; }
         IF16RollTrimIndicator RollTrim{ get; set; }
         IF16Tachometer RPM1 { get; set; }
         IF16Tachometer RPM2 { get; set; }
         IF16AzimuthIndicator RWR{ get; set; }
         IF16SpeedbrakeIndicator Speedbrake{ get; set; }
         IF16VerticalVelocityIndicator VVI{ get; set; }
         IMfdRenderer LMFD { get; set; }
         IMfdRenderer RMFD { get; set; }
		 IMfdRenderer MFD3 { get; set; }
		 IMfdRenderer MFD4 { get; set; }
		 IMfdRenderer HUD { get; set; }

         IInstrumentRenderer this [InstrumentType instrumentType] { get; }
    }

    public class InstrumentRendererSet : IInstrumentRendererSet
    {
        public IF16Accelerometer Accelerometer { get; set; }
        public IF16ADI ADI { get; set; }
        public IF16Altimeter Altimeter { get; set; }
        public IF16AngleOfAttackIndexer AOAIndexer { get; set; }
        public IF16AngleOfAttackIndicator AOAIndicator { get; set; }
        public IF16AirspeedIndicator ASI { get; set; }
        public IF16StandbyADI BackupADI { get; set; }
        public IF16CabinPressureAltitudeIndicator CabinPress { get; set; }
        public IF16CautionPanel CautionPanel { get; set; }
        public IF16CMDSPanel CMDS { get; set; }
        public IF16Compass Compass { get; set; }
        public IF16DataEntryDisplayPilotFaultList DED { get; set; }
        public IF16EHSI EHSI { get; set; }
        public IF16EPUFuelGauge EPUFuel { get; set; }
        public IF16FanTurbineInletTemperature FTIT1 { get; set; }
        public IF16FanTurbineInletTemperature FTIT2 { get; set; }
        public IF16FuelFlow FuelFlow { get; set; }
        public IF16FuelQuantityIndicator FuelQuantity { get; set; }
        public IF16HorizontalSituationIndicator HSI { get; set; }
        public IF16HydraulicPressureGauge HYDA { get; set; }
        public IF16HydraulicPressureGauge HYDB { get; set; }
        public IF16ISIS ISIS { get; set; }
        public IF16LandingGearWheelsLights GearLights { get; set; }
        public IF16NozzlePositionIndicator NOZ1 { get; set; }
        public IF16NozzlePositionIndicator NOZ2 { get; set; }
        public IF16NosewheelSteeringIndexer NWSIndexer { get; set; }
        public IF16OilPressureGauge OIL1 { get; set; }
        public IF16OilPressureGauge OIL2 { get; set; }
        public IF16DataEntryDisplayPilotFaultList PFL { get; set; }
        public IF16PitchTrimIndicator PitchTrim { get; set; }
        public IF16RollTrimIndicator RollTrim { get; set; }
        public IF16Tachometer RPM1 { get; set; }
        public IF16Tachometer RPM2 { get; set; }
        public IF16AzimuthIndicator RWR { get; set; }
        public IF16SpeedbrakeIndicator Speedbrake { get; set; }
        public IF16VerticalVelocityIndicator VVI { get; set; }
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
