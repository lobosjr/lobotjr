using LobotJR.Command;
using LobotJR.Modules.Fishing;
using LobotJR.Test.Command;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    /// <summary>
    /// Summary description for FishingTests
    /// </summary>
    [TestClass]
    public class FishingTests
    {
        private CommandManager commandManager;
        private FishingModule module;

        [TestInitialize]
        public void Setup()
        {
            var results = new List<TournamentResult>(new TournamentResult[]
            {
                new TournamentResult(DateTime.Now - new TimeSpan(0, 0, 30), new TournamentEntry[] { new TournamentEntry("User", 10), new TournamentEntry("Other", 20), new TournamentEntry("Winner", 30) }),
                new TournamentResult(DateTime.Now - new TimeSpan(0, 30, 30), new TournamentEntry[] { new TournamentEntry("User", 30), new TournamentEntry("Other", 20), new TournamentEntry("Winner", 10) }),
                new TournamentResult(DateTime.Now - new TimeSpan(1, 0, 30), new TournamentEntry[] { new TournamentEntry("User", 40), new TournamentEntry("Other", 20), new TournamentEntry("Winner", 50) }),
                new TournamentResult(DateTime.Now - new TimeSpan(1, 30, 30), new TournamentEntry[] { new TournamentEntry("User", 35), new TournamentEntry("Other", 20), new TournamentEntry("Winner", 10) }),
                new TournamentResult(DateTime.Now - new TimeSpan(2, 0, 30), new TournamentEntry[] { new TournamentEntry("User", 40), new TournamentEntry("Other", 60), new TournamentEntry("Winner", 50) })
            });
            commandManager = new CommandManager(new TestRepositoryManager(results));
            commandManager.Initialize("", "");
            module = new FishingModule(commandManager.RepositoryManager.TournamentResults);
            commandManager.LoadModules(module);
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentWithParticipation()
        {
            var results = commandManager.ProcessMessage("tournament-results", "User");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(3, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("00:00:30")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("Winner") && x.Contains("30")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("3rd") && x.Contains("10")));
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentWithoutParticipation()
        {
            var results = commandManager.ProcessMessage("tournament-results", "NotUser");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("00:00:30")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("Winner") && x.Contains("30")));
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentForWinner()
        {
            var results = commandManager.ProcessMessage("tournament-results", "Winner");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("00:00:30")));
            Assert.IsFalse(results.Responses.Any(x => x.Contains("Winner")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("You") && x.Contains("30")));
        }

        [TestMethod]
        public void TournamentResultsGetsErrorMessageWhenNoTournamentHasCompleted()
        {
            foreach (var result in commandManager.RepositoryManager.TournamentResults.Read())
            {
                commandManager.RepositoryManager.TournamentResults.Delete(result);
            }
            commandManager.RepositoryManager.TournamentResults.Commit();

            var results = commandManager.ProcessMessage("tournament-results", "Winner");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(1, results.Responses.Count);
        }

        [TestMethod]
        public void TournamentResultsCompactGetsLatestTournament()
        {
            var results = commandManager.ProcessMessage("tournament-results -c", "NotUser");
            var resultObject = JsonConvert.DeserializeObject<TournamentResultsResponse>(results.Responses.First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual("Winner", resultObject.Winner);
            Assert.AreEqual(30, resultObject.WinnerPoints);
            Assert.AreEqual(0, resultObject.Rank);
            Assert.AreEqual(0, resultObject.UserPoints);
        }

        [TestMethod]
        public void TournamentResultsCompactIncludesUserData()
        {
            var results = commandManager.ProcessMessage("tournament-results -c", "User");
            var resultObject = JsonConvert.DeserializeObject<TournamentResultsResponse>(results.Responses.First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual("Winner", resultObject.Winner);
            Assert.AreEqual(30, resultObject.WinnerPoints);
            Assert.AreEqual(3, resultObject.Rank);
            Assert.AreEqual(10, resultObject.UserPoints);
        }

        [TestMethod]
        public void TournamentResultsCompactReturnsNullIfNoTournamentsHaveTakenPlace()
        {            
            foreach (var result in commandManager.RepositoryManager.TournamentResults.Read())
            {
                commandManager.RepositoryManager.TournamentResults.Delete(result);
            }
            commandManager.RepositoryManager.TournamentResults.Commit();

            var results = commandManager.ProcessMessage("tournament-results -c", "User");
            Assert.IsNull(results.Responses);
        }

        [TestMethod]
        public void TournamentRecordsGetsUsersRecords()
        {
            var results = commandManager.ProcessMessage("tournament-records", "User");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("1st") && x.Contains("35 points")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("2nd") && x.Contains("40 points")));
        }

        [TestMethod]
        public void TournamentRecordsGetsErrorWhenUserHasNotCompetedInAnyTournaments()
        {
            var results = commandManager.ProcessMessage("tournament-records", "NotUser");
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(1, results.Responses.Count);
        }

        [TestMethod]
        public void TournamentRecordsCompactGetsUserRecords()
        {
            var results = commandManager.ProcessMessage("tournament-records -c", "User");
            var resultObject = JsonConvert.DeserializeObject<TournamentRecordsResponse>(results.Responses.First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual(1, resultObject.TopRank);
            Assert.AreEqual(35, resultObject.TopRankScore);
            Assert.AreEqual(40, resultObject.TopScore);
            Assert.AreEqual(2, resultObject.TopScoreRank);
        }

        [TestMethod]
        public void TournamentRecordsCompactGetsNullIfUserHasNeverEntered()
        {
            var results = commandManager.ProcessMessage("tournament-records -c", "NotUser");
            Assert.IsNull(results.Responses);
        }
    }
}
