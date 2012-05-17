using System;
using System.Windows;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using GG.Libraries;
using GG.Models;
using System.Collections.Generic;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for StatusGridContextMenu.xaml
    /// </summary>
    public partial class StatusGridContextMenu : ContextMenu
    {
        public StatusGridContextMenu()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Preprocessing prior to menu opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem stage = UIHelper.FindChild<MenuItem>(this, "Stage");
            MenuItem unstage = UIHelper.FindChild<MenuItem>(this, "Unstage");
            Separator stageSeparator = UIHelper.FindChild<Separator>(this, "StageSeparator");

            // Retrieve some info regarding what is selected on the grid.
            DataGrid statusGrid = PlacementTarget as DataGrid;

            bool hasStagedItems = statusGrid.SelectedItems.OfType<StatusItem>().Any(i => ((StatusItem) i).IsStaged);
            bool hasUnstagedItems = statusGrid.SelectedItems.OfType<StatusItem>().Any(i => ((StatusItem) i).IsStaged == false);

            // Hide/show stage and unstage menu items accordingly.
            if (hasStagedItems && hasUnstagedItems)
            {
                stage.Visibility = Visibility.Collapsed;
                unstage.Visibility = Visibility.Collapsed;
                stageSeparator.Visibility = Visibility.Collapsed;
            }
            else
            {
                stage.Visibility = !hasStagedItems ? Visibility.Visible : Visibility.Collapsed;
                unstage.Visibility = hasStagedItems ? Visibility.Visible : Visibility.Collapsed;
                stageSeparator.Visibility = Visibility.Visible;
            }
        }
    }
}
