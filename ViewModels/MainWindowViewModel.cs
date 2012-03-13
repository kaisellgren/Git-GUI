using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GG
{
    class MainWindowViewModel
    {
        public ObservableCollection<Repository> Repositories { get; set; }

        public MainWindowViewModel()
        {
            Repositories = new ObservableCollection<Repository>{
                new Repository {Name = "gi1", FullPath = "Z:/www/git1"},
                new Repository {Name = "7th-Degree test", FullPath = "Z:/www/7th-Degree"},
                new Repository {Name = "New tab", FullPath = null, NotOpened = true}
            };
        }
    }
}
