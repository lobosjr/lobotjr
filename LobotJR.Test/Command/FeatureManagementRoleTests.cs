using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class FeatureManagementRoleTests : FeatureManagementBase
    {
        [TestMethod]
        public void ListsRoles()
        {
            var command = module.Commands.Where(x => x.Name.Equals("ListRoles")).FirstOrDefault();
            var responses = command.Executor("", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains(commandManager.Roles.Count().ToString())));
            Assert.IsTrue(responses.Any(x => x.Contains("TestRole")));
        }

        [TestMethod]
        public void CreatesANewRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var responses = command.Executor("NewTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(2, commandManager.Roles.Count);
            Assert.IsTrue(commandManager.Roles.Any(x => x.Name.Equals("NewTestRole")));
        }

        [TestMethod]
        public void CreateRoleErrorsOnDuplicateRoleName()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            var responses = command.Executor("TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("Error", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(1, commandManager.Roles.Count);
        }

        [TestMethod]
        public void DescribesRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("TestRole", "").ToArray();
            Assert.AreEqual(2, responses.Count());
            Assert.IsTrue(responses.All(x => x.Contains("TestRole")));
            foreach (var commandString in role.Commands)
            {
                Assert.IsTrue(responses.Any(x => x.Contains(commandString)));
            }
            foreach (var user in role.Users)
            {
                Assert.IsTrue(responses.Any(x => x.Contains(user)));
            }
        }

        [TestMethod]
        public void DescribeRoleErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DescribeRole")).FirstOrDefault();
            var responses = command.Executor("NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeletesARole()
        {
            var add = module.Commands.Where(x => x.Name.Equals("CreateRole")).FirstOrDefault();
            add.Executor("NewTestRole", "");
            Assert.AreEqual(2, commandManager.Roles.Count());
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var responses = command.Executor("NewTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.AreEqual(1, commandManager.Roles.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnDeleteNonEmptyRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var responses = command.Executor("TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void DeleteRoleErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("DeleteRole")).FirstOrDefault();
            var responses = command.Executor("NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}