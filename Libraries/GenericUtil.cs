using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG.Libraries
{
    public static class GenericUtil
    {
        /// <summary>
        /// Converts a string to a hex string.
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";

            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint) System.Convert.ToUInt32(tmp.ToString()));
            }

            return hex;
        }
    }
}