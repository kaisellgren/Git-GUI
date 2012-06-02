using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG.Models
{
    public class RecentCommitMessage
    {
        private string croppedMessage;
        public int croppedMessageLength { set; get; }

        public string CroppedMessage { 
            set
            {
                croppedMessage = value.Substring(0, this.croppedMessageLength);
            }
            get
            {
                return croppedMessage;
            }
        }
        public string FullMessage { set; get; }
    }
}
