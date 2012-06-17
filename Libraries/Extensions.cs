using System;

namespace GG
{
    public static class Extensions
    {
        /// <summary>
        /// Crops the string to the specified length.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string str, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            if (length == 0 || str == null)
                return string.Empty;

            int len = str.Length;
            if (length >= len)
                return str;
            else
                return str.Substring(len - length, length);
        }

        /// <summary>
        /// Removes line breaks.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveLineBreaks(this string str)
        {
            return str.Replace("\n", "").Replace("\r", "");
        }
    }
}
