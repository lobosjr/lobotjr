using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace LobotJR.Command
{
    /// <summary>
    /// Represents a user role, as well as the users who are members of it.
    /// </summary>
    public class UserRole
    {
        private static string ListToString(IEnumerable<string> collection)
        {
            return string.Join(",", collection.Select(x => x.Replace(",", "\\,")));
        }

        private static List<string> StringToList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }
            return Regex.Split(value, "(?<!\\\\),").Select(x => x.Replace("\\,", ",")).ToList();
        }

        public int Id { get; set; }
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A comma-delimited list of user ids.
        /// </summary>
        public string UserList { get; set; }
        /// <summary>
        /// A list of users who are members of the role.
        /// </summary>
        [NotMapped]
        public List<string> Users
        {
            get
            {
                return StringToList(UserList);
            }
            private set
            {
                UserList = ListToString(value);
            }
        }
        /// <summary>
        /// A comma-delimited list of commands.
        /// </summary>
        public string CommandList { get; set; }
        /// <summary>
        /// A list of commands that require role access to execute.
        /// </summary>
        [NotMapped]
        public List<string> Commands
        {
            get
            {
                return StringToList(CommandList);
            }
            private set
            {
                CommandList = ListToString(value);
            }
        }


        /// <summary>
        /// Creates an empty user role.
        /// </summary>
        public UserRole()
        {
        }

        /// <summary>
        /// Creates a user role with a name.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        public UserRole(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a user role with a name.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        public UserRole(string name, IEnumerable<string> users, IEnumerable<string> commands)
        {
            Name = name;
            if (users != null)
            {
                Users = new List<string>(users);
            }
            else
            {
                Users = new List<string>();
            }
            if (commands != null)
            {
                Commands = new List<string>(commands);
            }
            else
            {
                Commands = new List<string>();
            }
        }

        /// <summary>
        /// Adds a user to this role.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        public void AddUser(string id)
        {
            var temp = Users;
            temp.Add(id);
            Users = temp;
        }

        /// <summary>
        /// Removes a user from this role.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        public void RemoveUser(string id)
        {
            var temp = Users;
            temp.Remove(id);
            Users = temp;
        }

        /// <summary>
        /// Adds a command to this role.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        public void AddCommand(string command)
        {
            var temp = Commands;
            temp.Add(command);
            Commands = temp;
        }
        /// <summary>
        /// Removes a command from this role.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        public void RemoveCommand(string command)
        {
            var temp = Commands;
            temp.Remove(command);
            Commands = temp;
        }

        /// <summary>
        /// Checks a command to see if it's covered by this role.
        /// </summary>
        /// <param name="commandId">The id of the command.</param>
        /// <returns>Whether or not the command is covered.</returns>
        public bool CoversCommand(string commandId)
        {
            return Commands.Any((command) =>
            {
                var index = command.IndexOf('*');
                if (index >= 0)
                {
                    return commandId.StartsWith(command.Substring(0, index));
                }
                return command.Equals(commandId);
            });
        }
    }
}
