using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace GG.Models
{
    public class Branch
    {
        /// <summary>
        /// The canonical name in form of "refs/heads/master".
        /// </summary>
        public string CanonicalName { get; set; }

        /// <summary>
        /// The actual name of the branch, e.g. "origin/master".
        /// </summary>
        public string Name { get; set; }

        public Branch TrackedBranch { get; set; }
        private string TrackedBranchName { get; set; }

        public Commit Tip { get; set; }
        public string TipHash { get; set; }

        public bool IsRemote { get; set; }
        public bool IsTracking { get; set; }

        public int AheadBy { get; set; }
        public int BehindBy { get; set; }

        /// <summary>
        /// Stores the right most visual position of any commit within this branch tree.
        /// </summary>
        public int RightMostVisualPosition { get; set; }

        private RepositoryViewModel repositoryViewModel { get; set; }

        /// <summary>
        /// Creates a new branch model.
        /// </summary>
        /// <returns></returns>
        public static Branch Create(RepositoryViewModel repositoryViewModel, LibGit2Sharp.Repository repo, LibGit2Sharp.Branch branch)
        {
            Branch newBranch = new Branch
            {
                CanonicalName = branch.CanonicalName,
                Name = branch.Name,
                IsRemote = branch.IsRemote,
                IsTracking = branch.IsTracking,
                TipHash = branch.Tip.Sha.ToString(),
                AheadBy = branch.AheadBy,
                BehindBy = branch.BehindBy,
                TrackedBranchName = branch.TrackedBranch != null ? branch.TrackedBranch.Name : null
            };

            newBranch.repositoryViewModel = repositoryViewModel;

            // Loop through the first N commits and let them know about me.
            foreach (LibGit2Sharp.Commit branchCommit in branch.Commits.Take(repositoryViewModel.CommitsPerPage))
            {
                Commit commit = repositoryViewModel.Commits.Where(c => c.Hash == branchCommit.Sha.ToString()).FirstOrDefault();

                if (commit != null)
                {
                    commit.Branches.Add(newBranch); // Let the commit know that I am one of her branches.

                    // Process commit DisplayTags (tags to display next to the commit description, in this case = branch Tips).
                    if (newBranch.TipHash == commit.Hash)
                    {
                        commit.DisplayTags.Add(branch.Name);
                        newBranch.Tip = commit;
                    }
                }
            }

            return newBranch;
        }

        /// <summary>
        /// Post processes the branch. This means setting the Tip and Tracking Branch properties.
        /// </summary>
        /// <param name="Branches"></param>
        public void PostProcess(ObservableCollection<Branch> branches, ObservableCollection<Commit> commits)
        {
            // Set the TrackedBranch to be an actual Branch model.
            TrackedBranch = branches.Where(b => b.Name == TrackedBranchName).FirstOrDefault();
            
            // Set the Tip to be an actual Commit model.
            Tip = commits.Where(c => c.Hash == TipHash).FirstOrDefault();
        }

        /// <summary>
        /// Deletes this branch.
        /// </summary>
        public void Delete()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(repositoryViewModel.RepositoryFullPath);

            repo.Branches.Delete(Name);

            repo.Dispose();

            repositoryViewModel.LoadEntireRepository();
        }
    }
}
