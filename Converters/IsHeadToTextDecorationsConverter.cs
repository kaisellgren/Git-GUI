using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GG.Converters
{
    /// <summary>
    /// Converts IsHead status into text decorations.
    /// </summary>
    class IsHeadToTextDecorationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? TextDecorations.Underline : new TextDecorationCollection();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}