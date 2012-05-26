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
using GG.UserControls.Dialogs;

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
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();

            // Open the selected repository, if possible.
            if (dialog.SelectedPath != null && dialog.SelectedPath.Length > 0)
            {
                if (OpenNewRepository(dialog.SelectedPath) == false)
                    MessageBox.Show(String.Format("Could not open \"{0}\". Are you sure it is an existing Git repository?", dialog.SelectedPath));
            }

            dialog.Dispose();
        }

        /// <summary>
        /// Fired upon pressing Create Local Repository.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreateLocalRepository(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            dialog.Description = "Create and choose the folder for your new repository.";
            dialog.ShowDialog();

            // Open the selected folder if possible.
            if (dialog.SelectedPath != null && dialog.SelectedPath.Length > 0)
            {
                LibGit2Sharp.Repository.Init(dialog.SelectedPath).Dispose();

                if (OpenNewRepository(dialog.SelectedPath) == false)
                    MessageBox.Show(String.Format("Something went wrong with the creation of the new repository. Try again.", dialog.SelectedPath));
            }

            dialog.Dispose();
        }

        /// <summary>
        /// A helper method for opening a new repository for the given path.
        /// 
        /// This will bring up a question regarding the name to use and initialize the repository view model and load it up in the tab control.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool OpenNewRepository(string path)
        {
            var repository = new RepositoryViewModel();
            repository.NotOpened = false;
            repository.RepositoryFullPath = path;

            // Try loading the repository information and see if it worked.
            var result = repository.Load();
            if (result == true)
            {
                var mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

                // Ask the user for the Name.
                var nameDialog = new PromptDialog();
                nameDialog.ResponseText = repository.RepositoryFullPath.Split(System.IO.Path.DirectorySeparatorChar).Last(); // Default to the folder name.
                nameDialog.Message = "Give a name for this repository:";
                nameDialog.Title = "Information needed";

                if (nameDialog.ShowDialog() == true)
                    repository.Name = nameDialog.ResponseText;
                else
                    repository.Name = repository.RepositoryFullPath;

                // Open the repository and display it visually.
                mainWindowViewModel.RepositoryViewModels.Add(repository);
                mainWindowViewModel.RecentRepositories.Add(repository);

                repository.SetThisRepositoryAsTheActiveTab();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}