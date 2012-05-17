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
        public string RepositoryFullPath { get; set; }
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
            DeleteFileCommand = new DelegateCommand(DeleteFile);
        }

        /// <summary>
        /// Commands.
        /// </summary>
        #region Commands.

        public DelegateCommand StageUnstageCommand { get; private set; }
        public DelegateCommand DeleteFileCommand { get; private set; }

        /// <summary>
        /// Stages or unstages the selected item.
        /// </summary>
        /// <param name="action"></param>
        private void StageUnstage(object action)
        {
            DataGrid statusGrid = UIHelper.FindChild<DataGrid>(Application.Current.MainWindow, "StatusGridElement");
            StatusItem item = statusGrid.SelectedItem as StatusItem;

            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(RepositoryFullPath);

            if (item.GenericStatus == "Staged")
                repo.Index.Unstage(RepositoryFullPath + "/" + item.Filename);
            else
                repo.Index.Stage(RepositoryFullPath + "/" + item.Filename);
        }

        /// <summary>
        /// Deletes a file or many.
        /// </summary>
        /// <param name="action"></param>
        private void DeleteFile(object action)
        {
            LibGit2Sharp.Repository repo = null;
            DataGrid statusGrid = UIHelper.FindChild<DataGrid>(Application.Current.MainWindow, "StatusGridElement");

            // Loop through all selected status items and remove the files physically (and in some cases also from the repository).
            foreach (StatusItem item in statusGrid.SelectedItems)
            {
                // TODO: --cached ?

                File.Delete(RepositoryFullPath + "/" + item.Filename);

                if (!item.Status.HasFlag(LibGit2Sharp.FileStatus.Untracked))
                {
                    if (!(repo is LibGit2Sharp.Repository))
                        repo = new LibGit2Sharp.Repository(RepositoryFullPath);

                    repo.Index.Stage(RepositoryFullPath + "/" + item.Filename);
                }
            }

            if (repo is LibGit2Sharp.Repository)
                repo.Dispose();
        }

        #endregion

        #region Loading and construction.

        /// <summary>
        /// A method that loads the entire repository.
        /// </summary>
        public void Load()
        {
            if (NotOpened == false)
            {
                Console.WriteLine("Loading and constructing repository data for \"" + RepositoryFullPath + "\".");

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
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(RepositoryFullPath);

            // Create tags.
            Console.WriteLine("Constructs repository tags for \"" + RepositoryFullPath + "\".");
            foreach (LibGit2Sharp.Tag tag in repo.Tags)
            {
                Tag t = Tag.Create(repo, tag);

                if (t.HasCommitAsTarget)
                    Tags.Add(t);
            }

            // Create commits.
            Console.WriteLine("Constructs repository commits for \"" + RepositoryFullPath + "\".");
            List<Commit> commitList = new List<Commit>();
            foreach (LibGit2Sharp.Commit commit in repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Refs }).Take(CommitsPerPage))
            {
                commitList.Add(Commit.Create(repo, commit, Tags));
            }
            Commits.AddRange(commitList);

            // Create branches.
            Console.WriteLine("Constructs repository branches for \"" + RepositoryFullPath + "\".");
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                Branch b = Branch.Create(repo, branch, Commits, CommitsPerPage);
                Branches.Add(b);
            }

            // Post-process branches (tips and tracking branches).
            Console.WriteLine("Post-processing repository branches for \"" + RepositoryFullPath + "\".");
            foreach (Branch branch in Branches)
            {
                branch.PostProcess(Branches, Commits);
            }

            // Post-process commits (commit parents).
            Console.WriteLine("Post-processing repository commits for \"" + RepositoryFullPath + "\".");
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
            Console.WriteLine("Loading status data for \"" + RepositoryFullPath + "\".");

            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(RepositoryFullPath);

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

                    string fileFullPath = RepositoryFullPath + "/" + fileStatus.FilePath;

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

#endregion

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
            watcher.Path = RepositoryFullPath;
            watcher.EnableRaisingEvents = true;
        }
    }
}