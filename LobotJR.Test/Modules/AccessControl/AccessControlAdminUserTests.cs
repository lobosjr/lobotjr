using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Modules.AccessControl
{
    [TestClass]
    public class AccessControlAdminUserTests : AccessControlAdminBase
    {
        [TestMethod]
        public void AddsUserToRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
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
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
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
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
            var result = command.Executor("NewUser NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddUserErrorsOnExistingAssignment()
        {
            var command = module.Commands.Where(x => x.Name.Equals("EnrollUser")).FirstOrDefault();
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
            var result = command.Executor("Foo TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemovesUserFromRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
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
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
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
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
            var result = command.Executor("NewUser TestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveUserErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnenrollUser")).FirstOrDefault();
            var role = commandManager.RepositoryManager.UserRoles.Read().FirstOrDefault();
            var result = command.Executor("Foo NotTestRole", "");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(1, result.Responses.Count());
            Assert.IsTrue(result.Responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }
    }
}