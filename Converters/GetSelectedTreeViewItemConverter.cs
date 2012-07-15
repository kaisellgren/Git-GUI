using System;
using System.Linq;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GG.Converters
{
    /// <summary>
    /// Returns the height of the body (data grid actual height - column header height).
    /// </summary>
    class GetSelectedTreeViewItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TreeView) value).SelectedItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}