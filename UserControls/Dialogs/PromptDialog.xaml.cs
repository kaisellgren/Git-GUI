using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GG.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog() : base()
        {
            InitializeComponent();

            Application.Current.MainWindow.Effect = new BlurEffect
            {
                Radius = 3
            };

            Closing += OnClosing;
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        public string Message
        {
            get { return MessageBlock.Text; }
            set { MessageBlock.Text = value; }
        }

        private void CloseSuccessfully()
        {
            DialogResult = true;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            CloseSuccessfully();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Fired upon window closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.MainWindow.Effect = null;
        }

        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                CloseSuccessfully();
            }
        }
    }
}