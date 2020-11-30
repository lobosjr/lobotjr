using LobotJR.Modules.Fishing;
using System.Data.Entity;

namespace LobotJR.Data
{
    public interface IDatabaseContext
    {
        DbSet<TournamentResult> FishingTournaments { get; set; }

        int SaveChanges();
    }
}
