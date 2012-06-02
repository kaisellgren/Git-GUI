using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GG.Libraries;
using System.Linq;
using System.Collections.ObjectModel;

namespace GG.Models
{
    public class Commit
    {
        public string       AuthorEmail      { get; set; }
        public string       AuthorName       { get; set; }
        public DateTime     Date             { get; set; }
        public string       Description      { get; set; }
        public string       ShortDescription { get; set; }
        public List<string> DisplayTags      { get; set; }
        public List<string> Tags             { get; set; }
        public string       Hash             { get; set; }
        public List<Branch> Branches         { get; set; }
        public List<Branch> BranchesAround   { get; set; }
        public List<string> ParentHashes     { get; set; }
        public int          ParentCount      { get; set; }
        public List<Commit> Parents          { get; set; }
        public List<Commit> Siblings         { get; set; }
        public bool         IsHead           { get; set; }

        /// <summary>
        /// Returns the ObjectId of LibGit2Sharp.
        /// </summary>
        public LibGit2Sharp.ObjectId ObjectId { get; set; }

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
        /// Returns whether this is a merge commit.
        /// </summary>
        /// <returns></returns>
        public bool IsMergeCommit()
        {
            return ParentCount > 1;
        }

        /// <summary>
        /// Creates a new commit object from the given parameters.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public static Commit Create(LibGit2Sharp.Repository repo,
                                    LibGit2Sharp.Commit commit,
                                    ObservableCollection<Tag> tags)
        {
            Commit c = new Commit();

            // Process Tags (Git tags to display next to the commit description).
            List<string> commitTags = new List<string>();
            foreach (Tag tag in tags)
            {
                if (tag.TargetSha == commit.Sha.ToString())
                {
                    commitTags.Add(tag.Name);
                    tag.Target = c;
                }
            }

            // Process display tags.
            List<string> displayTags = new List<string>();
            if (repo.Head.Tip == commit)
                displayTags.Add("HEAD");

            // Process ParentHashes.
            List<string> parentHashes = new List<string>();
            foreach (LibGit2Sharp.Commit parentCommit in commit.Parents)
            {
                parentHashes.Add(parentCommit.Sha.ToString());
            }

            // Set properties.
            c.AuthorEmail  = commit.Author.Email;
            c.AuthorName   = commit.Author.Name;
            c.Date         = commit.Author.When.DateTime;
            c.Description  = commit.Message;
            c.ShortDescription = commit.Message.Right(72).RemoveLineBreaks();
            c.DisplayTags  = displayTags;
            c.Branches     = new List<Branch>();
            c.Tags         = commitTags;
            c.Hash         = commit.Sha.ToString();
            c.ParentHashes = parentHashes;
            c.ParentCount  = commit.ParentsCount;
            c.Parents      = new List<Commit>();
            c.ObjectId     = commit.Id;

            return c;
        }

        /// <summary>
        /// Post-processes the commit. This means that we set up the parent object relationship.
        /// </summary>
        /// <param name="Commits"></param>
        public void PostProcess(ObservableCollection<Commit> commits, ObservableCollection<Branch> branches)
        {
            // Set Parents.
            if (ParentCount > 0)
            {
                foreach (string hash in ParentHashes)
                {
                    Commit parentCommit = commits.Where(c => c.Hash == hash).FirstOrDefault();

                    if (parentCommit != null)
                        Parents.Add(parentCommit);
                }
            }

            // Set Siblings.
            Siblings = RepoUtil.GetCommitSiblings(this, commits);

            // SiblingTreeCount ?

            // Set BranchesAround.
            BranchesAround = RepoUtil.GetBranchesAroundCommit(this, branches);
        }
    }
}