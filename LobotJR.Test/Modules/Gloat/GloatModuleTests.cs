using LobotJR.Command.Module.Gloat;
using LobotJR.Command.System.Gloat;
using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Test.Mocks;
using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Modules.Gloat
{
    /// <summary>
    /// Summary description for FishingTests
    /// </summary>
    [TestClass]
    public class FishingModuleTests
    {
        private SqliteRepositoryManager Manager;
        private Dictionary<string, int> Wolfcoins;
        private GloatSystem GloatSystem;
        private GloatModule GloatModule;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());

            var userLookup = new UserLookup(Manager.Users, Manager.AppSettings.Read().First());
            Wolfcoins = new Dictionary<string, int>();
            GloatSystem = new GloatSystem(Manager.Catches, Manager.AppSettings, Wolfcoins);
            GloatModule = new GloatModule(GloatSystem, userLookup);
        }

        [TestMethod]
        public void Gloats()
        {
            var user = Manager.Users.Read().First();
            var userId = user.TwitchId;
            Wolfcoins.Add(userId, GloatSystem.FishingGloatCost);
            var response = GloatModule.GloatFish("1", userId);
            var responses = response.Responses;
            var messages = response.Messages;
            var record = Manager.Catches.Read(x => x.UserId.Equals(userId)).OrderBy(x => x.FishId).First();
            Assert.IsTrue(response.Processed);
            Assert.IsNull(response.Errors);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(responses[0].Contains(GloatSystem.FishingGloatCost.ToString()));
            Assert.IsTrue(responses[0].Contains(record.Fish.Name));
            Assert.IsTrue(messages[0].Contains(user.Username));
            Assert.IsTrue(messages[0].Contains(record.Fish.Name));
            Assert.IsTrue(messages[0].Contains(record.Length.ToString()));
            Assert.IsTrue(messages[0].Contains(record.Weight.ToString()));
        }

        [TestMethod]
        public void GloatFailsWithInvalidFish()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            Wolfcoins.Add(userId, GloatSystem.FishingGloatCost);
            var response = GloatModule.GloatFish("", userId);
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
            var userId = Manager.Users.Read().First().TwitchId;
            Wolfcoins.Add(userId, GloatSystem.FishingGloatCost);
            DataUtils.ClearFisherRecords(Manager, userId);
            var response = GloatModule.GloatFish("1", userId);
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
            var userId = Manager.Users.Read().First().TwitchId;
            var response = GloatModule.GloatFish("1", userId);
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
