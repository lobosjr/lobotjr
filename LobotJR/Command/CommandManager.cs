﻿using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules;
using LobotJR.Modules.AccessControl;
using LobotJR.Modules.Fishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wolfcoins;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private const int MessageLimit = 450;

        private readonly Dictionary<string, string> commandStringToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CommandExecutor> commandIdToExecutorMap = new Dictionary<string, CommandExecutor>();
        private readonly Dictionary<string, AnonymousExecutor> anonymousIdToExecutorMap = new Dictionary<string, AnonymousExecutor>();
        private readonly Dictionary<string, CompactExecutor> compactIdToExecutorMap = new Dictionary<string, CompactExecutor>();
        private readonly Dictionary<string, Regex> commandStringRegexMap = new Dictionary<string, Regex>();

        /// <summary>
        /// Event raised when a module sends a push notification.
        /// </summary>
        public event PushNotificationHandler PushNotifications;

        /// <summary>
        /// Repository manager for access to stored data types.
        /// </summary>
        public IRepositoryManager RepositoryManager { get; set; }
        /// <summary>
        /// User lookup service used to translate between usernames and user ids.
        /// </summary>
        public UserLookup UserLookup { get; private set; }
        /// <summary>
        /// List of ids for registered commands.
        /// </summary>
        public IEnumerable<string> Commands
        {
            get
            {
                return commandIdToExecutorMap.Keys
                    .Union(compactIdToExecutorMap.Keys)
                    .Union(anonymousIdToExecutorMap.Keys)
                    .ToArray();
            }
        }

        private void AddCommand(CommandHandler command, string prefix)
        {
            var commandId = $"{prefix}.{command.Name}";
            if (command.AnonymousExecutor != null)
            {
                anonymousIdToExecutorMap.Add(commandId, command.AnonymousExecutor);
            }
            if (command.Executor != null)
            {
                commandIdToExecutorMap.Add(commandId, command.Executor);
            }
            if (command.CompactExecutor != null)
            {
                compactIdToExecutorMap.Add(commandId, command.CompactExecutor);
            }
            var exceptions = new List<Exception>();
            foreach (var commandString in command.CommandStrings)
            {
                if (commandStringToIdMap.ContainsKey(commandString))
                {
                    exceptions.Add(new Exception($"{commandId}: The command string \"{commandString}\" has already been registered by {commandStringToIdMap[commandString]}."));
                }
                else
                {
                    commandStringToIdMap.Add(commandString, commandId);
                }
            }
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private void AddModule(ICommandModule module, string prefix = null)
        {
            if (prefix != null)
            {
                prefix = $"{prefix}.{module.Name}";
            }
            else
            {
                prefix = module.Name;
            }

            module.PushNotification += Module_PushNotification;

            var exceptions = new List<Exception>();
            foreach (var command in module.Commands)
            {
                try
                {
                    AddCommand(command, prefix);
                }
                catch (AggregateException e)
                {
                    exceptions.Add(e);
                }
            }

            if (module.SubModules != null)
            {
                foreach (var subModule in module.SubModules)
                {
                    subModule.PushNotification += Module_PushNotification;
                    try
                    {
                        AddModule(subModule, prefix);
                    }
                    catch (AggregateException ae)
                    {
                        exceptions.AddRange(ae.InnerExceptions);
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Failed to load module", exceptions);
            }
        }

        private void Module_PushNotification(string userId, CommandResult commandResult)
        {
            PushNotifications?.Invoke(userId, commandResult);
        }

        private bool CanUserExecute(string commandId, string userId)
        {
            var roles = RepositoryManager.UserRoles.Read().Where(x => x.CoversCommand(commandId));
            return !roles.Any() || roles.Any(x => x.UserIds.Contains(userId));
        }

        private bool CanExecuteAnonymously(string commandId)
        {
            var roles = RepositoryManager.UserRoles.Read().Where(x => x.CoversCommand(commandId));
            return !roles.Any() && anonymousIdToExecutorMap.ContainsKey(commandId);
        }

        public CommandManager(IRepositoryManager repositoryManager, UserLookup userLookup)
        {
            RepositoryManager = repositoryManager;
            UserLookup = userLookup;
        }

        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        /// <param name="systemManager">System manager containing all loaded systems.</param>
        public void LoadAllModules(ISystemManager systemManager, Currency wolfcoins)
        {
            LoadModules(new AccessControlModule(this),
                new FishingModule(UserLookup, systemManager.Get<FishingSystem>(), RepositoryManager.TournamentResults, wolfcoins.coinList));
        }

        /// <summary>
        /// Loads all registered command modules.
        /// <param name="modules">An array of modules to load.</param>
        /// </summary>
        public void LoadModules(params ICommandModule[] modules)
        {
            var exceptions = new List<Exception>();
            foreach (var module in modules)
            {
                try
                {
                    AddModule(module);
                }
                catch (AggregateException e)
                {
                    exceptions.Add(e);
                }
            }
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Checks if a command id exists or is a valid wildcard pattern.
        /// </summary>
        /// <param name="commandId">The command id to validate.</param>
        /// <returns>Whether or not the command id is valid.</returns>
        public bool IsValidCommand(string commandId)
        {
            var index = commandId.IndexOf('*');
            if (index >= 0)
            {
                if (!commandStringRegexMap.ContainsKey(commandId))
                {
                    var commandString = commandId.Replace(".", "\\.").Replace("*", ".*");
                    commandStringRegexMap.Add(commandId, new Regex($"^{commandString}$"));
                }
                var commandRegex = commandStringRegexMap[commandId];
                return Commands.Any(x => commandRegex.IsMatch(x));
            }
            return Commands.Any(x => x.Equals(commandId));
        }

        private CommandResult PrepareCompactResponse(CommandRequest request, ICompactResponse response)
        {
            var entries = response.ToCompact();
            var prefix = $"{request.CommandString}: ";
            var toSend = prefix;
            var responses = new List<string>();
            foreach (var entry in entries)
            {
                if (toSend.Length + entry.Length > MessageLimit)
                {
                    responses.Add(toSend);
                    toSend = prefix;
                }
                toSend += entry;
            }
            responses.Add(toSend);
            return new CommandResult(responses.ToArray());
        }

        private CommandResult TryExecuteCommand(CommandRequest request)
        {
            try
            {
                if (request.IsCompact)
                {
                    if (compactIdToExecutorMap.TryGetValue(request.CommandId, out var compactExecutor))
                    {
                        var compactResponse = compactExecutor.Invoke(request.Data, request.UserId);
                        if (compactResponse != null)
                        {
                            return PrepareCompactResponse(request, compactResponse);
                        }
                        return new CommandResult($"Command requested produced no results.");
                    }
                    else
                    {
                        return new CommandResult($"Command {request.CommandId} does not support compact mode.");
                    }
                }
                else
                {
                    if (commandIdToExecutorMap.TryGetValue(request.CommandId, out var executor))
                    {
                        return executor.Invoke(request.Data, request.UserId);
                    }
                    if (anonymousIdToExecutorMap.TryGetValue(request.CommandId, out var anonymousExecutor))
                    {
                        return anonymousExecutor.Invoke(request.Data);
                    }
                }
            }
            catch (Exception e)
            {
                return new CommandResult(true, new Exception[] { e });
            }
            return new CommandResult();
        }

        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        /// <returns>Whether a command was found and executed.</returns>
        public CommandResult ProcessMessage(string message, string user)
        {
            var request = CommandRequest.Parse(message);
            if (commandStringToIdMap.TryGetValue(request.CommandString, out var commandId))
            {
                request.CommandId = commandId;
                request.UserId = UserLookup.GetId(user);
                if (request.UserId == null && !CanExecuteAnonymously(request.CommandId))
                {
                    return new CommandResult("User ID not found in cache, please try again in a few minutes. "
                        + "If you continue to see this error, please let the streamer or a mod know.");
                }
                if (CanUserExecute(request.CommandId, request.UserId))
                {
                    return TryExecuteCommand(request);
                }
                return new CommandResult(true, new Exception[] { new UnauthorizedAccessException($"User \"{user}\" attempted to execute unauthorized command \"{message}\"") });
            }
            return new CommandResult();
        }
    }
}
