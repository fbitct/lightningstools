namespace MFDExtractor.Networking
{
    public enum MessageTypes
    {
        AirspeedIndexIncrease,
        AirspeedIndexDecrease,
        ToggleNightMode,
        EHSILeftKnobIncrease,
        EHSILeftKnobDecrease,
        EHSIRightKnobIncrease,
        EHSIRightKnobDecrease,
        EHSIRightKnobDepressed,
        EHSIRightKnobReleased,
        EHSIMenuButtonDepressed,
        ISISBrightButtonDepressed,
        ISISStandardButtonDepressed,
        AzimuthIndicatorBrightnessIncrease,
        AzimuthIndicatorBrightnessDecrease,
        AccelerometerIsReset,
        EnableBMSAdvancedSharedmemValues,
        DisableBMSAdvancedSharedmemValues
    }
}