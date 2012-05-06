using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
        }

        public void Load()
        {
            // Add some test repositories.
            RepositoryViewModel repo = new RepositoryViewModel { Name = "Git test repository", FullPath = "Z:/www/test-repo" };
            repo.Load();

            RepositoryViewModel repo2 = new RepositoryViewModel { Name = "New tab", FullPath = null, NotOpened = true };
            repo2.Load();

            RepositoryViewModels.Add(repo);
            RepositoryViewModels.Add(repo2);

            // Add some "recent repositories".
            RecentRepositories.Add(repo);
        }
    }
}
