using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using GG.UserControls;
using Microsoft.Win32;
using GG.Libraries;
using GG.ViewModels;
using GG.Models;
using GG.UserControls.Dialogs;
using System.Collections;
using System.Threading.Tasks;

namespace GG
{
    public class RepositoryViewModel : BaseViewModel
    {
        #region Properties and fields.

        public string Name { get; set; }
        public string RepositoryFullPath { get; set; }
        public bool NotOpened { get; set; }

        public EnhancedObservableCollection<Commit> Commits { get; set; }
        public EnhancedObservableCollection<StatusItem> StatusItemsStaged { get; set; }
        public EnhancedObservableCollection<StatusItem> StatusItemsUnstaged { get; set; }
        public EnhancedObservableCollection<Branch> Branches { get; set; }
        public EnhancedObservableCollection<Tag> Tags { get; set; }
        public EnhancedObservableCollection<Remote> Remotes { get; set; }
        public EnhancedObservableCollection<Submodule> Submodules { get; set; }
        public EnhancedObservableCollection<Stash> Stashes { get; set; }
        public EnhancedObservableCollection<RecentCommitMessage> RecentCommitMessages { get; private set; }

        private Branch head;

        /// <summary>
        /// The HEAD. This can either be a reference to a DetachedHead or a Branch.
        /// </summary>
        public Branch Head
        {
            get { return head; }

            set
            {
                head = value;
                RaisePropertyChanged("Head");
            }
        }

        public int CommitsPerPage { get; set; }
        public int RecentCommitMessageCount { get; set; }

        /// <summary>
        /// Stores the diff text for the diff panel.
        /// </summary>
        private string statusItemDiff;

        public string StatusItemDiff
        {
            get { return statusItemDiff; }
            set
            {
                statusItemDiff = value;
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
            Commits = new EnhancedObservableCollection<Commit> { };
            StatusItemsStaged = new EnhancedObservableCollection<StatusItem> { };
            StatusItemsUnstaged = new EnhancedObservableCollection<StatusItem> { };
            Branches = new EnhancedObservableCollection<Branch> { };
            Tags = new EnhancedObservableCollection<Tag> { };
            Remotes = new EnhancedObservableCollection<Remote> { };
            Submodules = new EnhancedObservableCollection<Submodule> { };
            Stashes = new EnhancedObservableCollection<Stash> { };
            RecentCommitMessages = new EnhancedObservableCollection<RecentCommitMessage>();

            CommitsPerPage = 150;
            RecentCommitMessageCount = 10;

            // Initialize commands.
            AddNoteCommand = new DelegateCommand(AddNote);
            CheckoutCommand = new DelegateCommand(Checkout);
            CreateBranchCommand = new DelegateCommand(CreateBranch);
            CreateTagCommand = new DelegateCommand(CreateTag);
            CommitCommand = new DelegateCommand(CommitChanges, CommitChanges_CanExecute);
            CopyHashCommand = new DelegateCommand(CopyHash);
            CopyPatchCommand = new DelegateCommand(CopyPatch);
            DeleteFileCommand = new DelegateCommand(DeleteFile);
            DeleteTagCommand = new DelegateCommand(DeleteTag);
            ExportPatchCommand = new DelegateCommand(ExportPatch);
            OpenAboutCommand = new DelegateCommand(OpenAbout);
            ResetSoftCommand = new DelegateCommand(ResetSoft);
            ResetMixedCommand = new DelegateCommand(ResetMixed);
            StageUnstageCommand = new DelegateCommand(StageUnstage);

            // Diff panel.
            StatusItemDiff = "";
        }

        #region Commands.

        public DelegateCommand AddNoteCommand { get; private set; }
        public DelegateCommand CheckoutCommand { get; private set; }
        public DelegateCommand CreateBranchCommand { get; private set; }
        public DelegateCommand CreateTagCommand { get; private set; }
        public DelegateCommand CommitCommand { get; private set; }
        public DelegateCommand CopyHashCommand { get; private set; }
        public DelegateCommand CopyPatchCommand    { get; private set; }
        public DelegateCommand DeleteFileCommand { get; private set; }
        public DelegateCommand DeleteTagCommand { get; private set; }
        public DelegateCommand ExportPatchCommand { get; private set; }
        public DelegateCommand OpenAboutCommand    { get; private set; }
        public DelegateCommand ResetSoftCommand { get; private set; }
        public DelegateCommand ResetMixedCommand { get; private set; }
        public DelegateCommand StageUnstageCommand { get; private set; }

        /// <summary>
        /// Exports the given changeset as a patch to a file.
        /// </summary>
        /// <param name="action"></param>
        public void ExportPatch(object action)
        {
            var commit = action as Commit;

            if (commit == null)
                return;

            var dialog = new SaveFileDialog
            {
                FileName = commit.Description.Right(72),
                DefaultExt = ".patch",
                Filter = "Patch files|*.patch"
            };

            if (dialog.ShowDialog() == false)
                return;

            var filename = dialog.FileName;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    File.WriteAllText(filename, RepoUtil.GetTreeChangesForCommit(repo, commit).Patch);
                }
            });
        }

        /// <summary>
        /// Copies the given changeset as a patch to clipboard.
        /// </summary>
        /// <param name="action"></param>
        public void CopyPatch(object action)
        {
            var commit = action as Commit;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    Clipboard.SetText(RepoUtil.GetTreeChangesForCommit(repo, commit).Patch);
                }
            });
        }

        /// <summary>
        /// Checkouts a commit/branch/tag.
        /// </summary>
        /// <param name="action"></param>
        public void Checkout(object action)
        {
            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    if (action is Tag)
                        repo.Checkout(((Tag) action).CanonicalName);
                }
            });
        }

        /// <summary>
        /// Adds a new note for the given commit.
        /// </summary>
        /// <param name="action"></param>
        public void AddNote(object action)
        {
            var dialog = new PromptDialog
            {
                Title = "Add a note",
                Message = "Enter the note to add for the commit:"
            };

            dialog.ShowDialog();

            if (dialog.DialogResult != true)
                return;

            var commit = action as Commit;
            var response = dialog.ResponseText;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    //repo.Notes.Create(commit.Hash, dialog.ResponseText); // TODO: Complete?
                    LoadEntireRepository();
                }
            });
        }

        /// <summary>
        /// Copies the hash of a commit.
        /// </summary>
        /// <param name="action"></param>
        public void CopyHash(object action)
        {
            var commit = action as Commit;

            if (commit == null)
                return;

            Clipboard.SetText(commit.Hash);
        }

        /// <summary>
        /// Creates a tag.
        /// </summary>
        /// <param name="action"></param>
        public void CreateTag(object action)
        {
            var commit = action as Commit;

            var dialog = new PromptDialog
            {
                Title = "Create a new tag",
                Message = "Enter the name for the tag:"
            };

            dialog.ShowDialog();

            if (dialog.DialogResult != true || commit == null)
                return;

            var response = dialog.ResponseText;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    repo.Tags.Create(response, commit.Hash);
                    LoadEntireRepository();
                }
            });
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        /// <param name="action"></param>
        public void DeleteTag(object action)
        {
            var tag = (Tag) action;

            var dialog = new ConfirmDialog
            {
                Title = "Deleting a tag",
                Message = String.Format("Are you sure you want to delete this tag ({0})?", tag.Name)
            };

            dialog.ShowDialog();

            var pressedButton = dialog.PressedButton;
            if (pressedButton == null || ((string) pressedButton.Content) != "OK")
                return;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    // Remove the tag from the Git repository.
                    repo.Tags.Delete(tag.CanonicalName);

                    // Reload all tags.
                    LoadTags();

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action) (() => tag.Target.Tags.Remove(tag))
                    );
                }
            });
        }

        /// <summary>
        /// Resets (reset --mixed) the repository to the given changeset.
        /// </summary>
        /// <param name="action"></param>
        public void ResetMixed(object action)
        {
            var commit = action as Commit;

            if (commit == null)
                return;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    repo.Reset(LibGit2Sharp.ResetOptions.Soft, commit.Hash);
                    LoadEntireRepository();
                }
            });
        }

        /// <summary>
        /// Resets (reset --soft) the repository to the given changeset.
        /// </summary>
        /// <param name="action"></param>
        public void ResetSoft(object action)
        {
            var commit = action as Commit;

            if (commit == null)
                return;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    repo.Reset(LibGit2Sharp.ResetOptions.Mixed, commit.Hash);
                    LoadEntireRepository();
                }
            });
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

            if (dialog.DialogResult != true)
                return;

            var response = dialog.ResponseText;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    repo.Branches.Create(response, repo.Head.Tip.Sha.ToString(CultureInfo.InvariantCulture));
                }

                LoadEntireRepository();
            });
        }

        /// <summary>
        /// Stages or unstages the selected item.
        /// </summary>
        /// <param name="action"></param>
        private void StageUnstage(object action)
        {
            var collection = (IList) action;
            var items = collection.Cast<StatusItem>();

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    foreach (var item in items)
                    {
                        if (item.GenericStatus == "Staged")
                            repo.Index.Unstage(RepositoryFullPath + "/" + item.Filename);
                        else
                            repo.Index.Stage(RepositoryFullPath + "/" + item.Filename);
                    }
                }
            });
        }

        /// <summary>
        /// Commits the currently staged files.
        /// </summary>
        /// <param name="action"></param>
        private void CommitChanges(object action)
        {
            var commitMessage = ((TextBox) action).Text;

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    LibGit2Sharp.RepositoryExtensions.Commit(repo, commitMessage, false);

                    // Reconstruct the repository.
                    LoadEntireRepository();

                    // Clear the commit message box.
                    Application.Current.Dispatcher.BeginInvoke(
                        (Action)(() => UIHelper.FindChild<TextBox>(Application.Current.MainWindow, "CommitMessageTextBox").Clear())
                    );
                }
            });
        }

        /// <summary>
        /// Checks if there is anything to commit
        /// </summary>
        /// <param name="action"></param>
        private bool CommitChanges_CanExecute(object action)
        {
            var textBox = ((TextBox) action);

            if (textBox == null)
                return false;

            var commitMessage = textBox.Text;

            // Only allow commit if there's something to commit and there's a commit message.
            if (commitMessage != null && StatusItemsStaged.Count > 0)
                return commitMessage.Length > 0;

            return false;
        }

        /// <summary>
        /// Deletes a file or many.
        /// </summary>
        /// <param name="action"></param>
        private void DeleteFile(object action)
        {
            var dialog = new ConfirmDialog
            {
                Title = "Delete file(s)",
                Message = "Are you sure you want to delete the selected file(s)?\r\n\r\nYou may lose your changes!",
                ButtonSet = ConfirmDialog.ButtonsSet.OK_CANCEL
            };

            dialog.ShowDialog();

            var pressedButton = (Button) dialog.PressedButton;
            if (pressedButton != null && ((string) pressedButton.Content) == "OK")
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
                        if (repo == null)
                            repo = new LibGit2Sharp.Repository(RepositoryFullPath);

                        repo.Index.Unstage(RepositoryFullPath + "/" + item.Filename);
                    }
                }

                if (repo != null)
                    repo.Dispose();
            }
        }

        /// <summary>
        /// Opens the about dialog.
        /// </summary>
        /// <param name="action"></param>
        private void OpenAbout(object action)
        {
            Window about = new About();

            // Apply a blur effect to main window.
            var blur = new BlurEffect
            {
                Radius = 4
            };

            Application.Current.MainWindow.Effect = blur;

            about.ShowDialog();

            Application.Current.MainWindow.Effect = null;
        }

        #endregion

        #region Loading and construction.

        /// <summary>
        /// A method that loads the entire repository and initializes everything necessary.
        /// </summary>
        public bool Init()
        {
            // The "New Tab" page should not load data, i.e., the repository is not yet opened.
            if (NotOpened == false)
            {
                try
                {
                    LoadEntireRepository();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Loads and constructs the entire repository.
        /// 
        /// This will limit the amount of changesets to CommitsPerPage.
        /// </summary>
        public void LoadEntireRepository()
        {
            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    LoadTags(repo);
                    LoadBranchesAndCommits(repo);
                    LoadRepositoryStatus(repo);
                    LoadRecentCommitMessages();

                    ListenToDirectoryChanges();
                }
            });
        }

        /// <summary>
        /// Loads branches and commits.
        /// </summary>
        /// <param name="repo"></param>
        private void LoadBranchesAndCommits(LibGit2Sharp.Repository repo = null)
        {
            var dispose = false;
            if (repo == null)
            {
                repo = new LibGit2Sharp.Repository(RepositoryFullPath);
                dispose = true;
            }

            // Small performance boosts.
            Commits.DisableNotifications();
            Branches.DisableNotifications();

            // Create commits.
            Commits.Clear();
            var commitList = new List<Commit>();
            foreach (var commit in repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Branches }).Take(CommitsPerPage))
            {
                commitList.Add(Commit.Create(repo, commit, Tags));
            }
            Commits.AddRange(commitList);

            // Create branches.
            Branches.Clear();
            foreach (var branch in repo.Branches)
            {
                var b = Branch.Create(this, repo, branch);
                Branches.Add(b);
            }

            // Post-process branches (tips and tracking branches).
            foreach (var branch in Branches)
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
            foreach (var commit in Commits)
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
            foreach (var branch in Branches)
            {
                RepoUtil.IncrementCommitTreeVisualPositionsRecursively(branch.Tip);
            }

            // Fire notifications for the collections on the UI thread.
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action) (() =>
                {
                    Commits.EnableNotifications(true);
                    Branches.EnableNotifications(true);

                    var tabControl = UIHelper.FindChild<TabControl>(Application.Current.MainWindow, "RepositoryTabs");
                    var changesetHistory = UIHelper.FindChild<ChangesetHistory>(tabControl);

                    if (changesetHistory != null)
                        changesetHistory.RedrawGraph();
                })
            );

            if (dispose)
                repo.Dispose();
        }
        
        /// <summary>
        /// Loads the tags.
        /// </summary>
        private void LoadTags(LibGit2Sharp.Repository repo = null)
        {
            var dispose = false;
            if (repo == null)
            {
                repo = new LibGit2Sharp.Repository(RepositoryFullPath);
                dispose = true;
            }

            // Small performance boost.
            Tags.DisableNotifications();

            Tags.Clear();

            // Add new tags.
            foreach (var tag in repo.Tags)
            {
                var t = Tag.Create(repo, tag);

                if (t.HasCommitAsTarget)
                    Tags.Add(t);
            }

            // Fire notifications for the Tags collection on the UI thread.
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action) (() => Tags.EnableNotifications(true))
            );

            if (dispose)
                repo.Dispose();
        }

        /// <summary>
        /// Loads the repository status (modified, added, removed).
        /// </summary>
        private void LoadRepositoryStatus(LibGit2Sharp.Repository repo = null)
        {
            var dispose = false;
            if (repo == null)
            {
                repo = new LibGit2Sharp.Repository(RepositoryFullPath);
                dispose = true;
            }

            // A small performance boost.
            StatusItemsStaged.DisableNotifications();
            StatusItemsUnstaged.DisableNotifications();

            StatusItemsStaged.Clear();
            StatusItemsUnstaged.Clear();

            // Load status items.
            var itemList = new List<StatusItem>();

            var status = repo.Index.RetrieveStatus();
            foreach (var fileStatus in status)
            {
                foreach (LibGit2Sharp.FileStatus value in Enum.GetValues(typeof(LibGit2Sharp.FileStatus)))
                {
                    var isSet = fileStatus.State.HasFlag(value);

                    if (isSet == false || value.ToString() == "Unaltered" || value.ToString() == "Ignored")
                        continue;

                    // TODO: would it be better without full repo path?
                    var fileFullPath = RepositoryFullPath + "/" + fileStatus.FilePath;

                    // Only those enum statuses that were set will generate a row in the status grid (and those that are not ignored/unaltered).
                    var item = new StatusItem
                    {
                        Filename = fileStatus.FilePath,
                        Status = value,
                        Size = FileUtil.GetFormattedFileSize(fileFullPath), // TODO: Should these two file IO be done lazily?
                        IsBinary = FileUtil.IsBinaryFile(fileFullPath) ? "Yes" : "-"
                    };

                    itemList.Add(item);
                }
            }

            StatusItemsStaged.AddRange(itemList.Where(s => s.IsStaged));
            StatusItemsUnstaged.AddRange(itemList.Where(s => !s.IsStaged));

            if (dispose)
                repo.Dispose();

            // Fire notifications for the collection on the UI thread.
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(
                    () =>
                    {
                        StatusItemsStaged.EnableNotifications(true);
                        StatusItemsUnstaged.EnableNotifications(true);
                    }
                )
            );
        }

        /// <summary>
        /// Stores the last RecentCommitMessageCount commit messages.
        /// </summary>
        private void LoadRecentCommitMessages()
        {
            // A small performance boost.
            RecentCommitMessages.DisableNotifications();

            RecentCommitMessages.Clear();

            foreach (var commit in Commits.Take(RecentCommitMessageCount))
                RecentCommitMessages.Add(new RecentCommitMessage(commit.ShortDescription));

            // Fire notifications for the collection on the UI thread.
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action) (() => RecentCommitMessages.EnableNotifications(true))
            );
        }

#endregion

        /// <summary>
        /// Updates the diff panel text.
        /// </summary>
        /// <param name="collection"> </param>
        public void UpdateStatusItemDiff(IList collection)
        {
            if (NotOpened)
                return;

            var diff = "";
            var items = collection.Cast<StatusItem>();

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryFullPath))
                {
                    foreach (var item in items)
                    {

                    }

                    diff += repo.Diff.Compare(repo.Head.Tip.Tree, LibGit2Sharp.DiffTarget.Index).Patch;
                }

                StatusItemDiff = diff;
            });
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
            var watcher = new FileSystemWatcher();

            ReloadStatusDelegate reloadStatusDelegate = delegate(object sender, FileSystemEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    (Action) (() => LoadRepositoryStatus())
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