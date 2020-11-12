using LobotJR.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.FeatureManagement
{
    /// <summary>
    /// Command to add a user role
    /// </summary>
    public class AddRole : ICommand
    {
        private ICommandManager commandManager;

        public string Name => "FeatureManagement.AddRole";
        public IEnumerable<string> CommandStrings => new String[] { "AddRole", "add-role" };

        AddRole(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        /// <summary>
        /// Adds a user role to the command system.
        /// </summary>
        /// <param name="data">The name of the role.</param>
        /// <param name="user">The user sending the command.</param>
        public IEnumerable<string> Execute(string data, string user)
        {
            var existingRole = this.commandManager.Roles.Where(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole != null)
            {
                return new string[] { $"Error: Unable to add role, a role with the name \"{data}\" already exists." };
            }

            this.commandManager.Roles.Add(new UserRole() { Name = data });
            this.commandManager.UpdateRoles();
            return new string[] { $"Role \"${data}\" added successfully!" };
        }
    }
}
