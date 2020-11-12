﻿using LobotJR.Modules;
using System.Collections.Generic;

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
        /// List of ids for registered commands.
        /// </summary>
        IEnumerable<string> Commands { get; }

        /// <summary>
        /// Initialize the command manager, loading role data and registering
        /// the commands.
        /// </summary>
        /// <param name="broadcastUser">The name of the channel the bot is in.</param>
        /// <param name="chatUser">The name of the bot user.</param>
        void Initialize(string broadcastUser, string chatUser);
        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        void LoadAllModules();
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
        /// <param name="responses">The response messages to send to the user.</param>
        /// <returns>Whether a command was found and executed.</returns>
        bool ProcessMessage(string message, string user, out IEnumerable<string> responses);
        /// <summary>
        /// Save role data to the disk.
        /// </summary>
        void UpdateRoles();
    }
}
