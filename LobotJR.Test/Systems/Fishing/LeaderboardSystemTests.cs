using LobotJR.Command.Model.Fishing;
using LobotJR.Command.System.Fishing;
using LobotJR.Data;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using static LobotJR.Command.System.Fishing.LeaderboardSystem;

namespace LobotJR.Test.Systems.Fishing
{
    [TestClass]
    public class LeaderboardSystemTests
    {
        private SqliteRepositoryManager Manager;
        private FishingSystem FishingSystem;
        private TournamentSystem TournamentSystem;
        private LeaderboardSystem LeaderboardSystem;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());

            FishingSystem = new FishingSystem(
                Manager.Users,
                Manager.FishData,
                Manager.AppSettings);
            LeaderboardSystem = new LeaderboardSystem(Manager.Catches, Manager.FishingLeaderboard);
            TournamentSystem = new TournamentSystem(FishingSystem, LeaderboardSystem, Manager.TournamentResults, Manager.AppSettings);
        }

        [TestMethod]
        public void GetsLeaderboard()
        {
            var leaderboard = Manager.FishingLeaderboard.Read();
            var retrieved = LeaderboardSystem.GetLeaderboard();
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(leaderboard.Count(), retrieved.Count());
            for (var i = 0; i < leaderboard.Count(); i++)
            {
                var lEntry = leaderboard.ElementAt(i);
                var rEntry = retrieved.ElementAt(i);
                Assert.IsTrue(lEntry.DeeplyEquals(rEntry));
            }
        }

        [TestMethod]
        public void DeletesFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = LeaderboardSystem.GetPersonalLeaderboard(userId);
            var fish = records.First().FishId;
            LeaderboardSystem.DeleteFish(userId, 0);
            Assert.IsFalse(records.Any(x => x.Fish.Id.Equals(fish)));
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnNegativeIndex()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = LeaderboardSystem.GetPersonalLeaderboard(userId);
            var recordCount = records.Count();
            LeaderboardSystem.DeleteFish(userId, -1);
            Assert.AreEqual(recordCount, records.Count());
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnIndexAboveCount()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = LeaderboardSystem.GetPersonalLeaderboard(userId);
            var recordCount = records.Count();
            LeaderboardSystem.DeleteFish(userId, recordCount);
            Assert.AreEqual(recordCount, records.Count());
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnMissingFisher()
        {
            var recordCount = Manager.Catches.Read().Count();
            LeaderboardSystem.DeleteFish("Invalid Id", 0);
            Assert.AreEqual(recordCount, Manager.Catches.Read().Count());
        }

        [TestMethod]
        public void DeleteFishDoesNothingOnNullFisher()
        {
            var recordCount = Manager.Catches.Read().Count();
            LeaderboardSystem.DeleteFish(null, 0);
            Assert.AreEqual(recordCount, Manager.Catches.Read().Count());
        }

        [TestMethod]
        public void UpdatesPersonalLeaderboardWithNewFishType()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = Manager.Catches.Read(x => x.UserId.Equals(userId));
            DataUtils.ClearFisherRecords(Manager, userId);
            var catchData = new Catch()
            {
                Fish = Manager.FishData.Read().First(),
                UserId = userId,
                Weight = 100
            };
            var result = LeaderboardSystem.UpdatePersonalLeaderboard(userId, catchData);
            var updatedRecords = Manager.Catches.Read(x => x.UserId.Equals(userId));
            Assert.IsTrue(result);
            Assert.AreEqual(1, updatedRecords.Count());
            Assert.AreEqual(catchData.Fish.Id, updatedRecords.ElementAt(0).Fish.Id);
        }

        [TestMethod]
        public void UpdatesPersonalLeaderboardWithExistingFishType()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fish = Manager.FishData.Read().First();
            var existing = LeaderboardSystem.GetUserRecordForFish(userId, fish);
            var catchData = new Catch()
            {
                Fish = fish,
                UserId = userId,
                Weight = existing.Weight + 1
            };
            var result = LeaderboardSystem.UpdatePersonalLeaderboard(userId, catchData);
            Assert.IsTrue(result);
            Assert.AreEqual(catchData.Weight, LeaderboardSystem.GetUserRecordForFish(userId, fish).Weight);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWithNullFisher()
        {
            var result = LeaderboardSystem.UpdatePersonalLeaderboard(null, new Catch());
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWithNullCatchData()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var result = LeaderboardSystem.UpdatePersonalLeaderboard(userId, null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdatePersonalLeaderboardReturnsFalseWhenCatchIsNotNewRecord()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = LeaderboardSystem.GetPersonalLeaderboard(userId);
            var recordCount = records.Count();
            var record = records.FirstOrDefault();
            var catchData = new Catch()
            {
                UserId = userId,
                Fish = record.Fish,
                Weight = record.Weight - 0.01f
            };
            var result = LeaderboardSystem.UpdatePersonalLeaderboard(userId, catchData);
            Assert.IsFalse(result);
            Assert.AreEqual(recordCount, LeaderboardSystem.GetPersonalLeaderboard(userId).Count());
            Assert.AreNotEqual(catchData.Weight, LeaderboardSystem.GetUserRecordForFish(userId, record.Fish).Weight);
        }

        [TestMethod]
        public void UpdatesGlobalLeaderboardWithNewFishType()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fish = Manager.FishData.Read().First();
            var entry = Manager.FishingLeaderboard.Read(x => x.Fish.Id == fish.Id).First();
            Manager.FishingLeaderboard.Delete(entry);
            Manager.FishingLeaderboard.Commit();
            var initialCount = Manager.FishingLeaderboard.Read().Count();
            var catchData = new Catch()
            {
                Fish = Manager.FishData.Read().First(),
                UserId = userId,
                Weight = 100
            };
            var result = LeaderboardSystem.UpdateGlobalLeaderboard(catchData);
            var leaderboard = LeaderboardSystem.GetLeaderboard();
            Assert.IsTrue(result);
            Assert.AreEqual(initialCount + 1, leaderboard.Count());
            Assert.AreEqual(catchData.Weight, leaderboard.First(x => x.Fish.Id == fish.Id).Weight);
        }

        [TestMethod]
        public void UpdatesGlobalLeaderboardWithExistingFishType()
        {
            var fish = Manager.FishData.Read().First();
            var entry = Manager.FishingLeaderboard.Read(x => x.Fish.Id == fish.Id).First();
            var newUser = Manager.Users.Read(x => !x.TwitchId.Equals(entry.UserId)).First();
            var catchData = new Catch()
            {
                Fish = fish,
                UserId = newUser.TwitchId,
                Weight = entry.Weight + 1
            };
            var result = LeaderboardSystem.UpdateGlobalLeaderboard(catchData);
            var leaderboard = LeaderboardSystem.GetLeaderboard();
            Assert.IsTrue(result);
            Assert.AreEqual(catchData.Weight, leaderboard.First().Weight);
            Assert.AreEqual(newUser.TwitchId, leaderboard.First().UserId);
        }

        [TestMethod]
        public void UpdateGlobalLeaderboardReturnsFalseWithNullCatchData()
        {
            var result = LeaderboardSystem.UpdateGlobalLeaderboard(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateGlobalLeaderboardReturnsFalseWhenCatchIsNotNewRecord()
        {
            var fish = Manager.FishData.Read().First();
            var entry = Manager.FishingLeaderboard.Read(x => x.Fish.Id == fish.Id).First();
            var catchData = new Catch()
            {
                Fish = fish,
                Weight = entry.Weight - 1
            };
            var result = LeaderboardSystem.UpdateGlobalLeaderboard(catchData);
            var leaderboard = LeaderboardSystem.GetLeaderboard();
            Assert.IsFalse(result);
            Assert.AreNotEqual(catchData.Weight, leaderboard.First().Weight);
        }

        [TestMethod]
        public void CatchFishUpdatesLeaderboardWhileTournamentActive()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var callbackMock = new Mock<LeaderboardEventHandler>();
            LeaderboardSystem.NewGlobalRecord += callbackMock.Object;
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Manager.FishingLeaderboard.Delete(entry);
            }
            Manager.FishingLeaderboard.Commit();
            TournamentSystem.StartTournament();
            fisher.Hooked = Manager.FishData.Read().First();
            var catchData = FishingSystem.CatchFish(fisher);
            Assert.IsNotNull(catchData);
            Assert.AreEqual(1, Manager.FishingLeaderboard.Read().Count());
            callbackMock.Verify(x => x(It.IsAny<LeaderboardEntry>()), Times.Once);
        }
    }
}

