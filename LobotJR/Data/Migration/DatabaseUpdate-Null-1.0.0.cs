using LobotJR.Data.User;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Data.Migration
{
    public class DatabaseUpdate_Null_1_0_0 : IDatabaseUpdate
    {
        private readonly UserLookup UserLookup;
        private readonly string Token;
        private readonly string ClientId;

        public SemanticVersion FromVersion => null;
        public SemanticVersion ToVersion => new SemanticVersion(1, 0, 0);

        public DatabaseUpdate_Null_1_0_0(UserLookup userLookup, string token, string clientId)
        {
            UserLookup = userLookup;
            Token = token;
            ClientId = clientId;
        }

        public DatabaseMigrationResult Update(SqliteContext context, SqliteRepositoryManager repositoryManager)
        {
            var result = new DatabaseMigrationResult { Success = true };
            var commands = new string[]
            {
                "CREATE TABLE \"AppSettings\" ([Id] INTEGER PRIMARY KEY, [DatabaseVersion] nvarchar, [GeneralCacheUpdateTime] int NOT NULL, [FishingCastMinimum] int NOT NULL, [FishingCastMaximum] int NOT NULL, [FishingHookLength] int NOT NULL, [FishingUseNormalRarity] bit NOT NULL, [FishingUseNormalSizes] bit NOT NULL, [FishingGloatCost] int NOT NULL, [FishingTournamentDuration] int NOT NULL, [FishingTournamentInterval] int NOT NULL, [FishingTournamentCastMinimum] int NOT NULL, [FishingTournamentCastMaximum] int NOT NULL)",
                "CREATE TABLE \"Catches\" ([Id] INTEGER PRIMARY KEY, [UserId] nvarchar, [Length] real NOT NULL, [Weight] real NOT NULL, [Points] int NOT NULL, [Fish_Id] int, [Fisher_Id] int, FOREIGN KEY (Fish_Id) REFERENCES \"Fish\"(Id), FOREIGN KEY (Fisher_Id) REFERENCES \"Fishers\"(Id))",
                "CREATE TABLE \"LeaderboardEntries\" ([Id] INTEGER PRIMARY KEY, [UserId] nvarchar, [Length] real NOT NULL, [Weight] real NOT NULL, [Fish_Id] int, FOREIGN KEY (Fish_Id) REFERENCES \"Fish\"(Id))",
                "CREATE TABLE \"Fish\" ([Id] INTEGER PRIMARY KEY, [Name] nvarchar, [MinimumLength] real NOT NULL, [MaximumLength] real NOT NULL, [MinimumWeight] real NOT NULL, [MaximumWeight] real NOT NULL, [FlavorText] nvarchar, [Rarity_Id] int, [SizeCategory_Id] int, FOREIGN KEY (Rarity_Id) REFERENCES \"FishRarities\"(Id), FOREIGN KEY (SizeCategory_Id) REFERENCES \"FishSizes\"(Id))",
                "CREATE TABLE \"FishRarities\" ([Id] INTEGER PRIMARY KEY, [Name] nvarchar, [Weight] real NOT NULL)",
                "CREATE TABLE \"FishSizes\" ([Id] INTEGER PRIMARY KEY, [Name] nvarchar, [Message] nvarchar)",
                "CREATE TABLE \"Fishers\" ([Id] INTEGER PRIMARY KEY, [UserId] nvarchar)",
                "CREATE TABLE \"UserMaps\" ([Id] INTEGER PRIMARY KEY, [Username] nvarchar, [TwitchId] nvarchar)",
                "CREATE INDEX \"IX_Catch_Fish_Id\" ON \"Catches\" (\"Fish_Id\")",
                "CREATE INDEX \"IX_Catch_Fisher_Id\" ON \"Catches\" (\"Fisher_Id\")",
                "CREATE INDEX \"IX_Fish_Rarity_Id\" ON \"Fish\" (\"Rarity_Id\")",
                "CREATE INDEX \"IX_Fish_SizeCategory_Id\" ON \"Fish\" (\"SizeCategory_Id\")",
                "ALTER TABLE \"TournamentEntries\" RENAME COLUMN [Name] TO [UserId]"
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

            foreach (var tournament in repositoryManager.TournamentResults.Read())
            {
                foreach (var entry in tournament.Entries)
                {
                    UserLookup.GetId(entry.UserId);
                }
            }
            foreach (var userRole in repositoryManager.UserRoles.Read())
            {
                var userList = userRole.UserIds;
                foreach (var user in userList)
                {
                    UserLookup.GetId(user);
                }
            }
            UserLookup.UpdateCache(Token, ClientId);

            foreach (var tournament in repositoryManager.TournamentResults.Read())
            {
                var names = tournament.Entries.Select(x => x.UserId).ToList();
                foreach (var entry in tournament.Entries)
                {
                    entry.UserId = UserLookup.GetId(entry.UserId, false);
                }
                foreach (var entry in tournament.Entries.Where(x => x.UserId == null).ToList())
                {
                    repositoryManager.TournamentEntries.DeleteById(entry.Id);
                }
                tournament.Entries = tournament.Entries.Where(x => x.UserId != null).ToList();
                if (tournament.Entries.Count > 0)
                {
                    repositoryManager.TournamentResults.Update(tournament);
                }
                else
                {
                    repositoryManager.TournamentResults.DeleteById(tournament.Id);
                }
            }
            repositoryManager.TournamentResults.Commit();
            foreach (var userRole in repositoryManager.UserRoles.Read())
            {
                var userList = userRole.UserIds;
                var idList = new List<string>();
                foreach (var user in userList)
                {
                    idList.Add(UserLookup.GetId(user));
                }
                userRole.UserIds = idList;
                repositoryManager.UserRoles.Update(userRole);
            }
            repositoryManager.UserRoles.Commit();

            return result;
        }
    }
}
