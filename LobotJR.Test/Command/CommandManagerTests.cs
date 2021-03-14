using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class CommandManagerTests
    {
        private List<UserRole> UserRoles;
        private List<UserMap> IdCache;
        private IEnumerable<CommandHandler> CommandHandlers;
        private IEnumerable<CommandHandler> SubCommandHandlers;
        private CommandManager CommandManager;

        private Mock<ICommandModule> CommandModuleMock;
        private Mock<ICommandModule> SubCommandModuleMock;
        private Mock<IRepositoryManager> RepositoryManagerMock;
        private Mock<IRepository<UserMap>> UserMapMock;
        private Mock<IRepository<UserRole>> UserRoleMock;
        private Dictionary<string, Mock<CommandExecutor>> ExecutorMocks;

        [TestInitialize]
        public void Initialize()
        {
            ExecutorMocks = new Dictionary<string, Mock<CommandExecutor>>();
            var commands = new string[] { "Foobar", "Foo", "Bar", "Unrestricted" };
            foreach (var command in commands)
            {
                var executorMock = new Mock<CommandExecutor>();
                executorMock.Setup(x => x(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommandResult(""));
                ExecutorMocks.Add(command, executorMock);
            }
            SubCommandHandlers = new CommandHandler[]
            {
                new CommandHandler("Foobar", ExecutorMocks["Foobar"].Object, "Foobar"),
            };
            SubCommandModuleMock = new Mock<ICommandModule>();
            SubCommandModuleMock.Setup(x => x.Name).Returns("SubMock");
            SubCommandModuleMock.Setup(x => x.Commands).Returns(SubCommandHandlers);


            CommandHandlers = new CommandHandler[] {
                new CommandHandler("Foo", ExecutorMocks["Foo"].Object, (data, user) =>
                {
                    var items = new string[]
                    {
                        string.IsNullOrWhiteSpace(data) ? "Bar" : data
                    };
                    return new CompactCollection<string>(items, x => $"Foo|{x};");
                }, "Foo"),
                new CommandHandler("Bar", ExecutorMocks["Bar"].Object, "Bar"),
                new CommandHandler("Unrestricted", ExecutorMocks["Unrestricted"].Object, "Unrestricted")
            };
            CommandModuleMock = new Mock<ICommandModule>();
            CommandModuleMock.Setup(x => x.Name).Returns("CommandMock");
            CommandModuleMock.Setup(x => x.Commands).Returns(CommandHandlers);
            CommandModuleMock.Setup(x => x.SubModules).Returns(new ICommandModule[] { SubCommandModuleMock.Object });
            UserRoles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "12345" }),
                    new List<string>(new string[] { "CommandMock.Foo" }))
            });
            IdCache = new List<UserMap>(new UserMap[]
            {
                new UserMap() { Id = "12345", Username = "Auth" },
                new UserMap() { Id = "67890", Username = "NotAuth" }
            });
            RepositoryManagerMock = new Mock<IRepositoryManager>();
            UserMapMock = new Mock<IRepository<UserMap>>();
            UserMapMock.Setup(x => x.Read(It.IsAny<Func<UserMap, bool>>()))
                .Returns((Func<UserMap, bool> param) => IdCache.Where(param));
            RepositoryManagerMock.Setup(x => x.Users).Returns(UserMapMock.Object);
            UserRoleMock = new Mock<IRepository<UserRole>>();
            UserRoleMock.Setup(x => x.Read()).Returns(UserRoles);
            RepositoryManagerMock.Setup(x => x.UserRoles).Returns(UserRoleMock.Object);
            CommandManager = new CommandManager(RepositoryManagerMock.Object);
            CommandManager.LoadModules(CommandModuleMock.Object);
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
        public void IsValidCommandMatchesWildcardAtStart()
        {
            var module = CommandModuleMock.Object;
            Assert.IsTrue(CommandManager.IsValidCommand($"{module.Name}.*"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcardAtEnd()
        {
            var module = CommandModuleMock.Object;
            Assert.IsTrue(CommandManager.IsValidCommand($"*.{module.SubModules.FirstOrDefault().Name}"));
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
            Assert.AreEqual(null, result.Errors);
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
            Assert.AreEqual(null, result.Errors);
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
            Assert.AreEqual(null, result.Errors);
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void ProcessMessageRestrictsCommandsWithWildcardRoles()
        {
            var role = UserRoles.First();
            role.CommandList = "CommandMock.*";
            var module = new TestCommandModule();
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
            Assert.AreEqual(null, result.Errors);
        }

        [TestMethod]
        public void ProcessMessageCompactCommandsPassParameters()
        {
            var result = CommandManager.ProcessMessage("Foo -c value", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(@"Foo: Foo|value;", result.Responses.First());
            Assert.AreEqual(null, result.Errors);
        }

        [TestMethod]
        public void ProcessMessageDoesNotProcessNonAnonymousCommandsForUncachedUsers()
        {
            var result = CommandManager.ProcessMessage("Foo", "Uncached");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Responses.Any());
            ExecutorMocks["Foo"].Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
