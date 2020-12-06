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
        public IEnumerable<ICommandModule> SubModules { get; private set; }

        public AccessControl(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
            this.Commands = new CommandHandler[]
            {
                new CommandHandler("CheckAccess", this.CheckAccess, "CheckAccess", "check-access"),
            };
            this.SubModules = new ICommandModule[] { new AccessControlAdmin(commandManager) };
        }

        private CommandResult CheckAccess(string data, string user)
        {
            var roleName = data;
            if (roleName.Length == 0)
            {
                var roles = this.commandManager.Roles.Read(x => x.Users.Any(y => y.Equals(user, StringComparison.OrdinalIgnoreCase)));
                if (roles.Any())
                {
                    var count = roles.Count();
                    return new CommandResult($"You are a member of the following role{(count == 1 ? "" : "s")}: { string.Join(", ", roles.Select(x => x.Name)) }.");
                }
                else
                {
                    return new CommandResult("You are not a member of any roles.");
                }
            }

            var role = this.commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: No role with name \"{roleName}\" was found.");
            }

            var access = role.Users.Contains(user) ? "are" : "are not";
            return new CommandResult($"You {access} a member of \"{role.Name}\"!");
        }
    }
}
