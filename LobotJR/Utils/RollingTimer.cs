using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;

namespace LobotJR.Utils
{
    /// <summary>
    /// A timer that tracks occurrences in a rolling window.
    /// </summary>
    public class RollingTimer
    {
        private List<DateTime> Hits = new List<DateTime>();
        private TimeSpan TimerPeriod;
        public int MaxHits { get; set; }

        /// <summary>
        /// Creates a timer to track occurrences in a rolling window.
        /// </summary>
        /// <param name="timerPeriod">The period of the window.</param>
        /// <param name="maxHits">The maximum number of occurrences allowed in the timer period.</param>
        public RollingTimer(TimeSpan timerPeriod, int maxHits)
        {
            TimerPeriod = timerPeriod;
            MaxHits = maxHits;
        }

        /// <summary>
        /// Clears all occurrences in the timer.
        /// </summary>
        public void Reset()
        {
            Hits.Clear();
        }

        public int CurrentHitCount()
        {
            return Hits.Count;
        }

        /// <summary>
        /// Adds an occurrence to the timer.
        /// </summary>
        /// <param name="occurrence">The time of the occurrence.</param>
        public void AddOccurrence(DateTime occurrence)
        {
            Hits.Add(occurrence);
        }

        /// <summary>
        /// Gets the number of occurrences currently available in the timer window.
        /// </summary>
        /// <returns>The number of occurrences that can be added before the threshold is reached.</returns>
        public int AvailableOccurrences()
        {
            var now = DateTime.Now;
            var threshold = now - TimerPeriod;
            var toRemove = Hits.Where(x => x < threshold);
            Hits = Hits.Except(toRemove).ToList();
            return MaxHits - Hits.Count;
        }
    }
}
