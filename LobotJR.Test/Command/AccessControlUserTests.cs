using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class AccessControlUserTests : AccessControlBase
    {
        [TestMethod]
        public void AddsUserToRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var result = command.Executor("NewUser TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(3, role.Users.Count);
            Assert.IsTrue(role.Users.Contains("NewUser"));
        }

        [TestMethod]
        public void AddUserErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var result = command.Executor("BadInput", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            result = command.Executor(" NoUser", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            result = command.Executor("NoRole ", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddUserErrorsOnInvalidRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var result = command.Executor("NewUser NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddUserErrorsOnExistingAssignment()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var result = command.Executor("Foo TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
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
            var roles = commandManager.Roles.Select(x => x.Name);
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
                .Where(x => x.Users.Any(y => y.Equals(username)))
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

        [TestMethod]
        public void RemovesUserFromRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var result = command.Executor("Foo TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(role.Users.Contains("Foo"));
        }

        [TestMethod]
        public void RemoveUserErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var result = command.Executor("BadInput", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            result = command.Executor(" NoUser", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
            result = command.Executor("NoRole ", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveUserErrorsOnUserNotEnrolled()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var result = command.Executor("NewUser TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveUserErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var result = command.Executor("Foo NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}