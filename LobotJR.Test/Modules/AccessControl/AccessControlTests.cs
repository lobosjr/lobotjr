using LobotJR.Modules.AccessControl;
using LobotJR.Test.Command;
using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Modules.AccessControl
{
    [TestClass]
    public class AccessControlTests : CommandManagerTestBase
    {
        protected AccessControlModule Module;

        [TestInitialize]
        public void Setup()
        {
            InitializeCommandManager();
            Module = new AccessControlModule(CommandManager);
        }

        [TestMethod]
        public void AccessControlModuleLoadsAdminSubModule()
        {
            Assert.IsTrue(Module.SubModules.Any(x => x is AccessControlAdmin));
        }

        [TestMethod]
        public void ChecksUsersAccessOfSpecificRole()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var result = command.Executor("TestRole", CommandManager.UserLookup.GetId("Auth"));
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsFalse(result.Responses[0].Contains("not", StringComparison.OrdinalIgnoreCase));
            result = command.Executor("TestRole", CommandManager.UserLookup.GetId("NewUser"));
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses[0].Contains("not", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void CheckAccessGivesNoRoleMessage()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var username = "NewUser";
            var result = command.Executor(null, username);
            var roles = CommandManager.RepositoryManager.UserRoles.Read().Select(x => x.Name);
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsFalse(roles.Any(x => result.Responses.Any(y => y.Contains(x))));
        }

        [TestMethod]
        public void CheckAccessListsAllRoles()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var username = "Auth";
            var result = command.Executor(null, CommandManager.UserLookup.GetId(username));
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(CommandManager.RepositoryManager.UserRoles
                .Read(x => x.UserIds.Any(y => y.Equals(username)))
                .All(x => result.Responses.Any(y => y.Contains(x.Name))));
        }

        [TestMethod]
        public void ChecksAccessErrorsWithRoleNotFound()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var roleToCheck = "NotTestRole";
            var result = command.Executor(roleToCheck, CommandManager.UserLookup.GetId("Auth"));
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses[0].StartsWith("Error:", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(result.Responses[0].Contains(roleToCheck));
        }
    }
}
