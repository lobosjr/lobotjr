﻿using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class TournamentModule : ICommandModule
    {
        private readonly IRepository<TournamentResult> repository;
        private readonly TournamentSystem system;

        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Tournament";

        /// <summary>
        /// A collection of commands for managing access to commands.
        /// </summary>
        public IEnumerable<CommandHandler> Commands { get; private set; }

        /// <summary>
        /// Null response to indicate this module has no sub modules.
        /// </summary>
        public IEnumerable<ICommandModule> SubModules => null;

        public TournamentModule(TournamentSystem system, IRepository<TournamentResult> repository)
        {
            this.system = system;
            this.repository = repository;
            Commands = new CommandHandler[]
            {
                new CommandHandler("TournamentResults", TournamentResults, TournamentResultsCompact, "TournamentResults", "tournament-results"),
                new CommandHandler("TournamentRecords", TournamentRecords, TournamentRecordsCompact, "TournamentRecords", "tournament-records"),
                new CommandHandler("NextTournament", NextTournament, NextTournamentCompact, "NextTournament", "next-tournament")
            };
        }

        public CommandResult TournamentResults(string data, string userId)
        {
            var result = TournamentResultsCompact(data, userId);
            if (result == null)
            {
                return new CommandResult("No fishing tournaments have completed.");
            }
            var sinceEnded = DateTime.Now - result.Ended;
            var pluralized = "participant";
            if (result.Participants > 1)
            {
                pluralized += "s";
            }
            var responses = new List<string>(new string[] { $"The most recent tournament ended {sinceEnded.ToCommonString()} ago with {result.Participants} {pluralized}." });
            if (result.Rank > 0)
            {
                if (result.Winner.Equals(userId, StringComparison.OrdinalIgnoreCase))
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

        public TournamentResultsResponse TournamentResultsCompact(string data, string userId)
        {
            var tournament = repository.Read().OrderByDescending(x => x.Date).FirstOrDefault();
            if (tournament != null)
            {
                var winner = tournament.Winner;
                var output = new TournamentResultsResponse()
                {
                    Ended = tournament.Date,
                    Participants = tournament.Entries.Count,
                    Winner = winner.UserId,
                    WinnerPoints = winner.Points

                };
                var userEntry = tournament.GetEntryById(userId);
                if (userEntry != null)
                {
                    output.Rank = tournament.GetRankById(userEntry.UserId);
                    output.UserPoints = userEntry.Points;
                }
                return output;
            }
            return null;
        }

        public CommandResult TournamentRecords(string data, string userId)
        {
            var records = TournamentRecordsCompact(data, userId);
            if (records == null)
            {
                return new CommandResult("You have not entered any fishing tournaments.");
            }
            return new CommandResult($"Your highest score in a tournament was {records.TopScore} points, earning you {records.TopScoreRank.ToOrdinal()} place.",
                $"Your best tournament placement was {records.TopRank.ToOrdinal()} place, with {records.TopRankScore} points.");
        }

        public TournamentRecordsResponse TournamentRecordsCompact(string data, string userId)
        {
            var output = new Dictionary<string, string>();
            var tournaments = repository.Read(x => x.GetEntryById(userId) != null);
            if (!tournaments.Any())
            {
                return null;
            }
            var topRank = tournaments.OrderBy(x => x.GetRankById(userId)).First();
            var topRankAndScore = tournaments.Where(x => x.GetRankById(userId) == topRank.GetRankById(userId)).OrderByDescending(x => x.GetEntryById(userId).Points).First();
            var topScore = tournaments.OrderByDescending(x => x.GetEntryById(userId).Points).First();
            var topScoreAndRank = tournaments.Where(x => x.GetEntryById(userId).Points == topScore.GetEntryById(userId).Points).OrderBy(x => x.GetRankById(userId)).First();
            return new TournamentRecordsResponse()
            {
                TopRank = topRankAndScore.GetRankById(userId),
                TopRankScore = topRankAndScore.GetEntryById(userId).Points,
                TopScore = topScoreAndRank.GetEntryById(userId).Points,
                TopScoreRank = topScoreAndRank.GetRankById(userId)
            };
        }

        public CommandResult NextTournament(string data)
        {
            var compact = NextTournamentCompact(data, null);
            if (compact.Items.Count() == 0)
            {
                return new CommandResult("Stream is offline. Next fishing tournament will begin 15m after the beginning of next stream.");
            }
            var toNext = compact.Items.FirstOrDefault();
            if (toNext.TotalMilliseconds > 0)
            {
                return new CommandResult($"Next fishing tournament begins in {toNext.TotalMinutes} minutes.");
            }
            return new CommandResult($"A fishing tournament is active now! Go catch fish at: https://tinyurl.com/PlayWolfpackRPG !");
        }

        public CompactCollection<TimeSpan> NextTournamentCompact(string data, string userId)
        {
            if (system.NextTournament == null)
            {
                return new CompactCollection<TimeSpan>(new TimeSpan[0], null);
            }
            else
            {
                var toNext = (DateTime)system.NextTournament - DateTime.Now;
                return new CompactCollection<TimeSpan>(new TimeSpan[] { toNext }, x => x.ToString("c"));
            }
        }
    }

    public class TournamentResultsResponse : ICompactResponse
    {
        public DateTime Ended { get; set; }
        public int Participants { get; set; }
        public string Winner { get; set; }
        public int WinnerPoints { get; set; }
        public int Rank { get; set; }
        public int UserPoints { get; set; }

        public IEnumerable<string> ToCompact()
        {
            return new string[] { $"{Ended}|{Participants}|{Winner}|{WinnerPoints}|{Rank}|{UserPoints};" };
        }
    }

    public class TournamentRecordsResponse : ICompactResponse
    {
        public int TopRank { get; set; }
        public int TopRankScore { get; set; }
        public int TopScore { get; set; }
        public int TopScoreRank { get; set; }

        public IEnumerable<string> ToCompact()
        {
            return new string[] { $"{TopRank}|{TopRankScore}|{TopScore}|{TopScoreRank};" };
        }
    }
}