using System.Collections.Generic;

namespace LobotJR.Command
{
    public delegate CommandResult CommandExecutor(string data, string user);
    public delegate IDictionary<string, string> CompactExecutor(string data, string user);

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
        /// Delegate that executes the command, and provides the strings to
        /// return to the executing user.
        /// </summary>
        public CommandExecutor Executor { get; set; }

        /// <summary>
        /// Delegate that execute the command in compact mode, which provides a
        /// response in the form of a collection of key/value pairs.
        /// </summary>
        public CompactExecutor CompactExecutor { get; set; }

        /// <summary>
        /// Creates a new command handler.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="executor">A delegate to use to execute the command.</param>
        /// <param name="commandStrings">The strings that be used to trigger the command.</param>
        public CommandHandler(string name, CommandExecutor executor, params string[] commandStrings)
        {
            Name = name;
            Executor = executor;
            CommandStrings = commandStrings;
        }

        /// <summary>
        /// Creates a new command handler.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="executor">A delegate to use to execute the command.</param>
        /// <param name="compactExecutor">A delegate to use to execute the command in compact mode.</param>
        /// <param name="commandStrings">The strings that be used to trigger the command.</param>
        public CommandHandler(string name, CommandExecutor executor, CompactExecutor compactExecutor, params string[] commandStrings)
        {
            Name = name;
            Executor = executor;
            CompactExecutor = compactExecutor;
            CommandStrings = commandStrings;
        }
    }
}
