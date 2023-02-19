using LobotJR.Command;
using LobotJR.Data.User;
using LobotJR.Modules.Fishing.Model;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class LeaderboardModule : ICommandModule
    {
        private readonly LeaderboardSystem TournamentSystem;
        private readonly UserLookup UserLookup;

        /// <summary>
        /// Prefix applied to names of commands within this module.
        /// </summary>
        public string Name => "Fishing.Leaderboard";

        /// <summary>
        /// Notifications when a tournament starts or ends.
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

        public LeaderboardModule(LeaderboardSystem system, UserLookup userLookup)
        {
            TournamentSystem = system;
            system.NewGlobalRecord += System_NewGlobalRecord;
            UserLookup = userLookup;
            Commands = new CommandHandler[]
            {
                new CommandHandler("PlayerLeaderboard", PlayerLeaderboard, PlayerLeaderboardCompact, "fish"),
                new CommandHandler("GlobalLeaderboard", GlobalLeaderboard, GlobalLeaderboardCompact, "fishleaders", "leaderboards", "fish-leaders"),
                new CommandHandler("ReleaseFish", ReleaseFish, "releasefish", "release-fish")
            };
        }

        private void System_NewGlobalRecord(LeaderboardEntry catchData)
        {
            var recordMessage = $"{UserLookup.GetUsername(catchData.UserId)} just caught the heaviest {catchData.Fish.Name} ever! It weighs {catchData.Weight} pounds!";
            PushNotification?.Invoke(null, new CommandResult() { Messages = new string[] { recordMessage } });
        }

        public CompactCollection<Catch> PlayerLeaderboardCompact(string data, string userId)
        {
            string selectFunc(Catch x) => $"{x.Fish.Name}|{x.Length}|{x.Weight};";
            var records = TournamentSystem.GetPersonalLeaderboard(userId);
            if (string.IsNullOrWhiteSpace(data))
            {
                if (records != null && records.Any())
                {
                    return new CompactCollection<Catch>(records, selectFunc);
                }
                return new CompactCollection<Catch>(new Catch[0], null);
            }
            else
            {
                if (int.TryParse(data, out var id))
                {
                    var fish = records.ElementAtOrDefault(id - 1);
                    if (fish != null)
                    {
                        return new CompactCollection<Catch>(new Catch[] { fish }, selectFunc);
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

        public CompactCollection<LeaderboardEntry> GlobalLeaderboardCompact(string data, string userId)
        {
            return new CompactCollection<LeaderboardEntry>(TournamentSystem.GetLeaderboard(), x => $"{x.Fish.Name}|{x.Length}|{x.Weight}|{UserLookup.GetUsername(x.UserId)};");
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
                var records = TournamentSystem.GetPersonalLeaderboard(userId);
                if (records == null || !records.Any())
                {
                    return new CommandResult("You don't have any fish! Type !cast to try and fish for some!");
                }
                var count = records.Count();
                if (count >= param && param > 0)
                {
                    var fishName = records.ElementAtOrDefault(param - 1).Fish.Name;
                    TournamentSystem.DeleteFish(userId, param - 1);
                    return new CommandResult($"You released your {fishName}. Bye bye!");
                }
                return new CommandResult($"That fish doesn't exist. Fish # must be between 1 and {count}");
            }
            return new CommandResult($"Invalid request. Syntax: !releasefish <Fish #>");
        }
    }
}
