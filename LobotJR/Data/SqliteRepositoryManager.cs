using LobotJR.Command;
using LobotJR.Data.User;
using LobotJR.Modules.Fishing.Model;
using System;
using System.Data.Entity;

namespace LobotJR.Data
{
    /// <summary>
    /// Implementation of a repository manager using sqlite storage.
    /// </summary>
    public class SqliteRepositoryManager : IRepositoryManager, IContentManager, IDisposable
    {
        private DbContext context;

        public IRepository<AppSettings> AppSettings { get; private set; }
        public IRepository<UserMap> Users { get; private set; }
        public IRepository<UserRole> UserRoles { get; private set; }
        public IRepository<Catch> Catches { get; }
        public IRepository<LeaderboardEntry> FishingLeaderboard { get; }
        public IRepository<TournamentResult> TournamentResults { get; private set; }
        public IRepository<TournamentEntry> TournamentEntries { get; private set; }
        public IRepository<Fish> FishData { get; private set; }

        public SqliteRepositoryManager(DbContext context)
        {
            this.context = context;
            AppSettings = new SqliteRepository<AppSettings>(context);
            Users = new SqliteRepository<UserMap>(context);
            UserRoles = new SqliteRepository<UserRole>(context);
            Catches = new SqliteRepository<Catch>(context);
            FishingLeaderboard = new SqliteRepository<LeaderboardEntry>(context);
            TournamentResults = new SqliteRepository<TournamentResult>(context);
            TournamentEntries = new SqliteRepository<TournamentEntry>(context);

            FishData = new SqliteRepository<Fish>(context);
        }

        public void Dispose()
        {
            context.Database.Connection.Close();
            context.Dispose();
        }
    }
}
