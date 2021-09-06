using LobotJR.Modules.AccessControl;
using LobotJR.Test.Command;

namespace LobotJR.Test.Modules.AccessControl
{
    public abstract class AccessControlAdminBase : CommandManagerTestBase
    {
        protected AccessControlAdmin Module;

        public void InitializeAccessControlModule()
        {
            InitializeCommandManager();
            Module = new AccessControlAdmin(CommandManager);
        }
    }
}
