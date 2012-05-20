using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GG.Libraries;

namespace GG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.Load();

            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                HideMaximizeRestoreApplicationButton(true, false);
            else
                HideMaximizeRestoreApplicationButton(false, true);
        }

        #region Application maximize, minimize, restore and close.

        /// <summary>
        /// A helper method for showing/hiding maximize and restore buttons.
        /// </summary>
        /// <param name="hideMaximize"></param>
        /// <param name="hideRestore"></param>
        private void HideMaximizeRestoreApplicationButton(bool hideMaximize, bool hideRestore)
        {
            Button maximize = UIHelper.FindChild<Button>(Application.Current.MainWindow, "MaximizeApplicationButton");
            Button restore = UIHelper.FindChild<Button>(Application.Current.MainWindow, "RestoreApplicationButton");

            maximize.Visibility = hideMaximize ? Visibility.Collapsed : Visibility.Visible;
            restore.Visibility = hideRestore ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void MinimizeApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            HideMaximizeRestoreApplicationButton(true, false);
        }

        private void RestoreApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            HideMaximizeRestoreApplicationButton(false, true);
        }

        #endregion

        private void WindowStateChanged(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                HideMaximizeRestoreApplicationButton(true, false);
            else
                HideMaximizeRestoreApplicationButton(false, true);
        }
    }
}