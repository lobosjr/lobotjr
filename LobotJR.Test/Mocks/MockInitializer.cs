using SQLite.CodeFirst;
using System.Collections.Generic;
using System.Data.Entity;

namespace LobotJR.Test.Mocks
{
    /// <summary>
    /// In-memory sqlite database used as the database connection during unit tests.
    /// </summary>
    public class MockInitializer : SqliteDropCreateDatabaseAlways<MockContext>
    {
        private readonly List<ContextInitializer> initializers = new List<ContextInitializer>();

        public MockInitializer(DbModelBuilder dbModelBuilder, IEnumerable<ContextInitializer> initializers) : base(dbModelBuilder)
        {
            this.initializers.AddRange(initializers);
        }

        protected override void Seed(MockContext context)
        {
            // This is being called each time, but for some reason the data isn't being properly initialized. 
            foreach (var initializer in initializers)
            {
                initializer.Invoke(context);
                context.SaveChanges();
            }
        }
    }
}
