using LobotJR.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Holds the results of a fishing tournament, including entries for each
    /// user and their score.
    /// </summary>
    public class TournamentResult : TableObject
    {
        /// <summary>
        /// Datetime the tournament ended.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;
        /// <summary>
        /// Collection of entries for each user that participated.
        /// </summary>
        public virtual List<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();
        /// <summary>
        /// The entry for the winner of the tournament.
        /// </summary>
        public TournamentEntry Winner
        {
            get
            {
                return Entries.FirstOrDefault();
            }
        }

        /// <summary>
        /// Create an empty tournament result.
        /// </summary>
        public TournamentResult()
        {
        }

        /// <summary>
        /// Create a tournament result with all entries.
        /// </summary>
        /// <param name="entries">The entries for each user that participated
        /// in the tournament.</param>
        public TournamentResult(IEnumerable<TournamentEntry> entries)
        {
            Entries = entries?.OrderByDescending(x => x.Points).ToList();
        }

        /// <summary>
        /// Create a tournament result with all entries.
        /// </summary>
        /// <param name="entries">The entries for each user that
        /// in the tournament.</param>
        public TournamentResult(DateTime date, IEnumerable<TournamentEntry> entries)
        {
            Date = date;
            Entries = entries?.OrderByDescending(x => x.Points).ToList();
        }

        /// <summary>
        /// Gets the entry for a participant by username.
        /// </summary>
        /// <param name="name">The username of the participant to retrieve.</param>
        /// <returns>A tournament entry for the user, or null if they did not
        /// participate.</returns>
        public TournamentEntry GetEntryByName(string name)
        {
            return Entries.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the rank of a user in the tournament by username.
        /// </summary>
        /// <param name="name">The username of the participant to retrieve.</param>
        /// <returns>A 1-based value indicating the users placement.</returns>
        public int GetRankByName(string name)
        {
            return Entries.IndexOf(Entries.FirstOrDefault(x => x.Name.Equals(name))) + 1;
        }
    }
}
