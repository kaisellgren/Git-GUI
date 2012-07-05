using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for CenterArea.xaml
    /// </summary>
    public partial class CenterArea : UserControl
    {
        public CenterArea()
        {
            InitializeComponent();
        }

        private void GridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            // We want the grid splitter to snap in grid of 24 units.
            var excess = (int) ChangesetHistoryRowDefinition.Height.Value % 24;

            if (excess == 0)
                return;

            ChangesetHistoryRowDefinition.Height = new GridLength(ChangesetHistoryRowDefinition.Height.Value - excess);
        }
    }
}