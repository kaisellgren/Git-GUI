using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GG.Models;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for LeftToolbar.xaml
    /// </summary>
    public partial class LeftToolbar : UserControl
    {
        public DelegateCommand DeleteBranchCommand { get; private set; }

        public LeftToolbar()
        {
            InitializeComponent();

            DeleteBranchCommand = new DelegateCommand(DeleteBranch);
        }

        /// <summary>
        /// Deletes a branch.
        /// </summary>
        /// <param name="action"></param>
        private void DeleteBranch(object action)
        {
            Branch branch = action as Branch;

            // TODO: Needs confirmation.
            branch.Delete();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
    }
}
