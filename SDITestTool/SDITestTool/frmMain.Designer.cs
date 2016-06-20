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
            this.tabRawDataControl = new System.Windows.Forms.TabPage();
            this.gbRaw = new System.Windows.Forms.GroupBox();
            this.lblSubAddr = new System.Windows.Forms.Label();
            this.txtSubAddr = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblDataByte = new System.Windows.Forms.Label();
            this.txtDataByte = new System.Windows.Forms.TextBox();
            this.lblDeviceAddress = new System.Windows.Forms.Label();
            this.epErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.cbSerialPort = new System.Windows.Forms.ComboBox();
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.lblIdentification = new System.Windows.Forms.Label();
            this.tcTabs.SuspendLayout();
            this.tabRawDataControl.SuspendLayout();
            this.gbRaw.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // tcTabs
            // 
            this.tcTabs.Controls.Add(this.tabRawDataControl);
            this.tcTabs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tcTabs.Location = new System.Drawing.Point(0, 63);
            this.tcTabs.Margin = new System.Windows.Forms.Padding(6);
            this.tcTabs.Name = "tcTabs";
            this.tcTabs.SelectedIndex = 0;
            this.tcTabs.Size = new System.Drawing.Size(1852, 1083);
            this.tcTabs.TabIndex = 0;
            this.tcTabs.SelectedIndexChanged += new System.EventHandler(this.tcTabs_SelectedIndexChanged);
            // 
            // tabRawDataControl
            // 
            this.tabRawDataControl.Controls.Add(this.gbRaw);
            this.tabRawDataControl.Location = new System.Drawing.Point(4, 34);
            this.tabRawDataControl.Margin = new System.Windows.Forms.Padding(6);
            this.tabRawDataControl.Name = "tabRawDataControl";
            this.tabRawDataControl.Size = new System.Drawing.Size(1844, 1045);
            this.tabRawDataControl.TabIndex = 6;
            this.tabRawDataControl.Text = "Raw Data Control";
            this.tabRawDataControl.UseVisualStyleBackColor = true;
            // 
            // gbRaw
            // 
            this.gbRaw.Controls.Add(this.lblSubAddr);
            this.gbRaw.Controls.Add(this.txtSubAddr);
            this.gbRaw.Controls.Add(this.btnSend);
            this.gbRaw.Controls.Add(this.lblDataByte);
            this.gbRaw.Controls.Add(this.txtDataByte);
            this.gbRaw.Location = new System.Drawing.Point(17, 12);
            this.gbRaw.Margin = new System.Windows.Forms.Padding(6);
            this.gbRaw.Name = "gbRaw";
            this.gbRaw.Padding = new System.Windows.Forms.Padding(6);
            this.gbRaw.Size = new System.Drawing.Size(652, 338);
            this.gbRaw.TabIndex = 0;
            this.gbRaw.TabStop = false;
            this.gbRaw.Text = "Raw Data Control";
            // 
            // lblSubAddr
            // 
            this.lblSubAddr.AutoSize = true;
            this.lblSubAddr.Location = new System.Drawing.Point(50, 98);
            this.lblSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSubAddr.Name = "lblSubAddr";
            this.lblSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblSubAddr.TabIndex = 2;
            this.lblSubAddr.Text = "S&ubaddress:";
            this.lblSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSubAddr
            // 
            this.txtSubAddr.Location = new System.Drawing.Point(194, 92);
            this.txtSubAddr.Margin = new System.Windows.Forms.Padding(6);
            this.txtSubAddr.MaxLength = 4;
            this.txtSubAddr.Name = "txtSubAddr";
            this.txtSubAddr.Size = new System.Drawing.Size(88, 31);
            this.txtSubAddr.TabIndex = 3;
            this.txtSubAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSubAddr.Leave += new System.EventHandler(this.txtSubAddr_Leave);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(20, 267);
            this.btnSend.Margin = new System.Windows.Forms.Padding(6);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(150, 44);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblDataByte
            // 
            this.lblDataByte.AutoSize = true;
            this.lblDataByte.Location = new System.Drawing.Point(68, 148);
            this.lblDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDataByte.Name = "lblDataByte";
            this.lblDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblDataByte.TabIndex = 4;
            this.lblDataByte.Text = "Data &Byte:";
            this.lblDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDataByte
            // 
            this.txtDataByte.Location = new System.Drawing.Point(194, 142);
            this.txtDataByte.Margin = new System.Windows.Forms.Padding(6);
            this.txtDataByte.MaxLength = 4;
            this.txtDataByte.Name = "txtDataByte";
            this.txtDataByte.Size = new System.Drawing.Size(88, 31);
            this.txtDataByte.TabIndex = 5;
            this.txtDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDataByte.Leave += new System.EventHandler(this.txtDataByte_Leave);
            // 
            // lblDeviceAddress
            // 
            this.lblDeviceAddress.Location = new System.Drawing.Point(0, 0);
            this.lblDeviceAddress.Name = "lblDeviceAddress";
            this.lblDeviceAddress.Size = new System.Drawing.Size(100, 23);
            this.lblDeviceAddress.TabIndex = 0;
            // 
            // epErrorProvider
            // 
            this.epErrorProvider.ContainerControl = this;
            // 
            // cbSerialPort
            // 
            this.cbSerialPort.FormattingEnabled = true;
            this.cbSerialPort.Location = new System.Drawing.Point(146, 12);
            this.cbSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbSerialPort.Name = "cbSerialPort";
            this.cbSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbSerialPort.TabIndex = 1;
            this.cbSerialPort.SelectedIndexChanged += new System.EventHandler(this.cbSerialPort_SelectedIndexChanged);
            // 
            // lblSerialPort
            // 
            this.lblSerialPort.AutoSize = true;
            this.lblSerialPort.Location = new System.Drawing.Point(24, 17);
            this.lblSerialPort.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSerialPort.Name = "lblSerialPort";
            this.lblSerialPort.Size = new System.Drawing.Size(118, 25);
            this.lblSerialPort.TabIndex = 2;
            this.lblSerialPort.Text = "Serial &Port:";
            // 
            // lblIdentification
            // 
            this.lblIdentification.AutoSize = true;
            this.lblIdentification.Location = new System.Drawing.Point(432, 17);
            this.lblIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblIdentification.Name = "lblIdentification";
            this.lblIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblIdentification.TabIndex = 3;
            this.lblIdentification.Text = "Identification:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1852, 1146);
            this.Controls.Add(this.lblIdentification);
            this.Controls.Add(this.lblSerialPort);
            this.Controls.Add(this.cbSerialPort);
            this.Controls.Add(this.tcTabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SDI Test Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tcTabs.ResumeLayout(false);
            this.tabRawDataControl.ResumeLayout(false);
            this.gbRaw.ResumeLayout(false);
            this.gbRaw.PerformLayout();
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
        private System.Windows.Forms.TabPage tabRawDataControl;
        private System.Windows.Forms.Label lblIdentification;
        private System.Windows.Forms.GroupBox gbRaw;
        private System.Windows.Forms.Label lblSubAddr;
        private System.Windows.Forms.TextBox txtSubAddr;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblDataByte;
        private System.Windows.Forms.TextBox txtDataByte;



    }
}

