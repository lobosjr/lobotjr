using System.Data.Entity;

namespace LobotJR.Data.Migration
{
    /// <summary>
    /// SQLite implementation of the EF6 DbContext
    /// </summary>
    public class SqliteUpdateContext : DbContext
    {
        public DbSet<Metadata> Metadata { get; set; }

        public SqliteUpdateContext() { }
    }
}
