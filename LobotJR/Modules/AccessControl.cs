using LobotJR.Command;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class AccessControl : ICommandModule
    {
        private ICommandManager commandManager;
        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "AccessControl";

        /// <summary>
        /// A collection of commands for managing access to commands.
        /// </summary>
        public IEnumerable<CommandHandler> Commands { get; private set; }

        /// <summary>
        /// Null response to indicate this module has no sub modules.
        /// </summary>
        public IEnumerable<ICommandModule> SubModules => null;

        public AccessControl(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
            this.Commands = new CommandHandler[]
            {
                new CommandHandler("ListRoles", this.ListRoles, "ListRoles", "list-roles"),
                new CommandHandler("CreateRole", this.CreateRole, "CreateRole", "create-role"),
                new CommandHandler("DescribeRole", this.DescribeRole, "DescribeRole", "describe-role"),
                new CommandHandler("DeleteRole", this.DeleteRole, "DeleteRole", "delete-role"),

                new CommandHandler("EnrollUser", this.AddUserToRole, "EnrollUser", "enroll-user"),
                new CommandHandler("CheckAccess", this.CheckAccess, "CheckAccess", "check-access"),
                new CommandHandler("UnenrollUser", this.RemoveUserFromRole, "UnenrollUser", "unenroll-user"),

                new CommandHandler("RestrictCommand", this.AddCommandToRole, "RestrictCommand", "restrict-command"),
                new CommandHandler("ListCommands", this.ListCommands, "ListCommands", "list-commands"),
                new CommandHandler("UnrestrictCommand", this.RemoveCommandFromRole, "UnrestrictCommand", "unrestrict-command")
            };
        }

        private IEnumerable<string> ListRoles(string data, string user)
        {
            return new string[] { $"There are {this.commandManager.Roles.Count} roles: ${string.Join(", ", this.commandManager.Roles.Select(x => x.Name))}" };
        }

        private IEnumerable<string> CreateRole(string data, string user)
        {
            var existingRole = this.commandManager.Roles.Where(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole != null)
            {
                return new string[] { $"Error: Unable to create role, \"{data}\" already exists." };
            }

            this.commandManager.Roles.Add(new UserRole(data));
            this.commandManager.UpdateRoles();
            return new string[] { $"Role \"${data}\" created successfully!" };
        }

        private IEnumerable<string> DescribeRole(string data, string user)
        {
            var existingRole = this.commandManager.Roles.Where(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new string[] { $"Error: Role \"{data}\" not found." };
            }

            return new string[] {
                $"Role \"${data}\" contains the following commands: {string.Join(", ", existingRole.Commands)}",
                $"Role \"${data}\" contains the following users: {string.Join(", ", existingRole.Users)}"
            };
        }

        private IEnumerable<string> DeleteRole(string data, string user)
        {
            var existingRole = this.commandManager.Roles.Where(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new string[] { $"Error: Unable to delete role, \"{data}\" does not exist." };
            }

            if (existingRole.Commands.Count > 0)
            {
                return new string[] { $"Error: Unable to delete role, please remove all commands first." };
            }

            this.commandManager.Roles.Remove(existingRole);
            this.commandManager.UpdateRoles();
            return new string[] { $"Role \"${data}\" deleted successfully!" };
        }

        private IEnumerable<string> AddUserToRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new string[] { "Error: Invalid number of parameters. Expected parameters: {username} {role name}." };
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

            if (role.Users.Contains(userToAdd))
            {
                return new string[] { $"Error: User \"{userToAdd}\" is already a member of \"{roleName}\"" };
            }
            role.Users.Add(userToAdd);
            this.commandManager.UpdateRoles();

            return new string[] { $"User \"{userToAdd}\" was added to role \"{role.Name}\" successfully!" };
        }

        private IEnumerable<string> CheckAccess(string data, string user)
        {
            var roleName = data;
            if (roleName.Length == 0)
            {
                var roles = this.commandManager.Roles.Where(x => x.Users.Any(y => y.Equals(user, StringComparison.OrdinalIgnoreCase)));
                if (roles.Any())
                {
                    var count = roles.Count();
                    return new string[] { $"You are a member of the following role{(count == 1 ? "" : "s")}: { string.Join(", ", roles.Select(x => x.Name)) }." };
                }
                else
                {
                    return new string[] { "You are not a member of any roles." };
                }
            }

            var role = this.commandManager.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new string[] { $"Error: No role with name \"{roleName}\" was found." };
            }

            var access = role.Users.Contains(user) ? "are" : "are not";
            return new string[] { $"You {access} a member of \"{role.Name}\"!" };
        }

        private IEnumerable<string> RemoveUserFromRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new string[] { "Error: Invalid number of parameters. Expected parameters: {username} {role name}." };
            }

            var userToRemove = data.Substring(0, space);
            if (userToRemove.Length == 0)
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

            if (!role.Users.Contains(userToRemove))
            {
                return new string[] { $"Error: User \"{userToRemove}\" is not a member of \"{roleName}\"." };
            }
            role.Users.Remove(userToRemove);
            this.commandManager.UpdateRoles();

            return new string[] { $"User \"{userToRemove}\" was removed from role \"{role.Name}\" successfully!" };
        }

        public IEnumerable<string> AddCommandToRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new string[] { "Error: Invalid number of parameters. Expected parameters: {command name} {role name}." };
            }

            var commandName = data.Substring(0, space);
            if (commandName.Length == 0)
            {
                return new string[] { "Error: Command name cannot be empty." };
            }
            if (!this.commandManager.IsValidCommand(commandName))
            {
                return new string[] { $"Error: Command ${commandName} does not match any commands." };
            }


            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new string[] { "Error: Role name cannot be empty." };
            }
            var role = this.commandManager.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new string[] { $"Error: Role \"{roleName}\" does not exist." };
            }

            if (role.Commands.Contains(commandName))
            {
                return new string[] { $"Error: \"{roleName}\" already has access to \"{commandName}\"." };
            }

            role.Commands.Add(commandName);
            this.commandManager.UpdateRoles();

            return new string[] { $"Command \"{commandName}\" was added to the role \"{role.Name}\" successfully!" };
        }

        private IEnumerable<string> ListCommands(string data, string user)
        {
            var commands = this.commandManager.Commands;
            var modules = commands.Where(x => x.IndexOf('.') != -1).Select(x => x.Substring(0, x.IndexOf('.'))).Distinct().ToList();
            var response = new string[modules.Count + 1];
            response[0] = $"There are {commands.Count()} commands across {modules.Count} modules.";
            for (var i = 0; i < modules.Count; i++)
            {
                response[i + 1] = $"{modules[i]}: {string.Join(", ", commands.Where(x => x.StartsWith(modules[i])))}";
            }
            return response;
        }

        public IEnumerable<string> RemoveCommandFromRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new string[] { "Error: Invalid number of parameters. Expected paremeters: {command name} {role name}." };
            }

            var commandName = data.Substring(0, space);
            if (commandName.Length == 0)
            {
                return new string[] { "Error: Command name cannot be empty." };
            }
            if (!this.commandManager.IsValidCommand(commandName))
            {
                return new string[] { $"Error: Command ${commandName} does not match any commands." };
            }

            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new string[] { "Error: Role name cannot be empty." };
            }
            var role = this.commandManager.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new string[] { $"Error: Role \"{roleName}\" does not exist." };
            }

            if (!role.Commands.Contains(commandName))
            {
                return new string[] { $"Error: \"{roleName}\" doesn't have access to \"{commandName}\"." };
            }

            role.Commands.Remove(commandName);
            this.commandManager.UpdateRoles();

            return new string[] { $"Command \"{commandName}\" was removed from role \"{role.Name}\" successfully!" };
        }
    }
}
