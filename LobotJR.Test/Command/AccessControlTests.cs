using LobotJR.Command;
using LobotJR.Modules;
using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public abstract class AccessControlTests
    {
        protected CommandManager commandManager;
        protected AccessControl module;
        protected CommandModule commandModule;
        protected TestModule testModule;

        [TestInitialize]
        public void Setup()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Foo", "Bar" }),
                    new List<string>(new string[] { "Command.Foo", "Command.Bar", "Test.*" }))
            });
            commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize("", "");
            commandModule = new CommandModule();
            testModule = new TestModule();
            commandManager.LoadModules(commandModule, testModule);
            module = new AccessControl(commandManager);
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
            var result = command.Executor("", username);
            var roles = commandManager.Roles.Read().Select(x => x.Name);
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsFalse(roles.Any(x => result.Responses.Any(y => y.Contains(x))));
        }

        [TestMethod]
        public void CheckAccessListsAllRoles()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var username = "Foo";
            var result = command.Executor("", username);
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(commandManager.Roles
                .Read(x => x.Users.Any(y => y.Equals(username)))
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
