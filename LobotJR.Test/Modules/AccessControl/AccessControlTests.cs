using LobotJR.Command;
using LobotJR.Modules.AccessControl;
using LobotJR.Test.Command;
using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Modules.AccessControl
{
    [TestClass]
    public abstract class AccessControlTests
    {
        protected CommandManager commandManager;
        protected AccessControlModule module;

        [TestInitialize]
        public void Setup()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Foo", "Bar" }),
                    new List<string>(new string[] { "Command.Foo", "Command.Bar", "Test.*" }))
            });
            commandManager = new CommandManager(new TestRepositoryManager(roles));
            module = new AccessControlModule(commandManager);
            commandManager.LoadModules(module);
        }

        [TestMethod]
        public void AccessControlModuleLoadsAdminSubModule()
        {
            Assert.IsTrue(module.SubModules.Any(x => x is AccessControlAdmin));
        }

        [TestMethod]
        public void ChecksUsersAccessOfSpecificRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var result = command.Executor("TestRole", "Foo");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsFalse(result.Responses.Any(x => x.Contains("not", StringComparison.OrdinalIgnoreCase)));
            result = command.Executor("TestRole", "NewUser");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("not", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void CheckAccessGivesNoRoleMessage()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var username = "NewUser";
            var result = command.Executor(null, username);
            var roles = commandManager.RepositoryManager.UserRoles.Read().Select(x => x.Name);
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsFalse(roles.Any(x => result.Responses.Any(y => y.Contains(x))));
        }

        [TestMethod]
        public void CheckAccessListsAllRoles()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var username = "Foo";
            var result = command.Executor(null, username);
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(commandManager.RepositoryManager.UserRoles
                .Read(x => x.UserIds.Any(y => y.Equals(username)))
                .All(x => result.Responses.Any(y => y.Contains(x.Name))));
        }

        [TestMethod]
        public void ChecksAccessErrorsWithRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var result = command.Executor("NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
