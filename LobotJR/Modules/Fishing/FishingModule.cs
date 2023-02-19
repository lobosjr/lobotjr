using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Contains the compact methods for the fishing module.
    /// </summary>
    public class FishingModule : ICommandModule
    {
        private readonly FishingSystem FishingSystem;
        private readonly TournamentSystem TournamentSystem;
        private readonly LeaderboardSystem LeaderboardSystem;

        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Fishing";

        /// <summary>
        /// Invoked to notify users of fish being hooked or getting away, and
        /// for notifying chat when a user sets a new record.
        /// </summary>
        public event PushNotificationHandler PushNotification;

        /// <summary>
        /// A collection of commands for managing access to commands.
        /// </summary>
        public IEnumerable<CommandHandler> Commands { get; private set; }

        public FishingModule(FishingSystem fishingSystem, TournamentSystem tournamentSystem, LeaderboardSystem leaderboardSystem)
        {
            FishingSystem = fishingSystem;
            FishingSystem.FishHooked += FishingSystem_FishHooked;
            FishingSystem.FishGotAway += FishingSystem_FishGotAway;
            TournamentSystem = tournamentSystem;
            LeaderboardSystem = leaderboardSystem;
            Commands = new CommandHandler[]
            {
                new CommandHandler("CancelCast", CancelCast, "cancelcast", "cancel-cast"),
                new CommandHandler("CatchFish", CatchFish, "catch", "reel"),
                new CommandHandler("CastLine", Cast, "cast"),
            };
        }

        private void FishingSystem_FishHooked(Fisher fisher)
        {
            var hookMessage = $"{fisher.Hooked.SizeCategory.Message} Type !catch to reel it in!";
            PushNotification?.Invoke(fisher.UserId, new CommandResult(hookMessage));
        }

        private void FishingSystem_FishGotAway(Fisher fisher)
        {
            PushNotification?.Invoke(fisher.UserId, new CommandResult("Heck! The fish got away. Maybe next time..."));
        }

        public CommandResult CancelCast(string data, string userId)
        {
            var fisher = FishingSystem.GetFisherById(userId);
            if (fisher.IsFishing)
            {
                FishingSystem.UnhookFish(fisher);
                return new CommandResult("You reel in the empty line.");
            }
            return new CommandResult("Your line has not been cast.");
        }

        public CommandResult CatchFish(string data, string userId)
        {
            var fisher = FishingSystem.GetFisherById(userId);
            if (fisher.IsFishing)
            {
                var catchData = FishingSystem.CatchFish(fisher);
                if (catchData == null)
                {
                    return new CommandResult("Nothing is biting yet! To reset your cast, use !cancelcast");
                }

                if (TournamentSystem.IsRunning)
                {
                    var record = LeaderboardSystem.GetUserRecordForFish(userId, catchData.Fish);
                    var responses = new List<string>();
                    if (record.Weight == catchData.Weight)
                    {
                        responses.Add($"This is the biggest {catchData.Fish.Name} you've ever caught!");
                    }
                    var userEntry = TournamentSystem.CurrentTournament.Entries.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
                    var sorted = TournamentSystem.CurrentTournament.Entries.OrderBy(x => x.Points).ToList().IndexOf(userEntry) + 1;
                    responses.Add($"You caught a {catchData.Length} inch, {catchData.Weight} pound {catchData.Fish.Name} worth {catchData.Points} points! You are in {sorted.ToOrdinal()} place with {userEntry.Points} total points.");
                    return new CommandResult(responses.ToArray());
                }
                else
                {
                    return new CommandResult($"Congratulations! You caught a {catchData.Length} inch, {catchData.Weight} pound {catchData.Fish.Name}!");
                }
            }
            return new CommandResult($"Your line has not been cast. Use !cast to start fishing");
        }

        public CommandResult Cast(string data, string userId)
        {
            var fisher = FishingSystem.GetFisherById(userId);
            if (fisher.Hooked != null)
            {
                return new CommandResult("Something's already bit your line! Quick, type !catch to snag it!");
            }
            if (fisher.IsFishing)
            {
                return new CommandResult("Your line is already cast! I'm sure a fish'll be along soon...");
            }
            FishingSystem.Cast(userId);
            return new CommandResult("You cast your line out into the water.");
        }
    }
}
