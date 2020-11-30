using LobotJR.Command;
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
            var module = new CommandModule();
            var commandManager = new CommandManager(new TestDataAccess());
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
            var dataAccess = new TestDataAccess(null, false);
            var commandManager = new CommandManager(dataAccess);
            commandManager.Initialize(broadcastUser, chatUser);
            Assert.AreEqual(commandManager.Roles, dataAccess.WrittenData);
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
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "User1", "User2", "User3" }),
                    Commands = new List<string>(new string[] { "Command.One", "Test.*" })
                }
            });
            var commandManager = new CommandManager(new TestDataAccess(roles));
            commandManager.Initialize(null, null);
            Assert.AreEqual(JsonConvert.SerializeObject(roles), JsonConvert.SerializeObject(commandManager.Roles));
        }

        [TestMethod]
        public void IsValidCommandMatchesFullId()
        {
            var module = new CommandModule();
            var firstCommand = module.Commands.First();
            var commandManager = new CommandManager(new TestDataAccess());
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.{firstCommand.Name}"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcard()
        {
            var module = new CommandModule();
            var commandManager = new CommandManager(new TestDataAccess());
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.*"));
        }

        [TestMethod]
        public void ProcessMessageExecutesCommands()
        {
            var module = new CommandModule();
            var commandManager = new CommandManager(new TestDataAccess(new List<UserRole>()));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var commandStrings = module.Commands.First().CommandStrings;
            foreach (var command in commandStrings)
            {
                commandManager.ProcessMessage(command, "", out var responses);
            }
            Assert.AreEqual(commandStrings.Count(), module.Calls.Count());
        }

        [TestMethod]
        public void ProcessMessageRestrictsAccessToUnauthorizedUsers()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "Command.Foo" })
                }
            });
            var commandManager = new CommandManager(new TestDataAccess(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var wasProcessed = commandManager.ProcessMessage("Foo", "NotAuth", out var responses);
            Assert.IsFalse(wasProcessed);
        }

        [TestMethod]
        public void ProcessMessageAllowsAccessToAuthorizedUsers()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "Command.Foo" })
                }
            });
            var commandManager = new CommandManager(new TestDataAccess(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var wasProcessed = commandManager.ProcessMessage("Foo", "Auth", out var responses);
            Assert.IsTrue(wasProcessed);
        }

        [TestMethod]
        public void ProcessMessageRestrictsCommandsWithWildcardRoles()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "Auth" }),
                    Commands = new List<string>(new string[] { "Command.*" })
                }
            });
            var commandManager = new CommandManager(new TestDataAccess(roles));
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
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "User1", "User2", "User3" }),
                    Commands = new List<string>(new string[] { "Command.One", "Test.*" })
                }
            });
            var dataAccess = new TestDataAccess(roles);
            var commandManager = new CommandManager(dataAccess);
            commandManager.Roles = roles;
            commandManager.UpdateRoles();
            Assert.IsTrue(dataAccess.WriteCount > 0);
            Assert.AreEqual(roles, dataAccess.WrittenData);
        }
    }
}
