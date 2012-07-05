using System.Windows;
using System.Windows.Controls;
using GG.Libraries;
using GG.Models;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for CommitPanel.xaml
    /// </summary>
    public partial class CommitPanel : UserControl
    {
        public CommitPanel()
        {
            InitializeComponent();
        }

        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            ((TextBox) sender).Name = "CommitMessageTextBox";
        }

        private void OnRecentCommitMessagesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Retrieve elements.
            var recentCommitMessages = UIHelper.FindChild<ComboBox>(this, "RecentCommitMessages");
            var commitMessageBox = UIHelper.FindChild<TextBox>(this, "CommitMessageTextBox");

            var selectedRecentCommitMessage = recentCommitMessages.SelectedItem as RecentCommitMessage;

            if (selectedRecentCommitMessage == null)
                return;

            // Set the commit text box value.
            commitMessageBox.Text = selectedRecentCommitMessage.FullMessage;

            // Reset the drop down menu.
            recentCommitMessages.SelectedIndex = -1;
        }

        private void ComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            ((ComboBox) sender).Name = "RecentCommitMessages";
        }
    }
}
