using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Data.User;
using SQLite.CodeFirst;
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

        public static MockContext Create(params ContextInitializer[] initializers)
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            conn.Open();
            return new MockContext(conn, initializers);
        }

        private MockContext(DbConnection connection, IEnumerable<ContextInitializer> initializers) : base(connection)
        {
            this.initializers = new List<ContextInitializer>(initializers);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new MockInitializer(modelBuilder, initializers);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }

    public class MockInitializer : SqliteCreateDatabaseIfNotExists<MockContext>
    {
        private readonly List<ContextInitializer> initializers = new List<ContextInitializer>();

        public MockInitializer(DbModelBuilder dbModelBuilder, IEnumerable<ContextInitializer> initializers) : base(dbModelBuilder)
        {
            this.initializers.AddRange(initializers);
        }

        protected override void Seed(MockContext context)
        {
            var streamer = new UserMap() { TwitchId = "01", Username = "Streamer" };
            var bot = new UserMap() { TwitchId = "02", Username = "Bot" };
            var dev = new UserMap() { TwitchId = "03", Username = "Dev" };
            context.UserRoles.Add(new UserRole("Streamer", new string[] { streamer.TwitchId, bot.TwitchId }, new string[] { "*.Admin.*" }));
            context.UserRoles.Add(new UserRole("UIDev", new string[] { streamer.TwitchId, bot.TwitchId, dev.TwitchId }, new string[] { }));
            context.Users.Add(streamer);
            context.Users.Add(bot);
            context.Users.Add(dev);
            context.AppSettings.Add(new AppSettings());
            context.SaveChanges();
            foreach (var initializer in initializers)
            {
                initializer.Invoke(context);
            }
            context.SaveChanges();
        }
    }
}
