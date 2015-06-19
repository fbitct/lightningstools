using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimLinkup.UI.UserControls
{
    public partial class ModuleBasicVisualizer : UserControl
    {
        public event EventHandler ShowSignals;
        public ModuleBasicVisualizer()
        {
            InitializeComponent();
        }
        public string ModuleName 
        { 
            get { return lblModuleName.Text; }
            set { lblModuleName.Text = value; }
        }

        private void btnShowSignals_Click(object sender, EventArgs e)
        {
            if (ShowSignals != null)
            {
                ShowSignals(this, null);
            }
        }
        
    }
}
