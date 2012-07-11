using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
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
            var item = (RepositoryViewModel) ((FrameworkElement) e.OriginalSource).DataContext;

            if (item == null)
                return;

            // Find the tab contrl.
            var repositoryTabs = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");

            // The user double clicked one of the recent repositories.
            var mainWindowViewModel = (MainWindowViewModel) Application.Current.MainWindow.DataContext;

            // Skip if the repository is already opened.
            if (mainWindowViewModel.RepositoryViewModels.Contains(item))
            {
                repositoryTabs.SelectedItem = item;
                return;
            }

            // Load the repository.
            item.NotOpened = false;
            item.Init();

            // Open the tab.
            mainWindowViewModel.RepositoryViewModels.Add(item);
            repositoryTabs.SelectedItem = item;
        }

        /// <summary>
        /// Fired upon pressing Open Local Repository.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenLocalRepository(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            dialog.ShowDialog();

            // Open the selected repository, if possible.
            if (!String.IsNullOrEmpty(dialog.SelectedPath))
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
            var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Create and choose the folder for your new repository."
            };

            dialog.ShowDialog();

            // Open the selected folder if possible.
            if (!String.IsNullOrEmpty(dialog.SelectedPath))
            {
                LibGit2Sharp.Repository.Init(dialog.SelectedPath).Dispose();

                if (OpenNewRepository(dialog.SelectedPath) == false)
                    MessageBox.Show(String.Format("Something went wrong with the creation of the new repository. Try again."));
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
            var repository = new RepositoryViewModel
            {
                NotOpened = false,
                RepositoryFullPath = path
            };

            // Try loading the repository information and see if it worked.
            var result = repository.Init();
            if (result)
            {
                var mainWindowViewModel = (MainWindowViewModel) Application.Current.MainWindow.DataContext;

                // Ask the user for the Name.
                var nameDialog = new PromptDialog
                {
                    ResponseText = repository.RepositoryFullPath.Split(System.IO.Path.DirectorySeparatorChar).Last(),
                    Message = "Give a name for this repository:",
                    Title = "Information needed"
                };

                repository.Name = nameDialog.ShowDialog() == true ? nameDialog.ResponseText : repository.RepositoryFullPath;

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

        private void NewTabPageLoaded(object sender, RoutedEventArgs e)
        {
            RecentRepositoriesList.Focus();
        }
    }
}