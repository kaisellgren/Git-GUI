using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG.Libraries;

namespace GG.Models
{
    public class StatusItem
    {
        public LibGit2Sharp.FileStatus Status { set; get; }
        public string Filename { set; get; }
        public string Size { set; get; }
        public string IsBinary { set; get; }

        /// <summary>
        /// Returns the generic status (i.e. either "Staged" or "Unstaged").
        /// </summary>
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

        /// <summary>
        /// Returns true whether this status item is staged.
        /// </summary>
        public bool IsStaged
        {
            get
            {
                return GenericStatus == "Staged";
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(Filename);
            }
        }

        public bool IsIgnored()
        {
            return (Status & LibGit2Sharp.FileStatus.Ignored) == LibGit2Sharp.FileStatus.Ignored;
        }
    }
}
