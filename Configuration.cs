using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GG
{
    [XmlRoot("Configuration")]
    public class Configuration
    {
        [XmlArrayItem("Repository")]
        [XmlArray("RecentRepositories")]
        public List<RecentRepositoryConfiguration> RecentRepositories { get; set; }
    }

    public class RecentRepositoryConfiguration
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("RepositoryFullPath")]
        public string RepositoryFullPath { get; set; }
    }
}