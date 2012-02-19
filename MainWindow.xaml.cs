using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Git_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadGitLog_Click(object sender, RoutedEventArgs e)
        {
            Repository repo = new Repository("Z:/www/gg");
            Branch master = repo.Branches.ElementAt(0);

            foreach (LibGit2Sharp.Commit commit in master.Commits)
            {
                Commit c = new Commit();
                c.message = commit.Message;
                c.hash = commit.GetHashCode().ToString();
                c.author = commit.Author.Name;
                ChangesetHistory.Items.Add(c);
            }
        }
    }

    public struct Commit
    {
        public string hash { set; get; }
        public string message { set; get; }
        public string author { set; get; }
    }
}
