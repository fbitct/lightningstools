using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common.MacroProgramming;

namespace SimLinkup.UI
{
    public partial class SignalPicker : Form
    {
        private Common.UI.ListViewColumnSorter lvwColumnSorter;
        public Scripting.ScriptingContext ScriptingContext { get; set; }
        public SignalPicker()
        {
            InitializeComponent();
            lvwColumnSorter = new Common.UI.ListViewColumnSorter();
            lvwColumnSorter.SortColumn = 0;
            this.lvSignals.ListViewItemSorter = lvwColumnSorter;


        }

        private void SignalPicker_Load(object sender, EventArgs e)
        {
            BuildTreeView();
        }

        private void BuildTreeView()
        {
            tvSignalCategories.BeginUpdate();
            tvSignalCategories.Nodes.Clear();
            if (this.ScriptingContext != null)
            {
                List<string> distinctSignalSources = this.ScriptingContext.AllSignals.GetDistinctSignalSourceNames();
                if (distinctSignalSources != null && distinctSignalSources.Count >0)
                {
                    foreach (var signalSource in distinctSignalSources)
                    {
                        TreeNode tn = new TreeNode(signalSource.ToString());
                        tn.Tag = signalSource;
                        tvSignalCategories.Nodes.Add(tn);

                        SignalList<Signal> signalsThisSource = this.ScriptingContext.AllSignals.GetSignalsFromSource(signalSource);
                        List<string> signalCollections = signalsThisSource.GetDistinctSignalCollectionNames();
                        if (signalCollections != null)
                        {
                            foreach (var signalCollectionName in signalCollections)
                            {
                                TreeNode tn2= new TreeNode(signalCollectionName);
                                tn2.Tag = signalCollectionName;
                                tn.Nodes.Add(tn2);
                            }
                        }
                    }
                }
            }
            //tvSignalCategories.Sort();
            tvSignalCategories.EndUpdate();
        }
        private void UpdateListView()
        {
            string signalCollectionName = tvSignalCategories.SelectedNode.Tag as string;
            object signalSource = tvSignalCategories.SelectedNode.Parent.Tag;
            SignalList<Signal> signalsThisSource =this.ScriptingContext.AllSignals.GetSignalsFromSource(signalSource);
            lvSignals.SuspendLayout();
            lvSignals.BeginUpdate();
            lvSignals.Clear();
            lvSignals.Groups.Clear();

            lvSignals.Columns.Clear();
            lvSignals.Columns.Add("Signal");
            lvSignals.Columns.Add("Signal Type");

            SignalList<Signal> signalsThisCollection = signalsThisSource.GetSignalsByCollection(signalCollectionName);

            if (signalsThisCollection != null)
            {
                List<string> subSources = signalsThisCollection.GetUniqueSubSources();

                if (subSources != null && subSources.Count >0)
                {
                    foreach (var subSource in subSources)
                    {
                        ListViewGroup lvg = new ListViewGroup(subSource, HorizontalAlignment.Left);
                        lvSignals.Groups.Add(lvg);
                        SignalList<Signal> signalsThisSubsource = signalsThisCollection.GetSignalsBySubSourceFriendlyName(subSource);
                        foreach (var signal in signalsThisSubsource)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = signal.FriendlyName;
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "Signal Type", Text = signal.SignalType });
                            lvi.Tag = signal;
                            lvg.Items.Add(lvi);
                            lvSignals.Items.Add(lvi);
                        }
                    }
                }
                else
                {
                    foreach (var signal in signalsThisCollection)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = signal.FriendlyName;
                        lvi.Tag = signal;
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "Signal Type", Text = signal.SignalType });
                        lvSignals.Items.Add(lvi);
                    }
                }
            }

            for (int i = 0; i < lvSignals.Columns.Count; i++)
            {
                lvSignals.Columns[i].TextAlign = HorizontalAlignment.Left;
                lvSignals.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            lvSignals.EndUpdate();
            lvSignals.ResumeLayout();
            lvSignals.Sort();

        }

        private void tvSignalCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvSignalCategories.SelectedNode.Tag != null && tvSignalCategories.SelectedNode.Parent != null && tvSignalCategories.SelectedNode.Parent.Tag != null)
            {
                UpdateListView();
            }
            else
            {
                ClearListView();
            }

        }
        private void ClearListView()
        {
            lvSignals.Clear();
            lvSignals.Groups.Clear();
        }

        private void lvSignals_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lvSignals.Sort();

        }

    }
}
