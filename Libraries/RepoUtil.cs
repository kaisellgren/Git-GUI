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
        /// Retrives the tree changes for the given commit.
        /// </summary>
        /// <returns></returns>
        public static LibGit2Sharp.TreeChanges GetTreeChangesForCommit(LibGit2Sharp.Repository repo, Commit commit)
        {
            // Retrieve the Tree for this commit.
            LibGit2Sharp.Tree thisTree = ((LibGit2Sharp.Commit) repo.Lookup(commit.ObjectId)).Tree;

            // Retrieve the Tree for the previous commit (parent).
            // TODO: What about Merge commits?
            LibGit2Sharp.Tree parentTree = ((LibGit2Sharp.Commit) repo.Lookup(commit.Parents.ElementAt(0).ObjectId)).Tree;

            // Take the diff.
            return repo.Diff.Compare(parentTree, thisTree);
        }

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
                // Tip has to be found and in case multiple branches share the tree, get rid of the others -- messes up visual position counting.
                if (branch.Tip == null || list.Any(b => branch.Tip.Branches.Contains(b)) || list.Any(b => b.Tip.Branches.Contains(branch)))
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
        /// Increments the visual position for this commit tree.
        /// </summary>
        /// <param name="commit"></param>
        public static void IncrementCommitTreeVisualPositionsRecursively(Commit commit, int level = 0)
        {
            // The visual position for this commit has already been calculated or the commit does not exist.
            if (commit == null || commit.VisualPosition != -1)
                return;

            // We have reached a commit with multiple children, we only continue if this commit is the "left most chain".
            if (commit.Children.Count > 1 && level > 0)
                return;

            // Update commit's visual position.
            commit.VisualPosition = level;

            // Update commit's branches' visual positions if needed.
            commit.Branches.ForEach(b =>
            {
                if (b.RightMostVisualPosition < level)
                    b.RightMostVisualPosition = level;
            });

            if (commit.IsMergeCommit())
            {
                int i = 0;
                foreach (Commit parentCommit in commit.Parents)
                {
                    RepoUtil.IncrementCommitTreeVisualPositionsRecursively(parentCommit, level + i);

                    i++;
                }
            }
            else if (commit.Parents.Count > 0)
            {
                RepoUtil.IncrementCommitTreeVisualPositionsRecursively(commit.Parents.ElementAt(0));
            }
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

        /// <summary>
        /// Returns all branches that track the given branch.
        /// </summary>
        /// <param name="branch"></param>
        /// <returns></returns>
        public static IEnumerable<Branch> GetBranchesThatTrack(Branch branch, ObservableCollection<Branch> branches)
        {
            return branches.Where(b => b.TrackedBranch == branch);
        }
    }
}
