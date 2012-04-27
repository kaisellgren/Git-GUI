using System;
using System.Globalization;
using System.Windows.Data;

namespace GG.Converters
{
    /// <summary>
    /// Converts LibGit2Sharp.Status into a single character string.
    /// </summary>
    class StatusToCharacterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LibGit2Sharp.FileStatus status = (LibGit2Sharp.FileStatus) value;

            if (status.HasFlag(LibGit2Sharp.FileStatus.Removed) || status.HasFlag(LibGit2Sharp.FileStatus.Missing))
                return "D";

            if (status.HasFlag(LibGit2Sharp.FileStatus.Added))
                return "A";

            if (status.HasFlag(LibGit2Sharp.FileStatus.Staged) || status.HasFlag(LibGit2Sharp.FileStatus.Modified))
                return "M";

            if (status.HasFlag(LibGit2Sharp.FileStatus.Untracked))
                return "?";

            throw new Exception("Could not convert status " + status.ToString() + " to a character!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LibGit2Sharp.FileStatus.Removed;
        }
    }
}