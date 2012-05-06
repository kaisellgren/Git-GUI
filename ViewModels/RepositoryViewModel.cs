using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GG.Libraries;
using GG.ViewModels;
using GG.Models;
using System.Windows.Controls;

namespace GG
{
    public class RepositoryViewModel : BaseViewModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool   NotOpened { get; set; }

        public ObservableCollection<Commit>     Commits { get; set; }
        public ObservableCollection<StatusItem> StatusItems { get; set; }
        public ObservableCollection<Branch>     LocalBranches { get; set; }
        public ObservableCollection<Tag>        Tags { get; set; }
        public ObservableCollection<Remote>     Remotes { get; set; }
        public ObservableCollection<Submodule>  Submodules { get; set; }
        public ObservableCollection<Stash>      Stashes { get; set; }
        public ListCollectionView               StatusItemsGrouped { get; set; }

        /// <summary>
        /// The delegate used for reloading the status grid items upon filesystem changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        delegate void ReloadStatusDelegate(object sender, FileSystemEventArgs e);

        public DelegateCommand StageUnstageCommand { get; private set; } // TODO

        public RepositoryViewModel()
        {
            // Initialize empty collections.
            Commits       = new ObservableCollection<Commit> { };
            StatusItems   = new ObservableCollection<StatusItem> { };
            LocalBranches = new ObservableCollection<Branch> { };
            Tags          = new ObservableCollection<Tag> { };
            Remotes       = new ObservableCollection<Remote> { };
            Submodules    = new ObservableCollection<Submodule> { };
            Stashes       = new ObservableCollection<Stash> { };

            StatusItemsGrouped = new ListCollectionView(StatusItems);
            StatusItemsGrouped.GroupDescriptions.Add(new PropertyGroupDescription("GenericStatus"));
            StatusItemsGrouped.SortDescriptions.Add(new SortDescription("GenericStatus", ListSortDirection.Descending));

            StageUnstageCommand = new DelegateCommand(StageUnstageExecuted); // TODO
        }

        // TODO
        private void StageUnstageExecuted(object action)
        {
            DataGrid statusGrid = UIHelper.FindChild<DataGrid>(Application.Current.MainWindow, "StatusGridElement");
            StatusItem item = statusGrid.SelectedItem as StatusItem;

            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            if (item.GenericStatus == "Staged")
                repo.Index.Unstage(FullPath + "/" + item.Filename);
            else
                repo.Index.Stage(FullPath + "/" + item.Filename);
        }

        public void Load()
        {
            if (NotOpened == false)
            {
                LoadChangesets();
                LoadRepositoryStatus();
                LoadSidebarData();

                ListenToDirectoryChanges();
            }
        }

        /// <summary>
        /// Loads branches, tags, remotes, etc. to be displayed on left/side toolbar.
        /// </summary>
        private void LoadSidebarData()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            // Load local branches.
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                Branch b = new Branch();
                b.Name = branch.Name;
                b.Tip = branch.Tip.Sha;
                b.IsTracking = branch.IsTracking;
                b.IsRemote = branch.IsRemote;

                LocalBranches.Add(b);
            }

            // Load tags.
            foreach (LibGit2Sharp.Tag tag in repo.Tags)
            {
                Tag t = new Tag();
                t.Name = tag.Name;
                t.Target = tag.Target.Sha;

                Tags.Add(t);
            }

            // Load remotes. ONLY IN VERSION.NEXT
            /*LibGit2Sharp.RemoteCollection rc = repo.Remotes;
            
            foreach (LibGit2Sharp.Remote remote in repo.Remotes)
            {
                Remote r = new Remote();
                r.Name = remote.Name;

                Remotes.Add(r);
            }*/

            // Stashes?
            // Submodules?
        }

        /// <summary>
        /// Loads changesets/commits.
        /// </summary>
        private void LoadChangesets()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            // Load commits.
            foreach (LibGit2Sharp.Commit commit in repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Refs }))
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
            LibGit2Sharp.RepositoryStatus status = repo.Index.RetrieveStatus();
            foreach (LibGit2Sharp.StatusEntry fileStatus in status)
            {
                foreach (LibGit2Sharp.FileStatus value in Enum.GetValues(typeof(LibGit2Sharp.FileStatus)))
                {
                    bool isSet = fileStatus.State.HasFlag(value);

                    if (isSet == false || value.ToString() == "Unaltered" || value.ToString() == "Ignored")
                        continue;

                    String fileFullPath = FullPath + "/" + fileStatus.FilePath;

                    // Only those enum statuses that were set will generate a row in the status grid (and those that are not ignored/unaltered).
                    StatusItem item = new StatusItem();
                    item.Filename = fileStatus.FilePath;
                    item.Status = value;
                    item.Size = FileUtil.GetFormattedFileSize(fileFullPath);
                    item.IsBinary = FileUtil.IsBinaryFile(fileFullPath) ? "Yes" : "-";

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
            watcher.Created += new FileSystemEventHandler(reloadStatusDelegate);
            watcher.Path = FullPath;
            watcher.EnableRaisingEvents = true;
        }
    }
}