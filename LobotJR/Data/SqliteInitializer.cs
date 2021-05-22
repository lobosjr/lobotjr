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
            context.UserRoles.Add(new UserRole("Streamer", new string[] { "lobosjr", "lobotjr" }, new string[] { "*.Admin.*" }));
            context.UserRoles.Add(new UserRole("UIDev", new string[] { "lobosjr", "lobotjr", "empyrealhell" }, new string[] { }));
        }
    }
}
