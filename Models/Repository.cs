using System.Collections.Generic;

namespace GG
{
    public class Repository
    {
        public Repository() { }

        public string Name { get; set; }
        public string FullPath { get; set; }
    }

    public class RepositoryCollection : List<Repository>
    {
        public RepositoryCollection() { }
    }
}
