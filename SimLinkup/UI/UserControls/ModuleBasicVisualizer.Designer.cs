namespace SimLinkup.UI.UserControls
{
    partial class ModuleBasicVisualizer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleBasicVisualizer));
            this.panel = new System.Windows.Forms.Panel();
            this.btnShowSignals = new System.Windows.Forms.Button();
            this.lblModuleName = new System.Windows.Forms.Label();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.Controls.Add(this.btnShowSignals);
            this.panel.Controls.Add(this.lblModuleName);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(623, 65);
            this.panel.TabIndex = 0;
            // 
            // btnShowSignals
            // 
            this.btnShowSignals.Image = ((System.Drawing.Image)(resources.GetObject("btnShowSignals.Image")));
            this.btnShowSignals.Location = new System.Drawing.Point(13, 17);
            this.btnShowSignals.Name = "btnShowSignals";
            this.btnShowSignals.Size = new System.Drawing.Size(32, 32);
            this.btnShowSignals.TabIndex = 1;
            this.btnShowSignals.UseVisualStyleBackColor = true;
            this.btnShowSignals.Click += new System.EventHandler(this.btnShowSignals_Click);
            // 
            // lblModuleName
            // 
            this.lblModuleName.AutoSize = true;
            this.lblModuleName.Location = new System.Drawing.Point(51, 25);
            this.lblModuleName.Name = "lblModuleName";
            this.lblModuleName.Size = new System.Drawing.Size(95, 17);
            this.lblModuleName.TabIndex = 0;
            this.lblModuleName.Text = "Module Name";
            // 
            // ModuleBasicVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Name = "ModuleBasicVisualizer";
            this.Size = new System.Drawing.Size(623, 65);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Button btnShowSignals;
        private System.Windows.Forms.Label lblModuleName;
    }
}
