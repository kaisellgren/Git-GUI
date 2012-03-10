using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GG
{
    public class ChangesetHistory
    {
        protected MainWindow window;

        public ChangesetHistory(MainWindow window)
        {
            this.window = window;
        }

        public void Load()
        {
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository("Z:/www/git1");
            //Branch master = repo.Branches.ElementAt(0);

            foreach (LibGit2Sharp.Commit commit in repo.Commits)
            {
                IEnumerable<Branch> branches = ListBranchesContaininingCommit(repo, commit.Sha);

                Commit c = new Commit();
                c.AuthorEmail = commit.Author.Email;
                c.AuthorName = commit.Author.Name;
                c.Date = commit.Author.When.ToString("d.M.yyyy H:m:s");
                c.Description = commit.MessageShort;
                c.Hash = commit.Sha;
                c.Source = branches.ElementAt(0).ToString();
                
                //this.window.ChangesetHistory.Items.Add(c);
            }
        }

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