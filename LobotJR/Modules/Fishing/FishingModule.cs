using LobotJR.Command;
using LobotJR.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class FishingModule : ICommandModule
    {
        private ICommandManager commandManager;
        private IRepository<TournamentResult> context;

        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Fishing";

        /// <summary>
        /// A collection of commands for managing access to commands.
        /// </summary>
        public IEnumerable<CommandHandler> Commands { get; private set; }

        /// <summary>
        /// Null response to indicate this module has no sub modules.
        /// </summary>
        public IEnumerable<ICommandModule> SubModules => null;

        public FishingModule(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
            this.Commands = new CommandHandler[]
            {
                new CommandHandler("TournamentResults", this.TournamentResults, "TournamentResults", "tournament-results"),
                new CommandHandler("TournamentRecords", this.TournamentRecords, "TournamentRecords", "tournament-records")
            };
        }

        public CommandResult TournamentResults(string data, string user)
        {

            return null;
        }

        public CommandResult TournamentRecords(string data, string user)
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
            if (!this.commandManager.IsValidCommand(commandName))
            {
                return new CommandResult($"Error: Command ${commandName} does not match any commands.");
            }

            var roleName = data.Substring(space + 1);
            if (roleName.Length == 0)
            {
                return new CommandResult("Error: Role name cannot be empty.");
            }
            var role = this.commandManager.Roles.Read(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (!role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" doesn't have access to \"{commandName}\".");
            }

            role.Commands.Remove(commandName);


            return new CommandResult($"Command \"{commandName}\" was removed from role \"{role.Name}\" successfully!");
        }
    }
}
