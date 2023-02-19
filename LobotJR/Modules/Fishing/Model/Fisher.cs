using System;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Represents a user's fishing data.
    /// </summary>
    public class Fisher
    {
        /// <summary>
        /// The twitch id for the user.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Whether the user has their line out to try and catch a fish.
        /// </summary>
        public bool IsFishing { get; set; }
        /// <summary>
        /// The fish they have hooked, or null if nothing is on the line.
        /// </summary>
        public Fish Hooked { get; set; }
        /// <summary>
        /// The time of their last catch.
        /// </summary>
        public DateTime? CatchTime { get; set; }
        /// <summary>
        /// The time they hooked their current fish.
        /// </summary>
        public DateTime? HookedTime { get; set; }
    }
}
