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
    public partial class DigitalSignalBasicVisualizer : UserControl
    {
        private DigitalSignal _signal;
        public DigitalSignalBasicVisualizer()
        {
            InitializeComponent();
        }
        
        public DigitalSignal Signal 
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
                chkValue.Text = signal.FriendlyName;
            }
        }

        private void signal_SignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            chkValue.Checked = args.CurrentState;
        }
    }
}
