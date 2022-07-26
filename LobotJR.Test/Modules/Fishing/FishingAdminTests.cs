using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    /// <summary>
    /// Summary description for FishingTests
    /// </summary>
    [TestClass]
    public class FishingAdminTests : FishingTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeFishingModule();
        }

        [TestMethod]
        public void DebugTournamentStartsTournament()
        {
            var response = AdminModule.DebugTournament("");
            Assert.IsTrue(response.Processed);
            Assert.IsTrue(System.Tournament.IsRunning);
        }

        [TestMethod]
        public void DebugCatchCatchesManyFish()
        {
            var response = AdminModule.DebugCatch("");
            Assert.IsTrue(response.Processed);
            Assert.AreNotEqual(0, response.Debug.Count);
            Assert.IsTrue(response.Debug.Any(x => Manager.FishData.Read().Any(y => x.Contains(y.Name))));
        }
    }
}
