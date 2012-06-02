using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG.Models
{
    /// <summary>
    /// This class represents a recent commit message.
    /// </summary>
    public class RecentCommitMessage
    {
        public string CroppedMessage { set; get; }
        public string FullMessage { set; get; }

        public RecentCommitMessage(string message)
        {
            FullMessage = message;
            CroppedMessage = message.Right(72);
        }
    }
}
