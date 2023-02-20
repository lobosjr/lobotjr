using LobotJR.Command.Module.Fishing;
using LobotJR.Command.System.Fishing;
using LobotJR.Data;
using LobotJR.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace LobotJR.Test.Modules.Fishing
{
    /// <summary>
    /// Summary description for FishingTests
    /// </summary>
    [TestClass]
    public class FishingAdminModuleTests
    {
        private SqliteRepositoryManager Manager;
        private TournamentSystem TournamentSystem;
        private FishingAdmin AdminModule;

        [TestInitialize]
        public void Initialize()
        {
            Manager = new SqliteRepositoryManager(MockContext.Create());

            var FishingSystem = new FishingSystem(Manager.Users, Manager.FishData, Manager.AppSettings);
            var LeaderboardSystem = new LeaderboardSystem(Manager.Catches, Manager.FishingLeaderboard);

            TournamentSystem = new TournamentSystem(FishingSystem, LeaderboardSystem, Manager.TournamentResults, Manager.AppSettings);
            AdminModule = new FishingAdmin(FishingSystem, TournamentSystem);
        }

        [TestMethod]
        public void DebugTournamentStartsTournament()
        {
            var response = AdminModule.DebugTournament("");
            Assert.IsTrue(response.Processed);
            Assert.IsTrue(TournamentSystem.IsRunning);
        }

        [TestMethod]
        public void DebugCatchCatchesManyFish()
        {
            var response = AdminModule.DebugCatch("");
            Assert.IsTrue(response.Processed);
            Assert.AreEqual(50, response.Debug.Count);
            Assert.IsTrue(response.Debug.Any(x => Manager.FishData.Read().Any(y => x.Contains(y.Name))));
        }
    }
}
