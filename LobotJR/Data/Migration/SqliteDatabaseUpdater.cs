using LobotJR.Data.User;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;

namespace LobotJR.Data.Migration
{
    /// <summary>
    /// TODO: Add Comments
    /// </summary>
    public class SqliteDatabaseUpdater
    {
        public static readonly SemanticVersion LatestVersion = new SemanticVersion(1, 0, 1);

        private readonly IEnumerable<IDatabaseUpdate> DatabaseUpdates;

        public IRepositoryManager RepositoryManager { get; set; }
        public SqliteContext Context { get; set; }

        public SqliteDatabaseUpdater(IRepositoryManager manager, SqliteContext context, UserLookup userLookup, string broadcastToken, string clientId)
        {
            RepositoryManager = manager;
            Context = context;
            DatabaseUpdates = new IDatabaseUpdate[]
            {
                new DatabaseUpdate_Null_1_0_0(userLookup, broadcastToken, clientId),
                new DatabaseUpdate_1_0_0_1_0_1(userLookup, broadcastToken, clientId)
            };
        }

        public SemanticVersion GetDatabaseVersion(IRepositoryManager manager)
        {
            var updates = new List<IDatabaseUpdate>();
            try
            {
                var appSettings = manager.AppSettings.Read().FirstOrDefault();
                if (appSettings != null)
                {
                    if (SemanticVersion.TryParse(appSettings.DatabaseVersion, out var version))
                    {
                        return version;
                    }
                }
            }
            catch (EntityCommandExecutionException)
            {
            }
            return null;
        }

        public string GetDatabaseFile()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SqliteContext"].ConnectionString;
            return connectionString.Split('=')[1];
        }

        public string BackupDatabase(string databaseFile, SemanticVersion currentVersion)
        {
            var backupFile = $"{databaseFile}-{currentVersion}-{DateTime.Now.ToFileTimeUtc()}.backup";
            File.Copy(databaseFile, backupFile);
            return backupFile;
        }

        public bool RestoreBackup(string backupFile, string databaseFile)
        {
            try
            {
                File.Delete(databaseFile);
                File.Move(backupFile, databaseFile);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        public DatabaseMigrationResult ProcessDatabaseUpdates(SqliteContext context, IRepositoryManager repositoryManager, SemanticVersion currentVersion)
        {
            var result = new DatabaseMigrationResult { PreviousVersion = currentVersion };
            var updates = DatabaseUpdates.Where(x => currentVersion == null && x.FromVersion == null || x.FromVersion >= currentVersion).OrderBy(x => x.FromVersion);
            foreach (var update in updates)
            {
                var updateResult = update.Update(context, repositoryManager);
                result.DebugOutput.Add($"Updating database version from {update.FromVersion} to {update.ToVersion}...");
                if (updateResult.Success)
                {
                    result.DebugOutput.AddRange(updateResult.DebugOutput);
                    result.NewVersion = update.ToVersion;
                }
                else
                {
                    result.DebugOutput.Add($"Update failed, restoring database backup.");
                    return result;
                }
            }
            result.Success = true;
            return result;
        }

        public DatabaseMigrationResult UpdateDatabase()
        {
            var currentVersion = GetDatabaseVersion(RepositoryManager);
            if (currentVersion < LatestVersion)
            {
                var databaseFile = GetDatabaseFile();
                var backup = BackupDatabase(databaseFile, currentVersion);
                var results = ProcessDatabaseUpdates(Context, RepositoryManager, currentVersion);
                if (results.Success)
                {
                    var appSettings = RepositoryManager.AppSettings.Read().FirstOrDefault();
                    if (appSettings == null)
                    {
                        RepositoryManager.AppSettings.Create(new AppSettings());
                    }
                    else
                    {
                        appSettings.DatabaseVersion = LatestVersion.ToString();
                        RepositoryManager.AppSettings.Update(appSettings);
                    }
                    RepositoryManager.AppSettings.Commit();
                }
                else
                {
                    RestoreBackup(backup, databaseFile);
                }
                return results;
            }
            return null;
        }
    }
}
