using System;
using System.Collections.Generic;
using System.Windows.Controls;
using GG.Models;
using System.Linq;

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
        public static List<String> GetBranchesAroundCommit(Commit commit, LibGit2Sharp.Repository repo)
        {
            List<String> list = new List<String>();

            // Loop through all branches and determine if they are around the specified commit.
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                // The branch's tip must be newer/same than the commit.
                if (branch.Tip.Author.When >= commit.Date)
                {
                    list.Add(branch.Name.ToString());
                }
                else
                {
                    // None of the branches in this commit should exist in any of those branches containing the tip commit or else it's "around".
                    LibGit2Sharp.Branch targetBranch = RepoUtil.ListBranchesContaininingCommit(repo, commit.Hash).ElementAt(0);

                    bool foundThisBranch = false;
                    foreach (LibGit2Sharp.Branch b in RepoUtil.ListBranchesContaininingCommit(repo, branch.Tip.Sha))
                    {
                        if (targetBranch.CanonicalName == b.CanonicalName)
                        {
                            foundThisBranch = true;
                            break;
                        }
                    }

                    if (foundThisBranch == false)
                        list.Add(branch.Name.ToString());
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
        public static List<String> GetCommitSiblings(Commit commit, LibGit2Sharp.Repository repo)
        {
            List<String> commits = new List<String>();

            // If there 
            if (commit.ParentCount == 0)
                return commits;

            // Find one of the branches this commit belongs to.
            IEnumerable<LibGit2Sharp.Branch> myBranches = RepoUtil.ListBranchesContaininingCommit(repo, commit.Hash);

            // Loop through commits of this branch.
            foreach (String hash in commit.ParentHashes)
            {
                IEnumerable<LibGit2Sharp.Commit> commitsSharingParent = repo.Commits.QueryBy(new LibGit2Sharp.Filter { Since = repo.Refs })
                                                                                                .Where(c => c.Parents.Any(o => o.Sha.ToString() == hash));

                foreach (LibGit2Sharp.Commit c in commitsSharingParent)
                {
                    // Make sure that the siblings share at least one branch. If they don't they are not considered siblings.
                    if (myBranches.Any(o => RepoUtil.ListBranchesContaininingCommit(repo, c.Sha.ToString()).Contains(o)))
                        commits.Add(c.Sha.ToString());
                }
            }

            return commits;
        }

        /// <summary>
        /// A helper method for listing branches that contain the given commit.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commitSha"></param>
        /// <returns></returns>
        public static IEnumerable<LibGit2Sharp.Branch> ListBranchesContaininingCommit(LibGit2Sharp.Repository repo, string commitSha)
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
