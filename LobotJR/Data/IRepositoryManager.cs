using LobotJR.Command;
using LobotJR.Command.Model.Fishing;
using LobotJR.Data.User;

namespace LobotJR.Data
{
    /// <summary>
    /// Collection of repositories for data access.
    /// </summary>
    public interface IRepositoryManager
    {
        IRepository<AppSettings> AppSettings { get; }
        IRepository<UserMap> Users { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Catch> Catches { get; }
        IRepository<LeaderboardEntry> FishingLeaderboard { get; }
        IRepository<TournamentResult> TournamentResults { get; }
        IRepository<TournamentEntry> TournamentEntries { get; }
    }
}
