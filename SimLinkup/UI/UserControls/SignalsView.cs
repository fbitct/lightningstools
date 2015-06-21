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
        public void UpdateContents()
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
                        var signalSourceTreeNode = new TreeNode(signalSource);
                        signalSourceTreeNode.Tag = signalSource;
                        tvSignalCategories.Nodes.Add(signalSourceTreeNode);

                        var signalsThisSource =
                            Signals.GetSignalsFromSource(signalSource);
                        var signalCollections = signalsThisSource.GetDistinctSignalCollectionNames();
                        if (signalCollections != null)
                        {
                            foreach (var signalCollectionName in signalCollections)
                            {
                                var signalCollectionTreeNode = new TreeNode(signalCollectionName);
                                signalCollectionTreeNode.Tag = signalCollectionName;
                                signalSourceTreeNode.Nodes.Add(signalCollectionTreeNode);
                            }
                        }
                    }
                }
            }
            //tvSignalCategories.Sort();
            tvSignalCategories.EndUpdate();
        }

        private void UpdateListViewAfterTreeViewItemSelect()
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
            lvSignals.Columns.Add("Value");

            var signalsThisCollection = signalsThisSource.GetSignalsByCollection(signalCollectionName);
            if (signalsThisCollection == null && tvSignalCategories.SelectedNode.Parent !=null)
            {
                signalsThisCollection = signalsThisSource.GetSignalsByCollection(tvSignalCategories.SelectedNode.Parent.Tag as string);
            }
            if (signalsThisCollection != null)
            {
                var subSources = signalsThisCollection.GetUniqueSubSources();

                if (subSources != null && subSources.Count > 0)
                {
                    var signalsAlreadyAdded = new SignalList<Signal>();
                    foreach (var subSource in subSources)
                    {
                        var lvg = new ListViewGroup(subSource, HorizontalAlignment.Left);
                        lvSignals.Groups.Add(lvg);
                        var signalsThisSubsource =
                            signalsThisCollection.GetSignalsBySubSourceFriendlyName(subSource);
                        foreach (var signal in signalsThisSubsource)
                        {
                            var lvi = CreateListViewItemFromSignal(signal);
                            RegisterForSignalStateChangedEvents(lvi, signal);
                            lvg.Items.Add(lvi);
                            lvSignals.Items.Add(lvi);
                            signalsAlreadyAdded.Add(signal);
                        }
                    }
                    foreach (var signal in signalsThisCollection.Except(signalsAlreadyAdded))
                    {
                        var lvi = CreateListViewItemFromSignal(signal);
                        RegisterForSignalStateChangedEvents(lvi, signal);
                        lvSignals.Items.Add(lvi);
                    }
                }
                else
                {
                    foreach (var signal in signalsThisCollection)
                    {
                        var lvi = CreateListViewItemFromSignal(signal);
                        RegisterForSignalStateChangedEvents(lvi, signal);
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
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Name = "Value", Text = GetValue(signal) });
            lvi.Tag = signal;
            return lvi;
        }
        private void RegisterForSignalStateChangedEvents(ListViewItem listViewItem, Signal signal)
        {
            if (signal is DigitalSignal) 
            { 
                ((DigitalSignal)signal).SignalChanged += (s, e) => { UpdateSignalValue(listViewItem, signal); }; 
            }
            else if (signal is AnalogSignal) 
            { 
                ((AnalogSignal)signal).SignalChanged += (s, e) => { UpdateSignalValue(listViewItem, signal); }; 
            }
            else if (signal is TextSignal) 
            { 
                ((TextSignal)signal).SignalChanged += (s, e) => { UpdateSignalValue(listViewItem, signal); }; 
            }
        }

        private void UpdateSignalValue(ListViewItem listViewItem, Signal signal)
        {
            listViewItem.SubItems["Value"].Text = GetValue(signal);
            lvSignals.Update();
        }
        private static string GetValue(Signal signal) 
        {
            if (signal is DigitalSignal)
            {
                return ((DigitalSignal)signal).State ? "  \u2611" : "  \u2610";
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
                UpdateListViewAfterTreeViewItemSelect();
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
