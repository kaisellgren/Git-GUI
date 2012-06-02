using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GG.Libraries;
using GG.Models;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for Panel.xaml
    /// </summary>
    public partial class Panel : UserControl
    {
        #region Header property.
        /// <summary>
        /// The text or content to use for the header.
        /// </summary>
        public object Header
        {
            get { return (object) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header",
                                        typeof(object),
                                        typeof(Panel),
                                        new PropertyMetadata(null));

        private static void HeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Panel;
            if (control != null)
            {
                control.Header = (object) e.NewValue;
            }
        }
        #endregion

        public Panel()
        {
            InitializeComponent();
        }

        private void OnRecentCommitMessagesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Retrieve elements.
            var recentCommitMessages = UIHelper.FindChild<ComboBox>(this, "RecentCommitMessages");
            var commitMessageBox = UIHelper.FindChild<TextBox>(this, "CommitMessageTextBox");

            var selectedRecentCommitMessage = recentCommitMessages.SelectedItem as RecentCommitMessage;

            if (selectedRecentCommitMessage != null)
            {
                // Set the commit text box value.
                commitMessageBox.Text = selectedRecentCommitMessage.FullMessage;

                // Reset the drop down menu.
                recentCommitMessages.SelectedIndex = -1;
            }
        }
    }
}
