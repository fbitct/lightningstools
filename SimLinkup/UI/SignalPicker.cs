using System;
using System.Windows.Forms;
using Common.UI;
using SimLinkup.Scripting;

namespace SimLinkup.UI
{
    public partial class SignalPicker : Form
    {
        private readonly ListViewColumnSorter lvwColumnSorter;

        public SignalPicker()
        {
            InitializeComponent();
            lvwColumnSorter = new ListViewColumnSorter();
            lvwColumnSorter.SortColumn = 0;
            lvSignals.ListViewItemSorter = lvwColumnSorter;
        }

        public ScriptingContext ScriptingContext { get; set; }

        private void SignalPicker_Load(object sender, EventArgs e)
        {
            BuildTreeView();
        }

        private void BuildTreeView()
        {
            tvSignalCategories.BeginUpdate();
            tvSignalCategories.Nodes.Clear();
            if (ScriptingContext != null)
            {
                var distinctSignalSources = ScriptingContext.AllSignals.GetDistinctSignalSourceNames();
                if (distinctSignalSources != null && distinctSignalSources.Count > 0)
                {
                    foreach (var signalSource in distinctSignalSources)
                    {
                        var tn = new TreeNode(signalSource);
                        tn.Tag = signalSource;
                        tvSignalCategories.Nodes.Add(tn);

                        var signalsThisSource =
                            ScriptingContext.AllSignals.GetSignalsFromSource(signalSource);
                        var signalCollections = signalsThisSource.GetDistinctSignalCollectionNames();
                        if (signalCollections != null)
                        {
                            foreach (var signalCollectionName in signalCollections)
                            {
                                var tn2 = new TreeNode(signalCollectionName);
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
            var signalCollectionName = tvSignalCategories.SelectedNode.Tag as string;
            var signalSource = tvSignalCategories.SelectedNode.Parent.Tag;
            var signalsThisSource = ScriptingContext.AllSignals.GetSignalsFromSource(signalSource);
            lvSignals.SuspendLayout();
            lvSignals.BeginUpdate();
            lvSignals.Clear();
            lvSignals.Groups.Clear();

            lvSignals.Columns.Clear();
            lvSignals.Columns.Add("Signal");
            lvSignals.Columns.Add("Signal Type");

            var signalsThisCollection = signalsThisSource.GetSignalsByCollection(signalCollectionName);

            if (signalsThisCollection != null)
            {
                var subSources = signalsThisCollection.GetUniqueSubSources();

                if (subSources != null && subSources.Count > 0)
                {
                    foreach (var subSource in subSources)
                    {
                        var lvg = new ListViewGroup(subSource, HorizontalAlignment.Left);
                        lvSignals.Groups.Add(lvg);
                        var signalsThisSubsource =
                            signalsThisCollection.GetSignalsBySubSourceFriendlyName(subSource);
                        foreach (var signal in signalsThisSubsource)
                        {
                            var lvi = new ListViewItem();
                            lvi.Text = signal.FriendlyName;
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem
                                                 {Name = "Signal Type", Text = signal.SignalType});
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
                        var lvi = new ListViewItem();
                        lvi.Text = signal.FriendlyName;
                        lvi.Tag = signal;
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem
                                             {Name = "Signal Type", Text = signal.SignalType});
                        lvSignals.Items.Add(lvi);
                    }
                }
            }

            for (var i = 0; i < lvSignals.Columns.Count; i++)
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
            if (tvSignalCategories.SelectedNode.Tag != null && tvSignalCategories.SelectedNode.Parent != null &&
                tvSignalCategories.SelectedNode.Parent.Tag != null)
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
            lvSignals.Sort();
        }
    }
}