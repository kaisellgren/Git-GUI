using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GG.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public struct ButtonsSet
        {
            /// <summary>
            /// The OK-Cancel button set.
            /// </summary>
            public const int OK_CANCEL = 1;
        };

        public ConfirmDialog() : base()
        {
            InitializeComponent();

            Application.Current.MainWindow.Effect = new BlurEffect
            {
                Radius = 3
            };

            Closing += OnClosing;

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() =>
                {
                    InvalidateVisual();
                })
            );

            DataContext = this;
        }

        /// <summary>
        /// List of buttons to display to the user.
        /// 
        /// After the dialog has closed, the pressed button can be retrieved from PressedButton.
        /// </summary>
        private ObservableCollection<Button> buttons;

        public ObservableCollection<Button> Buttons
        {
            get
            {
                return buttons;
            }

            set
            {
                buttons = value;

                // Set a click event listener for each button that closes this window.
                foreach (Button button in buttons)
                {
                    button.Click += (object sender, RoutedEventArgs e) =>
                    {
                        PressedButton = (Button) sender;
                        Close();
                    };
                };
            }
        }

        /// <summary>
        /// Sets the button set to use.
        /// </summary>
        public int ButtonSet
        {
            set
            {
                if (value == ButtonsSet.OK_CANCEL)
                {
                    Buttons = new ObservableCollection<Button>
                    {
                        new Button
                        {
                            Content = "OK",
                            IsDefault = true
                        },
                        new Button
                        {
                            Content = "Cancel",
                            IsCancel = true
                        }
                    };
                }
            }
        }

        public ObservableCollection<string> foos = new ObservableCollection<string> { "foo", "bar" };
        public ObservableCollection<string> Foos { get { return foos; } }

        /// <summary>
        /// The message to display to the user.
        /// </summary>
        public string Message
        {
            get { return MessageBlock.Text; }
            set { MessageBlock.Text = value; }
        }

        /// <summary>
        /// Returns the pressed button.
        /// </summary>
        public Button PressedButton { get; private set; }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            // Find if there's a IsCancel button.
            foreach (Button button in buttons)
            {
                if (button.IsCancel == true)
                    PressedButton = button;
            }

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
                
                // Find if there's any button with IsDefault.
                foreach (Button button in buttons)
                {
                    if (button.IsDefault == true)
                    {
                        PressedButton = button;
                        Close();
                    }
                }
            }
        }
    }
}
