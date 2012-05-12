using System;
using System.IO;

namespace GG.Libraries
{
    class FileUtil
    {
        /// <summary>
        /// Returns a human formatted file size of the given file.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        public static string GetFormattedFileSize(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
                return "--";

            double bytes = new System.IO.FileInfo(fileFullPath).Length;

            string[] suffixes = { "B", "KB", "MB", "GB" };
            int order = 0;

            while (bytes >= 1024 && order + 1 < suffixes.Length)
            {
                order++;
                bytes = bytes / 1024;
            }

            return string.Format("{0:0.##} {1}", bytes, suffixes[order]);
        }

        /// <summary>
        /// Tells whether the given file is a binary file based on if there are any null bytes.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        public static bool IsBinaryFile(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
                return false;

            byte[] fileBytes = File.ReadAllBytes(fileFullPath);

            bool hadNullByte = false;

            foreach (byte b in fileBytes)
            {
                if (b == (byte) 0)
                {
                    hadNullByte = true;
                    break;
                }
            }

            return hadNullByte;
        }
    }
}
