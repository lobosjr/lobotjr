using LobotJR.Command.Model.Fishing;
using LobotJR.Command.System.Fishing;
using LobotJR.Data;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using static LobotJR.Command.System.Fishing.TournamentSystem;

namespace LobotJR.Test.TournamentSystems.Fishing
{
    [TestClass]
    public class TournamentSystemTests
    {
        private SqliteRepositoryManager Manager;
        private FishingSystem FishingSystem;
        private TournamentSystem TournamentSystem;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());


            FishingSystem = new FishingSystem(Manager, Manager);
            var leaderboardSystem = new LeaderboardSystem(Manager);
            TournamentSystem = new TournamentSystem(FishingSystem, leaderboardSystem, Manager);
        }

        [TestMethod]
        public void AddsTournamentPoints()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            TournamentSystem.CurrentTournament = new TournamentResult();
            TournamentSystem.CurrentTournament.Entries.Add(new TournamentEntry(userId, 10));
            var points = TournamentSystem.AddTournamentPoints(userId, 10);
            Assert.AreEqual(20, points);
            Assert.AreEqual(1, TournamentSystem.CurrentTournament.Entries.Count);
            Assert.AreEqual(points, TournamentSystem.CurrentTournament.Entries[0].Points);
        }

        [TestMethod]
        public void AddTournamentPointsAddsUserIfNoEntryExists()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            TournamentSystem.CurrentTournament = new TournamentResult();
            var points = TournamentSystem.AddTournamentPoints(userId, 10);
            Assert.AreEqual(10, points);
            Assert.AreEqual(1, TournamentSystem.CurrentTournament.Entries.Count);
            Assert.AreEqual(userId, TournamentSystem.CurrentTournament.Entries[0].UserId);
            Assert.AreEqual(points, TournamentSystem.CurrentTournament.Entries[0].Points);
        }

        [TestMethod]
        public void AddTournamentPointsDoesNothingIfNoTournamentRunning()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var points = TournamentSystem.AddTournamentPoints(userId, 10);
            Assert.AreEqual(-1, points);
            Assert.IsNull(TournamentSystem.CurrentTournament);
        }

        [TestMethod]
        public void StartsTournament()
        {
            var callbackMock = new Mock<TournamentStartHandler>();
            TournamentSystem.TournamentStarted += callbackMock.Object;
            TournamentSystem.StartTournament();
            Assert.IsNotNull(TournamentSystem.CurrentTournament);
            Assert.IsNull(TournamentSystem.NextTournament);
            callbackMock.Verify(x => x(It.IsAny<DateTime>()), Times.Once);
        }

        [TestMethod]
        public void StartTournamentDoesNothingIfTournamentAlreadyRunning()
        {
            var tournament = new TournamentResult() { Id = 123 };
            TournamentSystem.CurrentTournament = tournament;
            TournamentSystem.StartTournament();
            Assert.AreEqual(tournament.Id, TournamentSystem.CurrentTournament.Id);
        }

        [TestMethod]
        public void StartTournamentCancelsFishingUsers()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            FishingSystem.Cast(userId);
            FishingSystem.HookFish(fisher);
            TournamentSystem.StartTournament();
            Assert.IsNotNull(TournamentSystem.CurrentTournament);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
        }

        [TestMethod]
        public void StartTournamentUpdatesCastTimes()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var appSettings = Manager.AppSettings.Read().First();
            fisher.IsFishing = false;
            TournamentSystem.StartTournament();
            FishingSystem.Cast(fisher.UserId);
            Assert.IsTrue(fisher.IsFishing);
            Assert.IsTrue(fisher.HookedTime >= DateTime.Now.AddSeconds(appSettings.FishingTournamentCastMinimum));
            Assert.IsTrue(fisher.HookedTime <= DateTime.Now.AddSeconds(appSettings.FishingTournamentCastMaximum));
        }

        [TestMethod]
        public void EndsTournament()
        {
            var tournament = new TournamentResult() { Id = 123 };
            var callbackMock = new Mock<TournamentEndHandler>();
            TournamentSystem.TournamentEnded += callbackMock.Object;
            TournamentSystem.CurrentTournament = tournament;
            TournamentSystem.NextTournament = null;
            TournamentSystem.EndTournament(true);
            Assert.IsNull(TournamentSystem.CurrentTournament);
            Assert.IsNotNull(TournamentSystem.NextTournament);
            Assert.IsTrue(Manager.TournamentResults.Read(x => x.Id == tournament.Id).Any());
            callbackMock.Verify(x => x(It.IsAny<TournamentResult>(), It.IsAny<DateTime>()));
        }

        [TestMethod]
        public void EndTournamentDoesNothingIfNoTournamentRunning()
        {
            var resultCount = Manager.TournamentResults.Read().Count();
            TournamentSystem.NextTournament = null;
            TournamentSystem.EndTournament(true);
            Assert.IsNull(TournamentSystem.CurrentTournament);
            Assert.AreEqual(resultCount, Manager.TournamentResults.Read().Count());
        }

        [TestMethod]
        public void EndTournamentResetsCastTimes()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var appSettings = Manager.AppSettings.Read().First();
            fisher.IsFishing = false;
            TournamentSystem.StartTournament();
            TournamentSystem.EndTournament(true);
            FishingSystem.Cast(fisher.UserId);
            Assert.IsTrue(fisher.IsFishing);
            Assert.IsTrue(fisher.HookedTime >= DateTime.Now.AddSeconds(appSettings.FishingCastMinimum));
            Assert.IsTrue(fisher.HookedTime <= DateTime.Now.AddSeconds(appSettings.FishingCastMaximum));
        }

        [TestMethod]
        public void GetsTournamentResults()
        {
            var expectedResults = Manager.TournamentResults.Read().OrderByDescending(x => x.Date).First();
            var actualResults = TournamentSystem.GetLatestResults();
            Assert.AreEqual(expectedResults.Id, actualResults.Id);
        }

        [TestMethod]
        public void GetResultForUser()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var expectedResults = Manager.TournamentResults.Read(x => x.Entries.Any(y => y.UserId.Equals(userId)));
            var actualResults = TournamentSystem.GetResultsForUser(userId);
            Assert.AreEqual(expectedResults.Count(), actualResults.Count());
        }

        [TestMethod]
        public void ProcessStartsTournamentOnTimer()
        {
            TournamentSystem.NextTournament = DateTime.Now;
            TournamentSystem.Process(true);
            Assert.IsNotNull(TournamentSystem.CurrentTournament);
            Assert.IsNull(TournamentSystem.NextTournament);
        }

        [TestMethod]
        public void ProcessEndsTournamentOnTimer()
        {
            TournamentSystem.CurrentTournament = new TournamentResult()
            {
                Date = DateTime.Now
            };
            TournamentSystem.NextTournament = null;
            TournamentSystem.Process(true);
            Assert.IsNull(TournamentSystem.CurrentTournament);
            Assert.IsNotNull(TournamentSystem.NextTournament);
        }

        [TestMethod]
        public void ProcessCancelsTournamentWhenBroadcastingEnds()
        {
            TournamentSystem.CurrentTournament = new TournamentResult();
            TournamentSystem.Process(false);
            Assert.IsNull(TournamentSystem.CurrentTournament);
            Assert.IsNull(TournamentSystem.NextTournament);
        }
    }
}
