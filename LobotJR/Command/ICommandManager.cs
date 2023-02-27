using LobotJR.Command.Module;
using LobotJR.Data;
using LobotJR.Data.User;
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
        /// Initializes all registered command modules.
        /// </summary>
        void InitializeModules();
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
        /// <param name="isWhisper">Whether or not the message was sent as a whisper.</param>
        /// <returns>An object containing the results of the attempt to process the message.</returns>
        CommandResult ProcessMessage(string message, string user, bool isWhisper);
    }
}
