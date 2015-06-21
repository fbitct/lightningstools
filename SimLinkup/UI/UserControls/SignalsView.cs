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
            tvSignals.BeginUpdate();
            tvSignals.Nodes.Clear();
            if (Signals != null)
            {
                var distinctSignalSources = Signals.GetDistinctSignalSourceNames();
                if (distinctSignalSources != null && distinctSignalSources.Count > 0)
                {
                    foreach (var signalSource in distinctSignalSources)
                    {
                        var signalSourceTreeNode = new TreeNode(signalSource);
                        signalSourceTreeNode.Tag = "SOURCE:" + signalSource;
                        tvSignals.Nodes.Add(signalSourceTreeNode);

                        var signalsThisSource =
                            Signals.GetSignalsFromSource(signalSource);

                        var signalCategoriesThisSource = signalsThisSource.GetDistinctSignalCategories();
                        if (signalCategoriesThisSource != null)
                        {
                            foreach (var signalCategory in signalCategoriesThisSource)
                            {
                                var signalCategoryTreeNode = new TreeNode(signalCategory);
                                signalCategoryTreeNode.Tag = "CATEGORY:" + signalCategory;
                                signalSourceTreeNode.Nodes.Add(signalCategoryTreeNode);
                                var signalsThisCategory = signalsThisSource.GetSignalsByCategory(signalCategory);
                                if (signalsThisCategory != null)
                                {
                                    var signalCollections = signalsThisCategory.GetDistinctSignalCollectionNames();
                                    if (signalCollections != null)
                                    {
                                        foreach (var signalCollectionName in signalCollections)
                                        {
                                            var signalCollectionTreeNode = new TreeNode(signalCollectionName);
                                            signalCollectionTreeNode.Tag = "COLLECTION:" + signalCollectionName;
                                            signalCategoryTreeNode.Nodes.Add(signalCollectionTreeNode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //tvSignalCategories.Sort();
            tvSignals.EndUpdate();
        }

        private void UpdateListViewAfterTreeViewItemSelect()
        {
            lvSignals.SuspendLayout();
            lvSignals.BeginUpdate();
            lvSignals.Clear();
            lvSignals.Groups.Clear();

            var collectionIdentifier = tvSignals.SelectedNode.Tag as string;
            if (!collectionIdentifier.StartsWith("COLLECTION", StringComparison.OrdinalIgnoreCase)) return;
            collectionIdentifier = collectionIdentifier.Substring("COLLECTION:".Length);
            var categoryIdentifier = (tvSignals.SelectedNode.Parent.Tag as string).Substring("CATEGORY:".Length);
            var sourceIdentifier = (tvSignals.SelectedNode.Parent.Parent.Tag as string).Substring("SOURCE:".Length);

            lvSignals.Columns.Clear();
            lvSignals.Columns.Add("Signal");
            lvSignals.Columns.Add("Value");

            var signalsThisSource = Signals.GetSignalsFromSource(sourceIdentifier);
            var signalsThisCategory = signalsThisSource.GetSignalsByCategory(categoryIdentifier);
            var signalsThisCollection = signalsThisCategory.GetSignalsByCollection(collectionIdentifier);

            if (signalsThisCollection != null)
            {
                var subCollections = signalsThisCollection.GetUniqueSubcollections(collectionIdentifier);

                if (subCollections != null && subCollections.Count > 0)
                {
                    var signalsAlreadyAdded = new SignalList<Signal>();
                    foreach (var subcollectionName in subCollections)
                    {
                        var lvg = new ListViewGroup(subcollectionName, HorizontalAlignment.Left);
                        lvSignals.Groups.Add(lvg);
                        var signalsThisSubcollection =
                            signalsThisCollection.GetSignalsBySubcollectionName(collectionIdentifier, subcollectionName);
                        foreach (var signal in signalsThisSubcollection)
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
            lvSignals.Update();
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
                var aSig = ((AnalogSignal)signal);
                var value= aSig.State.FormatDecimal(aSig.Precision >0 ? aSig.Precision: 2);
                if (aSig.IsVoltage)
                {
                    value += "V";
                }
                return value;
            }
            else if (signal is TextSignal)
            {
                return ((TextSignal)signal).State;
            }
            return string.Empty;
        }


        private void tvSignals_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvSignals.SelectedNode.Tag != null && tvSignals.SelectedNode.Parent != null &&
                tvSignals.SelectedNode.Parent.Tag != null)
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
