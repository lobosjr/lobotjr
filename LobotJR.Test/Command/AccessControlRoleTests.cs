using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class AccessControlRoleTests : AccessControlBase
    {
        [TestMethod]
        public void ListsRoles()
        {
            var command = module.Commands.Where(x => x.Name.Equals("ListRoles")).FirstOrDefault();
            var result = command.Executor("", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains(commandManager.Roles.Count().ToString())));
            Assert.IsTrue(result.Responses.Any(x => x.Contains("TestRole")));
        }

        [TestMethod]
        public void CreatesANewRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var result = command.Executor("NewTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(2, commandManager.Roles.Count);
            Assert.IsTrue(commandManager.Roles.Any(x => x.Name.Equals("NewTestRole")));
        }

        [TestMethod]
        public void CreateRoleErrorsOnDuplicateRoleName()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var result = command.Executor("TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("Error", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(1, commandManager.Roles.Count);
        }

        [TestMethod]
        public void DescribesRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var result = command.Executor("TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(2, result.Responses.Count());
            Assert.IsTrue(result.Responses.All(x => x.Contains("TestRole")));
            foreach (var commandString in role.Commands)
            {
                Assert.IsTrue(result.Responses.Any(x => x.Contains(commandString)));
            }
            foreach (var user in role.Users)
            {
                Assert.IsTrue(result.Responses.Any(x => x.Contains(user)));
            }
        }

        [TestMethod]
        public void DescribeRoleErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var result = command.Executor("NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeletesARole()
        {
            var add = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            add.Executor("NewTestRole", "");
            Assert.AreEqual(2, commandManager.Roles.Count());
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.Executor("NewTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.AreEqual(1, commandManager.Roles.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnDeleteNonEmptyRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.Executor("TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var result = command.Executor("NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}