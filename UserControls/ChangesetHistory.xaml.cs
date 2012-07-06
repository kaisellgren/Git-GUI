using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GG.Libraries;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for ChangesetHistory.xaml
    /// </summary>
    public partial class ChangesetHistory : UserControl
    {
        public ChangesetHistory()
        {
            InitializeComponent();

            ChangesetHistoryGrid.Loaded += (sender, args) => RedrawGraph();
        }

        /// <summary>
        /// Recalculates the height for the graph and draws it.
        /// </summary>
        public void RedrawGraph()
        {
            // Redraw the graph.
            var changesetGraph = new ChangesetGraph((RepositoryViewModel) DataContext, Graph);
            changesetGraph.Draw(ChangesetHistoryGrid.Items);

            // Update the height for the Graph element.
            Graph.Height = changesetGraph.TotalHeight;

            // TODO: Set width also, and for the datagrid as well!
        }

        private void ChangesetHistoryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = UIHelper.FindChild<ScrollViewer>(this, "GraphScrollViewer");
            scrollViewer.ScrollToVerticalOffset(Math.Floor(e.VerticalOffset) * 24);

            var dataGrid = UIHelper.FindChild<DataGrid>(this, "ChangesetHistoryGrid");
            scrollViewer.Height = Math.Abs(dataGrid.ActualHeight - 24);
        }
    }
}