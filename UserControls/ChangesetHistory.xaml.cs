using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GG.Libraries;
using GG.Models;
using System.Linq;

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
        }

        /// <summary>
        /// When the changeset history data changes, redraw the graph.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataContextChanged(object sender, RoutedEventArgs e)
        {
            var tabControl = (TabControl) UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");

            ChangesetGraph graph = new ChangesetGraph((RepositoryViewModel) DataContext, UIHelper.FindChild<Canvas>(tabControl, "Graph"));
            graph.Draw(ChangesetHistoryGrid.Items);
        }

        private void ChangesetHistoryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //var scrollViewer = UIHelper.FindChild<ScrollViewer>(this, "GraphScrollViewer");
            //scrollViewer.ScrollToVerticalOffset(Math.Floor(e.VerticalOffset) * 24);
        }
    }
}