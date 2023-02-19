using LobotJR.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;

namespace LobotJR.Test.Mocks
{
    /// <summary>
    /// Adds data to the in-memory database during initialization.
    /// </summary>
    /// <param name="context"></param>
    public delegate void ContextInitializer(MockContext context);

    /// <summary>
    /// In-memory sqlite database used as the database connection during unit tests.
    /// </summary>
    public class MockContext : SqliteContext
    {
        private readonly List<ContextInitializer> initializers = new List<ContextInitializer>();

        public static MockContext CreateAndSeed(params ContextInitializer[] initializers)
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            conn.Open();
            return new MockContext(conn, initializers);
        }

        public static MockContext Create()
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            conn.Open();
            var context = new MockContext(conn);
            context.Database.Initialize(true);
            return context;
        }

        private MockContext(DbConnection connection, IEnumerable<ContextInitializer> initializers) : base(connection)
        {
            this.initializers = new List<ContextInitializer>(initializers);
        }

        private MockContext(DbConnection connection) : base(connection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new MockInitializer(modelBuilder, initializers);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
