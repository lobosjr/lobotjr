using LobotJR.Command;
using LobotJR.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobotJR.Test.Command
{
    [TestClass]
    public abstract class AccessControlAdminBase
    {
        protected CommandManager commandManager;
        protected AccessControlAdmin module;
        protected CommandModule commandModule;
        protected TestModule testModule;

        [TestInitialize]
        public void Setup()
        {
            var roles = new List<UserRole>(new UserRole[]
            {
                new UserRole("TestRole",
                    new List<string>(new string[] { "Foo", "Bar" }),
                    new List<string>(new string[] { "Command.Foo", "Command.Bar", "Test.*" }))

            });
            commandManager = new CommandManager(new TestRepository<UserRole>(roles));
            commandManager.Initialize("", "");
            commandModule = new CommandModule();
            testModule = new TestModule();
            commandManager.LoadModules(commandModule, testModule);
            module = new AccessControlAdmin(commandManager);
        }
    }
}
