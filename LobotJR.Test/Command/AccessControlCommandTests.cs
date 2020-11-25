using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class AccessControlCommandTests : AccessControlBase
    {
        [TestMethod]
        public void AddsCommandToRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("RestrictCommand")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Unrestricted TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(4, role.Commands.Count);
            Assert.IsTrue(role.Commands.Contains("Command.Unrestricted"));
        }

        [TestMethod]
        public void AddCommandErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("RestrictCommand")).FirstOrDefault();
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
        public void AddCommandErrorsOnInvalidRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("RestrictCommand")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Unrestricted NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddCommandErrorsOnInvalidCommand()
        {
            var command = module.Commands.Where(x => x.Name.Equals("RestrictCommand")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Not TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void AddCommandErrorsOnExistingAssignment()
        {
            var command = module.Commands.Where(x => x.Name.Equals("RestrictCommand")).FirstOrDefault();
            var role = commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Foo TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void ListsCommands()
        {
            var command = module.Commands.Where(x => x.Name.Equals("ListCommands")).FirstOrDefault();
            var responses = command.Executor("", "");
            Assert.AreEqual(3, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("4 commands", StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(responses.Any(x => x.Contains("2 modules", StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(responses.Any(
                x => x.Contains(commandModule.Name) &&
                commandModule.Commands.All(y => x.Contains(y.Name))));
            Assert.IsTrue(responses.Any(
                x => x.Contains(testModule.Name) &&
                testModule.Commands.All(y => x.Contains(y.Name))));
        }

        [TestMethod]
        public void RemovesCommandFromRole()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnrestrictCommand")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Foo TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.Contains("success", StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(role.Commands.Contains("Command.Foo"));
        }

        [TestMethod]
        public void RemoveCommandErrorsOnMissingParameters()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnrestrictCommand")).FirstOrDefault();
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
        public void RemoveCommandErrorsOnCommandNotAssigned()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnrestrictCommand")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Unrestricted TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveCommandErrorsOnRoleNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnrestrictCommand")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.Unrestricted NotTestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void RemoveCommandErrorsOnCommandNotFound()
        {
            var command = module.Commands.Where(x => x.Name.Equals("UnrestrictCommand")).FirstOrDefault();
            var role = this.commandManager.Roles.FirstOrDefault();
            var responses = command.Executor("Command.None TestRole", "");
            Assert.AreEqual(1, responses.Count());
            Assert.IsTrue(responses.Any(x => x.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)));
        }

    }
}