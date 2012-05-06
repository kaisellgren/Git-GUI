using System;
using System.Windows;
using System.Windows.Controls;

namespace GG.Components
{
    public partial class StatusGridContextMenu : ResourceDictionary
    {
        public StatusGridContextMenu()
        {
            InitializeComponent();
        }  

        void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Console.WriteLine("here i am!");
        }

        private void ContextMenu_ContextMenuClosing_1(object sender, ContextMenuEventArgs e)
        {
            Console.WriteLine("here i am2!");
        }
    }
}