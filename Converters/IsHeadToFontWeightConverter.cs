using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GG.Converters
{
    /// <summary>
    /// Converts IsHead status into a font weight.
    /// </summary>
    class IsHeadToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}