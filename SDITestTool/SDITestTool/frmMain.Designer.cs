namespace SDITestTool
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
            this.tcTabs = new System.Windows.Forms.TabControl();
            this.lblDeviceAddress = new System.Windows.Forms.Label();
            this.tabDoaRaw = new System.Windows.Forms.TabPage();
            this.gbDoa = new System.Windows.Forms.GroupBox();
            this.lblDoaSubAddr = new System.Windows.Forms.Label();
            this.txtDoaSubAddr = new System.Windows.Forms.TextBox();
            this.lblDoaDevAddr = new System.Windows.Forms.Label();
            this.btnSendDoa = new System.Windows.Forms.Button();
            this.txtDoaDevAddr = new System.Windows.Forms.TextBox();
            this.lbDoalDataByte = new System.Windows.Forms.Label();
            this.txtDoaDataByte = new System.Windows.Forms.TextBox();
            this.epErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.cbSerialPort = new System.Windows.Forms.ComboBox();
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.lblFirmwareVersion = new System.Windows.Forms.Label();
            this.tcTabs.SuspendLayout();
            this.tabDoaRaw.SuspendLayout();
            this.gbDoa.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // tcTabs
            // 
            this.tcTabs.Controls.Add(this.tabDoaRaw);
            this.tcTabs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tcTabs.Location = new System.Drawing.Point(0, 33);
            this.tcTabs.Name = "tcTabs";
            this.tcTabs.SelectedIndex = 0;
            this.tcTabs.Size = new System.Drawing.Size(926, 563);
            this.tcTabs.TabIndex = 0;
            this.tcTabs.SelectedIndexChanged += new System.EventHandler(this.tcTabs_SelectedIndexChanged);

            // 
            // tabDoaRaw
            // 
            this.tabDoaRaw.Controls.Add(this.gbDoa);
            this.tabDoaRaw.Location = new System.Drawing.Point(4, 22);
            this.tabDoaRaw.Name = "tabDoaRaw";
            this.tabDoaRaw.Size = new System.Drawing.Size(918, 537);
            this.tabDoaRaw.TabIndex = 6;
            this.tabDoaRaw.Text = "DOA";
            this.tabDoaRaw.UseVisualStyleBackColor = true;
            // 
            // gbDoa
            // 
            this.gbDoa.Controls.Add(this.lblDoaSubAddr);
            this.gbDoa.Controls.Add(this.txtDoaSubAddr);
            this.gbDoa.Controls.Add(this.lblDoaDevAddr);
            this.gbDoa.Controls.Add(this.btnSendDoa);
            this.gbDoa.Controls.Add(this.txtDoaDevAddr);
            this.gbDoa.Controls.Add(this.lbDoalDataByte);
            this.gbDoa.Controls.Add(this.txtDoaDataByte);
            this.gbDoa.Location = new System.Drawing.Point(8, 6);
            this.gbDoa.Name = "gbDoa";
            this.gbDoa.Size = new System.Drawing.Size(326, 176);
            this.gbDoa.TabIndex = 0;
            this.gbDoa.TabStop = false;
            this.gbDoa.Text = "Digital Output Type A Raw Data Control";
            // 
            // lblDoaSubAddr
            // 
            this.lblDoaSubAddr.AutoSize = true;
            this.lblDoaSubAddr.Location = new System.Drawing.Point(25, 51);
            this.lblDoaSubAddr.Name = "lblDoaSubAddr";
            this.lblDoaSubAddr.Size = new System.Drawing.Size(66, 13);
            this.lblDoaSubAddr.TabIndex = 2;
            this.lblDoaSubAddr.Text = "S&ubaddress:";
            this.lblDoaSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDoaSubAddr
            // 
            this.txtDoaSubAddr.Location = new System.Drawing.Point(97, 48);
            this.txtDoaSubAddr.MaxLength = 4;
            this.txtDoaSubAddr.Name = "txtDoaSubAddr";
            this.txtDoaSubAddr.Size = new System.Drawing.Size(46, 20);
            this.txtDoaSubAddr.TabIndex = 3;
            this.txtDoaSubAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDoaSubAddr.Leave += new System.EventHandler(this.txtDoaSubAddr_Leave);
            // 
            // lblDoaDevAddr
            // 
            this.lblDoaDevAddr.AutoSize = true;
            this.lblDoaDevAddr.Location = new System.Drawing.Point(6, 25);
            this.lblDoaDevAddr.Name = "lblDoaDevAddr";
            this.lblDoaDevAddr.Size = new System.Drawing.Size(85, 13);
            this.lblDoaDevAddr.TabIndex = 0;
            this.lblDoaDevAddr.Text = "Device &Address:";
            this.lblDoaDevAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSendDoa
            // 
            this.btnSendDoa.Location = new System.Drawing.Point(10, 139);
            this.btnSendDoa.Name = "btnSendDoa";
            this.btnSendDoa.Size = new System.Drawing.Size(75, 23);
            this.btnSendDoa.TabIndex = 6;
            this.btnSendDoa.Text = "&Send";
            this.btnSendDoa.UseVisualStyleBackColor = true;
            this.btnSendDoa.Click += new System.EventHandler(this.btnSendDoa_Click);
            // 
            // txtDoaDevAddr
            // 
            this.txtDoaDevAddr.Location = new System.Drawing.Point(97, 22);
            this.txtDoaDevAddr.MaxLength = 4;
            this.txtDoaDevAddr.Name = "txtDoaDevAddr";
            this.txtDoaDevAddr.Size = new System.Drawing.Size(46, 20);
            this.txtDoaDevAddr.TabIndex = 1;
            this.txtDoaDevAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDoaDevAddr.Leave += new System.EventHandler(this.txtDoaDevAddr_Leave);
            // 
            // lbDoalDataByte
            // 
            this.lbDoalDataByte.AutoSize = true;
            this.lbDoalDataByte.Location = new System.Drawing.Point(34, 77);
            this.lbDoalDataByte.Name = "lbDoalDataByte";
            this.lbDoalDataByte.Size = new System.Drawing.Size(57, 13);
            this.lbDoalDataByte.TabIndex = 4;
            this.lbDoalDataByte.Text = "Data &Byte:";
            this.lbDoalDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDoaDataByte
            // 
            this.txtDoaDataByte.Location = new System.Drawing.Point(97, 74);
            this.txtDoaDataByte.MaxLength = 4;
            this.txtDoaDataByte.Name = "txtDoaDataByte";
            this.txtDoaDataByte.Size = new System.Drawing.Size(46, 20);
            this.txtDoaDataByte.TabIndex = 5;
            this.txtDoaDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDoaDataByte.Leave += new System.EventHandler(this.txtDoaDataByte_Leave);

            // 
            // epErrorProvider
            // 
            this.epErrorProvider.ContainerControl = this;
            // 
            // cbSerialPort
            // 
            this.cbSerialPort.FormattingEnabled = true;
            this.cbSerialPort.Location = new System.Drawing.Point(73, 6);
            this.cbSerialPort.Name = "cbSerialPort";
            this.cbSerialPort.Size = new System.Drawing.Size(121, 21);
            this.cbSerialPort.TabIndex = 1;
            this.cbSerialPort.SelectedIndexChanged += new System.EventHandler(this.cbSerialPort_SelectedIndexChanged);
            // 
            // lblSerialPort
            // 
            this.lblSerialPort.AutoSize = true;
            this.lblSerialPort.Location = new System.Drawing.Point(12, 9);
            this.lblSerialPort.Name = "lblSerialPort";
            this.lblSerialPort.Size = new System.Drawing.Size(58, 13);
            this.lblSerialPort.TabIndex = 2;
            this.lblSerialPort.Text = "Serial &Port:";
            // 
            // lblFirmwareVersion
            // 
            this.lblFirmwareVersion.AutoSize = true;
            this.lblFirmwareVersion.Location = new System.Drawing.Point(216, 9);
            this.lblFirmwareVersion.Name = "lblFirmwareVersion";
            this.lblFirmwareVersion.Size = new System.Drawing.Size(90, 13);
            this.lblFirmwareVersion.TabIndex = 3;
            this.lblFirmwareVersion.Text = "PHCC Firmware Version:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(926, 596);
            this.Controls.Add(this.lblFirmwareVersion);
            this.Controls.Add(this.lblSerialPort);
            this.Controls.Add(this.cbSerialPort);
            this.Controls.Add(this.tcTabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SDI Test Tool";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.tcTabs.ResumeLayout(false);

            this.tabDoaRaw.ResumeLayout(false);
            this.gbDoa.ResumeLayout(false);
            this.gbDoa.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tcTabs;
        private System.Windows.Forms.Label lblDeviceAddress;
        private System.Windows.Forms.ErrorProvider epErrorProvider;
        private System.Windows.Forms.Label lblSerialPort;
        private System.Windows.Forms.ComboBox cbSerialPort;
        private System.Windows.Forms.TabPage tabDoaRaw;
        private System.Windows.Forms.Label lblFirmwareVersion;
        private System.Windows.Forms.GroupBox gbDoa;
        private System.Windows.Forms.Label lblDoaSubAddr;
        private System.Windows.Forms.TextBox txtDoaSubAddr;
        private System.Windows.Forms.Label lblDoaDevAddr;
        private System.Windows.Forms.Button btnSendDoa;
        private System.Windows.Forms.TextBox txtDoaDevAddr;
        private System.Windows.Forms.Label lbDoalDataByte;
        private System.Windows.Forms.TextBox txtDoaDataByte;



    }
}

