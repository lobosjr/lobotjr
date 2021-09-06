using LobotJR.Modules.Fishing.Model;
using LobotJR.Test.Modules.Fishing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using static LobotJR.Modules.Fishing.FishingSystem;

namespace LobotJR.Test.Systems.Fishing
{
    [TestClass]
    public class FishingSystemTests : FishingTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeFishingModule();
        }

        [TestMethod]
        public void GetsFisherById()
        {
            var fisher = FisherData[0];
            var retrieved = System.GetFisherById(fisher.UserId);
            Assert.AreEqual(fisher, retrieved);
        }

        [TestMethod]
        public void GetFisherByIdGetsNullOnMissingFisher()
        {
            var retrieved = System.GetFisherById("Invalid Id");
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public void GetsLeaderboard()
        {
            var retrieved = System.GetLeaderboard();
            Assert.AreEqual(LeaderboardMock.Read(), retrieved);
        }

        [TestMethod]
        public void DeletesFish()
        {
            var fisher = FisherData.FirstOrDefault(x => x.UserId.Equals("00"));
            var fish = fisher.Records[0];
            System.DeleteFish(fisher, fish);
            Assert.IsFalse(fisher.Records.Any(x => x.Fish.Id.Equals(fish.Fish.Id)));
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnMissingFish()
        {
            var fisher = FisherData[0];
            var recordCount = fisher.Records.Count;
            System.DeleteFish(fisher, new Catch() { Id = -1 });
            Assert.AreEqual(recordCount, fisher.Records.Count);
        }
        
        [TestMethod]
        public void DeleteFishDoesNothingOnNullFish()
        {
            var fisher = FisherData[0];
            var recordCount = fisher.Records.Count;
            System.DeleteFish(fisher, null);
            Assert.AreEqual(recordCount, fisher.Records.Count);
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnMissingFisher()
        {
            var fisherCount = FisherData.Count;
            System.DeleteFish(new Fisher() { Id = -1 }, new Catch());
            Assert.AreEqual(fisherCount, FisherData.Count);
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnNullFisher()
        {
            var fisherCount = FisherData.Count;
            System.DeleteFish(null, new Catch());
            Assert.AreEqual(fisherCount, FisherData.Count);
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
            var catchData = System.CalculateFishSizes(fisher);
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
                samples.Add(System.CalculateFishSizes(fisher));
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

            AppSettings.FishingUseNormalSizes = true;
            var sampleSize = 10000;
            var samples = new List<Catch>();
            for (var i = 0; i < sampleSize; i++)
            {
                samples.Add(System.CalculateFishSizes(fisher));
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
        public void UpdatesPersonalLeaderboardWithNewFishType()
        {
            var fisher = FisherData.First();
            fisher.Records.Clear();
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            };
            var result = System.UpdatePersonalLeaderboard(fisher, catchData);
            Assert.IsTrue(result);
            Assert.AreEqual(1, fisher.Records.Count);
            Assert.AreEqual(catchData.Fish.Id, fisher.Records[0].Fish.Id);
        }

        [TestMethod]
        public void UpdatesPersonalLeaderboardWithExistingFishType()
        {
            var fisher = FisherData.First();
            fisher.Records.Clear();
            fisher.Records.Add(new Catch()
            {
                Fish = FishData[0],
                Weight = 10
            });
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            };
            var result = System.UpdatePersonalLeaderboard(fisher, catchData);
            Assert.IsTrue(result);
            Assert.AreEqual(1, fisher.Records.Count);
            Assert.AreEqual(catchData.Weight, fisher.Records[0].Weight);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWithNullFisher()
        {
            var result = System.UpdatePersonalLeaderboard(null, new Catch());
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWithNullCatchData()
        {
            var result = System.UpdatePersonalLeaderboard(new Fisher(), null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWhenCatchIsNotNewRecord()
        {
            var fisher = FisherData.First();
            fisher.Records.Clear();
            fisher.Records.Add(new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            });
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 10
            };
            var result = System.UpdatePersonalLeaderboard(fisher, catchData);
            Assert.IsFalse(result);
            Assert.AreEqual(1, fisher.Records.Count);
            Assert.AreNotEqual(catchData.Weight, fisher.Records[0].Weight);
        }

        [TestMethod]
        public void UpdatesGlobalLeaderboardWithNewFishType()
        {
            LeaderboardMock.Data.Clear();
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            };
            var result = System.UpdateGlobalLeaderboard(catchData);
            var leaderboard = System.GetLeaderboard();
            Assert.IsTrue(result);
            Assert.AreEqual(1, leaderboard.Count());
            Assert.AreEqual(catchData.Weight, leaderboard.First().Weight);
        }

        [TestMethod]
        public void UpdatesGlobalLeaderboardWithExistingFishType()
        {
            LeaderboardMock.Data.Clear();
            LeaderboardMock.Data.Add(new Catch()
            {
                Fish = FishData[0],
                Weight = 10
            });
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            };
            var result = System.UpdateGlobalLeaderboard(catchData);
            var leaderboard = System.GetLeaderboard();
            Assert.IsTrue(result);
            Assert.AreEqual(1, leaderboard.Count());
            Assert.AreEqual(catchData.Weight, leaderboard.First().Weight);
        }

        [TestMethod]
        public void UpdateGlobalLeaderboardReturnsFalseWithNullCatchData()
        {
            var result = System.UpdateGlobalLeaderboard(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateGlobalLeaderboardReturnsFalseWhenCatchIsNotNewRecord()
        {
            LeaderboardMock.Data.Clear();
            LeaderboardMock.Data.Add(new Catch()
            {
                Fish = FishData[0],
                Weight = 100
            });
            var catchData = new Catch()
            {
                Fish = FishData[0],
                Weight = 10
            };
            var result = System.UpdateGlobalLeaderboard(catchData);
            var leaderboard = System.GetLeaderboard();
            Assert.IsFalse(result);
            Assert.AreEqual(1, leaderboard.Count());
            Assert.AreNotEqual(catchData.Weight, leaderboard.First().Weight);
        }

        [TestMethod]
        public void CastsLine()
        {
            var fisher = FisherData.First();
            fisher.IsFishing = false;
            System.Cast(fisher.UserId);
            Assert.IsTrue(fisher.IsFishing);
            Assert.IsTrue(fisher.HookedTime >= DateTime.Now.AddSeconds(AppSettings.FishingCastMinimum));
            Assert.IsTrue(fisher.HookedTime <= DateTime.Now.AddSeconds(AppSettings.FishingCastMaximum));
        }

        [TestMethod]
        public void CastCreatesNewFisherIfNoneExistsWithMatchingUserId()
        {
            var newId = "NewId";
            System.Cast(newId);
            var newFisher = FisherData.FirstOrDefault(x => x.UserId.Equals(newId));
            Assert.IsNotNull(newFisher);
        }

        [TestMethod]
        public void CastSetsHookTimeWithTournamentSettingsWhileTournamentActive()
        {
            var fisher = FisherData.First();
            fisher.IsFishing = false;
            System.Tournament.StartTournament();
            System.Cast(fisher.UserId);
            Assert.IsTrue(fisher.IsFishing);
            Assert.IsTrue(fisher.HookedTime >= DateTime.Now.AddSeconds(AppSettings.FishingTournamentCastMinimum));
            Assert.IsTrue(fisher.HookedTime <= DateTime.Now.AddSeconds(AppSettings.FishingTournamentCastMaximum));
        }

        [TestMethod]
        public void HooksFish()
        {
            var fisher = FisherData.First();
            var result = System.HookFish(fisher);
            Assert.IsTrue(result);
            Assert.IsNotNull(fisher.Hooked);
        }

        [TestMethod]
        public void HooksFishWithNormalRarityDistribution()
        {
            var fisher = FisherData.First();
            AppSettings.FishingUseNormalRarity = true;
            var rarities = FishData.Select(x => x.Rarity).Distinct().ToArray();
            var sampleSize = 10000;
            var samples = new List<Fish>();
            for (var i = 0; i < sampleSize; i++)
            {
                System.HookFish(fisher);
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
            var fisher = FisherData.First();
            var rarities = FishData.Select(x => x.Rarity).Distinct().ToArray();
            var sampleSize = 10000;
            var samples = new List<Fish>();
            for (var i = 0; i < sampleSize; i++)
            {
                System.HookFish(fisher);
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
            var fisher = FisherData.First();
            fisher.IsFishing = true;
            fisher.Hooked = new Fish();
            fisher.HookedTime = DateTime.Now;
            System.UnhookFish(fisher);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
        }

        [TestMethod]
        public void CatchesFish()
        {
            var fisher = FisherData.First();
            fisher.Hooked = FishData[0];
            var catchData = System.CatchFish(fisher);
            Assert.IsNotNull(catchData);
            Assert.AreEqual(FishData[0].Id, catchData.Fish.Id);
            Assert.AreEqual(fisher.UserId, catchData.UserId);
        }

        [TestMethod]
        public void CatchFishDoesNothingWhenFisherIsNull()
        {
            var catchData = System.CatchFish(null);
            Assert.IsNull(catchData);
        }

        [TestMethod]
        public void CatchFishDoesNothingIfNoFishHooked()
        {
            var fisher = FisherData.First();
            fisher.Hooked = null;
            var catchData = System.CatchFish(fisher);
            Assert.IsNull(catchData);
        }

        [TestMethod]
        public void CatchFishUpdatesLeaderboardWhileTournamentActive()
        {
            var fisher = FisherData.First();
            fisher.Hooked = FishData[0];
            var callbackMock = new Mock<LeaderboardEventHandler>();
            System.NewGlobalRecord += callbackMock.Object;
            LeaderboardMock.Data.Clear();
            System.Tournament.StartTournament();
            var catchData = System.CatchFish(fisher);
            Assert.IsNotNull(catchData);
            Assert.AreEqual(1, LeaderboardMock.Data.Count);
            callbackMock.Verify(x => x(It.IsAny<Catch>()), Times.Once);
        }

        [TestMethod]
        public void ProcessHooksFish()
        {
            var fisher = FisherData.First();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            var callbackMock = new Mock<FisherEventHandler>();
            System.FishHooked += callbackMock.Object;
            System.Process(true);
            Assert.IsNotNull(fisher.Hooked);
            callbackMock.Verify(x => x(fisher), Times.Once);
        }

        [TestMethod]
        public void ProcessReleasesFish()
        {
            var fisher = FisherData.First();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now.AddSeconds(-AppSettings.FishingHookLength);
            fisher.Hooked = FishData[0];
            var callbackMock = new Mock<FisherEventHandler>();
            System.FishGotAway += callbackMock.Object;
            System.Process(true);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
            callbackMock.Verify(x => x(fisher), Times.Once);
        }
    }
}

