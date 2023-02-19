using LobotJR.Data;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using static LobotJR.Modules.Fishing.FishingSystem;

namespace LobotJR.Test.Systems.Fishing
{
    [TestClass]
    public class FishingSystemTests
    {
        private SqliteRepositoryManager Manager;
        private FishingSystem FishingSystem;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());

            FishingSystem = new FishingSystem(
                Manager.Users,
                Manager.FishData,
                Manager.AppSettings);
        }

        [TestMethod]
        public void GetsFisherById()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var retrieved = FishingSystem.GetFisherById(userId);
            Assert.AreEqual(userId, retrieved.UserId);
        }

        [TestMethod]
        public void CreatesFisherWhenNoneExist()
        {
            var retrieved = FishingSystem.GetFisherById("Invalid Id");
            Assert.IsNotNull(retrieved);
        }

        [TestMethod]
        public void CalculatesFishSizes()
        {
            var fisher = new Fisher
            {
                Hooked = new Fish()
                {
                    MinimumWeight = 1,
                    MaximumWeight = 10,
                    MinimumLength = 11,
                    MaximumLength = 20,
                }
            };
            var catchData = FishingSystem.CalculateFishSizes(fisher);
            Assert.IsTrue(fisher.Hooked.MinimumWeight <= catchData.Weight);
            Assert.IsTrue(fisher.Hooked.MaximumWeight >= catchData.Weight);
            Assert.IsTrue(fisher.Hooked.MinimumLength <= catchData.Length);
            Assert.IsTrue(fisher.Hooked.MaximumLength >= catchData.Length);
        }

        [TestMethod]
        public void CalculateFishSizesRandomizesWithSteppedWeights()
        {
            var fisher = new Fisher();
            var fish = new Fish()
            {
                MinimumWeight = 1,
                MaximumWeight = 10,
                MinimumLength = 11,
                MaximumLength = 20,
            };
            fisher.Hooked = fish;
            var minWeightGroup = (fisher.Hooked.MaximumWeight - fisher.Hooked.MinimumWeight) / 5 + fisher.Hooked.MinimumWeight;
            var minLengthGroup = (fisher.Hooked.MaximumLength - fisher.Hooked.MinimumLength) / 5 + fisher.Hooked.MinimumLength;
            var maxWeightGroup = (fisher.Hooked.MaximumWeight - fisher.Hooked.MinimumWeight) / 5 * 4 + fisher.Hooked.MinimumWeight;
            var maxLengthGroup = (fisher.Hooked.MaximumLength - fisher.Hooked.MinimumLength) / 5 * 4 + fisher.Hooked.MinimumLength;
            var sampleSize = 10000;
            var samples = new List<Catch>();
            for (var i = 0; i < sampleSize; i++)
            {
                samples.Add(FishingSystem.CalculateFishSizes(fisher));
            }
            var minGroupSize = samples.Count(x => x.Length <= minLengthGroup && x.Weight <= minWeightGroup);
            var maxGroupSize = samples.Count(x => x.Length >= maxLengthGroup && x.Weight >= maxWeightGroup);
            Assert.IsTrue(minGroupSize > sampleSize * 0.39 && minGroupSize < sampleSize * 0.41);
            Assert.IsTrue(maxGroupSize > 0 && maxGroupSize < sampleSize * 0.02);
        }

        [TestMethod]
        public void CalculateFishSizesRandomizesWithNormalDistribution()
        {
            var fisher = new Fisher();
            var fish = new Fish()
            {
                MinimumWeight = 1,
                MaximumWeight = 10,
                MinimumLength = 11,
                MaximumLength = 20,
            };
            fisher.Hooked = fish;

            var weightRange = fish.MaximumWeight - fish.MinimumWeight;
            var lengthRange = fish.MaximumLength - fish.MinimumLength;
            var weightMean = (weightRange) / 2 + fish.MinimumWeight;
            var lengthMean = (lengthRange) / 2 + fish.MinimumLength;
            var weightStdMin = weightMean - weightRange / 6;
            var weightStdMax = weightMean + weightRange / 6;
            var lengthStdMin = lengthMean - lengthRange / 6;
            var lengthStdMax = lengthMean + lengthRange / 6;

            var appSettings = Manager.AppSettings.Read().First();
            appSettings.FishingUseNormalSizes = true;
            var sampleSize = 10000;
            var samples = new List<Catch>();
            for (var i = 0; i < sampleSize; i++)
            {
                samples.Add(FishingSystem.CalculateFishSizes(fisher));
            }
            var oneStdGroupSizeWeight = samples.Count(x => x.Weight >= weightStdMin && x.Weight <= weightStdMax);
            var oneStdGroupSizeLength = samples.Count(x => x.Length >= lengthStdMin && x.Length <= lengthStdMax);
            Assert.IsTrue(samples.Min(x => x.Weight) >= fish.MinimumWeight);
            Assert.IsTrue(samples.Max(x => x.Weight) <= fish.MaximumWeight);
            Assert.IsTrue(samples.Min(x => x.Length) >= fish.MinimumLength);
            Assert.IsTrue(samples.Max(x => x.Length) <= fish.MaximumLength);
            Assert.IsTrue(oneStdGroupSizeWeight > sampleSize * 0.66 && oneStdGroupSizeWeight < sampleSize * 0.70);
            Assert.IsTrue(oneStdGroupSizeLength > sampleSize * 0.66 && oneStdGroupSizeLength < sampleSize * 0.70);
        }

        [TestMethod]
        public void CastsLine()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = false;
            FishingSystem.Cast(fisher.UserId);
            var appSettings = Manager.AppSettings.Read().First();
            Assert.IsTrue(fisher.IsFishing);
            Assert.IsTrue(fisher.HookedTime >= DateTime.Now.AddSeconds(appSettings.FishingCastMinimum));
            Assert.IsTrue(fisher.HookedTime <= DateTime.Now.AddSeconds(appSettings.FishingCastMaximum));
        }

        [TestMethod]
        public void CastCreatesNewFisherIfNoneExistsWithMatchingUserId()
        {
            var newId = "NewId";
            FishingSystem.Cast(newId);
            var newFisher = FishingSystem.GetFisherById(newId);
            Assert.IsNotNull(newFisher);
            Assert.IsTrue(newFisher.IsFishing);
        }

        [TestMethod]
        public void HooksFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var result = FishingSystem.HookFish(fisher);
            Assert.IsTrue(result);
            Assert.IsNotNull(fisher.Hooked);
        }

        [TestMethod]
        public void HooksFishWithNormalRarityDistribution()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var appSettings = Manager.AppSettings.Read().First();
            appSettings.FishingUseNormalRarity = true;
            var rarities = Manager.FishData.Read().Select(x => x.Rarity).Distinct().ToArray();
            var sampleSize = 10000;
            var samples = new List<Fish>();
            for (var i = 0; i < sampleSize; i++)
            {
                FishingSystem.HookFish(fisher);
                samples.Add(fisher.Hooked);
            }
            var commonCount = samples.Count(x => x.Rarity.Equals(rarities[0]));
            var uncommonCount = samples.Count(x => x.Rarity.Equals(rarities[1]));
            var rareCount = samples.Count(x => x.Rarity.Equals(rarities[2]));
            Assert.IsTrue(commonCount >= sampleSize * 0.67 && commonCount <= sampleSize * 0.70);
            Assert.IsTrue(uncommonCount >= sampleSize * 0.25 && uncommonCount <= sampleSize * 0.28);
            Assert.IsTrue(rareCount > 0 && rareCount <= sampleSize * 0.05);
        }

        [TestMethod]
        public void HooksFishWithWeightedRarityDistribution()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var rarities = Manager.FishData.Read().Select(x => x.Rarity).Distinct().ToArray();
            var sampleSize = 10000;
            var samples = new List<Fish>();
            for (var i = 0; i < sampleSize; i++)
            {
                FishingSystem.HookFish(fisher);
                samples.Add(fisher.Hooked);
            }
            var weightTotal = (float)rarities.Sum(x => x.Weight);
            var commonCount = samples.Count(x => x.Rarity.Equals(rarities[0]));
            var uncommonCount = samples.Count(x => x.Rarity.Equals(rarities[1]));
            var rareCount = samples.Count(x => x.Rarity.Equals(rarities[2]));
            var commonWeight = (float)rarities[0].Weight / weightTotal;
            var uncommonWeight = (float)rarities[1].Weight / weightTotal;
            var rareWeight = (float)rarities[2].Weight / weightTotal;
            Assert.IsTrue(commonCount >= sampleSize * (commonWeight / 1.1) && commonCount <= sampleSize * (commonWeight * 1.1));
            Assert.IsTrue(uncommonCount >= sampleSize * (uncommonWeight / 1.1) && uncommonCount <= sampleSize * (uncommonWeight * 1.1));
            Assert.IsTrue(rareCount >= sampleSize * (rareWeight / 1.1) && rareCount <= sampleSize * (rareWeight * 1.1));
        }

        [TestMethod]
        public void UnhooksFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.Hooked = new Fish();
            fisher.HookedTime = DateTime.Now;
            FishingSystem.UnhookFish(fisher);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
        }

        [TestMethod]
        public void CatchesFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.Hooked = Manager.FishData.Read().First();
            var catchData = FishingSystem.CatchFish(fisher);
            Assert.IsNotNull(catchData);
            Assert.AreEqual(Manager.FishData.Read().First().Id, catchData.Fish.Id);
            Assert.AreEqual(fisher.UserId, catchData.UserId);
        }

        [TestMethod]
        public void CatchFishDoesNothingWhenFisherIsNull()
        {
            var catchData = FishingSystem.CatchFish(null);
            Assert.IsNull(catchData);
        }

        [TestMethod]
        public void CatchFishDoesNothingIfNoFishHooked()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.Hooked = null;
            var catchData = FishingSystem.CatchFish(fisher);
            Assert.IsNull(catchData);
        }

        [TestMethod]
        public void ProcessHooksFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            var callbackMock = new Mock<FisherEventHandler>();
            FishingSystem.FishHooked += callbackMock.Object;
            FishingSystem.Process(true);
            Assert.IsNotNull(fisher.Hooked);
            callbackMock.Verify(x => x(fisher), Times.Once);
        }

        [TestMethod]
        public void ProcessReleasesFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var appSettings = Manager.AppSettings.Read().First();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now.AddSeconds(-appSettings.FishingHookLength);
            fisher.Hooked = Manager.FishData.Read().First();
            var callbackMock = new Mock<FisherEventHandler>();
            FishingSystem.FishGotAway += callbackMock.Object;
            FishingSystem.Process(true);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
            callbackMock.Verify(x => x(fisher), Times.Once);
        }
    }
}

