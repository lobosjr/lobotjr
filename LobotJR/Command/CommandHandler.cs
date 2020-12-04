using System.Collections.Generic;

namespace LobotJR.Command
{
    public delegate CommandResult CommandExecutor(string data, string user);

    /// <summary>
    /// Represents a command the bot can execute in response to a message from
    /// a user.
    /// </summary>
    public class CommandHandler
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The strings that can be used to issue the command.
        /// </summary>
        public IEnumerable<string> CommandStrings { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="data">Any data passed by the user after the command
        /// string.</param>
        /// <param name="user">The name of the user executing the command.</param>
        /// <returns>The response to send to the user issuing the command.</returns>
        public CommandExecutor Executor { get; set; }

        /// <summary>
        /// Creates a new command handler.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="executor"></param>
        /// <param name="commandStrings"></param>
        public CommandHandler(string name, CommandExecutor executor, params string[] commandStrings)
        {
            Name = name;
            Executor = executor;
            CommandStrings = commandStrings;
        }
    }
}
