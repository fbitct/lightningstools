namespace ADITestTool
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
            this.lblDeviceAddress = new System.Windows.Forms.Label();
            this.epErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbRawDataControl = new System.Windows.Forms.GroupBox();
            this.lblSubAddr = new System.Windows.Forms.Label();
            this.txtSubAddr = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblDataByte = new System.Windows.Forms.Label();
            this.txtDataByte = new System.Windows.Forms.TextBox();
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.cbSerialPort = new System.Windows.Forms.ComboBox();
            this.lblIdentification = new System.Windows.Forms.Label();

            this.gbMain = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabRawData = new System.Windows.Forms.TabPage();

            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.gbRawDataControl.SuspendLayout();
            this.gbMain.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabRawData.SuspendLayout();
            this.SuspendLayout();
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
            // gbRawDataControl
            // 
            this.gbRawDataControl.Controls.Add(this.lblSubAddr);
            this.gbRawDataControl.Controls.Add(this.txtSubAddr);
            this.gbRawDataControl.Controls.Add(this.btnSend);
            this.gbRawDataControl.Controls.Add(this.lblDataByte);
            this.gbRawDataControl.Controls.Add(this.txtDataByte);
            this.gbRawDataControl.Location = new System.Drawing.Point(6, 6);
            this.gbRawDataControl.Margin = new System.Windows.Forms.Padding(6);
            this.gbRawDataControl.Name = "gbRawDataControl";
            this.gbRawDataControl.Padding = new System.Windows.Forms.Padding(6);
            this.gbRawDataControl.Size = new System.Drawing.Size(304, 293);
            this.gbRawDataControl.TabIndex = 0;
            this.gbRawDataControl.TabStop = false;
            this.gbRawDataControl.Text = "Raw Data Control";
            // 
            // lblSubAddr
            // 
            this.lblSubAddr.AutoSize = true;
            this.lblSubAddr.Location = new System.Drawing.Point(37, 118);
            this.lblSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSubAddr.Name = "lblSubAddr";
            this.lblSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblSubAddr.TabIndex = 2;
            this.lblSubAddr.Text = "S&ubaddress:";
            this.lblSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSubAddr
            // 
            this.txtSubAddr.Location = new System.Drawing.Point(181, 112);
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
            this.btnSend.Location = new System.Drawing.Point(42, 224);
            this.btnSend.Margin = new System.Windows.Forms.Padding(6);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(227, 44);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblDataByte
            // 
            this.lblDataByte.AutoSize = true;
            this.lblDataByte.Location = new System.Drawing.Point(55, 168);
            this.lblDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDataByte.Name = "lblDataByte";
            this.lblDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblDataByte.TabIndex = 4;
            this.lblDataByte.Text = "Data &Byte:";
            this.lblDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDataByte
            // 
            this.txtDataByte.Location = new System.Drawing.Point(181, 162);
            this.txtDataByte.Margin = new System.Windows.Forms.Padding(6);
            this.txtDataByte.MaxLength = 4;
            this.txtDataByte.Name = "txtDataByte";
            this.txtDataByte.Size = new System.Drawing.Size(88, 31);
            this.txtDataByte.TabIndex = 5;
            this.txtDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDataByte.Leave += new System.EventHandler(this.txtDataByte_Leave);
            // 
            // lblSerialPort
            // 
            this.lblSerialPort.AutoSize = true;
            this.lblSerialPort.Location = new System.Drawing.Point(10, 17);
            this.lblSerialPort.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSerialPort.Name = "lblSerialPort";
            this.lblSerialPort.Size = new System.Drawing.Size(118, 25);
            this.lblSerialPort.TabIndex = 5;
            this.lblSerialPort.Text = "Serial &Port:";
            // 
            // cbSerialPort
            // 
            this.cbSerialPort.FormattingEnabled = true;
            this.cbSerialPort.Location = new System.Drawing.Point(140, 13);
            this.cbSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbSerialPort.Name = "cbSerialPort";
            this.cbSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbSerialPort.TabIndex = 4;
            this.cbSerialPort.SelectedIndexChanged += new System.EventHandler(this.cbSerialPort_SelectedIndexChanged);
            // 
            // lblIdentification
            // 
            this.lblIdentification.AutoSize = true;
            this.lblIdentification.Location = new System.Drawing.Point(432, 17);
            this.lblIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblIdentification.Name = "lblIdentification";
            this.lblIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblIdentification.TabIndex = 6;
            this.lblIdentification.Text = "Identification:";
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.tabControl1);
            this.gbMain.Location = new System.Drawing.Point(3, 51);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(1060, 948);
            this.gbMain.TabIndex = 11;
            this.gbMain.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabRawData);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1054, 918);
            this.tabControl1.TabIndex = 0;
            // 
            // tabRawData
            // 
            this.tabRawData.Controls.Add(this.gbRawDataControl);
            this.tabRawData.Location = new System.Drawing.Point(4, 34);
            this.tabRawData.Name = "tabRawData";
            this.tabRawData.Size = new System.Drawing.Size(1046, 880);
            this.tabRawData.TabIndex = 4;
            this.tabRawData.Text = "Raw Data";
            this.tabRawData.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1067, 997);
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.lblSerialPort);
            this.Controls.Add(this.cbSerialPort);
            this.Controls.Add(this.lblIdentification);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADI Test Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).EndInit();
            this.gbRawDataControl.ResumeLayout(false);
            this.gbRawDataControl.PerformLayout();
            this.gbMain.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabRawData.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDeviceAddress;
        private System.Windows.Forms.ErrorProvider epErrorProvider;
        private System.Windows.Forms.Label lblSerialPort;
        private System.Windows.Forms.ComboBox cbSerialPort;
        private System.Windows.Forms.Label lblIdentification;
        private System.Windows.Forms.GroupBox gbRawDataControl;
        private System.Windows.Forms.Label lblSubAddr;
        private System.Windows.Forms.TextBox txtSubAddr;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblDataByte;
        private System.Windows.Forms.TextBox txtDataByte;

        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabRawData;
    }
}

