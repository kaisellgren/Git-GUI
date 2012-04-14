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
        public string AuthorEmail { set; get; }
        public string AuthorName { set; get; }
        public DateTime Date { set; get; }
        public string Description { set; get; }
        public string Hash { set; get; }
        public string Source { set; get; }

        public string FormattedDate
        {
            get
            {
                return DateUtil.GetRelativeDate(Date);
            }
        }
    }
}
