using LobotJR.Command;
using LobotJR.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LobotJR.Test.Command
{
    [TestClass]
    public abstract class AccessControlBase
    {
        protected CommandManager commandManager;
        protected AccessControl module;
        protected CommandModule commandModule;
        protected TestModule testModule;

        [TestInitialize]
        public void Setup()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole")
                {
                    Users = new List<string>(new string[] { "Foo", "Bar" }),
                    Commands = new List<string>(new string[] { "Command.Foo", "Command.Bar", "Test.*" })
                }
            });
            var rolesJson = JsonConvert.SerializeObject(roles);
            commandManager = new CommandManager(path => rolesJson, (path, data) => { }, path => true);
            commandManager.Initialize("", "");
            commandModule = new CommandModule();
            testModule = new TestModule();
            commandManager.LoadModules(commandModule, testModule);
            module = new AccessControl(commandManager);
        }
    }

    public class CommandModule : ICommandModule
    {
        public string Name => "Command";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands => new CommandHandler[]
        {
            new CommandHandler("Foo", (data, user) => { Calls.Add("Command.Foo"); return null; }, "Foo"),
            new CommandHandler("Bar", (data, user) => { Calls.Add("Command.Bar"); return null; }, "Bar"),
            new CommandHandler("Unrestricted", (data, user) => { Calls.Add("Command.Unrestricted"); return null; }, "Unrestricted")
        };

        public IEnumerable<ICommandModule> SubModules => null;
    }

    public class TestModule : ICommandModule
    {
        public string Name => "Test";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands => new CommandHandler[]
        {
            new CommandHandler("Test", (data, user) => { Calls.Add("Test.Test"); return null; }, "Test")
        };

        public IEnumerable<ICommandModule> SubModules => null;
    }
}
