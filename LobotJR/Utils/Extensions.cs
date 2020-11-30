using System;

namespace LobotJR.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="value">The substring to check for.</param>
        /// <param name="comparison">The string comparison method.</param>
        /// <returns>true if the value parameter occurs within the string, or if value is the empty string (""); otherwise, false.</returns>
        public static bool Contains(this string current, string value, StringComparison comparison)
        {
            return current.IndexOf(value, comparison) != -1;
        }

        /// <summary>
        /// Returns the ordinal form of an integer.
        /// </summary>
        /// <param name="current"></param>
        /// <returns>A string containing the number plus its ordinal suffix.</returns>
        public static string ToOrdinal(this int current)
        {
            if (current <= 0)
                return current.ToString();

            var tens = current % 100;
            if (tens >= 11 && tens <= 13)
            {
                return $"{current}th";
            }

            var ones = current % 10;
            switch (ones)
            {
                case 1:
                    return $"{current}st";
                case 2:
                    return $"{current}nd";
                case 3:
                    return $"{current}rd";
                default:
                    return $"{current}th";
            }
        }
    }
}
