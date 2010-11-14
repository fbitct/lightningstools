using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Phcc.DeviceManager.UI
{
    public partial class frmPromptForPeripheralAddress : Form
    {
        public byte BaseAddress
        {
            get;
            set;
        }
        public List<byte> ProhibitedBaseAddresses
        {
            get;
            set;
        }
        public frmPromptForPeripheralAddress()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            bool wasValid = ValidateBaseAddress();
            if (wasValid)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        private bool ValidateBaseAddress()
        {
            errorProvider1.Clear();
            string baseAddressAsEntered = txtBaseAddress.Text;
            byte val = 0;
            bool valid = TryParseBaseAddress(baseAddressAsEntered, out val);
            if (!valid)
            {
                errorProvider1.SetError(txtBaseAddress, "Invalid hexadecimal or decimal byte value.\nHex values should be preceded by the\ncharacters '0x' (zero, x) and\nshould be in the range 0x00-0xFF.\nDecimal values should be in the range 0-255.");
            }
            else
            {
                if (this.ProhibitedBaseAddresses != null)
                {
                    foreach (byte prohibitedAddress in this.ProhibitedBaseAddresses)
                    {
                        if (prohibitedAddress == val)
                        {
                            valid = false;
                            errorProvider1.SetError(txtBaseAddress, "That peripheral address is already being used by another peripheral.");
                            break;
                        }
                    }
                }
                if (valid)
                {
                    this.BaseAddress = val;
                }
            }
            return valid;
        }
        private bool TryParseBaseAddress(string baseAddress, out byte val)
        {
            bool valid = true;
            val = 0;
            if (!String.IsNullOrEmpty(baseAddress)) 
            {
                baseAddress = baseAddress.Trim();
            }
            if (String.IsNullOrEmpty(baseAddress))
            {
                valid = false;
            }
            else 
            {
                if (baseAddress.StartsWith("0x") || baseAddress.StartsWith("0X"))
                {
                    baseAddress = baseAddress.Substring(2, baseAddress.Length - 2);
                    valid = Byte.TryParse(baseAddress, System.Globalization.NumberStyles.HexNumber, null, out val);
                }
                else
                {
                    valid= Byte.TryParse(baseAddress, out val);
                }
            }
            return valid;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtBaseAddress_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }
    }
}
