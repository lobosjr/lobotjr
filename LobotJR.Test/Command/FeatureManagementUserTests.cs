using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class FeatureManagementUserTests : FeatureManagementBase
    {
        [TestMethod]
        public void AddsUserToRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("NewUser TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(3, role.Users.Count);
            Assert.IsTrue(role.Users.Contains("NewUser"));
        }

        [TestMethod]
        public void AddUserErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("BadInput", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            responses = command.Executor(" NoUser", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            responses = command.Executor("NoRole ", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddUserErrorsOnInvalidRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("NewUser NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddUserErrorsOnExistingAssignment()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Foo TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void ChecksUsersAccess()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var responses = command.Executor("TestRole", "Foo");
            Assert.AreEqual(1, responses.Count());
            Assert.IsFalse(responses.Any(x => x.Contains("not", StringComparison.OrdinalIgnoreCase)));
            responses = command.Executor("TestRole", "NewUser");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("not", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void ChecksUsersAccessErrorsWithNoRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var responses = command.Executor("", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void ChecksUsersAccessErrorsWithRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("CheckAccess")).FirstOrDefault();
            var responses = command.Executor("NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemovesUserFromRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Foo TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(role.Users.Contains("Foo"));
        }

        [TestMethod]
        public void RemoveUserErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("BadInput", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            responses = command.Executor(" NoUser", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            responses = command.Executor("NoRole ", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveUserErrorsOnUserNotEnrolled()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("NewUser TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveUserErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Foo NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}