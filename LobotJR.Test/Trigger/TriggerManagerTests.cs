using LobotJR.Shared.Client;
using LobotJR.Trigger;
using LobotJR.Trigger.Responder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Wolfcoins;

namespace LobotJR.Test.Trigger
{
    [TestClass]
    public class TriggerManagerTests
    {
        private TriggerManager Manager;
        private Currency Currency;

        [TestInitialize]
        public void Initialize()
        {
            Currency = new Currency(new ClientData());
            Currency.xpList = new Dictionary<string, int>
            {
                { "Level1", Currency.XPForLevel(1) },
                { "Level2", Currency.XPForLevel(2) }
            };
            Currency.subSet = new HashSet<string>
            {
                "Sub"
            };
            Manager = new TriggerManager(new ITriggerResponder[]
            {
                new BlockLinks(Currency),
                new NoceanMan()
            });
        }

        [TestMethod]
        public void TriggerManagerBlocksLinksForNewUsers()
        {
            var response = Manager.ProcessTrigger("butt.ass", "NewUser");
            Assert.IsTrue(response.Processed);
            Assert.IsTrue(response.TimeoutSender);
        }

        [TestMethod]
        public void TriggerManagerBlocksLinksForUsersUnderLevel2()
        {
            var response = Manager.ProcessTrigger("butt.ass", "Level1");
            Assert.IsTrue(response.Processed);
            Assert.IsTrue(response.TimeoutSender);
        }

        [TestMethod]
        public void TriggerManagerAllowsLinksForSubs()
        {
            var response = Manager.ProcessTrigger("butt.ass", "Sub");
            Assert.IsNull(response);
        }

        [TestMethod]
        public void TriggerManagerAllowsLinksForLevel2()
        {
            var response = Manager.ProcessTrigger("butt.ass", "Level2");
            Assert.IsNull(response);
        }
    }
}
