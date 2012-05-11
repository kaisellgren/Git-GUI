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

        // Stores the information about the last used branch color. We want to recycle these.
        private Int16 branchColorIndex = 0;

        // Stores the X and Y positions of commit dots. Used when drawing the lines that connect the dots.
        private Dictionary<String, Int32[]> CommitDotPositions;
        private List<Brush> AllowedBranchColors = new List<Brush>
        {
            Brushes.Red,
            //new SolidColorBrush(Color.FromRgb(255, 234, 0)), // Yellow
            new SolidColorBrush(Color.FromRgb(165, 212, 0)), // Lime
            new SolidColorBrush(Color.FromRgb(0, 212, 80)), // Green-blue
            new SolidColorBrush(Color.FromRgb(0, 189, 230)), // Cyan
            new SolidColorBrush(Color.FromRgb(65, 123, 255)), // Blue
            new SolidColorBrush(Color.FromRgb(255, 150, 0)), // Orange
            new SolidColorBrush(Color.FromRgb(146, 125, 255)), // Purple
            new SolidColorBrush(Color.FromRgb(255, 74, 236)), // Pink
        };
        private Dictionary<String, Brush> BranchColors = new Dictionary<String, Brush>();

        /// <summary>
        /// Sets colors for branches. Colors are chosen randomly from the list of allowed branch colors.
        /// </summary>
        private void SetBranchColors(LibGit2Sharp.BranchCollection branches)
        {
            foreach (LibGit2Sharp.Branch branch in branches)
            {
                BranchColors.Add(branch.Name, AllowedBranchColors.ElementAt(branchColorIndex++));

                if (branchColorIndex == AllowedBranchColors.Count)
                    branchColorIndex = 0;
            }
        }

        /// <summary>
        /// This methods draws the graph for the changeset history grid.
        /// </summary>
        private void DrawGraph()
        {
            CommitDotPositions = new Dictionary<String, Int32[]>();

            LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(((RepositoryViewModel) DataContext).FullPath);
            ItemCollection commitList = ChangesetHistoryGrid.Items;
            Int16 cellHeight = 24;

            SetBranchColors(repo.Branches);

            // Loop through all commits and draw the graph, in reverse order.
            commitList.MoveCurrentToLast();

            while (true)
            {
                Commit commit = commitList.CurrentItem as Commit;
                Int32 rowNumber = commitList.CurrentPosition;

                // Get a list of visual branches around this commit.
                List<String> branchesAroundCommit = RepoUtil.GetBranchesAroundCommit(commit, repo);

                // Add this commit's branch if it's not already on the list.
                String currentBranch = RepoUtil.ListBranchesContaininingCommit(repo, commit.Hash).ElementAt(0).Name;
                if (branchesAroundCommit.Contains(currentBranch) == false)
                {
                    branchesAroundCommit.Add(currentBranch);
                }

                // Add a few more visual branches (merge commits create these).
                List<String> commitSiblings = RepoUtil.GetCommitSiblings(commit, repo);

                // Sort the lists alphabetically.
                branchesAroundCommit.OrderBy(o => o.ToString());
                commitSiblings.OrderBy(o => o.ToString());

                // Retrieve the index of this commit's branch on the list. This index determines the horizontal positions of dots (commit dots).
                Int32 indexOfCurrentBranch = branchesAroundCommit.IndexOf(currentBranch);
                Int32 indexOfAmongSiblings = commitSiblings.IndexOf(commit.Hash);
                Int32 horizontalIndex = indexOfCurrentBranch + (indexOfAmongSiblings >= 0 ? indexOfAmongSiblings : 0);

                // Draw the dot/ellipse based on the index of the current branch.
                byte dotSize = 8;
                byte horizontalDotSpacing = 10;
                Int32 dotX = horizontalDotSpacing + dotSize * horizontalIndex + horizontalDotSpacing * horizontalIndex;
                Int32 dotY = cellHeight * rowNumber + cellHeight / 2 - dotSize / 2;

                // Store the dot position on the dictionary.
                CommitDotPositions.Add(commit.Hash, new Int32[2] {dotX, dotY});

                Ellipse dot = new Ellipse
                {
                    Fill = Brushes.Black,
                    StrokeThickness = 0,
                    Width = dotSize + 2,
                    Height = dotSize + 2
                };

                Canvas.SetLeft(dot, dotX - 1);
                Canvas.SetTop(dot, dotY - 1);
                Canvas.SetZIndex(dot, 1);

                Graph.Children.Add(dot);

                // Regular commits have a white circle inside.
                if (commit.IsMergeCommit() == false && commit.ParentCount < 2)
                {
                    Ellipse dotInner = new Ellipse
                    {
                        Fill = Brushes.White,
                        StrokeThickness = 0,
                        Width = dotSize,
                        Height = dotSize
                    };

                    Canvas.SetLeft(dotInner, dotX);
                    Canvas.SetTop(dotInner, dotY);
                    Canvas.SetZIndex(dotInner, 2);

                    Graph.Children.Add(dotInner);
                }

                // Draw the line to the parent dot(s)/commit(s).
                foreach (String hash in commit.ParentHashes)
                {
                    // Retrieve the parent commit dot position.
                    Int32[] parentPosition = CommitDotPositions.Where(o => o.Key == hash).First().Value;
                    Brush lineColor = BranchColors[currentBranch];

                    // Calculate line positions.
                    float startLineX1 = dotX + dotSize / 2;
                    float startLineY1 = dotY + dotSize / 2;
                    float endLineX2 = parentPosition[0] + dotSize / 2;
                    float endLineY2 = parentPosition[1] + dotSize / 2;
                    float startLineX2;
                    float startLineY2;
                    float endLineX1;
                    float endLineY1;

                    if (commit.IsMergeCommit())
                    {
                        startLineX2 = endLineX2;
                        startLineY2 = startLineY1;

                        endLineX1 = endLineX2;
                        endLineY1 = startLineY1;
                    }
                    else
                    {
                        startLineX2 = startLineX1;
                        startLineY2 = parentPosition[1] - cellHeight / 2 + dotSize / 2 + 6;

                        endLineX1 = startLineX1;
                        endLineY1 = parentPosition[1] - cellHeight / 2 + dotSize / 2 + 12;
                    }

                    // Construct and draw the line path.
                    Path path = new Path
                    {
                        Stroke = lineColor,
                        StrokeThickness = 3,
                        Data = new PathGeometry
                        {
                            Figures = new PathFigureCollection
                            {
                                new PathFigure
                                {
                                    StartPoint = new Point(startLineX1, startLineY1),
                                    Segments = new PathSegmentCollection
                                    {
                                        new PolyBezierSegment
                                        {
                                            Points = new PointCollection
                                            {
                                                new Point(startLineX2, startLineY2),
                                                new Point(endLineX1, endLineY1),
                                                new Point(endLineX2, endLineY2)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    Graph.Children.Add(path);
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