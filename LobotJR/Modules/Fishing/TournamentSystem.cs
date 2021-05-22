using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using System;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Runs the tournament logic for the fishing system.
    /// </summary>
    public class TournamentSystem : ISystem
    {
        private readonly IRepository<Fisher> Fishers;
        private readonly IRepository<TournamentResult> TournamentResults;
        private readonly AppSettings Settings;

        /// <summary>
        /// The current tournament, if one is running.
        /// </summary>
        public TournamentResult CurrentTournament { get; set; }
        /// <summary>
        /// The date and time of the next scheduled tournament.
        /// </summary>
        public DateTime? NextTournament { get; set; }
        /// <summary>
        /// Whether or not a tournament is currently running.
        /// </summary>
        public bool IsRunning { get { return CurrentTournament != null; } }

        public TournamentSystem(
            IRepository<Fisher> fishers,
            IRepository<TournamentResult> tournamentResults,
            IRepository<AppSettings> appSettings)
        {
            Fishers = fishers;
            TournamentResults = tournamentResults;
            
            Settings = appSettings.Read().First();
            NextTournament = DateTime.Now.AddMinutes(Settings.FishingTournamentInterval);
        }

        /// <summary>
        /// Adds points to a user in a tournament. If this is their first catch
        /// of the tournament, it will add an entry for them as well.
        /// </summary>
        /// <param name="userId">The user to update.</param>
        /// <param name="points">The amount of points to add.</param>
        /// <returns>The user's current point total.</returns>
        public int AddTournamentPoints(string userId, int points)
        {
            if (CurrentTournament != null)
            {
                var entry = CurrentTournament.Entries.Where(x => x.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (entry == null)
                {
                    entry = new TournamentEntry(userId, 0);
                    CurrentTournament.Entries.Add(entry);
                }
                entry.Points += points;
                return entry.Points;
            }
            return -1;
        }

        /// <summary>
        /// Starts a new tournament.
        /// </summary>
        public void StartTournament()
        {
            if (CurrentTournament == null)
            {
                var fishers = Fishers.Read(x => x.IsFishing).ToList();
                foreach (var fisher in fishers)
                {
                    fisher.IsFishing = false;
                    fisher.Hooked = null;
                    fisher.HookedTime = null;
                    Fishers.Update(fisher);
                }
                Fishers.Commit();

                CurrentTournament = new TournamentResult
                {
                    Date = DateTime.Now.AddMinutes(Settings.FishingTournamentDuration)
                };
                NextTournament = null;
            }
        }

        /// <summary>
        /// Ends the current tournament, saves the results, and schedules the next one.
        /// </summary>
        public void EndTournament()
        {
            if (CurrentTournament != null)
            {
                TournamentResults.Create(CurrentTournament);
                TournamentResults.Commit();
                NextTournament = CurrentTournament.Date.AddMinutes(Settings.FishingTournamentDuration + Settings.FishingTournamentInterval);
                CurrentTournament = null;
            }
        }

        /// <summary>
        /// Processes the tournament system, starting or ending the tournament as necessary.
        /// </summary>
        public void Process(bool broadcasting)
        {
            if (!broadcasting)
            {
                if (CurrentTournament != null)
                {
                    EndTournament();
                }
                NextTournament = null;
            }
            else
            {
                if (CurrentTournament != null && DateTime.Now >= CurrentTournament.Date)
                {
                    EndTournament();
                }
                else if (CurrentTournament == null && DateTime.Now >= NextTournament)
                {
                    StartTournament();
                }
            }
        }
    }
}
