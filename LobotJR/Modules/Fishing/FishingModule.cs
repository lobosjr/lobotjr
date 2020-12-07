using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Utils;
using System;
using System.Collections.Generic;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class FishingModule : ICommandModule
    {
        private readonly IRepository<TournamentResult> repository;

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

        public FishingModule(IRepository<TournamentResult> repository)
        {
            this.repository = repository;
            Commands = new CommandHandler[]
            {
                new CommandHandler("TournamentResults", TournamentResults, TournamentResultsCompact, "TournamentResults", "tournament-results"),
                new CommandHandler("TournamentRecords", TournamentRecords, TournamentRecordsCompact, "TournamentRecords", "tournament-records")
            };
        }

        public CommandResult TournamentResults(string data, string user)
        {
            var values = TournamentResultsCompact(data, user);
            var sinceEnded = DateTime.Now - DateTime.Parse(values["ended"]);
            var responses = new List<string>(new string[] { $"The most recent tournament ended {sinceEnded} ago.",
                $"The tournament was won by {values["winner"]} with {values["score"]} points." });
            if (values.ContainsKey("userScore"))
            {
                var rank = int.Parse(values["userRank"]).ToOrdinal();
                responses.Add($"You placed {rank} with {values["userScore"]}.");
            }
            return new CommandResult(responses.ToArray());
        }

        public Dictionary<string, string> TournamentResultsCompact(string data, string user)
        {
            var output = new Dictionary<string, string>();
            output.Add("ended", DateTime.Now.ToString());
            output.Add("winner", "arfafax");
            output.Add("score", 2000.ToString());
            if (true)   // check if the user was a participant in the fishing tournament
            {
                output.Add("userScore", 500.ToString());
                output.Add("userRank", 2.ToString());
            }
            return output;
        }

        public CommandResult TournamentRecords(string data, string user)
        {
            var values = TournamentRecordsCompact(data, user);
            var topRank = int.Parse(values["topRank"]);
            if (topRank == -1)
            {
                return new CommandResult("You have not entered any fishing tournaments.");
            }
            var topScoreRank = int.Parse(values["topScoreRank"]);
            return new CommandResult($"Your highest score in a tournament was {values["topScore"]} points, earning you {topScoreRank.ToOrdinal()} place.",
                $"Your highest rank in a tournament was {topRank.ToOrdinal()}, with {values["topRankScore"]} points.");
        }

        public Dictionary<string, string> TournamentRecordsCompact(string data, string user)
        {
            var output = new Dictionary<string, string>();
            if (false)  //if the user has never entered a tournament
            {
                output.Add("topRank", "-1");
                return output;
            }
            output.Add("topRank", "1");
            output.Add("topRankScore", "1000");
            output.Add("topScore", "2000");
            output.Add("topScoreRank", "3");
            return output;
        }
    }
}
