﻿using System;
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
            ShowSignals?.Invoke(this, null);
        }
        
    }
}
