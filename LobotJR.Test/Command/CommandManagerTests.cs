using LobotJR.Command;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class CommandManagerTests : CommandManagerTestBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeCommandManager();
        }

        [TestMethod]
        public void LoadModulesLoadsModules()
        {
            var commands = CommandManager.Commands;
            var module = CommandModuleMock.Object;
            var firstCommand = module.Commands.First().Name;
            Assert.IsTrue(commands.Count() >= module.Commands.Count());
            Assert.IsFalse(commands.Any(x => x.Equals(firstCommand)));
            Assert.IsTrue(commands.Any(x => x.Equals($"{module.Name}.{firstCommand}")));
        }

        [TestMethod]
        public void InitializeLoadsRoleData()
        {
            var userRolesJson = JsonConvert.SerializeObject(UserRoles);
            var loadedRolesJson = JsonConvert.SerializeObject(CommandManager.RepositoryManager.UserRoles.Read());
            Assert.AreEqual(userRolesJson, loadedRolesJson);
        }

        [TestMethod]
        public void IsValidCommandMatchesFullId()
        {
            var module = CommandModuleMock.Object;
            var firstCommand = module.Commands.First();
            Assert.IsTrue(CommandManager.IsValidCommand($"{module.Name}.{firstCommand.Name}"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcardAtEnd()
        {
            var module = CommandModuleMock.Object;
            Assert.IsTrue(CommandManager.IsValidCommand($"{module.Name}.*"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcardAtStart()
        {
            var module = CommandModuleMock.Object;
            Assert.IsTrue(CommandManager.IsValidCommand($"*.{module.SubModules.FirstOrDefault().Name}.*"));
        }

        [TestMethod]
        public void ProcessMessageExecutesCommands()
        {
            var module = CommandModuleMock.Object;
            var command = module.Commands.First();
            var commandStrings = command.CommandStrings;
            foreach (var commandString in commandStrings)
            {
                CommandManager.ProcessMessage(commandString, "Auth");
            }
            ExecutorMocks[command.Name].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(commandStrings.Count()));
        }

        [TestMethod]
        public void ProcessMessageWildcardAllowsAccessToSubModules()
        {
            var module = CommandModuleMock.Object;
            var role = UserRoles.First();
            role.CommandList = "CommandMock.*";
            role.UserList = "12345";
            var result = CommandManager.ProcessMessage("Foobar", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.IsNull(result.Errors);
            ExecutorMocks["Foobar"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once());
        }

        [TestMethod]
        public void ProcessMessageSubModuleAccessDoesNotAllowParentAccess()
        {
            var module = CommandModuleMock.Object;
            var role = UserRoles.First();
            role.CommandList = "CommandMock.SubMock.*";
            UserRoles.Add(new UserRole("OtherRole", null, new List<string>(new string[] { "CommandMock.*" })));
            var result = CommandManager.ProcessMessage("Foo", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Errors.Any());
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ProcessMessageAllowsAuthorizedUserWhenWildcardIsRestricted()
        {
            var module = CommandModuleMock.Object;
            var role = UserRoles.First();
            role.CommandList = "CommandMock.SubMock.*";
            UserRoles.Add(new UserRole("OtherRole", null, new List<string>(new string[] { "CommandMock.*" })));
            var result = CommandManager.ProcessMessage("Foobar", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.IsNull(result.Errors);
            ExecutorMocks["Foobar"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ProcessMessageRestrictsAccessToUnauthorizedUsers()
        {
            var result = CommandManager.ProcessMessage("Foo", "NotAuth");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Errors.Any());
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ProcessMessageAllowsAccessToAuthorizedUsers()
        {
            var result = CommandManager.ProcessMessage("Foo", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.IsNull(result.Errors);
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void ProcessMessageRestrictsCommandsWithWildcardRoles()
        {
            var role = UserRoles.First();
            role.CommandList = "CommandMock.*";
            var result = CommandManager.ProcessMessage("Foo", "NotAuth");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Errors.Any());
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ProcessMessageProcessesCompactCommands()
        {
            var result = CommandManager.ProcessMessage("Foo -c", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(@"Foo: Foo|Bar;", result.Responses.First());
            Assert.IsNull(result.Errors);
        }

        [TestMethod]
        public void ProcessMessageCompactCommandsPassParameters()
        {
            var result = CommandManager.ProcessMessage("Foo -c value", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(@"Foo: Foo|value;", result.Responses.First());
            Assert.IsNull(result.Errors);
        }

        [TestMethod]
        public void ProcessMessageDoesNotProcessNonAnonymousCommandsForUncachedUsers()
        {
            var result = CommandManager.ProcessMessage("Foo", "Uncached");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Responses.Any());
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ProcessMessageAllowsAnonymousCommandsForUncachedUsers()
        {
            var result = CommandManager.ProcessMessage("Unrestricted", "Uncached");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Responses.Any());
            AnonymousExecutorMock.Verify(x => x(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void ProcessMessageDoesNotAllowRestrictedAnonymousCommandsForUncachedUsers()
        {
            UserRoles[0].AddCommand("CommandMock.Unrestricted");
            var result = CommandManager.ProcessMessage("Unrestricted", "Uncached");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Responses.Any());
            AnonymousExecutorMock.Verify(x => x(It.IsAny<string>()), Times.Never());
        }
    }
}
