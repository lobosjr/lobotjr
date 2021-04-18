using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
