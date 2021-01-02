using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;
using System.Data.Entity;

namespace LobotJR.Data
{
    /// <summary>
    /// SQLite implementation of the EF6 DbContext
    /// </summary>
    public class SqliteContext : DbContext
    {
        public DbSet<TournamentResult> FishingTournaments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteInitializer(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
