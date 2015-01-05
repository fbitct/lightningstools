namespace AnalogDevicesTestTool
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainPanel = new System.Windows.Forms.Panel();
            this.lblDevice = new System.Windows.Forms.Label();
            this.lblVOutCalculated = new System.Windows.Forms.Label();
            this.lblVout = new System.Windows.Forms.Label();
            this.rdoDataValueB = new System.Windows.Forms.RadioButton();
            this.rdoDataValueA = new System.Windows.Forms.RadioButton();
            this.lblDataValueB = new System.Windows.Forms.Label();
            this.txtDataValueB = new System.Windows.Forms.TextBox();
            this.lblDataValueA = new System.Windows.Forms.Label();
            this.txtDataValueA = new System.Windows.Forms.TextBox();
            this.lblDACChannelGain = new System.Windows.Forms.Label();
            this.txtDACChannelGain = new System.Windows.Forms.TextBox();
            this.lblDACChannelOffset = new System.Windows.Forms.Label();
            this.txtDACChannelOffset = new System.Windows.Forms.TextBox();
            this.btnUpdateAllDACOutputs = new System.Windows.Forms.Button();
            this.btnResumeAllDACOutputs = new System.Windows.Forms.Button();
            this.btnSuspendAllDACOutputs = new System.Windows.Forms.Button();
            this.gbDACOutputs = new System.Windows.Forms.GroupBox();
            this.rdoDAC39 = new System.Windows.Forms.RadioButton();
            this.rdoDAC38 = new System.Windows.Forms.RadioButton();
            this.rdoDAC31 = new System.Windows.Forms.RadioButton();
            this.rdoDAC37 = new System.Windows.Forms.RadioButton();
            this.rdoDAC30 = new System.Windows.Forms.RadioButton();
            this.rdoDAC36 = new System.Windows.Forms.RadioButton();
            this.rdoDAC23 = new System.Windows.Forms.RadioButton();
            this.rdoDAC35 = new System.Windows.Forms.RadioButton();
            this.rdoDAC29 = new System.Windows.Forms.RadioButton();
            this.rdoDAC34 = new System.Windows.Forms.RadioButton();
            this.rdoDAC22 = new System.Windows.Forms.RadioButton();
            this.rdoDAC33 = new System.Windows.Forms.RadioButton();
            this.rdoDAC28 = new System.Windows.Forms.RadioButton();
            this.rdoDAC32 = new System.Windows.Forms.RadioButton();
            this.rdoDAC15 = new System.Windows.Forms.RadioButton();
            this.rdoDAC27 = new System.Windows.Forms.RadioButton();
            this.rdoDAC21 = new System.Windows.Forms.RadioButton();
            this.rdoDAC26 = new System.Windows.Forms.RadioButton();
            this.rdoDAC7 = new System.Windows.Forms.RadioButton();
            this.rdoDAC25 = new System.Windows.Forms.RadioButton();
            this.rdoDAC20 = new System.Windows.Forms.RadioButton();
            this.rdoDAC24 = new System.Windows.Forms.RadioButton();
            this.rdoDAC14 = new System.Windows.Forms.RadioButton();
            this.rdoDAC19 = new System.Windows.Forms.RadioButton();
            this.rdoDAC6 = new System.Windows.Forms.RadioButton();
            this.rdoDAC18 = new System.Windows.Forms.RadioButton();
            this.rdoDAC13 = new System.Windows.Forms.RadioButton();
            this.rdoDAC17 = new System.Windows.Forms.RadioButton();
            this.rdoDAC5 = new System.Windows.Forms.RadioButton();
            this.rdoDAC16 = new System.Windows.Forms.RadioButton();
            this.rdoDAC12 = new System.Windows.Forms.RadioButton();
            this.rdoDAC4 = new System.Windows.Forms.RadioButton();
            this.rdoDAC11 = new System.Windows.Forms.RadioButton();
            this.rdoDAC3 = new System.Windows.Forms.RadioButton();
            this.rdoDAC10 = new System.Windows.Forms.RadioButton();
            this.rdoDAC2 = new System.Windows.Forms.RadioButton();
            this.rdoDAC9 = new System.Windows.Forms.RadioButton();
            this.rdoDAC8 = new System.Windows.Forms.RadioButton();
            this.rdoDAC1 = new System.Windows.Forms.RadioButton();
            this.rdoDAC0 = new System.Windows.Forms.RadioButton();
            this.cboDevices = new System.Windows.Forms.ComboBox();
            this.gbDeviceOptions = new System.Windows.Forms.GroupBox();
            this.lblVREF0 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtVREF0 = new System.Windows.Forms.TextBox();
            this.btnPerformSoftPowerDown = new System.Windows.Forms.Button();
            this.lblVREF1 = new System.Windows.Forms.Label();
            this.btnSoftPowerUp = new System.Windows.Forms.Button();
            this.txtVREF1 = new System.Windows.Forms.TextBox();
            this.txtOffsetDAC0 = new System.Windows.Forms.TextBox();
            this.lblGroup1Offset = new System.Windows.Forms.Label();
            this.lblOffsetDAC0 = new System.Windows.Forms.Label();
            this.txtOffsetDAC1 = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbDataValues = new System.Windows.Forms.GroupBox();
            this.mainPanel.SuspendLayout();
            this.gbDACOutputs.SuspendLayout();
            this.gbDeviceOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.gbDataValues.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.AutoSize = true;
            this.mainPanel.Controls.Add(this.gbDACOutputs);
            this.mainPanel.Controls.Add(this.lblDevice);
            this.mainPanel.Controls.Add(this.cboDevices);
            this.mainPanel.Controls.Add(this.gbDeviceOptions);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(546, 443);
            this.mainPanel.TabIndex = 1;
            // 
            // lblDevice
            // 
            this.lblDevice.AutoSize = true;
            this.lblDevice.Location = new System.Drawing.Point(18, 15);
            this.lblDevice.Name = "lblDevice";
            this.lblDevice.Size = new System.Drawing.Size(41, 13);
            this.lblDevice.TabIndex = 38;
            this.lblDevice.Text = "Device";
            // 
            // lblVOutCalculated
            // 
            this.lblVOutCalculated.AutoSize = true;
            this.lblVOutCalculated.Location = new System.Drawing.Point(292, 38);
            this.lblVOutCalculated.Name = "lblVOutCalculated";
            this.lblVOutCalculated.Size = new System.Drawing.Size(0, 13);
            this.lblVOutCalculated.TabIndex = 25;
            // 
            // lblVout
            // 
            this.lblVout.AutoSize = true;
            this.lblVout.Location = new System.Drawing.Point(254, 38);
            this.lblVout.Name = "lblVout";
            this.lblVout.Size = new System.Drawing.Size(32, 13);
            this.lblVout.TabIndex = 24;
            this.lblVout.Text = "Vout:";
            // 
            // rdoDataValueB
            // 
            this.rdoDataValueB.AutoSize = true;
            this.rdoDataValueB.Location = new System.Drawing.Point(98, 52);
            this.rdoDataValueB.Name = "rdoDataValueB";
            this.rdoDataValueB.Size = new System.Drawing.Size(14, 13);
            this.rdoDataValueB.TabIndex = 23;
            this.rdoDataValueB.TabStop = true;
            this.rdoDataValueB.UseVisualStyleBackColor = true;
            this.rdoDataValueB.CheckedChanged += new System.EventHandler(this.rdoDataValueB_CheckedChanged);
            // 
            // rdoDataValueA
            // 
            this.rdoDataValueA.AutoSize = true;
            this.rdoDataValueA.Location = new System.Drawing.Point(98, 26);
            this.rdoDataValueA.Name = "rdoDataValueA";
            this.rdoDataValueA.Size = new System.Drawing.Size(14, 13);
            this.rdoDataValueA.TabIndex = 22;
            this.rdoDataValueA.TabStop = true;
            this.rdoDataValueA.UseVisualStyleBackColor = true;
            this.rdoDataValueA.CheckedChanged += new System.EventHandler(this.rdoDataValueA_CheckedChanged);
            // 
            // lblDataValueB
            // 
            this.lblDataValueB.AutoSize = true;
            this.lblDataValueB.Location = new System.Drawing.Point(112, 52);
            this.lblDataValueB.Name = "lblDataValueB";
            this.lblDataValueB.Size = new System.Drawing.Size(70, 13);
            this.lblDataValueB.TabIndex = 21;
            this.lblDataValueB.Text = "Data Value B";
            // 
            // txtDataValueB
            // 
            this.txtDataValueB.Location = new System.Drawing.Point(189, 49);
            this.txtDataValueB.MaxLength = 4;
            this.txtDataValueB.Name = "txtDataValueB";
            this.txtDataValueB.Size = new System.Drawing.Size(30, 20);
            this.txtDataValueB.TabIndex = 20;
            this.txtDataValueB.Text = "0";
            this.txtDataValueB.Validating += new System.ComponentModel.CancelEventHandler(this.txtDataValueB_Validating);
            this.txtDataValueB.Validated += new System.EventHandler(this.txtDataValueB_Validated);
            // 
            // lblDataValueA
            // 
            this.lblDataValueA.AutoSize = true;
            this.lblDataValueA.Location = new System.Drawing.Point(112, 26);
            this.lblDataValueA.Name = "lblDataValueA";
            this.lblDataValueA.Size = new System.Drawing.Size(70, 13);
            this.lblDataValueA.TabIndex = 19;
            this.lblDataValueA.Text = "Data Value A";
            // 
            // txtDataValueA
            // 
            this.txtDataValueA.Location = new System.Drawing.Point(188, 22);
            this.txtDataValueA.MaxLength = 4;
            this.txtDataValueA.Name = "txtDataValueA";
            this.txtDataValueA.Size = new System.Drawing.Size(30, 20);
            this.txtDataValueA.TabIndex = 18;
            this.txtDataValueA.Text = "FFFF";
            this.txtDataValueA.Validating += new System.ComponentModel.CancelEventHandler(this.txtDataValueA_Validating);
            this.txtDataValueA.Validated += new System.EventHandler(this.txtDataValueA_Validated);
            // 
            // lblDACChannelGain
            // 
            this.lblDACChannelGain.AutoSize = true;
            this.lblDACChannelGain.Location = new System.Drawing.Point(13, 52);
            this.lblDACChannelGain.Name = "lblDACChannelGain";
            this.lblDACChannelGain.Size = new System.Drawing.Size(29, 13);
            this.lblDACChannelGain.TabIndex = 17;
            this.lblDACChannelGain.Text = "Gain";
            // 
            // txtDACChannelGain
            // 
            this.txtDACChannelGain.Location = new System.Drawing.Point(54, 49);
            this.txtDACChannelGain.MaxLength = 4;
            this.txtDACChannelGain.Name = "txtDACChannelGain";
            this.txtDACChannelGain.Size = new System.Drawing.Size(34, 20);
            this.txtDACChannelGain.TabIndex = 16;
            this.txtDACChannelGain.Text = "FFFF";
            this.txtDACChannelGain.Validating += new System.ComponentModel.CancelEventHandler(this.txtDACChannelGain_Validating);
            this.txtDACChannelGain.Validated += new System.EventHandler(this.txtDACChannelGain_Validated);
            // 
            // lblDACChannelOffset
            // 
            this.lblDACChannelOffset.AutoSize = true;
            this.lblDACChannelOffset.Location = new System.Drawing.Point(13, 26);
            this.lblDACChannelOffset.Name = "lblDACChannelOffset";
            this.lblDACChannelOffset.Size = new System.Drawing.Size(35, 13);
            this.lblDACChannelOffset.TabIndex = 15;
            this.lblDACChannelOffset.Text = "Offset";
            // 
            // txtDACChannelOffset
            // 
            this.txtDACChannelOffset.Location = new System.Drawing.Point(54, 23);
            this.txtDACChannelOffset.MaxLength = 4;
            this.txtDACChannelOffset.Name = "txtDACChannelOffset";
            this.txtDACChannelOffset.Size = new System.Drawing.Size(34, 20);
            this.txtDACChannelOffset.TabIndex = 14;
            this.txtDACChannelOffset.Text = "8000";
            this.txtDACChannelOffset.Validating += new System.ComponentModel.CancelEventHandler(this.txtDACChannelOffset_Validating);
            this.txtDACChannelOffset.Validated += new System.EventHandler(this.txtDACChannelOffset_Validated);
            // 
            // btnUpdateAllDACOutputs
            // 
            this.btnUpdateAllDACOutputs.Location = new System.Drawing.Point(329, 243);
            this.btnUpdateAllDACOutputs.Name = "btnUpdateAllDACOutputs";
            this.btnUpdateAllDACOutputs.Size = new System.Drawing.Size(154, 23);
            this.btnUpdateAllDACOutputs.TabIndex = 11;
            this.btnUpdateAllDACOutputs.Text = "Update All DAC Outputs";
            this.btnUpdateAllDACOutputs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUpdateAllDACOutputs.UseVisualStyleBackColor = true;
            this.btnUpdateAllDACOutputs.Click += new System.EventHandler(this.btnUpdateAllDacOutputs_Click);
            // 
            // btnResumeAllDACOutputs
            // 
            this.btnResumeAllDACOutputs.Location = new System.Drawing.Point(169, 243);
            this.btnResumeAllDACOutputs.Name = "btnResumeAllDACOutputs";
            this.btnResumeAllDACOutputs.Size = new System.Drawing.Size(154, 23);
            this.btnResumeAllDACOutputs.TabIndex = 10;
            this.btnResumeAllDACOutputs.Text = "Resume All DAC Outputs";
            this.btnResumeAllDACOutputs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnResumeAllDACOutputs.UseVisualStyleBackColor = true;
            this.btnResumeAllDACOutputs.Click += new System.EventHandler(this.btnResumeAllDACOutputs_Click);
            // 
            // btnSuspendAllDACOutputs
            // 
            this.btnSuspendAllDACOutputs.Location = new System.Drawing.Point(7, 243);
            this.btnSuspendAllDACOutputs.Name = "btnSuspendAllDACOutputs";
            this.btnSuspendAllDACOutputs.Size = new System.Drawing.Size(154, 23);
            this.btnSuspendAllDACOutputs.TabIndex = 9;
            this.btnSuspendAllDACOutputs.Text = "Suspend All DAC Outputs";
            this.btnSuspendAllDACOutputs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSuspendAllDACOutputs.UseVisualStyleBackColor = true;
            this.btnSuspendAllDACOutputs.Click += new System.EventHandler(this.btnSuspendAllDACOutputs_Click);
            // 
            // gbDACOutputs
            // 
            this.gbDACOutputs.Controls.Add(this.rdoDAC39);
            this.gbDACOutputs.Controls.Add(this.rdoDAC38);
            this.gbDACOutputs.Controls.Add(this.rdoDAC31);
            this.gbDACOutputs.Controls.Add(this.rdoDAC37);
            this.gbDACOutputs.Controls.Add(this.rdoDAC30);
            this.gbDACOutputs.Controls.Add(this.rdoDAC36);
            this.gbDACOutputs.Controls.Add(this.rdoDAC23);
            this.gbDACOutputs.Controls.Add(this.rdoDAC35);
            this.gbDACOutputs.Controls.Add(this.rdoDAC29);
            this.gbDACOutputs.Controls.Add(this.rdoDAC34);
            this.gbDACOutputs.Controls.Add(this.rdoDAC22);
            this.gbDACOutputs.Controls.Add(this.btnUpdateAllDACOutputs);
            this.gbDACOutputs.Controls.Add(this.rdoDAC33);
            this.gbDACOutputs.Controls.Add(this.btnResumeAllDACOutputs);
            this.gbDACOutputs.Controls.Add(this.rdoDAC28);
            this.gbDACOutputs.Controls.Add(this.btnSuspendAllDACOutputs);
            this.gbDACOutputs.Controls.Add(this.rdoDAC32);
            this.gbDACOutputs.Controls.Add(this.rdoDAC15);
            this.gbDACOutputs.Controls.Add(this.rdoDAC27);
            this.gbDACOutputs.Controls.Add(this.rdoDAC21);
            this.gbDACOutputs.Controls.Add(this.rdoDAC26);
            this.gbDACOutputs.Controls.Add(this.rdoDAC7);
            this.gbDACOutputs.Controls.Add(this.rdoDAC25);
            this.gbDACOutputs.Controls.Add(this.rdoDAC20);
            this.gbDACOutputs.Controls.Add(this.rdoDAC24);
            this.gbDACOutputs.Controls.Add(this.rdoDAC14);
            this.gbDACOutputs.Controls.Add(this.rdoDAC19);
            this.gbDACOutputs.Controls.Add(this.rdoDAC6);
            this.gbDACOutputs.Controls.Add(this.rdoDAC18);
            this.gbDACOutputs.Controls.Add(this.rdoDAC13);
            this.gbDACOutputs.Controls.Add(this.rdoDAC17);
            this.gbDACOutputs.Controls.Add(this.rdoDAC5);
            this.gbDACOutputs.Controls.Add(this.rdoDAC16);
            this.gbDACOutputs.Controls.Add(this.rdoDAC12);
            this.gbDACOutputs.Controls.Add(this.rdoDAC4);
            this.gbDACOutputs.Controls.Add(this.rdoDAC11);
            this.gbDACOutputs.Controls.Add(this.rdoDAC3);
            this.gbDACOutputs.Controls.Add(this.rdoDAC10);
            this.gbDACOutputs.Controls.Add(this.rdoDAC2);
            this.gbDACOutputs.Controls.Add(this.rdoDAC9);
            this.gbDACOutputs.Controls.Add(this.rdoDAC8);
            this.gbDACOutputs.Controls.Add(this.rdoDAC1);
            this.gbDACOutputs.Controls.Add(this.rdoDAC0);
            this.gbDACOutputs.Controls.Add(this.gbDataValues);
            this.gbDACOutputs.Location = new System.Drawing.Point(18, 150);
            this.gbDACOutputs.Name = "gbDACOutputs";
            this.gbDACOutputs.Size = new System.Drawing.Size(511, 277);
            this.gbDACOutputs.TabIndex = 0;
            this.gbDACOutputs.TabStop = false;
            this.gbDACOutputs.Text = "DAC Outputs";
            // 
            // rdoDAC39
            // 
            this.rdoDAC39.AutoSize = true;
            this.rdoDAC39.Location = new System.Drawing.Point(440, 111);
            this.rdoDAC39.Name = "rdoDAC39";
            this.rdoDAC39.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC39.TabIndex = 9;
            this.rdoDAC39.TabStop = true;
            this.rdoDAC39.Text = "DAC 39";
            this.rdoDAC39.UseVisualStyleBackColor = true;
            // 
            // rdoDAC38
            // 
            this.rdoDAC38.AutoSize = true;
            this.rdoDAC38.Location = new System.Drawing.Point(378, 111);
            this.rdoDAC38.Name = "rdoDAC38";
            this.rdoDAC38.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC38.TabIndex = 8;
            this.rdoDAC38.TabStop = true;
            this.rdoDAC38.Text = "DAC 38";
            this.rdoDAC38.UseVisualStyleBackColor = true;
            // 
            // rdoDAC31
            // 
            this.rdoDAC31.AutoSize = true;
            this.rdoDAC31.Location = new System.Drawing.Point(440, 88);
            this.rdoDAC31.Name = "rdoDAC31";
            this.rdoDAC31.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC31.TabIndex = 9;
            this.rdoDAC31.TabStop = true;
            this.rdoDAC31.Text = "DAC 31";
            this.rdoDAC31.UseVisualStyleBackColor = true;
            // 
            // rdoDAC37
            // 
            this.rdoDAC37.AutoSize = true;
            this.rdoDAC37.Location = new System.Drawing.Point(316, 111);
            this.rdoDAC37.Name = "rdoDAC37";
            this.rdoDAC37.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC37.TabIndex = 7;
            this.rdoDAC37.TabStop = true;
            this.rdoDAC37.Text = "DAC 37";
            this.rdoDAC37.UseVisualStyleBackColor = true;
            // 
            // rdoDAC30
            // 
            this.rdoDAC30.AutoSize = true;
            this.rdoDAC30.Location = new System.Drawing.Point(378, 88);
            this.rdoDAC30.Name = "rdoDAC30";
            this.rdoDAC30.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC30.TabIndex = 8;
            this.rdoDAC30.TabStop = true;
            this.rdoDAC30.Text = "DAC 30";
            this.rdoDAC30.UseVisualStyleBackColor = true;
            // 
            // rdoDAC36
            // 
            this.rdoDAC36.AutoSize = true;
            this.rdoDAC36.Location = new System.Drawing.Point(254, 111);
            this.rdoDAC36.Name = "rdoDAC36";
            this.rdoDAC36.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC36.TabIndex = 6;
            this.rdoDAC36.TabStop = true;
            this.rdoDAC36.Text = "DAC 36";
            this.rdoDAC36.UseVisualStyleBackColor = true;
            // 
            // rdoDAC23
            // 
            this.rdoDAC23.AutoSize = true;
            this.rdoDAC23.Location = new System.Drawing.Point(440, 65);
            this.rdoDAC23.Name = "rdoDAC23";
            this.rdoDAC23.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC23.TabIndex = 9;
            this.rdoDAC23.TabStop = true;
            this.rdoDAC23.Text = "DAC 23";
            this.rdoDAC23.UseVisualStyleBackColor = true;
            // 
            // rdoDAC35
            // 
            this.rdoDAC35.AutoSize = true;
            this.rdoDAC35.Location = new System.Drawing.Point(192, 111);
            this.rdoDAC35.Name = "rdoDAC35";
            this.rdoDAC35.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC35.TabIndex = 5;
            this.rdoDAC35.TabStop = true;
            this.rdoDAC35.Text = "DAC 35";
            this.rdoDAC35.UseVisualStyleBackColor = true;
            // 
            // rdoDAC29
            // 
            this.rdoDAC29.AutoSize = true;
            this.rdoDAC29.Location = new System.Drawing.Point(316, 88);
            this.rdoDAC29.Name = "rdoDAC29";
            this.rdoDAC29.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC29.TabIndex = 7;
            this.rdoDAC29.TabStop = true;
            this.rdoDAC29.Text = "DAC 29";
            this.rdoDAC29.UseVisualStyleBackColor = true;
            // 
            // rdoDAC34
            // 
            this.rdoDAC34.AutoSize = true;
            this.rdoDAC34.Location = new System.Drawing.Point(130, 111);
            this.rdoDAC34.Name = "rdoDAC34";
            this.rdoDAC34.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC34.TabIndex = 4;
            this.rdoDAC34.TabStop = true;
            this.rdoDAC34.Text = "DAC 34";
            this.rdoDAC34.UseVisualStyleBackColor = true;
            // 
            // rdoDAC22
            // 
            this.rdoDAC22.AutoSize = true;
            this.rdoDAC22.Location = new System.Drawing.Point(378, 65);
            this.rdoDAC22.Name = "rdoDAC22";
            this.rdoDAC22.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC22.TabIndex = 8;
            this.rdoDAC22.TabStop = true;
            this.rdoDAC22.Text = "DAC 22";
            this.rdoDAC22.UseVisualStyleBackColor = true;
            // 
            // rdoDAC33
            // 
            this.rdoDAC33.AutoSize = true;
            this.rdoDAC33.Location = new System.Drawing.Point(68, 111);
            this.rdoDAC33.Name = "rdoDAC33";
            this.rdoDAC33.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC33.TabIndex = 3;
            this.rdoDAC33.TabStop = true;
            this.rdoDAC33.Text = "DAC 33";
            this.rdoDAC33.UseVisualStyleBackColor = true;
            // 
            // rdoDAC28
            // 
            this.rdoDAC28.AutoSize = true;
            this.rdoDAC28.Location = new System.Drawing.Point(254, 88);
            this.rdoDAC28.Name = "rdoDAC28";
            this.rdoDAC28.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC28.TabIndex = 6;
            this.rdoDAC28.TabStop = true;
            this.rdoDAC28.Text = "DAC 28";
            this.rdoDAC28.UseVisualStyleBackColor = true;
            // 
            // rdoDAC32
            // 
            this.rdoDAC32.AutoSize = true;
            this.rdoDAC32.Location = new System.Drawing.Point(6, 111);
            this.rdoDAC32.Name = "rdoDAC32";
            this.rdoDAC32.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC32.TabIndex = 2;
            this.rdoDAC32.TabStop = true;
            this.rdoDAC32.Text = "DAC 32";
            this.rdoDAC32.UseVisualStyleBackColor = true;
            // 
            // rdoDAC15
            // 
            this.rdoDAC15.AutoSize = true;
            this.rdoDAC15.Location = new System.Drawing.Point(440, 42);
            this.rdoDAC15.Name = "rdoDAC15";
            this.rdoDAC15.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC15.TabIndex = 9;
            this.rdoDAC15.TabStop = true;
            this.rdoDAC15.Text = "DAC 15";
            this.rdoDAC15.UseVisualStyleBackColor = true;
            // 
            // rdoDAC27
            // 
            this.rdoDAC27.AutoSize = true;
            this.rdoDAC27.Location = new System.Drawing.Point(192, 88);
            this.rdoDAC27.Name = "rdoDAC27";
            this.rdoDAC27.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC27.TabIndex = 5;
            this.rdoDAC27.TabStop = true;
            this.rdoDAC27.Text = "DAC 27";
            this.rdoDAC27.UseVisualStyleBackColor = true;
            // 
            // rdoDAC21
            // 
            this.rdoDAC21.AutoSize = true;
            this.rdoDAC21.Location = new System.Drawing.Point(316, 65);
            this.rdoDAC21.Name = "rdoDAC21";
            this.rdoDAC21.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC21.TabIndex = 7;
            this.rdoDAC21.TabStop = true;
            this.rdoDAC21.Text = "DAC 21";
            this.rdoDAC21.UseVisualStyleBackColor = true;
            // 
            // rdoDAC26
            // 
            this.rdoDAC26.AutoSize = true;
            this.rdoDAC26.Location = new System.Drawing.Point(130, 88);
            this.rdoDAC26.Name = "rdoDAC26";
            this.rdoDAC26.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC26.TabIndex = 4;
            this.rdoDAC26.TabStop = true;
            this.rdoDAC26.Text = "DAC 26";
            this.rdoDAC26.UseVisualStyleBackColor = true;
            // 
            // rdoDAC7
            // 
            this.rdoDAC7.AutoSize = true;
            this.rdoDAC7.Location = new System.Drawing.Point(440, 19);
            this.rdoDAC7.Name = "rdoDAC7";
            this.rdoDAC7.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC7.TabIndex = 9;
            this.rdoDAC7.TabStop = true;
            this.rdoDAC7.Text = "DAC 7";
            this.rdoDAC7.UseVisualStyleBackColor = true;
            // 
            // rdoDAC25
            // 
            this.rdoDAC25.AutoSize = true;
            this.rdoDAC25.Location = new System.Drawing.Point(68, 88);
            this.rdoDAC25.Name = "rdoDAC25";
            this.rdoDAC25.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC25.TabIndex = 3;
            this.rdoDAC25.TabStop = true;
            this.rdoDAC25.Text = "DAC 25";
            this.rdoDAC25.UseVisualStyleBackColor = true;
            // 
            // rdoDAC20
            // 
            this.rdoDAC20.AutoSize = true;
            this.rdoDAC20.Location = new System.Drawing.Point(254, 65);
            this.rdoDAC20.Name = "rdoDAC20";
            this.rdoDAC20.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC20.TabIndex = 6;
            this.rdoDAC20.TabStop = true;
            this.rdoDAC20.Text = "DAC 20";
            this.rdoDAC20.UseVisualStyleBackColor = true;
            // 
            // rdoDAC24
            // 
            this.rdoDAC24.AutoSize = true;
            this.rdoDAC24.Location = new System.Drawing.Point(6, 88);
            this.rdoDAC24.Name = "rdoDAC24";
            this.rdoDAC24.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC24.TabIndex = 2;
            this.rdoDAC24.TabStop = true;
            this.rdoDAC24.Text = "DAC 24";
            this.rdoDAC24.UseVisualStyleBackColor = true;
            // 
            // rdoDAC14
            // 
            this.rdoDAC14.AutoSize = true;
            this.rdoDAC14.Location = new System.Drawing.Point(378, 42);
            this.rdoDAC14.Name = "rdoDAC14";
            this.rdoDAC14.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC14.TabIndex = 8;
            this.rdoDAC14.TabStop = true;
            this.rdoDAC14.Text = "DAC 14";
            this.rdoDAC14.UseVisualStyleBackColor = true;
            // 
            // rdoDAC19
            // 
            this.rdoDAC19.AutoSize = true;
            this.rdoDAC19.Location = new System.Drawing.Point(192, 65);
            this.rdoDAC19.Name = "rdoDAC19";
            this.rdoDAC19.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC19.TabIndex = 5;
            this.rdoDAC19.TabStop = true;
            this.rdoDAC19.Text = "DAC 19";
            this.rdoDAC19.UseVisualStyleBackColor = true;
            // 
            // rdoDAC6
            // 
            this.rdoDAC6.AutoSize = true;
            this.rdoDAC6.Location = new System.Drawing.Point(378, 19);
            this.rdoDAC6.Name = "rdoDAC6";
            this.rdoDAC6.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC6.TabIndex = 8;
            this.rdoDAC6.TabStop = true;
            this.rdoDAC6.Text = "DAC 6";
            this.rdoDAC6.UseVisualStyleBackColor = true;
            // 
            // rdoDAC18
            // 
            this.rdoDAC18.AutoSize = true;
            this.rdoDAC18.Location = new System.Drawing.Point(130, 65);
            this.rdoDAC18.Name = "rdoDAC18";
            this.rdoDAC18.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC18.TabIndex = 4;
            this.rdoDAC18.TabStop = true;
            this.rdoDAC18.Text = "DAC 18";
            this.rdoDAC18.UseVisualStyleBackColor = true;
            // 
            // rdoDAC13
            // 
            this.rdoDAC13.AutoSize = true;
            this.rdoDAC13.Location = new System.Drawing.Point(316, 42);
            this.rdoDAC13.Name = "rdoDAC13";
            this.rdoDAC13.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC13.TabIndex = 7;
            this.rdoDAC13.TabStop = true;
            this.rdoDAC13.Text = "DAC 13";
            this.rdoDAC13.UseVisualStyleBackColor = true;
            // 
            // rdoDAC17
            // 
            this.rdoDAC17.AutoSize = true;
            this.rdoDAC17.Location = new System.Drawing.Point(68, 65);
            this.rdoDAC17.Name = "rdoDAC17";
            this.rdoDAC17.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC17.TabIndex = 3;
            this.rdoDAC17.TabStop = true;
            this.rdoDAC17.Text = "DAC 17";
            this.rdoDAC17.UseVisualStyleBackColor = true;
            // 
            // rdoDAC5
            // 
            this.rdoDAC5.AutoSize = true;
            this.rdoDAC5.Location = new System.Drawing.Point(316, 19);
            this.rdoDAC5.Name = "rdoDAC5";
            this.rdoDAC5.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC5.TabIndex = 7;
            this.rdoDAC5.TabStop = true;
            this.rdoDAC5.Text = "DAC 5";
            this.rdoDAC5.UseVisualStyleBackColor = true;
            // 
            // rdoDAC16
            // 
            this.rdoDAC16.AutoSize = true;
            this.rdoDAC16.Location = new System.Drawing.Point(6, 65);
            this.rdoDAC16.Name = "rdoDAC16";
            this.rdoDAC16.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC16.TabIndex = 2;
            this.rdoDAC16.TabStop = true;
            this.rdoDAC16.Text = "DAC 16";
            this.rdoDAC16.UseVisualStyleBackColor = true;
            // 
            // rdoDAC12
            // 
            this.rdoDAC12.AutoSize = true;
            this.rdoDAC12.Location = new System.Drawing.Point(254, 42);
            this.rdoDAC12.Name = "rdoDAC12";
            this.rdoDAC12.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC12.TabIndex = 6;
            this.rdoDAC12.TabStop = true;
            this.rdoDAC12.Text = "DAC 12";
            this.rdoDAC12.UseVisualStyleBackColor = true;
            // 
            // rdoDAC4
            // 
            this.rdoDAC4.AutoSize = true;
            this.rdoDAC4.Location = new System.Drawing.Point(254, 19);
            this.rdoDAC4.Name = "rdoDAC4";
            this.rdoDAC4.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC4.TabIndex = 6;
            this.rdoDAC4.TabStop = true;
            this.rdoDAC4.Text = "DAC 4";
            this.rdoDAC4.UseVisualStyleBackColor = true;
            // 
            // rdoDAC11
            // 
            this.rdoDAC11.AutoSize = true;
            this.rdoDAC11.Location = new System.Drawing.Point(192, 42);
            this.rdoDAC11.Name = "rdoDAC11";
            this.rdoDAC11.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC11.TabIndex = 5;
            this.rdoDAC11.TabStop = true;
            this.rdoDAC11.Text = "DAC 11";
            this.rdoDAC11.UseVisualStyleBackColor = true;
            // 
            // rdoDAC3
            // 
            this.rdoDAC3.AutoSize = true;
            this.rdoDAC3.Location = new System.Drawing.Point(192, 19);
            this.rdoDAC3.Name = "rdoDAC3";
            this.rdoDAC3.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC3.TabIndex = 5;
            this.rdoDAC3.TabStop = true;
            this.rdoDAC3.Text = "DAC 3";
            this.rdoDAC3.UseVisualStyleBackColor = true;
            // 
            // rdoDAC10
            // 
            this.rdoDAC10.AutoSize = true;
            this.rdoDAC10.Location = new System.Drawing.Point(130, 42);
            this.rdoDAC10.Name = "rdoDAC10";
            this.rdoDAC10.Size = new System.Drawing.Size(62, 17);
            this.rdoDAC10.TabIndex = 4;
            this.rdoDAC10.TabStop = true;
            this.rdoDAC10.Text = "DAC 10";
            this.rdoDAC10.UseVisualStyleBackColor = true;
            // 
            // rdoDAC2
            // 
            this.rdoDAC2.AutoSize = true;
            this.rdoDAC2.Location = new System.Drawing.Point(130, 19);
            this.rdoDAC2.Name = "rdoDAC2";
            this.rdoDAC2.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC2.TabIndex = 4;
            this.rdoDAC2.TabStop = true;
            this.rdoDAC2.Text = "DAC 2";
            this.rdoDAC2.UseVisualStyleBackColor = true;
            // 
            // rdoDAC9
            // 
            this.rdoDAC9.AutoSize = true;
            this.rdoDAC9.Location = new System.Drawing.Point(68, 42);
            this.rdoDAC9.Name = "rdoDAC9";
            this.rdoDAC9.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC9.TabIndex = 3;
            this.rdoDAC9.TabStop = true;
            this.rdoDAC9.Text = "DAC 9";
            this.rdoDAC9.UseVisualStyleBackColor = true;
            // 
            // rdoDAC8
            // 
            this.rdoDAC8.AutoSize = true;
            this.rdoDAC8.Location = new System.Drawing.Point(6, 42);
            this.rdoDAC8.Name = "rdoDAC8";
            this.rdoDAC8.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC8.TabIndex = 2;
            this.rdoDAC8.TabStop = true;
            this.rdoDAC8.Text = "DAC 8";
            this.rdoDAC8.UseVisualStyleBackColor = true;
            // 
            // rdoDAC1
            // 
            this.rdoDAC1.AutoSize = true;
            this.rdoDAC1.Location = new System.Drawing.Point(68, 19);
            this.rdoDAC1.Name = "rdoDAC1";
            this.rdoDAC1.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC1.TabIndex = 3;
            this.rdoDAC1.TabStop = true;
            this.rdoDAC1.Text = "DAC 1";
            this.rdoDAC1.UseVisualStyleBackColor = true;
            // 
            // rdoDAC0
            // 
            this.rdoDAC0.AutoSize = true;
            this.rdoDAC0.Location = new System.Drawing.Point(6, 19);
            this.rdoDAC0.Name = "rdoDAC0";
            this.rdoDAC0.Size = new System.Drawing.Size(56, 17);
            this.rdoDAC0.TabIndex = 2;
            this.rdoDAC0.TabStop = true;
            this.rdoDAC0.Text = "DAC 0";
            this.rdoDAC0.UseVisualStyleBackColor = true;
            // 
            // cboDevices
            // 
            this.cboDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDevices.FormattingEnabled = true;
            this.cboDevices.Location = new System.Drawing.Point(65, 12);
            this.cboDevices.Name = "cboDevices";
            this.cboDevices.Size = new System.Drawing.Size(464, 21);
            this.cboDevices.TabIndex = 21;
            this.cboDevices.SelectedIndexChanged += new System.EventHandler(this.cboDevices_SelectedIndexChanged);
            // 
            // gbDeviceOptions
            // 
            this.gbDeviceOptions.Controls.Add(this.lblVREF0);
            this.gbDeviceOptions.Controls.Add(this.btnReset);
            this.gbDeviceOptions.Controls.Add(this.txtVREF0);
            this.gbDeviceOptions.Controls.Add(this.btnPerformSoftPowerDown);
            this.gbDeviceOptions.Controls.Add(this.lblVREF1);
            this.gbDeviceOptions.Controls.Add(this.btnSoftPowerUp);
            this.gbDeviceOptions.Controls.Add(this.txtVREF1);
            this.gbDeviceOptions.Controls.Add(this.txtOffsetDAC0);
            this.gbDeviceOptions.Controls.Add(this.lblGroup1Offset);
            this.gbDeviceOptions.Controls.Add(this.lblOffsetDAC0);
            this.gbDeviceOptions.Controls.Add(this.txtOffsetDAC1);
            this.gbDeviceOptions.Location = new System.Drawing.Point(18, 38);
            this.gbDeviceOptions.Name = "gbDeviceOptions";
            this.gbDeviceOptions.Size = new System.Drawing.Size(511, 96);
            this.gbDeviceOptions.TabIndex = 26;
            this.gbDeviceOptions.TabStop = false;
            this.gbDeviceOptions.Text = "Device Options";
            // 
            // lblVREF0
            // 
            this.lblVREF0.AutoSize = true;
            this.lblVREF0.Location = new System.Drawing.Point(15, 18);
            this.lblVREF0.Name = "lblVREF0";
            this.lblVREF0.Size = new System.Drawing.Size(44, 13);
            this.lblVREF0.TabIndex = 22;
            this.lblVREF0.Text = "VREF 0";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(262, 66);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(154, 20);
            this.btnReset.TabIndex = 30;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // txtVREF0
            // 
            this.txtVREF0.Location = new System.Drawing.Point(65, 15);
            this.txtVREF0.MaxLength = 5;
            this.txtVREF0.Name = "txtVREF0";
            this.txtVREF0.Size = new System.Drawing.Size(40, 20);
            this.txtVREF0.TabIndex = 23;
            this.txtVREF0.Text = "5";
            this.txtVREF0.Validating += new System.ComponentModel.CancelEventHandler(this.txtVREF0_Validating);
            // 
            // btnPerformSoftPowerDown
            // 
            this.btnPerformSoftPowerDown.Location = new System.Drawing.Point(262, 40);
            this.btnPerformSoftPowerDown.Name = "btnPerformSoftPowerDown";
            this.btnPerformSoftPowerDown.Size = new System.Drawing.Size(154, 20);
            this.btnPerformSoftPowerDown.TabIndex = 29;
            this.btnPerformSoftPowerDown.Text = "Perform Soft Power Down";
            this.btnPerformSoftPowerDown.UseVisualStyleBackColor = true;
            this.btnPerformSoftPowerDown.Click += new System.EventHandler(this.btnPerformSoftPowerDown_Click);
            // 
            // lblVREF1
            // 
            this.lblVREF1.AutoSize = true;
            this.lblVREF1.Location = new System.Drawing.Point(15, 44);
            this.lblVREF1.Name = "lblVREF1";
            this.lblVREF1.Size = new System.Drawing.Size(44, 13);
            this.lblVREF1.TabIndex = 24;
            this.lblVREF1.Text = "VREF 1";
            // 
            // btnSoftPowerUp
            // 
            this.btnSoftPowerUp.Location = new System.Drawing.Point(262, 14);
            this.btnSoftPowerUp.Name = "btnSoftPowerUp";
            this.btnSoftPowerUp.Size = new System.Drawing.Size(154, 20);
            this.btnSoftPowerUp.TabIndex = 28;
            this.btnSoftPowerUp.Text = "Perform Soft Power Up";
            this.btnSoftPowerUp.UseVisualStyleBackColor = true;
            this.btnSoftPowerUp.Click += new System.EventHandler(this.btnSoftPowerUp_Click);
            // 
            // txtVREF1
            // 
            this.txtVREF1.Location = new System.Drawing.Point(65, 41);
            this.txtVREF1.MaxLength = 5;
            this.txtVREF1.Name = "txtVREF1";
            this.txtVREF1.Size = new System.Drawing.Size(40, 20);
            this.txtVREF1.TabIndex = 25;
            this.txtVREF1.Text = "5";
            this.txtVREF1.Validating += new System.ComponentModel.CancelEventHandler(this.txtVREF1_Validating);
            // 
            // txtOffsetDAC0
            // 
            this.txtOffsetDAC0.Location = new System.Drawing.Point(203, 15);
            this.txtOffsetDAC0.MaxLength = 4;
            this.txtOffsetDAC0.Name = "txtOffsetDAC0";
            this.txtOffsetDAC0.Size = new System.Drawing.Size(30, 20);
            this.txtOffsetDAC0.TabIndex = 32;
            this.txtOffsetDAC0.Text = "2000";
            this.txtOffsetDAC0.Validating += new System.ComponentModel.CancelEventHandler(this.txtOffsetDAC0_Validating);
            this.txtOffsetDAC0.Validated += new System.EventHandler(this.txtOffsetDAC0_Validated);
            // 
            // lblGroup1Offset
            // 
            this.lblGroup1Offset.AutoSize = true;
            this.lblGroup1Offset.Location = new System.Drawing.Point(122, 44);
            this.lblGroup1Offset.Name = "lblGroup1Offset";
            this.lblGroup1Offset.Size = new System.Drawing.Size(69, 13);
            this.lblGroup1Offset.TabIndex = 35;
            this.lblGroup1Offset.Text = "Offset DAC 1";
            // 
            // lblOffsetDAC0
            // 
            this.lblOffsetDAC0.AutoSize = true;
            this.lblOffsetDAC0.Location = new System.Drawing.Point(122, 18);
            this.lblOffsetDAC0.Name = "lblOffsetDAC0";
            this.lblOffsetDAC0.Size = new System.Drawing.Size(69, 13);
            this.lblOffsetDAC0.TabIndex = 33;
            this.lblOffsetDAC0.Text = "Offset DAC 0";
            // 
            // txtOffsetDAC1
            // 
            this.txtOffsetDAC1.Location = new System.Drawing.Point(202, 41);
            this.txtOffsetDAC1.MaxLength = 4;
            this.txtOffsetDAC1.Name = "txtOffsetDAC1";
            this.txtOffsetDAC1.Size = new System.Drawing.Size(31, 20);
            this.txtOffsetDAC1.TabIndex = 34;
            this.txtOffsetDAC1.Text = "2000";
            this.txtOffsetDAC1.Validating += new System.ComponentModel.CancelEventHandler(this.txtOffsetDAC1_Validating);
            this.txtOffsetDAC1.Validated += new System.EventHandler(this.txtOffsetDAC1_Validated);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // gbDataValues
            // 
            this.gbDataValues.Controls.Add(this.lblVOutCalculated);
            this.gbDataValues.Controls.Add(this.lblVout);
            this.gbDataValues.Controls.Add(this.txtDACChannelOffset);
            this.gbDataValues.Controls.Add(this.lblDACChannelOffset);
            this.gbDataValues.Controls.Add(this.rdoDataValueB);
            this.gbDataValues.Controls.Add(this.txtDACChannelGain);
            this.gbDataValues.Controls.Add(this.lblDACChannelGain);
            this.gbDataValues.Controls.Add(this.rdoDataValueA);
            this.gbDataValues.Controls.Add(this.txtDataValueA);
            this.gbDataValues.Controls.Add(this.lblDataValueA);
            this.gbDataValues.Controls.Add(this.lblDataValueB);
            this.gbDataValues.Controls.Add(this.txtDataValueB);
            this.gbDataValues.Location = new System.Drawing.Point(7, 143);
            this.gbDataValues.Name = "gbDataValues";
            this.gbDataValues.Size = new System.Drawing.Size(365, 81);
            this.gbDataValues.TabIndex = 26;
            this.gbDataValues.TabStop = false;
            this.gbDataValues.Text = "Data Values";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 443);
            this.Controls.Add(this.mainPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Analog Devices AD53xx DenseDAC Eval Board Test Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.gbDACOutputs.ResumeLayout(false);
            this.gbDACOutputs.PerformLayout();
            this.gbDeviceOptions.ResumeLayout(false);
            this.gbDeviceOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.gbDataValues.ResumeLayout(false);
            this.gbDataValues.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label lblDevice;
        private System.Windows.Forms.RadioButton rdoDataValueB;
        private System.Windows.Forms.RadioButton rdoDataValueA;
        private System.Windows.Forms.Label lblDataValueB;
        private System.Windows.Forms.TextBox txtDataValueB;
        private System.Windows.Forms.Label lblDataValueA;
        private System.Windows.Forms.TextBox txtDataValueA;
        private System.Windows.Forms.Label lblDACChannelGain;
        private System.Windows.Forms.TextBox txtDACChannelGain;
        private System.Windows.Forms.Label lblDACChannelOffset;
        private System.Windows.Forms.TextBox txtDACChannelOffset;
        private System.Windows.Forms.RadioButton rdoDAC39;
        private System.Windows.Forms.RadioButton rdoDAC38;
        private System.Windows.Forms.RadioButton rdoDAC37;
        private System.Windows.Forms.RadioButton rdoDAC36;
        private System.Windows.Forms.RadioButton rdoDAC35;
        private System.Windows.Forms.RadioButton rdoDAC34;
        private System.Windows.Forms.RadioButton rdoDAC33;
        private System.Windows.Forms.RadioButton rdoDAC32;
        private System.Windows.Forms.Button btnUpdateAllDACOutputs;
        private System.Windows.Forms.RadioButton rdoDAC31;
        private System.Windows.Forms.RadioButton rdoDAC30;
        private System.Windows.Forms.RadioButton rdoDAC29;
        private System.Windows.Forms.RadioButton rdoDAC28;
        private System.Windows.Forms.RadioButton rdoDAC27;
        private System.Windows.Forms.RadioButton rdoDAC26;
        private System.Windows.Forms.RadioButton rdoDAC25;
        private System.Windows.Forms.RadioButton rdoDAC24;
        private System.Windows.Forms.RadioButton rdoDAC23;
        private System.Windows.Forms.RadioButton rdoDAC22;
        private System.Windows.Forms.RadioButton rdoDAC21;
        private System.Windows.Forms.RadioButton rdoDAC20;
        private System.Windows.Forms.RadioButton rdoDAC19;
        private System.Windows.Forms.RadioButton rdoDAC18;
        private System.Windows.Forms.RadioButton rdoDAC17;
        private System.Windows.Forms.RadioButton rdoDAC16;
        private System.Windows.Forms.Button btnResumeAllDACOutputs;
        private System.Windows.Forms.RadioButton rdoDAC15;
        private System.Windows.Forms.RadioButton rdoDAC14;
        private System.Windows.Forms.RadioButton rdoDAC13;
        private System.Windows.Forms.RadioButton rdoDAC12;
        private System.Windows.Forms.RadioButton rdoDAC11;
        private System.Windows.Forms.RadioButton rdoDAC10;
        private System.Windows.Forms.RadioButton rdoDAC9;
        private System.Windows.Forms.RadioButton rdoDAC8;
        private System.Windows.Forms.Button btnSuspendAllDACOutputs;
        private System.Windows.Forms.GroupBox gbDACOutputs;
        private System.Windows.Forms.RadioButton rdoDAC7;
        private System.Windows.Forms.RadioButton rdoDAC6;
        private System.Windows.Forms.RadioButton rdoDAC5;
        private System.Windows.Forms.RadioButton rdoDAC4;
        private System.Windows.Forms.RadioButton rdoDAC3;
        private System.Windows.Forms.RadioButton rdoDAC2;
        private System.Windows.Forms.RadioButton rdoDAC1;
        private System.Windows.Forms.RadioButton rdoDAC0;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblGroup1Offset;
        private System.Windows.Forms.TextBox txtOffsetDAC1;
        private System.Windows.Forms.Label lblOffsetDAC0;
        private System.Windows.Forms.Button btnPerformSoftPowerDown;
        private System.Windows.Forms.TextBox txtOffsetDAC0;
        private System.Windows.Forms.TextBox txtVREF1;
        private System.Windows.Forms.Button btnSoftPowerUp;
        private System.Windows.Forms.Label lblVREF1;
        private System.Windows.Forms.TextBox txtVREF0;
        private System.Windows.Forms.Label lblVREF0;
        private System.Windows.Forms.ComboBox cboDevices;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label lblVOutCalculated;
        private System.Windows.Forms.Label lblVout;
        private System.Windows.Forms.GroupBox gbDeviceOptions;
        private System.Windows.Forms.GroupBox gbDataValues;

    }
}

