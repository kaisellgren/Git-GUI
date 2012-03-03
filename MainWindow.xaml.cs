using System;
using System.Windows;

namespace GG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

            this.Style = (Style) Resources["GlassStyle"];
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
