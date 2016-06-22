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
            this.gbPitchRawDataControl = new System.Windows.Forms.GroupBox();
            this.lblPitchSubAddr = new System.Windows.Forms.Label();
            this.txtPitchSubAddr = new System.Windows.Forms.TextBox();
            this.btnPitchSendRaw = new System.Windows.Forms.Button();
            this.lblPitchDataByte = new System.Windows.Forms.Label();
            this.txtPitchDataByte = new System.Windows.Forms.TextBox();
            this.lblPitchDeviceSerialPort = new System.Windows.Forms.Label();
            this.cbPitchDeviceSerialPort = new System.Windows.Forms.ComboBox();
            this.lblPitchDeviceIdentification = new System.Windows.Forms.Label();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabRawData = new System.Windows.Forms.TabPage();
            this.gbRollRawDataControl = new System.Windows.Forms.GroupBox();
            this.lblRollSubAddr = new System.Windows.Forms.Label();
            this.txtRollSubAddr = new System.Windows.Forms.TextBox();
            this.btnRollSendRaw = new System.Windows.Forms.Button();
            this.lblRollDataByte = new System.Windows.Forms.Label();
            this.txtRollDataByte = new System.Windows.Forms.TextBox();
            this.lblRollDeviceSerialPort = new System.Windows.Forms.Label();
            this.cbRollDeviceSerialPort = new System.Windows.Forms.ComboBox();
            this.lblRollDeviceIdentification = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.gbPitchRawDataControl.SuspendLayout();
            this.gbMain.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabRawData.SuspendLayout();
            this.gbRollRawDataControl.SuspendLayout();
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
            // gbPitchRawDataControl
            // 
            this.gbPitchRawDataControl.Controls.Add(this.lblPitchSubAddr);
            this.gbPitchRawDataControl.Controls.Add(this.txtPitchSubAddr);
            this.gbPitchRawDataControl.Controls.Add(this.btnPitchSendRaw);
            this.gbPitchRawDataControl.Controls.Add(this.lblPitchDataByte);
            this.gbPitchRawDataControl.Controls.Add(this.txtPitchDataByte);
            this.gbPitchRawDataControl.Location = new System.Drawing.Point(6, 6);
            this.gbPitchRawDataControl.Margin = new System.Windows.Forms.Padding(6);
            this.gbPitchRawDataControl.Name = "gbPitchRawDataControl";
            this.gbPitchRawDataControl.Padding = new System.Windows.Forms.Padding(6);
            this.gbPitchRawDataControl.Size = new System.Drawing.Size(304, 293);
            this.gbPitchRawDataControl.TabIndex = 0;
            this.gbPitchRawDataControl.TabStop = false;
            this.gbPitchRawDataControl.Text = "Raw Data Control (Pitch)";
            // 
            // lblPitchSubAddr
            // 
            this.lblPitchSubAddr.AutoSize = true;
            this.lblPitchSubAddr.Location = new System.Drawing.Point(37, 118);
            this.lblPitchSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPitchSubAddr.Name = "lblPitchSubAddr";
            this.lblPitchSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblPitchSubAddr.TabIndex = 2;
            this.lblPitchSubAddr.Text = "S&ubaddress:";
            this.lblPitchSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPitchSubAddr
            // 
            this.txtPitchSubAddr.Location = new System.Drawing.Point(181, 112);
            this.txtPitchSubAddr.Margin = new System.Windows.Forms.Padding(6);
            this.txtPitchSubAddr.MaxLength = 4;
            this.txtPitchSubAddr.Name = "txtPitchSubAddr";
            this.txtPitchSubAddr.Size = new System.Drawing.Size(88, 31);
            this.txtPitchSubAddr.TabIndex = 3;
            this.txtPitchSubAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPitchSubAddr.Leave += new System.EventHandler(this.txtPitchSubAddr_Leave);
            // 
            // btnPitchSendRaw
            // 
            this.btnPitchSendRaw.Location = new System.Drawing.Point(42, 224);
            this.btnPitchSendRaw.Margin = new System.Windows.Forms.Padding(6);
            this.btnPitchSendRaw.Name = "btnPitchSendRaw";
            this.btnPitchSendRaw.Size = new System.Drawing.Size(227, 44);
            this.btnPitchSendRaw.TabIndex = 6;
            this.btnPitchSendRaw.Text = "&Send";
            this.btnPitchSendRaw.UseVisualStyleBackColor = true;
            this.btnPitchSendRaw.Click += new System.EventHandler(this.btnPitchSendRaw_Click);
            // 
            // lblPitchDataByte
            // 
            this.lblPitchDataByte.AutoSize = true;
            this.lblPitchDataByte.Location = new System.Drawing.Point(55, 168);
            this.lblPitchDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPitchDataByte.Name = "lblPitchDataByte";
            this.lblPitchDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblPitchDataByte.TabIndex = 4;
            this.lblPitchDataByte.Text = "Data &Byte:";
            this.lblPitchDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPitchDataByte
            // 
            this.txtPitchDataByte.Location = new System.Drawing.Point(181, 162);
            this.txtPitchDataByte.Margin = new System.Windows.Forms.Padding(6);
            this.txtPitchDataByte.MaxLength = 4;
            this.txtPitchDataByte.Name = "txtPitchDataByte";
            this.txtPitchDataByte.Size = new System.Drawing.Size(88, 31);
            this.txtPitchDataByte.TabIndex = 5;
            this.txtPitchDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPitchDataByte.Leave += new System.EventHandler(this.txtPitchDataByte_Leave);
            // 
            // lblPitchDeviceSerialPort
            // 
            this.lblPitchDeviceSerialPort.AutoSize = true;
            this.lblPitchDeviceSerialPort.Location = new System.Drawing.Point(17, 17);
            this.lblPitchDeviceSerialPort.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPitchDeviceSerialPort.Name = "lblPitchDeviceSerialPort";
            this.lblPitchDeviceSerialPort.Size = new System.Drawing.Size(212, 25);
            this.lblPitchDeviceSerialPort.TabIndex = 5;
            this.lblPitchDeviceSerialPort.Text = "Pitch SDI Serial &Port:";
            // 
            // cbPitchDeviceSerialPort
            // 
            this.cbPitchDeviceSerialPort.FormattingEnabled = true;
            this.cbPitchDeviceSerialPort.Location = new System.Drawing.Point(238, 13);
            this.cbPitchDeviceSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbPitchDeviceSerialPort.Name = "cbPitchDeviceSerialPort";
            this.cbPitchDeviceSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbPitchDeviceSerialPort.TabIndex = 4;
            this.cbPitchDeviceSerialPort.SelectedIndexChanged += new System.EventHandler(this.cbPitchDeviceSerialPort_SelectedIndexChanged);
            // 
            // lblPitchDeviceIdentification
            // 
            this.lblPitchDeviceIdentification.AutoSize = true;
            this.lblPitchDeviceIdentification.Location = new System.Drawing.Point(530, 17);
            this.lblPitchDeviceIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPitchDeviceIdentification.Name = "lblPitchDeviceIdentification";
            this.lblPitchDeviceIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblPitchDeviceIdentification.TabIndex = 6;
            this.lblPitchDeviceIdentification.Text = "Identification:";
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.tabControl1);
            this.gbMain.Location = new System.Drawing.Point(3, 104);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(1060, 895);
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
            this.tabControl1.Size = new System.Drawing.Size(1054, 865);
            this.tabControl1.TabIndex = 0;
            // 
            // tabRawData
            // 
            this.tabRawData.Controls.Add(this.gbRollRawDataControl);
            this.tabRawData.Controls.Add(this.gbPitchRawDataControl);
            this.tabRawData.Location = new System.Drawing.Point(4, 34);
            this.tabRawData.Name = "tabRawData";
            this.tabRawData.Size = new System.Drawing.Size(1046, 827);
            this.tabRawData.TabIndex = 4;
            this.tabRawData.Text = "Raw Data";
            this.tabRawData.UseVisualStyleBackColor = true;
            // 
            // gbRollRawDataControl
            // 
            this.gbRollRawDataControl.Controls.Add(this.lblRollSubAddr);
            this.gbRollRawDataControl.Controls.Add(this.txtRollSubAddr);
            this.gbRollRawDataControl.Controls.Add(this.btnRollSendRaw);
            this.gbRollRawDataControl.Controls.Add(this.lblRollDataByte);
            this.gbRollRawDataControl.Controls.Add(this.txtRollDataByte);
            this.gbRollRawDataControl.Location = new System.Drawing.Point(322, 6);
            this.gbRollRawDataControl.Margin = new System.Windows.Forms.Padding(6);
            this.gbRollRawDataControl.Name = "gbRollRawDataControl";
            this.gbRollRawDataControl.Padding = new System.Windows.Forms.Padding(6);
            this.gbRollRawDataControl.Size = new System.Drawing.Size(304, 293);
            this.gbRollRawDataControl.TabIndex = 7;
            this.gbRollRawDataControl.TabStop = false;
            this.gbRollRawDataControl.Text = "Raw Data Control (Roll)";
            // 
            // lblRollSubAddr
            // 
            this.lblRollSubAddr.AutoSize = true;
            this.lblRollSubAddr.Location = new System.Drawing.Point(37, 118);
            this.lblRollSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblRollSubAddr.Name = "lblRollSubAddr";
            this.lblRollSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblRollSubAddr.TabIndex = 2;
            this.lblRollSubAddr.Text = "S&ubaddress:";
            this.lblRollSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRollSubAddr
            // 
            this.txtRollSubAddr.Location = new System.Drawing.Point(181, 112);
            this.txtRollSubAddr.Margin = new System.Windows.Forms.Padding(6);
            this.txtRollSubAddr.MaxLength = 4;
            this.txtRollSubAddr.Name = "txtRollSubAddr";
            this.txtRollSubAddr.Size = new System.Drawing.Size(88, 31);
            this.txtRollSubAddr.TabIndex = 3;
            this.txtRollSubAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRollSubAddr.Leave += new System.EventHandler(this.txtRollSubAddr_Leave);
            // 
            // btnRollSendRaw
            // 
            this.btnRollSendRaw.Location = new System.Drawing.Point(42, 224);
            this.btnRollSendRaw.Margin = new System.Windows.Forms.Padding(6);
            this.btnRollSendRaw.Name = "btnRollSendRaw";
            this.btnRollSendRaw.Size = new System.Drawing.Size(227, 44);
            this.btnRollSendRaw.TabIndex = 6;
            this.btnRollSendRaw.Text = "&Send";
            this.btnRollSendRaw.UseVisualStyleBackColor = true;
            this.btnRollSendRaw.Click += new System.EventHandler(this.btnRollSendRaw_Click);
            // 
            // lblRollDataByte
            // 
            this.lblRollDataByte.AutoSize = true;
            this.lblRollDataByte.Location = new System.Drawing.Point(55, 168);
            this.lblRollDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblRollDataByte.Name = "lblRollDataByte";
            this.lblRollDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblRollDataByte.TabIndex = 4;
            this.lblRollDataByte.Text = "Data &Byte:";
            this.lblRollDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRollDataByte
            // 
            this.txtRollDataByte.Location = new System.Drawing.Point(181, 162);
            this.txtRollDataByte.Margin = new System.Windows.Forms.Padding(6);
            this.txtRollDataByte.MaxLength = 4;
            this.txtRollDataByte.Name = "txtRollDataByte";
            this.txtRollDataByte.Size = new System.Drawing.Size(88, 31);
            this.txtRollDataByte.TabIndex = 5;
            this.txtRollDataByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRollDataByte.Leave += new System.EventHandler(this.txtRollDataByte_Leave);
            // 
            // lblRollDeviceSerialPort
            // 
            this.lblRollDeviceSerialPort.AutoSize = true;
            this.lblRollDeviceSerialPort.Location = new System.Drawing.Point(17, 62);
            this.lblRollDeviceSerialPort.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblRollDeviceSerialPort.Name = "lblRollDeviceSerialPort";
            this.lblRollDeviceSerialPort.Size = new System.Drawing.Size(201, 25);
            this.lblRollDeviceSerialPort.TabIndex = 13;
            this.lblRollDeviceSerialPort.Text = "Roll SDI Serial &Port:";
            // 
            // cbRollDeviceSerialPort
            // 
            this.cbRollDeviceSerialPort.FormattingEnabled = true;
            this.cbRollDeviceSerialPort.Location = new System.Drawing.Point(238, 58);
            this.cbRollDeviceSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbRollDeviceSerialPort.Name = "cbRollDeviceSerialPort";
            this.cbRollDeviceSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbRollDeviceSerialPort.TabIndex = 12;
            this.cbRollDeviceSerialPort.SelectedIndexChanged += new System.EventHandler(this.cbRollDeviceSerialPort_SelectedIndexChanged);
            // 
            // lblRollDeviceIdentification
            // 
            this.lblRollDeviceIdentification.AutoSize = true;
            this.lblRollDeviceIdentification.Location = new System.Drawing.Point(530, 62);
            this.lblRollDeviceIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblRollDeviceIdentification.Name = "lblRollDeviceIdentification";
            this.lblRollDeviceIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblRollDeviceIdentification.TabIndex = 14;
            this.lblRollDeviceIdentification.Text = "Identification:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1067, 997);
            this.Controls.Add(this.lblRollDeviceSerialPort);
            this.Controls.Add(this.cbRollDeviceSerialPort);
            this.Controls.Add(this.lblRollDeviceIdentification);
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.lblPitchDeviceSerialPort);
            this.Controls.Add(this.cbPitchDeviceSerialPort);
            this.Controls.Add(this.lblPitchDeviceIdentification);
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
            this.gbPitchRawDataControl.ResumeLayout(false);
            this.gbPitchRawDataControl.PerformLayout();
            this.gbMain.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabRawData.ResumeLayout(false);
            this.gbRollRawDataControl.ResumeLayout(false);
            this.gbRollRawDataControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDeviceAddress;
        private System.Windows.Forms.ErrorProvider epErrorProvider;
        private System.Windows.Forms.Label lblPitchDeviceSerialPort;
        private System.Windows.Forms.ComboBox cbPitchDeviceSerialPort;
        private System.Windows.Forms.Label lblPitchDeviceIdentification;
        private System.Windows.Forms.GroupBox gbPitchRawDataControl;
        private System.Windows.Forms.Label lblPitchSubAddr;
        private System.Windows.Forms.TextBox txtPitchSubAddr;
        private System.Windows.Forms.Button btnPitchSendRaw;
        private System.Windows.Forms.Label lblPitchDataByte;
        private System.Windows.Forms.TextBox txtPitchDataByte;

        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabRawData;
        private System.Windows.Forms.Label lblRollDeviceSerialPort;
        private System.Windows.Forms.ComboBox cbRollDeviceSerialPort;
        private System.Windows.Forms.Label lblRollDeviceIdentification;
        private System.Windows.Forms.GroupBox gbRollRawDataControl;
        private System.Windows.Forms.Label lblRollSubAddr;
        private System.Windows.Forms.TextBox txtRollSubAddr;
        private System.Windows.Forms.Button btnRollSendRaw;
        private System.Windows.Forms.Label lblRollDataByte;
        private System.Windows.Forms.TextBox txtRollDataByte;
    }
}

