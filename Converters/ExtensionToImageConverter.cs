using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GG.Libraries;

namespace GG.Converters
{
    /// <summary>
    /// Converts a file extension to an image source.
    /// </summary>
    class ExtensionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IconUtil.GetIcon((string) value, true, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}