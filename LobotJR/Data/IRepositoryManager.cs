using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;

namespace LobotJR.Data
{
    /// <summary>
    /// Collection of repositories for data access.
    /// </summary>
    public interface IRepositoryManager
    {
        IRepository<UserRole> UserRoles { get; }
        IRepository<TournamentResult> TournamentResults { get; }
    }
}
