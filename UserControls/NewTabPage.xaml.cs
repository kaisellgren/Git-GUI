using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GG.Libraries;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for NewTabPage.xaml
    /// </summary>
    public partial class NewTabPage : UserControl
    {
        public NewTabPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fired upon double clicking the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement) e.OriginalSource).DataContext as RepositoryViewModel;

            // The user double clicked one of the recent repositories.
            if (item != null)
            {
                var mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

                // Skip if the repository is already opened.
                if (mainWindowViewModel.RepositoryViewModels.Contains(item) == false)
                {
                    // Load the repository.
                    item.NotOpened = false;
                    item.Load();

                    // Open the tab.
                    mainWindowViewModel.RepositoryViewModels.Add(item);

                    var repositoryTabs = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
                    repositoryTabs.SelectedItem = item;
                }
            }
        }

        /// <summary>
        /// Fired upon pressing Open Local Repository.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenLocalRepository(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowDialog();

            // Open the selected repository, if possible.
            if (dialog.SelectedPath != null && dialog.SelectedPath.Length > 0)
            {
                var repository = new RepositoryViewModel();
                repository.NotOpened = false;
                repository.RepositoryFullPath = dialog.SelectedPath;

                // Try loading the repository information and see if it worked.
                var result = repository.Load();
                if (result == true)
                {
                    var mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

                    // Ask the user for the Name.
                    var nameDialog = new PromptDialog();
                    nameDialog.ResponseText = repository.RepositoryFullPath;
                    nameDialog.Message = "Give a name for this repository:";
                    nameDialog.Title = "Information needed";

                    if (nameDialog.ShowDialog() == true)
                    {
                        repository.Name = nameDialog.ResponseText;

                        // Open the repository and display it visually.
                        mainWindowViewModel.RepositoryViewModels.Add(repository);
                        mainWindowViewModel.RecentRepositories.Add(repository);

                        repository.SetThisRepositoryAsTheActiveTab();
                    }
                }
                else
                {
                    MessageBox.Show(String.Format("Could not open \"{0}\". Are you sure it is an existing Git repository?", dialog.SelectedPath));
                }
            }
        }
    }
}