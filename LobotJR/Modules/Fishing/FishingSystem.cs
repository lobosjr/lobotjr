using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using System;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Runs the logic for the fishing system.
    /// </summary>
    public class FishingSystem : ISystem
    {
        private readonly Random Random = new Random();
        private readonly int[] Chances = new int[] { 40, 70, 95, 99, 100 };

        private readonly IRepository<Fish> FishData;
        private readonly IRepository<Fisher> Fishers;
        private readonly IRepository<Catch> Leaderboard;

        private readonly AppSettings Settings;

        public TournamentSystem Tournament { get; set; }

        public FishingSystem(
            IRepository<Fish> fishData,
            IRepository<Fisher> fishers,
            IRepository<Catch> leaderboard,
            IRepository<TournamentResult> tournamentResults,
            IRepository<AppSettings> appSettings)
        {
            FishData = fishData;
            Fishers = fishers;
            Leaderboard = leaderboard;

            Settings = appSettings.Read().First();
            Tournament = new TournamentSystem(fishers, tournamentResults, appSettings);
        }


        /// <summary>
        /// Calculates the exact length, weight, and point value of a fish
        /// being caught.
        /// </summary>
        /// <param name="fisher">The fisher object for the user catching the
        /// fish.</param>
        /// <returns>The catch object with the calculated data values.</returns>
        public Catch CalculateFishSizes(Fisher fisher)
        {
            var catchData = new Catch();
            catchData.Username = fisher.Username;
            catchData.Fish = fisher.Hooked;

            var weightRange = (catchData.Fish.MaximumWeight - catchData.Fish.MinimumWeight) / 5;
            var lengthRange = (catchData.Fish.MaximumLength - catchData.Fish.MaximumLength) / 5;
            var weightVariance = Random.NextDouble() * weightRange;
            var lengthVariance = Random.NextDouble() * lengthRange;

            var size = Random.NextDouble() * 100;
            var category = Chances.Where(x => size > x).Count();
            catchData.Length = (float)Math.Round(catchData.Fish.MinimumLength + lengthRange * category + lengthVariance, 2);
            catchData.Weight = (float)Math.Round(catchData.Fish.MinimumWeight + weightRange * category + weightVariance, 2);
            catchData.Points = (int)Math.Max(Math.Round(size), 1);

            return catchData;
        }

        /// <summary>
        /// Updates the personal leaderboard with new data if the catch object
        /// would set a new record.
        /// </summary>
        /// <param name="fisher">The fisher object for the user catching the
        /// fish.</param>
        /// <param name="catchData">An object with catch data to use for the
        /// update.</param>
        /// <returns>Whether or not the leaderboard was updated.</returns>
        public bool UpdatePersonalLeaderboard(Fisher fisher, Catch catchData)
        {
            var record = fisher.Records.Where(x => x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    fisher.Records.Add(catchData);
                }
                else
                {
                    record.CopyFrom(catchData);
                }
                Fishers.Update(fisher);
                Fishers.Commit();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the global leaderboard with new data if the catch object
        /// would set a new record.
        /// </summary>
        /// <param name="catchData">An object with catch data to use for the
        /// update.</param>
        /// <returns>Whether or not the leaderboard was updated.</returns>
        public bool UpdateGlobalLeaderboard(Catch catchData)
        {
            var record = Leaderboard.Read(x => x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    Leaderboard.Create(catchData);
                }
                else
                {
                    record.CopyFrom(catchData);
                    Leaderboard.Update(record);
                }
                Leaderboard.Commit();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Casts the line out for a user, starting the fishing process.
        /// </summary>
        /// <param name="username">The user to begin fishing for.</param>
        public void Cast(string username)
        {
            var fisher = Fishers.Read(x => x.Username.Equals(username)).FirstOrDefault();
            var hookTime = DateTime.Now;
            if (Tournament.IsRunning)
            {
                hookTime.AddSeconds(Random.Next(Settings.FishingTournamentCastMinimum, Settings.FishingTournamentCastMaximum + 1));
            }
            else
            {
                hookTime.AddSeconds(Random.Next(Settings.FishingCastMinimum, Settings.FishingCastMaximum + 1));
            }
            if (fisher == null)
            {
                fisher = new Fisher()
                {
                    Username = username,
                    IsFishing = true,
                    HookedTime = hookTime
                };
                Fishers.Create(fisher);
            }
            else
            {
                fisher.IsFishing = true;
                fisher.HookedTime = hookTime;
                Fishers.Update(fisher);
            }
            Fishers.Commit();
        }

        /// <summary>
        /// Attempts to hook a fish.
        /// </summary>
        /// <param name="fisher">The fisher to update.</param>
        public void HookFish(Fisher fisher)
        {
            if (fisher != null
                && fisher.IsFishing
                && fisher.Hooked == null
                && DateTime.Now >= fisher.HookedTime)
            {
                var index = -1;
                var rarities = FishData.Read().Select(x => x.Rarity).Distinct().ToList();
                if (Settings.FishingUseWeights)
                {
                    index = Random.WeightedRandom(rarities.Select(x => (double)x.Weight).ToList());
                }
                else
                {
                    index = Random.NextNormalIndex(rarities.Count);
                }
                if (index >= 0)
                {
                    var rarityId = rarities[index].Id;
                    var fishList = FishData.Read(x => x.Rarity.Id == rarityId).ToList();
                    var fish = fishList[Random.Next(0, fishList.Count)];
                    fisher.Hooked = fish;
                    Fishers.Update(fisher);
                    Fishers.Commit();
                }
            }
        }

        /// <summary>
        /// Attempts to release the fish that has been hooked.
        /// </summary>
        /// <param name="fisher">The fisher to update.</param>
        public void ReleaseFish(Fisher fisher)
        {
            if (fisher != null
                && fisher.Hooked != null
                && fisher.HookedTime.HasValue
                && DateTime.Now >= fisher.HookedTime.Value.AddSeconds(Settings.FishingHookLength))
            {
                fisher.Hooked = null;
                fisher.HookedTime = null;
                fisher.IsFishing = false;
                Fishers.Update(fisher);
                Fishers.Commit();
            }
        }

        /// <summary>
        /// Catches the fish a user has hooked.
        /// </summary>
        /// <param name="fisher">The fisher that is trying to catch.</param>
        /// <returns>The catch data for this fish.</returns>
        public Catch CatchFish(Fisher fisher)
        {
            var catchData = default(Catch);
            if (fisher != null && fisher.Hooked != null)
            {
                catchData = CalculateFishSizes(fisher);
                if (Tournament.IsRunning)
                {
                    UpdatePersonalLeaderboard(fisher, catchData);
                    UpdateGlobalLeaderboard(catchData);
                    Tournament.AddTournamentPoints(fisher.Username, catchData.Points);
                }
            }
            return catchData;
        }

        /// <summary>
        /// Runs all active fishers to process hooking and releasing events.
        /// </summary>
        public void Process()
        {
            foreach (var fisher in Fishers.Read(x => x.IsFishing))
            {
                HookFish(fisher);
                ReleaseFish(fisher);
            }
            if (Tournament.IsRunning)
            {
                Tournament.Process();
            }
        }
    }
}
