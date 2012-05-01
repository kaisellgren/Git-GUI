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
        // Light colors.
        static SolidColorBrush COLOR_RED = new SolidColorBrush(Color.FromRgb(220, 104, 107));
        static SolidColorBrush COLOR_GREEN = new SolidColorBrush(Color.FromRgb(122, 183, 64));
        static SolidColorBrush COLOR_BLUE = new SolidColorBrush(Color.FromRgb(81, 140, 220));
        static SolidColorBrush COLOR_GRAY = new SolidColorBrush(Color.FromRgb(142, 142, 142));

        // Dark colors.
        static SolidColorBrush COLOR_DARK_RED = new SolidColorBrush(Color.FromRgb(167, 44, 47));
        static SolidColorBrush COLOR_DARK_GREEN = new SolidColorBrush(Color.FromRgb(62, 123, 04));
        static SolidColorBrush COLOR_DARK_BLUE = new SolidColorBrush(Color.FromRgb(21, 80, 160));
        static SolidColorBrush COLOR_DARK_GRAY = new SolidColorBrush(Color.FromRgb(82, 82, 82));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LibGit2Sharp.FileStatus status = (LibGit2Sharp.FileStatus) value;

            // Whether to use darker colors (for text).
            bool darkerColors = (string) parameter == "dark";

            // Removed & Missing = red.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Removed) || status.HasFlag(LibGit2Sharp.FileStatus.Missing))
                return darkerColors ? COLOR_DARK_RED : COLOR_RED;

            // Added = green.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Added))
                return darkerColors ? COLOR_DARK_GREEN : COLOR_GREEN;

            // Modified = blue.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Modified) || status.HasFlag(LibGit2Sharp.FileStatus.Staged))
                return darkerColors ? COLOR_DARK_BLUE : COLOR_BLUE;

            // Untracked = gray.
            if (status.HasFlag(LibGit2Sharp.FileStatus.Untracked))
                return darkerColors ? COLOR_DARK_GRAY : COLOR_GRAY;

            throw new Exception("Could not convert status " + status.ToString() + " to a SolidColorBrush!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LibGit2Sharp.FileStatus.Removed;
        }
    }
}
