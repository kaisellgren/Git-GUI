using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using GG.Libraries;
using GG.UserControls;
using GG.ViewModels;

namespace GG
{
    class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<RepositoryViewModel> RecentRepositories { get; set; }
        ObservableCollection<RepositoryViewModel> _repositoryViewModels;
        public ObservableCollection<RepositoryViewModel> RepositoryViewModels
        {
            get
            {
                if (_repositoryViewModels == null)
                {
                    _repositoryViewModels = new ObservableCollection<RepositoryViewModel>();
                    var itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(_repositoryViewModels);
                    itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
                }

                return _repositoryViewModels;
            }
        }

        public MainWindowViewModel()
        {
            RecentRepositories = new ObservableCollection<RepositoryViewModel>() { };

            CreateTabCommand = new DelegateCommand(CreateTab);
            CloseTabCommand = new DelegateCommand(CloseTab);
        }

        /// <summary>
        /// Loads the initial configuration.
        /// </summary>
        public void Load()
        {
            if (File.Exists("./Configuration.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                using (FileStream fileStream = new FileStream("./Configuration.xml", FileMode.Open))
                {
                    Configuration configuration = (Configuration) serializer.Deserialize(fileStream);

                    var alreadyOpenedOneRepo = false;

                    // Fill the "RecentRepositories" collection and load them.
                    foreach (RecentRepositoryConfiguration recent in configuration.RecentRepositories)
                    {
                        RepositoryViewModel repo = new RepositoryViewModel { Name = recent.Name, RepositoryFullPath = recent.RepositoryFullPath };

                        RecentRepositories.Add(repo);

                        // Only open the most recent one.
                        if (alreadyOpenedOneRepo == false)
                        {
                            repo.Init();
                            RepositoryViewModels.Add(repo);
                        }

                        alreadyOpenedOneRepo = true;
                    }
                }
            }

            CreateTab(new object()); // Create "New Tab".

            // Select the first tab.
            TabControl tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
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
            TabControl tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            ObservableCollection<RepositoryViewModel> repositories = tabControl.ItemsSource as ObservableCollection<RepositoryViewModel>;

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
            TabControl tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
            MainWindowViewModel mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            RepositoryViewModel repository = new RepositoryViewModel
            {
                Name = "Dashboard",
                NotOpened = true,
                RepositoryFullPath = ""
            };

            mainWindowViewModel.RepositoryViewModels.Add(repository);

            tabControl.SelectedItem = repository;

            // TODO: Automatically give focus to the ListView's first item.
            //ListView recentRepositoriesList = UIHelper.FindChild<ListView>(tabControl.ItemContainerGenerator.ContainerFromIndex(0), "RecentRepositoriesList");
        }

        #endregion
    }
}