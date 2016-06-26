using System.Collections.Generic;
using System.Windows.Forms;
using Common.SimSupport;
using Common.HardwareSupport;
using Common.MacroProgramming;

namespace SimLinkup.UI.UserControls
{
    public partial class ModuleList : UserControl
    {
        private IEnumerable<SimSupportModule> _simSupportModules;
        private IEnumerable<IHardwareSupportModule> _hardwareSupportModules;
        public ModuleList()
        {
            InitializeComponent();
        }
        public IEnumerable<SimSupportModule> SimSupportModules
        {
            get { return _simSupportModules; }
            set
            {
                panel.Controls.Clear();
                _simSupportModules = value;
                if (_simSupportModules == null)
                {
                    return;
                }
                foreach (var ssm in _simSupportModules)
                {
                    var visualizer = new ModuleBasicVisualizer();
                    visualizer.ModuleName = ssm.FriendlyName;
                    visualizer.ShowSignals += (s, e) =>
                    {

                    };
                    panel.Controls.Add(visualizer);
                }
            }
        }
        public IEnumerable<IHardwareSupportModule> HardwareSupportModules
        {
            get { return _hardwareSupportModules; }
            set
            {
                panel.Controls.Clear();
                _hardwareSupportModules = value;
                if (_hardwareSupportModules == null)
                {
                    return;
                }
                foreach (var hsm in _hardwareSupportModules)
                {
                    var visualizer = new ModuleBasicVisualizer();
                    visualizer.ModuleName = hsm.FriendlyName;
                    visualizer.ShowSignals += (s, e) =>
                    {
                        var signalsView = new frmSignalsViewer();
                        var signalList = new SignalList<Signal>();
                        if (hsm.AnalogInputs != null)
                        {
                            signalList.AddRange(hsm.AnalogInputs);
                        }
                        if (hsm.AnalogOutputs != null)
                        {
                            signalList.AddRange(hsm.AnalogOutputs);
                        }
                        if (hsm.DigitalInputs != null)
                        {
                            signalList.AddRange(hsm.DigitalInputs);
                        }
                        if (hsm.DigitalOutputs != null)
                        {
                            signalList.AddRange(hsm.DigitalOutputs);
                        }
                        signalsView.Signals = signalList;
                        signalsView.ShowDialog(this.ParentForm);
                    };
                    panel.Controls.Add(visualizer);
                }
            }
        }
    }
}
