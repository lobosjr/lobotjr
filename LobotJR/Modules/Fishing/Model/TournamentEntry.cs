using LobotJR.Data;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// An entry in a fishing tournament.
    /// </summary>
    public class TournamentEntry : TableObject
    {
        /// <summary>
        /// The id of the user this entry is for.
        /// </summary>
        public string UserId { get; set; }  //This needs DB migration to not wipe the data.
        /// <summary>
        /// The total points the user scored.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Foreign key id for the result set this entry is in.
        /// </summary>
        public int ResultId { get; set; }
        /// <summary>
        /// The tournament result set this entry is in.
        /// </summary>
        public virtual TournamentResult Result { get; set; }

        /// <summary>
        /// Creates an empty tournament entry. Necessary for Entity Framework
        /// since there is a non-empty constructor.
        /// </summary>
        public TournamentEntry()
        {

        }

        /// <summary>
        /// Creates a tournament entry with a username a point value.
        /// </summary>
        /// <param name="userId">The user id this entry represents.</param>
        /// <param name="points">The points this user scored in the tournament.</param>
        public TournamentEntry(string userId, int points)
        {
            UserId = userId;
            Points = points;
        }
    }
}
