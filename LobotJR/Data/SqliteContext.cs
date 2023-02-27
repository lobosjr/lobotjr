using Autofac;
using LobotJR.Command;
using LobotJR.Command.Model.Fishing;
using LobotJR.Data.User;
using System.Data.Common;
using System.Data.Entity;

namespace LobotJR.Data
{
    /// <summary>
    /// SQLite implementation of the EF6 DbContext
    /// </summary>
    public class SqliteContext : DbContext, IStartable
    {
        public DbSet<AppSettings> AppSettings { get; set; }

        /** User data */
        public DbSet<UserMap> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Catch> Catches { get; set; }
        public DbSet<LeaderboardEntry> FishingLeaderboard { get; set; }
        public DbSet<TournamentResult> FishingTournaments { get; set; }


        /** Content data */
        public DbSet<Fish> FishData { get; set; }

        public SqliteContext() { }

        public SqliteContext(DbConnection connection) : base(connection, true) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteInitializer(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public void Start()
        {
            this.Database.Initialize(false);
        }
    }
}
