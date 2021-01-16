namespace LobotJR.Command
{
    /// <summary>
    /// Represents a request from a user to execute a command.
    /// </summary>
    public class CommandRequest
    {
        /// <summary>
        /// The twitch id of the user making the request.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The id of the command to execute.
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// The string the user sent to trigger the command.
        /// </summary>
        public string CommandString { get; set; }
        /// <summary>
        /// Any arguments or other data sent along with the command request.
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// Whether or not the user issued the command with the compact switch (-c).
        /// </summary>
        public bool IsCompact { get; set; }

        /// <summary>
        /// Parses a command request from a user message.
        /// </summary>
        /// <param name="message">The message sent by the user.</param>
        /// <param name="userId">The twitch id of the user.</param>
        /// <returns></returns>
        public static CommandRequest Parse(string message, string userId)
        {
            var output = new CommandRequest();
            output.UserId = userId;
            var space = message.IndexOf(' ');
            if (space != -1)
            {
                output.CommandString = message.Substring(0, space);
                output.Data = message.Substring(space + 1);
                output.IsCompact = output.Data.StartsWith("-c");
                if (output.IsCompact)
                {
                    output.Data = output.Data.Substring(2).TrimStart();
                }
            }
            else
            {
                output.CommandString = message;
            }
            return output;
        }
    }
}
