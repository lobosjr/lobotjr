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


        protected ListRepository<Fish> FishDataMock;
        protected ListRepository<Fisher> FishersMock;
        protected ListRepository<Catch> LeaderboardMock;
        protected ListRepository<TournamentResult> TournamentResultsMock;
        protected ListRepository<AppSettings> AppSettingsMock;
        protected ListRepository<UserMap> UserMapMock;

        protected List<Fish> FishData;
        protected List<Fisher> FisherData;
        protected List<UserMap> UserMapData;
        protected UserLookup UserLookup;
        protected List<TournamentResult> TournamentResultsData;
        protected Dictionary<string, int> WolfcoinList;
        protected FishingSystem System;
        protected FishingModule Module;
        protected FishingAdmin AdminModule;
        protected TournamentModule TournamentModule;
        protected AppSettings AppSettings;

        private Fish createFish(int id, string name, string flavorText, int minLength, int maxLength, int minWeight, int maxWeight, int sizeId, string sizeName, string sizeMessage, int rarityId, string rarityName, int rarityWeight)
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

        private Fisher createFisher(int dbId, string userId, int catchIdStart, List<Fish> fish)
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

        private List<Catch> createLeaderboardFromFishers(List<Fish> fishData, List<Fisher> fisherData)
        {
            var output = new List<Catch>();
            var records = fisherData.Select(x => x.Records);
            foreach (var fish in fishData)
            {
                var catches = fisherData.Select(x => x.Records.Where(y => y.Fish.Id == fish.Id).FirstOrDefault());
                var ordered = catches.Where(x => x != null).OrderByDescending(x => x.Weight);
                output.Add(ordered.First());
            }
            return output;
        }

        protected void InitializeFishingModule()
        {
            FishData = new Fish[]
            {
                createFish(1, "SmallTestFish", "It's a small fish.", 10, 20, 50, 60, 1, "Small", "Light tug", 1, "Common", 1),
                createFish(2, "BigTestFish", "It's a big fish.", 100, 200, 500, 600, 2, "Big", "Heavy tug", 2, "Uncommon", 2)
            }.ToList();
            FishDataMock = new ListRepository<Fish>(FishData);

            FisherData = new Fisher[]
            {
                createFisher(0, "00", 0, FishData),
                createFisher(1, "01", FishData.Count, FishData),
                createFisher(2, "02", FishData.Count * 2, FishData),
                createFisher(3, "03", FishData.Count * 3, FishData),
            }.ToList();
            FishersMock = new ListRepository<Fisher>(FisherData);

            LeaderboardMock = new ListRepository<Catch>(createLeaderboardFromFishers(FishData, FisherData));

            UserMapData = new UserMap[]
            {
                new UserMap() { Id = "00", Username = "Foo"},
                new UserMap() { Id = "01", Username = "Bar"},
                new UserMap() { Id = "02", Username = "Fizz"},
                new UserMap() { Id = "03", Username = "Buzz"}
            }.ToList();
            UserMapMock = new ListRepository<UserMap>(UserMapData);
            AppSettings = new AppSettings() { GeneralCacheUpdateTime = 2 };
            UserLookup = new UserLookup(UserMapMock, AppSettings);

            TournamentResultsData = new TournamentResult[]
            {
                new TournamentResult(DateTime.Now - new TimeSpan(0, 0, 30), new TournamentEntry[] { new TournamentEntry("00", 10), new TournamentEntry("01", 20), new TournamentEntry("02", 30) }),
                new TournamentResult(DateTime.Now - new TimeSpan(0, 30, 30), new TournamentEntry[] { new TournamentEntry("00", 30), new TournamentEntry("01", 20), new TournamentEntry("02", 10) }),
                new TournamentResult(DateTime.Now - new TimeSpan(1, 0, 30), new TournamentEntry[] { new TournamentEntry("00", 40), new TournamentEntry("01", 20), new TournamentEntry("02", 50) }),
                new TournamentResult(DateTime.Now - new TimeSpan(1, 30, 30), new TournamentEntry[] { new TournamentEntry("00", 35), new TournamentEntry("01", 20), new TournamentEntry("02", 10) }),
                new TournamentResult(DateTime.Now - new TimeSpan(2, 0, 30), new TournamentEntry[] { new TournamentEntry("00", 40), new TournamentEntry("01", 60), new TournamentEntry("02", 50) })
            }.ToList();
            TournamentResultsMock = new ListRepository<TournamentResult>(TournamentResultsData);

            var appSettings = new AppSettings()
            {
                FishingCastMaximum = 20,
                FishingCastMinimum = 10,
                FishingGloatCost = 10,
                FishingHookLength = 10,
                FishingTournamentCastMaximum = 2,
                FishingTournamentCastMinimum = 1,
                FishingTournamentDuration = 5,
                FishingTournamentInterval = 10,
                FishingUseWeights = true
            };
            AppSettingsMock = new ListRepository<AppSettings>(new AppSettings[] { appSettings }.ToList());

            WolfcoinList = new Dictionary<string, int>();
            WolfcoinList.Add("Foo", 100);
            WolfcoinList.Add("Bar", 1);

            System = new FishingSystem(
                FishDataMock,
                FishersMock,
                LeaderboardMock,
                TournamentResultsMock,
                AppSettingsMock);
            Module = new FishingModule(
                UserLookup,
                System,
                TournamentResultsMock,
                WolfcoinList);
            AdminModule = Module.SubModules.Where(x => x is FishingAdmin).FirstOrDefault() as FishingAdmin;
            TournamentModule = Module.SubModules.Where(x => x is TournamentModule).FirstOrDefault() as TournamentModule;
        }
    }
}
