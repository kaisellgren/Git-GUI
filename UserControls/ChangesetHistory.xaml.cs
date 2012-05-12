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
            ChangesetGraph graph = new ChangesetGraph((RepositoryViewModel) DataContext, Graph);
            graph.Draw(ChangesetHistoryGrid.Items);
        }
    }
}