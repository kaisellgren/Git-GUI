using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GG.Models;

namespace GG.Converters
{
    /// <summary>
    /// Converts LibGit2Sharp.Status into a single character string.
    /// </summary>
    class RecentCommitMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RecentCommitMessage message = (RecentCommitMessage)value;
            return message.CroppedMessage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
