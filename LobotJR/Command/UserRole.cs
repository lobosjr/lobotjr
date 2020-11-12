using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command
{
    /// <summary>
    /// Represents a user role, as well as the users who are members of it.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A list of users who are members of the role.
        /// </summary>
        public List<string> Users { get; set; } = new List<string>();
        /// <summary>
        /// A list of commands that require role access to execute.
        /// </summary>
        public List<string> Commands { get; set; } = new List<string>();

        /// <summary>
        /// Creates a user role with a name.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        public UserRole(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Checks a command to see if it's covered by this role.
        /// </summary>
        /// <param name="commandId">The id of the command.</param>
        /// <returns>Whether or not the command is covered.</returns>
        public bool CoversCommand(string commandId)
        {
            return this.Commands.Any((command) =>
            {
                var index = command.IndexOf('*');
                if (index >= 0)
                {
                    return commandId.StartsWith(command.Substring(0, index));
                }
                return command.Equals(commandId);
            });
        }
    }
}
