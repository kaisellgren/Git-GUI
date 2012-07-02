using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using GG.Models;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;

namespace GG.Libraries
{
    class ChangesetGraph
    {
        private Canvas graph;
        private Int16 cellHeight = 24;
        private RepositoryViewModel repositoryViewModel;

        public int TotalHeight { get; private set; }

        // A highlight color to reuse in tooltips.
        private SolidColorBrush highlightColor = new SolidColorBrush()
        {
            Color = Color.FromRgb(8, 94, 160)
        };

        // Stores the information about the last used branch color. We want to recycle these.
        private Int16 branchColorIndex = 0;

        // Stores the X and Y positions of commit dots. Used when drawing the lines that connect the dots.
        private Dictionary<string, int[]> commitDotPositions = new Dictionary<string,int[]>();

        // A list of colors per branch.
        private Dictionary<string, Brush> BranchColors = new Dictionary<string, Brush>();

        // A list of allowed branch colors.
        private List<Brush> AllowedBranchColors = new List<Brush>
        {
            Brushes.Red,
            new SolidColorBrush(Color.FromRgb(165, 212, 0)), // Lime
            new SolidColorBrush(Color.FromRgb(0, 212, 80)), // Green-blue
            new SolidColorBrush(Color.FromRgb(0, 189, 230)), // Cyan
            new SolidColorBrush(Color.FromRgb(65, 123, 255)), // Blue
            new SolidColorBrush(Color.FromRgb(255, 150, 0)), // Orange
            new SolidColorBrush(Color.FromRgb(146, 125, 255)), // Purple
            new SolidColorBrush(Color.FromRgb(255, 74, 236)), // Pink
        };

        public ChangesetGraph(RepositoryViewModel repositoryViewModel, Canvas graph)
        {
            this.repositoryViewModel = repositoryViewModel;
            this.graph = graph;
        }

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
        public void Draw(ItemCollection commitList)
        {
            if (graph == null)
                return;

            Console.WriteLine("Drawing graph.");

            var repo = new LibGit2Sharp.Repository(repositoryViewModel.RepositoryFullPath);
            commitDotPositions.Clear();
            SetBranchColors(repo.Branches);

            // Loop through all commits and draw the graph, in reverse order.
            commitList.MoveCurrentToLast();

            // Clear the existing graph.
            graph.Children.Clear();

            TotalHeight = 0;
            
            while (true)
            {
                Commit commit = commitList.CurrentItem as Commit;
                int rowNumber = commitList.CurrentPosition;

                if (commit == null)
                    break;

                // Get a list of branches around this commit.
                List<Branch> branchesAroundCommit = commit.BranchesAround;

                // Sort the lists alphabetically.
                branchesAroundCommit.OrderBy(o => o.Name.ToString());

                // Retrieve the index of this commit's branch on the list. This index determines the horizontal positions of dots (commit dots).
                int indexOfCurrentBranch;
                if (commit.Branches.Count > 0)
                    indexOfCurrentBranch = branchesAroundCommit.IndexOf(commit.Branches.ElementAt(0));
                else
                    indexOfCurrentBranch = 0;

                int horizontalIndex = indexOfCurrentBranch + commit.VisualPosition;
                for (var i = indexOfCurrentBranch - 1; i >= 0; i--)
                    horizontalIndex += ((Branch) branchesAroundCommit.ElementAt(i)).RightMostVisualPosition;

                // Draw the dot/ellipse based on the index of the current branch.
                byte dotSize = 10;
                byte horizontalDotSpacing = 12;

                int dotX = horizontalDotSpacing + dotSize * horizontalIndex + horizontalDotSpacing * horizontalIndex;
                int dotY = cellHeight * rowNumber + cellHeight / 2 - dotSize / 2;

                if (TotalHeight == 0)
                    TotalHeight = cellHeight * (rowNumber + 1);

                // Store the dot position on the dictionary.
                commitDotPositions.Add(commit.Hash, new int[2] { dotX, dotY });

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

                graph.Children.Add(dot);

                // ToolTip for commits.
                var commitTooltip = new TextBlock
                {
                    MaxWidth = 320,
                    TextWrapping = TextWrapping.Wrap
                };

                // ToolTip for paths.
                var pathTooltip = new TextBlock
                {
                    MaxWidth = 320,
                    TextWrapping = TextWrapping.Wrap
                };

                if (commit.Branches.Count == 1)
                {
                    pathTooltip.Text = commit.Branches.ElementAt(0).Name;
                }
                else
                {
                    int i = 0, count = commit.Branches.Count;

                    commit.Branches.ForEach(b =>
                    {
                        i++;
                        pathTooltip.Inlines.AddRange(new Inline[]
                        {
                            new Run(b.Name + (i < count ? ", " : "")),
                        });
                    });
                }

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

                    graph.Children.Add(dotInner);

                    // ToolTip.
                    commitTooltip.Inlines.AddRange(new Inline[]
                    {
                        new Run("Commit: ")  {FontWeight = FontWeights.Bold},
                        new Run(commit.HashShort) {Foreground = highlightColor, FontWeight = FontWeights.Bold},
                        new LineBreak(),
                        new Run("Author: ") {FontWeight = FontWeights.Bold},
                        new Run(commit.AuthorName) ,
                        new LineBreak(),
                        new Run("Date: ") {FontWeight = FontWeights.Bold},
                        new Run(commit.FormattedDate),
                        new LineBreak(),
                        new LineBreak(),
                        new Run(commit.Description.TrimEnd())
                    });

                    dotInner.ToolTip = commitTooltip;
                    ToolTipService.SetShowDuration(dotInner, 60000);
                    ToolTipService.SetInitialShowDelay(dotInner, 1);
                }
                else
                {
                    // Tooltip.
                    commitTooltip.Inlines.AddRange(new Inline[]
                    {
                        new Run("Merge commit: ")  {FontWeight = FontWeights.Bold},
                        new Run(commit.HashShort) {Foreground = highlightColor, FontWeight = FontWeights.Bold},
                        new LineBreak(),
                        new Run("Author: ") {FontWeight = FontWeights.Bold},
                        new Run(commit.AuthorName) ,
                        new LineBreak(),
                        new Run("Date: ") {FontWeight = FontWeights.Bold},
                        new Run(commit.FormattedDate),
                        new LineBreak(),
                        new LineBreak(),
                        new Run(commit.Description.TrimEnd())
                    });

                    dot.ToolTip = commitTooltip;
                    ToolTipService.SetShowDuration(dot, 60000);
                    ToolTipService.SetInitialShowDelay(dot, 1);
                }

                if (commit.Branches.Count > 0)
                {
                    // Draw the line to the parent dot(s)/commit(s).
                    foreach (string hash in commit.ParentHashes)
                    {
                        // Retrieve the parent commit dot position.
                        var positions = commitDotPositions.Where(o => o.Key == hash);

                        if (positions.Count() > 0)
                        {
                            int[] parentPosition = commitDotPositions.Where(o => o.Key == hash).First().Value;

                            Brush lineColor = BranchColors[commit.Branches.ElementAt(0).Name];

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
                                StrokeThickness = 4,
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

                            graph.Children.Add(path);

                            path.ToolTip = pathTooltip;
                            ToolTipService.SetShowDuration(path, 60000);
                            ToolTipService.SetInitialShowDelay(path, 1);
                        }
                    }
                }

                commitList.MoveCurrentToPrevious();
                if (commitList.IsCurrentBeforeFirst)
                {
                    break;
                }
            }

            repo.Dispose();
        }
    }
}
