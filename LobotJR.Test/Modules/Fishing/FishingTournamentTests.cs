using LobotJR.Command;
using LobotJR.Modules;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    [TestClass]
    public class FishingTournamentTests : FishingTestBase
    {
        private TournamentResultsResponse ResultsFromCompact(string compact)
        {
            var data = compact.Substring(0, compact.Length - 1).Split('|').ToArray();
            return new TournamentResultsResponse()
            {
                Ended = DateTime.Parse(data[0]),
                Participants = int.Parse(data[1]),
                Winner = data[2],
                WinnerPoints = int.Parse(data[3]),
                Rank = int.Parse(data[4]),
                UserPoints = int.Parse(data[5])
            };
        }

        private TournamentRecordsResponse RecordsFromCompact(string compact)
        {
            var data = compact.Substring(0, compact.Length - 1).Split('|').ToArray();
            return new TournamentRecordsResponse()
            {
                TopRank = int.Parse(data[0]),
                TopRankScore = int.Parse(data[1]),
                TopScore = int.Parse(data[2]),
                TopScoreRank = int.Parse(data[3])
            };
        }

        private void ClearTournaments()
        {
            var tournamentResults = Manager.TournamentResults.Read();
            foreach (var entry in tournamentResults)
            {
                Manager.TournamentResults.Delete(entry);
            }
            Manager.TournamentResults.Commit();
        }

        [TestInitialize]
        public void Setup()
        {
            InitializeFishingModule();
        }

        [TestMethod]
        public void PushesNotificationOnTournamentStart()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            TournamentModule.PushNotification += handlerMock.Object;
            System.Tournament.StartTournament();
            handlerMock.Verify(x => x(null, It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Messages.Any(x => x.Contains("!cast")));
        }

        [TestMethod]
        public void CalculatesResultsOnTournamentEnd()
        {
            var firstFisher = Manager.Fishers.Read().First();
            var secondFisher = Manager.Fishers.Read(x => !x.UserId.Equals(firstFisher.UserId)).First();
            System.Tournament.StartTournament();
            System.Tournament.CurrentTournament.Entries.Add(new TournamentEntry(firstFisher.UserId, 100));
            System.Tournament.CurrentTournament.Entries.Add(new TournamentEntry(secondFisher.UserId, 200));
            System.Tournament.EndTournament(true);
            var results = Manager.TournamentResults.Read().OrderByDescending(x => x.Date).First();
            Assert.AreEqual(secondFisher.UserId, results.Winner.UserId);
        }

        [TestMethod]
        public void PushesNotificationOnTournamentEnd()
        {
            var user = Manager.Users.Read().First();
            var handlerMock = new Mock<PushNotificationHandler>();
            System.Tournament.StartTournament();
            TournamentModule.PushNotification += handlerMock.Object;
            System.Tournament.CurrentTournament.Entries.Add(new TournamentEntry(user.TwitchId, 100));
            System.Tournament.EndTournament(true);
            handlerMock.Verify(x => x(null, It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Messages.Any(x => x.Contains("end")));
            Assert.IsTrue(result.Messages.Any(x => x.Contains(user.Username)));
        }

        [TestMethod]
        public void PushesNotificationOnTournamentEndWithNoParticipants()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            System.Tournament.StartTournament();
            TournamentModule.PushNotification += handlerMock.Object;
            System.Tournament.EndTournament(true);
            handlerMock.Verify(x => x(null, It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Messages.Any(x => x.Contains("end")));
            Assert.IsFalse(result.Messages.Any(x => x.Contains("participants", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void PushesNotificationOnTournamentEndByStreamStopping()
        {
            var user = Manager.Users.Read().First();
            var handlerMock = new Mock<PushNotificationHandler>();
            System.Tournament.StartTournament();
            TournamentModule.PushNotification += handlerMock.Object;
            System.Tournament.CurrentTournament.Entries.Add(new TournamentEntry(user.TwitchId, 100));
            System.Tournament.EndTournament(false);
            handlerMock.Verify(x => x(null, It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Messages.Any(x => x.Contains("offline")));
            Assert.IsTrue(result.Messages.Any(x => x.Contains(user.Username, StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void PushesNotificationOnTournamentEndByStreamStoppingWithNoParticipants()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            System.Tournament.StartTournament();
            TournamentModule.PushNotification += handlerMock.Object;
            System.Tournament.EndTournament(false);
            handlerMock.Verify(x => x(null, It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Messages.Any(x => x.Contains("offline")));
            Assert.IsFalse(result.Messages.Any(x => x.Contains("winner", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentWithParticipation()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Foo"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(3, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("30 seconds")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("12") && x.Contains("30")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("3rd") && x.Contains("10")));
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentWithoutParticipation()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Buzz"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("30 seconds")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("12") && x.Contains("30")));
        }

        [TestMethod]
        public void TournamentResultsGetsLatestTournamentForWinner()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Fizz"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("30 seconds")));
            Assert.IsFalse(results.Responses.Any(x => x.Contains("Fizz")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("You") && x.Contains("30")));
        }

        [TestMethod]
        public void TournamentResultsGetsErrorMessageWhenNoTournamentHasCompleted()
        {
            ClearTournaments();
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Foo"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(1, results.Responses.Count);
        }

        [TestMethod]
        public void TournamentResultsCompactGetsLatestTournament()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.CompactExecutor("", UserLookup.GetId("Buzz"));
            var resultObject = ResultsFromCompact(results.ToCompact().First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual(UserLookup.GetId("Fizz"), resultObject.Winner);
            Assert.AreEqual(3, resultObject.Participants);
            Assert.AreEqual(30, resultObject.WinnerPoints);
            Assert.AreEqual(0, resultObject.Rank);
            Assert.AreEqual(0, resultObject.UserPoints);
        }

        [TestMethod]
        public void TournamentResultsCompactIncludesUserData()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.CompactExecutor("", UserLookup.GetId("Foo"));
            var resultObject = ResultsFromCompact(results.ToCompact().First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual(UserLookup.GetId("Fizz"), resultObject.Winner);
            Assert.AreEqual(3, resultObject.Participants);
            Assert.AreEqual(30, resultObject.WinnerPoints);
            Assert.AreEqual(3, resultObject.Rank);
            Assert.AreEqual(10, resultObject.UserPoints);
        }

        [TestMethod]
        public void TournamentResultsCompactReturnsNullIfNoTournamentsHaveTakenPlace()
        {
            ClearTournaments();
            var leftovers = Manager.TournamentResults.Read().ToArray();
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentResults")).FirstOrDefault();
            var results = command.CompactExecutor("", UserLookup.GetId("Buzz"));
            Assert.IsNull(results);
        }

        [TestMethod]
        public void TournamentRecordsGetsUsersRecords()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentRecords")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Foo"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(2, results.Responses.Count);
            Assert.IsTrue(results.Responses.Any(x => x.Contains("1st") && x.Contains("35 points")));
            Assert.IsTrue(results.Responses.Any(x => x.Contains("2nd") && x.Contains("40 points")));
        }

        [TestMethod]
        public void TournamentRecordsGetsErrorWhenUserHasNotCompetedInAnyTournaments()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentRecords")).FirstOrDefault();
            var results = command.Executor("", UserLookup.GetId("Buzz"));
            Assert.IsNotNull(results.Responses);
            Assert.AreEqual(1, results.Responses.Count);
        }

        [TestMethod]
        public void TournamentRecordsCompactGetsUserRecords()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentRecords")).FirstOrDefault();
            var results = command.CompactExecutor("", UserLookup.GetId("Foo"));
            var resultObject = RecordsFromCompact(results.ToCompact().First());
            Assert.IsNotNull(resultObject);
            Assert.AreEqual(1, resultObject.TopRank);
            Assert.AreEqual(35, resultObject.TopRankScore);
            Assert.AreEqual(40, resultObject.TopScore);
            Assert.AreEqual(2, resultObject.TopScoreRank);
        }

        [TestMethod]
        public void TournamentRecordsCompactGetsNullIfUserHasNeverEntered()
        {
            var command = TournamentModule.Commands.Where(x => x.Name.Equals("TournamentRecords")).FirstOrDefault();
            var results = command.CompactExecutor("", UserLookup.GetId("Buzz"));
            Assert.IsNull(results);
        }
    }
}
