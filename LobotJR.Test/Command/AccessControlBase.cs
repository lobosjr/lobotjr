using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            commandManager = new CommandManager(new TestDataAccess(roles));
            commandManager.Initialize("", "");
            commandModule = new CommandModule();
            testModule = new TestModule();
            commandManager.LoadModules(commandModule, testModule);
            module = new AccessControl(commandManager);
        }
    }

    public class TestDataAccess : IDataAccess<IList<UserRole>>
    {
        private IList<UserRole> _content;
        private bool _exists;

        public int ReadCount { get; private set; }
        public int WriteCount { get; private set; }
        public IList<UserRole> WrittenData { get; private set; }
        public TestDataAccess(IList<UserRole> content = null, bool exists = true)
        {
            _content = content;
            _exists = exists;
        }

        public bool Exists(string source)
        {
            return _exists;
        }

        public IList<UserRole> ReadData(string source)
        {
            ReadCount++;
            return _content;
        }

        public void WriteData(string source, IList<UserRole> content)
        {
            WriteCount++;
            WrittenData = content;
        }
    }

    public class CommandModule : ICommandModule
    {
        public string Name => "Command";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands => new CommandHandler[]
        {
            new CommandHandler("Foo", (data, user) => { Calls.Add("Command.Foo"); return new CommandResult(""); }, "Foo"),
            new CommandHandler("Bar", (data, user) => { Calls.Add("Command.Bar"); return new CommandResult(""); }, "Bar"),
            new CommandHandler("Unrestricted", (data, user) => { Calls.Add("Command.Unrestricted"); return new CommandResult(""); }, "Unrestricted")
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
