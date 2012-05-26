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
using GG.UserControls.Dialogs;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for TopToolbar.xaml
    /// </summary>
    public partial class TopToolbar : UserControl
    {
        public DelegateCommand CreateBranchCommand { get; private set; }

        public TopToolbar()
        {
            InitializeComponent();

            CreateBranchCommand = new DelegateCommand(CreateBranch);
        }

        /// <summary>
        /// Creates a branch.
        /// </summary>
        /// <param name="action"></param>
        private void CreateBranch(object action)
        {
            Console.WriteLine("foo");

            var dialog = new PromptDialog
            {
                Title = "Creating a new branch",
                Message = "Please give a name for your new branch:"
            };

            dialog.ShowDialog();

            var name = dialog.DialogResult;

            Console.WriteLine(name);
        }
    }
}