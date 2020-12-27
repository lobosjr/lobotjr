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

        private static string generateCommonString(int amount, string unit)
        {
            var value = unit;
            if (amount > 1)
            {
                value += "s";
            }
            return $"{amount} {value}";
        }

        /// <summary>
        /// Gets the value of a timespan in the way a person would naturally
        /// express it.
        /// </summary>
        /// <param name="current"></param>
        /// <returns>A string expressing the amount of the greatest unit of
        /// time that the timespan covers.</returns>
        public static string ToCommonString(this TimeSpan current)
        {
            var hours = (int)Math.Floor(current.TotalHours);
            if (hours > 0)
            {
                return generateCommonString(hours, "hour");
            }
            var minutes = (int)Math.Floor(current.TotalMinutes);
            if (minutes > 0)
            {
                return generateCommonString(minutes, "minute");
            }
            var seconds = (int)Math.Floor(current.TotalSeconds);
            return generateCommonString(seconds, "second");
        }
    }
}
