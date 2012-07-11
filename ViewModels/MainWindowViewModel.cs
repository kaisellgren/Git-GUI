using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GG.Libraries;
using GG.ViewModels;

namespace GG
{
    class MainWindowViewModel : BaseViewModel
    {
        public Configuration Config { get; private set; }

        public ObservableCollection<RepositoryViewModel> RecentRepositories { get; set; }
        private ObservableCollection<RepositoryViewModel> repositoryViewModels;
        public ObservableCollection<RepositoryViewModel> RepositoryViewModels
        {
            get
            {
                if (repositoryViewModels == null)
                {
                    repositoryViewModels = new ObservableCollection<RepositoryViewModel>();
                    var itemsView = (IEditableCollectionView) CollectionViewSource.GetDefaultView(repositoryViewModels);
                    itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
                }

                return repositoryViewModels;
            }
        }

        public MainWindowViewModel()
        {
            RecentRepositories = new ObservableCollection<RepositoryViewModel>() { };

            RepositoryViewModels.CollectionChanged += RepositoryViewModelsOnCollectionChanged;

            CreateTabCommand = new DelegateCommand(CreateTab);
            CloseTabCommand = new DelegateCommand(CloseTab);
        }

        private void RepositoryViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            // Update the configuration "recent repositories".
            // Basically, remove existing and prepend again, to make sure they appear on "top".
            foreach (RepositoryViewModel item in notifyCollectionChangedEventArgs.NewItems)
            {
                if (item.RepositoryFullPath == null)
                    continue;

                Config.RecentRepositories.RemoveAll(r => r.RepositoryFullPath == item.RepositoryFullPath);

                Config.RecentRepositories.Add(new RecentRepositoryConfiguration
                {
                    Name = item.Name,
                    RepositoryFullPath = item.RepositoryFullPath
                });
            }

            if (Config.RecentRepositories.Count > 20)
                Config.RecentRepositories.RemoveRange(20, Config.RecentRepositories.Count - 20);

            Config.Save();
        }

        /// <summary>
        /// Loads the initial configuration.
        /// </summary>
        public void Load()
        {
            var tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");

            Config = Configuration.LoadConfiguration();

            // Fill the "RecentRepositories" collection and load them.
            foreach (var recent in Config.RecentRepositories)
            {
                var repo = new RepositoryViewModel { Name = recent.Name, RepositoryFullPath = recent.RepositoryFullPath };

                RecentRepositories.Add(repo);

                // Only open the most recent one.
                /*if (recent.IsOpened)
                {
                    repo.Init();
                    RepositoryViewModels.Add(repo);
                }*/
            }

            // Create the dashboard tab if there are no tabs opened.
            if (tabControl.Items.Count == 1) // The "+" tab is counted as one.
                CreateTab(new object()); // Create "Dashboard".

            // Select the first tab.
            tabControl.SelectedIndex = 0;
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
            var tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            var repositories = (ObservableCollection<RepositoryViewModel>) tabControl.ItemsSource;

            if (action == null)
                repositories.Remove((RepositoryViewModel) tabControl.SelectedContent);
            else
                repositories.Remove((RepositoryViewModel) action);

            // Shut down when all tabs are closed. The "+" tab is counted as a tab.
            if (tabControl.Items.Count == 1)
                Application.Current.Shutdown();

            // Make sure the "+" tab is never selected.
            if (tabControl.SelectedIndex == tabControl.Items.Count - 1)
                tabControl.SelectedIndex -= 1;
        }

        /// <summary>
        /// Creates a new tab.
        /// </summary>
        /// <param name="action"></param>
        private void CreateTab(object action)
        {
            var tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            var mainWindowViewModel = (MainWindowViewModel) Application.Current.MainWindow.DataContext;

            var repository = new RepositoryViewModel
            {
                Name = "Dashboard",
                NotOpened = true,
                RepositoryFullPath = ""
            };

            mainWindowViewModel.RepositoryViewModels.Add(repository);

            tabControl.SelectedItem = repository;
        }

        #endregion
    }
}