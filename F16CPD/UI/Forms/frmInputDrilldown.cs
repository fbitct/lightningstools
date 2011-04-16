using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace F16CPD.UI.Forms
{
    public partial class frmInputDrilldown : Form
    {
        public frmInputDrilldown()
        {
            InitializeComponent();
        }

        public List<string> RadioButtonItems { get; set; }
        public string SelectedRadioButtonItem { get; set; }

        private void frmInputDrilldown_Load(object sender, EventArgs e)
        {
            pnlFlow.Margin = new Padding(0);
            pnlFlow.Padding = new Padding(0);
            int i = 0;
            foreach (string radioItem in RadioButtonItems)
            {
                var button = new RadioButton();
                button.Padding = new Padding(0);
                button.Margin = new Padding(0);
                button.Text = radioItem;
                button.Name = "radioButton" + i;
                button.AutoSize = true;
                button.CheckedChanged += button_CheckedChanged;
                pnlFlow.SetFlowBreak(button, true);
                pnlFlow.Controls.Add(button);
                i++;
            }
            var cancelButton = new Button();
            cancelButton.Text = "&Cancel";
            pnlFlow.SetFlowBreak(cancelButton, true);
            pnlFlow.Controls.Add(cancelButton);
            CancelButton = cancelButton;
        }

        private void button_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in pnlFlow.Controls)
            {
                var radio = control as RadioButton;
                if (radio != null)
                {
                    if (radio.Checked)
                    {
                        SelectedRadioButtonItem = radio.Text;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
        }
    }
}