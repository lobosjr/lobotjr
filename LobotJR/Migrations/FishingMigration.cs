using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LobotJR.Migrations
{
    public class FishingMigration
    {
        private readonly string fishingPath = "fishing.json";
        private readonly string fishingLeaderboardPath = "fishingLeaderboard.json";

        public void LoadDataLegacy()
        {

            if (File.Exists(fishingPath))
            {
                var fishingList = JsonConvert.DeserializeObject<Dictionary<string, Fisherman>>(File.ReadAllText(fishingPath));
                Console.WriteLine("Fishing data loaded.");
            }
            if (File.Exists(fishingLeaderboardPath))
            {
                var fishingLeaderboard = JsonConvert.DeserializeObject<List<Fish>>(File.ReadAllText(fishingLeaderboardPath));
                Console.WriteLine("Fishing data loaded.");
            }
        }
    }

    public class Fisherman
    {

    }

    public class Fish
    {

    }
}
