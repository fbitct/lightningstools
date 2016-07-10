using Common.MacroProgramming;
using System.Drawing;
using System.Threading.Tasks;

namespace Common.HardwareSupport
{
    public abstract class HardwareSupportModuleBase : IHardwareSupportModule
    {
        #region IHardwareSupportModule Members

        public abstract DigitalSignal[] DigitalInputs { get; }
        public abstract AnalogSignal[] AnalogInputs { get; }
        public abstract AnalogSignal[] AnalogOutputs { get; }
        public abstract DigitalSignal[] DigitalOutputs { get; }
        public abstract string FriendlyName { get; }
        public virtual async Task SynchronizeAsync() { }
        public virtual void Render(Graphics g, Rectangle destinationRectangle) { }

        #endregion
    }
}