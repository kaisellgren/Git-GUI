using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG.Models
{
    public class RecentCommitMessage
    {
        public string CroppedMessage { set; get; }
        public string FullMessage { set; get; }

        public RecentCommitMessage(string message)
        {
            FullMessage = message;
            CroppedMessage = message.Substring(0, message.Length >= 72 ? 72 : message.Length);
        }
    }
}
