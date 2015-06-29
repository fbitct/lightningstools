namespace SimLinkup.UI.UserControls
{
    partial class SignalsView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvSignals = new System.Windows.Forms.TreeView();
            this.lvSignals = new System.Windows.Forms.ListView();
            this.Source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.34783F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.65218F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.44894F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 786F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1115, 737);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.splitContainer1, 2);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(4, 4);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvSignals);
            this.splitContainer1.Panel1MinSize = 40;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvSignals);
            this.splitContainer1.Size = new System.Drawing.Size(1107, 729);
            this.splitContainer1.SplitterDistance = 465;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // tvSignals
            // 
            this.tvSignals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSignals.FullRowSelect = true;
            this.tvSignals.Location = new System.Drawing.Point(0, 0);
            this.tvSignals.Margin = new System.Windows.Forms.Padding(4);
            this.tvSignals.Name = "tvSignals";
            this.tvSignals.Size = new System.Drawing.Size(465, 729);
            this.tvSignals.TabIndex = 3;
            this.tvSignals.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvSignals_AfterSelect);
            // 
            // lvSignals
            // 
            this.lvSignals.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Source});
            this.lvSignals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSignals.FullRowSelect = true;
            this.lvSignals.LabelWrap = false;
            this.lvSignals.Location = new System.Drawing.Point(0, 0);
            this.lvSignals.Margin = new System.Windows.Forms.Padding(4);
            this.lvSignals.MultiSelect = false;
            this.lvSignals.Name = "lvSignals";
            this.lvSignals.Size = new System.Drawing.Size(637, 729);
            this.lvSignals.TabIndex = 2;
            this.lvSignals.UseCompatibleStateImageBehavior = false;
            this.lvSignals.View = System.Windows.Forms.View.Details;
            this.lvSignals.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvSignals_ColumnClick);
            // 
            // SignalsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Name = "SignalsView";
            this.Size = new System.Drawing.Size(1115, 737);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvSignals;
        private System.Windows.Forms.ListView lvSignals;
        private System.Windows.Forms.ColumnHeader Source;
    }
}
