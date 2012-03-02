using System;
using System.Windows;
using Microsoft.Windows.Controls.Ribbon;

namespace GG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        protected ChangesetHistory changesetHistory;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            this.changesetHistory = new ChangesetHistory(this);
            this.Preload();
        }

        /// <summary>
        /// Preloads the application with initial data.
        /// </summary>
        protected void Preload()
        {
            this.changesetHistory.Load();
        }
    }
}
