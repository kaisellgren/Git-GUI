using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GG.Converters
{
    /// <summary>
    /// Converts LibGit2Sharp.Status into a SolidColorBrush to be used in background / text.
    /// </summary>
    class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LibGit2Sharp.FileStatus status = (LibGit2Sharp.FileStatus) value;

            // Removed & Missing = red.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Removed) || status.HasFlag(LibGit2Sharp.FileStatus.Missing))
                return new SolidColorBrush(Color.FromRgb(220, 104, 107));

            // Added = green.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Added))
                return new SolidColorBrush(Color.FromRgb(122, 183, 64));

            // Modified = blue.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Modified) || status.HasFlag(LibGit2Sharp.FileStatus.Staged))
                return new SolidColorBrush(Color.FromRgb(81, 140, 220));

            // Untracked = gray.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Untracked))
                return new SolidColorBrush(Color.FromRgb(142, 142, 142));

            throw new Exception("Could not convert status " + status.ToString() + " to a SolidColorBrush!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LibGit2Sharp.FileStatus.Removed;
        }
    }
}
