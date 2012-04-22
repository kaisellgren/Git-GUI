using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG.Libraries;

namespace GG
{
    public class Commit
    {
        public string       AuthorEmail { set; get; }
        public string       AuthorName  { set; get; }
        public DateTime     Date        { set; get; }
        public string       Description { set; get; }
        public List<String> DisplayTags { get; set; }
        public string       Hash        { set; get; }
        public string       Source      { get; set; }

        /// <summary>
        /// Returns the date of this changeset in relative format.
        /// </summary>
        public string FormattedDate
        {
            get
            {
                return DateUtil.GetRelativeDate(Date);
            }
        }

        /// <summary>
        /// Returns a 7 character wide hash string.
        /// </summary>
        public string HashShort
        {
            get
            {
                return Hash.Substring(0, 7);
            }
        }

        /// <summary>
        /// Returns the author in format "name &lt;email&gt;".
        /// </summary>
        public string Author
        {
            get
            {
                return AuthorName + " <" + AuthorEmail + ">";
            }
        }

        /// <summary>
        /// Creates a new commit object from the given parameters.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public static Commit Create(LibGit2Sharp.Repository repo, LibGit2Sharp.Commit commit)
        {
            // Fetch branches.
            IEnumerable<LibGit2Sharp.Branch> branches = ListBranchesContaininingCommit(repo, commit.Sha);

            // Process DisplayTags (tags to display next to the commit description).
            List<String> tags = new List<String>();
            foreach (LibGit2Sharp.Branch branch in branches)
            {
                tags.Add(branch.Name);
            }

            // Create new commit model.
            Commit c = new Commit();

            c.AuthorEmail = commit.Author.Email;
            c.AuthorName = commit.Author.Name;
            c.Date = commit.Author.When.DateTime;
            c.Description = commit.MessageShort;
            c.Hash = commit.Sha;
            c.Source = branches.ElementAt(0).ToString();
            c.DisplayTags = tags;

            return c;
        }

        /// <summary>
        /// A helper method for listing branches that contain the given commit.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commitSha"></param>
        /// <returns></returns>
        private static IEnumerable<LibGit2Sharp.Branch> ListBranchesContaininingCommit(LibGit2Sharp.Repository repo, string commitSha)
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
