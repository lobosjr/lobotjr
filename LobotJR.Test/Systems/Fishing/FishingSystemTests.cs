using LobotJR.Test.Modules.Fishing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LobotJR.Test.Systems.Fishing
{
    [TestClass]
    public class FishingSystemTests : FishingTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeFishingModule();
        }


    }
}

