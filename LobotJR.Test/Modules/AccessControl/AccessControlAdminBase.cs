using LobotJR.Command;
using LobotJR.Command.Module;
using LobotJR.Command.Module.AccessControl;
using LobotJR.Data.User;
using LobotJR.Test.Command;

namespace LobotJR.Test.Modules.AccessControl
{
    public abstract class AccessControlAdminBase : CommandManagerTestBase
    {
        protected AccessControlAdmin Module;

        public void InitializeAccessControlModule()
        {
            InitializeCommandManager();
            var userLookup = new UserLookup(RepositoryManagerMock.Object);
            Module = new AccessControlAdmin(Manager, new UserLookup(Manager));
            CommandManager = new CommandManager(new ICommandModule[] { CommandModuleMock.Object, SubCommandModuleMock.Object, Module }, RepositoryManagerMock.Object, userLookup);
            CommandManager.InitializeModules();
        }
    }
}
