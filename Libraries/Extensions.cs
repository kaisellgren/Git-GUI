using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(stream);
            }
        }
    }
}
