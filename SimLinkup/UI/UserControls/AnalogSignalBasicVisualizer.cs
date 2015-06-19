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
using Common.Math;
namespace SimLinkup.UI.UserControls
{
    public partial class AnalogSignalBasicVisualizer : UserControl
    {
        private AnalogSignal _signal;
        public AnalogSignalBasicVisualizer()
        {
            InitializeComponent();
        }
        public AnalogSignal Signal
        {
            get { return _signal; }
            set
            {
                if (_signal != null)
                {
                    _signal.SignalChanged -= signal_SignalChanged; ;
                }
                var signal = value;
                signal.SignalChanged += signal_SignalChanged;
                _signal = signal;
                lblSignalName.Text = signal.FriendlyName;
            }
        }

        void signal_SignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            txtSignalValue.Text = args.CurrentState.FormatDecimal(_signal.Precision);
        }

    }
}
