using LobotJR.Command;
using LobotJR.Modules;
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
    public class FishingTests : FishingTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeFishingModule();
        }

        [TestMethod]
        public void PushesNotificationOnFishHooked()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            Module.PushNotification += handlerMock.Object;
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            System.Process(true);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsTrue(result.Responses.Any(x => x.Contains("!catch")));
        }

        [TestMethod]
        public void PushesNotificationOnFishGotAway()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            Module.PushNotification += handlerMock.Object;
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            fisher.HookedTime = DateTime.Now.AddSeconds(-AppSettings.FishingHookLength);
            System.Process(true);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsFalse(result.Responses.Any(x => x.Contains("!catch")));
        }

        [TestMethod]
        public void PushesNotificationOnNewGlobalRecord()
        {
            var handlerMock = new Mock<PushNotificationHandler>();
            Module.PushNotification += handlerMock.Object;
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Manager.FishingLeaderboard.Delete(entry);
            }
            Manager.FishingLeaderboard.Commit();
            System.Tournament.StartTournament();
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            fisher.HookedTime = DateTime.Now;
            Module.CatchFish("", fisher.UserId);
            handlerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<CommandResult>()), Times.Once);
            var result = handlerMock.Invocations[0].Arguments[1] as CommandResult;
            Assert.IsNull(result.Responses);
            Assert.IsTrue(result.Messages.Any(x => x.Contains(UserLookup.GetUsername(fisher.UserId))));
        }

        [TestMethod]
        public void RespondsWithPlayerLeaderboard()
        {
            var fisher = Manager.Fishers.Read().First();
            var response = Module.PlayerLeaderboard(null, fisher.UserId);
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
            var fisher = Manager.Fishers.Read().First();
            var responses = Module.PlayerLeaderboardCompact(null, fisher.UserId);
            Assert.AreEqual(3, responses.Items.Count());
            var compact = responses.ToCompact();
            foreach (var fish in fisher.Records)
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
            var noRecordsFisher = Manager.Fishers.Read(x => x.Records.Count == 0).First();
            var response = Module.PlayerLeaderboard(null, noRecordsFisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            foreach (var fish in Manager.FishData.Read())
            {
                Assert.IsFalse(responses[0].Contains(fish.Name));
            }
        }

        [TestMethod]
        public void PlayerLeaderboardProvidesSpecificFishDetails()
        {
            var fisher = Manager.Fishers.Read().First();
            var fish = fisher.Records[0];
            var response = Module.PlayerLeaderboard("1", fisher.UserId);
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
            var response = Module.GlobalLeaderboard("");
            var responses = response.Responses;
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Assert.IsTrue(
                    responses.Any(
                        x => x.Contains(entry.Fish.Name)
                        && x.Contains(entry.Length.ToString())
                        && x.Contains(entry.Weight.ToString())
                        && x.Contains(UserLookup.GetUsername(entry.UserId))
                    )
                );
            }
        }

        [TestMethod]
        public void RespondsWithCompactGlobalLeaderboard()
        {
            var responses = Module.GlobalLeaderboardCompact("", "00");
            var compact = responses.ToCompact();
            var leaderboard = Manager.FishingLeaderboard.Read();
            foreach (var entry in leaderboard)
            {
                Assert.IsTrue(
                    compact.Any(
                        x => x.Contains(entry.Fish.Name)
                        && x.Contains(entry.Length.ToString())
                        && x.Contains(entry.Weight.ToString())
                        && x.Contains(UserLookup.GetUsername(entry.UserId))
                    )
                );
            }
        }

        [TestMethod]
        public void ReleasesSpecificFish()
        {
            var fisher = Manager.Fishers.Read().First();
            var fish = fisher.Records[0];
            var response = Module.ReleaseFish("1", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains(fish.Fish.Name));
            Assert.IsFalse(fisher.Records.Any(x => x.Fish.Id.Equals(fish.Fish.Id)));
        }

        [TestMethod]
        public void ReleaseFishWithInvalidIdCausesError()
        {
            var fisher = Manager.Fishers.Read().First();
            var response = Module.ReleaseFish("0", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ReleaseFishWithNoFishTellsPlayerToFish()
        {
            var userid = UserLookup.GetId("Buzz");
            var response = Module.ReleaseFish("1", userid);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
        }

        [TestMethod]
        public void CancelsCast()
        {
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            var response = Module.CancelCast("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsFalse(fisher.IsFishing);
        }

        [TestMethod]
        public void CancelCastFailsIfLineNotCast()
        {
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = false;
            var response = Module.CancelCast("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsFalse(fisher.IsFishing);
        }

        [TestMethod]
        public void CatchesFish()
        {
            var fisher = Manager.Fishers.Read().First();
            System.Tournament.StartTournament();
            fisher.Records.Clear();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            fisher.Hooked = Manager.FishData.Read().First();
            var response = Module.CatchFish("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(2, responses.Count);
            Assert.IsTrue(responses.Any(x => x.Contains("biggest")));
            Assert.IsTrue(responses.All(x => x.Contains(fisher.Records[0].Fish.Name)));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(1, fisher.Records.Count);
        }

        [TestMethod]
        public void CatchFishFailsIfLineNotCast()
        {
            var fisher = Manager.Fishers.Read().First();
            System.Tournament.StartTournament();
            fisher.Records.Clear();
            fisher.IsFishing = false;
            fisher.Hooked = null;
            var response = Module.CatchFish("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(0, fisher.Records.Count);
        }

        [TestMethod]
        public void CatchFishFailsIfNoFishBiting()
        {
            var fisher = Manager.Fishers.Read().First();
            System.Tournament.StartTournament();
            fisher.Records.Clear();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            fisher.Hooked = null;
            var response = Module.CatchFish("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cancelcast"));
            Assert.IsFalse(fisher.IsFishing);
            Assert.AreEqual(0, fisher.Records.Count);
        }

        [TestMethod]
        public void CastsLine()
        {
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = false;
            var response = Module.Cast("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(fisher.IsFishing);
        }

        [TestMethod]
        public void CastLineFailsFailsIfLineAlreadyCast()
        {
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            var response = Module.Cast("", fisher.UserId);
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
            var fisher = Manager.Fishers.Read().First();
            fisher.IsFishing = true;
            fisher.Hooked = Manager.FishData.Read().First();
            var response = Module.Cast("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("already"));
            Assert.IsTrue(responses[0].Contains("!catch"));
        }

        [TestMethod]
        public void Gloats()
        {
            var fisher = Manager.Fishers.Read().First();
            var response = Module.Gloat("1", fisher.UserId);
            var responses = response.Responses;
            var messages = response.Messages;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(responses[0].Contains(System.GloatCost.ToString()));
            Assert.IsTrue(responses[0].Contains(fisher.Records[0].Fish.Name));
            Assert.IsTrue(messages[0].Contains(UserLookup.GetUsername(fisher.UserId)));
            Assert.IsTrue(messages[0].Contains(fisher.Records[0].Fish.Name));
            Assert.IsTrue(messages[0].Contains(fisher.Records[0].Length.ToString()));
            Assert.IsTrue(messages[0].Contains(fisher.Records[0].Weight.ToString()));
        }

        [TestMethod]
        public void GloatFailsWithInvalidFish()
        {
            var fisher = Manager.Fishers.Read().First();
            var response = Module.Gloat("", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.IsNull(response.Messages);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void GloatFailsWithNoFish()
        {
            var fisher = Manager.Fishers.Read().First();
            fisher.Records.Clear();
            var response = Module.Gloat("1", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.IsNull(response.Messages);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
        }

        [TestMethod]
        public void GloatFailsWithInsufficientCoins()
        {
            var userid = UserLookup.GetId("Fizz");
            var fisher = Manager.Fishers.Read(x => x.UserId.Equals(userid)).First();
            var response = Module.Gloat("1", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.IsNull(response.Messages);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("coins"));
            Assert.IsFalse(responses[0].Contains("wolfcoins"));
        }
    }
}
