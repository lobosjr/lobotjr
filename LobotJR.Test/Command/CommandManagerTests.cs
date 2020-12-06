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
            var commandManager = new CommandManager(new TestRepository<UserRole>());
            commandManager.Initialize("", "");
            commandManager.LoadModules(module);
            var commands = commandManager.Commands;
            var firstCommand = module.Commands.First().Name;
            Assert.IsTrue(commands.Count() >= module.Commands.Count());
            Assert.IsFalse(commands.Any(x => x.Equals(firstCommand)));
            Assert.IsTrue(commands.Any(x => x.Equals($"{module.Name}.{firstCommand}")));
        }

        [TestMethod]
        public void InitializeLoadsRoleData()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "User1", "User2", "User3" }),
                    new List<string>(new string[] { "Command.One", "Test.*" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            Assert.AreEqual(JsonConvert.SerializeObject(roles), JsonConvert.SerializeObject(commandManager.Roles.Read()));
        }

        [TestMethod]
        public void IsValidCommandMatchesFullId()
        {
            var module = new CommandModule();
            var firstCommand = module.Commands.First();
            var commandManager = new CommandManager(new TestRepository<UserRole>());
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.{firstCommand.Name}"));
        }

        [TestMethod]
        public void IsValidCommandMatchesWildcard()
        {
            var module = new CommandModule();
            var commandManager = new CommandManager(new TestRepository<UserRole>());
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            Assert.IsTrue(commandManager.IsValidCommand($"{module.Name}.*"));
        }

        [TestMethod]
        public void ProcessMessageExecutesCommands()
        {
            var module = new CommandModule();
            var commandManager = new CommandManager(new TestRepository<UserRole>());
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var commandStrings = module.Commands.First().CommandStrings;
            foreach (var command in commandStrings)
            {
                commandManager.ProcessMessage(command, "");
            }
            Assert.AreEqual(commandStrings.Count(), module.Calls.Count());
        }

        [TestMethod]
        public void ProcessMessageWildcardAllowsAccessToSubModules()
        {
            var module = new CommandModule();
            var subModule = module.SubModules.First() as SubCommandModule;
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.*" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage("Foobar", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(null, result.Errors);
            Assert.IsTrue(subModule.Calls.Any(x => x.Equals("Command.Sub.Foobar")));
        }

        [TestMethod]
        public void ProcessMessageSubModuleAccessDoesNotAllowParentAccess()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.Sub.*" })),
                new UserRole("OtherRole", null, new List<string>(new string[] { "Command.*" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage("Foo", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Errors.Any());
            Assert.IsFalse(module.Calls.Any(x => x.Equals("Command.Foo")));
        }

        [TestMethod]
        public void ProcessMessageAllowsAuthorizedUserWhenWildcardIsRestricted()
        {
            var module = new CommandModule();
            var subModule = module.SubModules.First() as SubCommandModule;
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.Sub.*" })),
                new UserRole("OtherRole", null, new List<string>(new string[] { "Command.*" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage("Foobar", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(null, result.Errors);
            Assert.IsTrue(subModule.Calls.Any(x => x.Equals("Command.Sub.Foobar")));
        }

        [TestMethod]
        public void ProcessMessageRestrictsAccessToUnauthorizedUsers()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.Foo" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage("Foo", "NotAuth");
            Assert.IsTrue(result.Processed);
            Assert.IsTrue(result.Errors.Any());
        }

        [TestMethod]
        public void ProcessMessageAllowsAccessToAuthorizedUsers()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.Foo" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage("Foo", "Auth");
            Assert.IsTrue(result.Processed);
            Assert.AreEqual(null, result.Errors);
        }

        [TestMethod]
        public void ProcessMessageRestrictsCommandsWithWildcardRoles()
        {
            var module = new CommandModule();
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Auth" }),
                    new List<string>(new string[] { "Command.*" }))
            });
            var commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize(null, null);
            commandManager.LoadModules(module);
            var result = commandManager.ProcessMessage(module.Commands.First().CommandStrings.First(), "NotAuth");
            Assert.IsTrue(result.Processed);
            Assert.AreNotEqual(null, result.Errors);
        }
    }
}
