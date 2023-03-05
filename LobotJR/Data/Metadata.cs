using LobotJR.Data.Migration;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LobotJR.Data
{
    [Table("Metadata")]
    public class Metadata : TableObject
    {
        /// <summary>
        /// The version of the database that was last opened by this user.
        /// </summary>
        public string DatabaseVersion { get; set; } = SqliteDatabaseUpdater.LatestVersion.ToString();
        /// <summary>
        /// The timestamp of the last time the database was updated.
        /// </summary>
        public DateTime LastSchemaUpdate { get; set; }

        public Metadata()
        {
            LastSchemaUpdate = DateTime.Now;
        }
    }
}
