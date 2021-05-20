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
            var fisher = FisherData.FirstOrDefault(x => x.UserId.Equals("00"));
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId.Equals("00"));
            fisher.IsFishing = true;
            fisher.Hooked = FishData[0];
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
            LeaderboardMock.Data.Clear();
            System.Tournament.StartTournament();
            var fisher = FisherData.FirstOrDefault(x => x.UserId.Equals("00"));
            fisher.IsFishing = true;
            fisher.Hooked = FishData[0];
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
            var response = Module.PlayerLeaderboard(null, "00");
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(FishData.Count + 1, responses.Count);
            foreach (var fish in FishData)
            {
                Assert.IsTrue(responses.Any(x => x.Contains(fish.Name)));
            }
        }

        [TestMethod]
        public void RespondsWithCompactPlayerLeaderboard()
        {
            var fisher = FisherData.FirstOrDefault(x => x.UserId.Equals("00"));
            var responses = Module.PlayerLeaderboardCompact(null, fisher.UserId);
            Assert.AreEqual(2, responses.Items.Count());
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
            var noRecordsFisher = FisherData.FirstOrDefault(x => x.Records.Count == 0);
            var response = Module.PlayerLeaderboard(null, noRecordsFisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            foreach (var fish in FishData)
            {
                Assert.IsFalse(responses[0].Contains(fish.Name));
            }
        }

        [TestMethod]
        public void PlayerLeaderboardProvidesSpecificFishDetails()
        {
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var leaderboard = LeaderboardMock.Read();
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
            var leaderboard = LeaderboardMock.Read();
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
            var fish = fisher.Records[0];
            var response = Module.ReleaseFish("1", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains(fish.Fish.Name));
        }

        [TestMethod]
        public void ReleaseFishWithInvalidIdCausesError()
        {
            var response = Module.ReleaseFish("0", "00");
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ReleaseFishWithNoFishTellsPlayerToFish()
        {
            var response = Module.ReleaseFish("1", "03");
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("!cast"));
        }

        [TestMethod]
        public void CancelsCast()
        {
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
            System.Tournament.StartTournament();
            fisher.Records.Clear();
            fisher.IsFishing = true;
            fisher.HookedTime = DateTime.Now;
            fisher.Hooked = FishData[0];
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
            fisher.IsFishing = true;
            fisher.Hooked = FishData[0];
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "00");
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
            var fisher = FisherData.FirstOrDefault(x => x.UserId == "01");
            var response = Module.Gloat("1", fisher.UserId);
            var responses = response.Responses;
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.IsNull(response.Messages);
            Assert.AreEqual(1, responses.Count);
            Assert.IsTrue(responses[0].Contains("coins"));
        }
    }
}
