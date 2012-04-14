using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        public RepositoryViewModel()
        {
            Commits = new ObservableCollection<Commit> { };
            StatusItems = new ObservableCollection<StatusItem> { };
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
                IEnumerable<Branch> branches = ListBranchesContaininingCommit(repo, commit.Sha);

                Commit c = new Commit();
                c.AuthorEmail = commit.Author.Email;
                c.AuthorName = commit.Author.Name;
                c.Date = commit.Author.When.DateTime;
                c.Description = commit.MessageShort;
                c.Hash = commit.Sha;
                c.Source = branches.ElementAt(0).ToString();

                Commits.Add(c);
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

            // Load status items.
            RepositoryStatus status = repo.Index.RetrieveStatus();
            foreach (LibGit2Sharp.StatusEntry fileStatus in status)
            {
                StatusItem item = new StatusItem();
                item.Filename = fileStatus.FilePath;
                item.Status = fileStatus.State.ToString();
                item.Type = "image/png";
                item.Size = "138 kB";

                StatusItems.Add(item);
            }

            repo.Dispose();
        }

        /// <summary>
        /// Refresh some data when the repository directory changes.
        /// </summary>
        private void ListenToDirectoryChanges()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => {
                LoadRepositoryStatus();
            });
            watcher.Path = FullPath;
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// A helper method for listing branches that contain the given commit.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commitSha"></param>
        /// <returns></returns>
        private IEnumerable<Branch> ListBranchesContaininingCommit(LibGit2Sharp.Repository repo, string commitSha)
        {
            bool directBranchHasBeenFound = false;
            foreach (var branch in repo.Branches)
            {
                if (branch.Tip.Sha != commitSha)
                {
                    continue;
                }

                directBranchHasBeenFound = true;
                yield return branch;
            }

            if (directBranchHasBeenFound)
            {
                yield break;
            }

            foreach (var branch in repo.Branches)
            {
                var commits = repo.Commits.QueryBy(new Filter { Since = branch }).Where(c => c.Sha == commitSha);

                if (commits.Count() == 0)
                {
                    continue;
                }

                yield return branch;
            }
        }
    }
}
