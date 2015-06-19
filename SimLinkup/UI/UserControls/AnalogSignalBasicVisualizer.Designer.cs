namespace SimLinkup.UI.UserControls
{
    partial class AnalogSignalBasicVisualizer
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
            this.lblSignalName = new System.Windows.Forms.Label();
            this.txtSignalValue = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblSignalName
            // 
            this.lblSignalName.AutoSize = true;
            this.lblSignalName.Location = new System.Drawing.Point(142, 6);
            this.lblSignalName.Name = "lblSignalName";
            this.lblSignalName.Size = new System.Drawing.Size(88, 17);
            this.lblSignalName.TabIndex = 0;
            this.lblSignalName.Text = "Signal Name";
            // 
            // txtSignalValue
            // 
            this.txtSignalValue.Location = new System.Drawing.Point(3, 3);
            this.txtSignalValue.Name = "txtSignalValue";
            this.txtSignalValue.Size = new System.Drawing.Size(133, 22);
            this.txtSignalValue.TabIndex = 1;
            // 
            // AnalogSignalBasicVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.txtSignalValue);
            this.Controls.Add(this.lblSignalName);
            this.Name = "AnalogSignalBasicVisualizer";
            this.Size = new System.Drawing.Size(233, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSignalName;
        private System.Windows.Forms.TextBox txtSignalValue;
    }
}
