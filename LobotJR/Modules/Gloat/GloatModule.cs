using LobotJR.Command;
using LobotJR.Data.User;
using System.Collections.Generic;

namespace LobotJR.Modules.Gloat
{
    /// <summary>
    /// Module of access control commands.
    /// </summary>
    public class GloatModule : ICommandModule
    {
        private readonly GloatSystem GloatSystem;
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

        public GloatModule(GloatSystem gloatSystem, UserLookup userLookup)
        {
            GloatSystem = gloatSystem;
            UserLookup = userLookup;
            Commands = new CommandHandler[]
            {
                new CommandHandler("GloatFish", GloatFish, "gloatfish", "fishgloat", "gloat-fish")
            };
        }

        public CommandResult GloatFish(string data, string userId)
        {
            if (int.TryParse(data, out var id))
            {
                if (GloatSystem.CanGloatFishing(userId))
                {
                    var record = GloatSystem.FishingGloat(userId, id - 1);
                    if (record != null)
                    {
                        return new CommandResult($"You spent {GloatSystem.FishingGloatCost} wolfcoins to brag about your biggest {record.Fish.Name}.")
                        {
                            Messages = new string[] { $"{UserLookup.GetUsername(userId)} gloats about the time they caught a {record.Length} in. long, {record.Weight} pound {record.Fish.Name} lobosSmug" }
                        };
                    }
                    return new CommandResult("You don't have any fish! Type !cast to try and fish for some!");
                }
                return new CommandResult("You don't have enough coins to gloat!");
            }
            return new CommandResult("Invalid request. Syntax: !gloatfish <Fish #>");
        }
    }
}
