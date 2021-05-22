using LobotJR.Data;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Systems.Fishing
{
    [TestClass]
    public class TournamentSystemTests
    {
        private ListRepository<Fisher> Fishers;
        private ListRepository<TournamentResult> TournamentResults;
        private ListRepository<AppSettings> AppSettings;
        private TournamentSystem System;

        [TestInitialize]
        public void Initialize()
        {
            Fishers = new ListRepository<Fisher>();
            TournamentResults = new ListRepository<TournamentResult>();
            AppSettings = new ListRepository<AppSettings>();
            AppSettings.Data.Add(new AppSettings());
            System = new TournamentSystem(Fishers, TournamentResults, AppSettings);
        }

        [TestMethod]
        public void AddsTournamentPoints()
        {
            var fisher = new Fisher() { Id = 0, UserId = "00" };
            Fishers.Data.Add(fisher);
            System.CurrentTournament = new TournamentResult();
            System.CurrentTournament.Entries.Add(new TournamentEntry(fisher.UserId, 10));
            var points = System.AddTournamentPoints(fisher.UserId, 10);
            Assert.AreEqual(20, points);
            Assert.AreEqual(1, System.CurrentTournament.Entries.Count);
            Assert.AreEqual(points, System.CurrentTournament.Entries[0].Points);
        }

        [TestMethod]
        public void AddTournamentPointsAddsUserIfNoEntryExists()
        {
            var fisher = new Fisher() { Id = 0, UserId = "00" };
            System.CurrentTournament = new TournamentResult();
            var points = System.AddTournamentPoints(fisher.UserId, 10);
            Assert.AreEqual(10, points);
            Assert.AreEqual(1, System.CurrentTournament.Entries.Count);
            Assert.AreEqual(fisher.UserId, System.CurrentTournament.Entries[0].UserId);
            Assert.AreEqual(points, System.CurrentTournament.Entries[0].Points);
        }

        [TestMethod]
        public void AddTournamentPointsDoesNothingIfNoTournamentRunning()
        {
            var fisher = new Fisher() { Id = 0, UserId = "00" };
            var points = System.AddTournamentPoints(fisher.UserId, 10);
            Assert.AreEqual(-1, points);
            Assert.IsNull(System.CurrentTournament);
        }

        [TestMethod]
        public void StartsTournament()
        {
            System.StartTournament();
            Assert.IsNotNull(System.CurrentTournament);
            Assert.IsNull(System.NextTournament);
        }

        [TestMethod]
        public void StartTournamentDoesNothingIfTournamentAlreadyRunning()
        {
            var tournament = new TournamentResult() { Id = 123 };
            System.CurrentTournament = tournament;
            System.StartTournament();
            Assert.AreEqual(tournament.Id, System.CurrentTournament.Id);
        }

        [TestMethod]
        public void StartTournamentCancelsFishingUsers()
        {
            var fisher = new Fisher() { Id = 0, UserId = "00", IsFishing = true, Hooked = new Fish(), HookedTime = DateTime.Now };
            Fishers.Data.Add(fisher);
            System.StartTournament();
            Assert.IsNotNull(System.CurrentTournament);
            Assert.IsFalse(fisher.IsFishing);
            Assert.IsNull(fisher.Hooked);
            Assert.IsNull(fisher.HookedTime);
        }

        [TestMethod]
        public void EndsTournament()
        {
            var tournament = new TournamentResult() { Id = 123 };
            System.CurrentTournament = tournament;
            System.NextTournament = null;
            System.EndTournament();
            Assert.IsNull(System.CurrentTournament);
            Assert.IsNotNull(System.NextTournament);
            Assert.IsTrue(TournamentResults.Data.Any(x => x.Id == tournament.Id));
        }

        [TestMethod]
        public void EndTournamentDoesNothingIfNoTournamentRunning()
        {
            System.NextTournament = null;
            System.EndTournament();
            Assert.IsNull(System.CurrentTournament);
            Assert.AreEqual(0, TournamentResults.Data.Count);
        }

        [TestMethod]
        public void ProcessStartsTournamentOnTimer()
        {
            System.NextTournament = DateTime.Now;
            System.Process(true);
            Assert.IsNotNull(System.CurrentTournament);
            Assert.IsNull(System.NextTournament);
        }

        [TestMethod]
        public void ProcessEndsTournamentOnTimer()
        {
            System.CurrentTournament = new TournamentResult()
            {
                Date = DateTime.Now
            };
            System.NextTournament = null;
            System.Process(true);
            Assert.IsNull(System.CurrentTournament);
            Assert.IsNotNull(System.NextTournament);
        }

        [TestMethod]
        public void ProcessCancelsTournamentWhenBroadcastingEnds()
        {
            System.CurrentTournament = new TournamentResult();
            System.Process(false);
            Assert.IsNull(System.CurrentTournament);
            Assert.IsNull(System.NextTournament);
        }
    }
}
