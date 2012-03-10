using System;
using System.Collections.Generic;
using System.Windows;

namespace GG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected ChangesetHistory changesetHistory;
        protected List<Repository> repositories;

        public List<Repository> Repositories
        {
            get;
            set;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Style = (Style) Resources["GlassStyle"];

            //create all 
            this.repositories = new List<Repository>{
                new Repository {Name = "Linux Kernel", FullPath = "Z:/www/git1"},
                new Repository {Name = "Symfony 2", FullPath = "Z:/www/7th-Degree"}
            };

            //show 
            this.RepositoryTabs.ItemsSource = this.repositories;
            this.RepositoryTabs.SelectedIndex = 0;

            //this.changesetHistory = new ChangesetHistory(this);
            //this.Preload();
        }

        /// <summary>
        /// Preloads the application with initial data.
        /// </summary>
        protected void Preload()
        {
            this.changesetHistory.Load();
        }
    }
}
