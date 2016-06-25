using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.MacroProgramming;
using Common.Math;
using Common.UI;
using Common.UI.UserControls;
using Common.Strings;
using System.Collections;
using Common.HardwareSupport;
using SimLinkup.Scripting;
namespace SimLinkup.UI.UserControls
{
    public partial class SignalsView : UserControl
    {
        private readonly ListViewColumnSorter _columnSorter;
        private readonly System.Windows.Forms.Timer _timer;
        public SignalsView()
        {
            InitializeComponent();
            lvSignals.DoubleBuffered(true);
            _columnSorter = new ListViewColumnSorter();
            _columnSorter.SortColumn = 0;
            lvSignals.ListViewItemSorter = _columnSorter;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 20;
            _timer.Tick += (s, e) =>
            {
                UpdateVisualization();
            };
            _timer.Enabled = true;

        }
        public ScriptingContext ScriptingContext { get; set; }
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
                var distinctSignalSources = Signals.GetDistinctSignalSourceNames().OrderBy(x=>x).ToList();
                if (distinctSignalSources != null && distinctSignalSources.Count > 0)
                {
                    foreach (var signalSource in distinctSignalSources)
                    {
                        var signalSourceTreeNode = new TreeNode(signalSource);
                        signalSourceTreeNode.Tag = "SOURCE:" + signalSource;
                        tvSignals.Nodes.Add(signalSourceTreeNode);

                        var signalsThisSource =
                            Signals.GetSignalsFromSource(signalSource);

                        var signalCategoriesThisSource = signalsThisSource.GetDistinctSignalCategories().OrderBy(x=>x).ToList();
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
                                    var signalCollections = signalsThisCategory.GetDistinctSignalCollectionNames().OrderBy(x=>x).ToList();
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
            lvSignals.Clear();
            lvSignals.Groups.Clear();

            var collectionIdentifier = tvSignals.SelectedNode.Tag as string;
            if (!collectionIdentifier.StartsWith("COLLECTION", StringComparison.OrdinalIgnoreCase)) return;
            collectionIdentifier = collectionIdentifier.Substring("COLLECTION:".Length);
            var categoryIdentifier = (tvSignals.SelectedNode.Parent.Tag as string).Substring("CATEGORY:".Length);
            var sourceIdentifier = (tvSignals.SelectedNode.Parent.Parent.Tag as string).Substring("SOURCE:".Length);

            lvSignals.Columns.Clear();
            lvSignals.Columns.Add("Signal");

            lvSignals.BeginUpdate();
            var signalsThisSource = Signals.GetSignalsFromSource(sourceIdentifier);
            var signalsThisCategory = signalsThisSource.GetSignalsByCategory(categoryIdentifier);
            var signalsThisCollection = signalsThisCategory.GetSignalsByCollection(collectionIdentifier);
            if (signalsThisCollection != null)
            {
                var subCollections = signalsThisCollection.GetUniqueSubcollections(collectionIdentifier).OrderBy(x => x).ToList(); ;
                

                if (subCollections != null && subCollections.Count > 0)
                {
                    var signalsAlreadyAdded = new SignalList<Signal>();
                    foreach (var subcollectionName in subCollections)
                    {
                        var lvg = new ListViewGroup(subcollectionName, HorizontalAlignment.Left);
                        lvSignals.Groups.Add(lvg);
                        var signalsThisSubcollection =
                            signalsThisCollection.GetSignalsBySubcollectionName(collectionIdentifier, subcollectionName).OrderBy(x=>x.FriendlyName).ToList();
                        foreach (var signal in signalsThisSubcollection)
                        {
                            var lvi = CreateListViewItemFromSignal(signal);
                            lvg.Items.Add(lvi);
                            lvSignals.Items.Add(lvi);
                            signalsAlreadyAdded.Add(signal);
                        }
                    }
                    foreach (var signal in signalsThisCollection.Except(signalsAlreadyAdded))
                    {
                        var lvi = CreateListViewItemFromSignal(signal);
                        lvSignals.Items.Add(lvi);
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
            lvSignals.Sort();
        }

        private static ListViewItem CreateListViewItemFromSignal(Signal signal)
        {
            var lvi = new ListViewItem();
            lvi.Text = signal.FriendlyName;
            lvi.Tag = signal;
            return lvi;
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
            if (e.Column == _columnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (_columnSorter.Order == SortOrder.Ascending)
                {
                    _columnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    _columnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _columnSorter.SortColumn = e.Column;
                _columnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            lvSignals.Sort();
        }

        private void UpdateVisualization()
        {
            if (pbVisualization.Image == null)
            {
                pbVisualization.Image = new Bitmap(pbVisualization.ClientSize.Width, pbVisualization.ClientSize.Height);
            }

            using (var graphics = Graphics.FromImage(pbVisualization.Image))
            {
                var targetRectangle = new Rectangle(0, 0, pbVisualization.Image.Width, pbVisualization.Image.Height);
                if (SelectedSignal == null)
                {
                    UpdateModuleVisualization(graphics, targetRectangle);
                }
                else
                {
                    UpdateSignalGraph(graphics, targetRectangle);
                }

            }
            pbVisualization.Invalidate();
            pbVisualization.Update();
        }
        private void UpdateModuleVisualization(Graphics graphics, Rectangle targetRectangle)
        {
            var hsm = SelectedHardwareSupportModule;
            if (hsm == null) return;
            hsm.Render(graphics, targetRectangle);
        }

        private void UpdateSignalGraph(Graphics graphics, Rectangle targetRectangle)
        {
            SelectedSignal.DrawGraph(graphics, targetRectangle);
        }
        private ListViewItem SelectedListViewItem 
        {
            get { return lvSignals.SelectedItems != null && lvSignals.SelectedItems.Count > 0 ? lvSignals.SelectedItems[0] : null; } 
        }
        private Signal SignalFor(ListViewItem listViewItem)
        {
            return listViewItem != null ? listViewItem.Tag as Signal : null;
        }
        private Signal SelectedSignal
        {
            get
            {
                return SignalFor(SelectedListViewItem);
            }
        }
        private IHardwareSupportModule SelectedHardwareSupportModule
        {
            get
            {
                return HardwareSupportModuleFor(tvSignals.SelectedNode);
            }
        }
        private IHardwareSupportModule HardwareSupportModuleFor(TreeNode treeNode)
        {
            return treeNode != null && ScriptingContext !=null 
                ? ScriptingContext.HardwareSupportModules
                    .Where(x => x.FriendlyName == treeNode.Text)
                    .FirstOrDefault() 
                : null;
        }
    }
}
