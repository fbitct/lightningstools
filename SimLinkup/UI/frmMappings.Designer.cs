namespace SimLinkup.UI
{
    partial class frmMappings
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
            this.SignalsList = new System.Windows.Forms.ListView();
            this.SourceSignalSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SourceSignalName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationSignalSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationSignalName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // SignalsList
            // 
            this.SignalsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.SourceSignalSource,
            this.SourceSignalName,
            this.DestinationSignalSource,
            this.DestinationSignalName});
            this.SignalsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SignalsList.FullRowSelect = true;
            this.SignalsList.LabelWrap = false;
            this.SignalsList.Location = new System.Drawing.Point(0, 0);
            this.SignalsList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SignalsList.MultiSelect = false;
            this.SignalsList.Name = "SignalsList";
            this.SignalsList.Size = new System.Drawing.Size(635, 280);
            this.SignalsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.SignalsList.TabIndex = 0;
            this.SignalsList.UseCompatibleStateImageBehavior = false;
            this.SignalsList.View = System.Windows.Forms.View.Details;
            // 
            // SourceSignalSource
            // 
            this.SourceSignalSource.Text = "Source Module";
            this.SourceSignalSource.Width = 114;
            // 
            // SourceSignalName
            // 
            this.SourceSignalName.Text = "Source Module Output";
            this.SourceSignalName.Width = 134;
            // 
            // DestinationSignalSource
            // 
            this.DestinationSignalSource.Text = "Destination Module";
            this.DestinationSignalSource.Width = 151;
            // 
            // DestinationSignalName
            // 
            this.DestinationSignalName.Text = "Destination Module Input";
            this.DestinationSignalName.Width = 167;
            // 
            // Signals
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 280);
            this.Controls.Add(this.SignalsList);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Signals";
            this.Text = "Signals";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView SignalsList;
        private System.Windows.Forms.ColumnHeader SourceSignalSource;
        private System.Windows.Forms.ColumnHeader SourceSignalName;
        private System.Windows.Forms.ColumnHeader DestinationSignalSource;
        private System.Windows.Forms.ColumnHeader DestinationSignalName;
    }
}