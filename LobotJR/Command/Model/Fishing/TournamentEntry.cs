using LobotJR.Data;
using System.ComponentModel.DataAnnotations;

namespace LobotJR.Command.Model.Fishing
{
    /// <summary>
    /// An entry in a fishing tournament.
    /// </summary>
    public class TournamentEntry : TableObject
    {
        /// <summary>
        /// The id of the user this entry is for.
        /// </summary>
        [Required]
        public string UserId { get; set; }
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

        public override bool Equals(object obj)
        {
            var other = obj as TournamentEntry;
            return other != null && other.UserId == UserId && other.ResultId == ResultId;
        }

        public override int GetHashCode()
        {
            var prime1 = 108301;
            var prime2 = 150151;
            unchecked
            {
                var hash = prime1; // random big prime number
                hash = (hash * prime2) ^ UserId.GetHashCode();
                hash = (hash * prime2) ^ ResultId.GetHashCode();
                return hash;
            }
        }
    }
}
