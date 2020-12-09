using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Utils;
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
            var result = TournamentResultsCompact(data, user);
            if (result == null)
            {
                return new CommandResult("No fishing tournaments have completed.");
            }
            var sinceEnded = DateTime.Now - result.Ended;
            var responses = new List<string>(new string[] { $"The most recent tournament ended {sinceEnded} ago." });
            if (result.Rank > 0)
            {
                if (result.Winner.Equals(user, StringComparison.OrdinalIgnoreCase))
                {
                    responses.Add($"You won the tournament with {result.WinnerPoints} points.");
                }
                else
                {
                    responses.Add($"The tournament was won by {result.Winner} with {result.WinnerPoints} points.");
                    responses.Add($"You placed {result.Rank.ToOrdinal()} with {result.UserPoints} points.");
                }
            }
            else
            {
                responses.Add($"The tournament was won by {result.Winner} with {result.WinnerPoints} points.");
            }
            return new CommandResult(responses.ToArray());
        }

        public TournamentResultsResponse TournamentResultsCompact(string data, string user)
        {
            var tournament = repository.Read().OrderByDescending(x => x.Date).FirstOrDefault();
            if (tournament != null)
            {
                var winner = tournament.Winner;
                var output = new TournamentResultsResponse()
                {
                    Ended = tournament.Date,
                    Winner = winner.Name,
                    WinnerPoints = winner.Points
                };
                var userEntry = tournament.GetEntryByName(user);
                if (userEntry != null)
                {
                    output.Rank = tournament.GetRankByName(userEntry.Name);
                    output.UserPoints = userEntry.Points;
                }
                return output;
            }
            return null;
        }

        public CommandResult TournamentRecords(string data, string user)
        {
            var records = TournamentRecordsCompact(data, user);
            if (records == null)
            {
                return new CommandResult("You have not entered any fishing tournaments.");
            }
            return new CommandResult($"Your highest score in a tournament was {records.TopScore} points, earning you {records.TopScoreRank.ToOrdinal()} place.",
                $"Your best tournament placement was {records.TopRank.ToOrdinal()} place, with {records.TopRankScore} points.");
        }

        public TournamentRecordsResponse TournamentRecordsCompact(string data, string user)
        {
            var output = new Dictionary<string, string>();
            var tournaments = repository.Read(x => x.GetEntryByName(user) != null);
            if (!tournaments.Any())
            {
                return null;
            }
            var topRank = tournaments.OrderBy(x => x.GetRankByName(user)).First();
            var topRankAndScore = tournaments.Where(x => x.GetRankByName(user) == topRank.GetRankByName(user)).OrderByDescending(x => x.GetEntryByName(user).Points).First();
            var topScore = tournaments.OrderByDescending(x => x.GetEntryByName(user).Points).First();
            var topScoreAndRank = tournaments.Where(x => x.GetEntryByName(user).Points == topScore.GetEntryByName(user).Points).OrderBy(x => x.GetRankByName(user)).First();
            return new TournamentRecordsResponse()
            {
                TopRank = topRankAndScore.GetRankByName(user),
                TopRankScore = topRankAndScore.GetEntryByName(user).Points,
                TopScore = topScoreAndRank.GetEntryByName(user).Points,
                TopScoreRank = topScoreAndRank.GetRankByName(user)
            };
        }
    }

    public class TournamentResultsResponse
    {
        public DateTime Ended { get; set; }
        public string Winner { get; set; }
        public int WinnerPoints { get; set; }
        public int Rank { get; set; }
        public int UserPoints { get; set; }
    }

    public class TournamentRecordsResponse
    {
        public int TopRank { get; set; }
        public int TopRankScore { get; set; }
        public int TopScore { get; set; }
        public int TopScoreRank { get; set; }
    }
}
