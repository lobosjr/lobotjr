using LobotJR.Command;
using SQLite.CodeFirst;
using System.Data.Entity;

namespace LobotJR.Data
{
    public class SqliteInitializer : SqliteCreateDatabaseIfNotExists<SqliteContext>
    {
        public SqliteInitializer(DbModelBuilder dbModelBuilder) : base(dbModelBuilder) { }
        protected override void Seed(SqliteContext context)
        {
            context.UserRoles.Add(new UserRole("Streamer", new string[] { "28640725", "lobotjr" }, new string[] { "*.Admin.*" }));
            context.UserRoles.Add(new UserRole("UIDev", new string[] { "28640725", "lobotjr", "26374083" }, new string[] { }));
            context.AppSettings.Add(new AppSettings());
        }
    }
}
