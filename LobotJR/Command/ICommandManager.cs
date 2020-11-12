using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// List of user roles.
        /// </summary>
        List<UserRole> Roles { get; }

        /// <summary>
        /// Initialize the command manager, loading role data and registering
        /// the commands.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Save role data to the disk.
        /// </summary>
        void UpdateRoles();
        
        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        bool ProcessMessage(string message, string user);

    }
}
