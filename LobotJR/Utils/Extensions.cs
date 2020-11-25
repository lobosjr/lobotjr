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
    }
}
