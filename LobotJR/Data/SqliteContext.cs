using LobotJR.Modules.Fishing;
using SQLite.CodeFirst;
using System.Data.Entity;

namespace LobotJR.Data
{
    public class SqliteContext : DbContext, IDatabaseContext
    {
        public static SqliteContext Instance = new SqliteContext();

        public DbSet<TournamentResult> FishingTournaments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SqliteContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
