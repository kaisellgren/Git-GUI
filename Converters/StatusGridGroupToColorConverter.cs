using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GG.Converters
{
    /// <summary>
    /// Converts LibGit2Sharp.Status into a single character string.
    /// </summary>
    class StatusGridGroupToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value) == "Staged" ? new SolidColorBrush(Color.FromRgb(76, 120, 0)) : new SolidColorBrush(Color.FromRgb(120, 37, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Brushes.White;
        }
    }
}