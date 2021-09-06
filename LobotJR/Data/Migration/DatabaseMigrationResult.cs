using NuGet.Versioning;
using System.Collections.Generic;

namespace LobotJR.Data.Migration
{
    /// <summary>
    /// Holds the data from a database migration.
    /// </summary>
    public class DatabaseMigrationResult
    {
        /// <summary>
        /// The debug output, which contains each sql statement executed.
        /// </summary>
        public List<string> DebugOutput { get; set; } = new List<string>();
        /// <summary>
        /// Whether or not the update was successful.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// The version of the database before the migration.
        /// </summary>
        public SemanticVersion PreviousVersion { get; set; }
        /// <summary>
        /// The version of the database after the migration.
        /// </summary>
        public SemanticVersion NewVersion { get; set; }
    }
}
