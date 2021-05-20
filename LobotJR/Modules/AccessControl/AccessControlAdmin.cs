using LobotJR.Command;
using LobotJR.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.AccessControl
{
    /// <summary>
    /// Module of access control admin commands.
    /// </summary>
    public class AccessControlAdmin : ICommandModule
    {
        private readonly ICommandManager commandManager;
        private readonly IRepository<UserRole> repository;

        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Admin";

        /// <summary>
        /// This module does not issue any push notifications.
        /// </summary>
        public event PushNotificationHandler PushNotification;

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
            repository = commandManager.RepositoryManager.UserRoles;
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

        private CommandResult ListRoles(string data)
        {
            return new CommandResult($"There are {repository.Read().Count()} roles: {string.Join(", ", repository.Read().Select(x => x.Name))}");
        }

        private CommandResult CreateRole(string data)
        {
            var existingRole = repository.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole != null)
            {
                return new CommandResult($"Error: Unable to create role, \"{data}\" already exists.");
            }

            repository.Create(new UserRole(data));
            repository.Commit();
            return new CommandResult($"Role \"{data}\" created successfully!");
        }

        private CommandResult DescribeRole(string data)
        {
            var existingRole = repository.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new CommandResult($"Error: Role \"{data}\" not found.");
            }

            return new CommandResult(
                $"Role \"{data}\" contains the following commands: {string.Join(", ", existingRole.Commands)}.",
                $"Role \"{data}\" contains the following users: {string.Join(", ", existingRole.UserIds)}."
            );
        }

        private CommandResult DeleteRole(string data)
        {
            var existingRole = repository.Read(x => x.Name.Equals(data)).FirstOrDefault();
            if (existingRole == null)
            {
                return new CommandResult($"Error: Unable to delete role, \"{data}\" does not exist.");
            }

            if (existingRole.Commands.Count > 0)
            {
                return new CommandResult($"Error: Unable to delete role, please remove all commands first.");
            }

            repository.Delete(existingRole);
            repository.Commit();
            return new CommandResult($"Role \"{data}\" deleted successfully!");
        }

        private CommandResult AddUserToRole(string data)
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
            var userId = commandManager.UserLookup.GetId(userToAdd);
            if (userId == null)
            {
                return new CommandResult("Error: User id not present in id cache, please try again in a few minutes.");
            }
            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }

            var role = repository.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: No role with name \"{roleName}\" was found.");
            }
            if (role.UserIds.Contains(userId))
            {
                return new CommandResult($"Error: User \"{userToAdd}\" is already a member of \"{roleName}\".");
            }
            role.AddUser(userId);
            repository.Update(role);
            repository.Commit();

            return new CommandResult($"User \"{userToAdd}\" was added to role \"{role.Name}\" successfully!");
        }

        private CommandResult RemoveUserFromRole(string data)
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
            var userId = commandManager.UserLookup.GetId(userToRemove);
            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }

            var role = repository.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: No role with name \"{roleName}\" was found.");
            }

            if (!role.UserIds.Contains(userId))
            {
                return new CommandResult($"Error: User \"{userToRemove}\" is not a member of \"{roleName}\".");
            }
            role.RemoveUser(userId);
            repository.Update(role);
            repository.Commit();

            return new CommandResult($"User \"{userToRemove}\" was removed from role \"{role.Name}\" successfully!");
        }

        private CommandResult AddCommandToRole(string data)
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
            var role = repository.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" already has access to \"{commandName}\".");
            }

            role.AddCommand(commandName);
            repository.Update(role);
            repository.Commit();

            return new CommandResult($"Command \"{commandName}\" was added to the role \"{role.Name}\" successfully!");
        }

        private CommandResult ListCommands(string data)
        {
            var commands = commandManager.Commands;
            var modules = commands.Where(x => x.LastIndexOf('.') != -1).Select(x => x.Substring(0, x.LastIndexOf('.'))).Distinct().ToList();
            var response = new string[modules.Count + 1];
            response[0] = $"There are {commands.Count()} commands across {modules.Count} modules.";
            for (var i = 0; i < modules.Count; i++)
            {
                response[i + 1] = $"{modules[i]}: {string.Join(", ", commands.Where(x => x.StartsWith(modules[i])))}";
            }
            return new CommandResult(response);
        }

        private CommandResult RemoveCommandFromRole(string data)
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
            var role = repository.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (!role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" doesn't have access to \"{commandName}\".");
            }

            role.RemoveCommand(commandName);
            repository.Update(role);
            repository.Commit();

            return new CommandResult($"Command \"{commandName}\" was removed from role \"{role.Name}\" successfully!");
        }
    }
}
