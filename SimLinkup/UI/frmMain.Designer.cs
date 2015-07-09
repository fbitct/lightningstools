namespace SimLinkup.UI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionsStart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuActionsStop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.btnOptions = new System.Windows.Forms.ToolStripButton();
            this.nfyTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.mnuTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuTrayStart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuTrayStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuTrayRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuTrayOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuTrayExit = new System.Windows.Forms.ToolStripMenuItem();
            this.panel = new System.Windows.Forms.Panel();
            this.signalsView = new SimLinkup.UI.UserControls.SignalsView();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.mnuTray.SuspendLayout();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.actionsToolStripMenuItem,
            this.mnuTools,
            this.mnuHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1839, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(50, 29);
            this.mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuFileExit.Size = new System.Drawing.Size(176, 30);
            this.mnuFileExit.Text = "E&xit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionsStart,
            this.toolStripMenuItem2,
            this.mnuActionsStop});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(83, 29);
            this.actionsToolStripMenuItem.Text = "&Actions";
            // 
            // mnuActionsStart
            // 
            this.mnuActionsStart.Image = ((System.Drawing.Image)(resources.GetObject("mnuActionsStart.Image")));
            this.mnuActionsStart.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuActionsStart.Name = "mnuActionsStart";
            this.mnuActionsStart.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuActionsStart.Size = new System.Drawing.Size(209, 30);
            this.mnuActionsStart.Text = "St&art";
            this.mnuActionsStart.Click += new System.EventHandler(this.mnuActionsStart_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(206, 6);
            // 
            // mnuActionsStop
            // 
            this.mnuActionsStop.Enabled = false;
            this.mnuActionsStop.Image = ((System.Drawing.Image)(resources.GetObject("mnuActionsStop.Image")));
            this.mnuActionsStop.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuActionsStop.Name = "mnuActionsStop";
            this.mnuActionsStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.mnuActionsStop.Size = new System.Drawing.Size(209, 30);
            this.mnuActionsStop.Text = "St&op";
            this.mnuActionsStop.Click += new System.EventHandler(this.mnuActionsStop_Click);
            // 
            // mnuTools
            // 
            this.mnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuToolsOptions});
            this.mnuTools.Name = "mnuTools";
            this.mnuTools.Size = new System.Drawing.Size(67, 29);
            this.mnuTools.Text = "&Tools";
            // 
            // mnuToolsOptions
            // 
            this.mnuToolsOptions.Image = ((System.Drawing.Image)(resources.GetObject("mnuToolsOptions.Image")));
            this.mnuToolsOptions.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuToolsOptions.Name = "mnuToolsOptions";
            this.mnuToolsOptions.Size = new System.Drawing.Size(168, 30);
            this.mnuToolsOptions.Text = "&Options...";
            this.mnuToolsOptions.Click += new System.EventHandler(this.mnuToolsOptions_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHelpAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(61, 29);
            this.mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Name = "mnuHelpAbout";
            this.mnuHelpAbout.Size = new System.Drawing.Size(146, 30);
            this.mnuHelpAbout.Text = "&About...";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 993);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 21, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1839, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStart,
            this.btnStop,
            this.btnOptions});
            this.toolStrip1.Location = new System.Drawing.Point(0, 33);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1839, 32);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnStart
            // 
            this.btnStart.Image = ((System.Drawing.Image)(resources.GetObject("btnStart.Image")));
            this.btnStart.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(73, 29);
            this.btnStart.Text = "Start";
            this.btnStart.ToolTipText = "Start";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(74, 29);
            this.btnStop.Text = "Stop";
            this.btnStop.ToolTipText = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(100, 29);
            this.btnOptions.Text = "Options";
            this.btnOptions.ToolTipText = "Options";
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // nfyTrayIcon
            // 
            this.nfyTrayIcon.ContextMenuStrip = this.mnuTray;
            this.nfyTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("nfyTrayIcon.Icon")));
            this.nfyTrayIcon.Text = "Sim Linkup";
            this.nfyTrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.nfyTrayIcon_MouseDoubleClick);
            // 
            // mnuTray
            // 
            this.mnuTray.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mnuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTrayStart,
            this.toolStripMenuItem3,
            this.mnuTrayStop,
            this.toolStripMenuItem4,
            this.mnuTrayRestore,
            this.toolStripMenuItem6,
            this.mnuTrayOptions,
            this.toolStripMenuItem5,
            this.mnuTrayExit});
            this.mnuTray.Name = "mnuTray";
            this.mnuTray.Size = new System.Drawing.Size(206, 178);
            // 
            // mnuTrayStart
            // 
            this.mnuTrayStart.Image = ((System.Drawing.Image)(resources.GetObject("mnuTrayStart.Image")));
            this.mnuTrayStart.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuTrayStart.Name = "mnuTrayStart";
            this.mnuTrayStart.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuTrayStart.Size = new System.Drawing.Size(205, 30);
            this.mnuTrayStart.Text = "St&art";
            this.mnuTrayStart.Click += new System.EventHandler(this.mnuTrayStart_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuTrayStop
            // 
            this.mnuTrayStop.Enabled = false;
            this.mnuTrayStop.Image = ((System.Drawing.Image)(resources.GetObject("mnuTrayStop.Image")));
            this.mnuTrayStop.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuTrayStop.Name = "mnuTrayStop";
            this.mnuTrayStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.mnuTrayStop.Size = new System.Drawing.Size(205, 30);
            this.mnuTrayStop.Text = "St&op";
            this.mnuTrayStop.Click += new System.EventHandler(this.mnuTrayStop_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuTrayRestore
            // 
            this.mnuTrayRestore.Name = "mnuTrayRestore";
            this.mnuTrayRestore.Size = new System.Drawing.Size(205, 30);
            this.mnuTrayRestore.Text = "&Restore";
            this.mnuTrayRestore.Click += new System.EventHandler(this.mnuTrayRestore_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuTrayOptions
            // 
            this.mnuTrayOptions.Image = ((System.Drawing.Image)(resources.GetObject("mnuTrayOptions.Image")));
            this.mnuTrayOptions.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuTrayOptions.Name = "mnuTrayOptions";
            this.mnuTrayOptions.Size = new System.Drawing.Size(205, 30);
            this.mnuTrayOptions.Text = "&Options...";
            this.mnuTrayOptions.Click += new System.EventHandler(this.mnuTrayOptions_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(202, 6);
            // 
            // mnuTrayExit
            // 
            this.mnuTrayExit.Name = "mnuTrayExit";
            this.mnuTrayExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuTrayExit.Size = new System.Drawing.Size(205, 30);
            this.mnuTrayExit.Text = "E&xit";
            this.mnuTrayExit.Click += new System.EventHandler(this.mnuTrayExit_Click);
            // 
            // panel
            // 
            this.panel.Controls.Add(this.signalsView);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 65);
            this.panel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(1839, 928);
            this.panel.TabIndex = 3;
            // 
            // signalsView
            // 
            this.signalsView.AutoSize = true;
            this.signalsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.signalsView.Location = new System.Drawing.Point(0, 0);
            this.signalsView.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.signalsView.Name = "signalsView";
            this.signalsView.Signals = null;
            this.signalsView.Size = new System.Drawing.Size(1839, 928);
            this.signalsView.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1839, 1015);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Sim Linkup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mnuTray.ResumeLayout(false);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.ToolStripMenuItem mnuTools;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpAbout;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripButton btnOptions;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuActionsStart;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuActionsStop;
        private System.Windows.Forms.NotifyIcon nfyTrayIcon;
        private System.Windows.Forms.ContextMenuStrip mnuTray;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayStart;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayStop;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayOptions;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayExit;
        private System.Windows.Forms.ToolStripMenuItem mnuTrayRestore;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.Panel panel;
        private UserControls.SignalsView signalsView;

    }
}

