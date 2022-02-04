using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Debex.Convert.Extensions
{
    public static class String
    {
        public static bool IsNullOrEmpty(this string val) => string.IsNullOrWhiteSpace(val);
        public static bool NotNullOrEmpty(this string val) => !string.IsNullOrWhiteSpace(val);

        public static int ToInt(this string str, int defaultValue = -1) => int.TryParse(str, out var i) ? i : defaultValue;
        public static float ToFloat(this string str, float defaultValue = float.MinValue) => float.TryParse(str, out var r) ? r : defaultValue;
        public static decimal ToDecimal(this string str, decimal defaultValue = decimal.MinValue) => decimal.TryParse(str, 
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var d) ? d : defaultValue;

        public static string JoinToString(this IEnumerable<char> val, string separator = null) => string.Join(separator, val);
        public static DateTime? TryParseDate(this string dt, params string[] format)
        {
            return DateTime.TryParseExact(dt, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var r) ? r : null;
        }

        public static string FirstLetterUpper(this string str)
        {
            if (str.IsNullOrEmpty()) return str;

            var r = str[0];
            return $"{r.ToString().ToUpperInvariant()}{str.Substring(1)}";
        }

        public static bool ContainsOneOf(this string str, params string[] check) => check.Any(str.Contains);
        public static bool IsOneOf(this string str, params string[] checks) => checks.Any(x => x == str);

        public static bool StartsWithOneOf(this string str, params string[] checks) =>
            str.NotNullOrEmpty() &&
            checks.Any(x => str.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
    }
}
