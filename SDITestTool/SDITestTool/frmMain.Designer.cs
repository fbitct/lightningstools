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
            this.lblDeviceAddress = new System.Windows.Forms.Label();
            this.epErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbRaw = new System.Windows.Forms.GroupBox();
            this.lblSubAddr = new System.Windows.Forms.Label();
            this.txtSubAddr = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblDataByte = new System.Windows.Forms.Label();
            this.txtDataByte = new System.Windows.Forms.TextBox();
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.cbSerialPort = new System.Windows.Forms.ComboBox();
            this.lblIdentification = new System.Windows.Forms.Label();
            this.gbLED = new System.Windows.Forms.GroupBox();
            this.rdoToggleLEDPerAcceptedCommand = new System.Windows.Forms.RadioButton();
            this.rdoLEDFlashesAtHeartbeatRate = new System.Windows.Forms.RadioButton();
            this.rdoLEDAlwaysOn = new System.Windows.Forms.RadioButton();
            this.rdoLEDAlwaysOff = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).BeginInit();
            this.gbRaw.SuspendLayout();
            this.gbLED.SuspendLayout();
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
            // gbRaw
            // 
            this.gbRaw.Controls.Add(this.lblSubAddr);
            this.gbRaw.Controls.Add(this.txtSubAddr);
            this.gbRaw.Controls.Add(this.btnSend);
            this.gbRaw.Controls.Add(this.lblDataByte);
            this.gbRaw.Controls.Add(this.txtDataByte);
            this.gbRaw.Location = new System.Drawing.Point(15, 303);
            this.gbRaw.Margin = new System.Windows.Forms.Padding(6);
            this.gbRaw.Name = "gbRaw";
            this.gbRaw.Padding = new System.Windows.Forms.Padding(6);
            this.gbRaw.Size = new System.Drawing.Size(306, 206);
            this.gbRaw.TabIndex = 0;
            this.gbRaw.TabStop = false;
            this.gbRaw.Text = "Raw Data Control";
            // 
            // lblSubAddr
            // 
            this.lblSubAddr.AutoSize = true;
            this.lblSubAddr.Location = new System.Drawing.Point(37, 46);
            this.lblSubAddr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSubAddr.Name = "lblSubAddr";
            this.lblSubAddr.Size = new System.Drawing.Size(133, 25);
            this.lblSubAddr.TabIndex = 2;
            this.lblSubAddr.Text = "S&ubaddress:";
            this.lblSubAddr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSubAddr
            // 
            this.txtSubAddr.Location = new System.Drawing.Point(181, 40);
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
            this.btnSend.Location = new System.Drawing.Point(20, 141);
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
            this.lblDataByte.Location = new System.Drawing.Point(55, 96);
            this.lblDataByte.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDataByte.Name = "lblDataByte";
            this.lblDataByte.Size = new System.Drawing.Size(112, 25);
            this.lblDataByte.TabIndex = 4;
            this.lblDataByte.Text = "Data &Byte:";
            this.lblDataByte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDataByte
            // 
            this.txtDataByte.Location = new System.Drawing.Point(181, 90);
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
            this.cbSerialPort.Location = new System.Drawing.Point(140, 23);
            this.cbSerialPort.Margin = new System.Windows.Forms.Padding(6);
            this.cbSerialPort.Name = "cbSerialPort";
            this.cbSerialPort.Size = new System.Drawing.Size(238, 33);
            this.cbSerialPort.TabIndex = 4;
            // 
            // lblIdentification
            // 
            this.lblIdentification.AutoSize = true;
            this.lblIdentification.Location = new System.Drawing.Point(432, 23);
            this.lblIdentification.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblIdentification.Name = "lblIdentification";
            this.lblIdentification.Size = new System.Drawing.Size(139, 25);
            this.lblIdentification.TabIndex = 6;
            this.lblIdentification.Text = "Identification:";
            // 
            // gbLED
            // 
            this.gbLED.Controls.Add(this.rdoToggleLEDPerAcceptedCommand);
            this.gbLED.Controls.Add(this.rdoLEDFlashesAtHeartbeatRate);
            this.gbLED.Controls.Add(this.rdoLEDAlwaysOn);
            this.gbLED.Controls.Add(this.rdoLEDAlwaysOff);
            this.gbLED.Location = new System.Drawing.Point(15, 76);
            this.gbLED.Name = "gbLED";
            this.gbLED.Size = new System.Drawing.Size(489, 188);
            this.gbLED.TabIndex = 7;
            this.gbLED.TabStop = false;
            this.gbLED.Text = "Onboard Diagnostic LED";
            // 
            // rdoToggleLEDPerAcceptedCommand
            // 
            this.rdoToggleLEDPerAcceptedCommand.AutoSize = true;
            this.rdoToggleLEDPerAcceptedCommand.Location = new System.Drawing.Point(6, 135);
            this.rdoToggleLEDPerAcceptedCommand.Name = "rdoToggleLEDPerAcceptedCommand";
            this.rdoToggleLEDPerAcceptedCommand.Size = new System.Drawing.Size(471, 29);
            this.rdoToggleLEDPerAcceptedCommand.TabIndex = 11;
            this.rdoToggleLEDPerAcceptedCommand.TabStop = true;
            this.rdoToggleLEDPerAcceptedCommand.Text = "Toggle LED ON/OFF per accepted command";
            this.rdoToggleLEDPerAcceptedCommand.UseVisualStyleBackColor = true;
            this.rdoToggleLEDPerAcceptedCommand.CheckedChanged += new System.EventHandler(this.rdoToggleLEDPerAcceptedCommand_CheckedChanged);
            // 
            // rdoLEDFlashesAtHeartbeatRate
            // 
            this.rdoLEDFlashesAtHeartbeatRate.AutoSize = true;
            this.rdoLEDFlashesAtHeartbeatRate.Location = new System.Drawing.Point(6, 100);
            this.rdoLEDFlashesAtHeartbeatRate.Name = "rdoLEDFlashesAtHeartbeatRate";
            this.rdoLEDFlashesAtHeartbeatRate.Size = new System.Drawing.Size(318, 29);
            this.rdoLEDFlashesAtHeartbeatRate.TabIndex = 10;
            this.rdoLEDFlashesAtHeartbeatRate.TabStop = true;
            this.rdoLEDFlashesAtHeartbeatRate.Text = "Flash LED at Heartbeat Rate";
            this.rdoLEDFlashesAtHeartbeatRate.UseVisualStyleBackColor = true;
            this.rdoLEDFlashesAtHeartbeatRate.CheckedChanged += new System.EventHandler(this.rdoLEDFlashesAtHeartbeatRate_CheckedChanged);
            // 
            // rdoLEDAlwaysOn
            // 
            this.rdoLEDAlwaysOn.AutoSize = true;
            this.rdoLEDAlwaysOn.Location = new System.Drawing.Point(6, 65);
            this.rdoLEDAlwaysOn.Name = "rdoLEDAlwaysOn";
            this.rdoLEDAlwaysOn.Size = new System.Drawing.Size(195, 29);
            this.rdoLEDAlwaysOn.TabIndex = 9;
            this.rdoLEDAlwaysOn.TabStop = true;
            this.rdoLEDAlwaysOn.Text = "LED Always ON";
            this.rdoLEDAlwaysOn.UseVisualStyleBackColor = true;
            this.rdoLEDAlwaysOn.CheckedChanged += new System.EventHandler(this.rdoLEDAlwaysON_CheckedChanged);
            // 
            // rdoLEDAlwaysOff
            // 
            this.rdoLEDAlwaysOff.AutoSize = true;
            this.rdoLEDAlwaysOff.Location = new System.Drawing.Point(6, 30);
            this.rdoLEDAlwaysOff.Name = "rdoLEDAlwaysOff";
            this.rdoLEDAlwaysOff.Size = new System.Drawing.Size(206, 29);
            this.rdoLEDAlwaysOff.TabIndex = 8;
            this.rdoLEDAlwaysOff.TabStop = true;
            this.rdoLEDAlwaysOff.Text = "LED Always OFF";
            this.rdoLEDAlwaysOff.UseVisualStyleBackColor = true;
            this.rdoLEDAlwaysOff.CheckedChanged += new System.EventHandler(this.rdoLEDAlwaysOff_CheckedChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1852, 1146);
            this.Controls.Add(this.gbLED);
            this.Controls.Add(this.gbRaw);
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
            this.Text = "SDI Test Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.epErrorProvider)).EndInit();
            this.gbRaw.ResumeLayout(false);
            this.gbRaw.PerformLayout();
            this.gbLED.ResumeLayout(false);
            this.gbLED.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDeviceAddress;
        private System.Windows.Forms.ErrorProvider epErrorProvider;
        private System.Windows.Forms.Label lblSerialPort;
        private System.Windows.Forms.ComboBox cbSerialPort;
        private System.Windows.Forms.Label lblIdentification;
        private System.Windows.Forms.GroupBox gbRaw;
        private System.Windows.Forms.Label lblSubAddr;
        private System.Windows.Forms.TextBox txtSubAddr;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblDataByte;
        private System.Windows.Forms.TextBox txtDataByte;
        private System.Windows.Forms.GroupBox gbLED;
        private System.Windows.Forms.RadioButton rdoToggleLEDPerAcceptedCommand;
        private System.Windows.Forms.RadioButton rdoLEDFlashesAtHeartbeatRate;
        private System.Windows.Forms.RadioButton rdoLEDAlwaysOn;
        private System.Windows.Forms.RadioButton rdoLEDAlwaysOff;
    }
}

