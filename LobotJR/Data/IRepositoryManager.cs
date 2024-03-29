﻿using LobotJR.Command;
using LobotJR.Data.User;
using LobotJR.Modules.Fishing.Model;

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
        IRepository<Fisher> Fishers { get; }
        IRepository<Catch> FishingLeaderboard { get; }
        IRepository<TournamentResult> TournamentResults { get; }
    }
}
