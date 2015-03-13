using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.App
{
    static class Utils
    {
        /// <summary>
        /// Builds C-compliant string (zero terminated)
        /// </summary>
        /// <param name="input">Input string to be formatted</param>
        /// <returns>C-compliant zero terminated char array</returns>
        public static char[] getCString(String input)
        {
            return (input + '\0').ToCharArray();
        }

        /// <summary>
        /// Converts color from ImageMagick format to ours
        /// </summary>
        /// <param name="number">input color-representing integer</param>
        /// <returns>converted ARGB color integer</returns>
        public static int convertColor(uint number)
        {
            // alpha channel is inverted
            uint alpha = 255 - ((number & 0xFF000000) >> 24);
            // remove old alpha and subtitute it with inverted one
            int c = (int)((number & 0x00FFFFFF) | (alpha << 24));
            return c;
        }
    }
}
