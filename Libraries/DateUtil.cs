using System;

namespace GG.Libraries
{
    public class DateUtil
    {
        /// <summary>
        /// Converts a DateTime object into a relative time string.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetRelativeDate(DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            if (delta < 2 * MINUTE)
            {
                return "One minute ago";
            }

            if (delta < 45 * MINUTE)
            {
                return ts.Minutes + " minutes ago";
            }

            if (delta < 90 * MINUTE)
            {
                return "An hour ago";
            }

            if (delta < 48 * HOUR)
            {
                return ts.Hours + " hours ago";
            }

            if (delta < 7 * DAY)
            {
                return ts.Days + " days ago";
            }

            return dt.ToShortDateString() + ' ' + dt.ToShortTimeString();
        }
    }
}
