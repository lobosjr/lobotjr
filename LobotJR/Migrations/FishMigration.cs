using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LobotJR.Migrations
{
    /// <summary>
    /// Loads fish data from legacy flat-file format and imports them into the
    /// new sql format.
    /// </summary>
    public class FishMigration
    {
        public static readonly string FishDataPath = "content/fishlist.ini";

        private static IEnumerable<FishSize> CreateFishSizes()
        {
            return new FishSize[]
            {
                new FishSize() { Id = 0, Name = "Tiny", Message = "You feel a light tug at your line!" },
                new FishSize() { Id = 1, Name = "Small", Message = "Something nibbles at your bait!" },
                new FishSize() { Id = 2, Name = "Medium", Message = "A strong tug snags your bait!" },
                new FishSize() { Id = 3, Name = "Large", Message = "Whoa! Something big grabs your line!" },
                new FishSize() { Id = 4, Name = "Huge", Message = "You're almost pulled into the water! Something HUGE is hooked!" }
            };
        }

        private static IEnumerable<FishRarity> CreateFishRarities(int numRarities)
        {
            const float totalChance = 100;
            float dividingFactor = totalChance / numRarities;
            List<float> chances = new List<float>();
            float updatedChance = totalChance;
            // algorithm to generate rarity %'s for each rarity (i.e., if there are 3 rarities, common/uncommon/epic,
            // assign each one a decreasing chance of it being picked ex: 66%/17%/8.5% based on this algorithm
            while (numRarities > 0)
            {
                chances.Add(dividingFactor * 2);
                updatedChance -= (dividingFactor * 2);
                numRarities--;
                if (numRarities == 2)
                {
                    // if last 2 rarities, assign 3/4 of remaining chance to 2nd to last and 1/4 to the last then break
                    chances.Add((3 * dividingFactor) / 4);
                    chances.Add(dividingFactor / 4);
                    break;
                }
                dividingFactor = (updatedChance / numRarities);
            }
            var rarities = new List<FishRarity>();
            for (var i = 0; i < chances.Count; i++)
            {
                rarities.Add(new FishRarity() { Id = i, Name = "", Weight = chances[i] });
            }
            return rarities;
        }

        private static IEnumerable<LegacyFish> LoadLegacyFishData(string fishDataPath)
        {
            var fileText = File.ReadAllLines(fishDataPath, UTF8Encoding.Default);

            var fishDatabase = new List<LegacyFish>();
            foreach (var line in fileText)
            {
                var data = File.ReadAllLines($"content/fishing/{line}", UTF8Encoding.Default);
                var id = fishDatabase.Count;
                var name = data[0].Split('=')[1];
                int.TryParse(data[1].Split('=')[1], out var sizeCategory);
                float.TryParse(data[2].Split('=')[1], out var lengthMin);
                float.TryParse(data[3].Split('=')[1], out var lengthMax);
                float.TryParse(data[4].Split('=')[1], out var weightMin);
                float.TryParse(data[5].Split('=')[1], out var weightMax);
                int.TryParse(data[6].Split('=')[1], out var rarity);
                var flavorText = data[7].Split('=')[1];
                fishDatabase.Add(new LegacyFish()
                {
                    ID = id,
                    name = name,
                    sizeCategory = sizeCategory,
                    lengthRange = new float[] { lengthMin, lengthMax },
                    weightRange = new float[] { weightMin, weightMax },
                    rarity = rarity,
                    flavorText = flavorText
                });
            }

            return fishDatabase;
        }

        /// <summary>
        /// Read the data from the legacy flat-file format, add them to the new
        /// repository, and commit the new data.
        /// </summary>
        /// <param name="fishDataPath">The path to the file containing the fish data.</param>
        /// <param name="fishRepository">The repository to import the the fish data to.</param>
        /// <returns>Whether or not the data was imported.</returns>
        public static bool ImportFishDataIntoSql(string fishDataPath, IRepository<Fish> fishRepository)
        {
            if (File.Exists(fishDataPath))
            {
                var fishDatabase = LoadLegacyFishData(fishDataPath);
                var sizes = CreateFishSizes();
                var rarities = CreateFishRarities(fishDatabase.Select(x => x.rarity).Distinct().Count());

                foreach (var fish in fishDatabase.OrderBy(x => x.rarity))
                {
                    fishRepository.Create(new Fish()
                    {
                        Id = fish.ID,
                        Name = fish.name,
                        SizeCategory = sizes.First(x => x.Id == fish.sizeCategory - 1),
                        Rarity = rarities.First(x => x.Id == fish.rarity - 1),
                        FlavorText = fish.flavorText,
                        MinimumLength = fish.lengthRange[0],
                        MaximumLength = fish.lengthRange[1],
                        MinimumWeight = fish.weightRange[0],
                        MaximumWeight = fish.weightRange[1]
                    });
                }
                fishRepository.Commit();
                return true;
            }
            return false;
        }
    }

    public class LegacyFish
    {
        public int sizeCategory { get; set; } = -1;
        public int ID { get; set; } = -1;
        public int rarity { get; set; } = -1;
        public string name { get; set; } = "";
        public float[] lengthRange { get; set; } = { -1, -1 };
        public float[] weightRange { get; set; } = { -1, -1 };
        public string flavorText { get; set; } = "";

    }
}
