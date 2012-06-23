using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using GG.Libraries;

namespace GG.Converters
{
    /// <summary>
    /// Converts a DataGrid to its total height value.
    /// </summary>
    class GetDataGridContentHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataGrid = (DataGrid) value;

            // Find first child of type ScrollViewer in the grid.
            var scrollViewer = UIHelper.FindChild<ScrollViewer>(dataGrid);

            if (scrollViewer != null)
            {
                Console.WriteLine(scrollViewer.ViewportHeight);
                return scrollViewer.ViewportHeight;
            }
            else
            {
                Console.WriteLine("nope");
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}