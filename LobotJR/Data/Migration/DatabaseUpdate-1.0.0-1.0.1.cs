using LobotJR.Data.User;
using NuGet.Versioning;
using System;

namespace LobotJR.Data.Migration
{
    public class DatabaseUpdate_1_0_0_1_0_1 : IDatabaseUpdate
    {
        private readonly UserLookup UserLookup;
        private readonly string Token;
        private readonly string ClientId;

        public SemanticVersion FromVersion => new SemanticVersion(1, 0, 0);
        public SemanticVersion ToVersion => new SemanticVersion(1, 0, 1);

        public DatabaseUpdate_1_0_0_1_0_1(UserLookup userLookup, string token, string clientId)
        {
            UserLookup = userLookup;
            Token = token;
            ClientId = clientId;
        }

        public DatabaseMigrationResult Update(SqliteContext context, IRepositoryManager repositoryManager)
        {
            var result = new DatabaseMigrationResult { Success = true };
            var commands = new string[]
            {
                "ALTER TABLE \"Catches\" RENAME COLUMN [Fish_Id] TO [FishId]",
                "ALTER TABLE \"LeaderboardEntries\" RENAME COLUMN [Fish_Id] TO [FishId]",
                "ALTER TABLE \"Fish\" RENAME COLUMN [Rarity_Id] TO [RarityId]",
                "ALTER TABLE \"Fish\" RENAME COLUMN [SizeCategory_Id] TO [SizeCategoryId]"
            };
            result.DebugOutput.Add("Executing SQL statements to add/update tables...");
            foreach (var command in commands)
            {
                result.DebugOutput.Add(command);
                try
                {
                    context.Database.ExecuteSqlCommand(command);
                }
                catch (Exception e)
                {
                    result.DebugOutput.Add($"Exception: {e}");
                }
            }
            return result;
        }
    }
}
