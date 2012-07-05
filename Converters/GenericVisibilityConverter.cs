using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GG.Converters
{
    public class GenericVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Whether to invert the visibility.
            var invert = (string) parameter == "not" ? true : false;

            // int 0
            if (value is int && (int) value == 0)
                return invert ? Visibility.Visible : Visibility.Collapsed;

            // int > 0
            if (value is int && (int) value > 0)
                return invert ? Visibility.Collapsed : Visibility.Visible;

            // null || bool == false
            if (value == null || (value is bool && (bool) value == false))
                return invert ? Visibility.Visible : Visibility.Collapsed;

            // bool == true
            if (value is bool && (bool) value == true)
                return invert ? Visibility.Collapsed : Visibility.Visible;

            // Fall back to "visible".
            return invert ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}