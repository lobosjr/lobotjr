using LobotJR.Command.Model.Fishing;
using LobotJR.Data;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command.System.Gloat
{
    /// <summary>
    /// Runs the tournament logic for the fishing system.
    /// </summary>
    public class GloatSystem : ISystem
    {
        private readonly IRepository<Catch> PersonalLeaderboard;
        private readonly Dictionary<string, int> Wolfcoins;
        public int FishingGloatCost { get; private set; }

        public GloatSystem(
            IRepository<Catch> personalLeaderboard,
            IRepository<AppSettings> appSettings,
            Dictionary<string, int> wolfcoins)
        {
            PersonalLeaderboard = personalLeaderboard;
            Wolfcoins = wolfcoins;
            FishingGloatCost = appSettings.Read().First().FishingGloatCost;
        }

        /// <summary>
        /// Checks if the user has the coins to gloat about a fishing record.
        /// </summary>
        /// <param name="userId">The user attempting to gloat.</param>
        /// <returns>True if the user has the coins to gloat, false if not.</returns>
        public bool CanGloatFishing(string userId)
        {
            if (Wolfcoins.TryGetValue(userId, out var coins))
            {
                return coins >= FishingGloatCost;
            }
            return false;
        }

        /// <summary>
        /// Attempts to gloat about a specific fishing record.
        /// </summary>
        /// <param name="userId">The user attempting to gloat.</param>
        /// <param name="fishId">The id of the fish to gloat about.</param>
        /// <returns>The details of the record to gloat about.</returns>
        public Catch FishingGloat(string userId, int fishId)
        {
            if (Wolfcoins.TryGetValue(userId, out var coins))
            {
                var records = PersonalLeaderboard.Read(x => x.UserId.Equals(userId)).OrderBy(x => x.FishId);
                if (fishId >= 0 && fishId < records.Count())
                {
                    Wolfcoins[userId] = coins - FishingGloatCost;
                    return records.ElementAt(fishId);
                }
            }
            return null;
        }

        public void Process(bool broadcasting)
        {
        }
    }
}
