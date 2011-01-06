namespace SimLinkup.UI
{
    partial class frmOptions
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabStartup = new System.Windows.Forms.TabPage();
            this.gbStartupOptions = new System.Windows.Forms.GroupBox();
            this.chkMinimizeWhenStarted = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToSystemTray = new System.Windows.Forms.CheckBox();
            this.chkStartAutomaticallyWhenLaunched = new System.Windows.Forms.CheckBox();
            this.chkLaunchAtSystemStartup = new System.Windows.Forms.CheckBox();
            this.tabPlugins = new System.Windows.Forms.TabPage();
            this.tabPluginsSubtabs = new System.Windows.Forms.TabControl();
            this.tabHardwareSupport = new System.Windows.Forms.TabPage();
            this.lblHardwareSupportModules = new System.Windows.Forms.Label();
            this.tabSimSupport = new System.Windows.Forms.TabPage();
            this.lblSimSupportModules = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStartup.SuspendLayout();
            this.gbStartupOptions.SuspendLayout();
            this.tabPlugins.SuspendLayout();
            this.tabPluginsSubtabs.SuspendLayout();
            this.tabHardwareSupport.SuspendLayout();
            this.tabSimSupport.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(12, 2);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "&OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(93, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cmdOK);
            this.splitContainer1.Panel2.Controls.Add(this.cmdCancel);
            this.splitContainer1.Size = new System.Drawing.Size(574, 283);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabStartup);
            this.tabControl1.Controls.Add(this.tabPlugins);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(574, 250);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.AutoScroll = true;
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(566, 224);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabStartup
            // 
            this.tabStartup.AutoScroll = true;
            this.tabStartup.Controls.Add(this.gbStartupOptions);
            this.tabStartup.Location = new System.Drawing.Point(4, 22);
            this.tabStartup.Name = "tabStartup";
            this.tabStartup.Padding = new System.Windows.Forms.Padding(3);
            this.tabStartup.Size = new System.Drawing.Size(566, 224);
            this.tabStartup.TabIndex = 1;
            this.tabStartup.Text = "Startup";
            this.tabStartup.UseVisualStyleBackColor = true;
            // 
            // gbStartupOptions
            // 
            this.gbStartupOptions.Controls.Add(this.chkMinimizeWhenStarted);
            this.gbStartupOptions.Controls.Add(this.chkMinimizeToSystemTray);
            this.gbStartupOptions.Controls.Add(this.chkStartAutomaticallyWhenLaunched);
            this.gbStartupOptions.Controls.Add(this.chkLaunchAtSystemStartup);
            this.gbStartupOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStartupOptions.Location = new System.Drawing.Point(3, 3);
            this.gbStartupOptions.Name = "gbStartupOptions";
            this.gbStartupOptions.Size = new System.Drawing.Size(560, 218);
            this.gbStartupOptions.TabIndex = 5;
            this.gbStartupOptions.TabStop = false;
            this.gbStartupOptions.Text = "Startup Options";
            // 
            // chkMinimizeWhenStarted
            // 
            this.chkMinimizeWhenStarted.AutoSize = true;
            this.chkMinimizeWhenStarted.Location = new System.Drawing.Point(6, 88);
            this.chkMinimizeWhenStarted.Name = "chkMinimizeWhenStarted";
            this.chkMinimizeWhenStarted.Size = new System.Drawing.Size(130, 17);
            this.chkMinimizeWhenStarted.TabIndex = 3;
            this.chkMinimizeWhenStarted.Text = "&Minimize when started";
            this.chkMinimizeWhenStarted.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToSystemTray
            // 
            this.chkMinimizeToSystemTray.AutoSize = true;
            this.chkMinimizeToSystemTray.Location = new System.Drawing.Point(6, 65);
            this.chkMinimizeToSystemTray.Name = "chkMinimizeToSystemTray";
            this.chkMinimizeToSystemTray.Size = new System.Drawing.Size(139, 17);
            this.chkMinimizeToSystemTray.TabIndex = 2;
            this.chkMinimizeToSystemTray.Text = "&Minimize to System Tray";
            this.chkMinimizeToSystemTray.UseVisualStyleBackColor = true;
            // 
            // chkStartAutomaticallyWhenLaunched
            // 
            this.chkStartAutomaticallyWhenLaunched.AutoSize = true;
            this.chkStartAutomaticallyWhenLaunched.Location = new System.Drawing.Point(6, 42);
            this.chkStartAutomaticallyWhenLaunched.Name = "chkStartAutomaticallyWhenLaunched";
            this.chkStartAutomaticallyWhenLaunched.Size = new System.Drawing.Size(188, 17);
            this.chkStartAutomaticallyWhenLaunched.TabIndex = 1;
            this.chkStartAutomaticallyWhenLaunched.Text = "&Start automatically when launched";
            this.chkStartAutomaticallyWhenLaunched.UseVisualStyleBackColor = true;
            // 
            // chkLaunchAtSystemStartup
            // 
            this.chkLaunchAtSystemStartup.AutoSize = true;
            this.chkLaunchAtSystemStartup.Location = new System.Drawing.Point(6, 19);
            this.chkLaunchAtSystemStartup.Name = "chkLaunchAtSystemStartup";
            this.chkLaunchAtSystemStartup.Size = new System.Drawing.Size(148, 17);
            this.chkLaunchAtSystemStartup.TabIndex = 0;
            this.chkLaunchAtSystemStartup.Text = "&Launch at System Startup";
            this.chkLaunchAtSystemStartup.UseVisualStyleBackColor = true;
            // 
            // tabPlugins
            // 
            this.tabPlugins.Controls.Add(this.tabPluginsSubtabs);
            this.tabPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPlugins.Name = "tabPlugins";
            this.tabPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlugins.Size = new System.Drawing.Size(566, 224);
            this.tabPlugins.TabIndex = 2;
            this.tabPlugins.Text = "Plug-ins";
            this.tabPlugins.UseVisualStyleBackColor = true;
            // 
            // tabPluginsSubtabs
            // 
            this.tabPluginsSubtabs.Controls.Add(this.tabHardwareSupport);
            this.tabPluginsSubtabs.Controls.Add(this.tabSimSupport);
            this.tabPluginsSubtabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPluginsSubtabs.Location = new System.Drawing.Point(3, 3);
            this.tabPluginsSubtabs.Name = "tabPluginsSubtabs";
            this.tabPluginsSubtabs.SelectedIndex = 0;
            this.tabPluginsSubtabs.Size = new System.Drawing.Size(560, 218);
            this.tabPluginsSubtabs.TabIndex = 0;
            // 
            // tabHardwareSupport
            // 
            this.tabHardwareSupport.AutoScroll = true;
            this.tabHardwareSupport.Controls.Add(this.lblHardwareSupportModules);
            this.tabHardwareSupport.Location = new System.Drawing.Point(4, 22);
            this.tabHardwareSupport.Name = "tabHardwareSupport";
            this.tabHardwareSupport.Padding = new System.Windows.Forms.Padding(3);
            this.tabHardwareSupport.Size = new System.Drawing.Size(552, 192);
            this.tabHardwareSupport.TabIndex = 0;
            this.tabHardwareSupport.Text = "Hardware Support Modules";
            this.tabHardwareSupport.UseVisualStyleBackColor = true;
            // 
            // lblHardwareSupportModules
            // 
            this.lblHardwareSupportModules.AutoSize = true;
            this.lblHardwareSupportModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHardwareSupportModules.Location = new System.Drawing.Point(3, 3);
            this.lblHardwareSupportModules.Name = "lblHardwareSupportModules";
            this.lblHardwareSupportModules.Size = new System.Drawing.Size(0, 13);
            this.lblHardwareSupportModules.TabIndex = 0;
            // 
            // tabSimSupport
            // 
            this.tabSimSupport.AutoScroll = true;
            this.tabSimSupport.Controls.Add(this.lblSimSupportModules);
            this.tabSimSupport.Location = new System.Drawing.Point(4, 22);
            this.tabSimSupport.Name = "tabSimSupport";
            this.tabSimSupport.Padding = new System.Windows.Forms.Padding(3);
            this.tabSimSupport.Size = new System.Drawing.Size(552, 192);
            this.tabSimSupport.TabIndex = 1;
            this.tabSimSupport.Text = "Sim Support Modules";
            this.tabSimSupport.UseVisualStyleBackColor = true;
            // 
            // lblSimSupportModules
            // 
            this.lblSimSupportModules.AutoSize = true;
            this.lblSimSupportModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSimSupportModules.Location = new System.Drawing.Point(3, 3);
            this.lblSimSupportModules.Name = "lblSimSupportModules";
            this.lblSimSupportModules.Size = new System.Drawing.Size(0, 13);
            this.lblSimSupportModules.TabIndex = 0;
            // 
            // frmOptions
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(574, 283);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Options";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabStartup.ResumeLayout(false);
            this.gbStartupOptions.ResumeLayout(false);
            this.gbStartupOptions.PerformLayout();
            this.tabPlugins.ResumeLayout(false);
            this.tabPluginsSubtabs.ResumeLayout(false);
            this.tabHardwareSupport.ResumeLayout(false);
            this.tabHardwareSupport.PerformLayout();
            this.tabSimSupport.ResumeLayout(false);
            this.tabSimSupport.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabStartup;
        private System.Windows.Forms.TabPage tabPlugins;
        private System.Windows.Forms.TabControl tabPluginsSubtabs;
        private System.Windows.Forms.TabPage tabHardwareSupport;
        private System.Windows.Forms.TabPage tabSimSupport;
        private System.Windows.Forms.GroupBox gbStartupOptions;
        private System.Windows.Forms.CheckBox chkMinimizeWhenStarted;
        private System.Windows.Forms.CheckBox chkMinimizeToSystemTray;
        private System.Windows.Forms.CheckBox chkStartAutomaticallyWhenLaunched;
        private System.Windows.Forms.CheckBox chkLaunchAtSystemStartup;
        private System.Windows.Forms.Label lblHardwareSupportModules;
        private System.Windows.Forms.Label lblSimSupportModules;
    }
}