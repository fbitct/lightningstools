using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.MacroProgramming;

namespace SimLinkup.UI.UserControls
{
    public partial class SignalCollectionBasicVisualizer : UserControl
    {
        private IEnumerable<Signal> _signals;
        public SignalCollectionBasicVisualizer()
        {
            InitializeComponent();
        }
        public IEnumerable<Signal> Signals 
        {
            get { return _signals; }
            set 
            {
                _signals = value;
                panel.Controls.Clear();
                foreach (var signal in Signals)
                {
                    if (signal is AnalogSignal)
                    {
                        var visualizer = new AnalogSignalBasicVisualizer();
                        visualizer.Signal = (AnalogSignal)signal;
                        panel.Controls.Add(visualizer);
                    }
                    else if (signal is DigitalSignal)
                    {
                        var visualizer = new DigitalSignalBasicVisualizer();
                        visualizer.Signal = (DigitalSignal)signal;
                        panel.Controls.Add(visualizer);
                    }
                }
            }
        }
    }
}
