using System;

namespace GG.Models
{
    public class Branch
    {
        public String Name { get; set; }
        public String Tip { get; set; }
        public bool IsRemote { get; set; }
        public bool IsTracking { get; set; }
    }
}
