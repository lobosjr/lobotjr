using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Modules;
using LobotJR.Modules.Fishing;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    /// <summary>
    /// Summary description for FishingTests
    /// </summary>
    [TestClass]
    public class FishingModuleTests
    {
        private SqliteRepositoryManager Manager;
        private FishingSystem FishingSystem;
        private TournamentSystem TournamentSystem;
        private LeaderboardSystem LeaderboardSystem;
        private FishingModule FishingModule;

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
            FishingModule = new FishingModule(FishingSystem, TournamentSystem, LeaderboardSystem);
        }

        [TestMethod]
        public void ImportsFishData()
        {
            //Todo: This test is to make sure the flat file data imports correctly. That code is no longer necessary, but does still technically exist.
        }

        [TestMethod]
        public void PushesNotificationOnFishHooked()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            FishingModule.PushNotification += handlerMock.Object;
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            FishingSystem.Process(true);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Responses.Any(x => x.Contains("!catch")));
        }

        [TestMethod]
        public void PushesNotificationOnFishGotAway()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            var appSettings = Manager.AppSettings.Read().First();
            FishingModule.PushNotification += handlerMock.Object;
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            fisher.HookedTime = DateTime.Now.AddSeconds(-appSettings.FishingHookLength);
            FishingSystem.Process(true);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsFalse(result.Responses.Any(x => x.Contains("!catch")));
        }

        [TestMethod]
        public void CancelsCast()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            var response = FishingModule.CancelCast("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsFalse(fisher.IsFishing);
        }

        [TestMethod]
        public void CancelCastFailsIfLineNotCast()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = false;
            var response = FishingModule.CancelCast("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsFalse(fisher.IsFishing);
        }

        [TestMethod]
        public void CatchesFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            TournamentSystem.StartTournament();
            DataUtils.ClearFisherRecords(Manager, userId);
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            fisher.Hooked = Manager.FishData.Read().First();
            var response = FishingModule.CatchFish("", userId);
            var responses = response.Responses;
            var newRecords = LeaderboardSystem.GetPersonalLeaderboard(userId);
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(2, responses.Count);
            Assert.IsTrue(responses.Any(x => x.Contains("biggest")));
            Assert.IsTrue(responses.All(x => x.Contains(newRecords.First().Fish.Name)));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(1, newRecords.Count());
        }

        [TestMethod]
        public void CatchFishFailsIfLineNotCast()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            TournamentSystem.StartTournament();
            DataUtils.ClearFisherRecords(Manager, userId);
            fisher.IsFishing = false;
            fisher.Hooked = null;
            var response = FishingModule.CatchFish("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(0, LeaderboardSystem.GetPersonalLeaderboard(userId).Count());
        }

        [TestMethod]
        public void CatchFishFailsIfNoFishBiting()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            TournamentSystem.StartTournament();
            DataUtils.ClearFisherRecords(Manager, userId);
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            fisher.Hooked = null;
            var response = FishingModule.CatchFish("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cancelcast"));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(0, LeaderboardSystem.GetPersonalLeaderboard(userId).Count());
        }

        [TestMethod]
        public void CastsLine()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = false;
            var response = FishingModule.Cast("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(fisher.IsFishing);
        }

        [TestMethod]
        public void CastLineFailsFailsIfLineAlreadyCast()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            var response = FishingModule.Cast("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("already"));
            Assert.IsFalse(responses[0].Contains("!catch"));
        }

        [TestMethod]
        public void CastLineFailsIfFishIsBiting()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            var response = FishingModule.Cast("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("already"));
            Assert.IsTrue(responses[0].Contains("!catch"));
        }
    }
}
