using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JS.Tools
{
    public static class StringExtensions
    {
        private static string Truncate(this string source, int maxlength)
        {
            if (source != null && source.Length > maxlength)
            {
                source = source.Substring(0, maxlength);
            }
            return source;
        }

        public static string ToUpperIgnoreNull(this string input)
        {
            return input != null ? input.ToUpper(CultureInfo.InvariantCulture) : null;
        }

        public static string TrimIgnoreNull(this string input)
        {
            return input != null ? input.Trim() : null;
        }

        /// <summary>
        /// Use the current culture info
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                return cultureInfo.TextInfo.ToTitleCase(value.ToLower());
            }
        }

        /// <summary>
        /// Overload method with the specified culture info
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return cultureInfo.TextInfo.ToTitleCase(value.ToLower());
            }
        }

        public static string ToLowerIgnoreNull(this string input)
        {
            return input != null ? input.ToLower(CultureInfo.CurrentCulture) : null;
        }


    }
}
