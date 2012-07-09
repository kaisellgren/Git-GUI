using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            commitMessageBox.Focus();

            // Reset the drop down menu.
            recentCommitMessages.SelectedIndex = -1;
        }

        private void CommitMessageLostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void CommitMessageGotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CommitPanelLoaded(object sender, RoutedEventArgs e)
        {
            // Find elements.
            var commitButton = UIHelper.FindChild<Button>(this, "CommitButton");
            var commitMessageBox = UIHelper.FindChild<TextBox>(this, "CommitMessageTextBox");

            // Set up bindings for commit button.
            commitButton.Command = ((RepositoryViewModel) DataContext).CommitCommand;
            commitButton.CommandParameter = commitMessageBox;

            // Set up bindings for commit text box.
            commitMessageBox.InputBindings.Add(new KeyBinding
            {
                Key = Key.Enter,
                Modifiers = ModifierKeys.Control,
                Command = ((RepositoryViewModel) DataContext).CommitCommand,
                CommandParameter = commitMessageBox
            });
        }

        #region Names.

        private void ComboBoxInitialized(object sender, EventArgs e)
        {
            ((ComboBox) sender).Name = "RecentCommitMessages";
        }

        private void TextBoxInitialized(object sender, EventArgs e)
        {
            ((TextBox) sender).Name = "CommitMessageTextBox";
        }

        private void ButtonInitialized(object sender, EventArgs e)
        {
            ((Button) sender).Name = "CommitButton";
        }

        #endregion
    }
}