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
        /// Returns a list of branches which are around the given commit (i.e. it has commits newer and older than this commit).
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static List<LibGit2Sharp.Branch> GetBranchesAroundCommit(Commit commit, LibGit2Sharp.Repository repo)
        {
            List<LibGit2Sharp.Branch> list = new List<LibGit2Sharp.Branch>();

            // Loop through all branches and determine if they are around the specified commit.
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                // The branch's tip must be newer than the commit.
                if (branch.Tip.Author.When > commit.Date)
                {
                    // The branch's first commit ever must be older than the commit.
                    if (branch.Commits.Last().Author.When < commit.Date)
                    {
                        list.Add(branch);
                    }
                }
            }

            return list;
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
