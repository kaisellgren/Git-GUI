using System;
using System.Windows;
using System.Windows.Controls;

namespace GG.UserControls
{
    /// <summary>
    /// Interaction logic for window structure.
    /// </summary>
    public partial class WindowStructure : ContentControl
    {
        #region Body property.
        /// <summary>
        /// The body to use for the window.
        /// </summary>
        public object Body
        {
            get { return (object) GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static DependencyProperty BodyProperty =
            DependencyProperty.Register("Body",
                                        typeof(object),
                                        typeof(WindowStructure),
                                        new PropertyMetadata(null));

        private static void BodyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WindowStructure;
            if (control != null)
            {
                control.Body = (object) e.NewValue;
            }
        }
        #endregion

        public WindowStructure()
        {
            InitializeComponent();
        }
    }
}
