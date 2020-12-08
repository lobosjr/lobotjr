using LobotJR.Data;
using LobotJR.Modules;
using LobotJR.Modules.AccessControl;
using LobotJR.Modules.Fishing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public class CommandManager : ICommandManager
    {

        private Dictionary<string, string> commandStringToIdMap;
        private Dictionary<string, CommandExecutor> commandIdToExecutorMap;
        private Dictionary<string, CompactExecutor> compactIdToExecutorMap;

        /// <summary>
        /// Repository manager for access to stored data types.
        /// </summary>
        public IRepositoryManager RepositoryManager { get; set; }
        /// <summary>
        /// List of ids for registered commands.
        /// </summary>
        public IEnumerable<string> Commands { get { return commandIdToExecutorMap.Keys.ToArray(); } }

        private void AddCommand(CommandHandler command, string prefix)
        {
            var commandId = $"{prefix}.{command.Name}";
            commandIdToExecutorMap.Add(commandId, command.Executor);
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

        private bool CanUserExecute(string commandId, string user)
        {
            var roles = RepositoryManager.UserRoles.Read().Where(x => x.CoversCommand(commandId));
            return !roles.Any() || roles.Any(x => x.Users.Contains(user));
        }

        public CommandManager(IRepositoryManager repositoryManager)
        {
            RepositoryManager = repositoryManager;
        }

        /// <summary>
        /// Initialize the command manager, loading role data and registering
        /// the commands.
        /// </summary>
        public void Initialize(string broadcastUser, string chatUser)
        {
            commandStringToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            commandIdToExecutorMap = new Dictionary<string, CommandExecutor>();
            compactIdToExecutorMap = new Dictionary<string, CompactExecutor>();
        }

        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        public void LoadAllModules()
        {
            var context = new SqliteContext();
            LoadModules(new AccessControlModule(this),
                new FishingModule(RepositoryManager.TournamentResults));
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
                return Commands.Any(x => x.StartsWith(commandId.Substring(0, index)));
            }
            return commandIdToExecutorMap.ContainsKey(commandId);
        }

        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        /// <param name="responses">The response messages to send to the user.</param>
        /// <returns>Whether a command was found and executed.</returns>
        public CommandResult ProcessMessage(string message, string user)
        {
            var space = message.IndexOf(' ');
            string commandString;
            string data = null;
            var compact = false;
            if (space != -1)
            {
                commandString = message.Substring(0, space);
                data = message.Substring(space + 1);
                compact = data.StartsWith("-c");
                if (compact)
                {
                    data = data.Substring(2).TrimStart();
                }
            }
            else
            {
                commandString = message;
            }

            if (commandStringToIdMap.TryGetValue(commandString, out var commandId))
            {
                if (CanUserExecute(commandId, user))
                {
                    try
                    {
                        if (compact)
                        {
                            if (compactIdToExecutorMap.TryGetValue(commandId, out var compactExecutor))
                            {
                                var compactResponse = compactExecutor.Invoke(data, user);
                                if (compactResponse != null)
                                {
                                    return new CommandResult(JsonConvert.SerializeObject(compactResponse));
                                }
                            }
                            else
                            {
                                return new CommandResult($"Command {commandId} does not support compact mode");
                            }
                        }
                        else
                        {
                            var executor = commandIdToExecutorMap[commandId];
                            var response = executor.Invoke(data, user);
                            if (response != null)
                            {
                                return response;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return new CommandResult(true, null, new Exception[] { e });
                    }
                }
                else
                {
                    return new CommandResult(true, null, new Exception[] { new UnauthorizedAccessException($"User \"{user}\" attempted to execute unauthorized command \"{message}\"") });
                }
            }
            return new CommandResult();
        }
    }
}
