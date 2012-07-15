using System.Windows.Controls;
using GG.Models;

namespace GG.UserControls.LeftToolbarContextMenus
{
    /// <summary>
    /// Interaction logic for ChangesetHistoryContextMenu.xaml
    /// </summary>
    public partial class TagContextMenu : ContextMenu
    {
        public TagContextMenu()
        {
            InitializeComponent();
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
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }
    }
}