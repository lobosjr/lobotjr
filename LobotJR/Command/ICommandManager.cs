using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules;
using System.Collections.Generic;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// Event raised when a module sends a push notification.
        /// </summary>
        event PushNotificationHandler PushNotifications;

        /// <summary>
        /// Repository manager for access to data.
        /// </summary>
        IRepositoryManager RepositoryManager { get; }
        /// <summary>
        /// User lookup service used to translate between usernames and user ids.
        /// </summary>
        UserLookup UserLookup { get; }
        /// <summary>
        /// List of ids for registered commands.
        /// </summary>
        IEnumerable<string> Commands { get; }

        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        /// <param name="systemManager">System manager containing all loaded systems.</param>
        void LoadAllModules(ISystemManager systemManager);
        /// <summary>
        /// Loads all registered command modules.
        /// <param name="modules">An array of modules to load.</param>
        /// </summary>
        void LoadModules(params ICommandModule[] modules);
        /// <summary>
        /// Checks if a command id exists or is a valid wildcard pattern.
        /// </summary>
        /// <param name="commandId">The command id to validate.</param>
        /// <returns>Whether or not the command id is valid.</returns>
        bool IsValidCommand(string commandId);
        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        /// <returns>An object containing the results of the attempt to process the message.</returns>
        CommandResult ProcessMessage(string message, string user);
    }
}
