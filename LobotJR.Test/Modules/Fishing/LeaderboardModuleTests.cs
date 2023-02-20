using LobotJR.Command;
using LobotJR.Command.Module;
using LobotJR.Command.Module.Fishing;
using LobotJR.Command.System.Fishing;
using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Test.Mocks;
using LobotJR.Utils;
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
    public class LeaderboardModuleTests
    {
        private SqliteRepositoryManager Manager;

        private FishingSystem FishingSystem;
        private TournamentSystem TournamentSystem;
        private LeaderboardSystem LeaderboardSystem;

        private FishingModule FishingModule;
        private LeaderboardModule LeaderboardModule;

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

            FishingModule = new FishingModule(
                FishingSystem,
                TournamentSystem,
                LeaderboardSystem);
            var userLookup = new UserLookup(Manager.Users, Manager.AppSettings.Read().First());
            LeaderboardModule = new LeaderboardModule(LeaderboardSystem, userLookup);
        }

        [TestMethod]
        public void PushesNotificationOnNewGlobalRecord()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            LeaderboardModule.PushNotification += handlerMock.Object;
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Manager.FishingLeaderboard.Delete(entry);
            }
            Manager.FishingLeaderboard.Commit();
            TournamentSystem.StartTournament();
            var user = Manager.Users.Read().First();
            var userId = user.TwitchId;
            var userName = user.Username;
            var fisher = FishingSystem.GetFisherById(userId);
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            fisher.HookedTime = DateTime.Now;
            FishingModule.CatchFish("", userId);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsNull(result.Responses);
            Assert.IsTrue(result.Messages.Any(x => x.Contains(userName)));
        }

        [TestMethod]
        public void RespondsWithPlayerLeaderboard()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var response = LeaderboardModule.PlayerLeaderboard(null, userId);
            var responses = response.Responses;
            var fishData = Manager.FishData.Read();
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(fishData.Count() + 1, responses.Count);
            foreach (var fish in fishData)
            {
                Assert.IsTrue(responses.Any(x => x.Contains(fish.Name)));
            }
        }

        [TestMethod]
        public void RespondsWithCompactPlayerLeaderboard()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = Manager.Catches.Read(x => x.UserId.Equals(userId));
            var responses = LeaderboardModule.PlayerLeaderboardCompact(null, userId);
            Assert.AreEqual(3, responses.Items.Count());
            var compact = responses.ToCompact();
            foreach (var fish in records)
            {
                Assert.IsTrue(
                    compact.Any(
                        x => x.Contains(fish.Fish.Name)
                        && x.Contains(fish.Length.ToString())
                        && x.Contains(fish.Weight.ToString())
                    )
                );
            }
        }

        [TestMethod]
        public void PlayerLeaderboardUserHasNoFishRecords()
        {
            var fishersWithRecords = Manager.Catches.Read().Select(x => x.UserId).Distinct();
            var users = Manager.Users.Read().Select(x => x.TwitchId);
            var noRecordsFisherId = users.Except(fishersWithRecords).FirstOrDefault();
            var response = LeaderboardModule.PlayerLeaderboard(null, noRecordsFisherId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            var fishNames = Manager.FishData.Read().Select(x => x.Name);
            Assert.AreEqual(0, fishNames.Where(x => responses.Any(y => y.Contains(x))).Count());
        }

        [TestMethod]
        public void PlayerLeaderboardProvidesSpecificFishDetails()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var fish = LeaderboardSystem.GetPersonalLeaderboard(userId).FirstOrDefault();
            var response = LeaderboardModule.PlayerLeaderboard("1", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.IsTrue(responses.Any(x => x.Contains(fish.Fish.Name)));
            Assert.IsTrue(responses.Any(x => x.Contains(fish.Length.ToString())));
            Assert.IsTrue(responses.Any(x => x.Contains(fish.Weight.ToString())));
            Assert.IsTrue(responses.Any(x => x.Contains(fish.Fish.SizeCategory.Name)));
            Assert.IsTrue(responses.Any(x => x.Contains(fish.Fish.FlavorText)));
        }

        [TestMethod]
        public void RespondsWithGlobalLeaderboard()
        {
            var response = LeaderboardModule.GlobalLeaderboard("");
            var responses = response.Responses;
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Assert.IsTrue(
                    responses.Any(
                        x => x.Contains(entry.Fish.Name)
                        && x.Contains(entry.Length.ToString())
                        && x.Contains(entry.Weight.ToString())
                        && x.Contains(Manager.Users.Read(y => y.TwitchId.Equals(entry.UserId)).First().Username)
                    )
                );
            }
        }

        [TestMethod]
        public void RespondsWithCompactGlobalLeaderboard()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var responses = LeaderboardModule.GlobalLeaderboardCompact("", userId);
            var compact = responses.ToCompact();
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Assert.IsTrue(
                    compact.Any(
                        x => x.Contains(entry.Fish.Name)
                        && x.Contains(entry.Length.ToString())
                        && x.Contains(entry.Weight.ToString())
                        && x.Contains(Manager.Users.Read(y => y.TwitchId.Equals(entry.UserId)).First().Username)
                    )
                );
            }
        }

        [TestMethod]
        public void ReleasesSpecificFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var fish = LeaderboardSystem.GetPersonalLeaderboard(userId).FirstOrDefault().Fish;
            var response = LeaderboardModule.ReleaseFish("1", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains(fish.Name));
            Assert.IsFalse(LeaderboardSystem.GetPersonalLeaderboard(userId).Any(x => x.Fish.Id.Equals(fish.Id)));
        }

        [TestMethod]
        public void ReleaseFishWithInvalidIndexCausesError()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var response = LeaderboardModule.ReleaseFish("0", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("doesn't exist", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ReleaseFishWithNonNumericIndexCausesError()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var response = LeaderboardModule.ReleaseFish("a", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ReleaseFishWithNoIndexCausesError()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var fisher = FishingSystem.GetFisherById(userId);
            var response = LeaderboardModule.ReleaseFish("", userId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ReleaseFishWithNoFishTellsPlayerToFish()
        {
            var usersWithFish = Manager.Catches.Read().Select(x => x.UserId).Distinct();
            var userid = Manager.Users.Read(x => !usersWithFish.Contains(x.TwitchId)).First().TwitchId;
            var response = LeaderboardModule.ReleaseFish("1", userid);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
        }
    }
}
