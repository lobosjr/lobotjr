using LobotJR.Command.System.Gloat;
using LobotJR.Data;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Systems.Gloat
{
    [TestClass]
    public class GloatSystemTests
    {
        private SqliteRepositoryManager Manager;
        private GloatSystem GloatSystem;
        private Dictionary<string, int> Wolfcoins;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());
            Wolfcoins = new Dictionary<string, int>();

            GloatSystem = new GloatSystem(Manager.Catches, Manager.AppSettings, Wolfcoins);
        }

        [TestMethod]
        public void CanGloatReturnsTrueWithEnoughCoins()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            Wolfcoins.Add(userId, Manager.AppSettings.Read().First().FishingGloatCost);
            var canGloat = GloatSystem.CanGloatFishing(userId);
            Assert.IsTrue(canGloat);
        }

        [TestMethod]
        public void CanGloatReturnsFalseWithoutEnoughCoins()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            Wolfcoins.Add(userId, Manager.AppSettings.Read().First().FishingGloatCost - 1);
            var canGloat = GloatSystem.CanGloatFishing(userId);
            Assert.IsFalse(canGloat);
        }

        [TestMethod]
        public void CanGloatReturnsFalseWithNoCoinEntry()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var canGloat = GloatSystem.CanGloatFishing(userId);
            Assert.IsFalse(canGloat);
        }

        [TestMethod]
        public void GloatReturnsCorrectRecordAndRemovesCoins()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var expectedFish = Manager.Catches.Read(x => x.UserId.Equals(userId)).OrderBy(x => x.FishId).First();
            Wolfcoins.Add(userId, Manager.AppSettings.Read().First().FishingGloatCost);
            var gloat = GloatSystem.FishingGloat(userId, 0);
            Assert.AreEqual(0, Wolfcoins[userId]);
            Assert.AreEqual(expectedFish.FishId, gloat.FishId);
        }

        [TestMethod]
        public void GloatReturnsNullWithNoRecords()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var records = Manager.Catches.Read(x => x.UserId.Equals(userId)).ToList();
            foreach (var record in records)
            {
                Manager.Catches.Delete(record);
            }
            Manager.Catches.Commit();
            var cost = Manager.AppSettings.Read().First().FishingGloatCost;
            Wolfcoins.Add(userId, cost);
            var gloat = GloatSystem.FishingGloat(userId, 0);
            Assert.AreEqual(cost, Wolfcoins[userId]);
            Assert.IsNull(gloat);
        }

        [TestMethod]
        public void GloatReturnsNullWithNegativeIndex()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var cost = Manager.AppSettings.Read().First().FishingGloatCost;
            Wolfcoins.Add(userId, cost);
            var gloat = GloatSystem.FishingGloat(userId, -1);
            Assert.AreEqual(cost, Wolfcoins[userId]);
            Assert.IsNull(gloat);
        }

        [TestMethod]
        public void GloatReturnsNullWithTooHighIndex()
        {
            var userId = Manager.Users.Read().First().TwitchId;
            var cost = Manager.AppSettings.Read().First().FishingGloatCost;
            var recordCount = Manager.Catches.Read(x => x.UserId.Equals(userId)).Count();
            Wolfcoins.Add(userId, cost);
            var gloat = GloatSystem.FishingGloat(userId, recordCount);
            Assert.AreEqual(cost, Wolfcoins[userId]);
            Assert.IsNull(gloat);
        }
    }
}
