using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GG.Libraries;
using System.Linq;

namespace GG.Models
{
    public class Commit
    {
        public string       AuthorEmail  { set; get; }
        public string       AuthorName   { set; get; }
        public DateTime     Date         { set; get; }
        public string       Description  { set; get; }
        public List<String> DisplayTags  { get; set; }
        public List<String> Tags         { get; set; }
        public string       Hash         { set; get; }
        public string       Source       { get; set; }
        public List<String> ParentHashes { get; set; }

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
            // Process DisplayTags (tags to display next to the commit description).
            List<String> displayTags = new List<String>();
            foreach (LibGit2Sharp.Branch branch in repo.Branches)
            {
                if (branch.Tip.Sha == commit.Sha)
                {
                    displayTags.Add(branch.Name);
                }
            }

            // Process Tags (Git tags to display next to the commit description).
            List<String> tags = new List<String>();
            foreach (LibGit2Sharp.Tag tag in repo.Tags)
            {
                if (tag.Target.Sha == commit.Sha)
                {
                    tags.Add(tag.Name);
                }
            }

            // Process ParentHashes.
            List<String> parentHashes = new List<String>();
            foreach (LibGit2Sharp.Commit parentCommit in commit.Parents)
            {
                parentHashes.Add(parentCommit.Sha.ToString());
            }

            // Create new commit model.
            Commit c = new Commit();

            c.AuthorEmail = commit.Author.Email;
            c.AuthorName = commit.Author.Name;
            c.Date = commit.Author.When.DateTime;
            c.Description = commit.MessageShort;
            c.Hash = commit.Sha;
            c.ParentHashes = parentHashes;
            c.Source = RepoUtil.ListBranchesContaininingCommit(repo, commit.Sha).ElementAt(0).ToString();
            c.Source = c.Source.Replace("refs/heads/", "").Replace("refs/remotes/", "");
            c.DisplayTags = displayTags;
            c.Tags = tags;

            return c;
        }
    }
}
