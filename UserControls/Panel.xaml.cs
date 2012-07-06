using System.Windows;
using System.Windows.Controls;

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

        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header",
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

        #region Body property.
        /// <summary>
        /// The text or content to use for the body.
        /// </summary>
        public object Body
        {
            get { return (object) GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static DependencyProperty BodyProperty = DependencyProperty.Register("Body",
                                                                                    typeof(object),
                                                                                    typeof(Panel),
                                                                                    new PropertyMetadata(null));

        private static void BodyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Panel;
            if (control != null)
            {
                control.Body = (object) e.NewValue;
            }
        }
        #endregion

        #region Header buttons property.
        /// <summary>
        /// The text or content to use for the header buttons.
        /// </summary>
        public object HeaderButtons
        {
            get { return (object) GetValue(HeaderButtonsProperty); }
            set { SetValue(HeaderButtonsProperty, value); }
        }

        public static DependencyProperty HeaderButtonsProperty = DependencyProperty.Register("HeaderButtons",
                                                                                             typeof(object),
                                                                                             typeof(Panel),
                                                                                             new PropertyMetadata(null));

        private static void HeaderButtonsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Panel;
            if (control != null)
            {
                control.HeaderButtons = (object) e.NewValue;
            }
        }
        #endregion

        public Panel()
        {
            InitializeComponent();
        }
    }
}
