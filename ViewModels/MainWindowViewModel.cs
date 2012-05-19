using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GG.Libraries;
using GG.ViewModels;

namespace GG
{
    class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<RepositoryViewModel> RepositoryViewModels { get; set; }
        public ObservableCollection<RepositoryViewModel> RecentRepositories { get; set; }

        public MainWindowViewModel()
        {
            RepositoryViewModels = new ObservableCollection<RepositoryViewModel> { };
            RecentRepositories = new ObservableCollection<RepositoryViewModel> { };

            CreateTabCommand = new DelegateCommand(CreateTab);
            CloseTabCommand = new DelegateCommand(CloseTab);
        }

        public void Load()
        {
            // C:/Program Files (x86)/linux-stable
            // Z:/www/git2
            // Z:/www/test-repo
            // C:/Program Files (x86)/symfony
            // C:/Program Files (x86)/node

            // Add some test repositories.
            RepositoryViewModel repo = new RepositoryViewModel { Name = "Git test repository", RepositoryFullPath = "Z:/www/test-repo" };
            repo.Load();

            RepositoryViewModel repo2 = new RepositoryViewModel { Name = "New tab", RepositoryFullPath = null, NotOpened = true };
            repo2.Load();

            RepositoryViewModels.Add(repo);
            RepositoryViewModels.Add(repo2);

            // Add some "recent repositories".
            RecentRepositories.Add(repo);
        }

        #region Commands.

        public DelegateCommand CreateTabCommand { get; private set; }
        public DelegateCommand CloseTabCommand { get; private set; }

        /// <summary>
        /// Closes the current tab.
        /// </summary>
        /// <param name="action"></param>
        private void CloseTab(object action)
        {
            TabControl tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            ObservableCollection<RepositoryViewModel> repositories = tabControl.ItemsSource as ObservableCollection<RepositoryViewModel>;
            repositories.Remove(tabControl.SelectedContent as RepositoryViewModel);
        }

        /// <summary>
        /// Creates a new tab.
        /// </summary>
        /// <param name="action"></param>
        private void CreateTab(object action)
        {
            TabControl tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            MainWindowViewModel mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            RepositoryViewModel repository = new RepositoryViewModel
            {
                Name = "New Tab",
                NotOpened = true,
                RepositoryFullPath = ""
            };

            mainWindowViewModel.RepositoryViewModels.Add(repository);

            tabControl.SelectedItem = repository;
        }

        #endregion
    }
}
