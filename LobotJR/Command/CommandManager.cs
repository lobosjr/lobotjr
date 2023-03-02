using Autofac;
using LobotJR.Command.Module;
using LobotJR.Command.Module.AccessControl;
using LobotJR.Data;
using LobotJR.Data.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public class CommandManager : ICommandManager
    {
        public static readonly char Prefix = '!';

        private const int MessageLimit = 450;

        private readonly Dictionary<string, string> commandStringToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CommandExecutor> commandIdToExecutorMap = new Dictionary<string, CommandExecutor>();
        private readonly Dictionary<string, AnonymousExecutor> anonymousIdToExecutorMap = new Dictionary<string, AnonymousExecutor>();
        private readonly Dictionary<string, CompactExecutor> compactIdToExecutorMap = new Dictionary<string, CompactExecutor>();
        private readonly List<string> whisperOnlyCommands = new List<string>();
        private readonly Dictionary<string, Regex> commandStringRegexMap = new Dictionary<string, Regex>();

        /// <summary>
        /// Event raised when a module sends a push notification.
        /// </summary>
        public event PushNotificationHandler PushNotifications;

        /// <summary>
        /// Command modules to be loaded.
        /// </summary>
        public IEnumerable<ICommandModule> CommandModules { get; private set; }
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
            if (command.WhisperOnly)
            {
                whisperOnlyCommands.Add(commandId);
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

        private void AddModule(ICommandModule module)
        {
            //This is a bad hack to get it working quickly, need a better way to provide back access
            //Create an access control system that can take the command manager as a parameter to get proper access
            if (module is AccessControlAdmin)
            {
                (module as AccessControlAdmin).CommandManager = this;
            }

            module.PushNotification += Module_PushNotification;

            var exceptions = new List<Exception>();
            foreach (var command in module.Commands)
            {
                try
                {
                    AddCommand(command, module.Name);
                }
                catch (AggregateException e)
                {
                    exceptions.Add(e);
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

        private bool CanExecuteInChat(string commandId)
        {
            return !whisperOnlyCommands.Contains(commandId);
        }

        public CommandManager(IEnumerable<ICommandModule> modules, IRepositoryManager repositoryManager, UserLookup userLookup)
        {
            CommandModules = modules;
            RepositoryManager = repositoryManager;
            UserLookup = userLookup;
        }

        /// <summary>
        /// Initializes all registered command modules.
        /// </summary>
        public void InitializeModules()
        {
            var exceptions = new List<Exception>();
            foreach (var module in CommandModules)
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
        /// <param name="isWhisper">Whether or not the message was sent as a whisper.</param>
        /// <returns>Whether a command was found and executed.</returns>
        public CommandResult ProcessMessage(string message, string user, bool isWhisper)
        {
            var request = CommandRequest.Parse(message);
            if (commandStringToIdMap.TryGetValue(request.CommandString, out var commandId))
            {
                if (!isWhisper && !CanExecuteInChat(commandId))
                {
                    return new CommandResult()
                    {
                        Processed = true,
                        TimeoutSender = true,
                        Responses = new string[]
                        {
                            "You just tried to use a command in chat that is only available by whispering me. Reply in this window on twitch or type '/w lobotjr' in chat to use that command.",
                            "Sorry for purging you. Just trying to do my job to keep chat clear! <3"
                        }
                    };
                }
                request.CommandId = commandId;
                request.UserId = UserLookup.GetId(user);
                if (request.UserId == null && !CanExecuteAnonymously(request.CommandId))
                {
                    return new CommandResult("It looks like we don't have your user ID. Give me some time to set up your character. "
                        + $"This can take up to {UserLookup.UpdateTime} minutes.");
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
