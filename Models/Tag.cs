using System;

namespace GG.Models
{
    public class Tag
    {
        public string CanonicalName { get; set; }
        public string Name { get; set; }
        public Commit Target { get; set; }
        public string TargetSha { get; set; }
        public bool IsAnnotated { get; set; }
        public bool HasCommitAsTarget { get; set; }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Tag Create(LibGit2Sharp.Repository repo, LibGit2Sharp.Tag tag)
        {
            Tag newTag = new Tag
            {
                CanonicalName = tag.CanonicalName,
                Name = tag.Name,
                //Annotation = tag.Annotation,
                IsAnnotated = false, //tag.IsAnnotated,
                TargetSha = tag.Target.Sha,
                HasCommitAsTarget = tag.Target.GetType().FullName == "LibGit2Sharp.Commit"
            };

            return newTag;
        }
    }
}