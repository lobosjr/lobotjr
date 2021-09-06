using LobotJR.Data.User;
using LobotJR.Modules.Fishing.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LobotJR.Data.Import
{
    /// <summary>
    /// Loads fisher data from legacy flat-file format and imports them into
    /// the new sql format.
    /// </summary>
    public class FisherDataImport
    {
        public static readonly string FisherDataPath = "fishing.json";
        public static readonly string FishingLeaderboardPath = "fishingLeaderboard.json";

        /// <summary>
        /// Read the data from the legacy flat-file format, add them to the new
        /// repository, and commit the new data.
        /// </summary>
        /// <param name="fisherDataPath">The path to the file containing the fisher data.</param>
        /// <param name="fisherRepository">The repository to import the fisher data to.</param>
        /// <param name="fishRepository">The repository containing the fish data.</param>
        /// <param name="userLookup">The user lookup system to convert the stored usernames into user ids.</param>
        /// <param name="token">The oauth token used to fetch user ids.</param>
        /// <param name="clientId">The client id used to identify the call to fetch the user ids.</param>
        /// <returns>Whether or not the data was imported.</returns>
        public static bool ImportFisherDataIntoSql(string fisherDataPath, IRepository<Fisher> fisherRepository, IRepository<Fish> fishRepository, UserLookup userLookup, string token, string clientId)
        {
            if (File.Exists(fisherDataPath))
            {
                var fisherList = JsonConvert.DeserializeObject<Dictionary<string, LegacyFisher>>(File.ReadAllText(FisherDataPath));
                foreach (var fisher in fisherList)
                {
                    userLookup.GetId(fisher.Value.username);
                }
                userLookup.UpdateCache(token, clientId);
                foreach (var fisher in fisherList)
                {
                    var records = new List<Catch>();
                    foreach (var fish in fisher.Value.biggestFish)
                    {
                        records.Add(new Catch()
                        {
                            Fish = fishRepository.ReadById(fish.ID),
                            UserId = userLookup.GetId(fish.caughtBy),
                            Length = fish.length,
                            Weight = fish.weight
                        });
                    }
                    fisherRepository.Create(new Fisher()
                    {
                        UserId = userLookup.GetId(fisher.Value.username),
                        Records = records
                    });
                }
                fisherRepository.Commit();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Read the data from the legacy flat-file format, add them to the new
        /// repository, and commit the new data.
        /// </summary>
        /// <param name="fishingLeaderboardDataPath">The path to the file containing the leaderboard data.</param>
        /// <param name="leaderboardRepository">The repository to import the leaderboard data to.</param>
        /// <param name="fishRepository">The repository containing the fish data.</param>
        /// <param name="userLookup">The user lookup system to convert the stored usernames into user ids.</param>
        /// <param name="token">The oauth token used to fetch user ids.</param>
        /// <param name="clientId">The client id used to identify the call to fetch the user ids.</param>
        /// <returns>Whether or not the data was imported.</returns>
        public static bool ImportLeaderboardDataIntoSql(string fishingLeaderboardDataPath, IRepository<Catch> leaderboardRepository, IRepository<Fish> fishRepository, UserLookup userLookup, string token, string clientId)
        {
            if (File.Exists(fishingLeaderboardDataPath))
            {
                var fishingLeaderboard = JsonConvert.DeserializeObject<List<LegacyCatch>>(File.ReadAllText(FishingLeaderboardPath));
                foreach (var record in fishingLeaderboard)
                {
                    userLookup.GetId(record.caughtBy);
                }
                userLookup.UpdateCache(token, clientId);
                foreach (var record in fishingLeaderboard)
                {
                    leaderboardRepository.Create(new Catch()
                    {
                        Fish = fishRepository.ReadById(record.ID),
                        UserId = userLookup.GetId(record.caughtBy),
                        Length = record.length,
                        Weight = record.weight
                    });
                }
                leaderboardRepository.Commit();
                return true;
            }
            return false;
        }
    }

    public class LegacyFisher
    {
        public string username { get; set; } = "";
        public int level { get; set; } = -1;
        public int XP { get; set; } = -1;
        public int tournamentPoints { get; set; } = 0;
        public List<LegacyCatch> biggestFish { get; set; } = new List<LegacyCatch>();
        public int lure { get; set; } = -1;
        public bool isFishing { get; set; } = false;
        public bool fishHooked { get; set; } = false;
        public int hookedFishID { get; set; } = -1;
        public DateTime timeOfCatch { get; set; }
        public DateTime timeSinceHook { get; set; }
    }

    public class LegacyCatch : LegacyFish
    {
        public float length { get; set; } = -1;
        public float weight { get; set; } = -1;
        public string caughtBy { get; set; } = "";
    }
}
