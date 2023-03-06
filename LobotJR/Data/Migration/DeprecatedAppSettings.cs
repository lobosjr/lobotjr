using NuGet.Versioning;
using System.ComponentModel.DataAnnotations.Schema;

namespace LobotJR.Data.Migration
{
    [Table("AppSettings")]
    public class DeprecatedAppSettings : TableObject
    {
        public string DatabaseVersion { get; set; } = new SemanticVersion(1, 0, 1).ToString();
    }
}
