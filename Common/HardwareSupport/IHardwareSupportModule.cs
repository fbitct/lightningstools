using Common.MacroProgramming;
using System.Drawing;
using System.Threading.Tasks;

namespace Common.HardwareSupport
{
    public interface IHardwareSupportModule
    {
        DigitalSignal[] DigitalInputs { get; }
        AnalogSignal[] AnalogInputs { get; }
        AnalogSignal[] AnalogOutputs { get; }
        DigitalSignal[] DigitalOutputs { get; }
        string FriendlyName { get; }
        Task SynchronizeAsync();
        void Render(Graphics g, Rectangle destinationRectangle);
    }
}