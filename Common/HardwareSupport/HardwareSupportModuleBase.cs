
using Common.MacroProgramming;
using System;
namespace Common.HardwareSupport
{
    public abstract class HardwareSupportModuleBase:IHardwareSupportModule
    {
        protected HardwareSupportModuleBase()
            : base()
        {
            
        }
        #region IHardwareSupportModule Members

        public abstract DigitalSignal[] DigitalInputs { get; }
        public abstract AnalogSignal[] AnalogInputs { get; }
        public abstract AnalogSignal[] AnalogOutputs { get; }
        public abstract DigitalSignal[] DigitalOutputs { get; }
        public abstract string FriendlyName { get; }
        #endregion
    }
}
