using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.UI;
using Common.MacroProgramming;
using Common.Math;
namespace SimLinkup.UI.UserControls
{
    public partial class SignalsView : UserControl
    {
        private readonly ListViewColumnSorter lvwColumnSorter;
        public SignalsView()
        {
            InitializeComponent();
            lvwColumnSorter = new ListViewColumnSorter();
            lvwColumnSorter.SortColumn = 0;
            lvSignals.ListViewItemSorter = lvwColumnSorter;
        }

        public SignalList<Signal> Signals { get; set; }
        public void Update()
        {
            BuildTreeView();
        }
        private void BuildTreeView()
        {
            tvSignalCategories.BeginUpdate();
            tvSignalCategories.Nodes.Clear();
            if (Signals != null)
            {
                var distinctSignalSources = Signals.GetDistinctSignalSourceNames();
                if (distinctSignalSources != null && distinctSignalSources.Count > 0)
                {
                    foreach (var signalSource in distinctSignalSources)
                    {
                        var tn = new TreeNode(signalSource);
                        tn.Tag = signalSource;
                        tvSignalCategories.Nodes.Add(tn);

                        var signalsThisSource =
                            Signals.GetSignalsFromSource(signalSource);
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
            var signalsThisSource = Signals.GetSignalsFromSource(signalSource);
            lvSignals.SuspendLayout();
            lvSignals.BeginUpdate();
            lvSignals.Clear();
            lvSignals.Groups.Clear();

            lvSignals.Columns.Clear();
            lvSignals.Columns.Add("Signal");
            lvSignals.Columns.Add("Signal Type");
            lvSignals.Columns.Add("Value");

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
                            var lvi = CreateListViewItemFromSignal(signal);
                            lvg.Items.Add(lvi);
                            lvSignals.Items.Add(lvi);
                        }
                    }
                }
                else
                {
                    foreach (var signal in signalsThisCollection)
                    {
                        var lvi = CreateListViewItemFromSignal(signal);
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

        private static ListViewItem CreateListViewItemFromSignal(Signal signal)
        {
            var lvi = new ListViewItem();
            lvi.Text = signal.FriendlyName;
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Name = "Signal Type", Text = signal.SignalType });
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Name = "Value", Text = GetValue(signal) });
            lvi.Tag = signal;
            return lvi;
        }
        private static string GetValue(Signal signal) 
        {
            if (signal is DigitalSignal)
            {
                return ((DigitalSignal)signal).State ? "ON" : "OFF";
            }
            else if (signal is AnalogSignal)
            {
                return ((AnalogSignal)signal).State.FormatDecimal(2);
            }
            else if (signal is TextSignal)
            {
                return ((TextSignal)signal).State;
            }
            return string.Empty;
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
