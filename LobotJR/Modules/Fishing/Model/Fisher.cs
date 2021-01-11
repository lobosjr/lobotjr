using LobotJR.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Represents a user's fishing data.
    /// </summary>
    public class Fisher : TableObject
    {
        /// <summary>
        /// The display name for the user.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Collection of record catches for this user.
        /// </summary>
        public List<Catch> Records { get; set; }
        /// <summary>
        /// Whether the user has their line out to try and catch a fish.
        /// </summary>
        public bool IsFishing { get; set; }
        /// <summary>
        /// The fish they have hooked, or null if nothing is on the line.
        /// </summary>
        [NotMapped]
        public Fish Hooked { get; set; }
        /// <summary>
        /// The time of their last catch.
        /// </summary>
        [NotMapped]
        public DateTime? CatchTime { get; set; }
        /// <summary>
        /// The time they hooked their current fish.
        /// </summary>
        [NotMapped]
        public DateTime? HookedTime { get; set; }
    }
}
