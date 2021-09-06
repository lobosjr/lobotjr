using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Data.User;
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
        private readonly UserLookup UserLookup;
        private readonly FishingSystem FishingSystem;
        private readonly Dictionary<string, int> Wolfcoins;

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

        /// <summary>
        /// Null response to indicate this module has no sub modules.
        /// </summary>
        public IEnumerable<ICommandModule> SubModules { get; private set; }

        public FishingModule(UserLookup userLookup, FishingSystem fishingSystem, IRepository<TournamentResult> tournamentResults, Dictionary<string, int> wolfcoins)
        {
            FishingSystem = fishingSystem;
            FishingSystem.FishHooked += FishingSystem_FishHooked;
            FishingSystem.FishGotAway += FishingSystem_FishGotAway;
            FishingSystem.NewGlobalRecord += FishingSystem_NewGlobalRecord;
            UserLookup = userLookup;
            Wolfcoins = wolfcoins;
            Commands = new CommandHandler[]
            {
                new CommandHandler("PlayerLeaderboard", PlayerLeaderboard, PlayerLeaderboardCompact, "fish"),
                new CommandHandler("GlobalLeaderboard", GlobalLeaderboard, GlobalLeaderboardCompact, "fishleaders", "leaderboards", "fish-leaders"),
                new CommandHandler("ReleaseFish", ReleaseFish, "releasefish", "release-fish"),
                new CommandHandler("CancelCast", CancelCast, "cancelcast", "cancel-cast"),
                new CommandHandler("CatchFish", CatchFish, "catch", "reel"),
                new CommandHandler("CastLine", Cast, "cast"),
            };
            SubModules = new ICommandModule[] { new FishingAdmin(fishingSystem), new TournamentModule(fishingSystem.Tournament, tournamentResults, userLookup) };
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

        private void FishingSystem_NewGlobalRecord(Catch catchData)
        {
            var recordMessage = $"{UserLookup.GetUsername(catchData.UserId)} just caught the heaviest {catchData.Fish.Name} ever! It weighs {catchData.Weight} pounds!";
            PushNotification?.Invoke(null, new CommandResult() { Messages = new string[] { recordMessage } });
        }

        public CompactCollection<Catch> PlayerLeaderboardCompact(string data, string userId)
        {
            string selectFunc(Catch x) => $"{x.Fish.Name}|{x.Length}|{x.Weight};";
            var fisher = FishingSystem.GetFisherById(userId);
            if (string.IsNullOrWhiteSpace(data))
            {
                if (fisher != null)
                {
                    return new CompactCollection<Catch>(fisher.Records, selectFunc);
                }
                return new CompactCollection<Catch>(new Catch[0], null);
            }
            else
            {
                if (int.TryParse(data, out var id))
                {
                    var fish = fisher.Records.ToList();
                    if (id > 0 && id <= fish.Count)
                    {
                        return new CompactCollection<Catch>(new Catch[] { fish[id - 1] }, selectFunc);
                    }
                }
                return null;
            }
        }

        public CommandResult PlayerLeaderboard(string data, string userId)
        {
            var compact = PlayerLeaderboardCompact(data, userId);
            var items = compact.Items.ToList();
            if (string.IsNullOrWhiteSpace(data))
            {
                if (items.Count > 0)
                {
                    var responses = new List<string>
                    {
                        $"You've caught {items.Count} different types of fish: "
                    };
                    responses.AddRange(items.Select((x, i) => $"{i}: {x.Fish.Name}"));
                    return new CommandResult(responses.ToArray());
                }
                else
                {
                    return new CommandResult($"You haven't caught any fish yet!");
                }
            }
            else
            {
                if (compact == null)
                {
                    return new CommandResult($"Invalid request. Syntax: !fish <Fish #>");
                }
                var fishCatch = compact.Items.FirstOrDefault();
                var responses = new List<string>
                {
                    $"Name - {fishCatch.Fish.Name}",
                    $"Length - {fishCatch.Length} in.",
                    $"Weight - {fishCatch.Weight} lbs.",
                    $"Size Category - {fishCatch.Fish.SizeCategory.Name}",
                    $"Description - {fishCatch.Fish.FlavorText}"
                };
                return new CommandResult(responses.ToArray());
            }
        }

        public CompactCollection<Catch> GlobalLeaderboardCompact(string data, string userId)
        {
            return new CompactCollection<Catch>(FishingSystem.GetLeaderboard(), x => $"{x.Fish.Name}|{x.Length}|{x.Weight}|{UserLookup.GetUsername(x.UserId)};");
        }

        public CommandResult GlobalLeaderboard(string data)
        {
            var compact = GlobalLeaderboardCompact(data, null);
            return new CommandResult(compact.Items.Select(x => $"Largest {x.Fish.Name} caught by {UserLookup.GetUsername(x.UserId)} at {x.Weight} lbs., {x.Length} in.").ToArray());
        }

        public CommandResult ReleaseFish(string data, string userId)
        {
            if (int.TryParse(data, out var param))
            {
                var fisher = FishingSystem.GetFisherById(userId);
                if (fisher == null || fisher.Records.Count == 0)
                {
                    return new CommandResult("You don't have any fish! Type !cast to try and fish for some!");
                }
                if (param > 0 && param <= fisher.Records.Count)
                {
                    var fish = fisher.Records[param - 1];
                    FishingSystem.DeleteFish(fisher, fish);
                    return new CommandResult($"You released your {fish.Fish.Name}. Bye bye!");
                }
            }
            return new CommandResult($"Invalid request. Syntax: !releasefish <Fish #>");
        }

        public CommandResult CancelCast(string data, string userId)
        {
            var fisher = FishingSystem.GetFisherById(userId);
            if (fisher != null && fisher.IsFishing)
            {
                FishingSystem.UnhookFish(fisher);
                return new CommandResult("You reel in the empty line.");
            }
            return new CommandResult("Your line has not been cast.");
        }

        public CommandResult CatchFish(string data, string userId)
        {
            var fisher = FishingSystem.GetFisherById(userId);
            if (fisher != null && fisher.IsFishing)
            {
                var catchData = FishingSystem.CatchFish(fisher);
                if (catchData == null)
                {
                    return new CommandResult("Nothing is biting yet! To reset your cast, use !cancelcast");
                }

                if (FishingSystem.Tournament.IsRunning)
                {
                    var record = fisher.Records.Where(x => x.Fish.Id == catchData.Fish.Id).FirstOrDefault();
                    var responses = new List<string>();
                    if (record.Weight == catchData.Weight)
                    {
                        responses.Add($"This is the biggest {catchData.Fish.Name} you've ever caught!");
                    }
                    var userEntry = FishingSystem.Tournament.CurrentTournament.Entries.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
                    var sorted = FishingSystem.Tournament.CurrentTournament.Entries.OrderBy(x => x.Points).ToList().IndexOf(userEntry) + 1;
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
            if (fisher != null)
            {
                if (fisher.Hooked != null)
                {
                    return new CommandResult("Something's already bit your line! Quick, type !catch to snag it!");
                }
                if (fisher.IsFishing)
                {
                    return new CommandResult("Your line is already cast! I'm sure a fish'll be along soon...");
                }
            }
            FishingSystem.Cast(userId);
            return new CommandResult("You cast your line out into the water.");
        }

        public CommandResult Gloat(string data, string userId)
        {
            if (Wolfcoins.TryGetValue(UserLookup.GetUsername(userId), out var currency))
            {
                if (currency >= FishingSystem.GloatCost)
                {
                    var fisher = FishingSystem.GetFisherById(userId);
                    if (fisher.Records.Any())
                    {
                        if (int.TryParse(data, out var id))
                        {
                            if (id > 0 && id <= fisher.Records.Count)
                            {
                                var fish = fisher.Records[id - 1];
                                return new CommandResult($"You spent {FishingSystem.GloatCost} wolfcoins to brag about your biggest {fish.Fish.Name}.")
                                {
                                    Messages = new string[] { $"{UserLookup.GetUsername(userId)} gloats about the time they caught a {fish.Length} in. long, {fish.Weight} pound {fish.Fish.Name} lobosSmug" }
                                };
                            }
                        }
                        return new CommandResult("Invalid request. Syntax: !gloatfish <Fish #>");
                    }
                    else
                    {
                        return new CommandResult("You don't have any fish! Type !cast to try and fish for some!");
                    }
                }
            }
            return new CommandResult("You don't have enough coins to gloat!");
        }
    }
}
