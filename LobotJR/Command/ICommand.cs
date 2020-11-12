using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Command
{
    /// <summary>
    /// Represents a command the bot can execute in response to a message from
    /// a user.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The strings that can be used to issue the command.
        /// </summary>
        IEnumerable<string> CommandStrings { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="data">Any data passed by the user after the command
        /// string.</param>
        /// <param name="user">The name of the user executing the command.</param>
        /// <returns>The response to send to the user issuing the command.</returns>
        IEnumerable<string> Execute(string data, string user);
    }
}
