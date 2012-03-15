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
                new Repository {
                    Name = "7th-Degree test",
                    FullPath = "Z:/www/7th-Degree",
                    Commits = new CommitCollection{
                        new Commit{ AuthorEmail = "example@email.com", Date = DateTime.Now, Description = "Bar", AuthorName = "Test", Hash = "asd", Source = "asd"},
                        new Commit{ AuthorEmail = "example@email.com", Date = new DateTime(2012, 12, 12), Description = "Bar", AuthorName = "Test", Hash = "asd", Source = "asd"},
                        new Commit{ AuthorEmail = "example@email.com", Date = new DateTime(2012, 12, 12), Description = "Bar", AuthorName = "Test", Hash = "asd", Source = "asd"}
                    }
                },
                new Repository {Name = "gi1", FullPath = "Z:/www/git1"},
                new Repository {Name = "New tab", FullPath = null, NotOpened = true}
            };
        }
    }
}
