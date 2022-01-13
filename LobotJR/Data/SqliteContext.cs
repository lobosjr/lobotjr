using LobotJR.Command;
using LobotJR.Data.User;
using LobotJR.Modules.Fishing.Model;
using System.Data.Entity;

namespace LobotJR.Data
{
    /// <summary>
    /// SQLite implementation of the EF6 DbContext
    /// </summary>
    public class SqliteContext : DbContext
    {
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<UserMap> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Fisher> Fishers { get; set; }
        public DbSet<LeaderboardEntry> FishingLeaderboard { get; set; }
        public DbSet<TournamentResult> FishingTournaments { get; set; }

        
        /** Content data */
        public DbSet<Fish> FishData { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteInitializer(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
