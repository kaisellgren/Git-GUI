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
using System.Windows.Controls;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using GG.Libraries;
using GG.ViewModels;
using GG.Models;
using GG.UserControls.Dialogs;
using System.Collections;
using System.Text.RegularExpressions;

namespace GG
{
    public class RepositoryViewModel : BaseViewModel
    {
        #region Properties and fields.

        public string Name { get; set; }
        public string RepositoryFullPath { get; set; }
        public bool NotOpened { get; set; }
        private bool alreadyLoaded = false;

        public RangedObservableCollection<Commit>        Commits { get; set; }
        public RangedObservableCollection<StatusItem>    StatusItems { get; set; }
        public ObservableCollection<Branch>              Branches { get; set; }
        public ObservableCollection<Tag>                 Tags { get; set; }
        public ObservableCollection<Remote>              Remotes { get; set; }
        public ObservableCollection<Submodule>           Submodules { get; set; }
        public ObservableCollection<Stash>               Stashes { get; set; }
        public ObservableCollection<RecentCommitMessage> RecentCommitMessages { get; private set; }
        public ListCollectionView                        StatusItemsGrouped { get; set; }

        /// <summary>
        /// The HEAD. This can either be a reference to a DetachedHead or a Branch.
        /// </summary>
        public Branch Head { get; set; }

        public int CommitsPerPage { get; set; }
        public int RecentCommitMessageCount { get; set; }

        /// <summary>
        /// Stores the diff text for the diff panel.
        /// </summary>
        private string _StatusItemDiff;

        public string StatusItemDiff
        {
            get { return _StatusItemDiff; }
            set
            {
                _StatusItemDiff = value;
                //var regex = new Regex("+.*");
                //regex.Replace(value, 
                RaisePropertyChanged("StatusItemDiff");
            }
        }

        /// <summary>
        /// The delegate used for reloading the status grid items upon filesystem changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        delegate void ReloadStatusDelegate(object sender, FileSystemEventArgs e);

        #endregion

        public RepositoryViewModel()
        {
            // Initialize empty collections.
            Commits = new RangedObservableCollection<Commit> { };
            StatusItems = new RangedObservableCollection<StatusItem> { };
            Branches = new ObservableCollection<Branch> { };
            Tags = new ObservableCollection<Tag> { };
            Remotes = new ObservableCollection<Remote> { };
            Submodules = new ObservableCollection<Submodule> { };
            Stashes = new ObservableCollection<Stash> { };
            RecentCommitMessages = new ObservableCollection<RecentCommitMessage>();

            CommitsPerPage = 50;
            RecentCommitMessageCount = 10;

            // Initialize status item view and group.
            StatusItemsGrouped = new ListCollectionView(StatusItems);
            StatusItemsGrouped.GroupDescriptions.Add(new PropertyGroupDescription("GenericStatus"));
            StatusItemsGrouped.SortDescriptions.Add(new SortDescription("GenericStatus", ListSortDirection.Descending));

            // Initialize commands.
            ExportPatchCommand = new DelegateCommand(ExportPatch);
            CopyPatchCommand = new DelegateCommand(CopyPatch);
            AddNoteCommand = new DelegateCommand(AddNote);
            CopyHashCommand = new DelegateCommand(CopyHash);
            TagCommand = new DelegateCommand(CreateTag);
            CreateBranchCommand = new DelegateCommand(CreateBranch);
            ResetSoftCommand = new DelegateCommand(ResetSoft);
            ResetMixedCommand = new DelegateCommand(ResetMixed);
            OpenAboutCommand = new DelegateCommand(OpenAbout);
            StageUnstageCommand = new DelegateCommand(StageUnstage);
            DeleteFileCommand = new DelegateCommand(DeleteFile);
            CommitCommand = new DelegateCommand(CommitChanges, CommitChanges_CanExecute);

            // Diff panel.
            StatusItemDiff = "";
        }

        /// <summary>
        /// Commands.
        /// </summary>
        #region Commands.

        public DelegateCommand ExportPatchCommand  { get; private set; }
        public DelegateCommand CopyPatchCommand    { get; private set; }
        public DelegateCommand AddNoteCommand      { get; private set; }
        public DelegateCommand CopyHashCommand     { get; private set; }
        public DelegateCommand TagCommand          { get; private set; }
        public DelegateCommand CreateBranchCommand { get; private set; }
        public DelegateCommand ResetSoftCommand    { get; private set; }
        public DelegateCommand ResetMixedCommand   { get; private set; }
        public DelegateCommand OpenAboutCommand    { get; private set; }
        public DelegateCommand StageUnstageCommand { get; private set; }
        public DelegateCommand DeleteFileCommand   { get; private set; }
        public DelegateCommand CommitCommand       { get; private set; }

        /// <summary>
        /// Exports the given changeset as a patch to a file.
        /// </summary>
        /// <param name="action"></param>
        public void ExportPatch(object action)
        {
            Commit commit = action as Commit;

            using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = commit.Description.Right(72);
                dialog.DefaultExt = ".patch";
                dialog.Filter = "Patch files|*.patch";

                if (dialog.ShowDialog() == true)
                {
                    // Save the patch to a file.
                    File.WriteAllText(dialog.FileName, RepoUtil.GetTreeChangesForCommit(repo, commit).Patch);
                }
            }
        }

        /// <summary>
        /// Copies the given changeset as a patch to clipboard.
        /// </summary>
        /// <param name="action"></param>
        public void CopyPatch(object action)
        {
            Commit commit = action as Commit;

            using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
            {
                Clipboard.SetText(RepoUtil.GetTreeChangesForCommit(repo, commit).Patch);
            }
        }

        /// <summary>
        /// Adds a new note for the given commit.
        /// </summary>
        /// <param name="action"></param>
        public void AddNote(object action)
        {
            var dialog = new PromptDialog();
            dialog.Title = "Add a note";
            dialog.Message = "Enter the note to add for the commit:";

            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                Commit commit = action as Commit;

                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    //repo.Notes.Create(commit.Hash, dialog.ResponseText);
                    ConstructRepository();
                }
            }
        }

        /// <summary>
        /// Copies the hash of a commit.
        /// </summary>
        /// <param name="action"></param>
        public void CopyHash(object action)
        {
            Commit commit = action as Commit;

            Clipboard.SetText(commit.Hash);
        }

        /// <summary>
        /// Creates a tag.
        /// </summary>
        /// <param name="action"></param>
        public void CreateTag(object action)
        {
            var dialog = new PromptDialog();
            dialog.Title = "Create a new tag";
            dialog.Message = "Enter the name for the tag:";

            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                Commit commit = action as Commit;

                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    repo.Tags.Create(dialog.ResponseText, commit.Hash);
                    ConstructRepository();
                }
            }
        }

        /// <summary>
        /// Resets (reset --mixed) the repository to the given changeset.
        /// </summary>
        /// <param name="action"></param>
        public void ResetMixed(object action)
        {
            Commit commit = action as Commit;

            using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
            {
                repo.Reset(LibGit2Sharp.ResetOptions.Soft, commit.Hash);
                ConstructRepository();
            }
        }

        /// <summary>
        /// Resets (reset --soft) the repository to the given changeset.
        /// </summary>
        /// <param name="action"></param>
        public void ResetSoft(object action)
        {
            Commit commit = action as Commit;

            using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
            {
                repo.Reset(LibGit2Sharp.ResetOptions.Mixed, commit.Hash);
                ConstructRepository();
            }
        }

        /// <summary>
        /// Creates a branch.
        /// </summary>
        /// <param name="action"></param>
        public void CreateBranch(object action)
        {
            var dialog = new PromptDialog
            {
                Title = "Creating a new branch",
                Message = "Please give a name for your new branch:"
            };

            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    var sha = repo.Head.Tip.Sha.ToString();
                    repo.Branches.Create(dialog.ResponseText, repo.Head.Tip.Sha.ToString());
                }

                ConstructRepository();
            }
        }

        /// <summary>
        /// Stages or unstages the selected item.
        /// </summary>
        /// <param name="action"></param>
        private void StageUnstage(object action)
        {
            var collection = (IList) action;
            var items = collection.Cast<StatusItem>();

            var repo = new LibGit2Sharp.Repository(RepositoryFullPath);

            foreach (StatusItem item in items)
            {
                if (item.GenericStatus == "Staged")
                    repo.Index.Unstage(RepositoryFullPath + "/" + item.Filename);
                else
                    repo.Index.Stage(RepositoryFullPath + "/" + item.Filename);
            }

            repo.Dispose();
        }

        /// <summary>
        /// Commits the currently staged files.
        /// </summary>
        /// <param name="action"></param>
        private void CommitChanges(object action)
        {
            //need to pass author, commitor, and the commit message
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(RepositoryFullPath);
            LibGit2Sharp.RepositoryExtensions.Commit(repo, action.ToString(), false);
            ConstructRepository();
        }

        /// <summary>
        /// Checks if there is anything to commit
        /// </summary>
        /// <param name="action"></param>
        private bool CommitChanges_CanExecute(object action)
        {
            if (action != null)
            {
                return action.ToString().Length > 0;
            }
            else
            {
                return false;
            }         
        }

        /// <summary>
        /// Deletes a file or many.
        /// </summary>
        /// <param name="action"></param>
        private void DeleteFile(object action)
        {
            var collection = (IList) action;
            var items = collection.Cast<StatusItem>();
            LibGit2Sharp.Repository repo = null;

            // Loop through all selected status items and remove the files physically (and in some cases also from the repository).
            foreach (StatusItem item in items)
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

        /// <summary>
        /// Opens the about dialog.
        /// </summary>
        /// <param name="action"></param>
        private void OpenAbout(object action)
        {
            Window about = new About();

            // Apply a blur effect to main window.
            BlurEffect blur = new BlurEffect();
            blur.Radius = 4;
            Application.Current.MainWindow.Effect = blur;

            about.ShowDialog();

            Application.Current.MainWindow.Effect = null;
        }

        #endregion

        #region Loading and construction.

        /// <summary>
        /// A method that loads the entire repository.
        /// </summary>
        public bool Load()
        {
            if (alreadyLoaded == true)
                throw new Exception("You may not load the repository more than once.");

            // The "New Tab" page should not load data, i.e., the repository is not yet opened.
            if (NotOpened == false)
            {
                Console.WriteLine("Loading and constructing repository data for \"" + RepositoryFullPath + "\".");

                var result = ConstructRepository();

                if (result == true)
                {
                    LoadRepositoryStatus();
                    LoadRecentCommitMessages();
                    ListenToDirectoryChanges();
                }

                return result;
            }

            alreadyLoaded = true;

            return false;
        }

        /// <summary>
        /// Loads and constructs the entire repository.
        /// 
        /// This will limit the amount of changesets to 100 / n.
        /// </summary>
        public bool ConstructRepository()
        {
            bool result;
            LibGit2Sharp.Repository repo = null;

            try
            {
                repo = new LibGit2Sharp.Repository(RepositoryFullPath);

                // Create tags.
                Console.WriteLine("Constructs repository tags for \"" + RepositoryFullPath + "\".");
                Tags.Clear();
                foreach (LibGit2Sharp.Tag tag in repo.Tags)
                {
                    Tag t = Tag.Create(repo, tag);

                    if (t.HasCommitAsTarget)
                        Tags.Add(t);
                }

                // Create commits.
                Console.WriteLine("Constructs repository commits for \"" + RepositoryFullPath + "\".");
                Commits.Clear();
                List<Commit> commitList = new List<Commit>();
                foreach (LibGit2Sharp.Commit commit in repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Branches }).Take(CommitsPerPage))
                {
                    commitList.Add(Commit.Create(repo, commit, Tags));
                }
                Commits.AddRange(commitList);

                // Create branches.
                Console.WriteLine("Constructs repository branches for \"" + RepositoryFullPath + "\".");
                Branches.Clear();
                foreach (LibGit2Sharp.Branch branch in repo.Branches)
                {
                    Branch b = Branch.Create(this, repo, branch);
                    Branches.Add(b);
                }

                // Post-process branches (tips and tracking branches).
                Console.WriteLine("Post-processing repository branches for \"" + RepositoryFullPath + "\".");
                foreach (Branch branch in Branches)
                {
                    // Set the HEAD property if it matches.
                    if (repo.Head.Name == branch.Name)
                    {
                        Head = branch;
                        branch.Tip.IsHead = true;
                    }

                    branch.PostProcess(Branches, Commits);
                }

                // Post-process commits (commit parents).
                Console.WriteLine("Post-processing repository commits for \"" + RepositoryFullPath + "\".");
                foreach (Commit commit in Commits)
                {
                    // Set the HEAD property to a DetachedHead branch if the HEAD matched and it was null.
                    if (Head == null && repo.Head.Tip.Sha == commit.Hash)
                    {
                        Head = new DetachedHead
                        {
                            Tip = commit
                        };

                        commit.IsHead = true;
                    }

                    commit.PostProcess(Commits, Branches);
                }

                // Calculate commit visual positions for each branch tree.
                foreach (Branch branch in Branches)
                {
                    RepoUtil.IncrementCommitTreeVisualPositionsRecursively(branch.Tip);
                }

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            if (repo is LibGit2Sharp.Repository)
                repo.Dispose();

            return result;
        }

        /// <summary>
        /// Loads the repository status (modified, added, removed).
        /// </summary>
        private void LoadRepositoryStatus()
        {
            Console.WriteLine("Loading status data for \"" + RepositoryFullPath + "\".");

            var repo = new LibGit2Sharp.Repository(RepositoryFullPath);

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

        /// <summary>
        /// Stores the last RecentCommitMessageCount commit messages.
        /// </summary>
        private void LoadRecentCommitMessages()
        {
            RecentCommitMessages.Clear();

            foreach (Commit commit in Commits.Take(RecentCommitMessageCount))
            {
                RecentCommitMessages.Add(new RecentCommitMessage(commit.ShortDescription));
            }
        }

#endregion

        /// <summary>
        /// Updates the diff panel text.
        /// </summary>
        /// <param name="items"></param>
        public void UpdateStatusItemDiff(IList collection)
        {
            if (NotOpened == true)
                return;

            var diff = "";
            var items = collection.Cast<StatusItem>();

            using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
            {
                foreach (StatusItem item in items)
                {
                    
                }

                diff += repo.Diff.Compare(repo.Head.Tip.Tree, LibGit2Sharp.DiffTarget.Index).Patch;
            }

            StatusItemDiff = diff;
        }

        /// <summary>
        /// Sets this repository as the active tab on the tab control.
        /// </summary>
        public void SetThisRepositoryAsTheActiveTab()
        {
            UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs").SelectedItem = this;
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
            watcher.Path = RepositoryFullPath;
            watcher.EnableRaisingEvents = true;
        }
    }
}