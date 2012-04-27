using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using GG.ViewModels;
using LibGit2Sharp;

namespace GG
{
    public class RepositoryViewModel : BaseViewModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool NotOpened { get; set; }

        public ObservableCollection<Commit> Commits { get; set; }
        public ObservableCollection<StatusItem> StatusItems { get; set; }
        public ListCollectionView StatusItemsGrouped { get; set; }

        delegate void ReloadStatusDelegate(object sender, FileSystemEventArgs e);

        public RepositoryViewModel()
        {
            Commits = new ObservableCollection<Commit> { };
            StatusItems = new ObservableCollection<StatusItem> { };
            StatusItemsGrouped = new ListCollectionView(StatusItems);
            StatusItemsGrouped.GroupDescriptions.Add(new PropertyGroupDescription("GenericStatus"));
            StatusItemsGrouped.SortDescriptions.Add(new SortDescription("GenericStatus", ListSortDirection.Descending));
        }

        public void Load()
        {
            if (NotOpened == false)
            {
                LoadChangesets();
                LoadRepositoryStatus();

                ListenToDirectoryChanges();
            }
        }

        /// <summary>
        /// Loads changesets/commits.
        /// </summary>
        private void LoadChangesets()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            // Load commits.
            foreach (LibGit2Sharp.Commit commit in repo.Commits)
            {
                Commits.Add(Commit.Create(repo, commit));
            }

            // Dispose.
            repo.Dispose();
        }

        /// <summary>
        /// Loads the repository status (modified, added, removed).
        /// </summary>
        private void LoadRepositoryStatus()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            StatusItems.Clear();

            // Load status items.
            RepositoryStatus status = repo.Index.RetrieveStatus();
            foreach (LibGit2Sharp.StatusEntry fileStatus in status)
            {
                foreach (FileStatus value in Enum.GetValues(typeof(FileStatus)))
                {
                    bool isSet = fileStatus.State.HasFlag(value);

                    if (isSet == false || value.ToString() == "Unaltered" || value.ToString() == "Ignored")
                        continue;

                    // Only those enum statuses that were set will generate a row in the status grid (and those that are not ignored/unaltered).
                    StatusItem item = new StatusItem();
                    item.Filename = fileStatus.FilePath;
                    item.Status = value;
                    item.Type = "image/png";
                    item.Size = "138 kB";

                    StatusItems.Add(item);
                }
            }

            repo.Dispose();
        }

        /// <summary>
        /// Refresh some data when the repository directory changes.
        /// </summary>
        private void ListenToDirectoryChanges()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            ReloadStatusDelegate reloadStatusDelegate = delegate(object sender, FileSystemEventArgs e)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (Action) delegate()
                    {
                        LoadRepositoryStatus();
                    }
                );
            };

            watcher.Changed += new FileSystemEventHandler(reloadStatusDelegate);
            watcher.Deleted += new FileSystemEventHandler(reloadStatusDelegate);
            watcher.Renamed += new RenamedEventHandler(reloadStatusDelegate);
            watcher.Path = FullPath;
            watcher.EnableRaisingEvents = true;
        }
    }
}
