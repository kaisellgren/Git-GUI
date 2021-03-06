﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using GG.Models;
using GG.Libraries;
using System.Windows.Documents;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for ChangesetHistoryContextMenu.xaml
    /// </summary>
    public partial class ChangesetHistoryContextMenu : ContextMenu
    {
        public ChangesetHistoryContextMenu()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Returns the currently selected commit.
        /// </summary>
        /// <returns></returns>
        private Commit GetCurrentSelectedCommit()
        {
            return ((DataGrid) PlacementTarget).SelectedItem as Commit;
        }

        /// <summary>
        /// Returns the current repository view model instance being used.
        /// </summary>
        /// <returns></returns>
        private RepositoryViewModel GetRepositoryViewModel()
        {
            return ((DataGrid) PlacementTarget).DataContext as RepositoryViewModel;
        }

        /// <summary>
        /// Preprocessing prior to menu opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            var menuItemIndex = 0;
            var commit = GetCurrentSelectedCommit();
            var repositoryViewModel = GetRepositoryViewModel();
            var highlightColor = new SolidColorBrush()
            {
                Color = Color.FromRgb(8, 94, 160)
            };

            // Remove previously added Dynamic menu items. We determine dynamically added menu items by their Tag == "Dynamic".
            RemoveDynamicallyAddedMenuItems();

            if (commit == null)
            {
                return;
            }

            // Add Checkout menu items. TODO: Add also non-branch-checkouts.
            var numberOfCheckoutItems = 0;
            foreach (var branch in commit.Branches)
            {
                // Only if the right clicked commit is the tip of the branch, AND the branch is not already checkout out, continue.
                if (branch.Tip == commit && (repositoryViewModel.Head) != branch && branch.IsRemote == false)
                {
                    var text = new TextBlock();
                    text.Inlines.AddRange(new Inline[]
                    {
                        new Run("Checkout "),
                        new Run(branch.Name) {Foreground = highlightColor}
                    });

                    Items.Insert(menuItemIndex++, CreateMenuItem(text, "Checkout"));
                    numberOfCheckoutItems++;
                }
            }

            if (numberOfCheckoutItems == 0)
            {
                var text = new TextBlock();
                text.Inlines.AddRange(new Inline[]
                {
                    new Run("Checkout commit "),
                    new Run(commit.HashShort) {Foreground = highlightColor}
                });

                Items.Insert(menuItemIndex++, CreateMenuItem(text, "Checkout"));
                numberOfCheckoutItems++;
            }

            if (numberOfCheckoutItems > 1)
            {
                Items.Insert(menuItemIndex++, new Separator
                {
                    Tag = "Dynamic"
                });
            }

            // Add Merge menu items.
            var numberOfMergeItems = 0;

            foreach (Branch branch in commit.Branches)
            {
                if (branch.Tip == commit && branch != (Branch) repositoryViewModel.Head)
                {
                    // Add those that track this branch.
                    foreach (Branch branchThatTracks in RepoUtil.GetBranchesThatTrack(branch, repositoryViewModel.Branches))
                    {
                        if (branchThatTracks.BehindBy > 0 &&
                            branchThatTracks.IsRemote == false &&
                            branch.Tip != branchThatTracks.Tip &&
                            repositoryViewModel.Head is DetachedHead == false &&
                            branch.Tip != branchThatTracks.Tip)
                        {
                            var text = new TextBlock();
                            text.Inlines.AddRange(new Inline[]
                            {
                                new Run("Merge "),
                                new Run(branch.Name) {Foreground = highlightColor},
                                new Run(" into "),
                                new Run(branchThatTracks.Name) {Foreground = highlightColor}
                            });

                            Items.Insert(menuItemIndex++, CreateMenuItem(text, "Merge"));
                            numberOfCheckoutItems++;
                        }
                    }
                }

                if (branch.Tip == commit &&
                    branch != (Branch) repositoryViewModel.Head &&
                    repositoryViewModel.Head is DetachedHead == false &&
                    commit != ((Branch) repositoryViewModel.Head).Tip)
                {
                    var text = new TextBlock();
                    text.Inlines.AddRange(new Inline[]
                    {
                        new Run("Merge "),
                        new Run(branch.Name) {Foreground = highlightColor},
                        new Run(" into "),
                        new Run(((Branch) repositoryViewModel.Head).Name) {Foreground = highlightColor}
                    });

                    Items.Insert(menuItemIndex++, CreateMenuItem(text, "Merge"));
                    numberOfCheckoutItems++;
                }
            }

            if (numberOfMergeItems > 1)
            {
                Items.Insert(menuItemIndex++, new Separator
                {
                    Tag = "Dynamic"
                });
            }
        }

        /// <summary>
        /// Removes all dynamically added menu items.
        /// </summary>
        private void RemoveDynamicallyAddedMenuItems()
        {
            IList<object> itemsToRemove = Items.Cast<object>().Where(mi =>
            {
                if (mi is MenuItem == false)
                    return false;

                var tag = ((MenuItem) mi).Tag;

                return tag != null && tag.ToString() == "Dynamic";
            }).ToList();

            foreach (MenuItem item in itemsToRemove)
            {
                Items.Remove(item);
            }
        }

        /// <summary>
        /// Creates and returns a new menu item.
        /// </summary>
        /// <returns></returns>
        private MenuItem CreateMenuItem(object header, string icon)
        {
            return new MenuItem
            {
                Header = header,
                Tag = "Dynamic",
                Icon = new Image
                {
                    Stretch = Stretch.None,
                    Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/" + icon + ".png", UriKind.Absolute))
                }
            };
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var commit = GetCurrentSelectedCommit();

            if (commit == null)
            {
                e.Handled = true;
                return;
            }
        }
    }
}