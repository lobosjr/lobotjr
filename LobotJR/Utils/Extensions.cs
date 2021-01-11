using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Returns a random floating-point number with a normal distribution.
        /// Of the generated values, 68% will be within one standard
        /// deviation of the mean, 95% will be within two, and 99.7% within
        /// three.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="mean">The mean or center of the distribution.</param>
        /// <param name="std">The standard deviation of the distribution.</param>
        /// <returns></returns>
        public static double NextNormal(this Random current, double mean, double std)
        {
            var u1 = current.NextDouble();
            var u2 = current.NextDouble();
            var stdNormal = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
            return mean + stdNormal * std;
        }

        /// <summary>
        /// Returns a random floating-point number with a standard normal
        /// distribution. This is a normal distribution with a mean of 0 and a
        /// standard deviation of 1.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public static double NextNormal(this Random current)
        {
            return current.NextNormal(0, 1);
        }

        /// <summary>
        /// Returns a random integer between min and max, inclusive, with a
        /// normal distribution. This allows for normal distributions of items
        /// in a list, using a standard deviation of 1/3 the range of values and
        /// mapping everything beyond 3 standard deviations to the last item in
        /// the list.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="min">The lowest value possible.</param>
        /// <param name="max">The highest value possible.</param>
        /// <returns>A number between min and max, with lower numbers more
        /// likely according to a normal distribution.</returns>
        public static int NextNormalIndex(this Random current, int min, int max)
        {
            var roll = current.NextNormal(0, (double)(max - min) / 3d);
            return (int)Math.Min(Math.Floor(Math.Abs(roll)) + min, max);
        }

        /// <summary>
        /// Returns a random integer between zero and max, inclusive, with a
        /// normal distribution. This allows for normal distributions of items
        /// in a list, using a standard deviation of 1/3 the range of values and
        /// mapping everything beyond 3 standard deviations to the last item in
        /// the list.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max">The highest value possible.</param>
        /// <returns>A number between zero and max, with lower numbers more
        /// likely according to a normal distribution.</returns>
        public static int NextNormalIndex(this Random current, int max)
        {
            return current.NextNormalIndex(0, max);
        }

        /// <summary>
        /// Returns a random integer between that corresponds to the index of
        /// an entry in the weights list. The likelyhood of each entry to be
        /// selected corresponds to the percent of that value that makes up the
        /// sum of all values in the list.
        /// 
        /// For example, if the weights list contains two entries, one with a
        /// value of 2 and the other with a value of 1, the first entry will be
        /// twice as likely to be selected.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="weights">A list of floating-point numbers, where each
        /// item is the ratio of that index being selected compared to the
        /// other items in the list.</param>
        /// <returns>An integer that corresponds to the index of an item in the
        /// weights list.</returns>
        public static int WeightedRandom(this Random current, IList<double> weights)
        {
            var total = weights.Sum();
            var roll = current.NextDouble() * total;
            for (var i = 0; i < weights.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
