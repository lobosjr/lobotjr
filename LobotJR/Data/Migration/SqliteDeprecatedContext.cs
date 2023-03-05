using System.Data.Entity;

namespace LobotJR.Data.Migration
{
    /// <summary>
    /// SQLite implementation of the EF6 DbContext
    /// </summary>
    public class SqliteDeprecatedContext : DbContext
    {
        public DbSet<AppSettings> AppSettings { get; set; }

        public SqliteDeprecatedContext() { }
    }
}
