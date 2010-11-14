namespace MFDExtractor.UI
{
    partial class frmSelectImportExportCoordinates
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkLeftMfdPrimary = new System.Windows.Forms.CheckBox();
            this.chkRightMfdPrimary = new System.Windows.Forms.CheckBox();
            this.chkMfd3Primary = new System.Windows.Forms.CheckBox();
            this.chkMfd4Primary = new System.Windows.Forms.CheckBox();
            this.chkHudPrimary = new System.Windows.Forms.CheckBox();
            this.grpPrimaryCoordinates = new System.Windows.Forms.GroupBox();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.grpSecondaryCoordinates = new System.Windows.Forms.GroupBox();
            this.chkLeftMfdSecondary = new System.Windows.Forms.CheckBox();
            this.chkHudSecondary = new System.Windows.Forms.CheckBox();
            this.chkRightMfdSecondary = new System.Windows.Forms.CheckBox();
            this.chkMfd4Secondary = new System.Windows.Forms.CheckBox();
            this.chkMfd3Secondary = new System.Windows.Forms.CheckBox();
            this.cmdSelectAll = new System.Windows.Forms.Button();
            this.cmdUnselectAll = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.grpPrimaryCoordinates.SuspendLayout();
            this.grpSecondaryCoordinates.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkLeftMfdPrimary
            // 
            this.chkLeftMfdPrimary.AutoSize = true;
            this.chkLeftMfdPrimary.Location = new System.Drawing.Point(6, 41);
            this.chkLeftMfdPrimary.Name = "chkLeftMfdPrimary";
            this.chkLeftMfdPrimary.Size = new System.Drawing.Size(70, 17);
            this.chkLeftMfdPrimary.TabIndex = 2;
            this.chkLeftMfdPrimary.Text = "Left MFD";
            this.chkLeftMfdPrimary.UseVisualStyleBackColor = true;
            // 
            // chkRightMfdPrimary
            // 
            this.chkRightMfdPrimary.AutoSize = true;
            this.chkRightMfdPrimary.Location = new System.Drawing.Point(6, 64);
            this.chkRightMfdPrimary.Name = "chkRightMfdPrimary";
            this.chkRightMfdPrimary.Size = new System.Drawing.Size(77, 17);
            this.chkRightMfdPrimary.TabIndex = 3;
            this.chkRightMfdPrimary.Text = "Right MFD";
            this.chkRightMfdPrimary.UseVisualStyleBackColor = true;
            // 
            // chkMfd3Primary
            // 
            this.chkMfd3Primary.AutoSize = true;
            this.chkMfd3Primary.Location = new System.Drawing.Point(6, 87);
            this.chkMfd3Primary.Name = "chkMfd3Primary";
            this.chkMfd3Primary.Size = new System.Drawing.Size(61, 17);
            this.chkMfd3Primary.TabIndex = 4;
            this.chkMfd3Primary.Text = "MFD 3 ";
            this.chkMfd3Primary.UseVisualStyleBackColor = true;
            // 
            // chkMfd4Primary
            // 
            this.chkMfd4Primary.AutoSize = true;
            this.chkMfd4Primary.Location = new System.Drawing.Point(6, 110);
            this.chkMfd4Primary.Name = "chkMfd4Primary";
            this.chkMfd4Primary.Size = new System.Drawing.Size(58, 17);
            this.chkMfd4Primary.TabIndex = 5;
            this.chkMfd4Primary.Text = "MFD 4";
            this.chkMfd4Primary.UseVisualStyleBackColor = true;
            // 
            // chkHudPrimary
            // 
            this.chkHudPrimary.AutoSize = true;
            this.chkHudPrimary.Location = new System.Drawing.Point(6, 133);
            this.chkHudPrimary.Name = "chkHudPrimary";
            this.chkHudPrimary.Size = new System.Drawing.Size(50, 17);
            this.chkHudPrimary.TabIndex = 6;
            this.chkHudPrimary.Text = "HUD";
            this.chkHudPrimary.UseVisualStyleBackColor = true;
            // 
            // grpPrimaryCoordinates
            // 
            this.grpPrimaryCoordinates.Controls.Add(this.chkLeftMfdPrimary);
            this.grpPrimaryCoordinates.Controls.Add(this.chkHudPrimary);
            this.grpPrimaryCoordinates.Controls.Add(this.chkRightMfdPrimary);
            this.grpPrimaryCoordinates.Controls.Add(this.chkMfd4Primary);
            this.grpPrimaryCoordinates.Controls.Add(this.chkMfd3Primary);
            this.grpPrimaryCoordinates.Location = new System.Drawing.Point(12, 45);
            this.grpPrimaryCoordinates.Name = "grpPrimaryCoordinates";
            this.grpPrimaryCoordinates.Size = new System.Drawing.Size(146, 165);
            this.grpPrimaryCoordinates.TabIndex = 1;
            this.grpPrimaryCoordinates.TabStop = false;
            this.grpPrimaryCoordinates.Text = "Primary 2D Cockpit View Image Source Coordinates";
            // 
            // cmdOk
            // 
            this.cmdOk.Location = new System.Drawing.Point(7, 265);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(107, 29);
            this.cmdOk.TabIndex = 15;
            this.cmdOk.Text = "&OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(120, 265);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(98, 29);
            this.cmdCancel.TabIndex = 16;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // grpSecondaryCoordinates
            // 
            this.grpSecondaryCoordinates.Controls.Add(this.chkLeftMfdSecondary);
            this.grpSecondaryCoordinates.Controls.Add(this.chkHudSecondary);
            this.grpSecondaryCoordinates.Controls.Add(this.chkRightMfdSecondary);
            this.grpSecondaryCoordinates.Controls.Add(this.chkMfd4Secondary);
            this.grpSecondaryCoordinates.Controls.Add(this.chkMfd3Secondary);
            this.grpSecondaryCoordinates.Location = new System.Drawing.Point(164, 45);
            this.grpSecondaryCoordinates.Name = "grpSecondaryCoordinates";
            this.grpSecondaryCoordinates.Size = new System.Drawing.Size(159, 165);
            this.grpSecondaryCoordinates.TabIndex = 7;
            this.grpSecondaryCoordinates.TabStop = false;
            this.grpSecondaryCoordinates.Text = "Secondary 2D Cockpit View Image Source Coordinates";
            // 
            // chkLeftMfdSecondary
            // 
            this.chkLeftMfdSecondary.AutoSize = true;
            this.chkLeftMfdSecondary.Location = new System.Drawing.Point(6, 41);
            this.chkLeftMfdSecondary.Name = "chkLeftMfdSecondary";
            this.chkLeftMfdSecondary.Size = new System.Drawing.Size(70, 17);
            this.chkLeftMfdSecondary.TabIndex = 8;
            this.chkLeftMfdSecondary.Text = "Left MFD";
            this.chkLeftMfdSecondary.UseVisualStyleBackColor = true;
            // 
            // chkHudSecondary
            // 
            this.chkHudSecondary.AutoSize = true;
            this.chkHudSecondary.Location = new System.Drawing.Point(6, 133);
            this.chkHudSecondary.Name = "chkHudSecondary";
            this.chkHudSecondary.Size = new System.Drawing.Size(50, 17);
            this.chkHudSecondary.TabIndex = 12;
            this.chkHudSecondary.Text = "HUD";
            this.chkHudSecondary.UseVisualStyleBackColor = true;
            // 
            // chkRightMfdSecondary
            // 
            this.chkRightMfdSecondary.AutoSize = true;
            this.chkRightMfdSecondary.Location = new System.Drawing.Point(6, 64);
            this.chkRightMfdSecondary.Name = "chkRightMfdSecondary";
            this.chkRightMfdSecondary.Size = new System.Drawing.Size(77, 17);
            this.chkRightMfdSecondary.TabIndex = 9;
            this.chkRightMfdSecondary.Text = "Right MFD";
            this.chkRightMfdSecondary.UseVisualStyleBackColor = true;
            // 
            // chkMfd4Secondary
            // 
            this.chkMfd4Secondary.AutoSize = true;
            this.chkMfd4Secondary.Location = new System.Drawing.Point(6, 110);
            this.chkMfd4Secondary.Name = "chkMfd4Secondary";
            this.chkMfd4Secondary.Size = new System.Drawing.Size(58, 17);
            this.chkMfd4Secondary.TabIndex = 11;
            this.chkMfd4Secondary.Text = "MFD 4";
            this.chkMfd4Secondary.UseVisualStyleBackColor = true;
            // 
            // chkMfd3Secondary
            // 
            this.chkMfd3Secondary.AutoSize = true;
            this.chkMfd3Secondary.Location = new System.Drawing.Point(6, 87);
            this.chkMfd3Secondary.Name = "chkMfd3Secondary";
            this.chkMfd3Secondary.Size = new System.Drawing.Size(61, 17);
            this.chkMfd3Secondary.TabIndex = 10;
            this.chkMfd3Secondary.Text = "MFD 3 ";
            this.chkMfd3Secondary.UseVisualStyleBackColor = true;
            // 
            // cmdSelectAll
            // 
            this.cmdSelectAll.Location = new System.Drawing.Point(83, 216);
            this.cmdSelectAll.Name = "cmdSelectAll";
            this.cmdSelectAll.Size = new System.Drawing.Size(75, 23);
            this.cmdSelectAll.TabIndex = 13;
            this.cmdSelectAll.Text = "Select All";
            this.cmdSelectAll.UseVisualStyleBackColor = true;
            this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
            // 
            // cmdUnselectAll
            // 
            this.cmdUnselectAll.Location = new System.Drawing.Point(164, 216);
            this.cmdUnselectAll.Name = "cmdUnselectAll";
            this.cmdUnselectAll.Size = new System.Drawing.Size(75, 23);
            this.cmdUnselectAll.TabIndex = 14;
            this.cmdUnselectAll.Text = "Unselect All";
            this.cmdUnselectAll.UseVisualStyleBackColor = true;
            this.cmdUnselectAll.Click += new System.EventHandler(this.cmdUnselectAll_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(311, 23);
            this.textBox1.TabIndex = 99;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "Choose one or more set(s) of coordinates to load or save.";
            // 
            // frmSelectImportExportCoordinates
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(341, 306);
            this.ControlBox = false;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cmdUnselectAll);
            this.Controls.Add(this.cmdSelectAll);
            this.Controls.Add(this.grpSecondaryCoordinates);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.grpPrimaryCoordinates);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmSelectImportExportCoordinates";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load/Save Coordinates";
            this.Load += new System.EventHandler(this.frmSelectImportExportCoordinates_Load);
            this.grpPrimaryCoordinates.ResumeLayout(false);
            this.grpPrimaryCoordinates.PerformLayout();
            this.grpSecondaryCoordinates.ResumeLayout(false);
            this.grpSecondaryCoordinates.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkLeftMfdPrimary;
        private System.Windows.Forms.CheckBox chkRightMfdPrimary;
        private System.Windows.Forms.CheckBox chkMfd3Primary;
        private System.Windows.Forms.CheckBox chkMfd4Primary;
        private System.Windows.Forms.CheckBox chkHudPrimary;
        private System.Windows.Forms.GroupBox grpPrimaryCoordinates;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.GroupBox grpSecondaryCoordinates;
        private System.Windows.Forms.CheckBox chkLeftMfdSecondary;
        private System.Windows.Forms.CheckBox chkHudSecondary;
        private System.Windows.Forms.CheckBox chkRightMfdSecondary;
        private System.Windows.Forms.CheckBox chkMfd4Secondary;
        private System.Windows.Forms.CheckBox chkMfd3Secondary;
        private System.Windows.Forms.Button cmdSelectAll;
        private System.Windows.Forms.Button cmdUnselectAll;
        private System.Windows.Forms.TextBox textBox1;
    }
}