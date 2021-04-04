using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Modules.AccessControl
{
    [TestClass]
    public class AccessControlAdminRoleTests : AccessControlAdminBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeAccessControlModule();
        }

        [TestMethod]
        public void ListsRoles()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("ListRoles")).FirstOrDefault();
            var result = command.AnonymousExecutor("");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses[0].Contains(UserRoles.Count.ToString()));
            Assert.IsTrue(result.Responses.Any(x => x.Contains("TestRole")));
        }

        [TestMethod]
        public void CreatesANewRole()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("NewTestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(2, CommandManager.RepositoryManager.UserRoles.Read().Count());
            Assert.IsTrue(CommandManager.RepositoryManager.UserRoles.Read().Any(x => x.Name.Equals("NewTestRole")));
        }

        [TestMethod]
        public void CreateRoleErrorsOnDuplicateRoleName()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("TestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("Error", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(1, CommandManager.RepositoryManager.UserRoles.Read().Count());
        }

        [TestMethod]
        public void DescribesRole()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var role = CommandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
            var result = command.AnonymousExecutor("TestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(2, result.Responses.Count());
            Assert.IsTrue(result.Responses.All(x => x.Contains("TestRole")));
            foreach (var commandString in role.Commands)
            {
                Assert.IsTrue(result.Responses.Any(x => x.Contains(commandString)));
            }
            foreach (var user in role.UserIds)
            {
                Assert.IsTrue(result.Responses.Any(x => x.Contains(user)));
            }
        }

        [TestMethod]
        public void DescribeRoleErrorsOnRoleNotFound()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("NotTestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeletesARole()
        {
            var add = Module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            add.AnonymousExecutor("NewTestRole");
            Assert.AreEqual(2, CommandManager.RepositoryManager.UserRoles.Read().Count());
            var command = Module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("NewTestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.AreEqual(1, CommandManager.RepositoryManager.UserRoles.Read().Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnDeleteNonEmptyRole()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("TestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnRoleNotFound()
        {
            var command = Module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.AnonymousExecutor("NotTestRole");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}