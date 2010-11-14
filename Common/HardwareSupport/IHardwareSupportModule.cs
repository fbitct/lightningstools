using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;

namespace Common.HardwareSupport
{
    public interface IHardwareSupportModule
    {
        DigitalSignal[] DigitalInputs { get; }
        AnalogSignal[] AnalogInputs { get; }
        AnalogSignal[] AnalogOutputs { get; }
        DigitalSignal[] DigitalOutputs { get; }
        string FriendlyName{ get; }
    }
}
