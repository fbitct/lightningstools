using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace F16CPD.UI.Forms
{
    public partial class frmInputDrilldown : Form
    {
        public frmInputDrilldown()
        {
            InitializeComponent();
        }
        public List<string> RadioButtonItems
        {
            get;
            set;
        }
        public string SelectedRadioButtonItem
        {
            get;
            set;
        }
        private void frmInputDrilldown_Load(object sender, EventArgs e)
        {
            pnlFlow.Margin = new Padding(0);
            pnlFlow.Padding = new Padding(0);
            int i = 0;
            foreach (string radioItem in this.RadioButtonItems)
            {
                RadioButton button = new RadioButton();
                button.Padding = new Padding(0);
                button.Margin = new Padding(0);
                button.Text = radioItem;
                button.Name = "radioButton" + i;
                button.AutoSize = true;
                button.CheckedChanged += new EventHandler(button_CheckedChanged);
                pnlFlow.SetFlowBreak(button, true);
                pnlFlow.Controls.Add(button);
                i++;
            }
            Button cancelButton = new Button();
            cancelButton.Text = "&Cancel";
            pnlFlow.SetFlowBreak(cancelButton, true);
            pnlFlow.Controls.Add(cancelButton);
            this.CancelButton = cancelButton;
        }

        void button_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in pnlFlow.Controls)
            {
                RadioButton radio = control as RadioButton;
                if (radio != null)
                {
                    if (radio.Checked)
                    {
                        this.SelectedRadioButtonItem = radio.Text;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }
    }
}
