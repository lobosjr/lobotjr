using LobotJR.Shared.Authentication;
using LobotJR.Shared.Client;
using LobotJR.Shared.User;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LobotJR.Data.Migration
{
    public class DatabaseUpdate_Null_1_0_0 : IDatabaseUpdate
    {
        private readonly string Token;
        private readonly string ClientId;

        public SemanticVersion FromVersion => null;
        public SemanticVersion ToVersion => new SemanticVersion(1, 0, 0);
        public bool UsesMetadata => false;


        public DatabaseUpdate_Null_1_0_0(TokenData token, ClientData client)
        {
            Token = token.BroadcastToken.AccessToken;
            ClientId = client.ClientId;
        }

        public DatabaseMigrationResult Update(DbContext context)
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

            var tournamentNames = context.Database.SqlQuery<string>("SELECT [UserId] from \"TournamentEntries\"");
            var roleNameLists = context.Database.SqlQuery<string>("SELECT [UserList] from \"UserRoles\"");
            var roleNames = roleNameLists.SelectMany(x => x.Split(','));
            var allNames = new List<string>(tournamentNames);
            allNames.AddRange(roleNames);
            var ids = Users.Get(Token, ClientId, allNames.Distinct()).GetAwaiter().GetResult();
            var idMap = new Dictionary<string, string>();
            foreach (var id in ids.Data)
            {
                if (id != null && !string.IsNullOrWhiteSpace(id.Id))
                {
                    idMap.Add(id.DisplayName, id.Id);
                }
            }

            foreach (var tournamentName in tournamentNames)
            {
                if (idMap.ContainsKey(tournamentName))
                {
                    context.Database.ExecuteSqlCommand($"UPDATE \"TournamentEntries\" SET [UserId] = '{idMap[tournamentName]}' WHERE [UserId] = '{tournamentName}'");
                }
                else
                {
                    context.Database.ExecuteSqlCommand($"DELETE FROM \"TournamentEntries\" WHERE [UserId] = '{tournamentName}'");
                }
            }
            context.Database.ExecuteSqlCommand($"DELETE R FROM \"TournamentResults\" R LEFT JOIN \"TournamentEntries\" E ON R.[Id] = E.[ResultId] WHERE E.[Id] IS NULL");

            foreach (var roleNameList in roleNameLists)
            {
                var list = roleNameList.Split(',');
                list.Select(x => idMap.ContainsKey(x) ? idMap[x] : null).Where(x => x != null);
                context.Database.ExecuteSqlCommand($"UPDATE \"UserRoles\" SET [UserList] = '{string.Join(",", list)}' where [UserList] = '{roleNameList}'");
            }

            return result;
        }
    }
}
