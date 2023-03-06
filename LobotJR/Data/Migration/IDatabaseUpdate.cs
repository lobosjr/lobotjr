using NuGet.Versioning;
using System.Data.Entity;

namespace LobotJR.Data.Migration
{
    /// <summary>
    /// Represents a class that can execute the SQL statements necessary to
    /// update the database schema between update versions.
    /// </summary>
    public interface IDatabaseUpdate
    {
        /// <summary>
        /// The version of database this updater expects. Null represents
        /// database versions from before versioning was added.
        /// </summary>
        SemanticVersion FromVersion { get; }
        /// <summary>
        /// The version the database will be after the update finishes.
        /// </summary>
        SemanticVersion ToVersion { get; }
        /// <summary>
        /// Whether or not this update should write the new version to the
        /// metadata table.
        /// </summary>
        bool UsesMetadata { get; }
        /// <summary>
        /// Updates the database schema.
        /// </summary>
        /// <param name="context">The database context to update.</param>
        /// <returns>The database migration results object.</returns>
        DatabaseMigrationResult Update(DbContext context);
    }
}
