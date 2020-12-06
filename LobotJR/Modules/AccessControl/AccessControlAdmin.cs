using LobotJR.Command;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules
{
    /// <summary>
    /// Module of access control admin commands.
    /// </summary>
    public class AccessControlAdmin : ICommandModule
    {
        private ICommandManager commandManager;
        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Admin";

        /// <summary>
        /// A collection of commands for managing access to commands.
        /// </summary>
        public IEnumerable<CommandHandler> Commands { get; private set; }

        /// <summary>
        /// Null response to indicate this module has no sub modules.
        /// </summary>
        public IEnumerable<ICommandModule> SubModules => null;

        public AccessControlAdmin(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
            Commands = new CommandHandler[]
            {
                new CommandHandler("ListRoles", ListRoles, "ListRoles", "list-roles"),
                new CommandHandler("CreateRole", CreateRole, "CreateRole", "create-role"),
                new CommandHandler("DescribeRole", DescribeRole, "DescribeRole", "describe-role"),
                new CommandHandler("DeleteRole", DeleteRole, "DeleteRole", "delete-role"),

                new CommandHandler("EnrollUser", AddUserToRole, "EnrollUser", "enroll-user"),
                new CommandHandler("UnenrollUser", RemoveUserFromRole, "UnenrollUser", "unenroll-user"),

                new CommandHandler("RestrictCommand", AddCommandToRole, "RestrictCommand", "restrict-command"),
                new CommandHandler("ListCommands", ListCommands, "ListCommands", "list-commands"),
                new CommandHandler("UnrestrictCommand", RemoveCommandFromRole, "UnrestrictCommand", "unrestrict-command")
            };
        }

        private CommandResult ListRoles(string data, string user)
        {
            return new CommandResult($"There are {commandManager.Roles.Read().Count()} roles: {string.Join(", ", commandManager.Roles.Read().Select(x => x.Name))}");
        }

        private CommandResult CreateRole(string data, string user)
        {
            var existingRole = commandManager.Roles.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole != null)
            {
                return new CommandResult($"Error: Unable to create role, \"{data}\" already exists.");
            }

            commandManager.Roles.Create(new UserRole(data));
            commandManager.Roles.Commit();
            return new CommandResult($"Role \"{data}\" created successfully!");
        }

        private CommandResult DescribeRole(string data, string user)
        {
            var existingRole = commandManager.Roles.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new CommandResult($"Error: Role \"{data}\" not found.");
            }

            return new CommandResult(
                $"Role \"{data}\" contains the following commands: {string.Join(", ", existingRole.Commands)}",
                $"Role \"{data}\" contains the following users: {string.Join(", ", existingRole.Users)}"
            );
        }

        private CommandResult DeleteRole(string data, string user)
        {
            var existingRole = commandManager.Roles.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new CommandResult($"Error: Unable to delete role, \"{data}\" does not exist.");
            }

            if (existingRole.Commands.Count > 0)
            {
                return new CommandResult($"Error: Unable to delete role, please remove all commands first.");
            }

            commandManager.Roles.Delete(existingRole);
            commandManager.Roles.Commit();
            return new CommandResult($"Role \"{data}\" deleted successfully!");
        }

        private CommandResult AddUserToRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new CommandResult("Error: Invalid number of parameters. Expected parameters: {username} {role name}.");
            }

            var userToAdd = data.Substring(0, space);
            if (userToAdd.Length == 0)
            {
                return new CommandResult("Error: Username cannot be empty.");
            }
            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }

            var role = commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: No role with name \"{roleName}\" was found.");
            }

            if (role.Users.Contains(userToAdd))
            {
                return new CommandResult($"Error: User \"{userToAdd}\" is already a member of \"{roleName}\"");
            }
            role.AddUser(userToAdd);
            commandManager.Roles.Update(role);
            commandManager.Roles.Commit();

            return new CommandResult($"User \"{userToAdd}\" was added to role \"{role.Name}\" successfully!");
        }

        private CommandResult RemoveUserFromRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new CommandResult("Error: Invalid number of parameters. Expected parameters: {username} {role name}.");
            }

            var userToRemove = data.Substring(0, space);
            if (userToRemove.Length == 0)
            {
                return new CommandResult("Error: Username cannot be empty.");
            }
            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }

            var role = commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: No role with name \"{roleName}\" was found.");
            }

            if (!role.Users.Contains(userToRemove))
            {
                return new CommandResult($"Error: User \"{userToRemove}\" is not a member of \"{roleName}\".");
            }
            role.RemoveUser(userToRemove);
            commandManager.Roles.Update(role);
            commandManager.Roles.Commit();

            return new CommandResult($"User \"{userToRemove}\" was removed from role \"{role.Name}\" successfully!");
        }

        private CommandResult AddCommandToRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new CommandResult("Error: Invalid number of parameters. Expected parameters: {command name} {role name}.");
            }

            var commandName = data.Substring(0, space);
            if (commandName.Length == 0)
            {
                return new CommandResult("Error: Command name cannot be empty.");
            }
            if (!commandManager.IsValidCommand(commandName))
            {
                return new CommandResult($"Error: Command {commandName} does not match any commands.");
            }


            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }
            var role = commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" already has access to \"{commandName}\".");
            }

            role.AddCommand(commandName);
            commandManager.Roles.Update(role);
            commandManager.Roles.Commit();

            return new CommandResult($"Command \"{commandName}\" was added to the role \"{role.Name}\" successfully!");
        }

        private CommandResult ListCommands(string data, string user)
        {
            var commands = commandManager.Commands;
            var modules = commands.Where(x => x.IndexOf('.') != -1).Select(x => x.Substring(0, x.IndexOf('.'))).Distinct().ToList();
            var response = new string[modules.Count + 1];
            response[0] = $"There are {commands.Count()} commands across {modules.Count} modules.";
            for (var i = 0; i < modules.Count; i++)
            {
                response[i + 1] = $"{modules[i]}: {string.Join(", ", commands.Where(x => x.StartsWith(modules[i])))}";
            }
            return new CommandResult(response);
        }

        private CommandResult RemoveCommandFromRole(string data, string user)
        {
            var space = data.IndexOf(' ');
            if (space == -1)
            {
                return new CommandResult("Error: Invalid number of parameters. Expected paremeters: {command name} {role name}.");
            }

            var commandName = data.Substring(0, space);
            if (commandName.Length == 0)
            {
                return new CommandResult("Error: Command name cannot be empty.");
            }
            if (!commandManager.IsValidCommand(commandName))
            {
                return new CommandResult($"Error: Command {commandName} does not match any commands.");
            }

            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }
            var role = commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (!role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" doesn't have access to \"{commandName}\".");
            }

            role.RemoveCommand(commandName);
            commandManager.Roles.Update(role);
            commandManager.Roles.Commit();

            return new CommandResult($"Command \"{commandName}\" was removed from role \"{role.Name}\" successfully!");
        }
    }
}
