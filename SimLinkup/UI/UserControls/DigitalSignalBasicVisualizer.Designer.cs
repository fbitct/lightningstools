namespace SimLinkup.UI.UserControls
{
    partial class DigitalSignalBasicVisualizer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkValue = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkValue
            // 
            this.chkValue.AutoSize = true;
            this.chkValue.Location = new System.Drawing.Point(4, 4);
            this.chkValue.Name = "chkValue";
            this.chkValue.Size = new System.Drawing.Size(106, 21);
            this.chkValue.TabIndex = 0;
            this.chkValue.Text = "SignalName";
            this.chkValue.UseVisualStyleBackColor = true;
            // 
            // DigitalSignalBasicVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.chkValue);
            this.Name = "DigitalSignalBasicVisualizer";
            this.Size = new System.Drawing.Size(121, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkValue;
    }
}
