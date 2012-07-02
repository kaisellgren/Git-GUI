using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using GG.Libraries;

namespace GG.Converters
{
    /// <summary>
    /// Returns the height of the body (data grid actual height - column header height).
    /// </summary>
    class GetDataGridContentHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataGrid = (DataGrid) value;

            return dataGrid.ActualHeight - 24;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}