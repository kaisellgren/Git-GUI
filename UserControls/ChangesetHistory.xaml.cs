using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GG.Libraries;
using GG.Models;
using System.Linq;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for ChangesetHistory.xaml
    /// </summary>
    public partial class ChangesetHistory : UserControl
    {
        public ChangesetHistory()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the changeset history data changes, redraw the graph.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataContextChanged(object sender, RoutedEventArgs e)
        {
            DrawGraph();
        }

        #region Graph drawing

        // Stores the X and Y positions of commit dots. Used when drawing the lines that connect the dots.
        private Dictionary<String, Int32[]> CommitDotPositions;

        /// <summary>
        /// This methods draws the graph for the changeset history grid.
        /// </summary>
        private void DrawGraph()
        {
            CommitDotPositions = new Dictionary<String, Int32[]>();
            ItemCollection commitList = ChangesetHistoryGrid.Items;
            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(((RepositoryViewModel) DataContext).FullPath);
            Int16 cellHeight = 24;

            // Loop through all commits and draw the graph, in reverse order.
            commitList.MoveCurrentToLast();

            while (true)
            {
                Commit commit = commitList.CurrentItem as Commit;
                Int32 rowNumber = commitList.CurrentPosition;

                // Get a list of branches around this commit.
                List<LibGit2Sharp.Branch> branches = RepoUtil.GetBranchesAroundCommit(commit, repo);

                // Add this commit's branch if it's not already on the list.
                LibGit2Sharp.Branch currentBranch = RepoUtil.ListBranchesContaininingCommit(repo, commit.Hash).ElementAt(0);
                if (branches.Contains(currentBranch) == false)
                {
                    branches.Add(currentBranch);
                }

                // Sort the list alphabetically.
                branches.OrderBy(o => o.Name);

                // Retrieve the index of this commit's branch on the list.
                Int32 indexOfCurrentBranch = branches.IndexOf(currentBranch);

                // Draw the dot/ellipse based on the index of the current branch.
                byte dotSize = 6;
                byte horizontalDotSpacing = 8;
                Int32 dotX = horizontalDotSpacing + dotSize * indexOfCurrentBranch + horizontalDotSpacing * indexOfCurrentBranch;
                Int32 dotY = cellHeight * rowNumber + cellHeight / 2 - dotSize / 2;

                // Store the dot position on the dictionary.
                CommitDotPositions.Add(commit.Hash, new Int32[2] {dotX, dotY});

                Ellipse dot = new Ellipse
                {
                    Fill = Brushes.Black,
                    StrokeThickness = 0,
                    Width = dotSize,
                    Height = dotSize
                };

                Canvas.SetLeft(dot, dotX);
                Canvas.SetTop(dot, dotY);
                Canvas.SetZIndex(dot, 1);

                Graph.Children.Add(dot);

                // Draw the line to the parent dot(s)/commit(s).
                foreach (String hash in commit.ParentHashes)
                {
                    // Retrieve the parent commit dot position.
                    Int32[] parentPosition = CommitDotPositions.Where(o => o.Key == hash).First().Value;

                    Line line = new Line
                    {
                        StrokeThickness = 2,
                        Stroke = Brushes.Red,
                        X1 = dotX + dotSize / 2,
                        Y1 = dotY + dotSize / 2,
                        X2 = parentPosition[0] + dotSize / 2,
                        Y2 = parentPosition[1] + dotSize / 2
                    };

                    Graph.Children.Add(line);
                }

                commitList.MoveCurrentToPrevious();
                if (commitList.IsCurrentBeforeFirst)
                {
                    break;
                }
            }

            repo.Dispose();
        }

        #endregion
    }
}