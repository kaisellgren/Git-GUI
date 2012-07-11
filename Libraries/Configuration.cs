using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace GG.Libraries
{
    [XmlRoot("Configuration")]
    public class Configuration
    {
        [XmlArrayItem("Repository")]
        [XmlArray("RecentRepositories")]
        public List<RecentRepositoryConfiguration> RecentRepositories { get; set; }

        /// <summary>
        /// Loads the configuration from the Configuration.xml file.
        /// </summary>
        /// <returns></returns>
        public static Configuration LoadConfiguration()
        {
            if (File.Exists("./Configuration.xml"))
            {
                using (var fileStream = new FileStream("./Configuration.xml", FileMode.Open))
                {
                    return (Configuration) new XmlSerializer(typeof(Configuration)).Deserialize(fileStream);
                }
            }

            // If the configuration file was not found, create a new empty configuration and save it.
            var configuration = new Configuration();
            configuration.Save();

            return configuration;
        }

        /// <summary>
        /// Saves the configuration to the Configuration.xml file.
        /// </summary>
        public void Save()
        {
            using (var fileStream = new FileStream("./Configuration.xml", FileMode.Create))
            {
                new XmlSerializer(typeof(Configuration)).Serialize(fileStream, this);
            }
        }

        public Configuration()
        {
            RecentRepositories = new List<RecentRepositoryConfiguration>();
        }
    }

    public class RecentRepositoryConfiguration
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("RepositoryFullPath")]
        public string RepositoryFullPath { get; set; }
    }
}