using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    public abstract class FishingTestBase
    {
        private readonly Random random = new Random();

        protected UserLookup UserLookup;
        protected Dictionary<string, int> WolfcoinList;
        protected FishingSystem System;
        protected FishingModule Module;
        protected FishingAdmin AdminModule;
        protected TournamentModule TournamentModule;
        protected AppSettings AppSettings;

        protected MockContext Context;
        protected SqliteRepositoryManager Manager;

        private Fish CreateFish(int id, string name, string flavorText, int minLength, int maxLength, int minWeight, int maxWeight, int sizeId, string sizeName, string sizeMessage, int rarityId, string rarityName, float rarityWeight)
        {
            return new Fish()
            {
                Id = id,
                Name = name,
                FlavorText = flavorText,
                MinimumLength = minLength,
                MaximumLength = maxLength,
                MinimumWeight = minWeight,
                MaximumWeight = maxWeight,
                SizeCategory = new FishSize()
                {
                    Id = sizeId,
                    Name = sizeName,
                    Message = sizeMessage
                },
                Rarity = new FishRarity()
                {
                    Id = rarityId,
                    Name = rarityName,
                    Weight = rarityWeight
                }
            };
        }

        private Fisher CreateFisher(int dbId, string userId, int catchIdStart, List<Fish> fish)
        {
            var records = new List<Catch>();
            for (var i = 0; i < fish.Count; i++)
            {
                records.Add(new Catch()
                {
                    Id = catchIdStart + i,
                    UserId = userId,
                    Fish = fish[i],
                    Length = (float)random.NextDouble() * (fish[i].MaximumLength - fish[i].MinimumLength) + fish[i].MinimumLength,
                    Weight = (float)random.NextDouble() * (fish[i].MaximumWeight - fish[i].MinimumWeight) + fish[i].MinimumWeight
                });
            }
            return new Fisher()
            {
                Id = dbId,
                UserId = userId,
                Records = records
            };
        }

        private List<LeaderboardEntry> CreateLeaderboardFromFishers(List<Fish> fishData, List<Fisher> fisherData)
        {
            var output = new List<LeaderboardEntry>();
            var records = fisherData.Select(x => x.Records);
            foreach (var fish in fishData)
            {
                var catches = fisherData.Select(x => x.Records.Where(y => y.Fish.Id == fish.Id).FirstOrDefault());
                var ordered = catches.Where(x => x != null).OrderByDescending(x => x.Weight).FirstOrDefault();
                if (ordered != null)
                {
                    output.Add(new LeaderboardEntry()
                    {
                        Fish = ordered.Fish,
                        Length = ordered.Length,
                        Weight = ordered.Weight,
                        UserId = ordered.UserId
                    });
                }
            }
            return output;
        }

        private TournamentResult CreateTournamentResult(int hours, int minutes, int seconds, params TournamentEntry[] entries)
        {
            return new TournamentResult(DateTime.Now - new TimeSpan(hours, minutes, seconds), entries);
        }

        private void InitializeContext(MockContext context)
        {
            context.FishData.Add(CreateFish(1, "SmallTestFish", "It's a small fish.", 10, 20, 50, 60, 1, "Small", "Light tug", 1, "Common", 3.5f));
            context.FishData.Add(CreateFish(2, "BigTestFish", "It's a big fish.", 100, 200, 500, 600, 2, "Big", "Heavy tug", 2, "Uncommon", 2.5f));
            context.FishData.Add(CreateFish(3, "RareTestFish", "It's a rare fish.", 1000, 2000, 5000, 6000, 3, "Rare", "Mystical tug", 3, "Rare", 1.5f));
            context.SaveChanges();

            var fishData = context.FishData.ToList();
            context.Fishers.Add(CreateFisher(0, "10", 0, fishData));
            context.Fishers.Add(CreateFisher(1, "11", fishData.Count, fishData));
            context.Fishers.Add(CreateFisher(2, "12", fishData.Count * 2, fishData));
            context.Fishers.Add(CreateFisher(3, "13", fishData.Count * 3, new List<Fish>()));
            context.SaveChanges();

            var leaderboard = CreateLeaderboardFromFishers(fishData, context.Fishers.ToList());
            foreach (var entry in leaderboard)
            {
                context.FishingLeaderboard.Add(entry);
            }

            context.Users.Add(new UserMap() { TwitchId = "10", Username = "Foo" });
            context.Users.Add(new UserMap() { TwitchId = "11", Username = "Bar" });
            context.Users.Add(new UserMap() { TwitchId = "12", Username = "Fizz" });
            context.Users.Add(new UserMap() { TwitchId = "13", Username = "Buzz" });

            context.FishingTournaments.Add(CreateTournamentResult(0, 0, 30, new TournamentEntry("10", 10), new TournamentEntry("11", 20), new TournamentEntry("12", 30)));
            context.FishingTournaments.Add(CreateTournamentResult(0, 30, 30, new TournamentEntry("10", 30), new TournamentEntry("11", 20), new TournamentEntry("12", 10)));
            context.FishingTournaments.Add(CreateTournamentResult(1, 0, 30, new TournamentEntry("10", 40), new TournamentEntry("11", 20), new TournamentEntry("12", 50)));
            context.FishingTournaments.Add(CreateTournamentResult(1, 30, 30, new TournamentEntry("10", 35), new TournamentEntry("11", 20), new TournamentEntry("12", 10)));
            context.FishingTournaments.Add(CreateTournamentResult(2, 0, 30, new TournamentEntry("10", 40), new TournamentEntry("11", 60), new TournamentEntry("12", 50)));
        }

        protected void InitializeFishingModule()
        {
            WolfcoinList = new Dictionary<string, int>
            {
                { "Foo", 100 },
                { "Bar", 1 }
            };

            /// Database Test
            Context = MockContext.Create(InitializeContext);
            Context.Database.Initialize(true);
            Manager = new SqliteRepositoryManager(Context);
            var appSettings = Manager.AppSettings.Read().First();
            appSettings.FishingCastMaximum = 20;
            appSettings.FishingCastMinimum = 10;
            appSettings.FishingGloatCost = 10;
            appSettings.FishingHookLength = 10;
            appSettings.FishingTournamentCastMaximum = 2;
            appSettings.FishingTournamentCastMinimum = 1;
            appSettings.FishingTournamentDuration = 5;
            appSettings.FishingTournamentInterval = 10;
            appSettings.FishingUseNormalRarity = false;
            appSettings.FishingUseNormalSizes = false;
            appSettings.GeneralCacheUpdateTime = 2;
            Manager.AppSettings.Update(appSettings);
            Manager.AppSettings.Commit();

            AppSettings = Manager.AppSettings.Read().First();
            UserLookup = new UserLookup(Manager.Users, AppSettings);

            System = new FishingSystem(
                Manager.FishData,
                Manager.Fishers,
                Manager.FishingLeaderboard,
                Manager.TournamentResults,
                Manager.AppSettings);
            Module = new FishingModule(
                UserLookup,
                System,
                Manager.TournamentResults,
                WolfcoinList);
            AdminModule = Module.SubModules.Where(x => x is FishingAdmin).FirstOrDefault() as FishingAdmin;
            TournamentModule = Module.SubModules.Where(x => x is TournamentModule).FirstOrDefault() as TournamentModule;
        }
    }
}
