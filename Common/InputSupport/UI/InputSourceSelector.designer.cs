using System.Windows.Forms;
namespace Common.InputSupport.UI
{
    partial class InputSourceSelector : Form
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
            this.lblPromptText = new System.Windows.Forms.Label();
            this.gbSelectInputSource = new System.Windows.Forms.GroupBox();
            this.txtHelpText = new System.Windows.Forms.TextBox();
            this.rdoNotAssigned = new System.Windows.Forms.RadioButton();
            this.txtKeystroke = new System.Windows.Forms.TextBox();
            this.gbPovDirections = new System.Windows.Forms.GroupBox();
            this.rdoPovUpLeft = new System.Windows.Forms.RadioButton();
            this.rdoPovLeft = new System.Windows.Forms.RadioButton();
            this.rdoPovDownLeft = new System.Windows.Forms.RadioButton();
            this.rdoPovDown = new System.Windows.Forms.RadioButton();
            this.rdoPovDownRight = new System.Windows.Forms.RadioButton();
            this.rdoPovRight = new System.Windows.Forms.RadioButton();
            this.rdoPovUpRight = new System.Windows.Forms.RadioButton();
            this.rdoPovUp = new System.Windows.Forms.RadioButton();
            this.cboJoystickControl = new System.Windows.Forms.ComboBox();
            this.lblJoystickControl = new System.Windows.Forms.Label();
            this.lblDeviceName = new System.Windows.Forms.Label();
            this.cbJoysticks = new System.Windows.Forms.ComboBox();
            this.rdoJoystick = new System.Windows.Forms.RadioButton();
            this.rdoKeystroke = new System.Windows.Forms.RadioButton();
            this.cmdOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbSelectInputSource.SuspendLayout();
            this.gbPovDirections.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPromptText
            // 
            this.lblPromptText.AutoSize = true;
            this.lblPromptText.Location = new System.Drawing.Point(12, 15);
            this.lblPromptText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPromptText.Name = "lblPromptText";
            this.lblPromptText.Size = new System.Drawing.Size(98, 17);
            this.lblPromptText.TabIndex = 0;
            this.lblPromptText.Text = "Control Name:";
            // 
            // gbSelectInputSource
            // 
            this.gbSelectInputSource.Controls.Add(this.txtHelpText);
            this.gbSelectInputSource.Controls.Add(this.rdoNotAssigned);
            this.gbSelectInputSource.Controls.Add(this.txtKeystroke);
            this.gbSelectInputSource.Controls.Add(this.gbPovDirections);
            this.gbSelectInputSource.Controls.Add(this.cboJoystickControl);
            this.gbSelectInputSource.Controls.Add(this.lblJoystickControl);
            this.gbSelectInputSource.Controls.Add(this.lblDeviceName);
            this.gbSelectInputSource.Controls.Add(this.cbJoysticks);
            this.gbSelectInputSource.Controls.Add(this.rdoJoystick);
            this.gbSelectInputSource.Controls.Add(this.rdoKeystroke);
            this.gbSelectInputSource.Location = new System.Drawing.Point(16, 34);
            this.gbSelectInputSource.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbSelectInputSource.Name = "gbSelectInputSource";
            this.gbSelectInputSource.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbSelectInputSource.Size = new System.Drawing.Size(383, 484);
            this.gbSelectInputSource.TabIndex = 1;
            this.gbSelectInputSource.TabStop = false;
            this.gbSelectInputSource.Text = "Select Input Source";
            // 
            // txtHelpText
            // 
            this.txtHelpText.BackColor = System.Drawing.SystemColors.Info;
            this.txtHelpText.ForeColor = System.Drawing.SystemColors.InfoText;
            this.txtHelpText.Location = new System.Drawing.Point(9, 23);
            this.txtHelpText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtHelpText.Multiline = true;
            this.txtHelpText.Name = "txtHelpText";
            this.txtHelpText.ReadOnly = true;
            this.txtHelpText.Size = new System.Drawing.Size(364, 72);
            this.txtHelpText.TabIndex = 4;
            this.txtHelpText.TabStop = false;
            // 
            // rdoNotAssigned
            // 
            this.rdoNotAssigned.AutoSize = true;
            this.rdoNotAssigned.Location = new System.Drawing.Point(9, 103);
            this.rdoNotAssigned.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoNotAssigned.Name = "rdoNotAssigned";
            this.rdoNotAssigned.Size = new System.Drawing.Size(139, 26);
            this.rdoNotAssigned.TabIndex = 9;
            this.rdoNotAssigned.TabStop = true;
            this.rdoNotAssigned.Text = "Unassigned";
            this.rdoNotAssigned.UseVisualStyleBackColor = true;
            // 
            // txtKeystroke
            // 
            this.txtKeystroke.Location = new System.Drawing.Point(39, 160);
            this.txtKeystroke.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtKeystroke.Name = "txtKeystroke";
            this.txtKeystroke.ReadOnly = true;
            this.txtKeystroke.Size = new System.Drawing.Size(307, 22);
            this.txtKeystroke.TabIndex = 8;
            // 
            // gbPovDirections
            // 
            this.gbPovDirections.Controls.Add(this.rdoPovUpLeft);
            this.gbPovDirections.Controls.Add(this.rdoPovLeft);
            this.gbPovDirections.Controls.Add(this.rdoPovDownLeft);
            this.gbPovDirections.Controls.Add(this.rdoPovDown);
            this.gbPovDirections.Controls.Add(this.rdoPovDownRight);
            this.gbPovDirections.Controls.Add(this.rdoPovRight);
            this.gbPovDirections.Controls.Add(this.rdoPovUpRight);
            this.gbPovDirections.Controls.Add(this.rdoPovUp);
            this.gbPovDirections.Location = new System.Drawing.Point(39, 281);
            this.gbPovDirections.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbPovDirections.Name = "gbPovDirections";
            this.gbPovDirections.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbPovDirections.Size = new System.Drawing.Size(308, 185);
            this.gbPovDirections.TabIndex = 7;
            this.gbPovDirections.TabStop = false;
            this.gbPovDirections.Text = "POV Hat Direction";
            // 
            // rdoPovUpLeft
            // 
            this.rdoPovUpLeft.AutoSize = true;
            this.rdoPovUpLeft.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rdoPovUpLeft.Location = new System.Drawing.Point(43, 57);
            this.rdoPovUpLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovUpLeft.Name = "rdoPovUpLeft";
            this.rdoPovUpLeft.Size = new System.Drawing.Size(95, 26);
            this.rdoPovUpLeft.TabIndex = 7;
            this.rdoPovUpLeft.TabStop = true;
            this.rdoPovUpLeft.Text = "UpLeft";
            this.rdoPovUpLeft.UseVisualStyleBackColor = true;
            // 
            // rdoPovLeft
            // 
            this.rdoPovLeft.AutoSize = true;
            this.rdoPovLeft.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rdoPovLeft.Location = new System.Drawing.Point(43, 85);
            this.rdoPovLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovLeft.Name = "rdoPovLeft";
            this.rdoPovLeft.Size = new System.Drawing.Size(71, 26);
            this.rdoPovLeft.TabIndex = 6;
            this.rdoPovLeft.TabStop = true;
            this.rdoPovLeft.Text = "Left";
            this.rdoPovLeft.UseVisualStyleBackColor = true;
            // 
            // rdoPovDownLeft
            // 
            this.rdoPovDownLeft.AutoSize = true;
            this.rdoPovDownLeft.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rdoPovDownLeft.Location = new System.Drawing.Point(24, 113);
            this.rdoPovDownLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovDownLeft.Name = "rdoPovDownLeft";
            this.rdoPovDownLeft.Size = new System.Drawing.Size(117, 26);
            this.rdoPovDownLeft.TabIndex = 5;
            this.rdoPovDownLeft.TabStop = true;
            this.rdoPovDownLeft.Text = "DownLeft";
            this.rdoPovDownLeft.UseVisualStyleBackColor = true;
            // 
            // rdoPovDown
            // 
            this.rdoPovDown.AutoSize = true;
            this.rdoPovDown.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.rdoPovDown.Location = new System.Drawing.Point(116, 142);
            this.rdoPovDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovDown.Name = "rdoPovDown";
            this.rdoPovDown.Size = new System.Drawing.Size(63, 46);
            this.rdoPovDown.TabIndex = 4;
            this.rdoPovDown.TabStop = true;
            this.rdoPovDown.Text = "Down";
            this.rdoPovDown.UseVisualStyleBackColor = true;
            // 
            // rdoPovDownRight
            // 
            this.rdoPovDownRight.AutoSize = true;
            this.rdoPovDownRight.Location = new System.Drawing.Point(161, 113);
            this.rdoPovDownRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovDownRight.Name = "rdoPovDownRight";
            this.rdoPovDownRight.Size = new System.Drawing.Size(129, 26);
            this.rdoPovDownRight.TabIndex = 3;
            this.rdoPovDownRight.TabStop = true;
            this.rdoPovDownRight.Text = "DownRight";
            this.rdoPovDownRight.UseVisualStyleBackColor = true;
            // 
            // rdoPovRight
            // 
            this.rdoPovRight.AutoSize = true;
            this.rdoPovRight.Location = new System.Drawing.Point(180, 85);
            this.rdoPovRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovRight.Name = "rdoPovRight";
            this.rdoPovRight.Size = new System.Drawing.Size(83, 26);
            this.rdoPovRight.TabIndex = 2;
            this.rdoPovRight.TabStop = true;
            this.rdoPovRight.Text = "Right";
            this.rdoPovRight.UseVisualStyleBackColor = true;
            // 
            // rdoPovUpRight
            // 
            this.rdoPovUpRight.AutoSize = true;
            this.rdoPovUpRight.Location = new System.Drawing.Point(161, 57);
            this.rdoPovUpRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovUpRight.Name = "rdoPovUpRight";
            this.rdoPovUpRight.Size = new System.Drawing.Size(107, 26);
            this.rdoPovUpRight.TabIndex = 1;
            this.rdoPovUpRight.TabStop = true;
            this.rdoPovUpRight.Text = "UpRight";
            this.rdoPovUpRight.UseVisualStyleBackColor = true;
            // 
            // rdoPovUp
            // 
            this.rdoPovUp.AutoSize = true;
            this.rdoPovUp.CheckAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.rdoPovUp.Location = new System.Drawing.Point(125, 12);
            this.rdoPovUp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoPovUp.Name = "rdoPovUp";
            this.rdoPovUp.Size = new System.Drawing.Size(40, 46);
            this.rdoPovUp.TabIndex = 0;
            this.rdoPovUp.TabStop = true;
            this.rdoPovUp.Text = "Up";
            this.rdoPovUp.UseVisualStyleBackColor = true;
            // 
            // cboJoystickControl
            // 
            this.cboJoystickControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboJoystickControl.FormattingEnabled = true;
            this.cboJoystickControl.Location = new System.Drawing.Point(101, 247);
            this.cboJoystickControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cboJoystickControl.Name = "cboJoystickControl";
            this.cboJoystickControl.Size = new System.Drawing.Size(244, 24);
            this.cboJoystickControl.TabIndex = 5;
            this.cboJoystickControl.SelectedIndexChanged += new System.EventHandler(this.cboJoystickControl_SelectedIndexChanged);
            // 
            // lblJoystickControl
            // 
            this.lblJoystickControl.AutoSize = true;
            this.lblJoystickControl.Location = new System.Drawing.Point(36, 251);
            this.lblJoystickControl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblJoystickControl.Name = "lblJoystickControl";
            this.lblJoystickControl.Size = new System.Drawing.Size(57, 17);
            this.lblJoystickControl.TabIndex = 4;
            this.lblJoystickControl.Text = "Control:";
            // 
            // lblDeviceName
            // 
            this.lblDeviceName.AutoSize = true;
            this.lblDeviceName.Location = new System.Drawing.Point(35, 222);
            this.lblDeviceName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(55, 17);
            this.lblDeviceName.TabIndex = 3;
            this.lblDeviceName.Text = "Device:";
            // 
            // cbJoysticks
            // 
            this.cbJoysticks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbJoysticks.FormattingEnabled = true;
            this.cbJoysticks.Location = new System.Drawing.Point(101, 218);
            this.cbJoysticks.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbJoysticks.Name = "cbJoysticks";
            this.cbJoysticks.Size = new System.Drawing.Size(244, 24);
            this.cbJoysticks.TabIndex = 2;
            this.cbJoysticks.SelectedIndexChanged += new System.EventHandler(this.cbJoysticks_SelectedIndexChanged);
            // 
            // rdoJoystick
            // 
            this.rdoJoystick.AutoSize = true;
            this.rdoJoystick.Location = new System.Drawing.Point(9, 192);
            this.rdoJoystick.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoJoystick.Name = "rdoJoystick";
            this.rdoJoystick.Size = new System.Drawing.Size(333, 26);
            this.rdoJoystick.TabIndex = 1;
            this.rdoJoystick.TabStop = true;
            this.rdoJoystick.Text = "Joystick or DirectInput Device Input";
            this.rdoJoystick.UseVisualStyleBackColor = true;
            this.rdoJoystick.CheckedChanged += new System.EventHandler(this.rdoJoystick_CheckedChanged);
            // 
            // rdoKeystroke
            // 
            this.rdoKeystroke.AutoSize = true;
            this.rdoKeystroke.Location = new System.Drawing.Point(9, 132);
            this.rdoKeystroke.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoKeystroke.Name = "rdoKeystroke";
            this.rdoKeystroke.Size = new System.Drawing.Size(243, 26);
            this.rdoKeystroke.TabIndex = 0;
            this.rdoKeystroke.TabStop = true;
            this.rdoKeystroke.Text = "Keystroke / Combination";
            this.rdoKeystroke.UseVisualStyleBackColor = true;
            this.rdoKeystroke.CheckedChanged += new System.EventHandler(this.rdoKeystroke_CheckedChanged);
            // 
            // cmdOk
            // 
            this.cmdOk.Location = new System.Drawing.Point(16, 526);
            this.cmdOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(100, 28);
            this.cmdOk.TabIndex = 2;
            this.cmdOk.Text = "OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(123, 526);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // InputSourceSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(428, 570);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.gbSelectInputSource);
            this.Controls.Add(this.lblPromptText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "InputSourceSelector";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Input Source";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.gbSelectInputSource.ResumeLayout(false);
            this.gbSelectInputSource.PerformLayout();
            this.gbPovDirections.ResumeLayout(false);
            this.gbPovDirections.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPromptText;
        private System.Windows.Forms.GroupBox gbSelectInputSource;
        private System.Windows.Forms.ComboBox cbJoysticks;
        private System.Windows.Forms.RadioButton rdoJoystick;
        private System.Windows.Forms.RadioButton rdoKeystroke;
        private System.Windows.Forms.ComboBox cboJoystickControl;
        private System.Windows.Forms.Label lblJoystickControl;
        private System.Windows.Forms.Label lblDeviceName;
        private System.Windows.Forms.GroupBox gbPovDirections;
        private System.Windows.Forms.RadioButton rdoPovLeft;
        private System.Windows.Forms.RadioButton rdoPovDownLeft;
        private System.Windows.Forms.RadioButton rdoPovDown;
        private System.Windows.Forms.RadioButton rdoPovDownRight;
        private System.Windows.Forms.RadioButton rdoPovRight;
        private System.Windows.Forms.RadioButton rdoPovUpRight;
        private System.Windows.Forms.RadioButton rdoPovUp;
        private System.Windows.Forms.RadioButton rdoPovUpLeft;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtKeystroke;
        private System.Windows.Forms.RadioButton rdoNotAssigned;
        private System.Windows.Forms.TextBox txtHelpText;
    }
}