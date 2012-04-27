using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG.Libraries;

namespace GG
{
    public class StatusItem
    {
        public LibGit2Sharp.FileStatus Status { set; get; }
        public string Filename { set; get; }
        public string Type { set; get; }
        public string Size { set; get; }

        public string GenericStatus
        {
            get
            {
                if ((Status & LibGit2Sharp.FileStatus.Staged) == LibGit2Sharp.FileStatus.Staged)
                {
                    return "Staged";
                }

                if ((Status & LibGit2Sharp.FileStatus.Removed) == LibGit2Sharp.FileStatus.Removed)
                {
                    return "Staged";
                }

                if ((Status & LibGit2Sharp.FileStatus.Added) == LibGit2Sharp.FileStatus.Added)
                {
                    return "Staged";
                }

                return "Not staged";
            }
        }

        public bool IsIgnored()
        {
            if ((Status & LibGit2Sharp.FileStatus.Ignored) == LibGit2Sharp.FileStatus.Ignored)
            {
                return true;
            }

            return false;
        }
    }
}
