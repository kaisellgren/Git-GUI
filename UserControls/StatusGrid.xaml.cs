using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GG.Libraries;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for StatusGrid.xaml
    /// </summary>
    public partial class StatusGrid : UserControl
    {
        public StatusGrid()
        {
            InitializeComponent();
        }

        private void StatusGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Retrieve repository view model.
            var repositoryTabs = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            var repositoryViewModel = repositoryTabs.SelectedItem as RepositoryViewModel;
            var dataGrid = (DataGrid) sender;

            // Tell repository view model to update status item diff.
            if (repositoryViewModel != null)
                repositoryViewModel.UpdateStatusItemDiff(dataGrid.SelectedItems);
        }
    }
}
