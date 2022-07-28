using LobotJR.Data;
using System.ComponentModel.DataAnnotations;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Data that describes an entry on the global leaderboard.
    /// </summary>
    public class LeaderboardEntry : TableObject
    {
        /// <summary>
        /// The user that caught the fish.
        /// </summary>
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// The foreign key id for the fish that was caught.
        /// </summary>
        [Required]
        public int FishId { get; set; }
        /// <summary>
        /// The fish that was caught.
        /// </summary>
        public virtual Fish Fish { get; set; }
        /// <summary>
        /// The length of the fish.
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// The weight of the fish.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Copies the record values from another leaderboard entry into this one.
        /// </summary>
        /// <param name="other">Another leaderboard entry.</param>
        public void CopyFrom(LeaderboardEntry other)
        {
            UserId = other.UserId;
            Length = other.Length;
            Weight = other.Weight;
        }
    }
}
