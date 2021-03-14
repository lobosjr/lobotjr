using LobotJR.Data;
using LobotJR.Modules;
using System.Collections.Generic;
using Wolfcoins;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// Repository manager for access to data.
        /// </summary>
        IRepositoryManager RepositoryManager { get; }
        /// <summary>
        /// List of ids for registered commands.
        /// </summary>
        IEnumerable<string> Commands { get; }

        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        /// <param name="systemManager">System manager containing all loaded systems.</param>
        /// <param name="wolfcoins">Holds data about wolfcoins in legacy format.</param>
        void LoadAllModules(ISystemManager systemManager, Currency wolfcoins);
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
