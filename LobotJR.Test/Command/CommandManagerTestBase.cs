using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules;
using LobotJR.Test.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    /// <summary>
    /// Class containing mocks for the command manager class. Due to the
    /// complexity of mocking the command manager, this class provides a simple
    /// base to add a mocked out command manager to tests.
    /// 
    /// Due to the number of internal variables exposed, this is presented as
    /// an abstract class to be extended. This removes the need to re-establish
    /// these variables in each test class.
    /// </summary>
    public abstract class CommandManagerTestBase
    {
        protected List<UserRole> UserRoles;
        protected List<UserMap> IdCache;
        protected IEnumerable<CommandHandler> CommandHandlers;
        protected IEnumerable<CommandHandler> SubCommandHandlers;
        protected CommandManager CommandManager;

        protected Dictionary<string, Mock<CommandExecutor>> ExecutorMocks;
        protected Mock<AnonymousExecutor> AnonymousExecutorMock;
        protected Mock<ICommandModule> CommandModuleMock;
        protected Mock<ICommandModule> SubCommandModuleMock;
        protected Mock<IRepositoryManager> RepositoryManagerMock;
        protected Mock<IRepository<UserMap>> UserMapMock;
        protected Mock<IRepository<UserRole>> UserRoleMock;

        /// <summary>
        /// Initializes a command manager object with all internals mocked out.
        /// This allows for testing without regard to the commands actually
        /// implemented, and without any need for sql connections or static
        /// data.
        /// </summary>
        public void InitializeCommandManager()
        {
            ExecutorMocks = new Dictionary<string, Mock<CommandExecutor>>();
            var commands = new string[] { "Foobar", "Foo", "Bar" };
            foreach (var command in commands)
            {
                var executorMock = new Mock<CommandExecutor>();
                executorMock.Setup(x => x(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommandResult(""));
                ExecutorMocks.Add(command, executorMock);
            }
            AnonymousExecutorMock = new Mock<AnonymousExecutor>();
            AnonymousExecutorMock.Setup(x => x(It.IsAny<string>())).Returns(new CommandResult(""));

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
                new CommandHandler("Unrestricted", AnonymousExecutorMock.Object, "Unrestricted")
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
            UserMapMock = new Mock<IRepository<UserMap>>();
            UserMapMock.Setup(x => x.Read()).Returns(IdCache);
            UserMapMock.Setup(x => x.Read(It.IsAny<Func<UserMap, bool>>()))
                .Returns((Func<UserMap, bool> param) => IdCache.Where(param));
            UserMapMock.Setup(x => x.Create(It.IsAny<UserMap>()))
                .Returns((UserMap param) => { IdCache.Add(param); return param; });
            UserMapMock.Setup(x => x.Update(It.IsAny<UserMap>()))
                .Returns((UserMap param) => { IdCache.Remove(IdCache.Where(x => x.Id == param.Id).FirstOrDefault()); IdCache.Add(param); return param; });
            UserMapMock.Setup(x => x.Delete(It.IsAny<UserMap>()))
                .Returns((UserMap param) => { IdCache.Remove(IdCache.Where(x => x.Id == param.Id).FirstOrDefault()); return param; });
            UserRoleMock = new Mock<IRepository<UserRole>>();
            UserRoleMock.Setup(x => x.Read()).Returns(UserRoles);
            UserRoleMock.Setup(x => x.Read(It.IsAny<Func<UserRole, bool>>()))
                .Returns((Func<UserRole, bool> param) => UserRoles.Where(param));
            UserRoleMock.Setup(x => x.Create(It.IsAny<UserRole>()))
                .Returns((UserRole param) => { UserRoles.Add(param); return param; });
            UserRoleMock.Setup(x => x.Update(It.IsAny<UserRole>()))
                .Returns((UserRole param) => { UserRoles.Remove(UserRoles.Where(x => x.Id == param.Id).FirstOrDefault()); UserRoles.Add(param); return param; });
            UserRoleMock.Setup(x => x.Delete(It.IsAny<UserRole>()))
                .Returns((UserRole param) => { UserRoles.Remove(UserRoles.Where(x => x.Id == param.Id).FirstOrDefault()); return param; });
            RepositoryManagerMock = new Mock<IRepositoryManager>();
            RepositoryManagerMock.Setup(x => x.Users).Returns(UserMapMock.Object);
            RepositoryManagerMock.Setup(x => x.UserRoles).Returns(UserRoleMock.Object);
            var appSettings = new ListRepository<AppSettings>();
            appSettings.Data.Add(new AppSettings());
            RepositoryManagerMock.Setup(x => x.AppSettings).Returns(appSettings);
            CommandManager = new CommandManager(RepositoryManagerMock.Object);
            CommandManager.LoadModules(CommandModuleMock.Object);
        }
    }
}
