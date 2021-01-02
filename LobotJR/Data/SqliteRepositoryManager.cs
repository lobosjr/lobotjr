using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;

namespace LobotJR.Data
{
    /// <summary>
    /// Implementation of a repository manager using sqlite storage.
    /// </summary>
    public class SqliteRepositoryManager : IRepositoryManager
    {
        public IRepository<UserRole> UserRoles { get; private set; }
        public IRepository<TournamentResult> TournamentResults { get; private set; }

        public SqliteRepositoryManager(SqliteContext context)
        {
            UserRoles = new SqliteRepository<UserRole>(context);
            TournamentResults = new SqliteRepository<TournamentResult>(context);
        }
    }
}
