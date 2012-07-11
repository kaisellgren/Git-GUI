using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var vm = DataContext as MainWindowViewModel;
            Debug.Assert(vm != null, "vm != null");
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
            var maximize = UIHelper.FindChild<Button>(Application.Current.MainWindow, "MaximizeApplicationButton");
            var restore = UIHelper.FindChild<Button>(Application.Current.MainWindow, "RestoreApplicationButton");

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

        private int lastRepositoryIndex;

        private void RepositoryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = (TabControl) e.Source;

            if (tabControl == null)
                return;

            // When the user switches the tab via ctrl+tab, make sure we never end up in the "+" tab page.
            if (tabControl.SelectedIndex == tabControl.Items.Count - 1)
            {
                if (lastRepositoryIndex == tabControl.SelectedIndex - 1)
                    tabControl.SelectedIndex = 0;
                else
                    tabControl.SelectedIndex = tabControl.Items.Count - 2;
            }

            lastRepositoryIndex = tabControl.SelectedIndex;
        }
    }
}