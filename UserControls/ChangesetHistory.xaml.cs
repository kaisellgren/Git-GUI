using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GG.Libraries;
using GG.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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

            ChangesetHistoryGrid.Loaded += (sender, args) =>
            {
                var commits = (EnhancedObservableCollection<Commit>) ChangesetHistoryGrid.ItemsSource;
                //commits.CollectionChanged += (sender2, args2) => Task.Run(() => Application.Current.Dispatcher.BeginInvoke((Action)(RedrawGraph)));

                //RedrawGraph();
            };
        }

        /// <summary>
        /// Recalculates the height for the graph and draws it.
        /// </summary>
        private void RedrawGraph()
        {
            Console.WriteLine("Redraw!");

            // Calculate the height.
            var scrollViewer = UIHelper.FindChild<ScrollViewer>(ChangesetHistoryGrid);
            var totalHeight = scrollViewer.ExtentHeight * 24;

            Graph.Height = totalHeight;

            Console.WriteLine(totalHeight);

            // Redraw.
            var changesetGraph = new ChangesetGraph((RepositoryViewModel)DataContext, Graph);
            changesetGraph.Draw(ChangesetHistoryGrid.Items);
        }

        private void ChangesetHistoryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //var scrollViewer = UIHelper.FindChild<ScrollViewer>(this, "GraphScrollViewer");
            //scrollViewer.ScrollToVerticalOffset(Math.Floor(e.VerticalOffset) * 24);
        }
    }
}