using LobotJR.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.FeatureManagement
{
    /// <summary>
    /// Command to grant a user access to a role.
    /// </summary>
    public class EntitleUser : ICommand
    {
        private ICommandManager commandManager;

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name => "FeatureManagement.EntitleUser";
        /// <summary>
        /// The strings that can be used to issue the command.
        /// </summary>
        public IEnumerable<string> CommandStrings => new string[] { "EntitleUser", "entitle-user" };

        EntitleUser(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        /// <summary>
        /// Grants a user access to a role.
        /// </summary>
        /// <param name="data">The name of the user and the name of the role.</param>
        /// <param name="user">The user sending the command.</param>
        public IEnumerable<string> Execute(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new string[] { "Error: Invalid number of parameters. Expected format: EntitleUser {username} {rolename}." };
            }

            var userToAdd = data.Substring(0, space);
            if (userToAdd.Length == 0)
            {
                return new string[] { "Error: Username cannot be empty." };
            }
            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new string[] { "Error: Role name cannot be empty." };
            }

            var role = this.commandManager.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new string[] { $"Error: No role with name \"{roleName}\" was found." };
            }

            role.Users.Add(userToAdd);
            this.commandManager.UpdateRoles();

            return new string[] { $"User \"{userToAdd}\" was added to role \"{roleName}\" successfully!" };
        }
    }
}
