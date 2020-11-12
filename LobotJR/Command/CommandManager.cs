using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LobotJR.Command
{
    /// <summary>
    /// Manages the commands the bot can execute.
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private static string _roleDataPath = "content/role.data";
        private Dictionary<string, ICommand> commands;
        
        /// <summary>
        /// List of user roles.
        /// </summary>
        public List<UserRole> Roles { get; set; }

        private void AddCommand(ICommand command)
        {
            var commandStrings = command.GetCommandStrings();
            foreach (var commandString in commandStrings)
            {
                if (this.commands.ContainsKey(commandString))
                {
                    throw (new Exception($"Command {command.Name}: The command string \"{commandString}\" has already been registered by Command {this.commands[commandString].Name}."));
                }
                this.commands.Add(commandString, command);
            }
        }

        private bool CanUserExecute(ICommand command, string user)
        {
            return false;
        }

        /// <summary>
        /// Initialize the command manager, loading role data and registering
        /// the commands.
        /// </summary>
        public void Initialize()
        {
            this.Roles = JsonConvert.DeserializeObject<List<UserRole>>(File.ReadAllText(_roleDataPath));

            // this.AddCommand();
        }

        /// <summary>
        /// Save role data to the disk.
        /// </summary>
        public void UpdateRoles()
        {
            File.WriteAllText(_roleDataPath, JsonConvert.SerializeObject(this.Roles));
        }

        /// <summary>
        /// Processes a message from a user to check for and execute a command.
        /// </summary>
        /// <param name="message">The message the user sent.</param>
        /// <param name="user">The user's name.</param>
        public bool ProcessMessage(string message, string user)
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

            var command = this.commands[commandString];
            if (command != null)
            {
                if (this.CanUserExecute(command, user))
                {
                    command.Execute(data, user);
                    return true;
                }
            }
            return false;
        }
    }
}
