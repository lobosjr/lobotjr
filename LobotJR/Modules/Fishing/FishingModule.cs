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
        private IDatabaseContext context;

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

            context = SqliteContext.Instance;
            // context.FishingTournaments.Add(new TournamentResult(new TournamentEntry[] { new TournamentEntry() { Name = "Tester", Points = 100 } }));
            // context.FishingTournaments.Add(new TournamentResult(new TournamentEntry[] { new TournamentEntry() { Name = "Bester", Points = 1000 } }));
            // context.SaveChanges();

            foreach (var tournament in context.FishingTournaments)
            {
                Console.WriteLine($"Fishing tournament {tournament.Id} on {tournament.Date} with {tournament.Entries.Count} entrants. Winner: {tournament.Winner.Name} with {tournament.Winner.Points}");
            }
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
            var role = this.commandManager.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                return new CommandResult($"Error: Role \"{roleName}\" does not exist.");
            }

            if (!role.Commands.Contains(commandName))
            {
                return new CommandResult($"Error: \"{roleName}\" doesn't have access to \"{commandName}\".");
            }

            role.Commands.Remove(commandName);
            this.commandManager.UpdateRoles();

            return new CommandResult($"Command \"{commandName}\" was removed from role \"{role.Name}\" successfully!");
        }
    }
}
