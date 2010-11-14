using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MFDExtractor.UI
{
    public partial class frmSelectImportExportCoordinates : Form
    {
        public bool EnableSelectLeftMfdPrimary
        {
            get;
            set;
        }
        public bool EnableSelectRightMfdPrimary
        {
            get;
            set;
        }
        public bool EnableSelectMfd3Primary
        {
            get;
            set;
        }
        public bool EnableSelectMfd4Primary
        {
            get;
            set;
        }
        public bool EnableSelectHudPrimary
        {
            get;
            set;
        }

        public bool EnableSelectLeftMfdSecondary
        {
            get;
            set;
        }
        public bool EnableSelectRightMfdSecondary
        {
            get;
            set;
        }
        public bool EnableSelectMfd3Secondary
        {
            get;
            set;
        }
        public bool EnableSelectMfd4Secondary
        {
            get;
            set;
        }
        public bool EnableSelectHudSecondary
        {
            get;
            set;
        }

        public bool ExportLeftMfdPrimary
        {
            get
            {
                return chkLeftMfdPrimary.Checked;
            }
            set
            {
                chkLeftMfdPrimary.Checked = value;
            }
        }
        public bool ExportRightMfdPrimary
        {
            get
            {
                return chkRightMfdPrimary.Checked;
            }
            set
            {
                chkRightMfdPrimary.Checked = value;
            }
        }
        public bool ExportHudPrimary
        {
            get
            {
                return chkHudPrimary.Checked;
            }
            set
            {
                chkHudPrimary.Checked = value;
            }
        }
        public bool ExportMfd3Primary
        {
            get
            {
                return chkMfd3Primary.Checked;
            }
            set
            {
                chkMfd3Primary.Checked = value;
            }
        }
        public bool ExportMfd4Primary
        {
            get
            {
                return chkMfd4Primary.Checked;
            }
            set
            {
                chkMfd4Primary.Checked = value;
            }
        }
        public bool ExportLeftMfdSecondary
        {
            get
            {
                return chkLeftMfdSecondary.Checked;
            }
            set
            {
                chkLeftMfdSecondary.Checked = value;
            }
        }
        public bool ExportRightMfdSecondary
        {
            get
            {
                return chkRightMfdSecondary.Checked;
            }
            set
            {
                chkRightMfdSecondary.Checked = value;
            }
        }
        public bool ExportHudSecondary
        {
            get
            {
                return chkHudSecondary.Checked;
            }
            set
            {
                chkHudSecondary.Checked = value;
            }
        }
        public bool ExportMfd3Secondary
        {
            get
            {
                return chkMfd3Secondary.Checked;
            }
            set
            {
                chkMfd3Secondary.Checked = value;
            }
        }
        public bool ExportMfd4Secondary
        {
            get
            {
                return chkMfd4Secondary.Checked;
            }
            set
            {
                chkMfd4Secondary.Checked = value;
            }
        }


        public frmSelectImportExportCoordinates()
        {
            InitializeComponent();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            int selectedItems = CountSelectedItems();
            int enabledItems = CountEnabledItems();
            if (selectedItems== 0 && enabledItems>0)
            {
                MessageBox.Show("Please select at least one coordinate set.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (selectedItems == 0 && enabledItems == 0)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cmdSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }
        private int CountSelectedItems()
        {
            int count = 0;
            if (chkLeftMfdPrimary.Checked) count++;
            if (chkRightMfdPrimary.Checked) count++;
            if (chkMfd3Primary.Checked) count++;
            if (chkMfd4Primary.Checked) count++;
            if (chkHudPrimary.Checked) count++;
            if (chkLeftMfdSecondary.Checked) count++;
            if (chkRightMfdSecondary.Checked) count++;
            if (chkMfd3Secondary.Checked) count++;
            if (chkMfd4Secondary.Checked) count++;
            if (chkHudSecondary.Checked) count++;
            return count;
        }
        private int CountEnabledItems()
        {
            int count = 0;
            if (chkLeftMfdPrimary.Enabled) count++;
            if (chkRightMfdPrimary.Enabled) count++;
            if (chkMfd3Primary.Enabled) count++;
            if (chkMfd4Primary.Enabled) count++;
            if (chkHudPrimary.Enabled) count++;
            if (chkLeftMfdSecondary.Enabled) count++;
            if (chkRightMfdSecondary.Enabled) count++;
            if (chkMfd3Secondary.Enabled) count++;
            if (chkMfd4Secondary.Enabled) count++;
            if (chkHudSecondary.Enabled) count++;
            return count;
        }
        public void SelectAll()
        {
            UpdateEnableDisableState();
            if (chkLeftMfdPrimary.Enabled) chkLeftMfdPrimary.Checked = true;
            if (chkRightMfdPrimary.Enabled) chkRightMfdPrimary.Checked = true;
            if (chkMfd3Primary.Enabled) chkMfd3Primary.Checked = true;
            if (chkMfd4Primary.Enabled) chkMfd4Primary.Checked = true;
            if (chkHudPrimary.Enabled) chkHudPrimary.Checked = true;
            if (chkLeftMfdSecondary.Enabled) chkLeftMfdSecondary.Checked = true;
            if (chkRightMfdSecondary.Enabled) chkRightMfdSecondary.Checked = true;
            if (chkMfd3Secondary.Enabled) chkMfd3Secondary.Checked = true;
            if (chkMfd4Secondary.Enabled) chkMfd4Secondary.Checked = true;
            if (chkHudSecondary.Enabled) chkHudSecondary.Checked = true;
        }

        private void cmdUnselectAll_Click(object sender, EventArgs e)
        {
            UnselectAll();
        }
        public void UnselectAll()
        {
            chkLeftMfdPrimary.Checked = false;
            chkRightMfdPrimary.Checked = false;
            chkMfd3Primary.Checked = false;
            chkMfd4Primary.Checked = false;
            chkHudPrimary.Checked = false;

            chkLeftMfdSecondary.Checked = false;
            chkRightMfdSecondary.Checked = false;
            chkMfd3Secondary.Checked = false;
            chkMfd4Secondary.Checked = false;
            chkHudSecondary.Checked = false;
        }
        private void frmSelectImportExportCoordinates_Load(object sender, EventArgs e)
        {
            UpdateEnableDisableState();
        }
        private void UpdateEnableDisableState()
        {
            chkLeftMfdPrimary.Enabled = this.EnableSelectLeftMfdPrimary;
            chkRightMfdPrimary.Enabled = this.EnableSelectRightMfdPrimary;
            chkMfd3Primary.Enabled = this.EnableSelectMfd3Primary;
            chkMfd4Primary.Enabled = this.EnableSelectMfd4Primary;
            chkHudPrimary.Enabled = this.EnableSelectHudPrimary;
            chkLeftMfdSecondary.Enabled = this.EnableSelectLeftMfdSecondary;
            chkRightMfdSecondary.Enabled = this.EnableSelectRightMfdSecondary;
            chkMfd3Secondary.Enabled = this.EnableSelectMfd3Secondary;
            chkMfd4Secondary.Enabled = this.EnableSelectMfd4Secondary;
            chkHudSecondary.Enabled = this.EnableSelectHudSecondary;
        }
        public void EnableAll()
        {
            this.EnableSelectLeftMfdPrimary = true;
            this.EnableSelectRightMfdPrimary = true;
            this.EnableSelectMfd3Primary = true;
            this.EnableSelectMfd4Primary = true;
            this.EnableSelectHudPrimary = true;
            this.EnableSelectLeftMfdSecondary = true;
            this.EnableSelectRightMfdSecondary = true;
            this.EnableSelectMfd3Secondary = true;
            this.EnableSelectMfd4Secondary = true;
            this.EnableSelectHudSecondary = true;
            UpdateEnableDisableState();
        }
        public void DisableAll()
        {
            this.EnableSelectLeftMfdPrimary = false;
            this.EnableSelectRightMfdPrimary = false;
            this.EnableSelectMfd3Primary = false;
            this.EnableSelectMfd4Primary = false;
            this.EnableSelectHudPrimary = false;
            this.EnableSelectLeftMfdSecondary = false;
            this.EnableSelectRightMfdSecondary = false;
            this.EnableSelectMfd3Secondary = false;
            this.EnableSelectMfd4Secondary = false;
            this.EnableSelectHudSecondary = false;
            UpdateEnableDisableState();
        }
    }
}
