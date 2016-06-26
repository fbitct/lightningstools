using Common.MacroProgramming;
using System;
using System.Windows.Forms;

namespace SimLinkup.UI
{
    public partial class frmSignalsViewer : Form
    {
        public frmSignalsViewer()
        {
            InitializeComponent();
        }
        public SignalList<Signal> Signals 
        { 
            get { return signalsView.Signals; }
            set { signalsView.Signals = value; }
        }
        protected override void OnShown(EventArgs e)
        {
            signalsView.UpdateContents();
            base.OnShown(e);

        }
    }
}
