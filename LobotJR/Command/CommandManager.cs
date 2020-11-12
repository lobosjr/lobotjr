using LobotJR.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private static string _roleDataPath = "content/role.data";

        private Action<string, string> writeDataAction;
        private Func<string, string> readDataAction;
        private Func<string, bool> checkDataAction;
        private Dictionary<string, string> commandStringToIdMap;
        private Dictionary<string, CommandExecutor> commandIdToExecutorMap;

        /// <summary>
        /// List of user roles.
        /// </summary>
        public List<UserRole> Roles { get; set; }
        /// <summary>
        /// List of ids for registered commands.
        /// </summary>
        public IEnumerable<string> Commands { get { return this.commandIdToExecutorMap.Keys.ToArray(); } }

        private void AddCommand(CommandHandler command, string prefix)
        {
            var commandId = $"{prefix}.{command.Name}";
            this.commandIdToExecutorMap.Add(commandId, command.Executor);
            foreach (var commandString in command.CommandStrings)
            {
                if (this.commandStringToIdMap.ContainsKey(commandString))
                {
                    throw (new Exception($"{commandId}: The command string \"{commandString}\" has already been registered by {this.commandStringToIdMap[commandString]}."));
                }
                this.commandStringToIdMap.Add(commandString, commandId);
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

            foreach (var command in module.Commands)
            {
                this.AddCommand(command, prefix);
            }

            if (module.SubModules != null)
            {
                foreach (var subModule in module.SubModules)
                {
                    this.AddModule(subModule, prefix);
                }
            }
        }

        private bool CanUserExecute(string commandId, string user)
        {
            var roles = this.Roles.Where(x => x.CoversCommand(commandId));
            return !roles.Any() || roles.Any(x => x.Users.Contains(user));
        }

        public CommandManager()
        {
            this.readDataAction = File.ReadAllText;
            this.writeDataAction = File.WriteAllText;
            this.checkDataAction = File.Exists;
        }

        public CommandManager(Func<string, string> readDataAction, Action<string, string> writeDataAction, Func<string, bool> checkDataAction)
        {
            this.readDataAction = readDataAction;
            this.writeDataAction = writeDataAction;
            this.checkDataAction = checkDataAction;
        }

        /// <summary>
        /// Initialize the command manager, loading role data and registering
        /// the commands.
        /// </summary>
        public void Initialize(string broadcastUser, string chatUser)
        {
            if (!this.checkDataAction(_roleDataPath))
            {
                this.Roles = new List<UserRole>();
                this.Roles.Add(new UserRole()
                {
                    Name = "Streamer",
                    Commands = new List<string>(new string[] { "FeatureManagement.*" }),
                    Users = new List<string>(new string[] { broadcastUser, chatUser }),
                });
                this.UpdateRoles();
            }
            else
            {
                this.Roles = JsonConvert.DeserializeObject<List<UserRole>>(this.readDataAction(_roleDataPath));
            }

            this.commandStringToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.commandIdToExecutorMap = new Dictionary<string, CommandExecutor>();
        }

        /// <summary>
        /// Loads all registered command modules.
        /// </summary>
        public void LoadAllModules()
        {
            this.AddModule(new FeatureManagement(this));
        }

        /// <summary>
        /// Loads all registered command modules.
        /// <param name="modules">An array of modules to load.</param>
        /// </summary>
        public void LoadModules(params ICommandModule[] modules)
        {
            foreach (var module in modules)
            {
                this.AddModule(module);
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
                return this.Commands.Any(x => x.StartsWith(commandId.Substring(0, index)));
            }
            return this.commandIdToExecutorMap.ContainsKey(commandId);
        }

        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        /// <param name="responses">The response messages to send to the user.</param>
        /// <returns>Whether a command was found and executed.</returns>
        public bool ProcessMessage(string message, string user, out IEnumerable<string> responses)
        {
            var space = message.IndexOf(' ');
            string commandString;
            string data = null;
            if (space != -1)
            {
                commandString = message.Substring(0, space);
                data = message.Substring(space + 1);
            }
            else
            {
                commandString = message;
            }

            var commandId = this.commandStringToIdMap[commandString];
            if (commandId != null)
            {
                if (this.CanUserExecute(commandId, user))
                {
                    var executor = this.commandIdToExecutorMap[commandId];
                    responses = executor.Invoke(data, user);
                    return true;
                }
            }
            responses = null;
            return false;
        }

        /// <summary>
        /// Save role data to the disk.
        /// </summary>
        public void UpdateRoles()
        {
            this.writeDataAction(_roleDataPath, JsonConvert.SerializeObject(this.Roles));
        }
    }
}
