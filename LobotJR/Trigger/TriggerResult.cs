using System.Collections.Generic;

namespace LobotJR.Trigger
{
    /// <summary>
    /// Holds the result of a positive match from a trigger.
    /// </summary>
    public class TriggerResult
    {
        /// <summary>
        /// Whether or not the trigger was processed.
        /// </summary>
        public bool Processed { get; set; } = true;
        /// <summary>
        /// Whether or not to timeout the user that fired the trigger.
        /// </summary>
        public bool TimeoutSender { get; set; }
        /// <summary>
        /// The message to send to the user as part of the timeout.
        /// </summary>
        public string TimeoutMessage { get; set; }
        /// <summary>
        /// Whispers to send to the user that triggered the result.
        /// </summary>
        public IEnumerable<string> Whispers { get; set; }
        /// <summary>
        /// Messages to send to public chat.
        /// </summary>
        public IEnumerable<string> Messages { get; set; }
    }
}
