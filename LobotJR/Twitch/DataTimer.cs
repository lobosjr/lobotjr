using System;
using System.ComponentModel.DataAnnotations;

namespace LobotJR.Twitch
{
    /// <summary>
    /// Class used to store KVP timers in the database.
    /// </summary>
    public class DataTimer
    {
        /// <summary>
        /// The key of the timer.
        /// </summary>
        [Key]
        public string Name { get; set; }
        /// <summary>
        /// The datetime of the occurrence of the timer.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
