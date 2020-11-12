using LobotJR.Command;
using LobotJR.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    [TestClass]
    public class CommandManagerTests
    {
        [TestMethod]
        public void LoadModulesLoadsModules()
        {
            var module = new DummyModule();
            var commandManager = new CommandManager(path => "",
                (path, data) => { },
                path => true);
            commandManager.Initialize("", "");
            commandManager.LoadModules(module);
            var commands = commandManager.Commands;
            var firstCommand = module.Commands.First().Name;
            Assert.IsTrue(commands.Count() >= module.Commands.Count());
            Assert.IsFalse(commands.Any(x => x.Equals(firstCommand)));
            Assert.IsTrue(commands.Any(x => x.Equals($"{module.Name}.{firstCommand}")));
        }

        [TestMethod]
        public void InitializeCreatesDefaultRoleIfNoDataFileIsFound()
        {
            var broadcastUser = "Broadcaster";
            var chatUser = "Chatter";
            var writtenJson = "";
            var commandManager = new CommandManager(path => "",
                (path, data) => { writtenJson = data; },
                path => false);
            commandManager.Initialize(broadcastUser, chatUser);
            commandManager.LoadModules(new DummyModule());
            Assert.AreEqual(JsonConvert.SerializeObject(commandManager.Roles), writtenJson);
            Assert.AreEqual(1, commandManager.Roles.Count);
            Assert.AreEqual("Streamer", commandManager.Roles[0].Name);
            Assert.IsTrue(commandManager.Roles[0].Users.Contains(broadcastUser));
            Assert.IsTrue(commandManager.Roles[0].Users.Contains(chatUser));
            Assert.IsTrue(commandManager.Roles[0].Commands.Contains("FeatureManagement.*"));
        }

        [TestMethod]
        public void InitializeLoadsRoleData()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole()
                {
                    Name = "TestRole",
                    Users = new List<string>(new string[] { "User1", "User2", "User3" }),
                    Commands = new List<string>(new string[] { "Command.One", "Test.*" })
                }
            });
            var commandManager = new CommandManager(path => JsonConvert.SerializeObject(roles),
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            Assert.AreEqual(JsonConvert.SerializeObject(roles), JsonConvert.SerializeObject(commandManager.Roles));
        }

        [TestMethod]
        public void IsValidCommandMatchesFullId()
        {
            var module = new DummyModule();
            var firstCommand = module.Commands.First();
            var commandManager = new CommandManager(path => "",
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.{firstCommand.Name}"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcard()
        {
            var module = new DummyModule();
            var commandManager = new CommandManager(path => "",
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.*"));
        }

        [TestMethod]
        public void ProcessMessageExecutesCommands()
        {
            var module = new DummyModule();
            var commandManager = new CommandManager(path => "[]",
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var commandStrings = module.Commands.First().CommandStrings;
            foreach (var command in commandStrings)
            {
                commandManager.ProcessMessage(command, "", out var responses);
            }
            Assert.AreEqual(commandStrings.Count(), module.callCount);
        }

        [TestMethod]
        public void ProcessMessageRestrictsAccessToUnauthorizedUsers()
        {
            var module = new DummyModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole()
                {
                    Name = "TestRole",
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "DummyModule.DummyCommand" })
                }
            });
            var commandManager = new CommandManager(path => JsonConvert.SerializeObject(roles),
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var wasProcessed = commandManager.ProcessMessage(module.Commands.First().CommandStrings.First(), "NotAuth", out var responses);
            Assert.IsFalse(wasProcessed);
        }

        [TestMethod]
        public void ProcessMessageAllowsAccessToAuthorizedUsers()
        {
            var module = new DummyModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole()
                {
                    Name = "TestRole",
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "DummyModule.DummyCommand" })
                }
            });
            var commandManager = new CommandManager(path => JsonConvert.SerializeObject(roles),
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var wasProcessed = commandManager.ProcessMessage(module.Commands.First().CommandStrings.First(), "Auth", out var responses);
            Assert.IsTrue(wasProcessed);
        }

        [TestMethod]
        public void ProcessMessageRestrictsCommandsWithWildcardRoles()
        {
            var module = new DummyModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole()
                {
                    Name = "TestRole",
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "DummyModule.*" })
                }
            });
            var commandManager = new CommandManager(path => JsonConvert.SerializeObject(roles),
                (path, data) => { },
                path => true);
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var wasProcessed = commandManager.ProcessMessage(module.Commands.First().CommandStrings.First(), "NotAuth", out var responses);
            Assert.IsFalse(wasProcessed);
        }

        [TestMethod]
        public void UpdateRolesCallsWriteAction()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole()
                {
                    Name = "TestRole",
                    Users = new List<string>(new string[] { "User1", "User2", "User3" }),
                    Commands = new List<string>(new string[] { "Command.One", "Test.*" })
                }
            });
            var wasCalled = false;
            string calledWith = null;
            var commandManager = new CommandManager(path => "",
                (path, data) => { wasCalled = true; calledWith = data; },
                path => true);
            commandManager.Roles = roles;
            commandManager.UpdateRoles();
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(JsonConvert.SerializeObject(roles), calledWith);
        }
    }

    public class DummyModule : ICommandModule
    {
        public string Name => "DummyModule";

        public int callCount { get; set; }

        public IEnumerable<CommandHandler> Commands => new CommandHandler[]
        {
            new CommandHandler("DummyCommand", (data, user) => { this.callCount++; return null; }, "DummyCommand", "dummy-command")
        };

        public IEnumerable<ICommandModule> SubModules => null;
    }
}
