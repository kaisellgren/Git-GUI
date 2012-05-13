using System;
using System.Collections.Generic;
using System.Windows.Controls;
using GG.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace GG.Libraries
{
    class RepoUtil
    {
        /// <summary>
        /// Returns a list of visual branches which are around the given commit.
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static List<Branch> GetBranchesAroundCommit(Commit commit, ObservableCollection<Branch> branches)
        {
            List<Branch> list = new List<Branch>();

            // Loop through all branches and determine if they are around the specified commit.
            foreach (Branch branch in branches)
            {
                if (branch.Tip == null)
                    continue;

                // The branch's tip must be newer/same than the commit.
                if (branch.Tip.Date >= commit.Date) // TODO: && first commit-ever must be older? We might not need to do that... ... ?
                {
                    list.Add(branch);
                }
                else
                {
                    // If there's a branch with a tip commit older than commit.Date, then it's around this commit if they don't share a single branch.
                    bool foundThisBranch = branch.Tip.Branches.Any(b => commit.Branches.Contains(b));

                    if (foundThisBranch == false)
                        list.Add(branch);
                }
            }

            return list;
        }

        /// <summary>
        /// Returns all commits that share the same parent (i.e. appear visually around each other -- determines the commit dot horizontal position).
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static List<Commit> GetCommitSiblings(Commit commit, ObservableCollection<Commit> commits)
        {
            List<Commit> siblings = new List<Commit>();

            return siblings;

            /*
            // If there 
            if (commit.ParentCount == 0)
                return siblings;

            // Find one of the branches this commit belongs to.
            IEnumerable<LibGit2Sharp.Branch> myBranches = RepoUtil.GetBranchesContaininingCommit(repo, commit.Hash);

            // Loop through commits of this branch.
            foreach (string hash in commit.ParentHashes)
            {
                IEnumerable<LibGit2Sharp.Commit> commitsSharingParent = repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Refs })
                                                                                                .Where(c => c.Parents.Any(o => o.Sha.ToString() == hash));

                foreach (LibGit2Sharp.Commit c in commitsSharingParent)
                {
                    // Make sure that the siblings share at least one branch. If they don't they are not considered siblings.
                    if (myBranches.Any(o => RepoUtil.GetBranchesContaininingCommit(repo, c.Sha.ToString()).Contains(o)))
                        commits.Add(c.Sha.ToString());
                }
            }

            return commits;
            */
        }

        /// <summary>
        /// A helper method for listing branches that contain the given commit.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commitSha"></param>
        /// <returns></returns>
        public static IEnumerable<LibGit2Sharp.Branch> GetBranchesContaininingCommit(LibGit2Sharp.Repository repo, string commitSha)
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
                var commits = repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = branch }).Where(c => c.Sha == commitSha);

                if (commits.Count() == 0)
                {
                    continue;
                }

                yield return branch;
            }
        }
    }
}
