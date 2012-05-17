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

        public RangedObservableCollection<Commit>     Commits { get; set; }
        public RangedObservableCollection<StatusItem> StatusItems { get; set; }
        public ObservableCollection<Branch>           Branches { get; set; }
        public ObservableCollection<Tag>              Tags { get; set; }
        public ObservableCollection<Remote>           Remotes { get; set; }
        public ObservableCollection<Submodule>        Submodules { get; set; }
        public ObservableCollection<Stash>            Stashes { get; set; }
        public ListCollectionView                     StatusItemsGrouped { get; set; }

        public int CommitsPerPage { get; set; }

        /// <summary>
        /// The delegate used for reloading the status grid items upon filesystem changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        delegate void ReloadStatusDelegate(object sender, FileSystemEventArgs e);

        /// <summary>
        /// Commands.
        /// </summary>
        #region Commands

        public DelegateCommand StageUnstageCommand { get; private set; } // TODO

        #endregion

        public RepositoryViewModel()
        {
            // Initialize empty collections.
            Commits       = new RangedObservableCollection<Commit> { };
            StatusItems   = new RangedObservableCollection<StatusItem> { };
            Branches      = new ObservableCollection<Branch> { };
            Tags          = new ObservableCollection<Tag> { };
            Remotes       = new ObservableCollection<Remote> { };
            Submodules    = new ObservableCollection<Submodule> { };
            Stashes       = new ObservableCollection<Stash> { };

            CommitsPerPage = 50;

            // Initialize status item view and group.
            StatusItemsGrouped = new ListCollectionView(StatusItems);
            StatusItemsGrouped.GroupDescriptions.Add(new PropertyGroupDescription("GenericStatus"));
            StatusItemsGrouped.SortDescriptions.Add(new SortDescription("GenericStatus", ListSortDirection.Descending));

            // Initialize commands.
            StageUnstageCommand = new DelegateCommand(StageUnstage);
        }

        /// <summary>
        /// Stages or unstages the selected item.
        /// </summary>
        /// <param name="action"></param>
        private void StageUnstage(object action)
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
                Console.WriteLine("Loading and constructing repository data for \"" + FullPath + "\".");

                ConstructRepository();
                LoadRepositoryStatus();

                ListenToDirectoryChanges();
            }
        }

        /// <summary>
        /// Loads and constructs the entire repository.
        /// 
        /// This will limit the amount of changesets to 100 / n.
        /// </summary>
        private void ConstructRepository()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            // Create tags.
            Console.WriteLine("Constructs repository tags for \"" + FullPath + "\".");
            foreach (LibGit2Sharp.Tag tag in repo.Tags)
            {
                Tag t = Tag.Create(repo, tag);

                if (t.HasCommitAsTarget)
                    Tags.Add(t);
            }

            // Create commits.
            Console.WriteLine("Constructs repository commits for \"" + FullPath + "\".");
            List<Commit> commitList = new List<Commit>();
            foreach (LibGit2Sharp.Commit commit in repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Refs }).Take(CommitsPerPage))
            {
                commitList.Add(Commit.Create(repo, commit, Tags));
            }
            Commits.AddRange(commitList);

            // Create branches.
            Console.WriteLine("Constructs repository branches for \"" + FullPath + "\".");
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                Branch b = Branch.Create(repo, branch, Commits, CommitsPerPage);
                Branches.Add(b);
            }

            // Post-process branches (tips and tracking branches).
            Console.WriteLine("Post-processing repository branches for \"" + FullPath + "\".");
            foreach (Branch branch in Branches)
            {
                branch.PostProcess(Branches, Commits);
            }

            // Post-process commits (commit parents).
            Console.WriteLine("Post-processing repository commits for \"" + FullPath + "\".");
            foreach (Commit commit in Commits)
            {
                commit.PostProcess(Commits, Branches);
            }

            repo.Dispose();
        }

        /// <summary>
        /// Loads the repository status (modified, added, removed).
        /// </summary>
        private void LoadRepositoryStatus()
        {
            Console.WriteLine("Loading status data for \"" + FullPath + "\".");

            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(FullPath);

            StatusItems.Clear();

            // Load status items.
            List<StatusItem> itemList = new List<StatusItem>();

            LibGit2Sharp.RepositoryStatus status = repo.Index.RetrieveStatus();
            foreach (LibGit2Sharp.StatusEntry fileStatus in status)
            {
                foreach (LibGit2Sharp.FileStatus value in Enum.GetValues(typeof(LibGit2Sharp.FileStatus)))
                {
                    bool isSet = fileStatus.State.HasFlag(value);

                    if (isSet == false || value.ToString() == "Unaltered" || value.ToString() == "Ignored")
                        continue;

                    string fileFullPath = FullPath + "/" + fileStatus.FilePath;

                    // Only those enum statuses that were set will generate a row in the status grid (and those that are not ignored/unaltered).
                    StatusItem item = new StatusItem
                    {
                        Filename = fileStatus.FilePath,
                        Status = value,
                        Size = FileUtil.GetFormattedFileSize(fileFullPath),
                        IsBinary = FileUtil.IsBinaryFile(fileFullPath) ? "Yes" : "-"
                    };

                    itemList.Add(item);
                }
            }

            StatusItems.AddRange(itemList);

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