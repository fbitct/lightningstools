namespace F4KeyFileEditor
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpenFile = new System.Windows.Forms.ToolStripButton();
            this.btnSaveFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.gridKeyBindings = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gridDirectInputBindings = new System.Windows.Forms.DataGridView();
            this.diLineNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diCallback = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diBindingType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diCockpitItemId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diComboKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diDeviceGuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diButtonIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.diPOVDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbLineNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbCallback = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbCockpitItemId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbComboKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbUIAccessibility = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kbMouseClickableOnly = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridKeyBindings)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDirectInputBindings)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1262, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpen,
            this.toolStripMenuItem1,
            this.mnuFileSave,
            this.mnuFileSaveAs,
            this.toolStripMenuItem2,
            this.mnuFileExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(35, 20);
            this.mnuFile.Text = "&File";
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Image = ((System.Drawing.Image)(resources.GetObject("mnuFileOpen.Image")));
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.Size = new System.Drawing.Size(152, 22);
            this.mnuFileOpen.Text = "&Open...";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // mnuFileSave
            // 
            this.mnuFileSave.Image = ((System.Drawing.Image)(resources.GetObject("mnuFileSave.Image")));
            this.mnuFileSave.Name = "mnuFileSave";
            this.mnuFileSave.Size = new System.Drawing.Size(152, 22);
            this.mnuFileSave.Text = "&Save...";
            this.mnuFileSave.Click += new System.EventHandler(this.mnuFileSave_Click);
            // 
            // mnuFileSaveAs
            // 
            this.mnuFileSaveAs.Name = "mnuFileSaveAs";
            this.mnuFileSaveAs.Size = new System.Drawing.Size(152, 22);
            this.mnuFileSaveAs.Text = "Save &As...";
            this.mnuFileSaveAs.Click += new System.EventHandler(this.mnuFileSaveAs_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.Size = new System.Drawing.Size(152, 22);
            this.mnuFileExit.Text = "E&xit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHelpAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(40, 20);
            this.mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Name = "mnuHelpAbout";
            this.mnuHelpAbout.Size = new System.Drawing.Size(152, 22);
            this.mnuHelpAbout.Text = "&About...";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenFile,
            this.btnSaveFile,
            this.toolStripSeparator});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1262, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenFile.Image")));
            this.btnOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(23, 22);
            this.btnOpenFile.Text = "&Open";
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveFile.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveFile.Image")));
            this.btnSaveFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(23, 22);
            this.btnSaveFile.Text = "&Save";
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // gridKeyBindings
            // 
            this.gridKeyBindings.AllowUserToAddRows = false;
            this.gridKeyBindings.AllowUserToDeleteRows = false;
            this.gridKeyBindings.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridKeyBindings.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.gridKeyBindings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridKeyBindings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.kbLineNum,
            this.kbCallback,
            this.kbDescription,
            this.kbCockpitItemId,
            this.kbComboKey,
            this.kbKey,
            this.kbUIAccessibility,
            this.kbMouseClickableOnly});
            this.gridKeyBindings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridKeyBindings.Location = new System.Drawing.Point(0, 0);
            this.gridKeyBindings.Name = "gridKeyBindings";
            this.gridKeyBindings.ReadOnly = true;
            this.gridKeyBindings.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.gridKeyBindings.Size = new System.Drawing.Size(1262, 276);
            this.gridKeyBindings.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 47);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gridKeyBindings);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gridDirectInputBindings);
            this.splitContainer1.Size = new System.Drawing.Size(1262, 389);
            this.splitContainer1.SplitterDistance = 276;
            this.splitContainer1.TabIndex = 3;
            // 
            // gridDirectInputBindings
            // 
            this.gridDirectInputBindings.AllowUserToAddRows = false;
            this.gridDirectInputBindings.AllowUserToDeleteRows = false;
            this.gridDirectInputBindings.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridDirectInputBindings.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.gridDirectInputBindings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDirectInputBindings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.diLineNum,
            this.diCallback,
            this.diBindingType,
            this.diCockpitItemId,
            this.diComboKey,
            this.diDeviceGuid,
            this.diButtonIndex,
            this.diPOVDirection});
            this.gridDirectInputBindings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDirectInputBindings.Location = new System.Drawing.Point(0, 0);
            this.gridDirectInputBindings.MultiSelect = false;
            this.gridDirectInputBindings.Name = "gridDirectInputBindings";
            this.gridDirectInputBindings.ReadOnly = true;
            this.gridDirectInputBindings.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.gridDirectInputBindings.Size = new System.Drawing.Size(1262, 109);
            this.gridDirectInputBindings.TabIndex = 0;
            // 
            // diLineNum
            // 
            this.diLineNum.HeaderText = "Line Number";
            this.diLineNum.Name = "diLineNum";
            this.diLineNum.ReadOnly = true;
            // 
            // diCallback
            // 
            this.diCallback.HeaderText = "Callback";
            this.diCallback.Name = "diCallback";
            this.diCallback.ReadOnly = true;
            // 
            // diBindingType
            // 
            this.diBindingType.HeaderText = "Binding Type";
            this.diBindingType.Name = "diBindingType";
            this.diBindingType.ReadOnly = true;
            // 
            // diCockpitItemId
            // 
            this.diCockpitItemId.HeaderText = "Cockpit Item ID";
            this.diCockpitItemId.Name = "diCockpitItemId";
            this.diCockpitItemId.ReadOnly = true;
            // 
            // diComboKey
            // 
            this.diComboKey.HeaderText = "Combo Key(s)";
            this.diComboKey.Name = "diComboKey";
            this.diComboKey.ReadOnly = true;
            // 
            // diDeviceGuid
            // 
            this.diDeviceGuid.HeaderText = "Device GUID";
            this.diDeviceGuid.Name = "diDeviceGuid";
            this.diDeviceGuid.ReadOnly = true;
            // 
            // diButtonIndex
            // 
            this.diButtonIndex.HeaderText = "Button Index";
            this.diButtonIndex.Name = "diButtonIndex";
            this.diButtonIndex.ReadOnly = true;
            // 
            // diPOVDirection
            // 
            this.diPOVDirection.HeaderText = "POV Direction";
            this.diPOVDirection.Name = "diPOVDirection";
            this.diPOVDirection.ReadOnly = true;
            // 
            // kbLineNum
            // 
            this.kbLineNum.HeaderText = "Line Number";
            this.kbLineNum.Name = "kbLineNum";
            this.kbLineNum.ReadOnly = true;
            // 
            // kbCallback
            // 
            this.kbCallback.HeaderText = "Callback";
            this.kbCallback.Name = "kbCallback";
            this.kbCallback.ReadOnly = true;
            // 
            // kbDescription
            // 
            this.kbDescription.HeaderText = "Description";
            this.kbDescription.Name = "kbDescription";
            this.kbDescription.ReadOnly = true;
            // 
            // kbCockpitItemId
            // 
            this.kbCockpitItemId.HeaderText = "Cockpit Item ID";
            this.kbCockpitItemId.Name = "kbCockpitItemId";
            this.kbCockpitItemId.ReadOnly = true;
            // 
            // kbComboKey
            // 
            this.kbComboKey.HeaderText = "Combo Key(s)";
            this.kbComboKey.Name = "kbComboKey";
            this.kbComboKey.ReadOnly = true;
            // 
            // kbKey
            // 
            this.kbKey.HeaderText = "Key(s)";
            this.kbKey.Name = "kbKey";
            this.kbKey.ReadOnly = true;
            // 
            // kbUIAccessibility
            // 
            this.kbUIAccessibility.HeaderText = "UI Accessibility";
            this.kbUIAccessibility.Name = "kbUIAccessibility";
            this.kbUIAccessibility.ReadOnly = true;
            // 
            // kbMouseClickableOnly
            // 
            this.kbMouseClickableOnly.HeaderText = "Mouse Clickable Only?";
            this.kbMouseClickableOnly.Name = "kbMouseClickableOnly";
            this.kbMouseClickableOnly.ReadOnly = true;
            this.kbMouseClickableOnly.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.kbMouseClickableOnly.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 435);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "Falcon KeyFile Editor";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridKeyBindings)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridDirectInputBindings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSave;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpAbout;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnOpenFile;
        private System.Windows.Forms.ToolStripButton btnSaveFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.DataGridView gridKeyBindings;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView gridDirectInputBindings;
        private System.Windows.Forms.DataGridViewTextBoxColumn diLineNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn diCallback;
        private System.Windows.Forms.DataGridViewTextBoxColumn diBindingType;
        private System.Windows.Forms.DataGridViewTextBoxColumn diCockpitItemId;
        private System.Windows.Forms.DataGridViewTextBoxColumn diComboKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn diDeviceGuid;
        private System.Windows.Forms.DataGridViewTextBoxColumn diButtonIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn diPOVDirection;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbLineNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbCallback;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbCockpitItemId;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbComboKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn kbUIAccessibility;
        private System.Windows.Forms.DataGridViewCheckBoxColumn kbMouseClickableOnly;
    }
}