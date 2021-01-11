using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;

namespace LobotJR.Data
{
    /// <summary>
    /// Implementation of a repository manager using sqlite storage.
    /// </summary>
    public class SqliteRepositoryManager : IRepositoryManager, IContentManager
    {
        public IRepository<AppSettings> AppSettings { get; private set; }
        public IRepository<UserRole> UserRoles { get; private set; }
        public IRepository<Fisher> Fishers { get; }
        public IRepository<Catch> FishingLeaderboard { get; }
        public IRepository<TournamentResult> TournamentResults { get; private set; }
        public IRepository<Fish> FishData { get; private set; }

        public SqliteRepositoryManager(SqliteContext context)
        {
            AppSettings = new SqliteRepository<AppSettings>(context);
            UserRoles = new SqliteRepository<UserRole>(context);
            Fishers = new SqliteRepository<Fisher>(context);
            FishingLeaderboard = new SqliteRepository<Catch>(context);
            TournamentResults = new SqliteRepository<TournamentResult>(context);

            FishData = new SqliteRepository<Fish>(context);
        }
    }
}
